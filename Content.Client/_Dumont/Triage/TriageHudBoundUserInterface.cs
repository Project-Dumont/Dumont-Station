// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Dumont.Triage;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._Dumont.Triage;

/// <summary>
/// Janela de triagem do medhud, é a mesma janela do marcador mas aberta no paciente
/// As prioridades vem do medhud de quem está olhando e o paciente é o dono
/// </summary>
public sealed partial class TriageHudBoundUserInterface : BoundUserInterface
{
    [Dependency] private IEntityManager _entManager = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private IPlayerManager _player = default!;

    [ViewVariables]
    private TriageTaggerWindow? _window;

    public TriageHudBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<TriageTaggerWindow>();
        _window.OnLevelPicked += level => SendMessage(new TriageTaggerSetMessage(level));

        Update();
    }

    public override void Update()
    {
        base.Update();

        if (_window == null || _player.LocalEntity is not { } user)
            return;

        var levels = new List<TriageIconPrototype>();

        if (_entManager.System<TriageHudSystem>().TryGetHud(user, out var hud))
        {
            foreach (var id in hud.Levels)
            {
                if (_proto.TryIndex(id, out var level))
                    levels.Add(level);
            }
        }

        var patient = Identity.Name(Owner, _entManager);

        if (_entManager.TryGetComponent(Owner, out HumanoidAppearanceComponent? humanoid) &&
            _proto.TryIndex(humanoid.Species, out var species))
        {
            patient = Loc.GetString("triage-tagger-patient",
                ("name", patient),
                ("species", Loc.GetString(species.Name)));
        }

        ProtoId<TriageIconPrototype>? current = null;

        if (_entManager.TryGetComponent(Owner, out TriageTagComponent? tag))
            current = tag.Level;

        _window.Update(patient, levels, current);
    }
}
