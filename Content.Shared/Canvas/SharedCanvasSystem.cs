using Content.Shared.Interaction.Events;
using Content.Shared.Interaction;
using Robust.Shared.Prototypes;
using Content.Shared.Popups;
using Robust.Shared.GameObjects;
using static Content.Shared.Canvas.SharedCanvasComponent;
using Robust.Shared.GameStates;

namespace Content.Shared.Canvas
{
    [Virtual]
    public abstract partial class SharedCanvasSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        public override void Initialize()
        {
            base.Initialize();
        }
    }
}
