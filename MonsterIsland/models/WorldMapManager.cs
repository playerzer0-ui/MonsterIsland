using Microsoft.Xna.Framework;
using System.Linq;

namespace NodeTesting.models
{
    public class WorldMapManager
    {
        private PathMap _pathMap;
        private Camera _camera;
        private ZoneArrowManager _arrowManager;

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

            _arrowManager = new ZoneArrowManager(transitionManager);

            transitionManager.OnMapChanged += (newMap, _) =>
            {
                _pathMap = newMap;
                _targetZoom = 1f;
                _targetCamPos = new Vector2(480, 320);
            };
        }

        /// <summary>
        /// worldMouse and clicked must be computed in Game1 where both the canvas
        /// scale and camera transform are known. See Game1.ScreenToWorld().
        /// </summary>
        public void Update(GameTime gt, Vector2 worldMouse, bool clicked)
        {
            float dt = (float)gt.ElapsedGameTime.TotalSeconds;

            _camera.Zoom = MathHelper.Lerp(_camera.Zoom, _targetZoom, ZoomSpeed * dt);
            _camera.Position = Vector2.Lerp(_camera.Position, _targetCamPos, ZoomSpeed * dt);

            if (!_pathMap.IsMoving)
            {
                Point grid = _pathMap.PlayerGridPosition;
                ZoneDefinition zone = ZoneRegistry.Zones
                    .FirstOrDefault(z => z.EntryTiles.Contains(grid));

                _targetZoom = zone != null ? zone.ZoomLevel : 1f;
                _targetCamPos = zone != null ? zone.ZoomTarget : new Vector2(480, 320);
            }

            _arrowManager.Update(worldMouse, clicked);
        }

        public void Draw()
        {
            _arrowManager.Draw();
        }
    }
}