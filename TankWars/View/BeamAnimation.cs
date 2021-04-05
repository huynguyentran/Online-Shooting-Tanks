using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using theMap;
using System.Drawing;
using System.Windows.Forms;

namespace View
{
    class BeamAnimation
    {
        private Beam thisBeam;
        public Beam ThisBeam
        {
            get { return thisBeam; }
        }

        private const int lifetime = 120;
        private int age;

        public BeamAnimation(Beam b)
        {
            thisBeam = b;
            age = 0;
        }

        public void Update()
        {
            age++;
        }

        public bool HasFinished()
        {
            return age > lifetime;
        }

        public static void Draw(object o, PaintEventArgs e)
        {
            BeamAnimation b = (BeamAnimation)o;
            int length = 2000;
            float width = 10 * (1-(((float)b.age) / BeamAnimation.lifetime));
            Color c = Color.Black;
            using (Brush br = new SolidBrush(c))
            {
                RectangleF bounds = new RectangleF(-(width / 2), -(length), width, length);
                e.Graphics.FillRectangle(br, bounds);
            }
        }
    }
}
