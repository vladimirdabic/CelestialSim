using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace CelestialSystem
{
    public class CelestialSystem
    {
        public readonly List<CelestialBody> Bodies;
        public float GravitationalConstant = 1f;
        public float TimeStep = 1f;
        public float FastForwardTime = 0f;

        public Method SimulationMethod = Method.Euler;

        public CelestialSystem(float timeStep)
        {
            Bodies = new List<CelestialBody>();
            TimeStep = timeStep;
        }

        public void Draw(Renderer r)
        {
            foreach (CelestialBody body in Bodies)
                body.Draw(r);
        }

        public void Update(float dt)
        {
            int steps = (int)((dt + FastForwardTime) / TimeStep);

            for (int i = 0; i < steps; ++i)
            {
                switch (SimulationMethod)
                {
                    case Method.Euler:
                        foreach (CelestialBody body in Bodies)
                            body.EulerUpdate(TimeStep);
                        break;

                    case Method.RK2:
                        foreach (CelestialBody body in Bodies)
                            body.RK2Midpoint(TimeStep);

                        foreach (CelestialBody body in Bodies)
                            body.RK2Update(TimeStep);
                        break;
                }
            }
        }

        public void ToBinary(BinaryWriter w)
        {
            // File Version
            w.Write((byte)1);

            // Settings
            w.Write(GravitationalConstant);
            w.Write(TimeStep);
            w.Write(0); // Step count, deprecated

            w.Write(FastForwardTime);
            w.Write((byte)SimulationMethod);

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

            byte fileVersion = r.ReadByte();

            GravitationalConstant = r.ReadSingle();
            TimeStep = r.ReadSingle();
            _ = r.ReadInt32(); // Step count, deprecated
            
            FastForwardTime = fileVersion > 0 ? r.ReadSingle() : 0f;
            SimulationMethod = fileVersion > 0 ? (Method)r.ReadByte() : Method.Euler;

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

                CelestialBody body = new CelestialBody(this, pX, pY, mass, new Vector2(vX, vY))
                {
                    DisplayColor = new SolidBrush(Color.FromArgb(cA, cR, cG, cB))
                };
            }
        }

        public enum Method
        {
            Euler, RK2
        }
    }
}
