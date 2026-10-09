// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Damage;
using Content.Goobstation.Maths.FixedPoint;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Blade;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class HereticArenaParticipantComponent : BaseSpriteOverlayComponent
{
    [DataField]
    public EntityUid? Weapon;

    [DataField]
    public EntityUid? Mind;

    [DataField]
    public EntProtoId WeaponProto = "HereticBladeArena";

    [DataField]
    public EntProtoId RoleProto = "MindRoleArenaParticipant";

    [DataField]
    public FixedPoint2 HereticHealPerCrit = 20;

    [DataField, AutoNetworkedField]
    public bool IsVictor;

    [DataField]
    public DamageSpecifier DamageOnCrit = new()
    {
        DamageDict =
        {
            { "Slash", 50 },
        }
    };

    [DataField]
    public DamageModifierSet ModifierSet = new()
    {
        Coefficients =
        {
            { "Radiation", 0f },
        },
        IgnoreArmorPierceFlags = (int) PartialArmorPierceFlags.All,
    };

    // Component name -> whether entity had this component before
    [DataField]
    public Dictionary<string, bool> GrantedComponentDictionary = new()
    {
        {"SpecialBreathingImmunity", false},
        {"MovementIgnoreGravity", false},
    };

    /// <summary>
    /// Status Effects to add
    /// </summary>
    [DataField]
    public List<EntProtoId> StatusEffects = new()
    {
        "StatusEffectPressureImmunityArena"
    };

    [DataField]
    public string FighterState = "arena_fighter";

    [DataField]
    public string VictorState = "arena_victor";

    [DataField]
    public override SpriteSpecifier? Sprite { get; set; } =
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/Effects/crown.rsi"), "arena_fighter");

    public override Vector2 Offset { get; set; } = new (0f, 2f / 3f);

    public override Enum Key { get; set; } = HereticArenaKey.Key;

    public override bool Unshaded { get; set; } = false;
}

public enum HereticArenaKey : byte
{
    Key
}
