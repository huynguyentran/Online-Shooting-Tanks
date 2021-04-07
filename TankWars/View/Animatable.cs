using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using TankWars;

namespace View
{
    abstract class Animatable
    {
        private int age = 0;
        protected int Age
        {
            get { return age; }
        }

        public abstract Vector2D Location
        {
            get;
        }

        public abstract float Orientation
        {
            get;
        }

        public virtual void Update()
        {
            age++;
        }

        public abstract bool HasFinished();

        public abstract void Draw(object o, PaintEventArgs e);
    }
}
