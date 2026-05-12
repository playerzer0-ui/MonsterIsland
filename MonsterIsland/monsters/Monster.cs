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
        public List<Move> Moves { get; set; } = new List<Move>();
        public int SheetIndex => _data.SheetIndex;
        public bool IsFusion => _data.IsFusion;

        public string TypeDisplay => Types.Count == 1
            ? GetTypeName(Types[0])
            : $"{GetTypeName(Types[0])}/{GetTypeName(Types[1])}";

        public Monster(int speciesId, int level = 1)
        {
            SpeciesId = speciesId;
            _data = MonsterSpeciesDatabase.GetMonster(speciesId);
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
                var move = MoveDatabase.GetMove(moveId);
                if (move != null)
                    Moves.Add(move);
            }
        }

        public static void LoadContent(string movesJsonPath, string monstersJsonPath)
        {
            _sheet = new SpriteSheet("monsters", 124);
            MoveDatabase.LoadMoves(movesJsonPath);
            MonsterSpeciesDatabase.LoadMonsters(monstersJsonPath);
        }

        public Monster EvolveTo()
        {
            if (_data.EvolvesTo.HasValue && Level >= _data.EvolutionLevel)
            {
                var evolved = new Monster(_data.EvolvesTo.Value, Level);
                return evolved;
            }
            return this;
        }

        // Check if this monster can fuse with another
        public bool CanFuseWith(Monster other)
        {
            if (!_data.IsFusion && !other._data.IsFusion) return false;

            // Check if this monster is part of other's fusion requirements
            if (other._data.IsFusion && other._data.Fusion.RequiredMonsterIds.Contains(SpeciesId))
                return true;

            // Check if other monster is part of this monster's fusion requirements
            if (_data.IsFusion && _data.Fusion.RequiredMonsterIds.Contains(other.SpeciesId))
                return true;

            return false;
        }

        // Get the fusion result if these two monsters can fuse
        public Monster GetFusionResult(Monster other)
        {
            // Find a fusion monster that requires both of these species
            foreach (var monsterData in MonsterSpeciesDatabase.GetAllMonsters().Values)
            {
                if (monsterData.IsFusion &&
                    monsterData.Fusion.RequiredMonsterIds.Contains(SpeciesId) &&
                    monsterData.Fusion.RequiredMonsterIds.Contains(other.SpeciesId))
                {
                    return new Monster(monsterData.Id, Math.Max(Level, other.Level));
                }
            }
            return null;
        }

        public static Monster Fuse(Monster monster1, Monster monster2)
        {
            foreach (var monsterData in MonsterSpeciesDatabase.GetAllMonsters().Values)
            {
                if (monsterData.IsFusion &&
                    monsterData.Fusion.RequiredMonsterIds.Contains(monster1.SpeciesId) &&
                    monsterData.Fusion.RequiredMonsterIds.Contains(monster2.SpeciesId))
                {
                    int requiredLevel = monsterData.Fusion.FusionLevel;
                    if (monster1.Level >= requiredLevel && monster2.Level >= requiredLevel)
                    {
                        int fusionLevel = Math.Max(monster1.Level, monster2.Level);
                        return new Monster(monsterData.Id, fusionLevel);
                    }
                }
            }
            return null;
        }

        // Special fusion for Prismadra (requires 5 specific monsters)
        public static Monster FusePrismadra(List<Monster> monsters)
        {
            var prismadraData = MonsterSpeciesDatabase.GetMonster(123);
            if (prismadraData == null || !prismadraData.IsFusion) return null;

            var requiredIds = prismadraData.Fusion.RequiredMonsterIds;

            // Check if all required monsters are present
            var providedIds = monsters.Select(m => m.SpeciesId).ToList();
            if (!requiredIds.All(id => providedIds.Contains(id))) return null;

            // Check level requirements
            int requiredLevel = prismadraData.Fusion.FusionLevel;
            if (!monsters.All(m => m.Level >= requiredLevel)) return null;

            int maxLevel = monsters.Max(m => m.Level);
            return new Monster(123, maxLevel);
        }

        public void Draw()
        {
            _sheet.DrawFrame(SheetIndex, Position);
        }

        public bool UseMove(int moveIndex, Monster target)
        {
            if (moveIndex >= Moves.Count) return false;

            Move move = Moves[moveIndex];

            if (new Random().Next(100) >= move.Accuracy)
                return false;

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
            int attackStat = move.Category == 0 ? Attack : Magic;
            int defenseStat = move.Category == 0 ? target.Defense : target.Resistance;

            int damage = (int)(((2 * Level / 5f + 2) * move.Power * attackStat / defenseStat / 50f + 2) * multiplier);
            return Math.Max(1, damage);
        }

        private void ApplyStatusEffect(Monster target, int effectType) { }
        private void ApplyStatBuff(int stat, int amount) { }
        private void ApplyStatDebuff(Monster target, int stat, int amount) { }

        private static string GetTypeName(int typeId)
        {
            return typeId switch
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
}