// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Humanoid.Markings;

public static class TraumaMarkingCategories
{
    public static bool IgnoresMatchSkin(this MarkingCategories category)
    {
        return category switch
        {
            MarkingCategories.HairSpecial => true,
            MarkingCategories.FacialHairSpecial => true,
            _ => false
        };
    }
}
