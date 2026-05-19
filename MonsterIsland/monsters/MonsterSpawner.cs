using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonsterIsland.monsters
{
    public class MonsterSpawner
    {
        private readonly EncounterMap _encounterMap;
        private readonly Random _rng = new();

        public MonsterSpawner(EncounterMap encounterMap)
        {
            _encounterMap = encounterMap;
        }

        public List<Monster> TrySpawnEncounter(Point gridTile)
        {
            int zoneId = _encounterMap.GetZoneId(gridTile);

            // Step on a tile with no encounter data — do nothing silently
            if (zoneId < 0) return null;

            Console.WriteLine($"[Spawner] Stepped on tile ({gridTile.X},{gridTile.Y}) — zone {zoneId}");

            EncounterZone zone = EncounterRegistry.GetZone(zoneId);
            if (zone == null || zone.Pool.Count == 0)
            {
                Console.WriteLine($"[Spawner] Zone {zoneId} has no pool defined, skipping.");
                return null;
            }

            // Roll encounter chance
            int roll = _rng.Next(100);
            Console.WriteLine($"[Spawner] Encounter roll: {roll} (need < {zone.EncounterRate} to trigger)");

            if (roll >= zone.EncounterRate)
            {
                Console.WriteLine($"[Spawner] No encounter this step.");
                return null;
            }

            // Decide group size
            int groupSize = _rng.Next(zone.MinGroupSize, zone.MaxGroupSize + 1);
            Console.WriteLine($"[Spawner] Battle triggered! Spawning {groupSize} monster(s).");

            var group = new List<Monster>(groupSize);
            for (int i = 0; i < groupSize; i++)
            {
                EncounterEntry entry = PickFromPool(zone.Pool);
                int level = _rng.Next(entry.MinLevel, entry.MaxLevel + 1);
                var monster = new Monster(entry.MonsterSpeciesId, level);
                group.Add(monster);
                Console.WriteLine($"[Spawner]   Monster {i + 1}: {monster.Name} (ID {entry.MonsterSpeciesId}) Lv.{level}");
            }

            return group;
        }

        private EncounterEntry PickFromPool(List<EncounterEntry> pool)
        {
            int total = 0;
            foreach (EncounterEntry e in pool) total += e.WeightChance;

            int roll = _rng.Next(total);
            int cumulative = 0;
            foreach (EncounterEntry e in pool)
            {
                cumulative += e.WeightChance;
                if (roll < cumulative) return e;
            }

            return pool[pool.Count - 1];
        }
    }
}