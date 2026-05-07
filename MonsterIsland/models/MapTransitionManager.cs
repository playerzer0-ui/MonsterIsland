using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace NodeTesting.models
{
    public class MapTransitionManager
    {
        /// <summary>
        /// Fired after a transition completes.
        /// Passes the new PathMap and the background sprite path so Game1 can
        /// swap the background and register the reverse connection.
        /// </summary>
        public event Action<PathMap, string> OnMapChanged;

        private PathMap _currentMap;
        private readonly List<MapConnection> _connections = new();

        private readonly string _tilesetTexture;
        private readonly int _tileW;
        private readonly int _tileH;

        public PathMap CurrentMap => _currentMap;

        public MapTransitionManager(PathMap initialMap, string tilesetTexture, int tileW, int tileH)
        {
            _currentMap = initialMap;
            _tilesetTexture = tilesetTexture;
            _tileW = tileW;
            _tileH = tileH;
        }

        public void AddConnection(MapConnection connection) => _connections.Add(connection);

        /// <summary>
        /// Call this instead of PathMap.TryMove. Handles both normal steps and
        /// edge-exit transitions transparently.
        /// </summary>
        public bool TryMove(Direction dir)
        {
            if (_currentMap.TryMove(dir))
                return true;

            Point grid = _currentMap.PlayerGridPosition;
            MapConnection exit = FindConnection(dir, grid);
            if (exit != null)
            {
                Transition(exit);
                return true;
            }

            return false;
        }

        // ------------------------------------------------------------------ //

        private MapConnection FindConnection(Direction dir, Point grid)
        {
            foreach (MapConnection c in _connections)
            {
                if (c.ExitDirection != dir) continue;

                bool onEdge = dir switch
                {
                    Direction.Up => grid.Y == 0 && c.ExitCoordinate == grid.X,
                    Direction.Down => grid.Y == _currentMap.Height - 1 && c.ExitCoordinate == grid.X,
                    Direction.Left => grid.X == 0 && c.ExitCoordinate == grid.Y,
                    Direction.Right => grid.X == _currentMap.Width - 1 && c.ExitCoordinate == grid.Y,
                    _ => false
                };

                if (onEdge) return c;
            }
            return null;
        }

        private void Transition(MapConnection exit)
        {
            PathMap newMap = new PathMap(
                exit.TargetMapTexture,
                _tileW, _tileH,
                exit.TargetMapCsv,
                exit.LandingTile.X,
                exit.LandingTile.Y
            );

            _connections.Clear();
            _currentMap = newMap;

            OnMapChanged?.Invoke(_currentMap, exit.BackgroundSprite);
        }
    }
}