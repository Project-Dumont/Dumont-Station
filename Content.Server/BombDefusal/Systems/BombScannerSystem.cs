using System.Linq;
using Content.Server.BombDefusal.Components;
using Content.Server.Popups;
using Content.Shared.BombDefusal;
using Content.Shared.Hands;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Trigger.Components;
using Robust.Server.GameObjects;

namespace Content.Server.BombDefusal.Systems;

public sealed class BombScannerSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BombScannerComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<BombScannerComponent, GotUnequippedHandEvent>(OnDropped);
    }

    private void OnAfterInteract(EntityUid uid, BombScannerComponent comp, AfterInteractEvent args)
    {
        if (args.Handled || args.Target == null || !args.CanReach)
            return;

        var target = args.Target.Value;

        if (!TryComp<BombDefusalComponent>(target, out var bombComp))
            return;

        if (!HasComp<ActiveTimerTriggerComponent>(target) || bombComp.IsDefused)
        {
            _popup.PopupEntity(Loc.GetString("bomb-scanner-not-armed"), target, args.User, PopupType.MediumCaution);
            args.Handled = true;
            return;
        }

        // Generate ruleset if it is not generated yet
        if (bombComp.RuleSet == null)
        {
            var defusalSystem = EntityManager.System<BombDefusalSystem>();
            defusalSystem.GenerateRuleSet(target, bombComp);
        }

        comp.ScannedBomb = target;
        _ui.OpenUi(uid, BombScannerUiKey.Key, args.User);
        _ui.SetUiState(uid, BombScannerUiKey.Key, new BombScannerUiState(bombComp.RuleSet));

        _popup.PopupEntity(Loc.GetString("bomb-scanner-scan-popup", ("name", target)), target, args.User);

        args.Handled = true;
    }

    private void OnDropped(EntityUid uid, BombScannerComponent comp, GotUnequippedHandEvent args)
    {
        _ui.CloseUi(uid, BombScannerUiKey.Key);
        comp.ScannedBomb = null;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BombScannerComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.ScannedBomb == null)
                continue;

            var bomb = comp.ScannedBomb.Value;

            if (Deleted(bomb) || !TryComp<BombDefusalComponent>(bomb, out var bombComp) || bombComp.IsDefused)
            {
                _ui.CloseUi(uid, BombScannerUiKey.Key);
                comp.ScannedBomb = null;
                continue;
            }

            var actors = _ui.GetActors(uid, BombScannerUiKey.Key);
            if (!actors.Any())
                continue;

            var bombPos = Transform(bomb).MapPosition;

            foreach (var actor in actors)
            {
                var actorPos = Transform(actor).MapPosition;
                if (bombPos.MapId != actorPos.MapId || (bombPos.Position - actorPos.Position).Length() > 3.0f)
                {
                    _ui.CloseUi(uid, BombScannerUiKey.Key, actor);
                }
            }
        }
    }
}
