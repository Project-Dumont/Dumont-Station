using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Graphics;
using Robust.Shared.Prototypes;
namespace Content.Client._UM.UserInterface.Controls;

/// <summary>
/// Control for putting an outline around text. TODO: make the outline thickness configurable
/// </summary>
public sealed class OutlineRichTextLabel : RichTextLabel
{
    private static readonly ProtoId<ShaderPrototype> OutlinePrototype = "FontOutline";

    private ShaderInstance? _outlineShader;

    public int Thickness;

    public OutlineRichTextLabel(int thickness = 2)
    {
        IoCManager.InjectDependencies(this);
        var prototypes = IoCManager.Resolve<IPrototypeManager>();
        _outlineShader = prototypes.Index(OutlinePrototype).InstanceUnique();
        Thickness = thickness;
    }

    private List<Vector2> BuildOutlineOffsets()
    {
        var list = new List<Vector2>();

        for (int x = -Thickness; x <= Thickness; x++)
        {
            for (int y = -Thickness; y <= Thickness; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                if (Math.Abs(x) == Thickness || Math.Abs(y) == Thickness)
                    list.Add(new Vector2(x, y));
            }
        }

        return list;
    }


    protected override void Draw(DrawingHandleScreen handle)
    {
        var offsets = BuildOutlineOffsets();

        handle.UseShader(_outlineShader);

        var originalTransform = handle.GetTransform();

        foreach (var o in offsets)
        {
            float scaleX = originalTransform.M11;
            float scaleY = originalTransform.M22;

            Vector2 scaledOffset = new Vector2(
                o.X / scaleX,
                o.Y / scaleY
            );

            var offsetMatrix = Matrix3x2.CreateTranslation(scaledOffset);

            handle.SetTransform(offsetMatrix * originalTransform);
            base.Draw(handle);
        }

        handle.UseShader(null);
        handle.SetTransform(originalTransform);

        base.Draw(handle);
    }
}
