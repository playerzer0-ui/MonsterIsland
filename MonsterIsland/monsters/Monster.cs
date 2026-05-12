using Microsoft.Xna.Framework;
using NodeTesting.models;
using System;
using System.Collections.Generic;

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
        public int Type => _data.Type;
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Magic { get; set; }
        public int Resistance { get; set; }
        public int Speed { get; set; }
        public List<Move> Moves { get; set; } = new List<Move>();

        public int SheetIndex => _data.SheetIndex;

        public Monster(int speciesId, int level = 1)
        {
            SpeciesId = speciesId;
            _data = MonsterSpeciesDatabase.GetMonster(speciesId);
            Level = Math.Max(1, level);

            // Calculate stats based on species data and level
            MaxHealth = _data.BaseHealth + (Level * 5);
            Health = MaxHealth;
            Attack = _data.BaseAttack + (Level * 2);
            Defense = _data.BaseDefense + (Level * 2);
            Magic = _data.BaseMagic + (Level * 3);
            Resistance = _data.BaseResistance + (Level * 2);
            Speed = _data.BaseSpeed + (Level * 3);

            // Load moves from move database
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
                evolved.Position = Position;
                return evolved;
            }
            return this;
        }

        public void Draw()
        {
            _sheet.DrawFrame(SheetIndex, Position);
        }

        public bool UseMove(int moveIndex, Monster target)
        {
            if (moveIndex >= Moves.Count) return false;

            Move move = Moves[moveIndex];

            // Check accuracy
            if (new Random().Next(100) >= move.Accuracy)
            {
                return false;
            }

            // Apply damage
            if (move.Category != 2)
            {
                float multiplier = TypeEffectiveness.GetMultiplier(move.Type, target.Type);
                int damage = CalculateDamage(move, target, multiplier);
                target.Health -= damage;
            }

            // Apply status effect
            if (move.EffectChance > 0 && new Random().Next(100) < move.EffectChance)
            {
                ApplyStatusEffect(target, move.EffectType);
            }

            // Apply self buff
            if (move.SelfBuffStat != -1)
            {
                ApplyStatBuff(move.SelfBuffStat, move.SelfBuffAmount);
            }

            // Apply target debuff
            if (move.TargetDebuffStat != -1)
            {
                ApplyStatDebuff(target, move.TargetDebuffStat, move.TargetDebuffAmount);
            }

            return true;
        }

        private int CalculateDamage(Move move, Monster target, float multiplier)
        {
            int attackStat = move.Category == 0 ? Attack : Magic;
            int defenseStat = move.Category == 0 ? target.Defense : target.Resistance;

            int damage = (int)(((2 * Level / 5f + 2) * move.Power * attackStat / defenseStat / 50f + 2) * multiplier);
            return Math.Max(1, damage);
        }

        private void ApplyStatusEffect(Monster target, int effectType)
        {
            // You can track status effects as properties on Monster
        }

        private void ApplyStatBuff(int stat, int amount)
        {
            // Buff self
        }

        private void ApplyStatDebuff(Monster target, int stat, int amount)
        {
            // Debuff target
        }
    }
}