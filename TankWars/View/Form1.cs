using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Controller;
using theMap;

namespace View
{
    public partial class Form1 : Form
    {
        private GameController gController;


        private const int menuSize = 40;
        private const int viewSize = 900;
        private DrawingPanel drawingPanel;
        public Form1(GameController _gController)
        {
            InitializeComponent();

            gController = _gController;
            gController.AddErrorHandler(MessageBoxForError);

            gController.updateView += WorldUpdate;

            gController.deathEvent += OnDeath;
         
            drawingPanel = new  DrawingPanel(gController.world);
            drawingPanel.Location = new Point(0, menuSize);
            drawingPanel.Size = new Size(viewSize, viewSize);
            this.Controls.Add(drawingPanel);

            //Handling Inputs
            //this.KeyDown += gController.HandleMoveRequest;
            drawingPanel.KeyDown += TranslateKeyPress;
            drawingPanel.KeyUp += TranslateKeyUp;
            drawingPanel.MouseMove += MouseMovement;

            drawingPanel.MouseDown += MouseFire;
            drawingPanel.MouseUp += MouseCancel;

            ClientSize = new Size(viewSize, viewSize + menuSize);

            //drawingPanel.MouseMove += gController.HandleMousePosition;

            //drawingPanel.MouseDown += gController.HandleMouseRequest;
            //drawingPanel.MouseUp += gController.CancelMouseRequest;
        }



        private void MessageBoxForError(string str)
        {
            MessageBox.Show(str);
        }

        private void WorldUpdate()
        {
            Invoke(new MethodInvoker(() => this.Invalidate(true)));
        }

        private void serverTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && (serverTextBox.Text !="" && nameTextBox.Text != ""))
            {
                //if both of them have something in it.
                gController.ConnectToServer(serverTextBox.Text, nameTextBox.Text);
                drawingPanel.Focus();
            }
         
        }

        private void nameTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && (serverTextBox.Text != "" && nameTextBox.Text != ""))
            {
                gController.ConnectToServer(serverTextBox.Text, nameTextBox.Text);
                drawingPanel.Focus();
            }
          

        }

        private void TranslateKeyPress(object sender, KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
              
                case Keys.W:
                    gController.HandleMoveRequest(GameController.MovementDirection.UP);
                    break;
                case Keys.S:
                    gController.HandleMoveRequest(GameController.MovementDirection.DOWN);
                    break;
                case Keys.A:
                    gController.HandleMoveRequest(GameController.MovementDirection.LEFT);
                    break;
                case Keys.D:
                    gController.HandleMoveRequest(GameController.MovementDirection.RIGHT);
                    break;
            }
        }

        private void TranslateKeyUp(object sender, KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                case Keys.W:
                    gController.CancelMoveRequest(GameController.MovementDirection.UP);
                    break;
                case Keys.S:
                    gController.CancelMoveRequest(GameController.MovementDirection.DOWN);
                    break;
                case Keys.A:
                    gController.CancelMoveRequest(GameController.MovementDirection.LEFT);
                    break;
                case Keys.D:
                    gController.CancelMoveRequest(GameController.MovementDirection.RIGHT);
                    break;
            }
        }

        private void MouseMovement(object sender, MouseEventArgs e)
        {
            
            gController.MouseMovementRequest(e.X-(viewSize/2), e.Y-(viewSize/2));
        }

        private void MouseFire(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    gController.HandleMouseRequest(GameController.MouseClickRequest.main);
                    break;
                case MouseButtons.Right:
                    gController.HandleMouseRequest(GameController.MouseClickRequest.alt);
                    break;
            }
        }

        private void MouseCancel(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    gController.MouseCancelRequest(GameController.MouseClickRequest.main);
                    break;
                case MouseButtons.Right:
                    gController.MouseCancelRequest(GameController.MouseClickRequest.alt);
                    break;

            }
        }

        private void OnDeath(object dead)
        {
            if (dead is Tank t)
            {
                drawingPanel.OnTankDeath(t);
            }
            else if (dead is Powerup pu)
            {

            }
            else if (dead is Projectile pr)
            {

            }
            else if (dead is Beam b)
            {
                drawingPanel.OnBeamArrive(b);
            }
        }
    }
}
