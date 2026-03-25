using System;
using System.Linq;
using System.Numerics;
using Content.Client.Stylesheets;
using Content.Shared._DragonStation.FighterProgression;
using Content.Shared._DragonStation.FighterProgression.Prototypes;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Timing;
using Robust.Shared.Prototypes;
using Robust.Shared.Maths;

namespace Content.Client._DragonStation.FighterProgression;

public sealed class FighterSkillTreeWindow : DefaultWindow
{
    private const bool MiniMapEnabled = false;

    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private readonly Label _thresholdsLabel;
    private readonly Label _powerLevelLabel;
    private readonly ProgressBar _xpBar;
    private readonly Label _xpLabel;
    private readonly Label _statusLabel;
    private readonly LayoutContainer _graphLayout;
    private readonly FighterSkillTreeGraphControl _graph;
    private readonly FighterSkillTreeMiniMapControl _miniMap;
    private readonly PanelContainer _miniMapFrame;
    private readonly ScrollContainer _graphScroll;
    private readonly Label _skillName;
    private readonly Label _branchName;
    private readonly RichTextLabel _description;
    private readonly RichTextLabel _effects;
    private readonly Button _choosePathButton;

    private FighterSkillTreeBoundUserInterfaceState? _state;
    private Dictionary<string, FighterSkillAvailability> _skillStates = new();
    private ProtoId<FighterSkillPrototype>? _selectedSkill;

    public event Action<ProtoId<FighterSkillPrototype>>? OnChoosePathPressed;

    public FighterSkillTreeWindow()
    {
        IoCManager.InjectDependencies(this);

        Title = Loc.GetString("fighter-tree-window-title");
        MinSize = SetSize = new Vector2(820f, 540f);

        var root = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 8,
        };
        Contents.AddChild(root);

