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
    class BeamAnimation : FrameByFrameAnimation
    {
        private Beam thisBeam;
        public Beam ThisBeam
        {
            get { return thisBeam; }
        }

        public override Vector2D Location => thisBeam.origin;

        public override float Orientation => thisBeam.Direction.ToAngle();

        protected override int LifeTime { get => lifeTime; set => lifeTime = value; }

        private int lifeTime;
        

        public BeamAnimation(Beam b, Image[] _frames) : base(_frames)
        {
            thisBeam = b;
        }

        public override void Draw(object o, PaintEventArgs e)
        {
            int length = 2000;
            float width;

            float expandPercent = 0.15f;

            if (Age < lifeTime * expandPercent)
            {
                width = 200 * ((((float)Age) / (lifeTime* expandPercent))) + 25;
            }
            else
            {
                width = 200 * (1 - (((float)Age - lifeTime*expandPercent) / (lifeTime*(1 - expandPercent)))) + 25;
            }
            
            RectangleF bounds = new RectangleF(-(width / 2), -(length), width, length);
            e.Graphics.DrawImage(CurrentFrame(), bounds);
        }
    }
}
