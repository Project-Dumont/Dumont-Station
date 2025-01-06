using Content.Client.Canvas.Ui;
using Content.Shared.Canvas;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.GameObjects;
using Robust.Shared.Maths;
using Robust.Shared.ViewVariables;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = Robust.Shared.Maths.Color;

namespace Content.Client.Canvas
{
    [RegisterComponent]
    public sealed partial class CanvasComponent : SharedCanvasComponent
    {
        [ViewVariables(VVAccess.ReadWrite)]
        public bool UIUpdateNeeded;
    }
}
