using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CelestialSystem
{
    public class CelestialSystem
    {
        public readonly List<CelestialBody> Bodies;
        public float GravitationalConstant = 1f;
        public float TimeStep = 1f;
        public int CalculationCount = 1;

        public CelestialSystem(float timeStep)
        {
            Bodies = new List<CelestialBody>();
            TimeStep = timeStep;
        }

        public void Accelerate(CelestialBody target, CelestialBody source, float deltaTime)
        {
            Vector2 vecTargetToSource = source.Position - target.Position;
            float distanceSquared = vecTargetToSource.LengthSquared();

            if (distanceSquared == 0) return;

            float force = GravitationalConstant * (target.Mass * source.Mass) / distanceSquared;

            // Unit vector
            Vector2 forceDirection = vecTargetToSource / vecTargetToSource.Length();
            // Scale the unit vector
            Vector2 forceVector = forceDirection * force;

            // a = F/m
            // v += a * dt
            target.Velocity += (forceVector / target.Mass) * deltaTime;
            target.TotalForce += forceVector;

            // Not actually needed since the body comes later in the update loop...
            //source.Velocity -= (forceVector / source.Mass) * deltaTime;
        }

        public void Draw(Renderer r)
        {
            foreach (CelestialBody body in Bodies)
                body.Draw(r);
        }

        public void Update()
        {
            for (int i = 0; i < CalculationCount; ++i)
                Step();
        }

        public void Step()
        {
            CelestialBody[] bodies = Bodies.ToArray();

            // Accelerate each body with respect to others
            for(int i = 0; i < bodies.Length; i++)
            {
                CelestialBody target = bodies[i];
                target.TotalForce = Vector2.Zero;
                
                for(int j = 0; j < bodies.Length; j++)
                {
                    // ignore target body
                    if (j == i) continue;
                    CelestialBody source = bodies[j];

                    Accelerate(target, source, TimeStep);

                    if (target.Collides(source))
                    {
                        if(target.Mass > source.Mass)
                            Bodies.Remove(source);
                        else
                            Bodies.Remove(target);
                    }
                }
            }

            foreach(var body in Bodies)
                body.Move(TimeStep);
        }

        public void ToBinary(BinaryWriter w)
        {
            // File Version
            w.Write((byte)0);

            // Settings
            w.Write(GravitationalConstant);
            w.Write(TimeStep);
            w.Write(CalculationCount);

            // Bodies
            w.Write(Bodies.Count);
            foreach(var body in Bodies)
            {
                w.Write(body.Mass);

                w.Write(body.Position.X);
                w.Write(body.Position.Y);

                w.Write(body.Velocity.X);
                w.Write(body.Velocity.Y);

                w.Write(body.DisplayColor.Color.A);
                w.Write(body.DisplayColor.Color.R);
                w.Write(body.DisplayColor.Color.G);
                w.Write(body.DisplayColor.Color.B);

            }
        }

        public void FromBinary(BinaryReader r)
        {
            Bodies.Clear();

            // Useless for now
            byte fileVersion = r.ReadByte();

            GravitationalConstant = r.ReadSingle();
            TimeStep = r.ReadSingle();
            CalculationCount = r.ReadInt32();

            int bodies = r.ReadInt32();
            for(int i = 0; i < bodies; i++)
            {
                float mass = r.ReadSingle();

                float pX = r.ReadSingle();
                float pY = r.ReadSingle();
                
                float vX = r.ReadSingle();
                float vY = r.ReadSingle();

                byte cA = r.ReadByte();
                byte cR = r.ReadByte();
                byte cG = r.ReadByte();
                byte cB = r.ReadByte();

                CelestialBody body = new CelestialBody(pX, pY, mass, new Vector2(vX, vY))
                {
                    DisplayColor = new SolidBrush(Color.FromArgb(cA, cR, cG, cB))
                };

                Bodies.Add(body);
            }
        }
    }
}
