using NodeTesting.models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using MonsterIsland.monsters;

namespace MonsterIsland
{
    public enum BattleState { Inactive, SwipingIn, Battle }

    public class BattleManager
    {
        private Sprite[] _backgrounds;
        private Sprite _actionBar;
        private SpriteFont _font;
        private ScreenTransition _transition;

        private BattleState _state = BattleState.Inactive;
        private List<Monster> _playerMonsters;
        private List<Monster> _wildMonsters;
        private List<Healthbar> _playerHealthbars;
        private List<Healthbar> _wildHealthbars;
        private int _bgIndex;

        private readonly Vector2[] _wildPositions = {
            new Vector2(320, 240),
            new Vector2(480, 240),
            new Vector2(640, 240)
        };
        private readonly Vector2[] _playerPositions = {
            new Vector2(320, 560),
            new Vector2(480, 560),
            new Vector2(640, 560)
        };

        // ── Slide-in animation ────────────────────────────────────────────
        private const float SlideDuration = 0.4f;
        private const float SlideStagger = 0.12f;
        private const float OffscreenTop = -120f;
        private const float OffscreenBot = 720f;

        private Vector2[] _wildDrawPos;
        private Vector2[] _playerDrawPos;
        private float[] _wildTimer;
        private float[] _playerTimer;
        private bool _monstersSliding = false;
        private bool _battleSceneBuilt = false;  // Track if battle scene is built

        // ── State exposed to Game1 ────────────────────────────────────────
        public bool IsBattleVisible => _state == BattleState.Battle;
        public bool IsSwipingIn => _state == BattleState.SwipingIn;
        public bool IsActive => _state != BattleState.Inactive;
        public bool HasBattleScene => _battleSceneBuilt;  // Does battle scene exist?

        public BattleManager(SpriteFont font)
        {
            _font = font;
            _transition = new ScreenTransition(font);

            _backgrounds = new Sprite[]
            {
                new Sprite("background/plains1",    new Vector2(480, 240)),
                new Sprite("background/plains2",    new Vector2(480, 240)),
                new Sprite("background/beach",      new Vector2(480, 240)),
                new Sprite("background/forest",     new Vector2(480, 240)),
                new Sprite("background/sea",        new Vector2(480, 240)),
                new Sprite("background/deepsea",    new Vector2(480, 240)),
                new Sprite("background/deepest",    new Vector2(480, 240)),
                new Sprite("background/cave",       new Vector2(480, 240)),
                new Sprite("background/stronghold", new Vector2(480, 240))
            };
            _actionBar = new Sprite("background/actionbar", new Vector2(480, 560));
        }

        public void StartBattle(List<Monster> playerParty, List<Monster> wildMonsters, int backgroundIndex = 0)
        {
            if (_state != BattleState.Inactive) return;

            Console.WriteLine($"[BattleManager] Starting battle");

            _playerMonsters = playerParty.Take(3).ToList();
            _wildMonsters = wildMonsters.Take(3).ToList();
            _bgIndex = MathHelper.Clamp(backgroundIndex, 0, _backgrounds.Length - 1);
            _monstersSliding = false;
            _battleSceneBuilt = false;

            _state = BattleState.SwipingIn;

            // Setup transition callbacks
            _transition.OnCovered = () => {
                Console.WriteLine("[BattleManager] Screen covered - building battle scene silently");
                BuildBattleScene();
            };
            _transition.OnComplete = () => {
                Console.WriteLine("[BattleManager] Transition complete! Starting monster slide-in");
                _monstersSliding = true;
                _state = BattleState.Battle;
            };
            _transition.Start("Wild Encounter!");
        }

        private void BuildBattleScene()
        {
            Console.WriteLine("[BattleManager] Building battle scene (hidden behind transition)");

            _playerHealthbars = new List<Healthbar>();
            _wildHealthbars = new List<Healthbar>();

            for (int i = 0; i < _wildMonsters.Count; i++)
            {
                _wildHealthbars.Add(new Healthbar(
                    _wildPositions[i] + new Vector2(0, 40), _wildMonsters[i].MaxHealth, 2f));
            }

            for (int i = 0; i < _playerMonsters.Count; i++)
            {
                _playerHealthbars.Add(new Healthbar(
                    _playerPositions[i] + new Vector2(0, 40), _playerMonsters[i].MaxHealth, 2f));
            }

            _wildDrawPos = new Vector2[_wildMonsters.Count];
            _playerDrawPos = new Vector2[_playerMonsters.Count];
            _wildTimer = new float[_wildMonsters.Count];
            _playerTimer = new float[_playerMonsters.Count];

            // Set initial offscreen positions
            for (int i = 0; i < _wildMonsters.Count; i++)
            {
                _wildDrawPos[i] = new Vector2(_wildPositions[i].X, OffscreenTop);
                _wildTimer[i] = -(i * SlideStagger);
            }

            for (int i = 0; i < _playerMonsters.Count; i++)
            {
                _playerDrawPos[i] = new Vector2(_playerPositions[i].X, OffscreenBot);
                _playerTimer[i] = -(i * SlideStagger);
            }

            _battleSceneBuilt = true;
            Console.WriteLine("[BattleManager] Battle scene built and ready behind transition");
        }

