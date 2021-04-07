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
    class TankExplosionAnimation : Animatable
    {
        private Vector2D location;

        public override Vector2D Location => location;

        public override float Orientation => 0f;

        private int lifeTime = 180;
        private int radius = 30;

        public TankExplosionAnimation(Tank toExplode)
        {
            location = toExplode.Location;
        }

        public override void Draw(object o, PaintEventArgs e)
        {
            float progress = Age / (float)lifeTime;

            Color c = Color.FromArgb((int)(255 * progress), Color.Red);

            float currentRadius = radius * progress;
            RectangleF rect = new RectangleF(-(currentRadius), -(currentRadius), currentRadius*2, currentRadius*2);
            
            using(Brush b = new SolidBrush(c))
            {
                e.Graphics.FillEllipse(b, rect);
            }
        }

        public override bool HasFinished()
        {
            return Age >= lifeTime;
        }
    }
}
