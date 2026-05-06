using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace NodeTesting.models
{
    public class ZoneExit
    {
        public Point ZoneTile;    // tile inside the zone the player stands on to exit
        public Point WorldTile;   // world path tile they return to
    }

    public class ZoneDefinition
    {
        public string Name;
        public List<Point> EntryTiles;   // ALL path tiles that border this zone
        public Vector2 ZoomTarget;       // world pixel the camera zooms toward
        public float ZoomLevel;
    }

    public static class ZoneRegistry
    {
        private const int T = 32; // tile size

        public static readonly List<ZoneDefinition> Zones = new()
        {
            // Zone 0 — large 3x3 town, rows 6-8 cols 15-17
            // Borders: left side touches path col 14 (rows 7,8), bottom touches path row 9 col 16
            new ZoneDefinition
            {
                Name = "Town",
                EntryTiles = new List<Point>
                {
                    new Point(14, 7),   // left entrance
                    new Point(18, 7),   // right entrance
                    new Point(16, 9),   // bottom entrance
                },
                ZoomTarget = new Vector2(16 * T, 7 * T),
                ZoomLevel = 2.5f
            },

            // Zone 1 — single tile, row 4 col 20
            // Only one path neighbor: col 20 row 5 (below it)
            new ZoneDefinition
            {
                Name = "Mage Tower",
                EntryTiles = new List<Point>
                {
                    new Point(20, 5),   // path tile directly below zone tile
                },
                ZoomTarget = new Vector2(20 * T, 4 * T),
                ZoomLevel = 3f
            },

            // Zone 2 — 3 tiles in a row, row 5 cols 6-8
            // Only path neighbor found: col 8 row 6 (below rightmost tile)
            new ZoneDefinition
            {
                Name = "Village",
                EntryTiles = new List<Point>
                {
                    new Point(8, 6),    // path tile below zone, col 8
                },
                ZoomTarget = new Vector2(7 * T, 5 * T),
                ZoomLevel = 2.5f
            },

            // Zone 3 — 2x3 block, rows 12-13 cols 7-9
            // Only path neighbor found: col 8 row 14 (below zone)
            new ZoneDefinition
            {
                Name = "Cave",
                EntryTiles = new List<Point>
                {
                    new Point(8, 14),   // path tile below zone
                },
                ZoomTarget = new Vector2(8 * T, 12 * T),
                ZoomLevel = 2.5f
            },
        };
    }
}