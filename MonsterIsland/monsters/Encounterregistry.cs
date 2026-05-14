using System.Collections.Generic;

namespace MonsterIsland.monsters
{
    /// <summary>
    /// Defines all encounter zones. Zone IDs here match the values in the
    /// encounter CSVs — zone 0 is the starter area grass, and so on.
    ///
    /// To add a new area: add a new EncounterZone with the next ID and populate
    /// its pool. The CSV for that map just needs to use that ID number on the
    /// relevant tiles.
    /// </summary>
    public static class EncounterRegistry
    {
        public static readonly List<EncounterZone> Zones = new()
        {
            // ----------------------------------------------------------------
            // Zone 0 — Starter area paths
            // 25% chance of battle per step, groups of 1–3 monsters
            // ----------------------------------------------------------------
            new EncounterZone
            {
                ZoneId        = 0,
                Name          = "Starter Wilds",
                EncounterRate = 25,
                MinGroupSize  = 1,
                MaxGroupSize  = 3,
                Pool = new List<EncounterEntry>
                {
                    new EncounterEntry { MonsterSpeciesId = 1,  WeightChance = 40, MinLevel = 2, MaxLevel = 5  }, // Kindlecko
                    new EncounterEntry { MonsterSpeciesId = 4,  WeightChance = 30, MinLevel = 2, MaxLevel = 5  }, // Sootruff
                    new EncounterEntry { MonsterSpeciesId = 6,  WeightChance = 20, MinLevel = 3, MaxLevel = 6  }, // Emberpup
                    new EncounterEntry { MonsterSpeciesId = 10, WeightChance = 10, MinLevel = 4, MaxLevel = 7  }, // Candleaf (rarer)
                }
            },

            // ----------------------------------------------------------------
            // Zone 1 — placeholder for the next map (sea area etc.)
            // Fill in when ready.
            // ----------------------------------------------------------------
            new EncounterZone
            {
                ZoneId        = 1,
                Name          = "Sea Shallows",
                EncounterRate = 30,
                MinGroupSize  = 1,
                MaxGroupSize  = 3,
                Pool = new List<EncounterEntry>
                {
                    // Populate with wave-type monsters when sea monsters are decided
                }
            },
        };

        /// <summary>Returns the zone definition for a given ID, or null if not found.</summary>
        public static EncounterZone GetZone(int zoneId)
        {
            foreach (EncounterZone z in Zones)
                if (z.ZoneId == zoneId) return z;
            return null;
        }
    }
}