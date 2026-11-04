// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Examine;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Shared.Emp;

public abstract partial class SharedEmpSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EmpDisabledComponent, ExaminedEvent>(OnExamine);
    }

    protected const string EmpDisabledEffectPrototype = "EffectEmpDisabled";

    public virtual void EmpPulse(MapCoordinates coordinates, float range, float energyConsumption, float duration)
    {
    }

    private void OnExamine(Entity<EmpDisabledComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("emp-disabled-comp-on-examine"));
    }
}
