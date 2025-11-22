// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Construction.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Popups;
using Content.Server.Radio.Components;
using Content.Shared._Gabystation.MalfAi;
using Content.Shared._Gabystation.MalfAi.Components;
using Content.Shared._Gabystation.MalfAi.Events;
using Content.Shared.Destructible;
using Content.Shared.Explosion.EntitySystems;
using Content.Shared.Maps;
using Content.Shared.Popups;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Utility;

namespace Content.Server._Gabystation.MalfAi;

public sealed partial class MalfAiSystem
{
    [Dependency] private readonly SharedStationAiSystem _ai = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedDestructibleSystem _destructible = default!;

    private void InitializeAbilities()
    {
        // Acho que esse evento pode ser mais genérico, pra qualquer entidade com IntrinsicRadioTransmitterComponent e ActiveRadioComponent.
        SubscribeLocalEvent<MalfunctioningAiComponent, RadioKeyUnlockedEvent>(OnRadioKeyUnlocked);
        SubscribeLocalEvent<MalfunctioningAiComponent, ExplodeMachineEvent>(OnExplodeMachine);
    }

    private void OnRadioKeyUnlocked(Entity<MalfunctioningAiComponent> malf, ref RadioKeyUnlockedEvent args)
    {
        if (TryComp<IntrinsicRadioTransmitterComponent>(malf.Owner, out var transmitter))
            transmitter.Channels.UnionWith(args.Channels);

        if (TryComp<ActiveRadioComponent>(malf.Owner, out var radio))
            radio.Channels.UnionWith(args.Channels);
    }

    private void OnExplodeMachine(Entity<MalfunctioningAiComponent> malf, ref ExplodeMachineEvent args)
    {
        // Isso funciona mas só permite que a IA exploda o que ela consegue interagir. Por padrão, a IA não consegue interagir com nenhuma máquina. Isto é, nenhuma máquina tem o componente `StationAiWhitelist`.
        // Uma solução é adicionar um novo campo nesse componente dividindo as formas de interação da IA. Tipo, um campo que diz se ela está permitida abrir a UI (falso em máquinas, positivo em todo o resto). Daí mesmo não podendo abrir as UIs, ela poderia interagir e, nessa situação, explodir.
        if (args.Handled)
            return;

        var coordinates = _transform.ToMapCoordinates(args.Target);
        var target = _lookup.GetEntitiesInRange(coordinates, 0.25f, LookupFlags.Uncontained)
            .Where(HasComp<MachineComponent>).FirstOrNull();

        if (target is not { } machine)
        {
            PopupAi(malf.Owner, "malfai-overload-invalid-target");
            return;
        }

        if (!_power.IsPowered(machine))
        {
            PopupAi(malf.Owner, "malfai-overload-not-powered");
            return;
        }

        var totalIntensity = _explosion.RadiusToIntensity(args.Radius, args.Slope, args.MaxIntensity);

        _explosion.QueueExplosion(
            machine,
            ExplosionSystem.DefaultExplosionPrototypeId,
            totalIntensity,
            args.Slope,
            args.MaxIntensity,
            user: malf.Owner
        );
        _destructible.DestroyEntity(machine); // Não queria que fosse deletada, só que quebrasse. Dificil.

        PopupAi(malf.Owner, "malfai-overload-success");
        args.Handled = true;
    }

    private void PopupAi(Entity<StationAiHeldComponent?> ai, LocId message, PopupType type = PopupType.Small)
    {
        if (!Resolve(ai.Owner, ref ai.Comp)
            || !_ai.TryGetCore(ai.Owner, out var aiCore)
            || aiCore.Comp is null)
            return;

        var popupTarget = aiCore.Comp.RemoteEntity ?? ai.Owner;

        _popup.PopupEntity(Loc.GetString(message), popupTarget, type);
    }
}
