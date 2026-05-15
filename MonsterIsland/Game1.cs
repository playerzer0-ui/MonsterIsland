using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonsterIsland.monsters;
using NodeTesting.models;
using System;

namespace MonsterIsland
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Canvas canvas;
        Camera camera;
        Sprite currentBackground;
        Player player;
        PathMap pathMap;
        WorldMapManager worldManager;
        MapTransitionManager transitionManager;
        BattleManager battleManager;
        MonsterSpawner spawner;
        SpriteFont spriteFont;
        Healthbar healthbar;

        private const string PathTileset = "monster-island";
        private const int TileW = 32;
        private const int TileH = 32;

        private MouseState _prevMouse;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Mouse.SetCursor(MouseCursor.Crosshair);
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 960;
            _graphics.PreferredBackBufferHeight = 640;
            Window.AllowUserResizing = true;
            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Globals.Content = Content;
            Globals.spriteBatch = _spriteBatch;
            Globals.graphics = _graphics;

            canvas = new Canvas(_graphics.GraphicsDevice, 960, 640);
            canvas.SetDestinationRectangle();

            healthbar = new Healthbar(new Vector2(300, 300), 100);

            spriteFont = Content.Load<SpriteFont>("pico8");

            Monster.LoadContent("Content/moves.json", "Content/monsters.json");

            // --- Maps ---
            pathMap = new PathMap(PathTileset, TileW, TileH, "Maps/starter_path.csv", 8, 15);
            currentBackground = new Sprite("Maps/starter", new Vector2(480, 320));

            // --- Encounters ---
            // Each map gets its own EncounterMap loaded from its own CSV.
            spawner = new MonsterSpawner(new EncounterMap("Maps/starter_encounter.csv"));

            // When the player finishes walking to a tile, ask the spawner
            // whether a battle should start.
            HookSpawnerToMap(pathMap);

            // --- Transitions ---
            transitionManager = new MapTransitionManager(pathMap, PathTileset, TileW, TileH);

            transitionManager.AddConnection(new MapConnection
            {
                ExitDirection = Direction.Up,
                ExitCoordinate = 25,
                TargetMapCsv = "Maps/sea_path.csv",
                TargetMapTexture = PathTileset,
                LandingTile = new Point(25, 19),
                BackgroundSprite = "Maps/sea"
            });

            transitionManager.OnMapChanged += OnMapChanged;

            // --- Everything else ---
            camera = new Camera();
            player = new Player("characters/player", 2, transitionManager);
            worldManager = new WorldMapManager(transitionManager, camera);
            battleManager = new BattleManager();
        }

        /// <summary>
        /// Subscribes the spawner to a PathMap's OnTileLanded event.
        /// Called once for the initial map, then again every time we transition
        /// to a new map so the new map is also covered.
        /// </summary>
        private void HookSpawnerToMap(PathMap map)
        {
            map.OnTileLanded += tile =>
            {
                var wildMonsters = spawner.TrySpawnEncounter(tile);
                if (wildMonsters != null)
                {
                    Console.WriteLine("OH NO THERE IS A BATTLE");
                }
                    
            };
        }

        private void OnMapChanged(PathMap newMap, string backgroundSprite)
        {
            currentBackground = new Sprite(backgroundSprite, new Vector2(480, 320));

            bool arrivedOnSea = newMap.PlayerGridPosition == new Point(25, 19);

            // Swap the encounter map to match the new area
            spawner = new MonsterSpawner(new EncounterMap(
                arrivedOnSea ? "Maps/sea_encounter.csv" : "Maps/starter_encounter.csv"
            ));

            // Hook the spawner to the new PathMap
            HookSpawnerToMap(newMap);

            transitionManager.AddConnection(arrivedOnSea
                ? new MapConnection
                {
                    ExitDirection = Direction.Down,
                    ExitCoordinate = 25,
                    TargetMapCsv = "Maps/starter_path.csv",
                    TargetMapTexture = PathTileset,
                    LandingTile = new Point(25, 0),
                    BackgroundSprite = "Maps/starter"
                }
                : new MapConnection
                {
                    ExitDirection = Direction.Up,
                    ExitCoordinate = 25,
                    TargetMapCsv = "Maps/sea_path.csv",
                    TargetMapTexture = PathTileset,
                    LandingTile = new Point(25, 19),
                    BackgroundSprite = "Maps/sea"
                });
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MouseState mouse = Mouse.GetState();
            bool clicked = mouse.LeftButton == ButtonState.Pressed
                            && _prevMouse.LeftButton == ButtonState.Released;
            Vector2 worldMouse = canvas.ScreenToWorld(camera.Transform());

            player.Update(gameTime);
            worldManager.Update(gameTime, worldMouse, clicked);

            canvas.SetResolution(
                _graphics.GraphicsDevice.Viewport.Width,
                _graphics.GraphicsDevice.Viewport.Height);

            _prevMouse = mouse;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            canvas.Activate();
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp,
                               transformMatrix: camera.Transform());

            currentBackground.Draw(Color.White);
            player.Draw();
            worldManager.Draw();
            healthbar.Draw(4f);

            _spriteBatch.End();
            canvas.Draw(_spriteBatch);
            base.Draw(gameTime);
        }
    }
}