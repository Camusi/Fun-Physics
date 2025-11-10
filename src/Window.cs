using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Screens;
using MonoGame.Extended.BitmapFonts;

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
            BitmapFont defaultFont = BitmapFont.FromFile(GraphicsDevice, "default.fnt");

            var lvlScreen = new LevelSelectScreen(graphics, spriteBatch, defaultFont);
            screenManager!.LoadScreen(lvlScreen);
        }
    }
}
