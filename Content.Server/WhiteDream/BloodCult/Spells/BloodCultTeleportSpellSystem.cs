// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.DoAfter;
using Content.Server.WhiteDream.BloodCult.Runes;
using Content.Server.WhiteDream.BloodCult.Runes.Teleport;
using Content.Shared.DoAfter;
using Content.Shared.WhiteDream.BloodCult.Spells;
using Content.Shared._White.ListViewSelector;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.Spells;

public sealed partial class BloodCultTeleportSpellSystem : EntitySystem
{
    // WhiteDream - teleport visuals
    private static readonly EntProtoId TeleportInEffect = "CultTeleportInEffect";
    private static readonly EntProtoId TeleportOutEffect = "CultTeleportOutEffect";

    [Dependency] private AudioSystem _audio = default!;
    [Dependency] private CultRuneBaseSystem _cultRune = default!;
    [Dependency] private CultRuneTeleportSystem _runeTeleport = default!;
    [Dependency] private DoAfterSystem _doAfter = default!;
    [Dependency] private TransformSystem _transform = default!;
    [Dependency] private UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BloodCultTeleportEvent>(OnTeleport);
        SubscribeLocalEvent<BloodCultSpellsHolderComponent, ListViewItemSelectedMessage>(OnTeleportRuneSelected);
        SubscribeLocalEvent<BloodCultSpellsHolderComponent, TeleportActionDoAfterEvent>(OnTeleportDoAfter);
    }

    private void OnTeleport(BloodCultTeleportEvent ev)
    {
        if (ev.Handled || !_runeTeleport.TryGetTeleportRunes(ev.Performer, out var runes))
            return;

        var metaData = new Dictionary<string, object>
        {
            ["target"] = GetNetEntity(ev.Target),
            ["duration"] = ev.DoAfterDuration
        };

        _ui.SetUiState(ev.Performer, ListViewSelectorUiKey.Key, new ListViewSelectorState(runes, metaData));
        _ui.TryToggleUi(ev.Performer, ListViewSelectorUiKey.Key, ev.Performer);
        ev.Handled = true;
    }

    private void OnTeleportRuneSelected(
        Entity<BloodCultSpellsHolderComponent> ent,
        ref ListViewItemSelectedMessage args
    )
    {
        if (!args.MetaData.TryGetValue("target", out var rawTarget) || rawTarget is not NetEntity netTarget ||
            !args.MetaData.TryGetValue("duration", out var rawDuration) || rawDuration is not TimeSpan duration)
            return;

        var target = GetEntity(netTarget);
        var teleportDoAfter = new TeleportActionDoAfterEvent
        {
            Rune = GetNetEntity(EntityUid.Parse(args.SelectedItem.Id))
        };
        var doAfterArgs = new DoAfterArgs(EntityManager, ent.Owner, duration, teleportDoAfter, target, target);

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnTeleportDoAfter(Entity<BloodCultSpellsHolderComponent> user, ref TeleportActionDoAfterEvent ev)
    {
        if (ev.Target is not { } target)
            return;

        var rune = GetEntity(ev.Rune);
        _audio.PlayPvs(ev.TeleportOutSound, target);

        _cultRune.StopPulling(target);
        // WhiteDream - visual effect on both ends
        Spawn(TeleportOutEffect, Transform(target).Coordinates);
        _transform.SetCoordinates(target, Transform(rune).Coordinates);
        Spawn(TeleportInEffect, Transform(target).Coordinates);

        _audio.PlayPvs(ev.TeleportInSound, rune);
    }
}
