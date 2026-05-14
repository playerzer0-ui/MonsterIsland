using System.Collections.Generic;

namespace MonsterIsland.monsters
{
    /// <summary>
    /// One possible monster in an encounter zone, with its spawn weight and
    /// level range. Weight is relative — a weight of 60 vs 40 means 60% chance.
    /// </summary>
    public class EncounterEntry
    {
        public int MonsterSpeciesId { get; set; }
        public int WeightChance { get; set; }   // relative weight, not a percentage
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }
    }

    /// <summary>
    /// A named group of possible monsters that can appear in tiles tagged with
    /// this zone's ID in the encounter CSV.
    /// </summary>
    public class EncounterZone
    {
        public int ZoneId { get; set; }
        public string Name { get; set; }

        /// <summary>Chance (0-100) that stepping on this tile triggers a battle.</summary>
        public int EncounterRate { get; set; }

        /// <summary>How many monsters appear in the wild battle (e.g. 1–3).</summary>
        public int MinGroupSize { get; set; }
        public int MaxGroupSize { get; set; }

        public List<EncounterEntry> Pool { get; set; } = new();
    }
}
