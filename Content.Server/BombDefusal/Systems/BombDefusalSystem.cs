using System.Linq;
using Content.Server.BombDefusal.Components;
using Content.Server.BombDefusal.Modules;
using Content.Server.Defusable.Components;
using Content.Server.Defusable.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Popups;
using Content.Shared.Administration.Logs;
using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Content.Shared.Construction.Components;
using Content.Shared.Database;
using Content.Shared.Defusable;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.Trigger.Components;
using Content.Shared.Trigger.Systems;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.BombDefusal.Systems;

/// <summary>
/// Server system for KTANE-style bomb defusal.
/// Handles module generation, interaction validation, strike tracking, and defusal/detonation.
/// </summary>
public sealed class BombDefusalSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly TriggerSystem _trigger = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private const string SerialChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BombDefusalComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BombDefusalComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<BombDefusalComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAltVerbs);
        SubscribeLocalEvent<BombDefusalComponent, AnchorAttemptEvent>(OnAnchorAttempt);
        SubscribeLocalEvent<BombDefusalComponent, UnanchorAttemptEvent>(OnUnanchorAttempt);
        SubscribeLocalEvent<BombDefusalComponent, BombModuleInteractionMessage>(OnModuleInteraction);
        SubscribeLocalEvent<BombDefusalComponent, BoundUIClosedEvent>(OnUiClosed);
    }

    /// <summary>
    /// Generate serial number on map init. Modules are generated when the bomb is armed.
    /// </summary>
    private void OnMapInit(EntityUid uid, BombDefusalComponent comp, MapInitEvent args)
    {
        comp.SerialNumber = GenerateSerialNumber();
    }

    /// <summary>
    /// Generate modules when the bomb is armed (so module count can be based on timer).
    /// </summary>
    public void GenerateModules(EntityUid uid, BombDefusalComponent comp)
    {
        if (comp.ModulesGenerated)
            return;

        var moduleCount = comp.ModuleCountOverride ?? GetModuleCountFromTimer(uid);
        moduleCount = Math.Max(1, moduleCount); // At least 1 module

        var availableTypes = Enum.GetValues<BombModuleType>();

        for (var i = 0; i < moduleCount; i++)
        {
            var moduleType = _random.Pick(availableTypes);
            var module = GenerateModule(moduleType, comp.SerialNumber);
            comp.Modules.Add(module);
        }

        comp.ModulesGenerated = true;
    }

    private BombModule GenerateModule(BombModuleType type, string serialNumber)
    {
        return type switch
        {
            BombModuleType.Wires => WiresModule.Generate(_random, serialNumber),
            BombModuleType.Symbols => SymbolsModule.Generate(_random),
            BombModuleType.Button => ButtonModule.Generate(_random, serialNumber),
            BombModuleType.SimonSays => SimonSaysModule.Generate(_random, serialNumber),
            BombModuleType.Codewords => CodewordsModule.Generate(_random),
            _ => WiresModule.Generate(_random, serialNumber),
        };
    }

    /// <summary>
    /// Calculate module count from timer: 1 module per minute.
    /// </summary>
    private int GetModuleCountFromTimer(EntityUid uid)
    {
        if (!TryComp<TimerTriggerComponent>(uid, out var timer))
            return 3; // fallback

        var seconds = timer.Delay.TotalSeconds;
        return (int) Math.Round(seconds / 60.0);
    }

    private string GenerateSerialNumber()
    {
        var length = 6;
        var chars = new char[length];
        for (var i = 0; i < length; i++)
        {
            chars[i] = SerialChars[_random.Next(SerialChars.Length)];
        }
        return new string(chars);
    }

    #region Event Handlers

    private void OnExamine(EntityUid uid, BombDefusalComponent comp, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        using (args.PushGroup(nameof(BombDefusalComponent)))
        {
            if (comp.IsDefused)
            {
                args.PushMarkup(Loc.GetString("bomb-defusal-examine-defused", ("name", uid)));
            }
            else if (HasComp<ActiveTimerTriggerComponent>(uid))
            {
                var remaining = _trigger.GetRemainingTime(uid);
                if (remaining != null)
                {
                    args.PushMarkup(Loc.GetString("bomb-defusal-examine-active", ("name", uid),
                        ("time", Math.Floor(remaining.Value.TotalSeconds))));
                }
                else
                {
                    args.PushMarkup(Loc.GetString("bomb-defusal-examine-active-no-time", ("name", uid)));
                }

                args.PushMarkup(Loc.GetString("bomb-defusal-examine-strikes", ("current", comp.Strikes), ("max", comp.MaxStrikes)));
                args.PushMarkup(Loc.GetString("bomb-defusal-examine-modules",
                    ("solved", comp.Modules.Count(m => m.IsSolved)),
                    ("total", comp.Modules.Count)));
            }
            else
            {
                args.PushMarkup(Loc.GetString("bomb-defusal-examine-inactive", ("name", uid)));
            }
        }
    }

    private void OnGetAltVerbs(EntityUid uid, BombDefusalComponent comp, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess || args.Hands == null)
            return;

        // Only show "Begin countdown" if not already armed and not defused
        if (HasComp<ActiveTimerTriggerComponent>(uid) || comp.IsDefused)
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("defusable-verb-begin"),
            Priority = 10,
            Act = () =>
            {
                TryStartCountdown(uid, args.User, comp);
            }
        });
    }

    private void OnAnchorAttempt(EntityUid uid, BombDefusalComponent comp, AnchorAttemptEvent args)
    {
        // Bolted when armed
        if (HasComp<ActiveTimerTriggerComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("defusable-popup-cant-anchor", ("name", uid)), uid, args.User);
            args.Cancel();
        }
    }

    private void OnUnanchorAttempt(EntityUid uid, BombDefusalComponent comp, UnanchorAttemptEvent args)
    {
        if (HasComp<ActiveTimerTriggerComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("defusable-popup-cant-anchor", ("name", uid)), uid, args.User);
            args.Cancel();
        }
    }

    private void OnModuleInteraction(EntityUid uid, BombDefusalComponent comp, BombModuleInteractionMessage args)
    {
        if (comp.IsDefused || !HasComp<ActiveTimerTriggerComponent>(uid))
            return;

        if (args.ModuleIndex < 0 || args.ModuleIndex >= comp.Modules.Count)
            return;

        var module = comp.Modules[args.ModuleIndex];

        if (module.IsSolved)
            return;

        bool success;

        // Simon Says needs strike count context
        if (module is SimonSaysModule simon)
        {
            success = simon.ValidateActionWithStrikes(args.Action, comp.Strikes);
        }
        else
        {
            success = module.ValidateAction(args.Action);
        }

        if (success)
        {
            if (module.IsSolved)
            {
                _audio.PlayPvs(comp.SolveSound, uid);

                _adminLogger.Add(LogType.Explosion, LogImpact.Medium,
                    $"{ToPrettyString(args.Actor):user} solved module {args.ModuleIndex} ({module.Type}) on {ToPrettyString(uid):entity}");

                // Check if all modules are solved
                if (comp.Modules.All(m => m.IsSolved))
                {
                    DefuseBomb(uid, args.Actor, comp);
                }
            }
        }
        else
        {
            AddStrike(uid, args.Actor, comp);
        }

        UpdateUiState(uid, comp);
    }

    private void OnUiClosed(EntityUid uid, BombDefusalComponent comp, BoundUIClosedEvent args)
    {
        // Closing the UI while the bomb is active and not defused = 1 strike
        if (comp.IsDefused || !HasComp<ActiveTimerTriggerComponent>(uid))
            return;

        // Make sure this is the defusal UI, not some other UI
        if (args.UiKey is not BombDefusalUiKey)
            return;

        _popup.PopupEntity(Loc.GetString("bomb-defusal-popup-strike-exit", ("name", uid)), uid, args.Actor, PopupType.MediumCaution);
        AddStrike(uid, args.Actor, comp);
        UpdateUiState(uid, comp);
    }

    #endregion

    #region Public API

    public void TryStartCountdown(EntityUid uid, EntityUid user, BombDefusalComponent comp)
    {
        if (comp.IsDefused)
        {
            _popup.PopupEntity(Loc.GetString("bomb-defusal-popup-already-defused", ("name", uid)), uid);
            return;
        }

        var xform = Transform(uid);
        if (!xform.Anchored)
            _transform.AnchorEntity(uid, xform);

        // Generate modules based on timer
        GenerateModules(uid, comp);

        // Start the timer
        if (TryComp<TimerTriggerComponent>(uid, out var timerTrigger))
        {
            _trigger.ActivateTimerTrigger((uid, timerTrigger));
        }

        _popup.PopupEntity(Loc.GetString("defusable-popup-begun", ("name", uid)), uid);

        _appearance.SetData(uid, DefusableVisuals.Active, true);

        _adminLogger.Add(LogType.Explosion, LogImpact.High,
            $"{ToPrettyString(user):user} armed bomb {ToPrettyString(uid):entity} with {comp.Modules.Count} modules");

        UpdateUiState(uid, comp);
    }

    public void AddStrike(EntityUid uid, EntityUid? user, BombDefusalComponent comp)
    {
        comp.Strikes++;

        _audio.PlayPvs(comp.StrikeSound, uid);

        var userStr = user != null ? ToPrettyString(user.Value) : "unknown";
        _adminLogger.Add(LogType.Explosion, LogImpact.Medium,
            $"Strike {comp.Strikes}/{comp.MaxStrikes} on {ToPrettyString(uid):entity} by {userStr}");

        if (comp.Strikes >= comp.MaxStrikes)
        {
            DetonateBomb(uid, user, comp);
        }
    }

    public void DefuseBomb(EntityUid uid, EntityUid? user, BombDefusalComponent comp)
    {
        comp.IsDefused = true;

        // Stop the timer
        RemComp<ActiveTimerTriggerComponent>(uid);

        // Unanchor
        var xform = Transform(uid);
        if (xform.Anchored)
            _transform.Unanchor(uid, xform);

        _audio.PlayPvs(comp.DefuseSound, uid);
        _popup.PopupEntity(Loc.GetString("bomb-defusal-popup-defused", ("name", uid)), uid);

        _appearance.SetData(uid, DefusableVisuals.Active, false);

        if (user != null)
        {
            _adminLogger.Add(LogType.Explosion, LogImpact.High,
                $"{ToPrettyString(user.Value):user} defused bomb {ToPrettyString(uid):entity}!");
        }

        UpdateUiState(uid, comp);
    }

    public void DetonateBomb(EntityUid uid, EntityUid? user, BombDefusalComponent comp)
    {
        _popup.PopupEntity(Loc.GetString("bomb-defusal-popup-detonated", ("name", uid)), uid, PopupType.LargeCaution);

        if (user != null)
        {
            _adminLogger.Add(LogType.Explosion, LogImpact.Extreme,
                $"Bomb {ToPrettyString(uid):entity} detonated (3 strikes) by {ToPrettyString(user.Value):user}");
        }

        _explosion.TriggerExplosive(uid, user: user);
        QueueDel(uid);
    }

    #endregion

    #region UI

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Periodically update UI for active bombs (timer display)
        var query = EntityQueryEnumerator<BombDefusalComponent, ActiveTimerTriggerComponent>();
        while (query.MoveNext(out var uid, out var comp, out _))
        {
            if (comp.IsDefused)
                continue;

            UpdateUiState(uid, comp);
        }
    }

    private void UpdateUiState(EntityUid uid, BombDefusalComponent comp)
    {
        if (!_ui.HasUi(uid, BombDefusalUiKey.Key))
            return;

        var moduleStates = new List<BombDefusalModuleState>();
        foreach (var module in comp.Modules)
        {
            moduleStates.Add(module.GetVisibleState());
        }

        var remaining = _trigger.GetRemainingTime(uid);

        var state = new BombDefusalUiState
        {
            Modules = moduleStates,
            Strikes = comp.Strikes,
            MaxStrikes = comp.MaxStrikes,
            SerialNumber = comp.SerialNumber,
            RemainingTime = remaining != null ? (float) remaining.Value.TotalSeconds : 0f,
            IsActive = HasComp<ActiveTimerTriggerComponent>(uid) && !comp.IsDefused,
        };

        _ui.SetUiState(uid, BombDefusalUiKey.Key, state);
    }

    #endregion
}
