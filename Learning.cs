using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FunPhysics
{
    class Simulation : Game {
        private GraphicsDeviceManager graphics;     // Creates the game window, controls resolution
        private SpriteBatch? spriteBatch;    // Used to draw textures/sprites

        private Vector2 ball1Pos;
        private Vector2 ball2Pos;
        private Vector2 ball1Vel;
        private Vector2 ball2Vel;
        private const float ballRadius = 15;
        private Texture2D? ballTexture;

        // Trails
        private Texture2D? trailTexture;
        private List<Vector2> ball1Trail = new List<Vector2>();
        private List<Vector2> ball2Trail = new List<Vector2>();
        private const int maxTrailLength = 50; // how many points to keep


        public Simulation()
        {
            graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
            graphics.PreferredBackBufferWidth = 1800;
            graphics.PreferredBackBufferHeight = 900;
            graphics.ApplyChanges();
        }

        Texture2D CreateCircleTexture(GraphicsDevice graphics, int radius, Color color)
        {
            int diameter = radius * 2;
            Texture2D texture = new Texture2D(graphics, diameter, diameter);
            Color[] data = new Color[diameter * diameter];

            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    int index = y * diameter + x;
                    Vector2 pos = new Vector2(x - radius, y - radius);
                    data[index] = pos.Length() <= radius ? color : Color.Transparent;
                }
            }

            texture.SetData(data);
            return texture;
        }


        protected override void Initialize()
        {
            ball1Pos = new Vector2(graphics.PreferredBackBufferWidth/2 + 100, graphics.PreferredBackBufferHeight/2 + 100);
            ball2Pos = new Vector2(graphics.PreferredBackBufferWidth/2 - 100, graphics.PreferredBackBufferHeight/2 - 100);

            ball1Vel = new Vector2(0, 100);
            ball2Vel = new Vector2(100, 0);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            trailTexture = CreateCircleTexture(GraphicsDevice, (int)ballRadius/4, Color.White);
            ballTexture = CreateCircleTexture(GraphicsDevice, (int)ballRadius, Color.Blue);
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds; // time elapsed since last frame

            float G = 20000f; // tweak this for visible attraction
            float m1 = 1f;
            float m2 = 1f;

            Vector2 direction = ball2Pos - ball1Pos;
            float r = direction.Length();

            // Gravitational acceleration formula: F = G*(m/r^2)
            //
            // G        =   Gravitational constant
            // m        =   Mass
            // r        =   Distance between center of ball and target

            Vector2 a1 = direction * G * m2 / (r * r);
            Vector2 a2 = -direction * G * m1 / (r * r);

            // Velocity = acceleration * time
            ball1Vel += a1 * dt;
            ball2Vel += a2 * dt;

            // Distance = velocity * time
            ball1Pos += ball1Vel * dt;
            ball2Pos += ball2Vel * dt;


            // Add current positions to trail
            ball1Trail.Add(ball1Pos);
            ball2Trail.Add(ball2Pos);

            // Limit trail length
            if (ball1Trail.Count > maxTrailLength)
                ball1Trail.RemoveAt(0);
            if (ball2Trail.Count > maxTrailLength)
                ball2Trail.RemoveAt(0);


            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch!.Begin();

            foreach (var pos in ball1Trail)
            {
                spriteBatch.Draw(trailTexture, pos - new Vector2(ballRadius/4), Color.White);
            }

            foreach (var pos in ball2Trail)
            {
                spriteBatch.Draw(trailTexture, pos - new Vector2(ballRadius/4), Color.White);
            }

            if (ballTexture != null)
            {
                spriteBatch.Draw(ballTexture, ball1Pos - new Vector2(ballRadius), Color.White);

                spriteBatch.Draw(ballTexture, ball2Pos - new Vector2(ballRadius), Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}