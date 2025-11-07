using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
            Radius = mass * 2f;
        }
    }

    class Particle
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public float Lifetime;
        public float MaxLifetime;
        public Color Color;
        public float Size;

        public Particle(Vector3 pos, Vector3 vel, float lifetime, Color color, float size)
        {
            Position = pos;
            Velocity = vel;
            Lifetime = lifetime;
            MaxLifetime = lifetime;
            Color = color;
            Size = size;
        }
    }

    public class Simulation : Game
    {
        private GraphicsDeviceManager graphics;
        private BasicEffect effect;
        private List<Ball> balls = new List<Ball>();

        // Sphere mesh
        private VertexPositionNormalTexture[] sphereVertices;
        private int[] sphereIndices;
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private int sphereResolution = 32;

        // Camera
        private Vector3 cameraPosition = new Vector3(0, 200, 1000);
        private float yaw = 0f;
        private float pitch = 0f;
        private Matrix view;
        private Matrix projection;

        // Particles
        private List<Particle> particles = new List<Particle>();
        private VertexPositionColor[] particleVertexArray;
        private BasicEffect particleEffect;

        public Simulation()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1720,
                PreferredBackBufferHeight = 880
            };
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            base.Initialize();

            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45f),
                GraphicsDevice.Viewport.AspectRatio,
                1f,
                5000f
            );

            Random rnd = new Random();
            balls.Add(new Ball(Vector3.Zero, Vector3.Zero, 50));
            for (int i = 0; i < 8; i++)
            {
                balls.Add(new Ball(
                    new Vector3(rnd.Next(-200, 200), rnd.Next(-200, 200), rnd.Next(-200, 200)),
                    new Vector3(rnd.Next(-50, 51), rnd.Next(-50, 51), rnd.Next(-50, 51)),
                    rnd.Next(5, 30)
                ));
            }

            GenerateSphere(sphereResolution, sphereResolution);

            // Particle effect
            particleEffect = new BasicEffect(GraphicsDevice)
            {
                LightingEnabled = false,
                VertexColorEnabled = true,
                Projection = projection
            };

            Mouse.SetPosition(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);
        }

        private void GenerateSphere(int latSegments, int lonSegments)
        {
            var verts = new List<VertexPositionNormalTexture>();
            var indices = new List<int>();

            for (int lat = 0; lat <= latSegments; lat++)
            {
                float theta = lat * MathHelper.Pi / latSegments;
                float sinTheta = (float)Math.Sin(theta);
                float cosTheta = (float)Math.Cos(theta);

                for (int lon = 0; lon <= lonSegments; lon++)
                {
                    float phi = lon * 2f * MathHelper.Pi / lonSegments;
                    float sinPhi = (float)Math.Sin(phi);
                    float cosPhi = (float)Math.Cos(phi);

                    Vector3 normal = new Vector3(cosPhi * sinTheta, cosTheta, sinPhi * sinTheta);
                    verts.Add(new VertexPositionNormalTexture(normal, normal, Vector2.Zero));
                }
            }

            for (int lat = 0; lat < latSegments; lat++)
            {
                for (int lon = 0; lon < lonSegments; lon++)
                {
                    int first = lat * (lonSegments + 1) + lon;
                    int second = first + lonSegments + 1;

                    indices.Add(first);
                    indices.Add(second);
                    indices.Add(first + 1);

                    indices.Add(second);
                    indices.Add(second + 1);
                    indices.Add(first + 1);
                }
            }

            sphereVertices = verts.ToArray();
            sphereIndices = indices.ToArray();

            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture),
                                            sphereVertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(sphereVertices);

            indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits,
                                          sphereIndices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(sphereIndices);

            // Enhanced lighting
            effect = new BasicEffect(GraphicsDevice)
            {
                LightingEnabled = true,
                PreferPerPixelLighting = true,
                DiffuseColor = Color.White.ToVector3()
            };

            effect.DirectionalLight0.Enabled = true;
            effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-1, -1, -1));
            effect.DirectionalLight0.DiffuseColor = Vector3.One;

            effect.DirectionalLight1.Enabled = true;
            effect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(1, -1, -1));
            effect.DirectionalLight1.DiffuseColor = Vector3.One * 0.7f;

            effect.DirectionalLight2.Enabled = true;
            effect.DirectionalLight2.Direction = Vector3.Normalize(new Vector3(0, -1, 1));
            effect.DirectionalLight2.DiffuseColor = Vector3.One * 0.5f;
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float G = 5000f;

            // Gravity
            for (int i = 0; i < balls.Count; i++)
            {
                Vector3 accel = Vector3.Zero;
                for (int j = 0; j < balls.Count; j++)
                {
                    if (i == j) continue;
                    Vector3 dir = balls[j].Position - balls[i].Position;
                    float r = dir.Length();
                    if (r < 0.01f) continue;
                    dir.Normalize();
                    accel += dir * G * balls[j].Mass / (r * r);
                }
                balls[i].Velocity += accel * dt;
            }

            // Move balls
            foreach (var ball in balls) ball.Position += ball.Velocity * dt;

            // Collisions
            for (int i = 0; i < balls.Count; i++)
            {
                for (int j = i + 1; j < balls.Count; j++)
                {
                    Vector3 delta = balls[j].Position - balls[i].Position;
                    float dist = delta.Length();
                    float minDist = balls[i].Radius + balls[j].Radius;

                    if (dist < minDist && dist > 0.0001f)
                    {
                        Vector3 correction = delta * (0.5f * (minDist - dist) / dist);
                        balls[i].Position -= correction;
                        balls[j].Position += correction;

                        Vector3 normal = Vector3.Normalize(delta);
                        Vector3 relVel = balls[i].Velocity - balls[j].Velocity;
                        float vAlongNormal = Vector3.Dot(relVel, normal);
                        if (vAlongNormal > 0) continue;

                        float e = 1f;
                        float jImp = -(1 + e) * vAlongNormal / (1 / balls[i].Mass + 1 / balls[j].Mass);
                        Vector3 impulse = jImp * normal;
                        balls[i].Velocity += impulse / balls[i].Mass;
                        balls[j].Velocity -= impulse / balls[j].Mass;
                    }
                }
            }

            // Spawn particle trails, scaled by ball radius
            foreach (var ball in balls)
            {
                Vector3 randomVel = new Vector3(
                    (float)(Random.Shared.NextDouble() - 0.5) * 20f * ball.Radius / 10f,
                    (float)(Random.Shared.NextDouble() - 0.5) * 20f * ball.Radius / 10f,
                    (float)(Random.Shared.NextDouble() - 0.5) * 20f * ball.Radius / 10f
                );
                particles.Add(new Particle(ball.Position, randomVel, 1.0f, Color.Cyan, ball.Radius * 0.3f));
            }

            // Update particles
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                Particle p = particles[i];
                p.Lifetime -= dt;
                if (p.Lifetime <= 0) particles.RemoveAt(i);
                else p.Position += p.Velocity * dt;
            }

            // Mouse look
            MouseState mouse = Mouse.GetState();
            float deltaX = mouse.X - Window.ClientBounds.Width / 2;
            float deltaY = mouse.Y - Window.ClientBounds.Height / 2;
            float sensitivity = 0.002f;

            yaw -= deltaX * sensitivity;
            pitch = MathHelper.Clamp(pitch - deltaY * sensitivity, -MathHelper.PiOver2 + 0.1f, MathHelper.PiOver2 - 0.1f);

            Vector3 forward = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationX(pitch) * Matrix.CreateRotationY(yaw));
            Vector3 right = Vector3.Cross(forward, Vector3.Up);

            // WASD movement
            KeyboardState kb = Keyboard.GetState();
            float speed = 500f * dt;
            if (kb.IsKeyDown(Keys.W)) cameraPosition += forward * speed;
            if (kb.IsKeyDown(Keys.S)) cameraPosition -= forward * speed;
            if (kb.IsKeyDown(Keys.A)) cameraPosition -= right * speed;
            if (kb.IsKeyDown(Keys.D)) cameraPosition += right * speed;
            if (kb.IsKeyDown(Keys.Space)) cameraPosition += Vector3.Up * speed;
            if (kb.IsKeyDown(Keys.LeftControl)) cameraPosition -= Vector3.Up * speed;

            view = Matrix.CreateLookAt(cameraPosition, cameraPosition + forward, Vector3.Up);
            Mouse.SetPosition(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // Draw balls
            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            GraphicsDevice.Indices = indexBuffer;

            foreach (var ball in balls)
            {
                effect.World = Matrix.CreateScale(ball.Radius) * Matrix.CreateTranslation(ball.Position);
                effect.View = view;
                effect.Projection = projection;

                foreach (var pass in effect.CurrentTechnique.Passes)
                    pass.Apply();

                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                                     0, 0,
                                                     sphereVertices.Length,
                                                     0, sphereIndices.Length / 3);
            }

            // Draw particles using one array
            particleVertexArray = new VertexPositionColor[particles.Count];
            for (int i = 0; i < particles.Count; i++)
            {
                Particle p = particles[i];
                float alpha = p.Lifetime / p.MaxLifetime;
                particleVertexArray[i] = new VertexPositionColor(p.Position, p.Color * alpha);
            }

            if (particleVertexArray.Length > 0)
            {
                GraphicsDevice.SetVertexBuffer(null);
                foreach (var pass in particleEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.PointList, particleVertexArray, 0, particleVertexArray.Length);
                }
            }

            base.Draw(gameTime);
        }
    }
}
