using Content.Shared.Actions;
using Content.Shared.Humanoid.Markings;
using Robust.Shared.Serialization;

namespace Content.Shared.Genetics;

public sealed partial class MorphismGenActionEvent : InstantActionEvent { }

[Serializable, NetSerializable]
public sealed class MorphismMenuOpenedEvent : EntityEventArgs
{
    public NetEntity TargetUser;
    public string Species;
    public Color SkinColor;
    public Color EyeColor;
    public MarkingSet Markings;

    public MorphismMenuOpenedEvent(NetEntity targetUser, string species, Color skinColor, Color eyeColor, MarkingSet markings)
    {
        TargetUser = targetUser;
        Species = species;
        SkinColor = skinColor;
        EyeColor = eyeColor;
        Markings = markings;
    }
}

[Serializable, NetSerializable]
public sealed class MorphismChangeMarkingEvent : EntityEventArgs
{
    public NetEntity TargetUser;
    public MarkingCategories Category;
    public int Slot;
    public string MarkingId;

    public MorphismChangeMarkingEvent(NetEntity targetUser, MarkingCategories category, int slot, string markingId)
    {
        TargetUser = targetUser;
        Category = category;
        Slot = slot;
        MarkingId = markingId;
    }
}

[Serializable, NetSerializable]
public sealed class MorphismChangeMarkingColorEvent : EntityEventArgs
{
    public NetEntity TargetUser;
    public MarkingCategories Category;
    public int Slot;
    public List<Color> Colors;

    public MorphismChangeMarkingColorEvent(NetEntity targetUser, MarkingCategories category, int slot, List<Color> colors)
    {
        TargetUser = targetUser;
        Category = category;
        Slot = slot;
        Colors = colors;
    }
}

[Serializable, NetSerializable]
public sealed class MorphismSlotEvent : EntityEventArgs
{
    public enum SlotAction { Add, Remove }
    
    public NetEntity TargetUser;
    public MarkingCategories Category;
    public SlotAction Action;
    public int SlotIndex;

    public MorphismSlotEvent(NetEntity targetUser, MarkingCategories category, SlotAction action, int slotIndex = 0)
    {
        TargetUser = targetUser;
        Category = category;
        Action = action;
        SlotIndex = slotIndex;
    }
}

[Serializable, NetSerializable]
public sealed class MorphismChangeBodyColorEvent : EntityEventArgs
{
    public enum BodyPart { Skin, Eye }
    public NetEntity TargetUser;
    public BodyPart Part;
    public Color NewColor;

    public MorphismChangeBodyColorEvent(NetEntity targetUser, BodyPart part, Color newColor)
    {
        TargetUser = targetUser;
        Part = part;
        NewColor = newColor;
    }
}
