using System.Numerics;
using System.Runtime.InteropServices;

namespace FunPhysics
{
    class Simulation : Game {
        private GraphicsDeviceManager graphics;     // Creates the game window, controls resolution
        private SpriteBatch spriteBatch;    // Used to draw textures/sprites

        private Vector2 ball1Pos;
        private Vector2 ball2Pos;
        private Vector2 ball1Vel;
        private Vector2 ball2Vel;

        public static void Simulate()
        {
            graphics = new GraphicsDeviceManager(this);
            spriteBatch = new SpriteBatch(this);
        }

        protected override Initialize()
        {
            ball1Pos = new Vector2(200, 50);
            ball2Pos = new Vector2(400, 100);

            ball1Vel = new Vector2(0, 0);
            ball2Vel = new Vector2(0, 0);

            base.Initialize();
        }

        protected override LoadContent()
        {
            ballTexture = new Texture2D(GraphicsDevice, (int)BallRadius * 2, (int)BallRadius * 2);
        }
    }
}