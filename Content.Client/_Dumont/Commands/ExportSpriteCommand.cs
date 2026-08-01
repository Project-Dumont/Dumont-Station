// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Sprite;
using JetBrains.Annotations;
using Robust.Shared.Console;

namespace Content.Client._Dumont.Commands;

[UsedImplicitly]
public sealed class ExportSpriteCommand : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public override string Command => "exportsprite";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length is not 1)
        {
            shell.WriteLine(Help);
            return;
        }

        if (!NetEntity.TryParse(args[0], out var targetUidNet)
            || !_entityManager.TryGetEntity(targetUidNet, out var targetUid))
        {
            shell.WriteLine("Entidade não encontrada");
            return;
        }

        var contentSprite = _entityManager.System<ContentSpriteSystem>();
        await contentSprite.Export(targetUid.Value);
    }
}