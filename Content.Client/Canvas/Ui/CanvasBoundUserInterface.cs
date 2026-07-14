// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Client.UserInterface;
using Content.Shared.Canvas;
using Content.Shared.Decals;
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
        private const string CanvasPaletteId = "CanvasPalette";

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
            var colors = _protoManager.Index<ColorPalettePrototype>(CanvasPaletteId).Colors.Values.ToList();

            EntMan.TryGetComponent<CanvasComponent>(Owner, out var canvasComponent);
            if (canvasComponent == null || _window == null)
                return;

            // Set properties from canvasComponent to the window
            _window.SetOwner(Owner);
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
