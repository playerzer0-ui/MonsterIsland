using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NodeTesting.models
{
    public class SpriteSheet
    {
        protected Texture2D Texture;
        protected Vector2 Position = Vector2.Zero;
        protected Color Color = Color.White;
        protected Vector2 Origin;
        protected float Rotation = 0f;
        protected float Scale = 1f;
        protected SpriteEffects SpriteEffect;
        protected Rectangle[] Rectangles;
        protected int frames;
        protected int width;
        protected int frameIndex = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteSheet"/> class.
        /// </summary>
        /// <param name="Texture">The path to the texture in the Content folder.</param>
        /// <param name="frames">The total number of frames in the texture.</param>
        /// <remarks>
        /// This constructor automatically divides the texture width evenly based on the specified frame count.
        /// </remarks>
        public SpriteSheet(string Texture, int frames)
        {
            this.frames = frames;
            this.Texture = Globals.Content.Load<Texture2D>(Texture);
            width = this.Texture.Width / frames;
            Rectangles = new Rectangle[frames];

            for (int i = 0; i < frames; i++)
                Rectangles[i] = new Rectangle(i * width, 0, width, this.Texture.Height);

            Origin = new Vector2(width / 2, this.Texture.Height / 2);
        }

        /// <summary>
        /// Draws the current frame of the sprite to the screen.
        /// </summary>
        /// <param name="spriteBatch">The <see cref="SpriteSheet"/> used for rendering.</param>
        public void Draw()
        {
            Globals.spriteBatch.Draw(Texture, Position, Rectangles[frameIndex], Color, Rotation, Origin, Scale, SpriteEffect, 0f);
        }
    }
}