        var header = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 10,
            HorizontalExpand = true,
        };
        root.AddChild(header);

        var thresholdsBox = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 2,
        };

        _thresholdsLabel = new Label { StyleClasses = { StyleBase.StyleClassLabelHeading } };
        _powerLevelLabel = new Label();
        _xpBar = new ProgressBar
        {
            HorizontalExpand = true,
            MinSize = new Vector2(180f, 18f),
        };
        _xpLabel = new Label();
        _statusLabel = new Label();

        thresholdsBox.AddChild(_thresholdsLabel);
        thresholdsBox.AddChild(_powerLevelLabel);
        header.AddChild(thresholdsBox);
        header.AddChild(_xpBar);
        header.AddChild(_xpLabel);
        header.AddChild(_statusLabel);

        var body = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 8,
            HorizontalExpand = true,
            VerticalExpand = true,
        };
        root.AddChild(body);

        var leftPanel = new PanelContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            PanelOverride = new StyleBoxFlat { BackgroundColor = Color.FromHex("#171717") },
        };

        _graphLayout = new LayoutContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true,
        };
        leftPanel.AddChild(_graphLayout);

        _graphScroll = new ScrollContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true,
        };
        LayoutContainer.SetAnchorPreset(_graphScroll, LayoutContainer.LayoutPreset.Wide);

        _graph = new FighterSkillTreeGraphControl
        {
            HorizontalExpand = true,
            VerticalExpand = true,
        };
        _graph.OnNodeSelected += OnNodeSelected;
        _graph.OnZoomChanged += (oldZoom, newZoom, mousePosition) =>
        {
            var scroll = _graphScroll.GetScrollValue();
            var scale = newZoom / Math.Max(oldZoom, 0.001f);
            var newMousePosition = mousePosition * scale;
            var viewportMousePosition = mousePosition - scroll;
            var nextScroll = newMousePosition - viewportMousePosition;
            _graphScroll.SetScrollValue(nextScroll);

            _miniMap?.UpdateTree(_skillStates, _selectedSkill, _graph.Zoom);
            UpdateMiniMapViewport();
        };
        _graph.OnPanRequested += delta =>
        {
            var next = _graphScroll.GetScrollValue() - delta;
            _graphScroll.SetScrollValue(next);
            UpdateMiniMapViewport();
        };
        _graphScroll.AddChild(_graph);
        _graphLayout.AddChild(_graphScroll);

        _miniMapFrame = new PanelContainer
        {
            MinSize = new Vector2(192f, 128f),
            MaxSize = new Vector2(192f, 128f),
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#0c0c0e").WithAlpha(0.95f),
                BorderColor = Color.FromHex("#424554"),
                BorderThickness = new Thickness(1f),
            },
            MouseFilter = MouseFilterMode.Ignore,
        };
        LayoutContainer.SetAnchorPreset(_miniMapFrame, LayoutContainer.LayoutPreset.TopLeft);

        _miniMap = new FighterSkillTreeMiniMapControl();
        _miniMapFrame.AddChild(_miniMap);
        _graphLayout.AddChild(_miniMapFrame);
        _miniMapFrame.Visible = MiniMapEnabled;

        body.AddChild(leftPanel);

        var rightPanel = new PanelContainer
        {
            MinSize = new Vector2(180f, 0f),
            MaxWidth = 180f,
            PanelOverride = new StyleBoxFlat { BackgroundColor = Color.FromHex("#101010") },
        };

        var detailBox = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 8,
            Margin = new Thickness(8),
        };
        rightPanel.AddChild(detailBox);

        _skillName = new Label { StyleClasses = { StyleBase.StyleClassLabelHeading } };
        _branchName = new Label();
        _description = new RichTextLabel();
        _effects = new RichTextLabel { VerticalExpand = true };
        _choosePathButton = new Button
        {
            Text = Loc.GetString("fighter-tree-choose-path-button"),
        };
        _choosePathButton.OnPressed += _ =>
        {
            if (_selectedSkill != null)
                OnChoosePathPressed?.Invoke(_selectedSkill.Value);
        };

        detailBox.AddChild(_skillName);
        detailBox.AddChild(_branchName);
        detailBox.AddChild(_description);
        detailBox.AddChild(_effects);
        detailBox.AddChild(_choosePathButton);

        body.AddChild(rightPanel);
    }

    public void UpdateState(FighterSkillTreeBoundUserInterfaceState state)
    {
        _state = state;
        _skillStates = state.Skills.ToDictionary(s => s.SkillId, s => s.Availability);
        _thresholdsLabel.Text = Loc.GetString("fighter-tree-thresholds-label", ("count", state.ThresholdsReached));
        _powerLevelLabel.Text = Loc.GetString("fighter-tree-power-level-label", ("powerLevel", state.CurrentPowerLevel));
        _xpBar.MaxValue = Math.Max(state.XpThreshold, 1);
        _xpBar.Value = Math.Min(state.CurrentXp, state.XpThreshold);
        _xpLabel.Text = Loc.GetString("fighter-tree-xp-label", ("xp", state.CurrentXp), ("threshold", state.XpThreshold));
        _statusLabel.Text = Loc.GetString(state.HasPendingBranchChoice
            ? "fighter-tree-status-branch-choice"
            : "fighter-tree-status-auto-advance");

        SelectRelevantSkill();
        _graph.UpdateTree(_skillStates, _selectedSkill);
        if (MiniMapEnabled)
        {
            _miniMap.UpdateTree(_skillStates, _selectedSkill, _graph.Zoom);
            UpdateMiniMapViewport();
        }
        UpdateDetails();
    }

    private void OnNodeSelected(ProtoId<FighterSkillPrototype> skillId)
    {
        _selectedSkill = skillId;
        _graph.UpdateTree(_skillStates, _selectedSkill);
        if (MiniMapEnabled)
            _miniMap.UpdateTree(_skillStates, _selectedSkill, _graph.Zoom);
        UpdateDetails();
    }

    private void SelectRelevantSkill()
    {
        if (_selectedSkill != null &&
            _prototype.HasIndex(_selectedSkill.Value) &&
            _skillStates.GetValueOrDefault(_selectedSkill.Value, FighterSkillAvailability.Hidden) != FighterSkillAvailability.Hidden)
            return;

        var selected = _skillStates
            .Where(pair => pair.Value != FighterSkillAvailability.Hidden)
            .Where(pair => pair.Value == FighterSkillAvailability.BranchChoiceAvailable)
            .Select(pair => _prototype.TryIndex(pair.Key, out FighterSkillPrototype? skill) ? skill : null)
            .OfType<FighterSkillPrototype>()
            .OrderBy(skill => skill.Position.X)
            .ThenBy(skill => skill.Position.Y)
            .FirstOrDefault()
            ?? _skillStates
                .Where(pair => pair.Value != FighterSkillAvailability.Hidden)
                .Where(pair => pair.Value == FighterSkillAvailability.NextAutoUnlock)
                .Select(pair => _prototype.TryIndex(pair.Key, out FighterSkillPrototype? skill) ? skill : null)
                .OfType<FighterSkillPrototype>()
                .OrderBy(skill => skill.Position.X)
                .ThenBy(skill => skill.Position.Y)
                .FirstOrDefault()
            ?? _skillStates
                .Where(pair => pair.Value != FighterSkillAvailability.Hidden)
                .Where(pair => pair.Value == FighterSkillAvailability.Unlocked)
                .Select(pair => _prototype.TryIndex(pair.Key, out FighterSkillPrototype? skill) ? skill : null)
                .OfType<FighterSkillPrototype>()
                .OrderBy(skill => skill.Position.X)
                .ThenBy(skill => skill.Position.Y)
                .FirstOrDefault()
            ?? _prototype.EnumeratePrototypes<FighterSkillPrototype>()
                .Where(skill => _skillStates.GetValueOrDefault(skill.ID) != FighterSkillAvailability.Hidden)
                .OrderBy(skill => skill.Position.X)
                .ThenBy(skill => skill.Position.Y)
                .FirstOrDefault();

        _selectedSkill = selected?.ID;
    }

    private void UpdateDetails()
    {
        if (_selectedSkill == null || !_prototype.TryIndex(_selectedSkill.Value, out FighterSkillPrototype? skill))
            return;

        var branch = _prototype.Index(skill.Branch);
        var availability = _skillStates.GetValueOrDefault(skill.ID);

        _skillName.Text = Loc.GetString(skill.Name);
        _branchName.Text = Loc.GetString("fighter-tree-branch-label", ("branch", Loc.GetString(branch.Name)));
        _branchName.ModulateSelfOverride = branch.Color;
        _description.SetMessage(Loc.GetString(skill.Description));

        var effectLines = string.Join('\n', skill.EffectDescriptions.Select(e => $"- {Loc.GetString(e)}"));
        if (string.IsNullOrWhiteSpace(effectLines))
            effectLines = Loc.GetString("fighter-tree-no-effects");

        var prereqLines = skill.Prerequisites.Count == 0
            ? Loc.GetString("fighter-tree-no-prereqs")
            : string.Join('\n', skill.Prerequisites.Select(p => $"- {Loc.GetString(_prototype.Index(p).Name)}"));

        _effects.SetMessage(Loc.GetString("fighter-tree-details-template",
            ("availability", Loc.GetString(GetAvailabilityLoc(availability))),
            ("prereqs", prereqLines),
            ("effects", effectLines)));

        _choosePathButton.Visible = availability == FighterSkillAvailability.BranchChoiceAvailable;
        _choosePathButton.Disabled = availability != FighterSkillAvailability.BranchChoiceAvailable;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);
        if (!MiniMapEnabled)
            return;

        UpdateMiniMapPlacement();
        UpdateMiniMapViewport();
    }

    private void UpdateMiniMapPlacement()
    {
        var position = new Vector2(12f, Math.Max(12f, _graphLayout.Size.Y - _miniMapFrame.Size.Y - 12f));
        LayoutContainer.SetPosition(_miniMapFrame, position);
    }

    private void UpdateMiniMapViewport()
    {
        _miniMap.UpdateViewport(_graphScroll.GetScrollValue(), _graphScroll.Size, _graph.MinSize);
    }

    private static string GetAvailabilityLoc(FighterSkillAvailability availability)
    {
        return availability switch
        {
            FighterSkillAvailability.Unlocked => "fighter-tree-availability-unlocked",
            FighterSkillAvailability.Hidden => "fighter-tree-availability-locked",
            FighterSkillAvailability.RequirementLocked => "fighter-tree-availability-requirement-locked",
            FighterSkillAvailability.NextAutoUnlock => "fighter-tree-availability-next-auto",
            FighterSkillAvailability.BranchChoiceAvailable => "fighter-tree-availability-branch-choice",
            FighterSkillAvailability.BlockedByBranchChoice => "fighter-tree-availability-branch-closed",
            _ => "fighter-tree-availability-locked",
        };
    }
}
