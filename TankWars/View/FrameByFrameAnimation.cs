using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TankWars;
using System.Drawing;

namespace View
{
    abstract class FrameByFrameAnimation : Animatable
    {
        protected abstract int LifeTime
        {
            get;
            set;
        }

        protected Image[] frames;

        public FrameByFrameAnimation(Image[] _frames, int _lifeTime)
        {
            frames = _frames;
            LifeTime = _lifeTime;
        }

        public FrameByFrameAnimation(Image[] _frames) : this(_frames, _frames.Length) { }

        protected Image CurrentFrame()
        {
            return frames[Math.Min((int)((Age / (float) LifeTime) * frames.Length), frames.Length- 1)];
        }

        public override bool HasFinished()
        {
            return Age > LifeTime;
        }
    }
}
