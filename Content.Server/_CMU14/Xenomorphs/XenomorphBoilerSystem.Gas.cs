using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Infection;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Buckle.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Drunk;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Jittering;
using Content.Shared.Maps;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Rejuvenate;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.Stunnable;
using Robust.Shared.Map;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class XenomorphBoilerGasComponent : Component
{
    [DataField] public bool Neurotoxin;
    [DataField] public int Radius = 3;
    [DataField] public int Step;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan NextSpread;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan NextDamage;
    public EntityUid? Shooter;
    public readonly HashSet<Vector2i> Filled = new();
    public readonly List<EntityUid> Tiles = new();
}

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class XenomorphBoilerGasExposureComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan NextAcid;
}

public sealed partial class XenomorphBoilerSystem
{
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private TurfSystem _turf = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private SharedStaminaSystem _stamina = default!;
    [Dependency] private SharedDrunkSystem _drunk = default!;
    [Dependency] private SharedJitteringSystem _jitter = default!;
    [Dependency] private SharedStutteringSystem _stutter = default!;
    [Dependency] private SharedInternalsSystem _internals = default!;
    [Dependency] private BlurryVisionSystem _blur = default!;
    [Dependency] private MovementModStatusSystem _slow = default!;
    [Dependency] private Content.Shared.StatusEffectNew.StatusEffectsSystem _status = default!;
    private readonly HashSet<Entity<MobStateComponent>> _victims = new();

    private void InitializeGas()
    {
        SubscribeLocalEvent<XenomorphBoilerGasComponent, EntityTerminatingEvent>(OnGasTerminating);
        SubscribeLocalEvent<XenomorphNeurotoxinComponent, RejuvenateEvent>(OnRejuvenate);
    }

    private bool Victim(EntityUid uid) => HasComp<MobStateComponent>(uid) && !_mobs.IsDead(uid)
        && !HasComp<XenomorphComponent>(uid) && !HasComp<SiliconComponent>(uid) && !_containers.IsEntityInContainer(uid)
        && !(HasComp<XenomorphInfectedComponent>(uid) && TryComp<BuckleComponent>(uid, out var buckle)
            && TryComp<XenomorphNestComponent>(buckle.BuckledTo, out _));

    private void CreateCloud(EntityCoordinates coordinates, bool neurotoxin, int radius, EntityUid? shooter)
    {
        var map = _transform.ToMapCoordinates(coordinates);
        var grid = Transform(coordinates.EntityId).GridUid ?? coordinates.EntityId;
        if (!HasComp<Robust.Shared.Map.Components.MapGridComponent>(grid) || TerminatingOrDeleted(grid)
            || Transform(grid).MapUid is not { } mapUid || TerminatingOrDeleted(mapUid)) return;
        var local = _transform.ToCoordinates(grid, map);
        coordinates = new EntityCoordinates(grid, MathF.Floor(local.X) + 0.5f, MathF.Floor(local.Y) + 0.5f);
        var cloud = Spawn("DumontXenomorphBoilerCloud", coordinates);
        var comp = Comp<XenomorphBoilerGasComponent>(cloud);
        comp.Neurotoxin = neurotoxin;
        comp.Radius = radius;
        comp.Shooter = shooter;
        SpreadGas((cloud, comp));
    }

    private bool GasTile(EntityCoordinates coordinates)
    {
        var tile = _turf.GetTileRef(coordinates);
        return tile is { } floor && !floor.Tile.IsEmpty && !_turf.IsTileBlocked(floor,
            Content.Shared.Physics.CollisionGroup.Impassable | Content.Shared.Physics.CollisionGroup.MidImpassable | Content.Shared.Physics.CollisionGroup.HighImpassable);
    }

    private void SpreadGas(Entity<XenomorphBoilerGasComponent> ent)
    {
        var center = Transform(ent).Coordinates;
        var candidates = new HashSet<Vector2i>();
        if (ent.Comp.Filled.Count == 0) candidates.Add(Vector2i.Zero);
        else foreach (var tile in ent.Comp.Filled)
            foreach (var direction in new[]{new Vector2i(0,1),new Vector2i(1,0),new Vector2i(0,-1),new Vector2i(-1,0)})
                candidates.Add(tile + direction);
        foreach (var offset in candidates)
        {
            if (ent.Comp.Filled.Contains(offset) || offset.LengthSquared > ent.Comp.Radius * ent.Comp.Radius) continue;
            var tile = center.Offset(new Vector2(offset.X, offset.Y));
            if (!GasTile(tile)) continue;
            ent.Comp.Filled.Add(offset);
            ent.Comp.Tiles.Add(Spawn(ent.Comp.Neurotoxin ? "DumontXenomorphBoilerNeuroGas" : "DumontXenomorphBoilerAcidGas", tile));
        }
        ent.Comp.NextSpread = _timing.CurTime + TimeSpan.FromSeconds(0.5);
        ent.Comp.Step++;
    }

