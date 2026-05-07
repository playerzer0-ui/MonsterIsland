using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace NodeTesting.models
{
    /// <summary>
    /// Defines one arrow exit for a zone.
    /// ArrowTile   = the path tile the arrow is drawn on (an entry tile of the zone).
    /// ArrowFrame  = direction the arrow faces — always OUTWARD from the zone.
    /// DestinationTile = where the player warps when they click this arrow.
    /// </summary>
    public class ArrowExit
    {
        public int ArrowFrame;
        public Point ArrowTile;
        public Point DestinationTile;
    }

    public class ZoneDefinition
    {
        public string Name;
        public List<Point> EntryTiles;
        public Vector2 ZoomTarget;
        public float ZoomLevel;
        public List<ArrowExit> ArrowExits = new();
    }

    public static class ZoneRegistry
    {
        private const int T = 32;

        public static readonly List<ZoneDefinition> Zones = new()
        {
            // ----------------------------------------------------------------
            // Town — three entry tiles: left (14,7), right (18,7), bottom (16,9)
            //
            // Every entry tile gets an arrow for EACH of the other two exits.
            // The arrow manager skips whichever arrow sits on the player's current tile,
            // so the player always sees exactly the exits they can jump to.
            //
            // Outward direction per tile:
            //   (14,7) left border  → arrow faces Left  (points back down the left road)
            //   (18,7) right border → arrow faces Right (points back down the right road)
            //   (16,9) bottom       → arrow faces Down  (points back down the bottom road)
            // ----------------------------------------------------------------
            new ZoneDefinition
            {
                Name = "Town",
                EntryTiles = new List<Point>
                {
                    new Point(14, 7),
                    new Point(18, 7),
                    new Point(16, 9),
                },
                ZoomTarget = new Vector2(16 * T, 7 * T),
                ZoomLevel  = 2.5f,
                ArrowExits = new List<ArrowExit>
                {
                    // Arrow ON left tile  → faces Left  → warps to left tile
                    // (shown when player is at right or bottom entry)
                    new ArrowExit
                    {
                        ArrowFrame      = ZoneArrow.Left,
                        ArrowTile       = new Point(14, 7),
                        DestinationTile = new Point(14, 7)
                    },
                    // Arrow ON right tile → faces Right → warps to right tile
                    // (shown when player is at left or bottom entry)
                    new ArrowExit
                    {
                        ArrowFrame      = ZoneArrow.Right,
                        ArrowTile       = new Point(18, 7),
                        DestinationTile = new Point(18, 7)
                    },
                    // Arrow ON bottom tile → faces Down → warps to bottom tile
                    // (shown when player is at left or right entry)
                    new ArrowExit
                    {
                        ArrowFrame      = ZoneArrow.Down,
                        ArrowTile       = new Point(16, 9),
                        DestinationTile = new Point(16, 9)
                    },
                }
            },

            // ----------------------------------------------------------------
            // Mage Tower — single entrance (20,5). Nothing to cross to.
            // ----------------------------------------------------------------
            new ZoneDefinition
            {
                Name       = "Mage Tower",
                EntryTiles = new List<Point> { new Point(20, 5) },
                ZoomTarget = new Vector2(20 * T, 4 * T),
                ZoomLevel  = 3f,
                ArrowExits = new List<ArrowExit>()
            },

            // ----------------------------------------------------------------
            // Village — single entrance (8,6). Nothing to cross to.
            // ----------------------------------------------------------------
            new ZoneDefinition
            {
                Name       = "Village",
                EntryTiles = new List<Point> { new Point(8, 6) },
                ZoomTarget = new Vector2(7 * T, 5 * T),
                ZoomLevel  = 2.5f,
                ArrowExits = new List<ArrowExit>()
            },

            // ----------------------------------------------------------------
            // Cave — single entrance (8,14). Nothing to cross to.
            // ----------------------------------------------------------------
            new ZoneDefinition
            {
                Name       = "Cave",
                EntryTiles = new List<Point> { new Point(8, 14) },
                ZoomTarget = new Vector2(8 * T, 12 * T),
                ZoomLevel  = 2.5f,
                ArrowExits = new List<ArrowExit>()
            },
        };
    }
}