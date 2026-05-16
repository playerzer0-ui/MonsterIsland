using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonsterIsland.monsters;
using NodeTesting.models;
using System.Collections.Generic;

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

            spriteFont = Content.Load<SpriteFont>("pico8");

            Monster.LoadContent("Content/moves.json", "Content/monsters.json");

            pathMap = new PathMap(PathTileset, TileW, TileH, "Maps/starter_path.csv", 8, 15);
            currentBackground = new Sprite("Maps/starter", new Vector2(480, 320));

            spawner = new MonsterSpawner(new EncounterMap("Maps/starter_encounter.csv"));
            HookSpawnerToMap(pathMap);

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

            camera = new Camera();
            player = new Player("characters/player", 2, transitionManager);
            worldManager = new WorldMapManager(transitionManager, camera);
            battleManager = new BattleManager(spriteFont);
        }

        private void HookSpawnerToMap(PathMap map)
        {
            map.OnTileLanded += tile =>
            {
                var wild = spawner.TrySpawnEncounter(tile);
                if (wild != null)
                {
                    // Create a proper player party! You need to store your actual party somewhere
                    var playerParty = new List<Monster>();
                    for (int i = 0; i < 3; i++)
                    {
                        playerParty.Add(new Monster(1, 5)); // 3 Kindlecko level 5
                    }
                    battleManager.StartBattle(playerParty, wild);
                }
            };
        }

        private void OnMapChanged(PathMap newMap, string backgroundSprite)
        {
            currentBackground = new Sprite(backgroundSprite, new Vector2(480, 320));
            bool arrivedOnSea = newMap.PlayerGridPosition == new Point(25, 19);

            spawner = new MonsterSpawner(new EncounterMap(
                arrivedOnSea ? "Maps/sea_encounter.csv" : "Maps/starter_encounter.csv"));
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

            // Only update world when the battle scene itself isn't visible.
            // During SwipingIn the world still updates (player is frozen but world ticks).
            if (!battleManager.IsBattleVisible)
            {
                player.Update(gameTime);
                worldManager.Update(gameTime, worldMouse, clicked);
            }

            battleManager.Update(gameTime);

            canvas.SetResolution(
                _graphics.GraphicsDevice.Viewport.Width,
                _graphics.GraphicsDevice.Viewport.Height);

            _prevMouse = mouse;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            canvas.Activate();

            // ── World pass (camera transform) ─────────────────────────────
            // Always draw the world. During SwipingIn it shows behind the panel.
            // Once the panel covers the screen (Battle state), battle.Draw() takes over.
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp,
                               transformMatrix: camera.Transform());

            if (!battleManager.IsBattleVisible)
            {
                currentBackground.Draw(Color.White);
                player.Draw();
                worldManager.Draw();
            }

            _spriteBatch.End();

            // ── UI / overlay pass (no camera transform) ───────────────────
            // Battle scene and transition panel live here — they are always
            // in canvas space (0-960, 0-640) regardless of camera zoom or pan.
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            battleManager.Draw();   // draws battle scene + transition panel on top

            _spriteBatch.End();

            canvas.Draw(_spriteBatch);
            base.Draw(gameTime);
        }
    }
}