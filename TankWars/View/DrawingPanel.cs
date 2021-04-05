using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using theMap;
using TankWars;

namespace View
{
    class DrawingPanel : Panel
    {
        private Model model;
        private Vector2D lastClientPosition;
        public DrawingPanel(Model m)
        {
            DoubleBuffered = true;
            model = m;
            lastClientPosition = new Vector2D(0, 0);
        }


        public void OnTankDeath(Tank t)
        {
            if (t.TankID == model.clientID)
            {
                lastClientPosition = t.Location;
            }
        }

        // A delegate for DrawObjectWithTransform
        // Methods matching this delegate can draw whatever they want using e  
        public delegate void ObjectDrawer(object o, PaintEventArgs e);


        /// <summary>
        /// This method performs a translation and rotation to drawn an object in the world.
        /// </summary>
        /// <param name="e">PaintEventArgs to access the graphics (for drawing)</param>
        /// <param name="o">The object to draw</param>
        /// <param name="worldX">The X coordinate of the object in world space</param>
        /// <param name="worldY">The Y coordinate of the object in world space</param>
        /// <param name="angle">The orientation of the objec, measured in degrees clockwise from "up"</param>
        /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
        private void DrawObjectWithTransform(PaintEventArgs e, object o, double worldX, double worldY, double angle, ObjectDrawer drawer)
        {
            // "push" the current transform
            System.Drawing.Drawing2D.Matrix oldMatrix = e.Graphics.Transform.Clone();

            e.Graphics.TranslateTransform((int)worldX, (int)worldY);
            e.Graphics.RotateTransform((float)angle);
            drawer(o, e);

            // "pop" the transform
            e.Graphics.Transform = oldMatrix;
        }

        private void WallDrawer(object wall, PaintEventArgs e)
        {
            Wall castWall = (Wall)wall;

            using (Brush b = new SolidBrush(Color.Red))
            {
                double totalWidth = Math.Abs(castWall.Start.GetX() - castWall.End.GetX());
                double totalHeight = Math.Abs(castWall.Start.GetY() - castWall.End.GetY());

                if (totalWidth == 0)
                {
                    totalHeight += 50;
                    totalWidth = 50;
                }
                else if (totalHeight == 0)
                {
                    totalHeight = 50;
                    totalWidth += 50;
                }

                Rectangle rect = new Rectangle((int)-(totalWidth / 2), (int)-(totalHeight / 2), (int)totalWidth, (int)totalHeight);
                e.Graphics.FillRectangle(b, rect);
            }
        }

        private void TankDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;

            int width = 60;
            int height = 60;



            using (Brush b = new SolidBrush(Color.Blue))
            {
                Rectangle rec = new Rectangle(-(width / 2), -(height / 2), width, height);

                e.Graphics.FillRectangle(b, rec);


            }

        }

        private void TurretDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;
            int widthTurret = 50;
            int heightTurret = 50;
            using (Brush b = new SolidBrush(Color.Purple))
            {
                Rectangle tur = new Rectangle(-(widthTurret / 2), -(heightTurret / 2), widthTurret, heightTurret);
                e.Graphics.FillRectangle(b, tur);
            }
        }

        private void ProjectileDrawer(object o, PaintEventArgs e)
        {
            Projectile p = (Projectile)o;

            int majorDiameter = 20;
            int minorDiameter = 10;

            Color c = Color.Green;

            if (p.PlayerID == model.clientID)
                c = Color.Red;

            using (Brush b = new SolidBrush(c))
            {
                Rectangle bounds = new Rectangle(-(minorDiameter / 2), -(majorDiameter / 2), minorDiameter, majorDiameter);
                e.Graphics.FillEllipse(b, bounds);
            }
        }

        private void PowerupDrawer(object o, PaintEventArgs e)
        {
            Powerup p = (Powerup)o;
            int size = 10;

            Color c = Color.Orange;

            using (Brush b = new SolidBrush(c))
            {
                Rectangle bounds = new Rectangle(-(size / 2), -(size / 2), size, size);
                e.Graphics.FillEllipse(b, bounds);
            }
        }

        private void BeamDrawer(object o, PaintEventArgs e)
        {
            Beam b = (Beam)o;
            int length = 900;
            int width = 1;
            Color c = Color.Yellow;
            using (Brush br = new SolidBrush(c))
            {
                Rectangle bounds = new Rectangle(-(width / 2), -(length), width, length);
                e.Graphics.FillRectangle(br, bounds);
               
            }
        }


        // This method is invoked when the DrawingPanel needs to be re-drawn
        protected override void OnPaint(PaintEventArgs e)
        {

            // Center the view on the middle of the world,
            // since the image and world use different coordinate systems
            int viewSize = Size.Width; // view is square, so we can just use width
            float xLoc;
            float yLoc;

            if (model.Tanks.ContainsKey(model.clientID))
            {
                xLoc = (float)model.Tanks[model.clientID].Location.GetX();
                yLoc = (float)model.Tanks[model.clientID].Location.GetY();
            }
            else
            {
                xLoc = (float)lastClientPosition.GetX();
                yLoc = (float)lastClientPosition.GetY();
            }

            e.Graphics.TranslateTransform(viewSize / 2 - xLoc, viewSize / 2 - yLoc);


            lock (model)
            {
                // Draw the players
                foreach (Wall w in model.Walls.Values)
                {
                    DrawObjectWithTransform(e, w, (w.Start.GetX() + w.End.GetX()) / 2, (w.Start.GetY() + w.End.GetY()) / 2, 0, WallDrawer);
                }

                foreach (Tank t in model.Tanks.Values)
                {
                    DrawObjectWithTransform(e, t, t.Location.GetX(), t.Location.GetY(), t.Orientation.ToAngle(), TankDrawer);
                    DrawObjectWithTransform(e, t, t.Location.GetX(), t.Location.GetY(), t.TurretDirection.ToAngle(), TurretDrawer);
                }

                foreach (Projectile p in model.Projectiles.Values)
                {

                    DrawObjectWithTransform(e, p, p.Location.GetX(), p.Location.GetY(), p.Orientation.ToAngle(), ProjectileDrawer);
                }

                foreach (Powerup p in model.Powerups.Values)
                {
                    DrawObjectWithTransform(e, p, p.Location.GetX(), p.Location.GetY(), 0, PowerupDrawer);
                }

                foreach (Beam b in model.Beams.Values)
                {
                    DrawObjectWithTransform(e, b, b.origin.GetX(), b.origin.GetY(), b.Direction.ToAngle(), BeamDrawer);
              
                }
            }
            // Do anything that Panel (from which we inherit) needs to do
            base.OnPaint(e);
        }

    }
}
