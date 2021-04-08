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
    /// <summary>
    /// Drawing panel object that handles all drawing in the game. 
    /// </summary>
    class DrawingPanel : Panel
    {
        // Creating a model, which holds the information of object. 
        private Model model;

        // A private instance variable that holds the client last position in the drawing panel. 
        private Vector2D lastClientPosition;

        // A dictionary that contains the list of Tank sprites.
        private Dictionary<int, Tuple<Image, Image, Image>> spriteList;

        //A queue that holds the Images. 
        private Queue<Tuple<Image, Image, Image>> queue;

        //The background and the wall sprite. 
        private Image world, wall;

        //A HashSet of beam animation. 
        private HashSet<BeamAnimation> animationBeams;

        //A HashSet of tank explosions. 
        private HashSet<TankExplosionAnimation> explosions;

        /// <summary>
        /// A constructor to initializing the drawing panel. 
        /// </summary>
        /// <param name="m">The model that contains the information</param>
        public DrawingPanel(Model m)
        {
            DoubleBuffered = true;
            model = m;
            lastClientPosition = new Vector2D(0, 0);
            animationBeams = new HashSet<BeamAnimation>();
            explosions = new HashSet<TankExplosionAnimation>();

            //Taking images from the Resources folder. 
            string root = AppDomain.CurrentDomain.BaseDirectory;
            world = Image.FromFile(root + @"..\..\..\Resources\Image\Background.png");
            wall = Image.FromFile(root + @"..\..\..\Resources\Image\WallSprite.png");

            //The queue will have a list of initial Tank, Turret, and Projectiles sprite.Every time sprite is use, we dequeue the sprite and immediately enqueue so it at the bottom of the list. With this we can make sure that the initial 8 Tanks are unique. 
            queue = new Queue<Tuple<Image, Image, Image>>();
            //A dictionary that holds the Tuple of sprites with the color as its key. 
            Dictionary<string, Tuple<Image, Image, Image>> colorSpriteCollection = new Dictionary<string, Tuple<Image, Image, Image>>();

            //These 3 loops will loop through every file object in the resources, and add them correctly into the Tuple Queue. For example, A red tank will have a red turret and a red projectile.
            Regex tankRegex = new Regex(@"^.*\\(?'color'.+)Tank\.png$");
            foreach (string file in System.IO.Directory.GetFiles(root + @"..\..\..\Resources\Image\Tanks", "*png"))
            {
                Image image = Image.FromFile(file);
                Tuple<Image, Image, Image> t = new Tuple<Image, Image, Image>(image, null, null);
                String color = GetColor(tankRegex, file);
                colorSpriteCollection.Add(color, t);
            }

            Regex turretRegex = new Regex(@"^.*\\(?'color'.+)Turret\.png$");
            foreach (string file in System.IO.Directory.GetFiles(root + @"..\..\..\Resources\Image\Turrets", "*png"))
            {
                Image image = Image.FromFile(file);
                String color = GetColor(turretRegex, file);
                colorSpriteCollection[color] = new Tuple<Image, Image, Image>(colorSpriteCollection[color].Item1, image, null);
            }

            Regex projectileRegex = new Regex(@"^.*\\shot[-_](?'color'.+)\.png$");
            foreach (string file in System.IO.Directory.GetFiles(root + @"..\..\..\Resources\Image\Projectiles", "*png"))
            {
                Image image = Image.FromFile(file);
                String color = GetColor(projectileRegex, file);
                colorSpriteCollection[color] = new Tuple<Image, Image, Image>(colorSpriteCollection[color].Item1, colorSpriteCollection[color].Item2, image);
            }

            foreach (Tuple<Image, Image, Image> spriteGroup in colorSpriteCollection.Values)
            {
                queue.Enqueue(spriteGroup);
            }
               

            spriteList = new Dictionary<int, Tuple<Image, Image, Image>>();
        }


        /// <summary>
        /// Get the color from the file name. This method is depended on the Regex, so the naming scheme of the file has to be correct;
        /// </summary>
        /// <param name="regex">The regex</param>
        /// <param name="file">The name of the file</param>
        /// <returns></returns>
        private string GetColor(Regex regex, String file)
        {
            Match nameMatch = regex.Match(file);
            string color = "";
            foreach (Group g in nameMatch.Groups)
                if (g.Name.Equals("color"))
                    color = g.Value;

            color = color.ToUpper();
            return color;
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
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
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
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

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
                e.Graphics.DrawImage(spriteList[t.TankID].Item1, rec);
            }
        }

        private void TurretDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int widthTurret = 50;
            int heightTurret = 50;

            Rectangle tur = new Rectangle(-(widthTurret / 2), -(heightTurret / 2), widthTurret, heightTurret);
            e.Graphics.DrawImage(spriteList[t.TankID].Item2, tur);
        }

        private void ProjectileDrawer(object o, PaintEventArgs e)
        {
            Projectile p = (Projectile)o;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int size = 30;

            Rectangle bounds = new Rectangle(-(size / 2), -(size / 2), size, size);
            e.Graphics.DrawImage(spriteList[p.PlayerID].Item3, bounds);
        }

        private void PowerupDrawer(object o, PaintEventArgs e)
        {
            Powerup p = (Powerup)o;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
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
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Rectangle bounds = new Rectangle(-(size / 2), -(size / 2), size, size);
            e.Graphics.DrawImage(world, bounds);
        }

        public void NameAndScoreDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            FontFamily fontFamily = new FontFamily("Arial");
            Font font = new Font(
               fontFamily,
               14,
               FontStyle.Regular,
               GraphicsUnit.Pixel);
            using (Brush b = new SolidBrush(Color.White))
            {
                StringFormat drawFormat = new StringFormat();
                drawFormat.Alignment = StringAlignment.Center;
                e.Graphics.DrawString("Player name: " + t.Name + "\nScore: " + t.Score, font, b, 0, 40, drawFormat);
            }
        }

        public void HealthbarDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Color c = new Color();
            int height = 5;
            int width = 50 * t.HitPoints / 3;
            int maxhealth = 3;
            if (t.HitPoints > maxhealth * 2 / 3f)
            {
                c = Color.Green;
            }
            else if (t.HitPoints <= maxhealth * 1 / 3f)
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
            animationBeams.Add(new BeamAnimation(b));
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

}
