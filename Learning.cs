using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace FunPhysics
{
    class Ball
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public float Mass;
        public float Radius;
        public Ball(Vector3 pos, Vector3 vel, float mass)
        {
            Position = pos;
            Velocity = vel;
            Mass = mass;
            Radius = mass; // scale radius with mass
        }
    }

    public class Simulation : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch? spriteBatch;

        private List<Ball> balls = new List<Ball>();
        private Model? sphereModel;

        // Camera
        private Vector3 cameraPosition = new Vector3(0, 200, 800);
        private Vector3 cameraTarget = new Vector3(0, 0, 0);
        private Matrix view;
        private Matrix projection;

        public Simulation()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Initialize camera
            view = Matrix.CreateLookAt(cameraPosition, cameraTarget, Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45f),
                GraphicsDevice.Viewport.AspectRatio,
                1f,
                5000f
            );

            // Create balls
            Random rnd = new Random();
            balls.Add(new Ball(new Vector3(0, 0, 0), Vector3.Zero, 50)); // heavy center ball
            for (int i = 0; i < 8; i++)
            {
                balls.Add(new Ball(
                    new Vector3(rnd.Next(-200, 200), rnd.Next(-200, 200), rnd.Next(-200, 200)),
                    new Vector3(rnd.Next(-50, 51), rnd.Next(-50, 51), rnd.Next(-50, 51)),
                    rnd.Next(5, 30)
                ));
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            // Load a basic sphere model
            // You can create a sphere model in Blender and export as .fbx
            sphereModel = Content.Load<Model>("sphere"); 
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float G = 500f;

            // Compute gravitational forces
            for (int i = 0; i < balls.Count; i++)
            {
                Vector3 totalAccel = Vector3.Zero;
                for (int j = 0; j < balls.Count; j++)
                {
                    if (i == j) continue;
                    Vector3 direction = balls[j].Position - balls[i].Position;
                    float r = direction.Length();
                    if (r < 0.01f) continue;
                    direction.Normalize();
                    totalAccel += direction * G * balls[j].Mass / (r * r);
                }
                balls[i].Velocity += totalAccel * dt;
            }

            // Update positions
            foreach (var ball in balls)
            {
                ball.Position += ball.Velocity * dt;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            foreach (var ball in balls)
            {
                Matrix world = Matrix.CreateScale(ball.Radius) * Matrix.CreateTranslation(ball.Position);

                foreach (ModelMesh mesh in sphereModel.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.World = world;
                        effect.View = view;
                        effect.Projection = projection;
                        effect.DiffuseColor = Color.Blue.ToVector3();
                    }
                    mesh.Draw();
                }
            }

            base.Draw(gameTime);
        }
    }
}
