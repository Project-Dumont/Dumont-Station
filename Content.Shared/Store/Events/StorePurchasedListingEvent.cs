// SPDX-FileCopyrightText: 2024 Rinary <72972221+Rinary1@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Store.Events;

public record struct StorePurchasedListingEvent(EntityUid Purchaser, ListingData Listing, EntityUid? Item, EntityUid? Action);
