// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
// SPDX-FileCopyrightText: 2025 ss14-Starlight <ss14-Starlight@outlook.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Starlight.Weapon.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Examine;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed.Commands.Values;
using Robust.Shared.Utility;

namespace Content.Server.Weapons.Ranged.Systems;

public sealed partial class GunSystem
{
    protected override void InitializeCartridge()
    {
        base.InitializeCartridge();
        SubscribeLocalEvent<CartridgeAmmoComponent, ExaminedEvent>(OnCartridgeExamine);
        SubscribeLocalEvent<CartridgeAmmoComponent, DamageExamineEvent>(OnCartridgeDamageExamine);
        SubscribeLocalEvent<BasicEntityAmmoProviderComponent, DamageExamineEvent>(OnBasicEntityDamageExamine); // Goobstation
        SubscribeLocalEvent<HitScanCartridgeAmmoComponent, ExaminedEvent>(OnHitScanCartridgeExamine);
        SubscribeLocalEvent<HitScanCartridgeAmmoComponent, DamageExamineEvent>(OnHitScanCartridgeDamageExamine);
    }


    private void OnCartridgeDamageExamine(EntityUid uid, CartridgeAmmoComponent component, ref DamageExamineEvent args)
    {
        var damageSpec = GetProjectileDamage(component.Prototype);

        if (damageSpec == null)
            return;

        _damageExamine.AddDamageExamine(args.Message, Damageable.ApplyUniversalAllModifiers(damageSpec), Loc.GetString("damage-projectile"));

        // Goobstation - partial armor penetration
        var ap = GetProjectilePenetration(component.Prototype);
        if (ap == 0)
            return;

        var abs = Math.Abs(ap);
        args.Message.AddMarkupPermissive("\n" + Loc.GetString("armor-penetration", ("arg", ap/abs), ("abs", abs)));
    }

    private DamageSpecifier? GetProjectileDamage(string proto)
    {
        if (!ProtoManager.TryIndex<EntityPrototype>(proto, out var entityProto))
            return null;

        if (entityProto.Components
            .TryGetValue(Factory.GetComponentName<ProjectileComponent>(), out var projectile))
        {
            var p = (ProjectileComponent) projectile.Component;

            if (!p.Damage.Empty)
            {
                return p.Damage * Damageable.UniversalProjectileDamageModifier;
            }
        }

        return null;
    }

    private void OnCartridgeExamine(EntityUid uid, CartridgeAmmoComponent component, ExaminedEvent args)
    {
        if (component.Spent)
        {
            args.PushMarkup(Loc.GetString("gun-cartridge-spent"));
        }
        else
        {
            args.PushMarkup(Loc.GetString("gun-cartridge-unspent"));
        }
    }

    // Goobstation start - partial armor penetration
    private void OnBasicEntityDamageExamine(EntityUid uid, BasicEntityAmmoProviderComponent component, ref DamageExamineEvent args)
    {
        if (component.Proto == null)
            return;

        var damageSpec = GetProjectileDamage(component.Proto);

        if (damageSpec == null)
            return;

        _damageExamine.AddDamageExamine(args.Message, Damageable.ApplyUniversalAllModifiers(damageSpec), Loc.GetString("damage-projectile"));

        var ap = GetProjectilePenetration(component.Proto);
        if (ap == 0)
            return;

        var abs = Math.Abs(ap);
        args.Message.AddMarkupPermissive("\n" + Loc.GetString("armor-penetration", ("arg", ap/abs), ("abs", abs)));
    }
    private int GetProjectilePenetration(string proto)
    {
        if (!ProtoManager.TryIndex<EntityPrototype>(proto, out var entityProto)
        || !entityProto.Components.TryGetValue(Factory.GetComponentName<ProjectileComponent>(), out var projectile))
            return 0;

        var p = (ProjectileComponent) projectile.Component;

        return p.IgnoreResistances ? 100 : (int)Math.Round(p.Damage.ArmorPenetration * 100);
    }
    // Goobstation end

    private void OnHitScanCartridgeDamageExamine(EntityUid uid, HitScanCartridgeAmmoComponent component, ref DamageExamineEvent args)
    {
        var damageSpec = GetHitscanProjectileDamage(component.Hitscan);

        if (damageSpec == null)
            return;

        _damageExamine.AddDamageExamine(args.Message, Damageable.ApplyUniversalAllModifiers(damageSpec), Loc.GetString("damage-projectile"));

        // var ArmorMessage = GetArmorPenetrationExplain(component.Hitscan);

        // args.Message.AddMessage(ArmorMessage);

    }

    // TODO. Gaby Station. Portar https://github.com/ss14Starlight/space-station-14/pull/227 por completo
    // private FormattedMessage GetArmorPenetrationExplain(string proto)
    // {
    //     var msg = new FormattedMessage();
    //     if (!ProtoManager.TryIndex<HitscanPrototype>(proto, out var entityProto))
    //         return msg;

    //     if (entityProto.ArmorPenetration == 0)
    //         return msg;

    //     if (entityProto.ArmorPenetration > 0)
    //     {
    //         msg.PushNewline();
    //         msg.TryAddMarkup(Loc.GetString("damage-examine-penetration-positive", ("penetration", entityProto.ArmorPenetration * 100)), out var error);
    //     }

    //     if (entityProto.ArmorPenetration < 0)
    //     {
    //         msg.PushNewline();
    //         msg.TryAddMarkup(Loc.GetString("damage-examine-penetration-negative", ("penetration", entityProto.ArmorPenetration * -100)), out var error);
    //     }
    //     return msg;
    // }

    private DamageSpecifier? GetHitscanProjectileDamage(string proto) {
        if (!ProtoManager.TryIndex<HitscanPrototype>(proto, out var entityProto))
            return null;

        if (entityProto.Damage == null)
            return null;

        if (!entityProto.Damage.Empty)
            return entityProto.Damage * Damageable.UniversalHitscanDamageModifier;

        return null;
    }

    private void OnHitScanCartridgeExamine(EntityUid uid, HitScanCartridgeAmmoComponent component, ExaminedEvent args)
    {
        if (component.Spent)
            args.PushMarkup(Loc.GetString("gun-cartridge-spent"));
        else
            args.PushMarkup(Loc.GetString("gun-cartridge-unspent"));
    }
}
