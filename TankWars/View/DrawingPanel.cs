using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Model;
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
        private ClientModel model;

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

        private Image[] laserFrames;

        private Image[] tankExplosionFrames;

        /// <summary>
        /// A constructor to initializing the drawing panel. 
        /// </summary>
        /// <param name="m">The model that contains the information</param>
        public DrawingPanel(ClientModel m)
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

            //These 3 loops will loop through every Tank, Turret, and Projectile object in Resources folder, and add them correctly into the Tuple Queue. For example, A red tank will have a red turret and a red projectile.
            Regex tankRegex = new Regex(@"^.*\\(?'color'.+)Tank\.png$");
            foreach (string file in System.IO.Directory.GetFiles(root + @"..\..\..\Resources\Image\Tanks", "*png"))
            {
                //Take the image from the file
                Image image = Image.FromFile(file);
                //Create the tuple of Tank, Turret, and Projectile
                Tuple<Image, Image, Image> t = new Tuple<Image, Image, Image>(image, null, null);
                //Group the 3 into the same group by their color 
                string color = GetInfoFromRegexGroup(tankRegex, file, "color").ToUpper();
                //Addd into the dictionary. 
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

            //Queue the tank so that the first 8 Tanks have differerent color from each other. 
            foreach (Tuple<Image, Image, Image> spriteGroup in colorSpriteCollection.Values)
            {
                queue.Enqueue(spriteGroup);
            }

            spriteList = new Dictionary<int, Tuple<Image, Image, Image>>();

            //These 2 loops will loop through each Image object in the laserFile and the tankEplosionFiles so that the program can run animation when the tank is exploded or a laser has been shoot. 
            string[] laserFiles = System.IO.Directory.GetFiles(root + @"..\..\..\Resources\Image\Laser", "*png");
            laserFrames = new Image[laserFiles.Length];

            Regex laserRegex = new Regex(@"^.*\\laserAnimation[0]*(?'index'\d+)\.png$");
            foreach(string laserFile in laserFiles)
            {
                Image laserFrame = Image.FromFile(laserFile);
                int index = int.Parse(GetInfoFromRegexGroup(laserRegex, laserFile, "index")) - 1;
                laserFrames[index] = laserFrame;
            }

            string[] tankExplosionFiles = System.IO.Directory.GetFiles(root + @"..\..\..\Resources\Image\TankExplosion", "*png");
            tankExplosionFrames = new Image[tankExplosionFiles.Length];

            Regex tankExplosionRegex = new Regex(@"^.*\\tankExplosion[0]*(?'index'\d+)\.png$");
            foreach (string tankExplosionFile in tankExplosionFiles)
            {
                Image tankExplosionFrame = Image.FromFile(tankExplosionFile);
                int index = int.Parse(GetInfoFromRegexGroup(tankExplosionRegex, tankExplosionFile, "index")) - 1;
                tankExplosionFrames[index] = tankExplosionFrame;
            }

        }

        /// <summary>
        /// Convert the name of the File and group it into different kind of tags and IDs. 
        /// </summary>
        /// <param name="r">Regex of the name</param>
        /// <param name="file">Name of the file</param>
        /// <param name="groupName">group name that the object associated in.</param>
        /// <returns></returns>
        private string GetInfoFromRegexGroup(Regex r, string file, string groupName)
        {
            Match nameMatch = r.Match(file);
            string output = "";
            foreach (Group g in nameMatch.Groups)
                if (g.Name.Equals(groupName))
                    output = g.Value;
            return output;
        }


        /// <summary>
        /// When a tank has registered as "died", we need to call the explosion animation for that tank.
        /// If the tank is the client tank, the camera will stay on the explosion for the duration. 
        /// </summary>
        /// <param name="t"></param>
        public void OnTankDeath(Tank t)
        {
            if (t.TankID == model.clientID)
            {
                lastClientPosition = t.Location;
            }
            explosions.Add(new TankExplosionAnimation(t,tankExplosionFrames,220));
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

        /// <summary>
        /// A Drawer delegate for the Wall. This is used in DrawObjectWithTransform 
        /// </summary>
        private void WallDrawer(object o, PaintEventArgs e)
        {
            Wall castWall = (Wall)o;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            double totalWidth = Math.Abs(castWall.Start.GetX() - castWall.End.GetX());
            double totalHeight = Math.Abs(castWall.Start.GetY() - castWall.End.GetY());

            //Since the Wall information is sent with 2 point at the start and the end of the wall, 
            //either the width or the length will be equal to 0 since it is a line. 
            if (totalWidth == 0)
            {
                //Add 50 so that we stretch so that we go to the outline of the wall, instead of going from the center
                //of the starting point wall to the center of the ending point wall.
                totalHeight += 50;
                //The other lenght would be 50 since wall are composed of square blocks. 
                totalWidth = 50;
                //Draw each block of wall for the entire length. 
                for (int i = (int)-(totalHeight / 2); i < totalHeight / 2; i += 50)
                {
                    Rectangle rect = new Rectangle(-25, i, 50, 50);
                    e.Graphics.DrawImage(wall, rect);
                }
            }
            //Repeat for the other if height is 0
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

        /// <summary>
        /// A Drawer delegate for the Tanks. This is used in DrawObjectWithTransform 
        /// </summary>
        private void TankDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int width = 60;
            int height = 60;
            Rectangle rec = new Rectangle(-(width / 2), -(height / 2), width, height);
            //If the group that represented a tank has not been added into the spriteList. We dequeue the group at the top of the Tuple,
            //and use it as sprite for the tank.
            if (!spriteList.ContainsKey(t.TankID))
            {
                Tuple<Image, Image, Image> usedImage = queue.Dequeue();
                spriteList.Add(t.TankID, usedImage);
                e.Graphics.DrawImage(usedImage.Item1, rec);
                queue.Enqueue(usedImage);
            }
            //Else, we just draw the tank normally. This means the tank has connected to the server. 
            else
            {
                e.Graphics.DrawImage(spriteList[t.TankID].Item1, rec);
            }
        }

        /// <summary>
        /// A Drawer delegate for the Turrets. This is used in DrawObjectWithTransform 
        /// </summary>
        private void TurretDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int widthTurret = 50;
            int heightTurret = 50;
            Rectangle tur = new Rectangle(-(widthTurret / 2), -(heightTurret / 2), widthTurret, heightTurret);
            e.Graphics.DrawImage(spriteList[t.TankID].Item2, tur);
        }

        /// <summary>
        /// A Drawer delegate for the Projectiles. This is used in DrawObjectWithTransform 
        /// </summary>
        private void ProjectileDrawer(object o, PaintEventArgs e)
        {
            Projectile p = (Projectile)o;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int size = 30;
            Rectangle bounds = new Rectangle(-(size / 2), -(size / 2), size, size);
            e.Graphics.DrawImage(spriteList[p.PlayerID].Item3, bounds);
        }

        /// <summary>
        /// A Drawer delegate for the Powerups. This is used in DrawObjectWithTransform 
        /// </summary>
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

        /// <summary>
        /// A Drawer delegate for the World map. This is used in DrawObjectWithTransform 
        /// </summary>
        public void WorldDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int size = model.MapSize;
            Rectangle bounds = new Rectangle(-(size / 2), -(size / 2), size, size);
            e.Graphics.DrawImage(world, bounds);
        }


        /// <summary>
        /// A Drawer delegate for the name and score of the player. This is used in DrawObjectWithTransform 
        /// </summary>
        public void NameAndScoreDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            FontFamily fontFamily = new FontFamily("Arial");
            Font font = new Font( fontFamily, 14,  FontStyle.Regular, GraphicsUnit.Pixel);
            using (Brush b = new SolidBrush(Color.White))
            {
                StringFormat drawFormat = new StringFormat();
                drawFormat.Alignment = StringAlignment.Center;
                e.Graphics.DrawString("Player name: " + t.Name + "\nScore: " + t.Score, font, b, 0, 40, drawFormat);
            }
        }

        /// <summary>
        /// A Drawer delegate for the health bar of the player. This is used in DrawObjectWithTransform 
        /// </summary>
        public void HealthbarDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Color c = new Color();
            int height = 5;
            //We are assuming that the maximum health point is 3 in all scenarios. 
            int width = 50 * t.HitPoints / 3;
            int maxhealth = 3;

            //If the hitpoint is more than 2/3 of the maximum hit points, the color of healthbar is green.
            if (t.HitPoints > maxhealth * 2 / 3f)
            {
                c = Color.Green;
            }

            //If the hitpoint is less than 1/3 of the maximum hit points, the color of healthbar is red.
            else if (t.HitPoints <= maxhealth * 1 / 3f)
            {
                c = Color.Red;
            }
            
            //Else, the healthbar is yellow. 
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

            //Focus on the tank in the middle of the screen. 
            e.Graphics.TranslateTransform(viewSize / 2 - xLoc, viewSize / 2 - yLoc);

            //Draw the world one time.
            DrawObjectWithTransform(e, world, 0, 0, 0, WorldDrawer);

            lock (model)
            {
                // Draw the walls. 
                foreach (Wall w in model.Walls.Values)
                {
                    DrawObjectWithTransform(e, w, (w.Start.GetX() + w.End.GetX()) / 2, (w.Start.GetY() + w.End.GetY()) / 2, 0, WallDrawer);
                }
                // Draw the tanks,turret, id, scores, and healthbar.  
                foreach (Tank t in model.Tanks.Values)
                {
                    DrawObjectWithTransform(e, t, t.Location.GetX(), t.Location.GetY(), t.Orientation.ToAngle(), TankDrawer);
                    DrawObjectWithTransform(e, t, t.Location.GetX(), t.Location.GetY(), t.TurretDirection.ToAngle(), TurretDrawer);
                    DrawObjectWithTransform(e, t, t.Location.GetX(), t.Location.GetY(), 0, NameAndScoreDrawer);
                    DrawObjectWithTransform(e, t, t.Location.GetX(), t.Location.GetY(), 0, HealthbarDrawer);
                }

                // Draw the projectiles.
                foreach (Projectile p in model.Projectiles.Values)
                {
                    DrawObjectWithTransform(e, p, p.Location.GetX(), p.Location.GetY(), p.Orientation.ToAngle(), ProjectileDrawer);
                }

                // Draw the random powerups. 
                foreach (Powerup p in model.Powerups.Values)
                {
                    DrawObjectWithTransform(e, p, p.Location.GetX(), p.Location.GetY(), 0, PowerupDrawer);
                }
                
                // Draw the explosion when tank "died".
                DrawAnimations(explosions, e);

                // Draw the beam aniamtion when a beam is fired. 
                DrawAnimations(animationBeams, e);
            }
            // Do anything that Panel (from which we inherit) needs to do
            base.OnPaint(e);
        }

        /// <summary>
        /// When an alt fire is registered, add into the beam list so the OnPaint method can draw. 
        /// </summary>
        /// <param name="b"></param>
        public void OnBeamArrive(Beam b)
        {
            animationBeams.Add(new BeamAnimation(b, laserFrames));
        }

        /// <summary>
        /// An Draw animation method to draw the animation when it is necessary. 
        /// Generic because there are different kind of animation.
        /// </summary>
        private void DrawAnimations<T>(HashSet<T> anims, PaintEventArgs e) where T : Animatable
        {
            HashSet<T> animsToRemove = new HashSet<T>();
            //For each animation that registered, Draw that animation and remove from the list. 
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
