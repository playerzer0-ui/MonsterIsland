using Microsoft.Xna.Framework;

namespace NodeTesting.models
{
    /// <summary>
    /// A clickable arrow drawn on a path tile when the player is near a zone.
    /// Extends SpriteSheet — the sheet is sliced into 4 equal frames automatically.
    /// Frame layout (left to right): Up=0, Right=1, Down=2, Left=3
    /// Arrows face OUTWARD from the zone to signal an available exit.
    /// </summary>
    public class ZoneArrow : SpriteSheet
    {
        public const int Up = 0;
        public const int Right = 1;
        public const int Down = 2;
        public const int Left = 3;

        private const int TileSize = 32;

        private readonly CollisionRect _hitRect;

        public Point DestinationTile { get; }

        public ZoneArrow(int arrowFrame, Point gridTile, Point destinationTile)
            : base("arrow-sheet", 4)
        {
            frameIndex = arrowFrame;
            DestinationTile = destinationTile;

            Position = new Vector2(
                gridTile.X * TileSize + TileSize / 2f,
                gridTile.Y * TileSize + TileSize / 2f
            );

            _hitRect = new CollisionRect(
                (int)Position.X,
                (int)Position.Y,
                TileSize - 4,
                TileSize - 4
            );
        }

        /// <summary>
        /// worldMouse must already be in world space (screen mouse → canvas scale → camera inverse).
        /// clicked should be true only on the first frame the mouse button is down.
        /// Returns true when hovered AND clicked.
        /// </summary>
        public bool Update(Vector2 worldMouse, bool clicked)
        {
            bool hovered = _hitRect.Contains(worldMouse.ToPoint());
            Color = hovered ? Color.Yellow : Color.White;
            return hovered && clicked;
        }

        public new void Draw()
        {
            base.Draw();
        }
    }
}