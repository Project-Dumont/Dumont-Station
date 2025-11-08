<<<<<<<< HEAD:Content.Server/Cocoon/CocoonComponent.cs
namespace Content.Server.Arachne
========
namespace Content.Shared.Cocoon
>>>>>>>> a78775bc99 (Cocoon Cleanup & Minor Bloodsucker Tweaks (#1058)):Content.Shared/Cocoon/CocoonComponent.cs
{
    [RegisterComponent]
    public sealed partial class CocoonComponent : Component
    {
        public string? OldAccent;

        public EntityUid? Victim;

        [DataField("damagePassthrough")]
        public float DamagePassthrough = 0.5f;

    }
}
