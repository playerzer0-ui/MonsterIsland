using Microsoft.Xna.Framework;
using System.IO;

namespace MonsterIsland.monsters
{
    /// <summary>
    /// Loads an encounter CSV alongside a path map. Each cell matches the same
    /// grid position as the path — -1 means no encounter, any other value is
    /// the EncounterZone ID that applies to that tile.
    /// </summary>
    public class EncounterMap
    {
        private int[,] _grid;
        private int _rows;
        private int _cols;

        public EncounterMap(string csvPath)
        {
            Load(csvPath);
        }

        private void Load(string csvPath)
        {
            string fullPath = FindFile(csvPath);
            string[] lines = File.ReadAllLines(fullPath);

            _rows = lines.Length;
            _cols = lines[0].Split(',').Length;
            _grid = new int[_rows, _cols];

            for (int row = 0; row < _rows; row++)
            {
                string[] values = lines[row].Split(',');
                for (int col = 0; col < _cols; col++)
                {
                    if (col < values.Length && int.TryParse(values[col].Trim(), out int v))
                        _grid[row, col] = v;
                    else
                        _grid[row, col] = -1;
                }
            }
        }

        /// <summary>
        /// Returns the zone ID for a grid tile, or -1 if there is no encounter here.
        /// </summary>
        public int GetZoneId(Point gridTile)
        {
            if (gridTile.Y < 0 || gridTile.Y >= _rows) return -1;
            if (gridTile.X < 0 || gridTile.X >= _cols) return -1;
            return _grid[gridTile.Y, gridTile.X];
        }

        private static string FindFile(string path)
        {
            string[] candidates =
            {
                Path.Combine(Directory.GetCurrentDirectory(), path),
                Path.Combine(Directory.GetCurrentDirectory(), "Content", path),
                Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Content", path),
                path
            };
            foreach (string c in candidates)
                if (File.Exists(c)) return c;
            throw new FileNotFoundException($"Could not find encounter file '{path}'");
        }
    }
}
