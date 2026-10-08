// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using System.Text;
using Content.Server.Antag;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Server.RoundEnd;
using Content.Server.AlertLevel;
using Content.Server.Chat.Systems;
using Content.Shared.Inventory;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Goobstation.Shared.Clothing.Components;
using Content.Goobstation.Shared.Clothing.Systems;
using Robust.Shared.Player;
using Content.Shared.GameTicking;
using Content.Server.Objectives;
using Content.Server.Roles;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Content.Shared.Station.Components;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Content.Trauma.Server.Heretic.Components;
using Content.Trauma.Shared.Heretic.Components;
using Content.Trauma.Shared.Heretic.Events;
using Content.Trauma.Server.Objectives.Components;
using Content.Trauma.Shared.Heretic.Systems;
using Robust.Server.GameObjects;
using Robust.Server.Audio;
using Robust.Shared.Audio;
// Dumont end

namespace Content.Trauma.Server.Heretic.Systems;

public sealed partial class HereticRuleSystem : GameRuleSystem<HereticRuleComponent>
{
    [Dependency] private MindSystem _mind = default!;
    [Dependency] private AntagSelectionSystem _antag = default!;
    [Dependency] private SharedRoleSystem _role = default!;
    [Dependency] private ObjectivesSystem _objective = default!;
    [Dependency] private UserInterfaceSystem _ui = default!;
    [Dependency] private GameTicker _ticker = default!;

    // Dumont start
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;

    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly AlertLevelSystem _alerts = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly ToggleableClothingSystem _toggleable = default!;
    [Dependency] private readonly SharedSealableClothingSystem _sealable = default!;

    private const string VaticanAlertSound = "/Audio/Announcements/Alerts/code_octarine.ogg";
    private const string VaticanMusic = "/Audio/_Dumont/Heretic/cadiastands_inspection.ogg";
    private bool _ascensionResponseCalled;
    private EntityUid? _pendingResponse;
    private TimeSpan _responseAt;
    private TimeSpan? _musicAt;

    [SubscribeLocalEvent]
    private void OnRoundRestart(RoundRestartCleanupEvent args)
    {
        _ascensionResponseCalled = false;
        _pendingResponse = null;
        _musicAt = null;
    }
    // Dumont end

    public static readonly SoundSpecifier BriefingSound =
        new SoundPathSpecifier("/Audio/_Goobstation/Heretic/Ambience/Antag/Heretic/heretic_gain.ogg");

    public static readonly SoundSpecifier BriefingSoundIntense =
        new SoundPathSpecifier("/Audio/_Goobstation/Heretic/Ambience/Antag/Heretic/heretic_gain_intense.ogg");

    public static EntProtoId MindRole = "MindRoleHeretic";

    public static EntProtoId RealityShift = "EldritchInfluence";

    [SubscribeLocalEvent]
    private void OnGetBriefing(Entity<HereticRoleComponent> ent, ref GetBriefingEvent args)
    {
        var uid = args.Mind.Comp.OwnedEntity;

        if (uid == null)
            return;

        var briefingShort = Loc.GetString("heretic-role-greeting-short");
        args.Append(briefingShort);
    }

    [SubscribeLocalEvent]
    private void OnAntagSelect(Entity<HereticRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        TryMakeHeretic(args.EntityUid, ent.Comp);

        SpawnInfluence(ent.Comp.RealityShiftPerHeretic);
    }

    public void SpawnInfluence(int amount)
    {
        if (amount <= 0)
            return;

        if (!TryGetRandomStation(out var station))
            return;

        if (GetStationMainGrid(Comp<StationDataComponent>(station.Value)) is not { } grid)
            return;

        for (var i = 0; i < amount; i++)
        {
            if (TryFindTileOnGrid(grid, out _, out var coords))
                Spawn(RealityShift, coords);
        }
    }

    public bool TryMakeHeretic(EntityUid target, HereticRuleComponent rule)
    {
        if (!_mind.TryGetMind(target, out var mindId, out var mind))
            return false;

        _role.MindAddRole(mindId, MindRole.Id, mind, true);

        // briefing
        if (HasComp<MetaDataComponent>(target))
        {
            _antag.SendBriefing(target, Loc.GetString("heretic-role-greeting-fluff"), Color.MediumPurple, null);
            _antag.SendBriefing(target, Loc.GetString("heretic-role-greeting"), Color.Red, BriefingSound);
        }

        // add store
        InitializeStore(mindId);

        // heretic after store because it requires store on startup
        EnsureComp<HereticComponent>(mindId);

        rule.Minds.Add(mindId);

        _ui.SetUi(mindId, StoreUiKey.Key, new InterfaceData("StoreBoundUserInterface", -1));
        _ui.SetUi(mindId, HereticLivingHeartKey.Key, new InterfaceData("LivingHeartMenuBoundUserInterface", -1));

        return true;
    }

