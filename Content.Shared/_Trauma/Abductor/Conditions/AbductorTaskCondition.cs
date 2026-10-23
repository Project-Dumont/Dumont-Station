// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Trauma.Abductor.Conditions;

/// <summary>
/// Something a subject or one of its parts has to meet for an abductor task.
/// </summary>
[ImplicitDataDefinitionForInheritors]
public abstract partial class AbductorTaskCondition
{
    /// <summary>
    /// Flips the result of this condition.
    /// </summary>
    [DataField]
    public bool Inverted;

    protected abstract bool Check(EntityUid target, IEntityManager entMan);

    public bool Condition(EntityUid target, IEntityManager entMan)
        => Check(target, entMan) ^ Inverted;

    /// <summary>
    /// Returns true if every condition passes, or if there are none.
    /// </summary>
    public static bool All(AbductorTaskCondition[]? conditions, EntityUid target, IEntityManager entMan)
    {
        if (conditions == null)
            return true;

        foreach (var condition in conditions)
        {
            if (!condition.Condition(target, entMan))
                return false;
        }

        return true;
    }
}
