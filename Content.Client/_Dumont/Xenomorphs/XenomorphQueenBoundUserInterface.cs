using Content.Shared._CMU14.Xenomorphs;
using Content.Client._CMU14.Xenomorphs;
using Robust.Client.UserInterface.CustomControls;
using Content.Client.UserInterface.Controls;
using Content.Shared._Dumont.Xenomorphs;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Prototypes;
using Content.Shared._White.Xenomorphs.Caste;

namespace Content.Client._Dumont.Xenomorphs;

public sealed class XenomorphQueenBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private DefaultWindow? _window;
    private BoxContainer? _members;
    private LineEdit? _text;
    private Label? _mode;
    private BoxContainer? _pheromoneBox;
    private ScrollContainer? _scroll;
    private Button? _send;
    private readonly Dictionary<string, Button> _pheromones = new();
    protected override void Open()
    {
        base.Open();
        _window = new DefaultWindow { Title = Loc.GetString("xenomorph-queen-hive-title"), MinSize = new System.Numerics.Vector2(540, 390) };
        var root = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Vertical, SeparationOverride = 8 };
        _window.Contents.AddChild(root);
        var back = new Button { Text = Loc.GetString("xenomorph-queen-return") };
        back.OnPressed += _ => SendMessage(new XenomorphQueenUiMessage("return"));
        root.AddChild(back);
        var pheromones = _pheromoneBox = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Horizontal, SeparationOverride = 4 };
        foreach (var mode in new[] { "frenzy", "warding", "recovery", "stopphero" })
        {
            var choice = mode;
            var button = new Button { Text = Loc.GetString("xenomorph-queen-phero-" + mode), ToggleMode = true };
            if (mode != "stopphero")
            {
                var content = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Horizontal, SeparationOverride = 4, MouseFilter = Control.MouseFilterMode.Ignore };
                content.AddChild(new TextureRect
                {
                    MouseFilter = Control.MouseFilterMode.Ignore,
                    Texture = EntMan.System<Robust.Client.GameObjects.SpriteSystem>().Frame0(new Robust.Shared.Utility.SpriteSpecifier.Rsi(
                        new Robust.Shared.Utility.ResPath("/Textures/_RMC14/Interface/xeno_pheromones.rsi"), mode + "_queen")),
                });
                content.AddChild(new Label { Text = button.Text, MouseFilter = Control.MouseFilterMode.Ignore });
                button.Text = null;
                button.AddChild(content);
            }
            _pheromones[mode] = button;
            button.OnPressed += _ => SendMessage(new XenomorphQueenUiMessage(choice));
            pheromones.AddChild(button);
        }
        root.AddChild(pheromones);
        _mode = new Label(); root.AddChild(_mode);
        _text = new LineEdit { PlaceHolder = Loc.GetString("xenomorph-queen-message-placeholder") };
        root.AddChild(_text);
        var send = _send = new Button { Text = Loc.GetString("xenomorph-queen-send") };
        send.OnPressed += _ => SendMessage(new XenomorphQueenUiMessage("send", text: _text.Text));
        root.AddChild(send);
        var scroll = _scroll = new ScrollContainer { VerticalExpand = true };
        _members = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Vertical, SeparationOverride = 4 };
        scroll.AddChild(_members);root.AddChild(scroll);
        _window.OnClose += Close;
        _window.OpenCentered();
    }
    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not XenomorphQueenUiState hive || _members == null || _mode == null || _text == null) return;
        _mode.Text = Loc.GetString("xenomorph-queen-mode-" + hive.Mode);
        _text.Visible = hive.Mode is not ("hive" or "pheromones");
        if (_send != null) _send.Visible = _text.Visible;
        if (_scroll != null) _scroll.Visible = hive.Mode == "hive";
        if (_pheromoneBox != null) _pheromoneBox.Visible = hive.Mode == "pheromones";
        foreach (var (name, button) in _pheromones) button.Pressed = name == hive.Pheromones;
        _members.RemoveAllChildren();
        var prototypes = IoCManager.Resolve<IPrototypeManager>();
        foreach (var member in hive.Members)
        {
            var row = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Horizontal, SeparationOverride = 6 };
            var casteName = prototypes.TryIndex<XenomorphCastePrototype>(member.Caste, out var caste)
                ? Loc.GetString(caste.Name)
                : member.Caste;
            row.AddChild(new Label { Text = member.Name + " (" + casteName + (member.Tier is >= 1 and <= 3 ? " — " + Loc.GetString($"xenomorph-rank-{member.Tier}") : "") + ")", HorizontalExpand = true, ClipText = true });
            var watch = new Button { Text = Loc.GetString("xenomorph-queen-watch") };
            watch.OnPressed += _ => SendMessage(new XenomorphQueenUiMessage("watch", member.Entity));
            row.AddChild(watch);
            var leader = new Button { Text = Loc.GetString(member.Leader ? "xenomorph-queen-remove-leader" : "xenomorph-queen-leader") };
            leader.OnPressed += _ => SendMessage(new XenomorphQueenUiMessage("leader", member.Entity));
            row.AddChild(leader);
            _members.AddChild(row);
        }
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _window?.Dispose();
    }
}
