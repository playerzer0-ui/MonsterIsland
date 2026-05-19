using Microsoft.Xna.Framework;
using NodeTesting.models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonsterIsland.monsters
{
    public class Monster
    {
        private static SpriteSheet _sheet;
        private MonsterData _data;

        public Vector2 Position { get; set; }
        public int Level { get; set; }
        public int SpeciesId { get; protected set; }
        public string Name => _data.Name;
        public List<int> Types => _data.Types;
        public int PrimaryType => _data.PrimaryType;
        public int? SecondaryType => _data.SecondaryType;
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Magic { get; set; }
        public int Resistance { get; set; }
        public int Speed { get; set; }
        public List<Move> Moves { get; set; } = new();
        public int SheetIndex => _data.SheetIndex;
        public bool IsFusion => _data.IsFusion;

        public string TypeDisplay => Types.Count == 1
            ? GetTypeName(Types[0])
            : $"{GetTypeName(Types[0])}/{GetTypeName(Types[1])}";

        public Monster(int speciesId, int level = 1)
        {
            SpeciesId = speciesId;
            _data = MonsterSpeciesDatabase.GetMonster(speciesId);

            if (_data == null)
            {
                Console.WriteLine($"[Monster] ERROR: no species found for ID {speciesId}");
                return;
            }

            Level = Math.Max(1, level);
            CalculateStats();
            LoadMoves();
        }

        private void CalculateStats()
        {
            MaxHealth = _data.BaseHealth + (Level * 5);
            Health = MaxHealth;
            Attack = _data.BaseAttack + (Level * 2);
            Defense = _data.BaseDefense + (Level * 2);
            Magic = _data.BaseMagic + (Level * 3);
            Resistance = _data.BaseResistance + (Level * 2);
            Speed = _data.BaseSpeed + (Level * 3);
        }

        private void LoadMoves()
        {
            Moves.Clear();
            foreach (int moveId in _data.MoveIds)
            {
                Move move = MoveDatabase.GetMove(moveId);
                if (move != null)
                    Moves.Add(move);
                else
                    Console.WriteLine($"[Monster] WARNING: move ID {moveId} not found for {_data.Name}");
            }
        }

        /// <summary>
        /// Call once in Game1.LoadContent() before creating any Monster instances.
        /// </summary>
        public static void LoadContent(string movesJsonPath, string monstersJsonPath)
        {
            _sheet = new SpriteSheet("monsters-Sheet", 124);   // monsters-Sheet.png
            MoveDatabase.LoadMoves(movesJsonPath);
            MonsterSpeciesDatabase.LoadMonsters(monstersJsonPath);
            TypeSymbol.LoadContent();                           // symbol-Sheet.png
        }

        public Monster EvolveTo()
        {
            if (_data.EvolvesTo.HasValue && Level >= _data.EvolutionLevel)
                return new Monster(_data.EvolvesTo.Value, Level);
            return this;
        }

        public bool CanFuseWith(Monster other)
        {
            if (!_data.IsFusion && !other._data.IsFusion) return false;
            if (other._data.IsFusion && other._data.Fusion.RequiredMonsterIds.Contains(SpeciesId)) return true;
            if (_data.IsFusion && _data.Fusion.RequiredMonsterIds.Contains(other.SpeciesId)) return true;
            return false;
        }

        public Monster GetFusionResult(Monster other)
        {
            foreach (var data in MonsterSpeciesDatabase.GetAllMonsters().Values)
            {
                if (data.IsFusion &&
                    data.Fusion.RequiredMonsterIds.Contains(SpeciesId) &&
                    data.Fusion.RequiredMonsterIds.Contains(other.SpeciesId))
                    return new Monster(data.Id, Math.Max(Level, other.Level));
            }
            return null;
        }

        public static Monster Fuse(Monster a, Monster b)
        {
            foreach (var data in MonsterSpeciesDatabase.GetAllMonsters().Values)
            {
                if (data.IsFusion &&
                    data.Fusion.RequiredMonsterIds.Contains(a.SpeciesId) &&
                    data.Fusion.RequiredMonsterIds.Contains(b.SpeciesId))
                {
                    if (a.Level >= data.Fusion.FusionLevel && b.Level >= data.Fusion.FusionLevel)
                        return new Monster(data.Id, Math.Max(a.Level, b.Level));
                }
            }
            return null;
        }

        public static Monster FusePrismadra(List<Monster> monsters)
        {
            var prismadraData = MonsterSpeciesDatabase.GetMonster(123);
            if (prismadraData == null || !prismadraData.IsFusion) return null;

            var requiredIds = prismadraData.Fusion.RequiredMonsterIds;
            var providedIds = monsters.Select(m => m.SpeciesId).ToList();
            if (!requiredIds.All(id => providedIds.Contains(id))) return null;

            if (!monsters.All(m => m.Level >= prismadraData.Fusion.FusionLevel)) return null;

            return new Monster(123, monsters.Max(m => m.Level));
        }

        public void Draw()
        {
            _sheet.DrawFrame(SheetIndex, Position);
        }

        public bool UseMove(int moveIndex, Monster target)
        {
            if (moveIndex >= Moves.Count) return false;

            Move move = Moves[moveIndex];

            if (new Random().Next(100) >= move.Accuracy) return false;

            if (move.Category != 2)
            {
                float multiplier = TypeEffectiveness.GetMultiplier(move.Type, target.Types);
                int damage = CalculateDamage(move, target, multiplier);
                target.Health -= damage;
            }

            if (move.EffectChance > 0 && new Random().Next(100) < move.EffectChance)
                ApplyStatusEffect(target, move.EffectType);

            if (move.SelfBuffStat != -1)
                ApplyStatBuff(move.SelfBuffStat, move.SelfBuffAmount);

            if (move.TargetDebuffStat != -1)
                ApplyStatDebuff(target, move.TargetDebuffStat, move.TargetDebuffAmount);

            return true;
        }

        private int CalculateDamage(Move move, Monster target, float multiplier)
        {
            int atk = move.Category == 0 ? Attack : Magic;
            int def = move.Category == 0 ? target.Defense : target.Resistance;
            int dmg = (int)(((2 * Level / 5f + 2) * move.Power * atk / def / 50f + 2) * multiplier);
            return Math.Max(1, dmg);
        }

        private void ApplyStatusEffect(Monster target, int effectType) { }
        private void ApplyStatBuff(int stat, int amount) { }
        private void ApplyStatDebuff(Monster target, int stat, int amount) { }

        private static string GetTypeName(int typeId) => typeId switch
        {
            0 => "Flame",
            1 => "Wave",
            2 => "Gale",
            3 => "Stone",
            4 => "Spark",
            5 => "Dragon",
            6 => "Prismatic",
            _ => "Unknown"
        };
    }
}