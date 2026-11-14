using Content.Shared.Damage;
using Content.Shared._Shitmed.Targeting;
using Robust.Shared.GameStates;

namespace Content.Shared.Trigger.Components.Effects;

/// <summary>
/// Will damage an entity when triggered.
/// If TargetUser is true it the user will take damage instead.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DamageOnTriggerComponent : BaseXOnTriggerComponent
{
    /// <summary>
    /// Should the damage ignore resistances?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IgnoreResistances;

    [DataField, AutoNetworkedField]
    public TargetBodyPart? TargetPart;

    /// <summary>
    /// The base damage amount that is dealt.
    /// May be further modified by <see cref="Systems.BeforeDamageOnTriggerEvent"/> subscriptions.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public DamageSpecifier Damage = default!;
}
