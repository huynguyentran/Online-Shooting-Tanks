using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using theMap;
using TankWars;
using System.Text.RegularExpressions;

namespace View
{
    class DrawingPanel : Panel
    {
        private Model model;
        private Vector2D lastClientPosition;

        private Dictionary<int, Tuple<Image, Image, Image>> spriteList;
        private Queue<Tuple<Image, Image, Image>> queue;

        Image world, wall;

        private HashSet<BeamAnimation> animationBeams;

        private HashSet<TankExplosionAnimation> explosions;

        private Image[] laserFrames;
        public DrawingPanel(Model m)
        {
            DoubleBuffered = true;
            model = m;
            lastClientPosition = new Vector2D(0, 0);

            animationBeams = new HashSet<BeamAnimation>();
            explosions = new HashSet<TankExplosionAnimation>();

            string root = AppDomain.CurrentDomain.BaseDirectory;
            world = Image.FromFile(root + @"..\..\..\Resources\Image\Background.png");
            wall = Image.FromFile(root + @"..\..\..\Resources\Image\WallSprite.png");

            queue = new Queue<Tuple<Image, Image, Image>>();

            Dictionary<string, Tuple<Image, Image, Image>> colorSpriteCollection = new Dictionary<string, Tuple<Image, Image, Image>>();

            Regex tankRegex = new Regex(@"^.*\\(?'color'.+)Tank\.png$");

            foreach (string file in System.IO.Directory.GetFiles(root + @"..\..\..\Resources\Image\Tanks", "*png"))
            {
                Image image = Image.FromFile(file);
                Tuple<Image, Image, Image> t = new Tuple<Image, Image, Image>(image, null, null);

                string color = GetInfoFromRegexGroup(tankRegex, file, "color").ToUpper();

                colorSpriteCollection.Add(color, t);
            }

            Regex turretRegex = new Regex(@"^.*\\(?'color'.+)Turret\.png$");

            foreach (string file in System.IO.Directory.GetFiles(root + @"..\..\..\Resources\Image\Turrets", "*png"))
            {
                Image image = Image.FromFile(file);

                string color = GetInfoFromRegexGroup(turretRegex, file, "color").ToUpper();

                colorSpriteCollection[color] = new Tuple<Image, Image, Image>(colorSpriteCollection[color].Item1, image, null);
            }

            Regex projectileRegex = new Regex(@"^.*\\shot[-_](?'color'.+)\.png$");

            foreach (string file in System.IO.Directory.GetFiles(root + @"..\..\..\Resources\Image\Projectiles", "*png"))
            {
                Image image = Image.FromFile(file);

                string color = GetInfoFromRegexGroup(projectileRegex, file, "color").ToUpper();

                colorSpriteCollection[color] = new Tuple<Image, Image, Image>(colorSpriteCollection[color].Item1, colorSpriteCollection[color].Item2, image);
            }

            foreach (Tuple<Image, Image, Image> spriteGroup in colorSpriteCollection.Values)
                queue.Enqueue(spriteGroup);

            spriteList = new Dictionary<int, Tuple<Image, Image, Image>>();

            string[] laserFiles = System.IO.Directory.GetFiles(root + @"..\..\..\Resources\Image\Laser", "*png");
            laserFrames = new Image[laserFiles.Length];

            Regex laserRegex = new Regex(@"^.*\\laserAnimation[0]*(?'index'\d+)\.png$");

            foreach(string laserFile in laserFiles)
            {
                Image laserFrame = Image.FromFile(laserFile);
                int index = int.Parse(GetInfoFromRegexGroup(laserRegex, laserFile, "index")) - 1;
                laserFrames[index] = laserFrame;
            }


        }

        private string GetInfoFromRegexGroup(Regex r, string file, string groupName)
        {
            Match nameMatch = r.Match(file);
            string output = "";
            foreach (Group g in nameMatch.Groups)
                if (g.Name.Equals(groupName))
                    output = g.Value;
            return output;
        }



        public void OnTankDeath(Tank t)
        {
            explosions.Add(new TankExplosionAnimation(t));

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


        private void WallDrawer(object o, PaintEventArgs e)
        {
            Wall castWall = (Wall)o;

            double totalWidth = Math.Abs(castWall.Start.GetX() - castWall.End.GetX());
            double totalHeight = Math.Abs(castWall.Start.GetY() - castWall.End.GetY());

            if (totalWidth == 0)
            {
                totalHeight += 50;
                totalWidth = 50;


                for (int i = (int)-(totalHeight / 2); i < totalHeight / 2; i += 50)
                {
                    Rectangle rect = new Rectangle(-25, i, 50, 50);
                    e.Graphics.DrawImage(wall, rect);
                }
            }
            else if (totalHeight == 0)
            {
                totalHeight = 50;
                totalWidth += 50;
                for (int i = (int)-(totalWidth / 2); i < totalWidth / 2; i += 50)
                {
                    Rectangle rect = new Rectangle(i, -25, 50, 50);
                    e.Graphics.DrawImage(wall, rect);
                }
            }



        }

        private void TankDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;

            int width = 60;
            int height = 60;
            
            Rectangle rec = new Rectangle(-(width / 2), -(height / 2), width, height);
            if (!spriteList.ContainsKey(t.TankID))
            {
                Tuple<Image, Image, Image> usedImage = queue.Dequeue();
                spriteList.Add(t.TankID, usedImage);
                e.Graphics.DrawImage(usedImage.Item1, rec);
                queue.Enqueue(usedImage);
            }
            else
            {
                e.Graphics.DrawImage(spriteList[t.TankID].Item1,rec);
            }
        }

