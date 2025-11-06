using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;


namespace FunPhysics
{
    class Simulation : Game {
        private GraphicsDeviceManager graphics;     // Creates the game window, controls resolution
        private SpriteBatch? spriteBatch;    // Used to draw textures/sprites

        private Vector2 ball1Pos;
        private Vector2 ball2Pos;
        private Vector2 ball1Vel;
        private Vector2 ball2Vel;
        private const float ballRadius = 40;

        public Simulation()
        {
            graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            ball1Pos = new Vector2(200, 50);
            ball2Pos = new Vector2(400, 100);

            ball1Vel = new Vector2(0, 0);
            ball2Vel = new Vector2(0, 0);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            CircleF circle = new CircleF(ball1Pos, ballRadius);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch!.Begin();

            spriteBatch.DrawCircle(new CircleF(ball1Pos, ballRadius), 2, Color.Red);
            spriteBatch.DrawCircle(new CircleF(ball2Pos, ballRadius), 2, Color.Blue);

            spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}