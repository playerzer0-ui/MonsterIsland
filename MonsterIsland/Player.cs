using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using NodeTesting.models;
using System;
namespace MonsterIsland
{
    public class Player : SpriteSheet
    {
        private const float InitialDelay = 0.2f;
        private const float RepeatDelay = 0.12f;
        private float _holdTimer = 0f;
        private bool _held = false;
        private PathMap _pathMap;
        private CollisionRect colRect;
        private KeyboardState old = Keyboard.GetState();
        public Player(string Texture, int frames) : base(Texture, frames)
        {
            _pathMap = new PathMap("monster-island", 32, 32, "Maps/starter_path.csv", 8, 14);
            colRect = new CollisionRect(0, 0, 10, 10);
        }
        public void Update(GameTime gt)
        {
            KeyboardState kState = Keyboard.GetState();
            float dt = (float)gt.ElapsedGameTime.TotalSeconds;
            _pathMap.Update(gt);
            Position = _pathMap.PlayerWorldPosition;
            if (!_pathMap.IsMoving)
            {
                Direction? dir = null;
                if (kState.IsKeyDown(Keys.W)) dir = Direction.Up;
                if (kState.IsKeyDown(Keys.S)) dir = Direction.Down;
                if (kState.IsKeyDown(Keys.A)) dir = Direction.Left;
                if (kState.IsKeyDown(Keys.D)) dir = Direction.Right;
                if (dir.HasValue)
                {
                    bool justPressed = (kState.IsKeyDown(Keys.W) && old.IsKeyUp(Keys.W))
                                    || (kState.IsKeyDown(Keys.S) && old.IsKeyUp(Keys.S))
                                    || (kState.IsKeyDown(Keys.A) && old.IsKeyUp(Keys.A))
                                    || (kState.IsKeyDown(Keys.D) && old.IsKeyUp(Keys.D));
                    _holdTimer += dt;
                    if (justPressed || _holdTimer >= (_held ? RepeatDelay : InitialDelay))
                    {
                        _pathMap.TryMove(dir.Value);
                        _holdTimer = 0f;
                        _held = !justPressed;
                    }
                }
                else { _holdTimer = 0f; _held = false; }
            }
            old = kState;
            colRect.UpdateRect((int)Math.Round(Position.X), (int)Math.Round(Position.Y));
        }
        public void DrawRect()
        {
            colRect.Draw(Color.Red);
        }
    }
}