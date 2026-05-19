using Microsoft.Xna.Framework;
using NodeTesting.models;

namespace MonsterIsland.monsters
{
    /// <summary>
    /// Draws the type icon for a given type ID (0=Flame, 1=Wave, 2=Gale,
    /// 3=Stone, 4=Spark, 5=Dragon, 6=Prismatic).
    /// The symbol sheet has 15 frames — first 7 are the monster types.
    /// Call TypeSymbol.LoadContent() once in Monster.LoadContent().
    /// </summary>
    public static class TypeSymbol
    {
        private static SpriteSheet _sheet;

        public static void LoadContent()
        {
            _sheet = new SpriteSheet("symbols-Sheet", 15);
        }

        /// <summary>
        /// Draws the type icon centred on the given position.
        /// typeId 0–6 maps to Flame–Prismatic. Out-of-range IDs are ignored.
        /// </summary>
        public static void Draw(int typeId, Vector2 centrePosition)
        {
            if (_sheet == null || typeId < 0 || typeId > 6) return;
            _sheet.DrawFrame(typeId, centrePosition, 0.5f);
        }
    }
}