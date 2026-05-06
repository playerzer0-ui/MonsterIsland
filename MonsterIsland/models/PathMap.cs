using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace NodeTesting.models
{
    public class PathMap : CollisionMap
    {
        private Point playerGridPos;
        private Vector2 playerWorldPos;
        private Vector2 targetWorldPos;
        private bool isMoving;
        private float moveSpeed = 128f; // pixels per second

        public Point PlayerGridPosition => playerGridPos;
        public Vector2 PlayerWorldPosition => playerWorldPos;
        public bool IsMoving => isMoving;

        public PathMap(string texturePath, int tileWidth, int tileHeight, string csvPath)
            : base(texturePath, tileWidth, tileHeight, csvPath)
        {
            // Find first walkable tile as start position
            for (int row = 0; row < MapHeight; row++)
            {
                for (int col = 0; col < MapWidth; col++)
                {
                    if (IsWalkable(col, row))
                    {
                        playerGridPos = new Point(col, row);
                        playerWorldPos = GetTileCenterWorld(col, row);
                        targetWorldPos = playerWorldPos;
                        return;
                    }
                }
            }
        }

        public PathMap(string texturePath, int tileWidth, int tileHeight, string csvPath, int startCol, int startRow)
        : base(texturePath, tileWidth, tileHeight, csvPath)
        {
            playerGridPos = new Point(startCol, startRow);
            playerWorldPos = GetTileCenterWorld(startCol, startRow);
            targetWorldPos = playerWorldPos;
        }

        /// <summary>
        /// Checks if a grid position is walkable (not -1 and not out of bounds)
        /// </summary>
        public bool IsWalkable(int gridX, int gridY)
        {
            if (gridX < 0 || gridX >= MapWidth || gridY < 0 || gridY >= MapHeight)
                return false;
            return MapData[gridY, gridX] != -1;
        }

        /// <summary>
        /// Gets the world position of a tile's center
        /// </summary>
        public Vector2 GetTileCenterWorld(int gridX, int gridY)
        {
            return new Vector2(
                gridX * TileWidth + TileWidth / 2,
                gridY * TileHeight + TileHeight / 2
            );
        }

        /// <summary>
        /// Converts world position to grid coordinates
        /// </summary>
        public Point WorldToGrid(Vector2 worldPos)
        {
            return new Point(
                (int)(worldPos.X / TileWidth),
                (int)(worldPos.Y / TileHeight)
            );
        }

        /// <summary>
        /// Attempts to move the player in a direction
        /// </summary>
        public bool TryMove(Direction dir)
        {
            if (isMoving) return false;

            Point newGridPos = playerGridPos;
            switch (dir)
            {
                case Direction.Up: newGridPos.Y--; break;
                case Direction.Down: newGridPos.Y++; break;
                case Direction.Left: newGridPos.X--; break;
                case Direction.Right: newGridPos.X++; break;
            }

            if (IsWalkable(newGridPos.X, newGridPos.Y))
            {
                playerGridPos = newGridPos;
                targetWorldPos = GetTileCenterWorld(playerGridPos.X, playerGridPos.Y);
                isMoving = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Updates player movement (call in Game.Update)
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (isMoving)
            {
                float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
                Vector2 moveDir = targetWorldPos - playerWorldPos;
                float dist = moveDir.Length();
                float step = moveSpeed * dt;
                if (dist <= step) { playerWorldPos = targetWorldPos; isMoving = false; }
                else playerWorldPos += Vector2.Normalize(moveDir) * step;

                if (Vector2.Distance(playerWorldPos, targetWorldPos) < 1f)
                {
                    playerWorldPos = targetWorldPos;
                    isMoving = false;
                }
            }
        }

        /// <summary>
        /// Draws the map and player
        /// </summary>
        public void DrawPlayer(Sprite playerSprite)
        {
            playerSprite.Pos = playerWorldPos;
            playerSprite.Draw(Color.White);
        }

        /// <summary>
        /// Debug: Draw walkable tiles highlight
        /// </summary>
        public void DrawWalkableDebug(Texture2D pixelTexture)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    if (IsWalkable(x, y))
                    {
                        Globals.spriteBatch.Draw(pixelTexture,
                            new Rectangle(x * TileWidth, y * TileHeight, TileWidth, TileHeight),
                            Color.Green * 0.3f);
                    }
                }
            }
        }
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
}