        private void TurretDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;
            int widthTurret = 50;
            int heightTurret = 50;
            
            Rectangle tur = new Rectangle(-(widthTurret / 2), -(heightTurret / 2), widthTurret, heightTurret);
            e.Graphics.DrawImage(spriteList[t.TankID].Item2, tur);
        }

        private void ProjectileDrawer(object o, PaintEventArgs e)
        {
            Projectile p = (Projectile)o;

            int size = 30;

            Rectangle bounds = new Rectangle(-(size / 2), -(size / 2), size, size);
            e.Graphics.DrawImage(spriteList[p.PlayerID].Item3, bounds);
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

        public void WorldDrawer(object o, PaintEventArgs e)
        {
            int size = model.mapSize;

            Rectangle bounds = new Rectangle(-(size / 2), -(size / 2), size, size);
            e.Graphics.DrawImage(world, bounds);
        }

        public void NameAndScoreDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;
            FontFamily fontFamily = new FontFamily("Arial");
            Font font = new Font(
               fontFamily,
               14,
               FontStyle.Regular,
               GraphicsUnit.Pixel);
            using (Brush b  = new SolidBrush(Color.White))
            {
                StringFormat drawFormat = new StringFormat();
                drawFormat.Alignment = StringAlignment.Center;
                e.Graphics.DrawString("Player name: " +t.Name + "\nScore: " +t.Score,font,b,0,40,drawFormat);
            }
        }

        public void HealthbarDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;
            Color c = new Color();
            int height = 5;
            int width = 50*t.HitPoints/3;
            int maxhealth = 3;
            if (t.HitPoints > maxhealth * 2/3f)
            {
                c = Color.Green;
            }
            else if (t.HitPoints <= maxhealth * 1/3f)
            {
                c = Color.Red;
            }
            else
            {
                c = Color.Yellow;
            }
            
            using (Brush b = new SolidBrush(c))
            {
                Rectangle bounds = new Rectangle(-(width / 2), -(height / 2) - 40, width, height);
                e.Graphics.FillRectangle(b, bounds);     
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


            DrawObjectWithTransform(e, world, 0, 0, 0, WorldDrawer);



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
                    DrawObjectWithTransform(e, t, t.Location.GetX(), t.Location.GetY(), 0, NameAndScoreDrawer);
                    DrawObjectWithTransform(e, t, t.Location.GetX(), t.Location.GetY(), 0, HealthbarDrawer);
                }

                foreach (Projectile p in model.Projectiles.Values)
                {

                    DrawObjectWithTransform(e, p, p.Location.GetX(), p.Location.GetY(), p.Orientation.ToAngle(), ProjectileDrawer);
                }

                foreach (Powerup p in model.Powerups.Values)
                {
                    DrawObjectWithTransform(e, p, p.Location.GetX(), p.Location.GetY(), 0, PowerupDrawer);
                }

                DrawAnimations(explosions, e);

                DrawAnimations(animationBeams, e);

            }
            // Do anything that Panel (from which we inherit) needs to do
            base.OnPaint(e);
        }

        public void OnBeamArrive(Beam b)
        {
            animationBeams.Add(new BeamAnimation(b, laserFrames));
        }

        private void DrawAnimations<T>(HashSet<T> anims, PaintEventArgs e) where T : Animatable
        {
            HashSet<T> animsToRemove = new HashSet<T>();

            foreach (T anim in anims)
            {
                anim.Update();
                DrawObjectWithTransform(e, anim, anim.Location.GetX(), anim.Location.GetY(), anim.Orientation, anim.Draw);
                if (anim.HasFinished())
                    animsToRemove.Add(anim);
            }

            foreach (T anim in animsToRemove)
                anims.Remove(anim);
        }
    }

    //public static class ListExtension
    //{
    //    private static Random rng = new Random();
    //    public static void Shuffle<T>(this IList<T> list)
    //    {
    //        int n = list.Count;
    //        while (n > 1)
    //        {
    //            n--;
    //            int k = rng.Next(n + 1);
    //            T value = list[k];
    //            list[k] = list[n];
    //            list[n] = value;
    //        }
    //    }

    //}
}
