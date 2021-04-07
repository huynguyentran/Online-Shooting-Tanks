using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using theMap;
using System.Drawing;
using System.Windows.Forms;
using TankWars;

namespace View
{
    class BeamAnimation : Animatable
    {
        private Beam thisBeam;
        public Beam ThisBeam
        {
            get { return thisBeam; }
        }

        public override Vector2D Location => thisBeam.origin;

        public override float Orientation => thisBeam.Direction.ToAngle();

        private const int lifetime = 120;
        

        public BeamAnimation(Beam b)
        {
            thisBeam = b;
        }

        public override bool HasFinished()
        {
            return Age > lifetime;
        }

        public override void Draw(object o, PaintEventArgs e)
        {
            BeamAnimation b = (BeamAnimation)o;
            int length = 2000;
            float width = 10 * (1-(((float)b.Age) / BeamAnimation.lifetime));
            Color c = Color.Black;
            using (Brush br = new SolidBrush(c))
            {
                RectangleF bounds = new RectangleF(-(width / 2), -(length), width, length);
                e.Graphics.FillRectangle(br, bounds);
            }
        }
    }
}
