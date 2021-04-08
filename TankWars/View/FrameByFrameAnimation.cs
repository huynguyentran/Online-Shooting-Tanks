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

    /// <summary>
    /// An abstract class that return the frame of the current animation life cycle. 
    /// </summary>
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

        /// <summary>
        /// The method that returns the right frame on the animation lifetime. 
        /// </summary>
        /// <returns>The frame in the frame array.</returns>
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
