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
        public Texture2D? Texture;

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

        public Texture2D CreateCircleTexture(GraphicsDevice graphics, int radius, Color color)
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
            int numBalls = 8;

            Random random = new Random();

            for (int i = 0; i < numBalls; i++)
            {
                balls.Add(new Ball(
                    new Vector2(random.Next(100, 901), random.Next(100, 701)),
                    new Vector2(random.Next(-50, 51), random.Next(-50, 51)),
                    random.Next(1, 8)));
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            trailTexture = CreateCircleTexture(GraphicsDevice, (int)ballRadius / 4, Color.LightBlue);
            
            Color color = Color.Blue;
            foreach (var ball in balls)
            {
                color = new Color(MathHelper.Clamp(color.R + 20, 0, 255),color.G,color.B);
                ball.Texture = CreateCircleTexture(GraphicsDevice, (int)ball.Mass*10, color);
            }
        }

       protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float G = 500f;

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
                // Draw the trail
                foreach (var pos in ball.Trail)
                    spriteBatch.Draw(trailTexture, pos - new Vector2(ballRadius / 4), Color.White);

                // Draw the ball using its own texture
                if (ball.Texture != null)
                {
                    spriteBatch.Draw(
                        ball.Texture,
                        ball.Position - new Vector2(ball.Texture.Width / 2, ball.Texture.Height / 2),
                        Color.White
                    );
                }
                
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}