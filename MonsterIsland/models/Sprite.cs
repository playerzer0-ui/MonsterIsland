using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NodeTesting.models
{
    public class Sprite
    {
        protected Texture2D texture;
        protected Vector2 pos;
        protected Vector2 origin;
        protected float rotation = 0f;

        public Vector2 Pos { get => pos; set => pos = value; }
        public Texture2D Texture { get => texture; set => texture = value; }

        public Sprite(string texture, Vector2 pos)
        {
            this.Texture = Globals.Content.Load<Texture2D>(texture);
            this.pos = pos;
            origin = new Vector2(this.Texture.Width / 2f, this.Texture.Height / 2f);
        }

        /// <summary>Draw with a uniform scale (default usage).</summary>
        public void Draw(Color color, float scale = 1f)
        {
            Globals.spriteBatch.Draw(texture, pos, null, color, rotation, origin, scale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draw with independent X/Y scale — useful for UI elements, not health bars
        /// (use Healthbar's Rectangle approach for bars instead).
        /// </summary>
        public void Draw(Color color, Vector2 scale)
        {
            Globals.spriteBatch.Draw(texture, pos, null, color, rotation, origin, scale, SpriteEffects.None, 0f);
        }
    }
}