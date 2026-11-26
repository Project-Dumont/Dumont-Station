using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

namespace Content.Client._Maid.UserInterface.AnimatedBackground;

public sealed class AnimatedBackgroundControl : Control
{
    private const string DefaultState = "animated";

    private readonly TextureRect _textureRect = new();
    private readonly AnimatedTextureRect _animatedTextureRect = new();

    public AnimatedBackgroundControl()
    {
        LayoutContainer.SetAnchorPreset(_textureRect, LayoutContainer.LayoutPreset.Wide);
        _textureRect.Stretch = TextureRect.StretchMode.KeepAspectCovered;
        _textureRect.Visible = false;
        AddChild(_textureRect);

        LayoutContainer.SetAnchorPreset(_animatedTextureRect, LayoutContainer.LayoutPreset.Wide);
        LayoutContainer.SetAnchorPreset(_animatedTextureRect.DisplayRect, LayoutContainer.LayoutPreset.Wide);
        _animatedTextureRect.DisplayRect.Stretch = TextureRect.StretchMode.KeepAspectCovered;
        _animatedTextureRect.Visible = false;
        AddChild(_animatedTextureRect);
    }

    public void SetRSI(RSI? rsi)
    {
        if (rsi is null)
        {
            _textureRect.Visible = false;
            _animatedTextureRect.Visible = false;
            return;
        }

        _textureRect.Visible = false;
        _animatedTextureRect.Visible = true;
        _animatedTextureRect.SetFromSpriteSpecifier(new SpriteSpecifier.Rsi(rsi.Path, DefaultState));
    }

    public void SetTexture(Texture? texture)
    {
        _animatedTextureRect.Visible = false;
        _textureRect.Visible = true;
        _textureRect.Texture = texture;
    }
}
