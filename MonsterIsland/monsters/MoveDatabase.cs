using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MonsterIsland.monsters
{
    public class MoveData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public int Category { get; set; }
        public int Power { get; set; }
        public int Accuracy { get; set; }
        public int EffectChance { get; set; }
        public int EffectType { get; set; }
        public int SelfBuffStat { get; set; }
        public int SelfBuffAmount { get; set; }
        public int TargetDebuffStat { get; set; }
        public int TargetDebuffAmount { get; set; }
    }

    public class MovesJson
    {
        public List<MoveData> Moves { get; set; }
    }

    public static class MoveDatabase
    {
        private static Dictionary<int, Move> _moves;

        public static void LoadMoves(string jsonPath)
        {
            string json = File.ReadAllText(jsonPath);
            var movesData = JsonConvert.DeserializeObject<MovesJson>(json);  // Newtonsoft — case-insensitive by default

            if (movesData?.Moves == null)
            {
                System.Console.WriteLine("[MoveDatabase] ERROR: moves.json deserialized to null. Check the file path and JSON structure.");
                return;
            }

            _moves = new Dictionary<int, Move>();
            foreach (MoveData data in movesData.Moves)
            {
                var move = new Move(data.Name, data.Type, data.Category, data.Power, data.Accuracy)
                {
                    EffectChance = data.EffectChance,
                    EffectType = data.EffectType,
                    SelfBuffStat = data.SelfBuffStat,
                    SelfBuffAmount = data.SelfBuffAmount,
                    TargetDebuffStat = data.TargetDebuffStat,
                    TargetDebuffAmount = data.TargetDebuffAmount
                };
                _moves[data.Id] = move;
            }

            System.Console.WriteLine($"[MoveDatabase] Loaded {_moves.Count} moves.");
        }

        public static Move GetMove(int id) => _moves != null && _moves.ContainsKey(id) ? _moves[id] : null;
        public static Move GetMove(string name) => _moves?.Values.FirstOrDefault(m => m.Name == name);
        public static Dictionary<int, Move> GetAllMoves() => _moves;
    }
}