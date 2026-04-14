// SPDX-FileCopyrightText: 2024 Rinary <72972221+Rinary1@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Dreykor <Dreykor12@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Client.Alerts;
using Content.Client.UserInterface.Systems.Alerts.Controls;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Content.Shared.Vampire;
using Content.Shared.Vampire.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Client.Vampire;

public sealed class VampireSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VampireIconComponent, GetStatusIconsEvent>(GetVampireIcon);
        SubscribeLocalEvent<VampireAlertComponent, UpdateAlertSpriteEvent>(OnUpdateAlert);
    }

    private void GetVampireIcon(EntityUid uid, VampireIconComponent component, ref GetStatusIconsEvent args)
    {
        var iconPrototype = _prototype.Index(component.StatusIcon);
        args.StatusIcons.Add(iconPrototype);
    }

    private void OnUpdateAlert(EntityUid uid, VampireAlertComponent component, ref UpdateAlertSpriteEvent args)
    {
        if (args.Alert.ID != component.BloodAlert)
            return;

        var blood = Math.Clamp(component.BloodAmount, 0, 999);
        _sprite.LayerSetRsiState(args.SpriteViewEnt.Owner, VampireVisualLayers.Digit1, $"{(blood / 100) % 10}");
        _sprite.LayerSetRsiState(args.SpriteViewEnt.Owner, VampireVisualLayers.Digit2, $"{(blood / 10) % 10}");
        _sprite.LayerSetRsiState(args.SpriteViewEnt.Owner, VampireVisualLayers.Digit3, $"{blood % 10}");
    }
}
