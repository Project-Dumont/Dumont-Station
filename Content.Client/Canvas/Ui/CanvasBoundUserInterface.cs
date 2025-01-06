using Content.Client.UserInterface;
using Content.Shared.Canvas;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using static Content.Shared.Canvas.SharedCanvasComponent;
using static Robust.Client.UserInterface.Controls.MenuBar;
using System.ComponentModel;
using Color = Robust.Shared.Maths.Color;

namespace Content.Client.Canvas.Ui
{
    public sealed class CanvasBoundUserInterface : BoundUserInterface
    {
        [Dependency] private readonly IPrototypeManager _protoManager = default!;

        [ViewVariables]
        private CanvasWindow? _window;

        public CanvasBoundUserInterface(EntityUid owner, object uiKey) : base(owner, (Enum) uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _window = this.CreateWindow<CanvasWindow>();
            _window.OnColorSelected += SelectColor;
            _window.OnSelected += Select;
            _window.OnFinalize += Finalize;
            _window.OnSignature += Signature;
            _window.OnResizeHeight += ResizeHeight;
            _window.OnResizeWidth += ResizeWidth;
            _window.OnClose += Close;
            PopulateCanvas(Owner);
            _window.OpenCentered();
        }

        private void PopulateCanvas(EntityUid uid)
        {
            var colors = new List<Color>
            {
                Color.Transparent,
                Color.White,  // Transparent / White
                Color.Gray,   // Darker White (lowercase)
                Color.Red,
                Color.DarkRed, // Darker Red (lowercase)
                Color.Green,
                Color.DarkGreen, // Darker Green (lowercase)
                Color.Blue,
                Color.DarkBlue, // Darker Blue (lowercase)
                Color.Yellow,
                Color.LightYellow, // Darker Yellow (lowercase)
                Color.Cyan,
                Color.DarkCyan, // Darker Cyan (lowercase)
                Color.Magenta,
                Color.DarkMagenta, // Darker Magenta (lowercase)
                new Color(1.0f, 0.65f, 0.0f), // Orange
                new Color(0.5f, 0.32f, 0.0f), // Darker Orange (lowercase)
                new Color(0.75f, 0.0f, 0.75f), // Purple
                new Color(0.37f, 0.0f, 0.37f), // Darker Purple (lowercase)
                new Color(0.33f, 0.55f, 0.2f), // Teal
                new Color(0.16f, 0.27f, 0.1f), // Darker Teal (lowercase)
                new Color(0.6f, 0.3f, 0.1f),   // Brown
                new Color(0.3f, 0.15f, 0.05f), // Darker Brown (lowercase)
                new Color(0.9f, 0.8f, 0.7f),   // Beige
                new Color(0.45f, 0.4f, 0.35f), // Darker Beige (lowercase)
                Color.LightGray,
                Color.DarkGray, // Darker Light Gray (lowercase)
                Color.Gray,     // Dark Gray
                new Color(0.2f, 0.2f, 0.2f),   // Darker Gray (lowercase)
                new Color(0.5f, 0.5f, 1.0f),   // Pastel Blue
                new Color(0.25f, 0.25f, 0.5f), // Darker Pastel Blue (lowercase)
                new Color(1.0f, 0.5f, 0.5f),   // Pastel Pink
                new Color(0.5f, 0.25f, 0.25f), // Darker Pastel Pink (lowercase)
                new Color(0.0f, 0.5f, 0.5f),   // Dark Cyan
                new Color(0.0f, 0.25f, 0.25f), // Darker Dark Cyan (lowercase)
                new Color(0.4f, 0.2f, 0.6f),   // Deep Purple
                new Color(0.2f, 0.1f, 0.3f),   // Darker Deep Purple (lowercase)
                Color.Black, // Darker Black (lowercase)
                Color.Black, // Black
                new Color(0.76f, 0.29f, 0.56f), // New Color 1 (Pinkish)
                new Color(0.38f, 0.15f, 0.28f), // Darker New Color 1 (lowercase)
                new Color(0.13f, 0.14f, 0.25f), // New Color 2 (Dark Blue)
                new Color(0.06f, 0.07f, 0.13f), // Darker New Color 2 (lowercase)
                new Color(0.73f, 0.47f, 0.25f), // New Color 3 (Brownish)
                new Color(0.36f, 0.24f, 0.13f), // Darker New Color 3 (lowercase)
                new Color(0.05f, 0.05f, 0.05f), // New Color 4 (Black)
                new Color(0.02f, 0.02f, 0.02f), // Darker New Color 4 (lowercase)
                new Color(0.48f, 0.26f, 0.22f), // New Color 5 (Muted Brown)
                new Color(0.24f, 0.13f, 0.11f), // Darker New Color 5 (lowercase)
                new Color(0.61f, 0.37f, 0.29f), // New Color 6 (Light Brown)
                new Color(0.3f, 0.18f, 0.15f),  // Darker New Color 6 (lowercase)
                new Color(0.13f, 0.18f, 0.3f),  // New Color 7 (Darker Blue)
                new Color(0.06f, 0.09f, 0.15f), // Darker New Color 7 (lowercase)
                new Color(0.53f, 0.26f, 0.18f), // New Color 8 (Orange-Brown)
                new Color(0.27f, 0.13f, 0.09f), // Darker New Color 8 (lowercase)
                new Color(1.0f, 0.35f, 0.38f),  // New Color 9 (Red)
                new Color(0.5f, 0.17f, 0.19f),  // Darker New Color 9 (lowercase)
                new Color(0.06f, 0.08f, 0.19f), // New Color 10 (Dark Purple)
                new Color(0.03f, 0.04f, 0.09f)  // Darker New Color 10 (lowercase)
            };


            EntMan.TryGetComponent<CanvasComponent>(Owner, out var canvasComponent);
            if (canvasComponent == null || _window == null)
                return;

            // Set properties from canvasComponent to the window
            _window.SetPaintingCode(canvasComponent?.PaintingCode ?? string.Empty);
            _window.SetHeight(canvasComponent?.Height ?? 16);
            _window.SetWidth(canvasComponent?.Width ?? 16);
            _window.SetSignature(canvasComponent?.Signature ?? string.Empty);


            if (!string.IsNullOrEmpty(canvasComponent?.Artist))
            {
                _window.SetArtist(canvasComponent.Artist);
            }
            _window?.PopulateColorSelector(colors);
            _window?.PopulatePaintingGrid();
        }


        public override void OnProtoReload(PrototypesReloadedEventArgs args)
        {
            base.OnProtoReload(args);
        }

        protected override void ReceiveMessage(BoundUserInterfaceMessage message)
        {
            base.ReceiveMessage(message);

            if (_window is null || message is not CanvasUsedMessage canvasMessage)
                return;

            _window.AdvanceState(canvasMessage.DrawnDecal);
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);


            var castState = (CanvasBoundUserInterfaceState) state;

            _window?.UpdateState(castState);
        }

        public void Select(string state)
        {
            SendMessage(new CanvasSelectMessage(state));
        }

        public void Finalize(string state)
        {
            SendMessage(new CanvasFinalizeMessage(state));
        }
        public void Signature(string state)
        {
            SendMessage(new CanvasSignatureMessage(state));
        }
        public void ResizeHeight(int height)
        {
            SendMessage(new CanvasHeightMessage(height));
        }
        public void ResizeWidth(int width)
        {
            SendMessage(new CanvasWidthMessage(width));
        }

        public void SelectColor(Color color)
        {
            SendMessage(new CanvasColorMessage(color));
        }
    }
}