    private void OnGasTerminating(Entity<XenomorphBoilerGasComponent> ent, ref EntityTerminatingEvent args)
    {
        foreach (var tile in ent.Comp.Tiles) if (Exists(tile)) QueueDel(tile);
    }

    private void InjectNeuro(EntityUid target, float amount, bool contact)
    {
        if (!Victim(target) || contact && _internals.AreInternalsWorking(target)) return;
        var neuro = EnsureComp<XenomorphNeurotoxinComponent>(target);
        if (contact && _timing.CurTime < neuro.NextInjection) return;
        neuro.NextInjection = _timing.CurTime + TimeSpan.FromSeconds(1);
        neuro.Amount += amount;
        neuro.BlurUntil = _timing.CurTime + TimeSpan.FromSeconds(contact ? 24 : 12);
        Dirty(target, neuro);
        _blur.UpdateBlurMagnitude(target);
        _slow.TryUpdateMovementSpeedModDuration(target, "XenomorphNeurotoxinSlowdown", TimeSpan.FromSeconds(6), 0.8f);
    }

    private void OnRejuvenate(Entity<XenomorphNeurotoxinComponent> ent, ref RejuvenateEvent args) => RemCompDeferred<XenomorphNeurotoxinComponent>(ent);

    private void UpdateGas()
    {
        var gas = EntityQueryEnumerator<XenomorphBoilerGasComponent>();
        while (gas.MoveNext(out var uid, out var cloud))
        {
            if (cloud.Step <= cloud.Radius * 2 && _timing.CurTime >= cloud.NextSpread) SpreadGas((uid, cloud));
            if (_timing.CurTime < cloud.NextDamage) continue;
            cloud.NextDamage = _timing.CurTime + TimeSpan.FromSeconds(1);
            foreach (var tile in cloud.Tiles)
            {
                if (!Exists(tile)) continue;
                _victims.Clear();
                _lookup.GetEntitiesInRange(_transform.GetMapCoordinates(tile), 0.55f, _victims);
                foreach (var victim in _victims)
                {
                    if (!Victim(victim)) continue;
                    if (cloud.Neurotoxin)
                    {
                        if (_internals.AreInternalsWorking(victim)) continue;
                        var neuro = CompOrNull<XenomorphNeurotoxinComponent>(victim);
                        if (neuro != null && _timing.CurTime < neuro.NextInjection) continue;
                        InjectNeuro(victim, 6, true);
                        _damage.TryChangeDamage(victim, new DamageSpecifier { DamageDict = { ["Asphyxiation"] = 5 } }, origin: cloud.Shooter);
                    }
                    else
                    {
                        var exposure = EnsureComp<XenomorphBoilerGasExposureComponent>(victim);
                        if (_timing.CurTime < exposure.NextAcid) continue;
                        exposure.NextAcid = _timing.CurTime + TimeSpan.FromSeconds(1);
                        _damage.TryChangeDamage(victim, new DamageSpecifier { DamageDict = { ["Caustic"] = 35 } }, origin: cloud.Shooter);
                        _damage.TryChangeDamage(victim, new DamageSpecifier { DamageDict = { ["Caustic"] = 20, ["Asphyxiation"] = 3 }, ArmorPenetration = 1 }, origin: cloud.Shooter);
                        _spitter.ShowAcidSplash(victim);
                    }
                }
            }
        }
        var query = EntityQueryEnumerator<XenomorphNeurotoxinComponent>();
        while (query.MoveNext(out var uid, out var neuro))
        {
            if (_timing.CurTime < neuro.NextEffect) continue;
            neuro.NextEffect = _timing.CurTime + TimeSpan.FromSeconds(1);
            neuro.Amount = Math.Max(0, neuro.Amount - 1);
            Dirty(uid, neuro);
            if (neuro.Amount <= 0)
            {
                if (_timing.CurTime >= neuro.BlurUntil) RemCompDeferred<XenomorphNeurotoxinComponent>(uid);
                continue;
            }
            if (!Victim(uid)) continue;
            _stamina.TakeStaminaDamage(uid, 7, visual: false);
            _drunk.TryApplyDrunkenness(uid, 12, false);
            if (neuro.Amount >= 10) _stutter.DoStutter(uid, TimeSpan.FromSeconds(20), true);
            if (neuro.Amount >= 15) _jitter.DoJitter(uid, TimeSpan.FromSeconds(15), true);
            if (neuro.Amount >= 20) _status.TryUpdateStatusEffectDuration(uid, "TemporaryBlindness", TimeSpan.FromSeconds(2));
            if (neuro.Amount >= 27)
            {
                _slow.TryUpdateMovementSpeedModDuration(uid, "XenomorphNeurotoxinSlowdown", TimeSpan.FromSeconds(2), 0.6f);
                _damage.TryChangeDamage(uid, new DamageSpecifier { DamageDict = { ["Poison"] = 1, ["Radiation"] = 1 } });
            }
            if (neuro.Amount >= 50) _damage.TryChangeDamage(uid, new DamageSpecifier { DamageDict = { ["Asphyxiation"] = 150 } });
        }
    }
}
