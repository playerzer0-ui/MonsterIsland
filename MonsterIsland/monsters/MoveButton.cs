using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NodeTesting.models;
using System;

namespace MonsterIsland.monsters
{
    public enum MoveCategory
    {
        Physical,
        Special,
        Status
    }

    public class MoveButton
    {
        private static Texture2D _roundedRect;
        private static SpriteSheet _categoryIconSheet;
        private static bool _initialized = false;

        private static readonly Color PhysicalColor = new Color(255, 163, 0);
        private static readonly Color SpecialColor = new Color(126, 37, 83);
        private static readonly Color StatusColor = new Color(171, 82, 54);

        private const int PhysicalFrame = 0;
        private const int SpecialFrame = 1;
        private const int StatusFrame = 2;

        private string _moveName;
        private MoveCategory _category;
        private Rectangle _bounds;
        private bool _isHovered;
        private bool _isSelected;

        public event Action OnClick;

        public static void Initialize()
        {
            if (_initialized) return;
            _roundedRect = Globals.Content.Load<Texture2D>("rounded-rectangle");
            _categoryIconSheet = new SpriteSheet("moves/movetype", 3);
            _initialized = true;
        }

        public MoveButton(string moveName, MoveCategory category, Rectangle bounds)
        {
            _moveName = moveName;
            _category = category;
            _bounds = bounds;
        }

        public void Update(Vector2 mousePosition, bool clicked)
        {
            _isHovered = _bounds.Contains(mousePosition);

            if (_isHovered && clicked)
            {
                OnClick?.Invoke();
            }
        }

        public void Draw(SpriteFont font)
        {
            Color bgColor = _category switch
            {
                MoveCategory.Physical => PhysicalColor,
                MoveCategory.Special => SpecialColor,
                MoveCategory.Status => StatusColor,
                _ => Color.Gray
            };

            if (_isSelected)
                bgColor = Color.Lerp(bgColor, Color.White, 0.3f);
            else if (_isHovered)
                bgColor = Color.Lerp(bgColor, Color.Black, 0.2f);

            // Draw rounded rectangle background
            Globals.spriteBatch.Draw(_roundedRect, _bounds, bgColor);

            // Draw border if selected
            if (_isSelected)
            {
                Rectangle borderRect = new Rectangle(_bounds.X - 2, _bounds.Y - 2, _bounds.Width + 4, _bounds.Height + 4);
                Globals.spriteBatch.Draw(_roundedRect, borderRect, Color.White * 0.8f);
            }

            // Draw move type icon
            const float iconSize = 16f;
            float iconX = _bounds.X + 8;
            float iconY = _bounds.Y + (_bounds.Height / 2f) - (iconSize / 2f);

            int frameIndex = _category switch
            {
                MoveCategory.Physical => PhysicalFrame,
                MoveCategory.Special => SpecialFrame,
                MoveCategory.Status => StatusFrame,
                _ => 0
            };

            _categoryIconSheet.DrawFrame(frameIndex, new Vector2(iconX + iconSize / 2f, iconY + iconSize / 2f));

            // Draw move name
            Vector2 textSize = font.MeasureString(_moveName);
            float textX = iconX + iconSize + 8;
            float textY = _bounds.Y + (_bounds.Height / 2f) - (textSize.Y / 2f);

            Globals.spriteBatch.DrawString(font, _moveName, new Vector2(textX, textY), Color.White);
        }

        public void SetSelected(bool selected) => _isSelected = selected;
        public Rectangle Bounds => _bounds;
    }
}