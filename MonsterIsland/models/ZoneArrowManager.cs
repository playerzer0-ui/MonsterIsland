using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace NodeTesting.models
{
    /// <summary>
    /// Watches the player's grid position. When they stand on a zone entry tile,
    /// arrows appear on ALL other exits of that zone (never on the tile they're
    /// already standing on). Clicking an arrow teleports the player.
    /// </summary>
    public class ZoneArrowManager
    {
        private PathMap _pathMap;
        private ZoneDefinition _activeZone;
        private Point _playerEntryTile;   // which entry tile the player is currently on
        private readonly List<ZoneArrow> _arrows = new();
        private MouseState _prevMouse;

        public ZoneArrowManager(MapTransitionManager transitionManager)
        {
            _pathMap = transitionManager.CurrentMap;

            transitionManager.OnMapChanged += (newMap, _) =>
            {
                _pathMap = newMap;
                _activeZone = null;
                _arrows.Clear();
            };
        }

        /// <summary>
        /// worldMouse: mouse already converted to world space — must be computed
        /// in Game1 where both canvas scale and camera transform are known.
        /// </summary>
        public void Update(Vector2 worldMouse, bool clicked)
        {
            // Determine which zone (if any) the player is standing on
            ZoneDefinition zone = null;
            Point grid = _pathMap.PlayerGridPosition;

            if (!_pathMap.IsMoving)
            {
                foreach (ZoneDefinition z in ZoneRegistry.Zones)
                {
                    if (z.EntryTiles.Contains(grid)) { zone = z; break; }
                }
            }

            // Rebuild arrows whenever zone OR the specific entry tile changes
            if (zone != _activeZone || (zone != null && grid != _playerEntryTile))
            {
                _activeZone = zone;
                _playerEntryTile = grid;
                _arrows.Clear();

                if (zone != null)
                {
                    foreach (ArrowExit exit in zone.ArrowExits)
                    {
                        // Skip the arrow whose tile is the one the player is already on
                        if (exit.ArrowTile == grid) continue;

                        _arrows.Add(new ZoneArrow(
                            exit.ArrowFrame,
                            exit.ArrowTile,
                            exit.DestinationTile
                        ));
                    }
                }
            }

            // Update each arrow; teleport on the first click hit
            foreach (ZoneArrow arrow in _arrows)
            {
                if (arrow.Update(worldMouse, clicked))
                {
                    _pathMap.SetPlayerPosition(
                        arrow.DestinationTile.X,
                        arrow.DestinationTile.Y);
                    _arrows.Clear();
                    _activeZone = null;
                    break;
                }
            }
        }

        public void Draw()
        {
            foreach (ZoneArrow arrow in _arrows)
                arrow.Draw();
        }
    }
}