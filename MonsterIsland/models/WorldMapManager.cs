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

        public WorldMapManager(PathMap pathMap, Camera camera)
        {
            _pathMap = pathMap;
            _camera = camera;
            _targetCamPos = new Vector2(480, 320);
            _camera.Position = _targetCamPos;
            _camera.Zoom = 1f;
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

        private void EnterZone(ZoneDefinition zone, Point entryTile)
        {
            _currentZone = zone;
            _entryTile = entryTile;
            _state = WorldState.InZone;
            _targetZoom = zone.ZoomLevel;
            Console.WriteLine("ENTER");

            // Center the camera on the zone's pixel center, accounting for zoom and viewport
            Vector2 viewport = new Vector2(
                Globals.graphics.GraphicsDevice.Viewport.Width,
                Globals.graphics.GraphicsDevice.Viewport.Height
            );
            _targetCamPos = zone.ZoomTarget - (viewport / 2f) / _targetZoom;
        }

        private void ExitZone(ZoneExit exit)
        {
            _state = WorldState.World;
            _currentZone = null;
            Console.WriteLine("LEAVE");

            // Reset zoom and camera position back to neutral
            _targetZoom = 1f;
            _targetCamPos = new Vector2(480, 320);

            _pathMap.SetPlayerPosition(exit.WorldTile.X, exit.WorldTile.Y);
        }
    }
}