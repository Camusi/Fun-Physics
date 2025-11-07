using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FunPhysics
{
    class Ball
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Mass;
        public List<Vector2> Trail = new List<Vector2>();

        public Ball(Vector2 pos, Vector2 vel, float mass)
        {
            Position = pos;
            Velocity = vel;
            Mass = mass;
        }
    }


    class Simulation : Game
    {
        private GraphicsDeviceManager graphics;     // Creates the game window, controls resolution
        private SpriteBatch? spriteBatch;    // Used to draw textures/sprites

        // Balls
        private List<Ball> balls = new List<Ball>();
        private const float ballRadius = 15;
        private Texture2D? ballTexture;

        // Trails
        private Texture2D? trailTexture;
        private const int maxTrailLength = 50;


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
            float offset = 400;

            balls.Add(new Ball(new Vector2(400 + offset, 300), new Vector2(0, 50), 1f));
            balls.Add(new Ball(new Vector2(600 + offset, 300), new Vector2(-50, 0), 1f));
            balls.Add(new Ball(new Vector2(500 + offset, 500), new Vector2(0, -50), 1f));
            balls.Add(new Ball(new Vector2(300 + offset, 400), new Vector2(50, -25), 1f));
            balls.Add(new Ball(new Vector2(700 + offset, 400), new Vector2(-50, 25), 1f));
            balls.Add(new Ball(new Vector2(450 + offset, 250), new Vector2(25, 50), 1f));
            balls.Add(new Ball(new Vector2(550 + offset, 350), new Vector2(-25, -50), 1f));
            balls.Add(new Ball(new Vector2(350 + offset, 500), new Vector2(50, 0), 1f));
            balls.Add(new Ball(new Vector2(650 + offset, 500), new Vector2(-50, -25), 1f));
            balls.Add(new Ball(new Vector2(500 + offset, 400), new Vector2(0, 0), 1f));


            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            trailTexture = CreateCircleTexture(GraphicsDevice, (int)ballRadius / 4, Color.LightBlue);
            ballTexture = CreateCircleTexture(GraphicsDevice, (int)ballRadius, Color.Blue);
        }

       protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float G = 2000f;

            // Compute gravitational forces
            for (int i = 0; i < balls.Count; i++)
            {
                Vector2 totalAccel = new Vector2(0, 0);

                for (int j = 0; j < balls.Count; j++)
                {
                    if (i != j)
                    {
                        Vector2 direction = balls[j].Position - balls[i].Position;
                        float r = direction.Length();

                        // Gravitational acceleration formula: F = G*(m/r^2)
                        //
                        // G        =   Gravitational constant
                        // m        =   Mass
                        // r        =   Distance between center of ball and target
                        totalAccel += direction * G * balls[j].Mass / (r * r);
                    }
                }

                // Velocity = acceleration * time
                balls[i].Velocity += totalAccel * dt;
            }

            // Update positions and trails
            foreach (var ball in balls)
            {
                // Distance = velocity * time
                ball.Position += ball.Velocity * dt;

                ball.Trail.Add(ball.Position);
                if (ball.Trail.Count > maxTrailLength)
                    ball.Trail.RemoveAt(0);
            }

            base.Update(gameTime);
        }



        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch!.Begin();

            foreach (var ball in balls)
            {
                foreach (var pos in ball.Trail)
                    spriteBatch.Draw(trailTexture, pos - new Vector2(ballRadius/4), Color.White);

                if (ballTexture != null)
                    spriteBatch.Draw(ballTexture, ball.Position - new Vector2(ballRadius), Color.White);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}