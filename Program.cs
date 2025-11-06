using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FunPhysics
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch? _spriteBatch;
        private Texture2D? _ballTexture;

        private Vector2 _ball1Pos;
        private Vector2 _ball2Pos;
        private Vector2 _ball1Vel;
        private Vector2 _ball2Vel;

        private const float Gravity = 500f; // pixels/sec^2
        private const float BallRadius = 20f;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Initial positions
            _ball1Pos = new Vector2(200, 50);
            _ball2Pos = new Vector2(400, 100);

            _ball1Vel = Vector2.Zero;
            _ball2Vel = Vector2.Zero;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create a simple circle texture
            _ballTexture = new Texture2D(GraphicsDevice, (int)BallRadius * 2, (int)BallRadius * 2);
            Color[] data = new Color[(int)BallRadius * 2 * (int)BallRadius * 2];
            for (int y = 0; y < BallRadius * 2; y++)
            {
                for (int x = 0; x < BallRadius * 2; x++)
                {
                    int index = y * (int)BallRadius * 2 + x;
                    Vector2 pos = new Vector2(x - BallRadius, y - BallRadius);
                    data[index] = (pos.Length() <= BallRadius) ? Color.Red : Color.Transparent;
                }
            }
            _ballTexture.SetData(data);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update velocities with gravity
            _ball1Vel.Y += Gravity * dt;
            _ball2Vel.Y += Gravity * dt;

            // Update positions
            _ball1Pos += _ball1Vel * dt;
            _ball2Pos += _ball2Vel * dt;

            // Bounce on floor
            float floor = _graphics.PreferredBackBufferHeight - BallRadius;
            if (_ball1Pos.Y > floor)
            {
                _ball1Pos.Y = floor;
                _ball1Vel.Y *= -0.7f; // some energy loss
            }
            if (_ball2Pos.Y > floor)
            {
                _ball2Pos.Y = floor;
                _ball2Vel.Y *= -0.7f;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch!.Begin();
            _spriteBatch!.Draw(_ballTexture!, _ball1Pos - new Vector2(BallRadius, BallRadius), Color.White);
            _spriteBatch!.Draw(_ballTexture!, _ball2Pos - new Vector2(BallRadius, BallRadius), Color.White);
            _spriteBatch!.End();

            base.Draw(gameTime);
        }
    }
}
