using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MonsterIsland.monsters
{
    public class FusionData
    {
        public List<int> RequiredMonsterIds { get; set; }
        public int FusionLevel { get; set; }
    }

    public class MonsterData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<int> Types { get; set; }
        public int BaseHealth { get; set; }
        public int BaseAttack { get; set; }
        public int BaseDefense { get; set; }
        public int BaseMagic { get; set; }
        public int BaseResistance { get; set; }
        public int BaseSpeed { get; set; }
        public List<int> MoveIds { get; set; }
        public int? EvolvesTo { get; set; }
        public int EvolutionLevel { get; set; }
        public int SheetIndex { get; set; }
        public FusionData Fusion { get; set; }  // Null for non-fusion monsters

        public int PrimaryType => Types != null && Types.Count > 0 ? Types[0] : 0;
        public int? SecondaryType => Types != null && Types.Count > 1 ? Types[1] : (int?)null;
        public bool IsFusion => Fusion != null;
    }

    public class MonstersJson
    {
        public List<MonsterData> Monsters { get; set; }
    }

    public static class MonsterSpeciesDatabase
    {
        private static Dictionary<int, MonsterData> _monsters;

        public static void LoadMonsters(string jsonPath)
        {
            string json = File.ReadAllText(jsonPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var monstersData = JsonSerializer.Deserialize<MonstersJson>(json, options);

            _monsters = new Dictionary<int, MonsterData>();
            foreach (var data in monstersData.Monsters)
            {
                _monsters[data.Id] = data;
            }
        }

        public static MonsterData GetMonster(int id) => _monsters.ContainsKey(id) ? _monsters[id] : null;
        public static MonsterData GetMonster(string name) => _monsters.Values.FirstOrDefault(m => m.Name == name);
        public static Dictionary<int, MonsterData> GetAllMonsters() => _monsters;
    }
}