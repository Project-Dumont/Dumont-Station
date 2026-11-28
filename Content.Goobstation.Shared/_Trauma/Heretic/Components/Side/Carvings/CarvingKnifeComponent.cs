// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.Side.Carvings;

[RegisterComponent, NetworkedComponent]
public sealed partial class CarvingKnifeComponent : Component
{
    [DataField]
    public List<EntProtoId> Carvings = new();

    [DataField(serverOnly: true)]
    public List<EntityUid> DrawnRunes = new();

    [DataField]
    public int MaxRuneAmount = 3;

    [DataField]
    public TimeSpan RuneDrawTime = TimeSpan.FromSeconds(1f);

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Items/sheath.ogg");

    [DataField]
    public EntProtoId RunebreakAction = "ActionRunebreak";

    [DataField]
    public EntityUid? RunebreakActionEntity;
}

[Serializable, NetSerializable]
public sealed class RuneCarvingSelectedMessage(EntProtoId protoId) : BoundUserInterfaceMessage
{
    public EntProtoId ProtoId { get; } = protoId;
}

[Serializable, NetSerializable]
public enum CarvingKnifeUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed partial class CarveRuneDoAfterEvent : DoAfterEvent
{
    public EntProtoId Carving;

    public CarveRuneDoAfterEvent() {}
    public CarveRuneDoAfterEvent(EntProtoId carving)
    {
        Carving = carving;
    }

    public override DoAfterEvent Clone() => new CarveRuneDoAfterEvent(Carving);
}

public sealed partial class DeleteAllCarvingsEvent : InstantActionEvent;
