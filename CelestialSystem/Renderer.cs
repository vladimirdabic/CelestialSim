using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CelestialSystem
{
    public class Renderer
    {
        public int CameraX { get; set; }
        public int CameraY { get; set; }
        public int MouseX { get; private set; }
        public int MouseY { get; private set; }
        public float Zoom { get; set; }
        public Graphics Graphics { get; private set; }

        private PictureBox _pictureBox;
        private int _camOriginX, _camOriginY, _camStartX, _camStartY;
        private bool _camMoving = false;

        public event MouseEventHandler Clicked;
        public event EventHandler Paint;

        public Renderer(PictureBox pictureBox)
        {
            pictureBox.Resize += PictureBox_Resize;
            pictureBox.MouseWheel += PictureBox_MouseWheel;
            pictureBox.MouseDown += PictureBox_MouseDown;
            pictureBox.MouseUp += PictureBox_MouseUp;
            pictureBox.MouseMove += PictureBox_MouseMove;
            pictureBox.MouseEnter += PictureBox_MouseEnter;
            pictureBox.MouseClick += PictureBox_MouseClick;
            pictureBox.Paint += PictureBox_Paint;

            _pictureBox = pictureBox;
            CameraX = 0;
            CameraY = 0;
            Zoom = 1f;
        }

        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            Graphics = e.Graphics;
            Paint(this, null);
        }

        private void PictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            float eX = (e.X + CameraX) / Zoom;
            float eY = (e.Y + CameraY) / Zoom;

            var args = new MouseEventArgs(e.Button, e.Clicks, (int)eX, (int)eY, e.Delta);

            Clicked(sender, args);
        }

        private void PictureBox_MouseEnter(object sender, EventArgs e)
        {
            _pictureBox.Focus();
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if(_camMoving)
            {
                CameraX = _camStartX - (e.X - _camOriginX);
                CameraY = _camStartY - (e.Y - _camOriginY);
            }

            MouseX = (int)((e.X + CameraX) / Zoom);
            MouseY = (int)((e.Y + CameraY) / Zoom);
            //_pictureBox.Invalidate();
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            _camMoving = false;
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                _camMoving = true;
                _camStartX = CameraX;
                _camStartY = CameraY;
                
                _camOriginX = e.X;
                _camOriginY = e.Y;
            }
        }

        private void PictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
                Zoom /= 0.95f;
            else
                Zoom *= 0.95f;
        }

        private void PictureBox_Resize(object sender, EventArgs e)
        {
            //Graphics = _pictureBox.CreateGraphics();
        }

        public void FillRectangle(Brush brush, float x, float y, float width, float height)
        {
            Graphics.FillRectangle(brush, (x * Zoom) - CameraX, (y * Zoom) - CameraY, width * Zoom, height * Zoom);
        }

        public void FillEllipse(Brush brush, float x, float y, float width, float height)
        {
            Graphics.FillEllipse(brush, (x * Zoom) - CameraX, (y * Zoom) - CameraY, width * Zoom, height * Zoom);
        }

        public void DrawLine(Pen pen, float x, float y, float x1, float y1)
        {
            Graphics.DrawLine(pen, (x * Zoom) - CameraX, (y * Zoom) - CameraY, (x1 * Zoom) - CameraX, (y1 * Zoom) - CameraY);
        }

        public void Clear(Color color)
        {
            Graphics.Clear(color);
        }

        public void Render()
        {
            _pictureBox.Invalidate();
        }

        public (float, float) ScreenToWorld(int x, int y)
        {
            return ((x + CameraX) / Zoom, (y + CameraY) / Zoom);
        }
    }
}
