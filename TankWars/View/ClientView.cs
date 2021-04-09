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
using Model;

namespace View
{
    /// <summary>
    /// The view of the 2D Tank Wars game. 
    /// </summary>
    /// <author>Huy Nguyen</author>
    /// <author>William Erignac</author>
    /// <version>04/09/2021</version>
    public partial class ClientView : Form
    {
        /// <summary>
        /// The controller communicates between model, server, and the view. 
        /// </summary>
        private GameController gameController;

        /// <summary>
        /// Constant size of the game window and menu.
        /// </summary>
        private const int menuSize = 40;
        private const int viewSize = 900;

        /// <summary>
        /// The drawing panel object that handles all drawing. 
        /// </summary>
        private DrawingPanel drawingPanel;
        public ClientView(GameController _gController)
        {
            InitializeComponent();

            gameController = _gController;

            //An event that comes from the Controller to notify the view of the error. 
            gameController.AddErrorHandler(MessageBoxForError);

            //Events that update the view on what happens in the game. 
            gameController.updateView += WorldUpdate;
            gameController.deathEvent += OnDeath;

            //Initializing drawing panel object. 
            drawingPanel = new DrawingPanel(gameController.world);
            drawingPanel.Location = new Point(0, menuSize);
            drawingPanel.Size = new Size(viewSize, viewSize);
            this.Controls.Add(drawingPanel);

            //Registering events for movement of the client tank. 
            drawingPanel.KeyDown += TranslateKeyPress;
            drawingPanel.KeyUp += TranslateKeyUp;
            drawingPanel.MouseMove += MouseMovement;

            //Registering events for attacks of the client tank. 
            drawingPanel.MouseDown += MouseFire;
            drawingPanel.MouseUp += MouseCancel;

            //Creating the windom form. 
            ClientSize = new Size(viewSize, viewSize + menuSize);
        }

        /// <summary>
        /// A message box that shows the client the error. 
        /// </summary>
        /// <param name="str">Errors</param>
        private void MessageBoxForError(string str)
        {
            MessageBox.Show(str,"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


   
        /// <summary>
        /// Updating the world on every frame. 
        /// </summary>
        private void WorldUpdate()
        {
            Invoke(new MethodInvoker(() => this.Invalidate(true)));
            gameController.OnNewFrame();
        }

        /// <summary>
        /// Register the client key. The client press Enter to connect to the server.
        /// Indicating errors if one of the message box is empty. 
        /// </summary>
        private void serverTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (serverTextBox.Text != "" && nameTextBox.Text != "")
                {
                    gameController.ConnectToServer(serverTextBox.Text, nameTextBox.Text);
                    drawingPanel.Focus();
                    e.Handled = true;
                    serverTextBox.Enabled = false;
                    nameTextBox.Enabled = false;
                }
                else
                {
                    MessageBoxForError("Can not have empty name or server address.");
                }
            }

        }

        /// <summary>
        /// Register the client key. The client press Enter to connect to the server.
        /// Indicating errors if one of the message box is empty. 
        /// </summary>
        private void nameTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            serverTextBox_KeyPress(sender, e);
        }

        /// <summary>
        /// Register the client keys to send to the controler for the client tank movements. 
        /// </summary>
        private void TranslateKeyPress(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    gameController.HandleMoveRequest(GameController.MovementDirection.UP);
                    break;
                case Keys.S:
                    gameController.HandleMoveRequest(GameController.MovementDirection.DOWN);
                    break;
                case Keys.A:
                    gameController.HandleMoveRequest(GameController.MovementDirection.LEFT);
                    break;
                case Keys.D:
                    gameController.HandleMoveRequest(GameController.MovementDirection.RIGHT);
                    break;
            }
        }

        /// <summary>
        /// Register the client keys to request canceling the tank movement. 
        /// </summary>
        private void TranslateKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    gameController.CancelMoveRequest(GameController.MovementDirection.UP);
                    break;
                case Keys.S:
                    gameController.CancelMoveRequest(GameController.MovementDirection.DOWN);
                    break;
                case Keys.A:
                    gameController.CancelMoveRequest(GameController.MovementDirection.LEFT);
                    break;
                case Keys.D:
                    gameController.CancelMoveRequest(GameController.MovementDirection.RIGHT);
                    break;
            }
           
        }

        /// <summary>
        /// Register the client mouse movement for the rotation of the client turret. 
        /// </summary>
        private void MouseMovement(object sender, MouseEventArgs e)
        {

            gameController.MouseMovementRequest(e.X - (viewSize / 2), e.Y - (viewSize / 2));
        }

        /// <summary>
        /// Register the client key for the attack of the client tank. 
        /// </summary>
        private void MouseFire(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    gameController.HandleMouseRequest(GameController.MouseClickRequest.main);
                    break;
                case MouseButtons.Right:
                    gameController.HandleMouseRequest(GameController.MouseClickRequest.alt);
                    break;
            }
        }

        /// <summary>
        /// Register the client keys to request canceling the client tank attack. 
        /// </summary>
        private void MouseCancel(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    gameController.MouseCancelRequest(GameController.MouseClickRequest.main);
                    break;
                case MouseButtons.Right:
                    gameController.MouseCancelRequest(GameController.MouseClickRequest.alt);
                    break;
            }
        }

        /// <summary>
        /// Notify the drawing pannel if an object has "died". 
        /// </summary>
        /// <param name="dead"></param>
        private void OnDeath(object dead)
        {
            if (dead is Tank t)
            {
                drawingPanel.OnTankDeath(t);
            }
            else if (dead is Beam b)
            {
                drawingPanel.OnBeamArrive(b);
            }
        }

        /// <summary>
        /// Call the controller to clear all movements.
        /// </summary>
        private void ClientView_Deactivate(object sender, EventArgs e)
        {
            gameController.ClearAllMovement();
        }
    }
}
