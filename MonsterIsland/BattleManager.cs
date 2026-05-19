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

        private const float SlideDuration = 0.4f;
        private const float SlideStagger = 0.12f;
        private const float OffscreenTop = -120f;
        private const float OffscreenBot = 720f;

        private Vector2[] _wildDrawPos;
        private Vector2[] _playerDrawPos;
        private float[] _wildTimer;
        private float[] _playerTimer;
        private bool _monstersSliding = false;
        private bool _battleSceneBuilt = false;

        // Text color configuration for different backgrounds
        private struct TextColors
        {
            public Color NameColor;
            public Color LevelColor;
            public Color HealthTextColor;
        }

        private TextColors _currentTextColors;

        public bool IsBattleVisible => _state == BattleState.Battle;
        public bool IsSwipingIn => _state == BattleState.SwipingIn;
        public bool IsActive => _state != BattleState.Inactive;
        public bool HasBattleScene => _battleSceneBuilt;

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

            // Set default text colors
            SetTextColorsForBackground(0);
        }

        private void SetTextColorsForBackground(int backgroundIndex)
        {
            switch (backgroundIndex)
            {
                case 0: // plains1 - bright grassy
                    _currentTextColors = new TextColors
                    {
                        NameColor = new Color(40, 40, 60),      // Dark blue-gray
                        LevelColor = new Color(80, 70, 40),     // Dark gold/brown
                        HealthTextColor = Color.Black
                    };
                    break;

                case 1: // plains2 - bright field
                    _currentTextColors = new TextColors
                    {
                        NameColor = new Color(30, 50, 30),      // Dark green
                        LevelColor = new Color(100, 80, 20),    // Dark gold
                        HealthTextColor = Color.Black
                    };
                    break;

                case 2: // beach - bright sand
                    _currentTextColors = new TextColors
                    {
                        NameColor = new Color(50, 40, 80),      // Dark purple
                        LevelColor = new Color(120, 80, 30),    // Dark orange
                        HealthTextColor = Color.Black
                    };
                    break;

                case 3: // forest - medium dark
                    _currentTextColors = new TextColors
                    {
                        NameColor = Color.White,
                        LevelColor = new Color(255, 220, 100),  // Light gold
                        HealthTextColor = Color.White
                    };
                    break;

                case 4: // sea - bright blue water
                    _currentTextColors = new TextColors
                    {
                        NameColor = new Color(20, 30, 50),      // Deep blue
                        LevelColor = new Color(60, 80, 100),    // Steel blue
                        HealthTextColor = Color.Black
                    };
                    break;

                case 5: // deepsea - dark blue
                    _currentTextColors = new TextColors
                    {
                        NameColor = Color.White,
                        LevelColor = new Color(200, 220, 255),  // Light blue-white
                        HealthTextColor = Color.White
                    };
                    break;

                case 6: // deepest - very dark
                    _currentTextColors = new TextColors
                    {
                        NameColor = Color.White,
                        LevelColor = new Color(255, 200, 150),  // Light orange
                        HealthTextColor = Color.White
                    };
                    break;

                case 7: // cave - dark
                    _currentTextColors = new TextColors
                    {
                        NameColor = Color.White,
                        LevelColor = new Color(255, 180, 100),  // Amber
                        HealthTextColor = Color.White
                    };
                    break;

                case 8: // stronghold - dark stone
                    _currentTextColors = new TextColors
                    {
                        NameColor = Color.White,
                        LevelColor = new Color(200, 180, 100),  // Pale gold
                        HealthTextColor = Color.White
                    };
                    break;

                default:
                    _currentTextColors = new TextColors
                    {
                        NameColor = Color.Black,
                        LevelColor = new Color(131, 118, 156), // Default purple-gray
                        HealthTextColor = Color.Black
                    };
                    break;
            }
        }

        public void StartBattle(List<Monster> playerParty, List<Monster> wildMonsters, int backgroundIndex = 0)
        {
            if (_state != BattleState.Inactive) return;

            Console.WriteLine($"[BattleManager] Starting battle");

            _playerMonsters = playerParty.Take(3).ToList();
            _wildMonsters = wildMonsters.Take(3).ToList();
            _bgIndex = MathHelper.Clamp(backgroundIndex, 0, _backgrounds.Length - 1);

            // Set colors based on the background
            SetTextColorsForBackground(_bgIndex);

            _monstersSliding = false;
            _battleSceneBuilt = false;

            _state = BattleState.SwipingIn;

            _transition.OnCovered = () => {
                Console.WriteLine("[BattleManager] Screen covered - building battle scene");
                BuildBattleScene();
            };
            _transition.OnComplete = () => {
                Console.WriteLine("[BattleManager] Transition complete - starting slide-in");
                _monstersSliding = true;
                _state = BattleState.Battle;
            };
            _transition.Start("Wild Encounter!");
        }

        private void BuildBattleScene()
        {
            _playerHealthbars = new List<Healthbar>();
            _wildHealthbars = new List<Healthbar>();

            for (int i = 0; i < _wildMonsters.Count; i++)
                _wildHealthbars.Add(new Healthbar(
                    _wildPositions[i] + new Vector2(0, 40), _wildMonsters[i].MaxHealth, 2f));

            for (int i = 0; i < _playerMonsters.Count; i++)
                _playerHealthbars.Add(new Healthbar(
                    _playerPositions[i] + new Vector2(0, 40), _playerMonsters[i].MaxHealth, 2f));

            _wildDrawPos = new Vector2[_wildMonsters.Count];
            _playerDrawPos = new Vector2[_playerMonsters.Count];
            _wildTimer = new float[_wildMonsters.Count];
            _playerTimer = new float[_playerMonsters.Count];

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
        }

        public void Update(GameTime gameTime)
        {
            if (_state == BattleState.Inactive) return;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _transition.Update(gameTime);

            if (_monstersSliding)
            {
                bool allDone = true;

                for (int i = 0; i < _wildMonsters.Count; i++)
                {
                    if (_wildTimer[i] < SlideDuration)
                    {
                        allDone = false;
                        _wildTimer[i] += dt;
                        if (_wildTimer[i] >= 0)
                        {
                            float t = Math.Min(_wildTimer[i] / SlideDuration, 1f);
                            _wildDrawPos[i] = new Vector2(_wildPositions[i].X,
                                MathHelper.Lerp(OffscreenTop, _wildPositions[i].Y, EaseOut(t)));
                        }
                    }
                }

                for (int i = 0; i < _playerMonsters.Count; i++)
                {
                    if (_playerTimer[i] < SlideDuration)
                    {
                        allDone = false;
                        _playerTimer[i] += dt;
                        if (_playerTimer[i] >= 0)
                        {
                            float t = Math.Min(_playerTimer[i] / SlideDuration, 1f);
                            _playerDrawPos[i] = new Vector2(_playerPositions[i].X,
                                MathHelper.Lerp(OffscreenBot, _playerPositions[i].Y, EaseOut(t)));
                        }
                    }
                }

                if (allDone)
                    Console.WriteLine("[BattleManager] All monsters finished sliding in");

                if (Keyboard.GetState().IsKeyDown(Keys.B))
                {
                    _state = BattleState.Inactive;
                    _monstersSliding = false;
                    _battleSceneBuilt = false;
                }
            }
        }

        public void DrawBattleScene()
        {
            if (!_battleSceneBuilt) return;

            _backgrounds[_bgIndex].Draw(Color.White);
            _actionBar.Draw(Color.White);

            // Wild monsters
            for (int i = 0; i < _wildMonsters.Count; i++)
            {
                float cx = _wildDrawPos[i].X;
                float cy = _wildDrawPos[i].Y;

                _wildMonsters[i].Position = _wildDrawPos[i];
                _wildMonsters[i].Draw();

                if (_wildTimer[i] >= SlideDuration)
                {
                    // Name centred above sprite
                    DrawCentredString(_wildMonsters[i].Name, new Vector2(cx, cy - 50), _currentTextColors.NameColor, 0.5f);

                    // Type symbol(s) + Lv on the same row
                    DrawTypeAndLevel(_wildMonsters[i], new Vector2(cx, cy - 35), _currentTextColors.LevelColor);

                    _wildHealthbars[i].Update(_wildMonsters[i].Health, _wildMonsters[i].MaxHealth);
                    _wildHealthbars[i].SetPosition(new Vector2(cx, cy + 40));
                    _wildHealthbars[i].Draw();
                }
            }

            // Player monsters
            for (int i = 0; i < _playerMonsters.Count; i++)
            {
                float cx = _playerDrawPos[i].X;
                float cy = _playerDrawPos[i].Y;

                _playerMonsters[i].Position = _playerDrawPos[i];
                _playerMonsters[i].Draw();

                if (_playerTimer[i] >= SlideDuration)
                {
                    _playerHealthbars[i].Update(_playerMonsters[i].Health, _playerMonsters[i].MaxHealth);
                    _playerHealthbars[i].SetPosition(new Vector2(cx, cy + 40));
                    _playerHealthbars[i].Draw();

                    // Type symbol(s) + Lv on the same row
                    DrawTypeAndLevel(_playerMonsters[i], new Vector2(cx, cy - 25), _currentTextColors.LevelColor);

                    // Name below
                    DrawCentredString(_playerMonsters[i].Name, new Vector2(cx, cy - 10), _currentTextColors.NameColor, 0.5f);
                }
            }
        }

        /// <summary>
        /// Draws the type symbol(s) and level text centred on the given position,
        /// laid out as:  [symbol1] [symbol2?]  Lv:N
        /// Everything is treated as one group and centred together.
        /// </summary>
        private void DrawTypeAndLevel(Monster monster, Vector2 centre, Color levelColor)
        {
            const float symbolSize = 12f;   // assumed frame size of symbol-Sheet at scale 1 — adjust if needed
            const float gap = 2f;    // gap between symbols and between symbol and text
            const float textScale = 0.5f;

            string lvText = $"Lv:{monster.Level}";
            Vector2 textSize = _font.MeasureString(lvText) * textScale;

            bool dualType = monster.SecondaryType.HasValue;
            int symbolCount = dualType ? 2 : 1;

            // Total width: symbols + gaps + text
            float totalWidth = symbolCount * symbolSize
                             + (symbolCount - 1) * gap   // gap between symbols
                             + gap                       // gap between last symbol and text
                             + textSize.X;

            float startX = centre.X - totalWidth / 2f;
            float midY = centre.Y;

            // Draw primary type symbol
            TypeSymbol.Draw(monster.PrimaryType, new Vector2(startX + symbolSize / 2f, midY));
            float cursorX = startX + symbolSize + gap;

            // Draw secondary type symbol if present
            if (dualType)
            {
                TypeSymbol.Draw(monster.SecondaryType.Value, new Vector2(cursorX + symbolSize / 2f, midY));
                cursorX += symbolSize + gap;
            }

            // Draw Lv text with background-appropriate color
            Globals.spriteBatch.DrawString(_font, lvText,
                new Vector2(cursorX, midY - textSize.Y / 2f),
                levelColor, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Optionally, you can add outline to text for better visibility on all backgrounds.
        /// </summary>
        private void DrawCentredStringWithOutline(string text, Vector2 centre, Color color, Color outlineColor, float scale = 1f)
        {
            Vector2 size = _font.MeasureString(text) * scale;
            Vector2 topLeft = new Vector2(centre.X - size.X / 2f, centre.Y - size.Y / 2f);

            // Draw outline (8 directions)
            Vector2[] offsets = new Vector2[]
            {
                new Vector2(-1, -1), new Vector2(0, -1), new Vector2(1, -1),
                new Vector2(-1,  0),                      new Vector2(1,  0),
                new Vector2(-1,  1), new Vector2(0,  1), new Vector2(1,  1)
            };

            foreach (Vector2 offset in offsets)
            {
                Globals.spriteBatch.DrawString(_font, text, topLeft + offset * scale, outlineColor,
                    0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }

            // Draw main text
            Globals.spriteBatch.DrawString(_font, text, topLeft, color,
                0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

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