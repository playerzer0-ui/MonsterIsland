using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterIsland.monsters
{
    public class Move
    {
        public string Name { get; set; }
        public int Type { get; set; }      // 0=Flame, 1=Wave, 2=Gale, 3=Stone, 4=Spark, 5=Dragon, 6=Prismatic
        public int Category { get; set; }   // 0=Physical, 1=Special, 2=Status
        public int Power { get; set; }
        public int Accuracy { get; set; }

        // Animation frame index in your sheet
        public int AnimationFrame { get; set; }

        // Effect (0=none, 1=burn, 2=freeze, 3=paralysis, 4=confusion, 5=flinch, etc.)
        public int EffectChance { get; set; }
        public int EffectType { get; set; }

        // Stat buffs/debuffs
        public int SelfBuffStat { get; set; }     // -1=none, 0=Attack, 1=Defense, 2=Speed, etc.
        public int SelfBuffAmount { get; set; }   // Positive or negative
        public int TargetDebuffStat { get; set; }
        public int TargetDebuffAmount { get; set; }

        public Move(string name, int type, int category, int power, int accuracy, int animationFrame = -1)
        {
            Name = name;
            Type = type;
            Category = category;
            Power = power;
            Accuracy = accuracy;
            AnimationFrame = animationFrame;
            EffectChance = 0;
            EffectType = 0;
            SelfBuffStat = -1;
            TargetDebuffStat = -1;
        }
    }
}
