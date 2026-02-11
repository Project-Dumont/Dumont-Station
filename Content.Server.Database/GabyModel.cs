// SPDX-FileCopyrightText: 2025 AvianMaiden <188556051+AvianMaiden@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2026 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// File to store as much CD related database things outside of Model.cs

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Content.Server.Database;

public static class GabyModel
{
    [Table("gaby_store_owned_items")]
    public class GabyStoreOwnedItems
    {
        [Key]
        public int Id { get; set; }

        public Guid PlayerId { get; set; }
        public DbPurchaseType Type { get; set; }
        public string Prototype { get; set; } = null!;
        public DateTime PurchaseDate { get; set; }
    }

    public enum DbPurchaseType : byte
    {
        Title = 0,
        GhostSkin = 1
    }
}
