using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace NodeTesting.models;

/// <summary>
/// Represents a rendering canvas that uses an internal <see cref="RenderTarget2D"/> 
/// to render the game scene at a fixed resolution and scale it to fit the window.
/// </summary>
public class Canvas
{
    private readonly RenderTarget2D _target;
    private readonly GraphicsDevice _graphicsDevice;
    private Rectangle _destinationRectangle;
    private float _scale;

    public Canvas(GraphicsDevice graphicsDevice, int width, int height)
    {
        _graphicsDevice = graphicsDevice;
        _target = new RenderTarget2D(_graphicsDevice, width, height);
    }

    public void SetDestinationRectangle()
    {
        var screenSize = _graphicsDevice.PresentationParameters.Bounds;

        float scaleX = (float)screenSize.Width  / _target.Width;
        float scaleY = (float)screenSize.Height / _target.Height;
        _scale = Math.Min(scaleX, scaleY);   // stored so mouse conversion can use it

        int newWidth  = (int)(_target.Width  * _scale);
        int newHeight = (int)(_target.Height * _scale);
        int posX = (screenSize.Width  - newWidth)  / 2;
        int posY = (screenSize.Height - newHeight) / 2;

        _destinationRectangle = new Rectangle(posX, posY, newWidth, newHeight);
    }

    /// <summary>
    /// Converts a raw screen-space mouse position into virtual canvas space
    /// (the fixed 960x640 coordinate system the game is drawn in).
    ///
    /// This accounts for both the letterbox offset (black bars) and the scale
    /// factor, so the result is always correct regardless of window size.
    /// </summary>
    public Vector2 ScreenToCanvas(Vector2 screenPos)
    {
        return new Vector2(
            (screenPos.X - _destinationRectangle.X) / _scale,
            (screenPos.Y - _destinationRectangle.Y) / _scale
        );
    }

    /// <summary>
    /// Converts a raw screen-space mouse position all the way into world space,
    /// applying both the canvas letterbox correction and the camera transform.
    ///
    /// Use this everywhere you need mouse-in-world coordinates. You will never
    /// have to think about scaling again.
    /// </summary>
    public Vector2 ScreenToWorld(Vector2 screenPos, Matrix cameraTransform)
    {
        Vector2 canvasPos = ScreenToCanvas(screenPos);
        return Vector2.Transform(canvasPos, Matrix.Invert(cameraTransform));
    }

    /// <summary>Convenience overload that reads the mouse position for you.</summary>
    public Vector2 ScreenToWorld(Matrix cameraTransform)
    {
        MouseState mouse = Mouse.GetState();
        return ScreenToWorld(new Vector2(mouse.X, mouse.Y), cameraTransform);
    }

    public void Activate()
    {
        _graphicsDevice.SetRenderTarget(_target);
        _graphicsDevice.Clear(Color.DarkGray);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _graphicsDevice.SetRenderTarget(null);
        _graphicsDevice.Clear(Color.Black);

        spriteBatch.Begin();
        spriteBatch.Draw(_target, _destinationRectangle, Color.White);
        spriteBatch.End();
    }

    public void SetResolution(int width, int height)
    {
        Globals.graphics.PreferredBackBufferWidth  = width;
        Globals.graphics.PreferredBackBufferHeight = height;
        Globals.graphics.ApplyChanges();
        SetDestinationRectangle();
    }
}