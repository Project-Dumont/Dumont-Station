// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Weapons;

[ByRefEvent]
public record struct GetLightAttackRangeEvent(EntityUid? Target, EntityUid User, float Range, bool Cancel = false);


[ByRefEvent]
public record struct LightAttackSpecialInteractionEvent(EntityUid? Target, EntityUid User, float Range, bool Cancel = false);
