using Content.Shared.Interaction.Events;
using Content.Shared.DoAfter;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Random;
using Robust.Shared.Audio.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Network;

namespace Content.Shared._Dumont.Weapons.Ranged.Systems;

public abstract partial class SharedWeaponJammingSystem : EntitySystem
{
    [Dependency] private IRobustRandom _rand = default!;
    [Dependency] protected SharedPopupSystem _popup = default!;
    [Dependency] protected SharedAudioSystem _audio = default!;
    [Dependency] private SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private ILogManager _log = default!;
    [Dependency] private INetManager _net = default!;



    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GunComponent, GunShotEvent>(AttemptJam);
        SubscribeLocalEvent<JammedGunComponent, UseInHandEvent>(AttemptUnjam);
        SubscribeLocalEvent<JammedGunComponent, UnjamDoAfterEvent>(UnjamDoAfter);
        SubscribeLocalEvent<JammedGunComponent, AttemptShootEvent>(AttemptShootJammed);

    }
    private void AttemptJam(Entity<GunComponent> gun, ref GunShotEvent ev)
    {
        ISawmill Log = _log.GetSawmill("gun");

        if (gun.Comp.Quality <= 0)
        {
            if (gun.Comp.TimeToUnjam > TimeSpan.Zero)
                Log.Warning($"Gun entity {gun.Owner} incapable of jamming yet has timeToUnjam bigger than zero");

            return;
        }

        if (!_net.IsServer)
            return;

        // rola rng apenas no server pois em shared isso pode causar estado divergente
        if (_rand.Next(1, gun.Comp.Quality + 1) == 1)
            Jam(gun, ev.User);
    }

    private void Jam(Entity<GunComponent> gun, EntityUid user)
    {
        if (TryComp<JammedGunComponent>(gun.Owner, out var _))
            return;

        var jammedComp = AddComp<JammedGunComponent>(gun.Owner);

        // dirty na arma ja que tamo no server
        Dirty(gun);
        GunJamEffect(gun.Owner, jammedComp, "gun-jammed", user);
    }

    private void AttemptUnjam(Entity<JammedGunComponent> jammedGun, ref UseInHandEvent args)
    {
        if (!TryComp<GunComponent>(jammedGun.Owner, out var gunComp)) { return; }


        if (gunComp.TimeToUnjam > TimeSpan.Zero)
        {
            _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, gunComp.TimeToUnjam, new UnjamDoAfterEvent(), jammedGun.Owner, used: jammedGun.Owner)
            {
                BreakOnMove = true,
                BreakOnDamage = true,
                BreakOnHandChange = true,
                NeedHand = true
            });
        }
        else
        {
            RemComp<JammedGunComponent>(jammedGun.Owner);
            _popup.PopupPredicted(Loc.GetString("gun-unjammed"), jammedGun.Owner, args.User);
        }

    }

    private void UnjamDoAfter(EntityUid gunUid, JammedGunComponent comp, UnjamDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled) { return; }

        RemComp<JammedGunComponent>(gunUid);
        _popup.PopupPredicted(Loc.GetString("gun-unjammed"), gunUid, args.User);
    }

    private void AttemptShootJammed(Entity<JammedGunComponent> gun, ref AttemptShootEvent ev)
    {
        ev.Cancelled = true;
        ev.Message = Loc.GetString("gun-is-jammed");
    }

    private void GunJamEffect(EntityUid gunUid, JammedGunComponent jamComp, string loc, EntityUid user)
    {
        _popup.PopupPredicted(Loc.GetString(loc), gunUid, user);
        _audio.PlayPredicted(jamComp.SoundJammed, gunUid, null);
    }
}
