using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NodeTesting.models;

namespace MonsterIsland
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Canvas canvas;
        Camera camera;
        Sprite starter;
        Player player;
        TileMap path;
        PathMap pathMap;
        WorldMapManager worldManager;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferWidth = 960;
            _graphics.PreferredBackBufferHeight = 640;
            Window.AllowUserResizing = true;
            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            Globals.Content = Content;
            Globals.spriteBatch = _spriteBatch;
            Globals.graphics = _graphics;
            // TODO: use this.Content to load your game content here
            canvas = new Canvas(_graphics.GraphicsDevice, 960, 640);
            canvas.SetDestinationRectangle();

            pathMap = new PathMap("monster-island", 32, 32, "Maps/starter_path.csv", 8, 15);
            camera = new Camera();
            starter = new Sprite("Maps/starter", new Vector2(480, 320));
            player = new Player("characters/player", 2, pathMap);
            path = new TileMap("monster-island",32,32,"Maps/starter_path.csv");
            worldManager = new WorldMapManager(pathMap, camera);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            player.Update(gameTime);
            worldManager.Update(gameTime);
            // TODO: Add your update logic here
            canvas.SetResolution(_graphics.GraphicsDevice.Viewport.Width, _graphics.GraphicsDevice.Viewport.Height);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            canvas.Activate();
            // TODO: Add your drawing code here
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.Transform());
            starter.Draw(Color.White);
            path.Draw();
            player.Draw(0);
            player.DrawRect();
            _spriteBatch.End();

            canvas.Draw(_spriteBatch);

            base.Draw(gameTime);
        }
    }
}
