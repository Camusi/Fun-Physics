using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Microsoft.Xna.Framework.Input;


namespace FunPhysics
{
    class Ball
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Mass;
        public List<Vector2> Trail = new List<Vector2>();
        public Texture2D? Texture;
        public double Radius;
        public bool Ghostly;

        public Ball(Vector2 pos, Vector2 vel, float mass, double radius, bool ghostly)
        {
            Position = pos;
            Velocity = vel;
            Mass = mass;
            Radius = radius;
            Ghostly = ghostly;
        }
    }


    class Simulation : Game
    {
        private GraphicsDeviceManager graphics;     // Creates the game window, controls resolution
        private SpriteBatch? spriteBatch;    // Used to draw textures/sprites
        
        private List<Ball> balls = new List<Ball>();
        private Texture2D? trailTexture;

        // Settings
        private bool wallsEnabled = false;
        private float elasticity = 0.98f;
        private float G = 100;
        private float panAmount = 10;
        private const int maxTrailLength = 50;
        private int numBalls = 30;


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

        private void MoveBalls(Vector2 direction, float panAmount)
        {
            foreach (Ball ball in balls)
            {
                ball.Position += panAmount * direction;
            }
        }

        private void MovePastPositions(Vector2 direction, float panAmount)
        {
            foreach (Ball ball in balls)
            {
                for (int i = 0; i < ball.Trail.Count; i++)
                {
                    ball.Trail[i] += panAmount * direction;
                }
            }
        }

        protected override void Initialize()
        {
            Random random = new Random();

            // balls.Add(new Ball(
            //         new Vector2(900, 500),
            //         new Vector2(0, 0),
            //         2000,
            //         2,
            //         true));

            // balls.Add(new Ball(
            //         new Vector2(900, 500),
            //         new Vector2(0, 0),
            //         1000,
            //         200,
            //         false));

            for (int i = 0; i < numBalls; i++)
            {
                float mass = random.Next(20, 100);
                balls.Add(new Ball(
                    new Vector2(random.Next(100, 1700), random.Next(100, 701)),
                    new Vector2(random.Next(-50, 51), random.Next(-50, 51)),
                    mass,
                    2 * Math.Sqrt(mass),
                    false));
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            trailTexture = CreateCircleTexture(GraphicsDevice, 3, Color.LightBlue);

            Color color = Color.Blue;
            foreach (var ball in balls)
            {
                color = new Color(MathHelper.Clamp(color.R + 20, 0, 255), color.G, color.B);
                ball.Texture = CreateCircleTexture(GraphicsDevice, (int)ball.Radius, color);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Compute gravitational forces
            for (int i = 0; i < balls.Count; i++)
            {
                Vector2 totalAccel = new Vector2(0, 0);
                Vector2 n = new Vector2(0, 0);

                for (int j = 0; j < balls.Count; j++)
                {
                    if (i != j)
                    {
                        Vector2 direction = balls[j].Position - balls[i].Position;
                        float r = direction.Length();

                        // Gravitational force formula: F = G*((m1*m2)/r^2)
                        // Gravitational acceleration formula: a = G*(m/r^2)
                        //
                        // G        =   Gravitational constant
                        // m        =   Mass
                        // r        =   Distance between center of ball and target
                        totalAccel += direction * G * balls[j].Mass / (r * r);
                    }

                    if (balls[i].Ghostly == false && balls[j].Ghostly == false)
                    {
                        // Ball collision
                        Vector2 separation_vec = balls[i].Position - balls[j].Position;
                        float separation = separation_vec.Length();
                        if (separation <= balls[i].Radius + balls[j].Radius && separation != 0 && i >= j)
                        {
                            n = separation_vec / separation;
                            Vector2 tangent_vec = new Vector2(-n.Y, n.X);

                            float v1Tangent = tangent_vec.Dot(balls[i].Velocity);
                            float v2Tangent = tangent_vec.Dot(balls[j].Velocity);

                            float v1NI = n.Dot(balls[i].Velocity);
                            float v2NI = n.Dot(balls[j].Velocity);

                            float v1NF = (v1NI * (balls[i].Mass - balls[j].Mass) + elasticity * 2 * balls[j].Mass * v2NI) / (balls[i].Mass + balls[j].Mass);
                            float v2NF = (v2NI * (balls[j].Mass - balls[i].Mass) + elasticity * 2 * balls[i].Mass * v1NI) / (balls[i].Mass + balls[j].Mass);

                            float extraBounce1 = 0;
                            float extraBounce2 = 0;
                            float overlap = (float)(balls[i].Radius + balls[j].Radius - separation);

                            balls[i].Velocity = v1Tangent * tangent_vec + (v1NF + extraBounce1) * n;
                            balls[j].Velocity = v2Tangent * tangent_vec + (v2NF + extraBounce2) * n;
                            balls[i].Position = balls[i].Position + n * overlap;
                        }

                        if (wallsEnabled)
                        {
                            if (balls[i].Position.X - balls[i].Radius < 0)
                            {
                                balls[i].Velocity.X *= -1;
                            }
                            if (balls[i].Position.X + balls[i].Radius > graphics.PreferredBackBufferWidth)
                            {
                                balls[i].Velocity.X *= -1;
                            }
                            if (balls[i].Position.Y - balls[i].Radius < 0)
                            {
                                balls[i].Velocity.Y *= -1;
                            }
                            if (balls[i].Position.Y + balls[i].Radius > graphics.PreferredBackBufferHeight)
                            {
                                balls[i].Velocity.Y *= -1;
                            }
                        }
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

                // Note: you could optimise the trails by calculating curves between significant points 
                //       (like on collision or when direction is > 90deg from another past point) and then sampling the functions at equal intervals.
                ball.Trail.Add(ball.Position);
                if (ball.Trail.Count > maxTrailLength)
                    ball.Trail.RemoveAt(0);
            }

            // Pan with wasd
            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.W))
            {
                MoveBalls(new Vector2(0, 1), panAmount);
                MovePastPositions(new Vector2(0, 1), panAmount);
            }
            if (state.IsKeyDown(Keys.A))
            {
                MoveBalls(new Vector2(1, 0), panAmount);
                MovePastPositions(new Vector2(1, 0), panAmount);
            }
            if (state.IsKeyDown(Keys.S))
            {
                MoveBalls(new Vector2(0, -1), panAmount);
                MovePastPositions(new Vector2(0, -1), panAmount);
            }
            if (state.IsKeyDown(Keys.D)){
                MoveBalls(new Vector2(-1, 0), panAmount);
                MovePastPositions(new Vector2(-1, 0), panAmount);
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
                    spriteBatch.Draw(trailTexture, pos - new Vector2(3), Color.White);

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

            // MouseState currentMouseState = Mouse.GetState();
            // if (currentMouseState.LeftButton == ButtonState.Pressed)
            // {
            //     int mouseX = currentMouseState.X;
            //     int mouseY = currentMouseState.Y;
            // }

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}