    public StoreComponent InitializeStore(EntityUid mindId)
    {
        var store = EnsureComp<StoreComponent>(mindId);
        foreach (var category in HereticRuleComponent.StoreCategories)
        {
            store.Categories.Add(category);
        }

        store.CurrencyWhitelist.Add(SharedHereticSystem.Currency);
        store.CurrencyWhitelist.Add(SharedHereticSystem.SideCurrency);
        store.Balance[SharedHereticSystem.SideCurrency] = 1; // 1 free side point
        return store;
    }

    [SubscribeLocalEvent]
    private void OnTextPrepend(Entity<HereticRuleComponent> ent, ref ObjectivesTextPrependEvent args)
    {
        var sb = new StringBuilder();

        var mostKnowledge = 0f;
        var mostKnowledgeName = string.Empty;

        var query = EntityQueryEnumerator<HereticComponent, MindComponent>();
        while (query.MoveNext(out var mindId, out var heretic, out var mind))
        {
            var name = _objective.GetTitle((mindId, mind), Name(mind.OwnedEntity ?? mindId));
            if (_mind.TryGetObjectiveComp<HereticKnowledgeConditionComponent>(mindId, out var objective, mind))
            {
                if (objective.Researched > mostKnowledge)
                    mostKnowledge = objective.Researched;
                mostKnowledgeName = name;
            }

            var message =
                $"roundend-prepend-heretic-ascension-{(heretic.Ascended ? "success" : heretic.CanAscend ? "fail" : "fail-owls")}";
            var str = Loc.GetString(message, ("name", name));
            sb.AppendLine(str);
        }

        sb.AppendLine("\n" + Loc.GetString("roundend-prepend-heretic-knowledge-named",
            ("name", mostKnowledgeName),
            ("number", mostKnowledge)));

        args.Text = sb.ToString();
    }

    // Dumont start
    public void SpawnERTOnAscension()
    {
        if (_ascensionResponseCalled)
            return;

        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var rule, out _))
        {
            _ascensionResponseCalled = true;
            rule.HasAHereticAscended = true;

            _pendingResponse = rule.Owner;
            _responseAt = Timing.CurTime + rule.ResponseDelay;
            break;
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_pendingResponse is { } pending && Timing.CurTime >= _responseAt)
        {
            _pendingResponse = null;
            if (_ticker.RunLevel == GameRunLevel.InRound && TryComp<HereticRuleComponent>(pending, out var rule))
                SendVaticanResponse(rule);
        }

        if (_musicAt is { } musicAt && Timing.CurTime >= musicAt)
        {
            _musicAt = null;
            if (_ticker.RunLevel == GameRunLevel.InRound)
                _audio.PlayGlobal(VaticanMusic, Filter.Broadcast(), true, AudioParams.Default.WithVolume(-13f));
        }
    }

    private void SendVaticanResponse(HereticRuleComponent rule)
    {
        if (rule.ERTEvent is { } ertEvent)
        {
            if (_prototypes.HasIndex(ertEvent))
                _ticker.StartGameRule(ertEvent);
            else
                Log.Warning($"Heretic ascension ERT rule {ertEvent} does not exist.");
        }

        var stations = EntityQueryEnumerator<AlertLevelComponent, StationDataComponent>();
        while (stations.MoveNext(out var station, out _, out _))
            _alerts.SetLevel(station, "cataclysm", false, false, force: true, locked: true);

        _chat.DispatchGlobalAnnouncement(Loc.GetString("heretic-ascension-evacuation"),
            Loc.GetString("heretic-vatican-sender"), playSound: false, colorOverride: Color.FromHex("#FDFFCF"));
        _roundEnd.RecallLocked = true;
        if (!_roundEnd.IsRoundEndRequested())
            _roundEnd.RequestRoundEnd(rule.EvacuationDelay, checkCooldown: false, announce: false, playSound: false);

        _audio.PlayGlobal(VaticanAlertSound, Filter.Broadcast(), true);
        _musicAt = Timing.CurTime + _audio.GetAudioLength(VaticanAlertSound);
    }

    [SubscribeLocalEvent]
    private void OnInquisitorEquipped(Entity<VaticanInquisitorComponent> ent, ref StartingGearEquippedEvent args)
    {
        if (!_inventory.TryGetSlotEntity(ent, "back", out var control)
            || !TryComp<ToggleableClothingComponent>(control, out var toggleable))
            return;

        foreach (var (clothing, slot) in toggleable.ClothingUids)
            _toggleable.EquipClothing(ent, (control.Value, toggleable), clothing, slot, silent: true);

        if (TryComp<SealableClothingControlComponent>(control, out var sealable) && !sealable.IsCurrentlySealed)
            _sealable.TryStartSealToggleProcess((control.Value, sealable));
    }
    // Dumont end
}
