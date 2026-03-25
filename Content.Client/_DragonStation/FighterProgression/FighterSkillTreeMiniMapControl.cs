using System.Linq;
using System.Numerics;
using Content.Shared._DragonStation.FighterProgression;
using Content.Shared._DragonStation.FighterProgression.Prototypes;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;

namespace Content.Client._DragonStation.FighterProgression;

public sealed class FighterSkillTreeMiniMapControl : Control
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private readonly Dictionary<string, FighterSkillAvailability> _availability = new();

    private float _graphZoom = 1f;
    private ProtoId<FighterSkillPrototype>? _selectedSkill;
    private Vector2 _viewportOffset;
    private Vector2 _viewportSize;
    private Vector2 _contentSize;

    public FighterSkillTreeMiniMapControl()
    {
        IoCManager.InjectDependencies(this);
        MouseFilter = MouseFilterMode.Ignore;
        MinSize = SetSize = new Vector2(180f, 118f);
    }

    public void UpdateTree(
        Dictionary<string, FighterSkillAvailability> availability,
        ProtoId<FighterSkillPrototype>? selectedSkill,
        float graphZoom)
    {
        _availability.Clear();
        foreach (var (id, state) in availability)
        {
            _availability[id] = state;
        }

        _selectedSkill = selectedSkill;
        _graphZoom = graphZoom;
    }

    public void UpdateViewport(Vector2 viewportOffset, Vector2 viewportSize, Vector2 contentSize)
    {
        _viewportOffset = viewportOffset;
        _viewportSize = viewportSize;
        _contentSize = contentSize;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        var skills = _prototype.EnumeratePrototypes<FighterSkillPrototype>()
            .OrderBy(skill => skill.Position.Y)
            .ThenBy(skill => skill.Position.X)
            .ToArray();

        if (skills.Length == 0 || _contentSize.X <= 0f || _contentSize.Y <= 0f)
            return;

        var inner = UIBox2.FromDimensions(new Vector2(6f, 6f), Size - new Vector2(12f, 12f));
        var bounds = GetSkillBounds(skills);
        var boundsSize = bounds.Size;
        if (boundsSize.X <= 0f || boundsSize.Y <= 0f)
            return;

        var scale = MathF.Min(inner.Width / boundsSize.X, inner.Height / boundsSize.Y) * 1.15f;
        var scaledSize = boundsSize * scale;
        var origin = inner.Center - scaledSize / 2f - bounds.TopLeft * scale;

        foreach (var skill in skills)
        {
            foreach (var prereq in skill.Prerequisites)
            {
                if (!_prototype.TryIndex(prereq, out FighterSkillPrototype? source))
                    continue;

                var sourceRect = ScaleRect(FighterSkillTreeGraphControl.GetNodeRect(source, _graphZoom), origin, scale);
                var targetRect = ScaleRect(FighterSkillTreeGraphControl.GetNodeRect(skill, _graphZoom), origin, scale);
                var lineColor = GetAvailabilityColor(_availability.GetValueOrDefault(skill.ID)).WithAlpha(0.7f);

                var from = new Vector2(sourceRect.Right, sourceRect.Center.Y);
                var to = new Vector2(targetRect.Left, targetRect.Center.Y);
                handle.DrawLine(from, to, lineColor);
            }
        }

        foreach (var skill in skills)
        {
            var rect = ScaleRect(FighterSkillTreeGraphControl.GetNodeRect(skill, _graphZoom), origin, scale);
            var color = GetAvailabilityColor(_availability.GetValueOrDefault(skill.ID));
            handle.DrawRect(rect, color.WithAlpha(0.85f));

            if (_selectedSkill == skill.ID)
                handle.DrawRect(rect, Color.White, false);
        }

        var viewportRect = UIBox2.FromDimensions(
            origin + (_viewportOffset - bounds.TopLeft) * scale,
            new Vector2(
                MathF.Max(_viewportSize.X * scale, 6f),
                MathF.Max(_viewportSize.Y * scale, 6f)));
        var clippedViewport = ClipRect(viewportRect, inner);
        handle.DrawRect(clippedViewport, Color.White.WithAlpha(0.12f));
        handle.DrawRect(clippedViewport, Color.White, false);
    }

    private static UIBox2 ScaleRect(UIBox2 rect, Vector2 origin, float scale)
    {
        return UIBox2.FromDimensions(origin + rect.TopLeft * scale, rect.Size * scale);
    }

    private UIBox2 GetSkillBounds(IReadOnlyCollection<FighterSkillPrototype> skills)
    {
        var rects = skills
            .Select(skill => FighterSkillTreeGraphControl.GetNodeRect(skill, _graphZoom))
            .ToArray();

        var left = rects.Min(rect => rect.Left);
        var top = rects.Min(rect => rect.Top);
        var right = rects.Max(rect => rect.Right);
        var bottom = rects.Max(rect => rect.Bottom);
        return new UIBox2(left, top, right, bottom);
    }

    private static UIBox2 ClipRect(UIBox2 rect, UIBox2 bounds)
    {
        var topLeft = new Vector2(
            MathF.Max(rect.Left, bounds.Left),
            MathF.Max(rect.Top, bounds.Top));
        var bottomRight = new Vector2(
            MathF.Min(rect.Right, bounds.Right),
            MathF.Min(rect.Bottom, bounds.Bottom));

        if (bottomRight.X <= topLeft.X || bottomRight.Y <= topLeft.Y)
            return UIBox2.FromDimensions(bounds.TopLeft, Vector2.Zero);

        return new UIBox2(topLeft, bottomRight);
    }

    private static Color GetAvailabilityColor(FighterSkillAvailability availability)
    {
        return availability switch
        {
            FighterSkillAvailability.Unlocked => Color.LightGreen,
            FighterSkillAvailability.RequirementLocked => Color.FromHex("#8e6a2b"),
            FighterSkillAvailability.NextAutoUnlock => Color.White,
            FighterSkillAvailability.BranchChoiceAvailable => Color.Gold,
            FighterSkillAvailability.BlockedByBranchChoice => Color.FromHex("#6a4048"),
            _ => Color.DimGray,
        };
    }
}
