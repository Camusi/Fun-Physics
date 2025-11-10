using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;


namespace FunPhysics
{
    public class LevelSelectScreen : Screen
    {
        private SpriteBatch spriteBatch;
        private GraphicsDevice graphicsDevice;
        private GraphicsDeviceManager graphicsDeviceManager;
        private SpriteFont? font;

        private Rectangle[]? _levelButtons;
        private string[] _levelNames = { "Level 1", "Level 2", "Level 3" };

        public LevelSelectScreen(GraphicsDeviceManager graphicsDeviceManagerIn, SpriteBatch spriteBatchIn, SpriteFont fontIn)
        {
            graphicsDeviceManager = graphicsDeviceManagerIn;
            graphicsDevice = graphicsDeviceManager.GraphicsDevice;
            font = fontIn;
            spriteBatch = spriteBatchIn;
        }

        public override void LoadContent()
        {
            int buttonWidth = 200;
            int buttonHeight = 60;
            int startY = 200;
            int spacing = 100;

            _levelButtons = new Rectangle[_levelNames.Length];
            for (int i = 0; i < _levelNames.Length; i++)
            {
                _levelButtons[i] = new Rectangle(
                    (graphicsDevice.Viewport.Width - buttonWidth) / 2,
                    startY + i * spacing,
                    buttonWidth,
                    buttonHeight
                );
            }
        }

        public override void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();

            if (mouse.LeftButton == ButtonState.Pressed)
            {
                for (int i = 0; i < _levelButtons.Length; i++)
                {
                    if (_levelButtons[i].Contains(mouse.Position))
                    {
                        // When a level is selected, load the SimulationScreen
                        var simulationScreen = new SimulationScreen(graphicsDeviceManager, spriteBatch);
                        ScreenManager.LoadScreen(simulationScreen, new FadeTransition(graphicsDevice, Color.Black));
                    }
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            graphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            for (int i = 0; i < _levelButtons.Length; i++)
            {
                // Draw button rectangle
                spriteBatch.Draw(
                    TextureHelper.WhiteTexture(graphicsDevice),
                    _levelButtons[i],
                    Color.DarkGray
                );

                // Draw button text centered
                Vector2 textSize = font.MeasureString(_levelNames[i]);
                Vector2 textPos = new Vector2(
                    _levelButtons[i].X + (_levelButtons[i].Width - textSize.X) / 2,
                    _levelButtons[i].Y + (_levelButtons[i].Height - textSize.Y) / 2
                );
                spriteBatch.DrawString(font, _levelNames[i], textPos, Color.White);
            }

            spriteBatch.End();
        }
    }

    // Helper to create a 1x1 white texture for drawing rectangles
    public static class TextureHelper
    {
        private static Texture2D _whiteTexture;

        public static Texture2D WhiteTexture(GraphicsDevice device)
        {
            if (_whiteTexture == null)
            {
                _whiteTexture = new Texture2D(device, 1, 1);
                _whiteTexture.SetData(new[] { Color.White });
            }
            return _whiteTexture;
        }
    }
}
