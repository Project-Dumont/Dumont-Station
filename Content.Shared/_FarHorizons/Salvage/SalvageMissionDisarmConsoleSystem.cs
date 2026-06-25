using Content.Shared._FarHorizons.Salvage.Components;
using Content.Shared.UserInterface;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._FarHorizons.Salvage;

public sealed class SalvageMissionDisarmConsoleSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SalvageMissionDisarmConsoleComponent, ActivatableUIOpenAttemptEvent>(OnUiOpenAttempt);
        SubscribeLocalEvent<SalvageMissionDisarmConsoleComponent, SalvageMissionDisarmSubmitCodeMessage>(OnSubmitCode);
    }

    private void OnUiOpenAttempt(Entity<SalvageMissionDisarmConsoleComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        if (!ent.Comp.Enabled || !ent.Comp.Armed)
            args.Cancel();
    }

    private void OnSubmitCode(Entity<SalvageMissionDisarmConsoleComponent> ent, ref SalvageMissionDisarmSubmitCodeMessage args)
    {
        if (args.Code != ent.Comp.Code)
        {
            _audio.PlayPvs(ent.Comp.FailSound, ent.Owner);
            return;
        }

        _audio.PlayPvs(ent.Comp.SuccessSound, ent.Owner);
        ent.Comp.Armed = false;
        _appearance.SetData(ent, SalvageMissionDisarmConsoleVisuals.Armed, false);
        _ui.CloseUis(ent.Owner);
    }

    public void SetupConsole(Entity<SalvageMissionDisarmConsoleComponent?> ent, int code)
    {
        if (!Resolve(ent, ref ent.Comp) ||
            !TryComp<AppearanceComponent>(ent, out var appearance))
            return;

        ent.Comp.Enabled = true;
        ent.Comp.Armed = true;
        ent.Comp.Code = code;

        _appearance.SetData(ent, SalvageMissionDisarmConsoleVisuals.Enabled, true, appearance);
        _appearance.SetData(ent, SalvageMissionDisarmConsoleVisuals.Armed, true, appearance);
    }
}