using System;
using System.Linq;
using System.Numerics;
using Content.Shared.Input;
using Content.Shared._DragonStation.FighterProgression;
using Content.Shared._DragonStation.FighterProgression.Prototypes;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;
using Robust.Shared.Input;
using Robust.Shared.Prototypes;

namespace Content.Client._DragonStation.FighterProgression;

public sealed class FighterSkillTreeGraphControl : LayoutContainer
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IResourceCache _resources = default!;

    public static readonly Vector2 BaseNodeSize = new(170f, 56f);
    public static readonly Vector2 BaseGridSpacing = new(190f, 104f);
    public static readonly Vector2 BasePadding = new(32f, 28f);
    private const float ZoomStep = 0.1f;
    private const float MinZoom = 0.65f;
    private const float MaxZoom = 1.8f;

    private readonly Dictionary<string, FighterSkillAvailability> _availability = new();
    private readonly Dictionary<string, UIBox2> _nodeRects = new();
    private readonly Dictionary<string, Button> _nodeButtons = new();
    private bool _dragging;
    public event Action<ProtoId<FighterSkillPrototype>>? OnNodeSelected;
    public event Action<Vector2>? OnPanRequested;
    public event Action<float, float, Vector2>? OnZoomChanged;

    public ProtoId<FighterSkillPrototype>? SelectedSkill { get; private set; }
    public float Zoom { get; private set; } = 1f;

    private Vector2 NodeSize => BaseNodeSize * Zoom;
    private Vector2 GridSpacing => BaseGridSpacing * Zoom;
    private Vector2 Padding => BasePadding * Zoom;

    public FighterSkillTreeGraphControl()
    {
        IoCManager.InjectDependencies(this);
        MouseFilter = MouseFilterMode.Stop;
    }

    public void UpdateTree(
        Dictionary<string, FighterSkillAvailability> state,
        ProtoId<FighterSkillPrototype>? selectedSkill = null)
    {
        _availability.Clear();
        foreach (var (id, availability) in state)
        {
            _availability[id] = availability;
        }

        SelectedSkill = selectedSkill;
        RebuildLayout();
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        foreach (var source in EnumerateVisibleSkills())
        {
            if (!TryGetLiveNodeRect(source.ID, out var sourceRect))
                continue;

            var children = EnumerateVisibleSkills()
                .Where(skill => skill.Prerequisites.Contains(source.ID) && _nodeButtons.ContainsKey(skill.ID))
                .Where(skill => !(source.SpecialChallengeUnlock && skill.SpecialChallengeUnlock))
                .OrderBy(skill => skill.Position.Y)
                .ThenBy(skill => skill.Position.X)
                .ToArray();

            if (children.Length == 0)
                continue;

            DrawConnections(handle, sourceRect, children);
        }
    }

    protected override void MouseWheel(GUIMouseWheelEventArgs args)
    {
        base.MouseWheel(args);

        var oldZoom = Zoom;
        var nextZoom = Math.Clamp(Zoom + args.Delta.Y * ZoomStep, MinZoom, MaxZoom);
        if (Math.Abs(nextZoom - oldZoom) < 0.001f)
        {
            args.Handle();
            return;
        }

        Zoom = nextZoom;
        RebuildLayout();
        OnZoomChanged?.Invoke(oldZoom, Zoom, args.RelativePosition);
        args.Handle();
    }

    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);

        if (args.Function != EngineKeyFunctions.UIClick)
            return;

        _dragging = true;
    }

    protected override void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        base.KeyBindUp(args);

        if (args.Function != EngineKeyFunctions.UIClick)
            return;

        _dragging = false;
    }

    protected override void MouseMove(GUIMouseMoveEventArgs args)
    {
        base.MouseMove(args);

        if (!_dragging)
            return;

        if (args.Relative == Vector2.Zero)
            return;

        OnPanRequested?.Invoke(new Vector2(args.Relative.X, args.Relative.Y));
    }

    private void RebuildLayout()
    {
        foreach (var child in Children.ToArray())
        {
            RemoveChild(child);
        }

        _nodeRects.Clear();
        _nodeButtons.Clear();

        var fontSize = Math.Clamp((int)MathF.Round(11f * Zoom), 8, 18);
        var nodeFont = new VectorFont(_resources.GetResource<FontResource>("/Fonts/NotoSans/NotoSans-Bold.ttf"), fontSize);

        var maxPos = Vector2.Zero;
        foreach (var skill in EnumerateVisibleSkills())
        {
            var availability = _availability.GetValueOrDefault(skill.ID);
            var branch = _prototype.Index(skill.Branch);

            var position = new Vector2(skill.Position.X * GridSpacing.X, skill.Position.Y * GridSpacing.Y) + Padding;
            var rect = UIBox2.FromDimensions(position, NodeSize);
            _nodeRects[skill.ID] = rect;
            maxPos = Vector2.Max(maxPos, rect.BottomRight);

            var button = new Button
            {
                Text = Loc.GetString(skill.Name),
                ToolTip = Loc.GetString(skill.Description),
                MinSize = NodeSize,
                SetSize = NodeSize,
                TextAlign = Label.AlignMode.Center,
                VerticalAlignment = VAlignment.Center,
                ClipText = true,
            };
            button.Label.FontColorOverride = Color.White;
            button.Label.FontOverride = nodeFont;

            button.OnPressed += _ =>
            {
                SelectedSkill = skill.ID;
                RefreshButtonStyles();
                OnNodeSelected?.Invoke(skill.ID);
            };

            AddChild(button);
            LayoutContainer.SetPosition(button, position);
            _nodeButtons[skill.ID] = button;
            ApplyButtonStyle(button, branch.Color, availability, SelectedSkill == skill.ID);
        }

        MinSize = maxPos + Padding;
    }

    public static UIBox2 GetNodeRect(FighterSkillPrototype skill, float zoom = 1f)
    {
        var nodeSize = BaseNodeSize * zoom;
        var gridSpacing = BaseGridSpacing * zoom;
        var padding = BasePadding * zoom;
        var position = new Vector2(skill.Position.X * gridSpacing.X, skill.Position.Y * gridSpacing.Y) + padding;
        return UIBox2.FromDimensions(position, nodeSize);
    }

    private IEnumerable<FighterSkillPrototype> EnumerateSkills()
    {
        return _prototype.EnumeratePrototypes<FighterSkillPrototype>()
            .OrderBy(skill => skill.Position.Y)
            .ThenBy(skill => skill.Position.X);
    }

    private IEnumerable<FighterSkillPrototype> EnumerateVisibleSkills()
    {
        return EnumerateSkills()
            .Where(skill => _availability.GetValueOrDefault(skill.ID) != FighterSkillAvailability.Hidden);
    }

    private void RefreshButtonStyles()
    {
        foreach (var skill in EnumerateVisibleSkills())
        {
            if (!_nodeButtons.TryGetValue(skill.ID, out var button))
                continue;

            var availability = _availability.GetValueOrDefault(skill.ID);
            var branch = _prototype.Index(skill.Branch);
            ApplyButtonStyle(button, branch.Color, availability, SelectedSkill == skill.ID);
        }
    }

    private void DrawConnections(
        DrawingHandleScreen handle,
        UIBox2 sourceRect,
        IReadOnlyCollection<FighterSkillPrototype> children)
    {
        var sourceAnchor = new Vector2(sourceRect.Right, sourceRect.Center.Y);
        var childRects = children
            .Select(skill => (skill, rect: GetLiveNodeRect(skill.ID)))
            .Where(pair => pair.rect != null)
            .Select(pair => (pair.skill, rect: pair.rect!.Value))
            .ToArray();

        if (childRects.Length == 0)
            return;

        var minChildLeft = childRects.Min(pair => pair.rect.Left);
        var branchX = MathF.Min(sourceAnchor.X + 28f, minChildLeft - 18f);

        if (branchX > sourceAnchor.X)
            handle.DrawLine(sourceAnchor, new Vector2(branchX, sourceAnchor.Y), GetSharedLineColor(children).WithAlpha(0.85f));

        if (childRects.Length > 1)
        {
            var minY = childRects.Min(pair => pair.rect.Center.Y);
            var maxY = childRects.Max(pair => pair.rect.Center.Y);
            handle.DrawLine(new Vector2(branchX, minY), new Vector2(branchX, maxY), GetSharedLineColor(children).WithAlpha(0.7f));
        }

        foreach (var (child, targetRect) in childRects)
        {
            var targetAnchor = new Vector2(targetRect.Left, targetRect.Center.Y);
            var color = GetLineColor(_availability.GetValueOrDefault(child.ID)).WithAlpha(0.9f);

            if (Math.Abs(targetAnchor.Y - sourceAnchor.Y) > 0.5f)
                handle.DrawLine(new Vector2(branchX, sourceAnchor.Y), new Vector2(branchX, targetAnchor.Y), color.WithAlpha(0.75f));

            handle.DrawLine(new Vector2(branchX, targetAnchor.Y), targetAnchor, color);
        }
    }

    private Color GetSharedLineColor(IReadOnlyCollection<FighterSkillPrototype> children)
    {
        if (children.Any(skill => _availability.GetValueOrDefault(skill.ID) == FighterSkillAvailability.BranchChoiceAvailable))
            return GetLineColor(FighterSkillAvailability.BranchChoiceAvailable);

        if (children.Any(skill => _availability.GetValueOrDefault(skill.ID) == FighterSkillAvailability.NextAutoUnlock))
            return GetLineColor(FighterSkillAvailability.NextAutoUnlock);

        if (children.All(skill => _availability.GetValueOrDefault(skill.ID) == FighterSkillAvailability.Unlocked))
            return GetLineColor(FighterSkillAvailability.Unlocked);

        return Color.DimGray;
    }

    private bool TryGetLiveNodeRect(string skillId, out UIBox2 rect)
    {
        rect = default;

        if (!_nodeButtons.TryGetValue(skillId, out var button))
            return false;

        rect = new UIBox2(
            button.GlobalPixelPosition - GlobalPixelPosition,
            button.GlobalPixelPosition - GlobalPixelPosition + button.PixelSize);
        return true;
    }

    private UIBox2? GetLiveNodeRect(string skillId)
    {
        return TryGetLiveNodeRect(skillId, out var rect) ? rect : null;
    }

    private static void ApplyButtonStyle(Button button, Color branchColor, FighterSkillAvailability availability, bool selected)
    {
        var background = GetNodeColor(branchColor, availability);
        var border = selected ? Color.White : background.WithAlpha(0.9f);

        button.StyleBoxOverride = new StyleBoxFlat
        {
            BackgroundColor = background,
            BorderColor = border,
            BorderThickness = new Thickness(2f),
        };
    }

    private static Color GetNodeColor(Color branchColor, FighterSkillAvailability availability)
    {
        return availability switch
        {
            FighterSkillAvailability.Hidden => Color.Transparent,
            FighterSkillAvailability.Unlocked => Color.LightGreen,
            FighterSkillAvailability.RequirementLocked => Color.FromHex("#6b5330"),
            FighterSkillAvailability.NextAutoUnlock => branchColor,
            FighterSkillAvailability.BranchChoiceAvailable => Color.Gold.WithAlpha(0.9f),
            FighterSkillAvailability.BlockedByBranchChoice => Color.FromHex("#4b2f37"),
            _ => Color.DimGray,
        };
    }

    private static Color GetLineColor(FighterSkillAvailability availability)
    {
        return availability switch
        {
            FighterSkillAvailability.Hidden => Color.Transparent,
            FighterSkillAvailability.Unlocked => Color.LightGreen,
            FighterSkillAvailability.RequirementLocked => Color.FromHex("#c58f28"),
            FighterSkillAvailability.NextAutoUnlock => Color.White,
            FighterSkillAvailability.BranchChoiceAvailable => Color.Gold,
            FighterSkillAvailability.BlockedByBranchChoice => Color.FromHex("#6a4048"),
            _ => Color.DimGray,
        };
    }
}
