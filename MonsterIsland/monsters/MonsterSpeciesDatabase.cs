using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MonsterIsland.monsters
{
    public class MonsterData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
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
            var monstersData = JsonSerializer.Deserialize<MonstersJson>(json);

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