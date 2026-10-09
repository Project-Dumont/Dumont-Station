// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Systems;

namespace Content.Shared._Trauma.Abductor.Conditions;

/// <summary>
/// Requires the subject to have an organ in a given slot, like <c>heart</c> or <c>eyes</c>.
/// Optionally checks nested conditions against the organ itself.
/// </summary>
public sealed partial class HasOrgan : AbductorTaskCondition
{
    [DataField(required: true)]
    public string Slot = string.Empty;

    /// <summary>
    /// Conditions the organ must meet.
    /// </summary>
    [DataField]
    public AbductorTaskCondition[]? Conditions;

    protected override bool Check(EntityUid target, IEntityManager entMan)
    {
        var body = entMan.System<SharedBodySystem>();
        foreach (var (organ, comp) in body.GetBodyOrgans(target))
        {
            if (comp.SlotId == Slot && All(Conditions, organ, entMan))
                return true;
        }

        return false;
    }
}
