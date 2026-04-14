// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Wizard.Traps;
using Robust.Client.GameObjects;

namespace Content.Client._Shitcode.Wizard.Systems;

public sealed class WizardTrapsSystem : SharedWizardTrapsSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WizardTrapComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(Entity<WizardTrapComponent> ent, ref AppearanceChangeEvent args)
    {
        if (!args.AppearanceData.TryGetValue(TrapVisuals.Alpha, out var alpha))
            return;

        if (args.Sprite is not { } sprite)
            return;

        _sprite.SetColor((ent, sprite), sprite.Color.WithAlpha((float) alpha));
    }
}
