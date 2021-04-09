using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;
using System.Drawing;
using System.Windows.Forms;
using TankWars;

namespace View
{
    /// <summary>
    /// A class represents the tank explosion animation.
    /// </summary>
    class TankExplosionAnimation : FrameByFrameAnimation
    {
        private Vector2D location;

        public override Vector2D Location => location;

        public override float Orientation => 0f;

        protected override int LifeTime { get => lifeTime; set => lifeTime= value; }

        private int lifeTime = 0;

        public TankExplosionAnimation(Tank t, Image[] _frames, int _lifetime) : base(_frames,_lifetime)
        {
            location = t.Location;
        }

        /// <summary>
        /// Draw the tank explosion current frame.
        /// </summary>
        public override void Draw(object o, PaintEventArgs e)
        {
            float progress = Age / (float)lifeTime;
            Image frame = CurrentFrame();
            int size = 160;
            RectangleF rect = new RectangleF(-(size/2), -(size/2), size, size);
            e.Graphics.DrawImage(frame, rect);
        }

        public override bool HasFinished()
        {
            return Age >= lifeTime;
        }
    }
}