        public void Update(GameTime gameTime)
        {
            if (_state == BattleState.Inactive) return;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Always tick the transition
            _transition.Update(gameTime);

            // Update monster slide-in animations
            if (_monstersSliding)
            {
                bool allDone = true;

                // Update wild monsters
                for (int i = 0; i < _wildMonsters.Count; i++)
                {
                    if (_wildTimer[i] < SlideDuration)
                    {
                        allDone = false;
                        _wildTimer[i] += dt;

                        if (_wildTimer[i] >= 0)
                        {
                            float t = Math.Min(_wildTimer[i] / SlideDuration, 1f);
                            float newY = MathHelper.Lerp(OffscreenTop, _wildPositions[i].Y, EaseOut(t));
                            _wildDrawPos[i] = new Vector2(_wildPositions[i].X, newY);
                        }
                    }
                }

                // Update player monsters
                for (int i = 0; i < _playerMonsters.Count; i++)
                {
                    if (_playerTimer[i] < SlideDuration)
                    {
                        allDone = false;
                        _playerTimer[i] += dt;

                        if (_playerTimer[i] >= 0)
                        {
                            float t = Math.Min(_playerTimer[i] / SlideDuration, 1f);
                            float newY = MathHelper.Lerp(OffscreenBot, _playerPositions[i].Y, EaseOut(t));
                            _playerDrawPos[i] = new Vector2(_playerPositions[i].X, newY);
                        }
                    }
                }

                if (allDone)
                {
                    Console.WriteLine("[BattleManager] All monsters finished sliding!");
                }

                // Quick exit with B button for testing
                if (Keyboard.GetState().IsKeyDown(Keys.B))
                {
                    Console.WriteLine("[BattleManager] Exiting battle with B button");
                    _state = BattleState.Inactive;
                    _monstersSliding = false;
                    _battleSceneBuilt = false;
                }
            }
        }

        // Draw just the battle scene (no transition)
        public void DrawBattleScene()
        {
            if (!_battleSceneBuilt) return;

            _backgrounds[_bgIndex].Draw(Color.White);
            _actionBar.Draw(Color.White);

            // Draw wild monsters
            for (int i = 0; i < _wildMonsters.Count; i++)
            {
                float cx = _wildDrawPos[i].X;
                float cy = _wildDrawPos[i].Y;

                _wildMonsters[i].Position = _wildDrawPos[i];
                _wildMonsters[i].Draw();

                // Only show UI after monster finished animating
                if (_wildTimer[i] >= SlideDuration)
                {
                    DrawCentredString(_wildMonsters[i].Name, new Vector2(cx, cy - 50), Color.White, 0.5f);
                    DrawCentredString($"Lv:{_wildMonsters[i].Level}", new Vector2(cx, cy - 35), Color.Yellow, 0.5f);
                    _wildHealthbars[i].Update(_wildMonsters[i].Health, _wildMonsters[i].MaxHealth);
                    _wildHealthbars[i].SetPosition(new Vector2(cx, cy + 40));
                    _wildHealthbars[i].Draw();
                }
            }

            // Draw player monsters
            for (int i = 0; i < _playerMonsters.Count; i++)
            {
                float cx = _playerDrawPos[i].X;
                float cy = _playerDrawPos[i].Y;

                _playerMonsters[i].Position = _playerDrawPos[i];
                _playerMonsters[i].Draw();

                // Only show UI after monster finished animating
                if (_playerTimer[i] >= SlideDuration)
                {
                    _playerHealthbars[i].Update(_playerMonsters[i].Health, _playerMonsters[i].MaxHealth);
                    _playerHealthbars[i].SetPosition(new Vector2(cx, cy + 40));
                    _playerHealthbars[i].Draw();
                    DrawCentredString($"Lv:{_playerMonsters[i].Level}", new Vector2(cx, cy - 25), Color.Yellow, 0.5f);
                    DrawCentredString(_playerMonsters[i].Name, new Vector2(cx, cy - 10), Color.White, 0.5f);
                }
            }
        }

        // Draw just the transition overlay
        public void DrawTransition()
        {
            _transition.Draw();
        }

        private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);

        private void DrawCentredString(string text, Vector2 centre, Color color, float scale = 1f)
        {
            Vector2 size = _font.MeasureString(text) * scale;
            Vector2 topLeft = new Vector2(centre.X - size.X / 2f, centre.Y - size.Y / 2f);
            Globals.spriteBatch.DrawString(_font, text, topLeft, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }
}