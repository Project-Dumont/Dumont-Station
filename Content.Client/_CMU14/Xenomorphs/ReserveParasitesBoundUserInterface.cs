using Content.Shared._CMU14.Xenomorphs;
using Content.Client._Dumont.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._CMU14.Xenomorphs;

[UsedImplicitly]
public sealed class ReserveParasitesBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private ReserveParasitesWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<ReserveParasitesWindow>();
        if (EntMan.TryGetComponent<XenomorphCarrierComponent>(Owner, out var paras))
            _window.SetReserveShown(paras.ReservedParasites);
        else if (EntMan.TryGetComponent<XenomorphEggMorpherComponent>(Owner, out var morpher))
            _window.SetReserveShown(morpher.ReservedParasites);

        _window.ApplyButton.OnPressed += _ =>
        {
            SendMessage(new XenomorphCarrierReserveMessage(_window.ReserveBar.Value));
            _window.Close();
        };
    }

    public void ChangeReserve(int newReserve)
    {
        SendMessage(new XenomorphCarrierReserveMessage(newReserve));
    }
}
