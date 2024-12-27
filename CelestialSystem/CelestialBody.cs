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
        public Vector2 Position { get; set; }
        
        [Browsable(false)]
        [Description("Current velocity of the body")]
        public Vector2 Velocity { get; set; }

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

        public CelestialBody(float x, float y, float mass) : this(x, y, mass, Vector2.Zero)
        {
        }

        public CelestialBody(float x, float y, float mass, Vector2 velocity)
        {
            Velocity = velocity;
            Position = new Vector2(x, y);
            Mass = mass;
            DisplayColor = new SolidBrush(Color.White);
        }

        public void Move(float deltaTime)
        {
            Position = new Vector2(Position.X + Velocity.X * deltaTime, Position.Y + Velocity.Y * deltaTime);
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
    }
}
