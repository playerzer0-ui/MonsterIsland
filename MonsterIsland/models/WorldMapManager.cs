using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace NodeTesting.models
{
    public enum WorldState { World, InZone }

    public class WorldMapManager
    {
        private PathMap _pathMap;
        private Camera _camera;
        private WorldState _state = WorldState.World;
        private ZoneDefinition _currentZone;
        private Point _entryTile;

        private float _targetZoom = 1f;
        private Vector2 _targetCamPos = Vector2.Zero;
        private const float ZoomSpeed = 3f;

        public WorldMapManager(MapTransitionManager transitionManager, Camera camera)
        {
            _camera = camera;
            _pathMap = transitionManager.CurrentMap;

            _targetCamPos = new Vector2(480, 320);
            _camera.Position = _targetCamPos;
            _camera.Zoom = 1f;

            // Keep _pathMap in sync whenever the active map changes
            transitionManager.OnMapChanged += (newMap, _) =>
            {
                _pathMap = newMap;

                // Reset zoom/camera so we don't carry over the previous map's zone state
                _state = WorldState.World;
                _currentZone = null;
                _targetZoom = 1f;
                _targetCamPos = new Vector2(480, 320);
            };
        }

        public void Update(GameTime gt)
        {
            float dt = (float)gt.ElapsedGameTime.TotalSeconds;

            _camera.Zoom = MathHelper.Lerp(_camera.Zoom, _targetZoom, ZoomSpeed * dt);
            _camera.Position = Vector2.Lerp(_camera.Position, _targetCamPos, ZoomSpeed * dt);

            if (_pathMap.IsMoving) return;

            Point grid = _pathMap.PlayerGridPosition;

            ZoneDefinition zone = ZoneRegistry.Zones
                .FirstOrDefault(z => z.EntryTiles.Contains(grid));

            if (zone != null)
            {
                _targetZoom = zone.ZoomLevel;
                _targetCamPos = zone.ZoomTarget;
            }
            else
            {
                _targetZoom = 1f;
                _targetCamPos = new Vector2(480, 320);
            }
        }
    }
}