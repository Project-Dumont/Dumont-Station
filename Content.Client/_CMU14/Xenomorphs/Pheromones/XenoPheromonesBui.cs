using Content.Shared._CMU14.Xenomorphs;
using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._CMU14.Xenomorphs.Pheromones;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

namespace Content.Client._CMU14.Xenomorphs.Pheromones;

[UsedImplicitly]
public sealed partial class XenoPheromonesBui : BoundUserInterface
{
    [Dependency] private IClyde _displayManager = default!;
    [Dependency] private IInputManager _inputManager = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IEyeManager _eye = default!;

    private readonly SpriteSystem _sprite;
    private readonly TransformSystem _transform;

    [ViewVariables]
    private XenoPheromonesMenu? _xenoPheromonesMenu;

    private const string HelpButtonTexture = "radial_help";

    public XenoPheromonesBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);

        _sprite = EntMan.System<SpriteSystem>();
        _transform = EntMan.System<TransformSystem>();
    }

    protected override void Open()
    {
        base.Open();

        _xenoPheromonesMenu = this.CreateWindow<XenoPheromonesMenu>();
        var parent = _xenoPheromonesMenu.FindControl<RadialContainer>("Main");

        if (EntMan.HasComponent<Content.Shared._White.Xenomorphs.Xenomorph.XenomorphComponent>(Owner))
        {
            var helpTexture = new TextureRect
            {
                VerticalAlignment = Control.VAlignment.Center,
                HorizontalAlignment = Control.HAlignment.Center,
                Texture = _sprite.Frame0(new SpriteSpecifier.Rsi(new ResPath("/Textures/_RMC14/Interface/radial.rsi"), HelpButtonTexture)),
                TextureScale = new Vector2(2f, 2f),
            };

            var helpButton = new RadialMenuTextureButton
            {
                StyleClasses = { "RadialMenuButton" },
                SetSize = new Vector2(64, 64),
            };

            helpButton.OnButtonDown += _ => SendMessage(new XenoPheromonesHelpButtonBuiMsg());

            helpButton.AddChild(helpTexture);
            parent.AddChild(helpButton);

            AddPheromonesButton(XenoPheromones.Frenzy, parent, Owner);
            AddPheromonesButton(XenoPheromones.Warding, parent, Owner);
            AddPheromonesButton(XenoPheromones.Recovery, parent, Owner);
        }

        var vpSize = _displayManager.ScreenSize;
        var pos = _inputManager.MouseScreenPosition.Position / vpSize;

        if (EntMan.TryGetComponent<EyeComponent>(Owner, out var eyeComp) &&
            eyeComp.Target != null)
            pos = _eye.WorldToScreen(_transform.GetMapCoordinates((EntityUid)eyeComp.Target).Position) / vpSize;

        else if (_player.LocalEntity is { } ent)
            pos = _eye.WorldToScreen(_transform.GetMapCoordinates(ent).Position) / vpSize;

        _xenoPheromonesMenu.OpenCenteredAt(pos);
    }

    private void AddPheromonesButton(XenoPheromones pheromone, RadialContainer parent, EntityUid owner)
    {
        var name = pheromone.ToString().ToLowerInvariant();
        var suffix = EntMan.TryGetComponent<XenomorphPheromonesComponent>(owner, out var pheromones) ? pheromones.PheroSuffix : null;

        if (suffix != null)
            suffix = "_" + suffix;

        var texture = new TextureRect
        {
            VerticalAlignment = Control.VAlignment.Center,
            HorizontalAlignment = Control.HAlignment.Center,
            Texture = _sprite.Frame0(new SpriteSpecifier.Rsi(new ResPath("/Textures/_RMC14/Interface/xeno_pheromones.rsi"), name + suffix)),
            TextureScale = new Vector2(2f, 2f),
        };

        var button = new RadialMenuTextureButton
        {
            StyleClasses = { "RadialMenuButton" },
            SetSize = new Vector2(64, 64),
            ToolTip = Loc.GetString("xenomorph-pheromones-" + name),
        };

        button.OnButtonDown += _ => SendMessage(new XenoPheromonesChosenBuiMsg(pheromone));

        button.AddChild(texture);
        parent.AddChild(button);
    }
}
