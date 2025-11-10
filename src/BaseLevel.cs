using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Screens;


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


    class SimulationScreen : Screen
    {
        private GraphicsDeviceManager? graphics;     // Creates the game window, controls resolution
        private SpriteBatch? spriteBatch;    // Used to draw textures/sprites
        
        private List<Ball> balls = new List<Ball>();
        private Texture2D? trailTexture;

        // Settings
        private bool wallsEnabled = true;
        private float elasticity = 0.7f;
        private float G = 100;
        private float panAmount = 10;
        private const int maxTrailLength = 50;
        private int numBalls = 10;
        private bool twoPlayer = true;
        private float playerAccel = 30;
        private float wallBounciness = 0.5f;

        // Hard level params = T, 0.7, 100, 10, 50, 10, T, 10, 0.5


        public SimulationScreen(GraphicsDeviceManager graphicsIn, SpriteBatch spriteBatchIn)
        {
            graphics = graphicsIn;
            spriteBatch = spriteBatchIn;
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

        public override void Initialize()
        {
            Random random = new Random();

            if (twoPlayer == true)
            {
                balls.Add(new Ball(
                    new Vector2(950, 500),
                    new Vector2(0, 0),
                    1000,
                    20,
                    false));
                balls.Add(new Ball(
                    new Vector2(850, 500),
                    new Vector2(0, 0),
                    1000,
                    20,
                    false));
            }

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

        public override void LoadContent()
        {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            trailTexture = CreateCircleTexture(graphics.GraphicsDevice, 3, Color.LightBlue);

            Color color = Color.Blue;

            if (twoPlayer)
            {
                foreach (var ball in balls.Skip(2).ToArray())
                {
                    color = new Color(MathHelper.Clamp(color.R + 20, 0, 255), color.G, color.B);
                    ball.Texture = CreateCircleTexture(graphics.GraphicsDevice, (int)ball.Radius, color);
                }
                balls[0].Texture = CreateCircleTexture(graphics.GraphicsDevice, (int)balls[0].Radius, Color.Green);
                balls[1].Texture = CreateCircleTexture(graphics.GraphicsDevice, (int)balls[1].Radius, Color.Red);
            }
            else
            {
                foreach (var ball in balls)
                {
                    color = new Color(MathHelper.Clamp(color.R + 20, 0, 255), color.G, color.B);
                    ball.Texture = CreateCircleTexture(graphics.GraphicsDevice, (int)ball.Radius, color);
                }
            }
            
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Compute gravitational forces
            for (int i = 0; i < balls.Count; i++)
            {
                Vector2 totalAccel = new Vector2(0, 0);
                Vector2 n = new Vector2(0, 0);

                if (wallsEnabled)
                {
                    if (balls[i].Position.X - balls[i].Radius < 0)
                    {
                        balls[i].Velocity.X *= -1 * wallBounciness;
                        balls[i].Position.X -= balls[i].Position.X - (float) balls[i].Radius;
                    }
                    if (balls[i].Position.X + balls[i].Radius > graphics.PreferredBackBufferWidth)
                    {
                        balls[i].Velocity.X *= -1 * wallBounciness;
                        balls[i].Position.X -= balls[i].Position.X + (float) balls[i].Radius - graphics.PreferredBackBufferWidth;
                    }
                    if (balls[i].Position.Y - balls[i].Radius < 0)
                    {
                        balls[i].Velocity.Y *= -1 * wallBounciness;
                        balls[i].Position.Y -= balls[i].Position.Y - (float) balls[i].Radius;
                    }
                    if (balls[i].Position.Y + balls[i].Radius > graphics.PreferredBackBufferHeight)
                    {
                        balls[i].Velocity.Y *= -1 * wallBounciness;
                        balls[i].Position.Y -= balls[i].Position.Y + (float) balls[i].Radius - graphics.PreferredBackBufferHeight;
                    }
                }
                        
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

                            
                            float overlap = (float)(balls[i].Radius + balls[j].Radius - separation);

                            balls[i].Velocity = v1Tangent * tangent_vec + v1NF * n;
                            balls[j].Velocity = v2Tangent * tangent_vec + v2NF * n;
                            balls[i].Position = balls[i].Position + n * overlap;
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

            if (twoPlayer == false)
            {
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
                if (state.IsKeyDown(Keys.D))
                {
                    MoveBalls(new Vector2(-1, 0), panAmount);
                    MovePastPositions(new Vector2(-1, 0), panAmount);
                }
            }
            else
            {
                if (state.IsKeyDown(Keys.W))
                {
                    balls[0].Velocity.Y -= playerAccel;
                }
                if (state.IsKeyDown(Keys.A))
                {
                    balls[0].Velocity.X -= playerAccel;
                }
                if (state.IsKeyDown(Keys.S))
                {
                    balls[0].Velocity.Y += playerAccel;
                }
                if (state.IsKeyDown(Keys.D))
                {
                    balls[0].Velocity.X += playerAccel;
                }

                if (state.IsKeyDown(Keys.Up))
                {
                    balls[1].Velocity.Y -= playerAccel;
                }
                if (state.IsKeyDown(Keys.Left))
                {
                    balls[1].Velocity.X -= playerAccel;
                }
                if (state.IsKeyDown(Keys.Down))
                {
                    balls[1].Velocity.Y += playerAccel;
                }
                if (state.IsKeyDown(Keys.Right))
                {
                    balls[1].Velocity.X += playerAccel;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);
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
        }
    }
}