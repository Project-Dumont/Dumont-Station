namespace Content.Shared.Clothing
{
    [RegisterComponent]
    public sealed partial class TrayGlassesComponent : Component
    {
        [DataField("slot")]
        public string Slot = "eyes";
    }
}
