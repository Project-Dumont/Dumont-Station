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

    // _Starfall Start
    private int _thickness = 2;
    private Vector2[] _outlineOffsets = Array.Empty<Vector2>();

    // Only change thickness so we aren't updating it every frame
    public int Thickness
    {
        get => _thickness;
        set
        {
            if (_thickness == value)
                return;

            _thickness = value;
            _outlineOffsets = BuildOutlineOffsets();
        }
    }
    // _Starfall End

    public OutlineRichTextLabel(int thickness = 2)
    {
        IoCManager.InjectDependencies(this);
        var prototypes = IoCManager.Resolve<IPrototypeManager>();
        _outlineShader = prototypes.Index(OutlinePrototype).InstanceUnique();

        _thickness = thickness;
        _outlineOffsets = BuildOutlineOffsets();
    }

    // _Starfall Start
    private Vector2[] BuildOutlineOffsets()
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

        return list.ToArray();
    }
    // _Starfall End


    protected override void Draw(DrawingHandleScreen handle)
    {
        // _Starfall: we dont need to rebuild this every frame, we just do it when thickness actually changes
        // var offsets = BuildOutlineOffsets();

        handle.UseShader(_outlineShader);

        var originalTransform = handle.GetTransform();

        // _Starfall: Handle outline thickness outside of foreach
        var invScale = new Vector2(
            1f / originalTransform.M11,
            1f / originalTransform.M22);

        // _Starfall: Iterates the cached array
        foreach (var offset in _outlineOffsets)
        {
            var scaledOffset = offset * invScale;

            var offsetMatrix = Matrix3x2.CreateTranslation(scaledOffset);

            handle.SetTransform(offsetMatrix * originalTransform);
            base.Draw(handle);
        }

        handle.UseShader(null);
        handle.SetTransform(originalTransform);

        base.Draw(handle);
    }
}
