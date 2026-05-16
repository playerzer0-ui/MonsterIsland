using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NodeTesting.models;
using System;

namespace MonsterIsland
{
    public enum TransitionPhase { Idle, SwipeIn, Hold, SwipeOut }

    /// <summary>
    /// Black panel swipes in from the left, holds, then swipes out to the left.
    /// Fully reusable — call Start() again for every new encounter.
    /// </summary>
    public class ScreenTransition
    {
        private const int W = 960;
        private const int H = 640;
        private const float SwipeDuration = 0.3f;
        private const float HoldDuration = 0.6f;

        private readonly Texture2D _pixel;
        private readonly SpriteFont _font;

        private TransitionPhase _phase = TransitionPhase.Idle;
        private float _timer;
        private float _panelX;   // left edge of the black panel
        private string _label;

        /// <summary>True while panel is covering the screen (hold phase).</summary>
        public bool IsFullyCovered => _phase == TransitionPhase.Hold;

        /// <summary>True while any part of the transition is running.</summary>
        public bool IsRunning => _phase != TransitionPhase.Idle;

        /// <summary>True while the panel is still coming in (world should still render behind).</summary>
        public bool IsSwipingIn => _phase == TransitionPhase.SwipeIn;

        /// <summary>Fired once the panel fully covers the screen.</summary>
        public Action OnCovered;

        /// <summary>Fired once the panel has completely left the screen.</summary>
        public Action OnComplete;

        public ScreenTransition(SpriteFont font)
        {
            _font = font;
            _pixel = new Texture2D(Globals.graphics.GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Start(string label = "")
        {
            _label = label;
            _timer = 0f;
            _panelX = -W;           // start fully off the left
            _phase = TransitionPhase.SwipeIn;
        }

        public void Update(GameTime gt)
        {
            if (_phase == TransitionPhase.Idle) return;

            _timer += (float)gt.ElapsedGameTime.TotalSeconds;

            switch (_phase)
            {
                case TransitionPhase.SwipeIn:
                    _panelX = MathHelper.Lerp(-W, 0f, EaseOut(_timer / SwipeDuration));
                    if (_timer >= SwipeDuration)
                    {
                        _panelX = 0f;
                        _timer = 0f;
                        _phase = TransitionPhase.Hold;
                        OnCovered?.Invoke();   // safe to swap scene now
                    }
                    break;

                case TransitionPhase.Hold:
                    if (_timer >= HoldDuration)
                    {
                        _timer = 0f;
                        _phase = TransitionPhase.SwipeOut;
                    }
                    break;

                case TransitionPhase.SwipeOut:
                    _panelX = MathHelper.Lerp(0f, -W, EaseIn(_timer / SwipeDuration));
                    if (_timer >= SwipeDuration)
                    {
                        _panelX = -W;          // fully off left again
                        _phase = TransitionPhase.Idle;
                        OnComplete?.Invoke();
                    }
                    break;
            }
        }

        public void Draw()
        {
            if (_phase == TransitionPhase.Idle) return;

            Globals.spriteBatch.Draw(_pixel, new Rectangle((int)_panelX, 0, W, H), Color.Black);

            if (_phase == TransitionPhase.Hold && !string.IsNullOrEmpty(_label))
            {
                Vector2 size = _font.MeasureString(_label);
                Globals.spriteBatch.DrawString(_font, _label,
                    new Vector2(W / 2f - size.X / 2f, H / 2f - size.Y / 2f),
                    Color.White);
            }
        }

        private static float EaseOut(float t) => 1f - (1f - Math.Min(t, 1f)) * (1f - Math.Min(t, 1f));
        private static float EaseIn(float t) => Math.Min(t, 1f) * Math.Min(t, 1f);
    }
}