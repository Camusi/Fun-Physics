using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;


namespace FunPhysics
{
    public class LevelSelectScreen : Screen
    {
        private SpriteBatch spriteBatch;
        private GraphicsDevice graphicsDevice;
        private GraphicsDeviceManager graphicsDeviceManager;
        private BitmapFont? font;

        private Rectangle[]? levelButtons;
        private string[] levelNames = { "Level 1", "Level 2", "Level 3" };

        public LevelSelectScreen(GraphicsDeviceManager graphicsDeviceManagerIn, SpriteBatch spriteBatchIn, BitmapFont fontIn)
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

            levelButtons = new Rectangle[levelNames.Length];
            for (int i = 0; i < levelNames.Length; i++)
            {
                levelButtons[i] = new Rectangle(
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
                for (int i = 0; i < levelButtons!.Length; i++)
                {
                    if (levelButtons[i].Contains(mouse.Position))
                    {
                        var simulationScreen = new SimulationScreen(graphicsDeviceManager, spriteBatch);
                        ScreenManager.LoadScreen(simulationScreen, new FadeTransition(graphicsDevice, Color.Black));
                    }
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            graphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            for (int i = 0; i < levelButtons!.Length; i++)
            {
                spriteBatch.Draw(
                    TextureHelper.WhiteTexture(graphicsDevice),
                    levelButtons[i],
                    Color.DarkGray
                );

                Vector2 textSize = font!.MeasureString(levelNames[i]);
                Vector2 textPos = new Vector2(
                    levelButtons[i].X + (levelButtons[i].Width - textSize.X) / 2,
                    levelButtons[i].Y + (levelButtons[i].Height - textSize.Y) / 2
                );
                spriteBatch.DrawString(font, levelNames[i], textPos, Color.White);
            }

            spriteBatch.End();
        }
    }

    // Helper to create a 1x1 white texture for drawing rectangles
    public static class TextureHelper
    {
        private static Texture2D? whiteTexture;

        public static Texture2D WhiteTexture(GraphicsDevice device)
        {
            if (whiteTexture == null)
            {
                whiteTexture = new Texture2D(device, 1, 1);
                whiteTexture.SetData(new[] { Color.White });
            }
            return whiteTexture;
        }
    }
}
