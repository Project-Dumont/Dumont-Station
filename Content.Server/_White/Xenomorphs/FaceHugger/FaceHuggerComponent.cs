// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Part;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._White.Xenomorphs.FaceHugger;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class FaceHuggerComponent : Component
{
    [DataField]
    public (BodyPartType Type, BodyPartSymmetry Symmetry) InfectionBodyPart = (BodyPartType.Chest, BodyPartSymmetry.None);

    [DataField]
    public DamageSpecifier DamageOnInfect = new();

    [DataField]
    public EntityWhitelist? Blacklist;

    [DataField]
    public EntProtoId? InfectionPrototype = "XenomorphInfection";

    [DataField]
    public string InfectionSlotId = "xenomorph_larva";

    [DataField]
    public string Slot = "mask";

    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan ProtectionDelay = TimeSpan.FromSeconds(15);

    [ViewVariables]
    public DoAfterId? ProtectionDoAfter;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextStruggleSound;

    [ViewVariables]
    public bool ProtectionRequired;

    [DataField]
    public TimeSpan MaxRestTime = TimeSpan.FromSeconds(5); // Goobstation - 20 to 5. Facehuggers shouldn't take that long to recover.

    // Goobstation start
    [DataField]
    public string? SleepChem = "Nocturine"; // Dumont

    [DataField]
    public float SleepChemAmount = 10f;

    [DataField]
    public TimeSpan InjectionInterval = TimeSpan.FromSeconds(5); // How often to inject chemicals

    [DataField]
    public TimeSpan InitialInjectionDelay = TimeSpan.FromSeconds(5); // Delay before the first injection

    [ViewVariables]
    public TimeSpan NextInjectionTime = TimeSpan.Zero; // Saves the time of the next injection

    [DataField]
    public TimeSpan MinRestTime = TimeSpan.FromSeconds(3); // Must be less than MaxRestTime (makes facehugger jump randomly between max & min)

    [DataField]
    public float MinChemicalThreshold = 0f; // Minimum amount of the chemical required to prevent additional injections
    // Goobstation end

    [ViewVariables]
    public bool Active = true;

    [ViewVariables]
    public TimeSpan RestIn = TimeSpan.Zero;
}
