using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CelestialSystem
{
    public class CelestialBody
    {
        [Category("Properties")]
        [Description("Mass of the body")]
        public float Mass
        { 
            get => _mass;

            set
            {
                _mass = value;
                DisplaySize = (float)Math.Max(Math.Log(_mass, 1.1), _minDrawingDiameter);
            }
        }

        [Browsable(false)]
        [Description("World position of the body")]
        public Vector2 Position { get => _states[0].Position; }
        
        [Browsable(false)]
        [Description("Current velocity of the body")]
        public Vector2 Velocity { get => _states[0].Velocity; }

        [Browsable(false)]
        public float DisplaySize { get; set; }

        [Browsable(false)]
        public SolidBrush DisplayColor { get; set; }

        [Browsable(false)]
        public Vector2 TotalForce { get; internal set; }


        [Category("Appearance")]
        [Description("Color of the body")]
        [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
        public Color Color
        {
            get { return DisplayColor.Color; }
            set { DisplayColor.Color = value; }
        }


        private readonly float _minDrawingDiameter = 5f;
        private float _mass;
        private CelestialSystem _parentSystem;
        private State[] _states = new State[2];

        public CelestialBody(CelestialSystem parent, float x, float y, float mass) : this(parent, x, y, mass, Vector2.Zero)
        {
        }

        public CelestialBody(CelestialSystem parent, float x, float y, float mass, Vector2 velocity)
        {
            _states[0] = new State()
            {
                Velocity = velocity,
                Position = new Vector2(x, y)
            };

            //Velocity = velocity;
            //Position = new Vector2(x, y);
            Mass = mass;
            DisplayColor = new SolidBrush(Color.White);
            _parentSystem = parent;
            parent.Bodies.Add(this);
        }

        /// <summary>
        /// Gets the current instantaneous change 
        /// </summary>
        /// <returns>Derivative</returns>
        public Derivative GetDerivative(int stateIdx)
        {
            Vector2 velocity = Vector2.Zero;
            Vector2 acceleration = Vector2.Zero;
            State thisState = _states[stateIdx];

            foreach (CelestialBody body in _parentSystem.Bodies)
            {
                // ignore self
                if (body == this) continue;
                State bodyState = body._states[stateIdx];

                Vector2 dr = bodyState.Position - thisState.Position;
                float d = dr.Length();

                if (d <= 0) continue;

                Vector2 grav_accel = _parentSystem.GravitationalConstant * body.Mass * dr / (d * d * d);
                acceleration += grav_accel;
            }

            velocity = thisState.Velocity;

            return new Derivative()
            {
                Velocity = velocity,
                Acceleration = acceleration
            };
        }

        public void EulerUpdate(float dt)
        {
            // Euler's method
            Derivative ds = GetDerivative(0);
            TotalForce = ds.Acceleration * Mass;

            _states[0].Velocity += ds.Acceleration * dt;
            _states[0].Position += ds.Velocity * dt;
        }

        public void RK2Midpoint(float dt)
        {
            Derivative ds = GetDerivative(0);
            _states[1].Velocity = _states[0].Velocity + ds.Acceleration * (dt / 2);
            _states[1].Position = _states[0].Position + ds.Velocity * (dt / 2);
        }

        public void RK2Update(float dt)
        {
            Derivative ds2 = GetDerivative(1);
            _states[0].Velocity += ds2.Acceleration * dt;
            _states[0].Position += ds2.Velocity * dt;
            TotalForce = ds2.Acceleration * Mass;
        }

        public void Draw(Renderer r)
        {
            float topLeftX = Position.X - DisplaySize / 2;
            float topLeftY = Position.Y - DisplaySize / 2;
            float size = DisplaySize;

            r.FillEllipse(DisplayColor, topLeftX, topLeftY, size, size);
        }

        public bool Collides(CelestialBody other)
        {
            Vector2 vec = other.Position - Position;
            return vec.Length() < (DisplaySize/2 - other.DisplaySize/2);
        }

        public bool Overlaps(int x, int y)
        {
            float topLeftX = Position.X - DisplaySize / 2;
            float topLeftY = Position.Y - DisplaySize / 2;

            return x >= topLeftX && x <= topLeftX + DisplaySize && y >= topLeftY && y <= topLeftY + DisplaySize;
        }

        public float DistanceTo(float x, float y)
        {
            return (float)Math.Sqrt(Math.Pow(x - Position.X, 2) + Math.Pow(y - Position.Y, 2));
        }

        public struct Derivative
        {
            public Vector2 Velocity;
            public Vector2 Acceleration;
        }

        private struct State
        {
            public Vector2 Position;
            public Vector2 Velocity;
        }
    }
}
