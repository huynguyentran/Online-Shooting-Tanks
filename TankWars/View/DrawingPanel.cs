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
        public DrawingPanel(Model m)
        {
            DoubleBuffered = true;
            model = m;
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
            int height = 70;

            using (Brush b = new SolidBrush(Color.Blue))
            {
                Rectangle rec = new Rectangle(-(width / 2), -(height / 2), width, height);

                e.Graphics.FillRectangle(b, rec);
            }

        }


        // This method is invoked when the DrawingPanel needs to be re-drawn
        protected override void OnPaint(PaintEventArgs e)
        {

            // Center the view on the middle of the world,
            // since the image and world use different coordinate systems
            int viewSize = Size.Width; // view is square, so we can just use width
            if (model.Tanks.ContainsKey(model.clientID))
            {
                float xLoc = (float)model.Tanks[model.clientID].Location.GetX();
                float yLoc = (float)model.Tanks[model.clientID].Location.GetY();
                e.Graphics.TranslateTransform(viewSize/2 - xLoc , viewSize/2 - yLoc );
            }

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
                }

            }
            // Do anything that Panel (from which we inherit) needs to do
            base.OnPaint(e);
        }

    }
}
