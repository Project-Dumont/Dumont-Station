using Content.Shared._Dumont.Xenomorphs;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Content.Shared.Alert;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Explosion;
using Robust.Shared.Prototypes;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class XenomorphArmorSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _prototypes = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private INetManager _net = default!;
    public static readonly ProtoId<AlertPrototype> ArmorAlert = "DumontXenomorphArmor";
    private static readonly ProtoId<DamageGroupPrototype> ArmorGroup = "Brute";

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphArmorComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<XenomorphArmorComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<XenomorphArmorComponent, XenomorphArmorAlertEvent>(OnAlert);
        SubscribeLocalEvent<XenomorphArmorComponent, DamageModifyEvent>(OnDamage);
        SubscribeLocalEvent<XenomorphArmorComponent, GetExplosionResistanceEvent>(OnExplosion);
    }

    private void OnInit(Entity<XenomorphArmorComponent> ent, ref MapInitEvent args) => RefreshAlert(ent);

    private void OnShutdown(Entity<XenomorphArmorComponent> ent, ref ComponentShutdown args)
    {
        if (!_net.IsClient)
            _alerts.ClearAlert(ent, ArmorAlert);
    }

    public (int General, int Frontal, int Side, int Explosion) GetArmor(Entity<XenomorphArmorComponent> ent)
    {
        var general = ent.Comp.Armor;
        if (TryComp<XenomorphVenatorComponent>(ent, out var venator)) general -= venator.AcidCharges * 5;
        if (TryComp<XenomorphDrainSurgeComponent>(ent, out var surge) && surge.Running) general += surge.Armor;
        if (TryComp<XenomorphTantrumComponent>(ent, out var tantrum) && tantrum.Running) general += tantrum.Armor;
        if (TryComp<XenomorphRageComponent>(ent, out var rage))
            general += rage.Rage * 4;
        if (TryComp<XenomorphHedgehogComponent>(ent, out var hedgehog))
            general += (int)(hedgehog.Shards / 50 * 2.5f);
        if (TryComp<XenomorphCarrionMantleComponent>(ent, out var mantle) && mantle.Active)
            general += mantle.Armor;
        if (TryComp<XenomorphSpitterComponent>(ent, out var spitter) && spitter.Charging)
            general += 5;
        if (TryComp<XenomorphDespoilerComponent>(ent, out var despoiler))
            general += despoiler.Stacks / 2 * 5;
        var frontal = ent.Comp.FrontalArmor;
        var explosion = ent.Comp.ExplosionArmor;
        if (TryComp<XenomorphCrestComponent>(ent, out var crest) && crest.Lowered)
            general += crest.Armor;
        if (TryComp<XenomorphFortifyComponent>(ent, out var fortify) && fortify.Fortified)
        {
            general += fortify.Armor;
            frontal += fortify.FrontalArmor;
            explosion += fortify.ExplosionArmor;
        }
        var side = ent.Comp.SideArmor;
        if (TryComp<XenomorphBulwarkComponent>(ent, out var bulwark))
        {
            frontal += bulwark.PassiveFrontalArmor;
            side += bulwark.PassiveSideArmor;
            if (bulwark.Encased)
            {
                frontal += bulwark.EncasedFrontalArmor;
                side += bulwark.EncasedSideArmorPenalty;
            }
        }
        return (general, general + frontal, general + side, explosion);
    }

    public void RefreshAlert(EntityUid uid)
    {
        if (_net.IsClient || !TryComp<XenomorphArmorComponent>(uid, out var armor))
            return;
        var value = GetArmor((uid, armor)).General;
        _alerts.ShowAlert(uid, ArmorAlert, (short) (11 - Math.Clamp(value / 5, 0, 11)));
    }

    private void OnAlert(Entity<XenomorphArmorComponent> ent, ref XenomorphArmorAlertEvent args)
    {
        if (_net.IsClient || args.Handled)
            return;
        var armor = GetArmor(ent);
        _popup.PopupEntity(Loc.GetString("xenomorph-armor-values",
            ("general", armor.General), ("front", armor.Frontal),
            ("side", armor.Side), ("explosion", armor.Explosion)), ent, ent);
        args.Handled = true;
    }

    private void OnExplosion(Entity<XenomorphArmorComponent> ent, ref GetExplosionResistanceEvent args)
    {
        var armor = GetArmor(ent).Explosion;
        if (armor > 0)
            args.DamageCoefficient /= (float) Math.Pow(1.1, armor / 5.0);
    }

    private void OnDamage(EntityUid uid, XenomorphArmorComponent component, DamageModifyEvent args)
    {
        var values = GetArmor((uid, component));
        var armor = values.General;
        if (args.Origin is { } origin && Exists(origin))
        {
            var source = _transform.GetMapCoordinates(origin);
            var target = _transform.GetMapCoordinates(uid);
            if (source.MapId == target.MapId && source.Position != target.Position)
            {
                var incoming = (source.Position - target.Position).ToWorldAngle().GetCardinalDir();
                var facing = _transform.GetWorldRotation(uid).GetCardinalDir();
                if (incoming == facing)
                    armor = values.Frontal;
                else if (incoming != facing.GetOpposite())
                    armor = values.Side;
            }
        }

        if (armor <= 0 || component.DamageMultiplier <= 0)
            return;

        var damage = new DamageSpecifier(args.Damage);
        var types = _prototypes.Index(ArmorGroup).DamageTypes;
        var resistance = Math.Pow(1.1, armor / 5.0);
        foreach (var type in types)
        {
            if (damage.DamageDict.TryGetValue(type, out var amount) && amount > FixedPoint2.Zero)
                damage.DamageDict[type] = amount / resistance;
        }

        var total = damage.GetTotal();
        if (component.FlatReduction && total > FixedPoint2.Zero && total < armor * 2)
        {
            var reduced = FixedPoint2.Max(0, total * component.DamageMultiplier - armor);
            foreach (var type in types)
            {
                if (damage.DamageDict.TryGetValue(type, out var amount) && amount > FixedPoint2.Zero)
                    damage.DamageDict[type] = amount * reduced / (total * component.DamageMultiplier);
            }
        }

        var penetration = Math.Clamp(args.Damage.ArmorPenetration, 0f, 1f);
        foreach (var type in types)
        {
            if (damage.DamageDict.TryGetValue(type, out var reduced)
                && args.Damage.DamageDict.TryGetValue(type, out var original) && original > FixedPoint2.Zero)
                damage.DamageDict[type] = reduced + (original - reduced) * penetration;
        }
        args.Damage = damage;
    }
}
