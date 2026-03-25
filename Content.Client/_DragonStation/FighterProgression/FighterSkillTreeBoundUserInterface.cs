using Content.Shared._DragonStation.FighterProgression;
using Content.Shared._DragonStation.FighterProgression.Prototypes;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._DragonStation.FighterProgression;

[UsedImplicitly]
public sealed class FighterSkillTreeBoundUserInterface : BoundUserInterface
{
    private FighterSkillTreeWindow? _window;

    public FighterSkillTreeBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<FighterSkillTreeWindow>();
        _window.OnClose += Close;
        _window.OnChoosePathPressed += OnChoosePathPressed;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not FighterSkillTreeBoundUserInterfaceState cast)
            return;

        _window?.UpdateState(cast);
    }

    private void OnChoosePathPressed(ProtoId<FighterSkillPrototype> skillId)
    {
        SendPredictedMessage(new FighterChooseBranchMessage(skillId));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        _window?.Close();
        _window = null;
    }
}
