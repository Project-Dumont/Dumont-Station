using System.Linq;
using Content.Shared._Maid.GameTicking.Prototypes;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client._Maid.UserInterface.AnimatedBackground;

public sealed class AnimatedBackgroundControl : Control
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private static readonly ResPath RSIFallback = new("/Textures/_Maid/LobbyScreens/AnimatedScreens/sea.rsi");
    private static readonly string DefaultState = "animated";

    private ResPath? _rsiPath;
    private AnimatedTextureRect _animatedTextureRect = new AnimatedTextureRect();

    public ResPath RsiPath => _rsiPath ?? RSIFallback;

    private readonly TextureRect _textureRect = new();

    public AnimatedBackgroundControl()
    {
        IoCManager.InjectDependencies(this);
        LayoutContainer.SetAnchorPreset(_textureRect, LayoutContainer.LayoutPreset.Wide);
        _textureRect.Stretch = TextureRect.StretchMode.KeepAspectCovered;
        _textureRect.Visible = false;
        AddChild(_textureRect);

        LayoutContainer.SetAnchorPreset(_animatedTextureRect, LayoutContainer.LayoutPreset.Wide);
        _animatedTextureRect.DisplayRect.Stretch = TextureRect.StretchMode.KeepAspectCovered;
        _animatedTextureRect.Visible = false;
        AddChild(_animatedTextureRect);

        InitializeStates();
    }

    private void InitializeStates()
    {
        var specifier = new SpriteSpecifier.Rsi(RsiPath, DefaultState);
        _animatedTextureRect.SetFromSpriteSpecifier(specifier);
    }

    public void SetRSI(RSI? rsi)
    {
        if(rsi is null)
        {
            _rsiPath = null;
            _textureRect.Visible = false;
            _animatedTextureRect.Visible = false;
            return;
        }

        _rsiPath = rsi.Path;
        _textureRect.Visible = false;
        _animatedTextureRect.Visible = true;
        InitializeStates();
    }

    public void SetTexture(Texture? texture)
    {
        _rsiPath = null;
        _animatedTextureRect.Visible = false;
        _textureRect.Visible = true;
        _textureRect.Texture = texture;
    }

    protected override void Resized()
    {
        base.Resized();
        _textureRect.SetSize = Size;
        _animatedTextureRect.SetSize = Size;
    }

    public void RandomizeBackground()
    {
        var backgroundsProto = _prototypeManager.EnumeratePrototypes<AnimatedLobbyScreenPrototype>().ToList();
        if (backgroundsProto.Count == 0)
            return;

        var random = new Random();
        var index = random.Next(backgroundsProto.Count);
        _rsiPath = backgroundsProto[index].Path;
        InitializeStates();
    }
}
