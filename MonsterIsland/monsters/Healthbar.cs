using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NodeTesting.models;

namespace MonsterIsland.monsters
{
    public class Healthbar
    {
        private Sprite _border;
        private Texture2D _fillTexture;
        private Vector2 _position;
        private int _maxHealth;
        private int _currentHealth;

        // Add a scale field
        private float _scale;

        // The full pixel width of the fill area inside the border.
        // Set this to match the inner width of your healthbar-bg sprite.
        private int _baseFillWidth;
        private int _baseFillHeight;

        public Healthbar(Vector2 position, int maxHealth, float scale = 1f)
        {
            _position = position;
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
            _scale = scale;

            _border = new Sprite("healthbar-bg", position);

            // Load the fill texture directly so we can draw it as a Rectangle
            // and scale only the width independently of the height.
            _fillTexture = Globals.Content.Load<Texture2D>("healthbar");
            _baseFillWidth = _fillTexture.Width;
            _baseFillHeight = _fillTexture.Height;
        }

        public int GetBaseFillWidth() => _baseFillWidth;
        public int GetBaseFillHeight() => _baseFillHeight;

        public void Update(int currentHealth, int maxHealth)
        {
            _maxHealth = maxHealth;
            _currentHealth = MathHelper.Clamp(currentHealth, 0, maxHealth);
        }

        public void SetPosition(Vector2 position)
        {
            _position = position;
        }

        // Add a method to change scale if needed
        public void SetScale(float scale)
        {
            _scale = scale;
        }

        public void Draw()
        {
            Draw(1f); // Default scale of 1 for backward compatibility
        }

        public void Draw(float scaleOverride = 1f)
        {
            float healthPercent = (float)_currentHealth / _maxHealth;

            // Use the passed scale or the stored scale
            float finalScale = scaleOverride * _scale;

            // Calculate scaled dimensions
            int scaledFillWidth = (int)(_baseFillWidth * finalScale);
            int scaledFillHeight = (int)(_baseFillHeight * finalScale);

            // Draw the background/border first (full size, centred on _position)
            _border.Pos = _position;
            _border.Draw(Color.White, finalScale); // Apply scale to border

            // Calculate the fill rectangle using scaled dimensions
            int currentFillWidth = (int)(scaledFillWidth * healthPercent);
            int left = (int)(_position.X - scaledFillWidth / 2f);
            int top = (int)(_position.Y - scaledFillHeight / 2f);

            Rectangle destRect = new Rectangle(left, top, currentFillWidth, scaledFillHeight);

            // Calculate source rectangle to avoid stretching
            int sourceWidth = (int)(_baseFillWidth * healthPercent);
            Rectangle sourceRect = new Rectangle(0, 0, sourceWidth, _baseFillHeight);

            Color healthColor = healthPercent > 0.5f ? Color.LimeGreen :
                                healthPercent > 0.25f ? Color.Yellow : Color.Red;

            // Draw the fill with scaling
            Globals.spriteBatch.Draw(_fillTexture, destRect, sourceRect, healthColor);
        }
    }
}