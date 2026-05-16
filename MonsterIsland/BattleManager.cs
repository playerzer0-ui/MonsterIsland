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
    public enum BattleState { Inactive, SwipingIn, Battle, SwipingOut }

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
        private const float SlideDuration = 0.4f;      // How long each monster takes to slide
        private const float SlideStagger = 0.12f;      // Delay between each monster
        private const float OffscreenTop = -120f;
        private const float OffscreenBot = 720f;

        private Vector2[] _wildDrawPos;
        private Vector2[] _playerDrawPos;
        private float[] _wildTimer;
        private float[] _playerTimer;
        private bool _animationsStarted = false;

        // ── State exposed to Game1 ────────────────────────────────────────
        public bool IsBattleVisible => _state == BattleState.Battle;
        public bool IsSwipingIn => _state == BattleState.SwipingIn;
        public bool IsActive => _state != BattleState.Inactive;

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

            Console.WriteLine($"[BattleManager] Starting battle - Player count: {playerParty.Count}, Wild count: {wildMonsters.Count}");

            _playerMonsters = playerParty.Take(3).ToList();
            _wildMonsters = wildMonsters.Take(3).ToList();
            _bgIndex = MathHelper.Clamp(backgroundIndex, 0, _backgrounds.Length - 1);

            _animationsStarted = false;
            _state = BattleState.SwipingIn;

            _transition.OnCovered = BuildBattleScene;
            _transition.OnComplete = StartMonsterAnimations;  // KEY: Start animations AFTER screen is completely gone
            _transition.Start("Wild Encounter!");
        }

        private void BuildBattleScene()
        {
            Console.WriteLine("[BattleManager] Building battle scene (screen is black)");

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

            // Set initial offscreen positions (but don't start timers yet!)
            for (int i = 0; i < _wildMonsters.Count; i++)
            {
                _wildDrawPos[i] = new Vector2(_wildPositions[i].X, OffscreenTop);
                _wildTimer[i] = 0;  // Will be set when animations actually start
            }

            for (int i = 0; i < _playerMonsters.Count; i++)
            {
                _playerDrawPos[i] = new Vector2(_playerPositions[i].X, OffscreenBot);
                _playerTimer[i] = 0;  // Will be set when animations actually start
            }

            // Don't switch to Battle state yet - stay in SwipingIn until transition completes
            // _state will stay BattleState.SwipingIn until OnComplete fires
        }

        private void StartMonsterAnimations()
        {
            Console.WriteLine("[BattleManager] Screen fully uncovered! Starting monster slide-in animations NOW");

            // Now set the timers with stagger so they enter left-to-right
            for (int i = 0; i < _wildMonsters.Count; i++)
            {
                _wildTimer[i] = -(i * SlideStagger);  // Negative = delay before starting
                Console.WriteLine($"[BattleManager] Wild {i} will start in {Math.Abs(_wildTimer[i]):F2}s");
            }

            for (int i = 0; i < _playerMonsters.Count; i++)
            {
                _playerTimer[i] = -(i * SlideStagger);  // Negative = delay before starting
                Console.WriteLine($"[BattleManager] Player {i} will start in {Math.Abs(_playerTimer[i]):F2}s");
            }

            _animationsStarted = true;
            _state = BattleState.Battle;  // Now switch to Battle state so drawing happens
        }

        public void Update(GameTime gameTime)
        {
            if (_state == BattleState.Inactive) return;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Always tick the transition
            _transition.Update(gameTime);

            // Only animate if we're in Battle state AND animations have started
            if (_state == BattleState.Battle && _animationsStarted)
            {
                bool anyAnimating = false;

                // Update wild monster animations
                for (int i = 0; i < _wildMonsters.Count; i++)
                {
                    if (_wildTimer[i] < SlideDuration)  // Still animating or waiting to start
                    {
                        anyAnimating = true;
                        _wildTimer[i] += dt;

                        if (_wildTimer[i] >= 0)  // Only move once timer reaches 0 or positive
                        {
                            float t = Math.Min(_wildTimer[i] / SlideDuration, 1f);
                            float newY = MathHelper.Lerp(OffscreenTop, _wildPositions[i].Y, EaseOut(t));
                            _wildDrawPos[i] = new Vector2(_wildPositions[i].X, newY);

                            if (t < 1f && _wildTimer[i] < SlideDuration)
                                Console.WriteLine($"[BattleManager] Wild {i} sliding: Y={newY:F1}, progress={t:F2}");
                        }
                    }
                }

                // Update player monster animations
                for (int i = 0; i < _playerMonsters.Count; i++)
                {
                    if (_playerTimer[i] < SlideDuration)  // Still animating or waiting to start
                    {
                        anyAnimating = true;
                        _playerTimer[i] += dt;

                        if (_playerTimer[i] >= 0)  // Only move once timer reaches 0 or positive
                        {
                            float t = Math.Min(_playerTimer[i] / SlideDuration, 1f);
                            float newY = MathHelper.Lerp(OffscreenBot, _playerPositions[i].Y, EaseOut(t));
                            _playerDrawPos[i] = new Vector2(_playerPositions[i].X, newY);

                            if (t < 1f && _playerTimer[i] < SlideDuration)
                                Console.WriteLine($"[BattleManager] Player {i} sliding: Y={newY:F1}, progress={t:F2}");
                        }
                    }
                }

                if (!anyAnimating)
                {
                    Console.WriteLine("[BattleManager] All monsters have finished sliding in!");
                }

                // Quick exit with B button for testing
                KeyboardState keyboardState = Keyboard.GetState();
                if (keyboardState.IsKeyDown(Keys.B))
                {
                    Console.WriteLine("[BattleManager] Exiting battle with B button");
                    _state = BattleState.Inactive;
                    _animationsStarted = false;
                }
            }
        }

        public void Draw()
        {
            // Don't draw ANY battle content while swiping in/out - only after transition is complete
            if (_state != BattleState.Battle) return;
            if (!_animationsStarted) return;

            _backgrounds[_bgIndex].Draw(Color.White);
            _actionBar.Draw(Color.White);

            // Draw wild monsters
            for (int i = 0; i < _wildMonsters.Count; i++)
            {
                float cx = _wildDrawPos[i].X;
                float cy = _wildDrawPos[i].Y;

                _wildMonsters[i].Position = _wildDrawPos[i];
                _wildMonsters[i].Draw();

                // Only show UI if monster has finished animating (timer >= SlideDuration)
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

                // Only show UI if monster has finished animating (timer >= SlideDuration)
                if (_playerTimer[i] >= SlideDuration)
                {
                    _playerHealthbars[i].Update(_playerMonsters[i].Health, _playerMonsters[i].MaxHealth);
                    _playerHealthbars[i].SetPosition(new Vector2(cx, cy + 40));
                    _playerHealthbars[i].Draw();
                    DrawCentredString($"Lv:{_playerMonsters[i].Level}", new Vector2(cx, cy - 25), Color.Yellow, 0.5f);
                    DrawCentredString(_playerMonsters[i].Name, new Vector2(cx, cy - 10), Color.White, 0.5f);
                }
            }

            // Transition panel draws last
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