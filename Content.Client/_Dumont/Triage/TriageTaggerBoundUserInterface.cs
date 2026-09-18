// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Dumont.Triage;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._Dumont.Triage;

/// <summary>
/// Lê o alvo e a marcação direto dos componentes em vez do estado da janela, porque o estado
/// pode chegar uma tick depois da janela abrir.
/// </summary>
public sealed class TriageTaggerBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    [ViewVariables]
    private TriageTaggerWindow? _window;

    public TriageTaggerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
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

        if (_window == null || !_entManager.TryGetComponent(Owner, out TriageTaggerComponent? tagger))
            return;

        var levels = new List<TriageIconPrototype>();

        foreach (var id in tagger.Levels)
        {
            if (_proto.TryIndex(id, out var level))
                levels.Add(level);
        }

        var patient = string.Empty;
        ProtoId<TriageIconPrototype>? current = null;

        if (tagger.Target is { } netTarget && _entManager.TryGetEntity(netTarget, out var target))
        {
            patient = Identity.Name(target.Value, _entManager);

            if (_entManager.TryGetComponent(target.Value, out HumanoidAppearanceComponent? humanoid) &&
                _proto.TryIndex(humanoid.Species, out var species))
            {
                patient = Loc.GetString("triage-tagger-patient",
                    ("name", patient),
                    ("species", Loc.GetString(species.Name)));
            }

            if (_entManager.TryGetComponent(target.Value, out TriageTagComponent? tag))
                current = tag.Level;
        }

        _window.Update(patient, levels, current);
    }
}
