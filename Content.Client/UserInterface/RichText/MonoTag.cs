// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Southbridge <7013162+southbridge-fur@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.UserInterface.RichText;

/// <summary>
/// Sets the font to a monospaced variant
/// </summary>
public sealed class MonoTag : IMarkupTagHandler
{
    public static readonly ProtoId<FontPrototype> MonoFont = "Monospace";

    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly FontTagHijackHolder _fontHijack = default!;

    public string Name => "mono";

    /// <inheritdoc/>
    public void PushDrawContext(MarkupNode node, MarkupDrawingContext context)
    {
        var size = FontTag.GetSizeForFontTag(context.Font, node);

        if (_fontHijack.Hijack?.Invoke(MonoFont, size) is { } hijackedFont)
        {
            context.Font.Push(hijackedFont);
            return;
        }

#pragma warning disable CS0618
        if (!_prototypeManager.TryIndex<FontPrototype>(MonoFont, out var prototype))
            prototype = _prototypeManager.Index<FontPrototype>(FontTag.DefaultFont);

        var fontResource = _resourceCache.GetResource<FontResource>(prototype.Path);
#pragma warning restore CS0618
        var font = new VectorFont(fontResource, size);
        context.Font.Push(font);
    }

    /// <inheritdoc/>
    public void PopDrawContext(MarkupNode node, MarkupDrawingContext context)
    {
        context.Font.Pop();
    }
}
