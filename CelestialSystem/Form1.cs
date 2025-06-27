using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TCPMon;

namespace CelestialSystem
{
    public partial class Form1 : Form
    {
        private CelestialSystem _celestialSystem;
        private CelestialBody _selectedBody;
        private readonly Renderer _renderer;
        private DateTime _lastTime;

        private AboutForm _aboutForm = new AboutForm();

        private Pen _arrowPen = new Pen(Brushes.LightPink, 2) { StartCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor };

        public Form1()
        {
            InitializeComponent();
            _lastTime = DateTime.Now;
            bodyStatusLabel.Text = "No body selected";

            _renderer = new Renderer(renderWin);
            _celestialSystem = new CelestialSystem(0.05f);

            _renderer.Clicked += _renderer_Clicked;
            _renderer.Paint += _renderer_Paint;

            timer1.Start();
            _renderer.Zoom = 0.5f;
            _renderer.CameraY = -20;

            eulerToolStripMenuItem.Checked = true;
        }

        private void _renderer_Clicked(object sender, MouseEventArgs e)
        {
            switch(e.Button)
            {
                case MouseButtons.Left:
                    foreach (var body in _celestialSystem.Bodies)
                    {
                        if (body.Overlaps(e.X, e.Y))
                        {
                            _selectedBody = body;
                            propertyGrid1.SelectedObject = body;
                            break;
                        }
                    }
                    break;

                case MouseButtons.Middle:
                    {
                        float.TryParse(dM.Text, out float mass);

                        float.TryParse(dVX.Text, out float vX);
                        float.TryParse(dVY.Text, out float vY);

                        var body = new CelestialBody(_celestialSystem, e.X, e.Y, Math.Max(mass, 1), new Vector2(vX, vY));
                        _selectedBody = body;
                        propertyGrid1.SelectedObject = body;
                        break;
                    }
            }
        }

        private void _renderer_Paint(object sender, EventArgs e)
        {
            _renderer.Graphics.Clear(Color.Black);
            _celestialSystem.Draw(_renderer);

            foreach (var body in _celestialSystem.Bodies)
            {
                if (showVelocityToolStripMenuItem.Checked)
                {
                    float vScaleFactor = (float)Math.Log(body.Velocity.Length(), 1.08f);
                    _arrowPen.Color = Color.Aqua;
                    _renderer.DrawLine(_arrowPen, (body.Position.X + body.Velocity.X * vScaleFactor), (body.Position.Y + body.Velocity.Y * vScaleFactor), body.Position.X, body.Position.Y);

                }

                if(showForcesMenuItem1.Checked)
                {
                    float fScaleFactor = (float)Math.Log(body.TotalForce.Length(), 1.008f);

                    _arrowPen.Color = Color.Orange;
                    _renderer.DrawLine(_arrowPen, (body.Position.X - body.TotalForce.X * fScaleFactor), (body.Position.Y - body.TotalForce.Y * fScaleFactor), body.Position.X, body.Position.Y);
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime newNow = DateTime.Now;
            TimeSpan diff = newNow - _lastTime;
            _lastTime = newNow;

            if (runSim.Checked)
                _celestialSystem.Update((float)diff.TotalSeconds);

            _renderer.Render();

            if(!(_selectedBody is null))
            {
                float dist = _selectedBody.DistanceTo(_renderer.MouseX, _renderer.MouseY);

                bodyStatusLabel.Text = $"Distance: {dist:F2} | Velocity: ({_selectedBody.Velocity.X:F2}, {_selectedBody.Velocity.Y:F2}) | Speed: {_selectedBody.Velocity.Length():F2}";
            }
        }

        private void timeStepText_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(timeStepText.Text, out float timeStep)) timeStep = 1f;

            _celestialSystem.TimeStep = timeStep;
        }

        private void gConstText_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(gConstText.Text, out float gConstant)) gConstant = 1f;

            _celestialSystem.GravitationalConstant = gConstant;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _aboutForm.StartPosition = FormStartPosition.CenterParent;
            _aboutForm.ShowDialog();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _celestialSystem = new CelestialSystem(1f);
            gConstText.Text = "1";
            timeStepText.Text = "0.1";
            speedUpTime.Text = "0";
            runSim.Checked = true;
            propertyGrid1.SelectedObject = null;
            _selectedBody = null;
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = "CelestialSim File|*.csm",
                RestoreDirectory = true
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                BinaryReader r = new BinaryReader(dialog.OpenFile());
                _celestialSystem.FromBinary(r);
                r.Close();

                gConstText.Text = _celestialSystem.GravitationalConstant.ToString();
                timeStepText.Text = _celestialSystem.TimeStep.ToString();
                speedUpTime.Text = _celestialSystem.FastForwardTime.ToString();

                switch(_celestialSystem.SimulationMethod)
                {
                    case CelestialSystem.Method.Euler:
                        eulerToolStripMenuItem.Checked = true;
                        rK2ToolStripMenuItem.Checked = false;
                        break;

                    case CelestialSystem.Method.RK2:
                        eulerToolStripMenuItem.Checked = false;
                        rK2ToolStripMenuItem.Checked = true;
                        break;
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog()
            {
                Filter = "CelestialSim File|*.csm",
                RestoreDirectory = true
            };

            if(dialog.ShowDialog() == DialogResult.OK)
            {
                BinaryWriter w = new BinaryWriter(dialog.OpenFile());
                _celestialSystem.ToBinary(w);
                w.Close();
            }
        }

        private void speedUpTime_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(speedUpTime.Text, out float fastForward)) fastForward = 0;

            _celestialSystem.FastForwardTime = fastForward;
        }

        private void eulerToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            rK2ToolStripMenuItem.Checked = !eulerToolStripMenuItem.Checked;
            _celestialSystem.SimulationMethod = CelestialSystem.Method.Euler;
        }

        private void rK2ToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            eulerToolStripMenuItem.Checked = !rK2ToolStripMenuItem.Checked;
            _celestialSystem.SimulationMethod = CelestialSystem.Method.RK2;
        }
    }
}
