using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Screens;

namespace FunPhysics
{
    public class Window : Game
    {
        private ScreenManager? screenManager;
        private GraphicsDeviceManager graphics;

        public Window()
        {
            graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
            graphics.PreferredBackBufferWidth = 1800;
            graphics.PreferredBackBufferHeight = 900;
        }

        protected override void Initialize()
        {
            screenManager = new ScreenManager();
            Components.Add(screenManager);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
            SpriteFont defaultFont = Content.Load<SpriteFont>("default");

            // Start at the simulation screen
            var simulationScreen = new LevelSelectScreen(graphics, spriteBatch, defaultFont);
            screenManager!.LoadScreen(simulationScreen);
        }
    }
}
