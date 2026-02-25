namespace Content.Shared.Edible.Matter;

[RegisterComponent]
public sealed partial class EdibleMatterComponent : Component
{
    [DataField("nutritionValue")]
    public float NutritionValue = 5f;

    [DataField("canBeEaten")]
    public bool CanBeEaten = true;
}
