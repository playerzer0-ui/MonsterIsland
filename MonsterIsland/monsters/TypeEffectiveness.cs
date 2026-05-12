using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterIsland.monsters
{
    public static class TypeEffectiveness
    {
        // Order: 0=Flame, 1=Wave, 2=Gale, 3=Stone, 4=Spark, 5=Dragon, 6=Prismatic
        private static float[,] chart = new float[7, 7]
        {
        // Attacking type →, Defending type ↓
        //           Flame  Wave   Gale   Stone  Spark  Dragon Prismatic
        /*Flame*/   { 1.0f, 0.5f, 2.0f, 2.0f, 0.5f, 0.5f, 0.5f },
        /*Wave*/    { 2.0f, 1.0f, 0.5f, 0.5f, 2.0f, 1.0f, 0.5f },
        /*Gale*/    { 0.5f, 2.0f, 1.0f, 1.0f, 1.0f, 2.0f, 0.5f },
        /*Stone*/   { 0.5f, 2.0f, 2.0f, 1.0f, 2.0f, 1.0f, 0.5f },
        /*Spark*/   { 2.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f, 0.5f },
        /*Dragon*/  { 2.0f, 1.0f, 0.5f, 2.0f, 2.0f, 2.0f, 0.5f },
        /*Prismatic*/{2.0f,2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f }
        };

        public static float GetMultiplier(int attackType, int defenseType)
        {
            if (attackType < 0 || attackType > 6 || defenseType < 0 || defenseType > 6)
                return 1.0f;

            return chart[attackType, defenseType];
        }
    }
}
