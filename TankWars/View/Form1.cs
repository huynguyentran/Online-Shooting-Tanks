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
        private const int viewSize = 1000;
        private DrawingPanel drawingPanel;
        public Form1(GameController _gController)
        {
            InitializeComponent();

            gController = _gController;
            gController.AddErrorHandler(MessageBoxForError);

            gController.updateView += WorldUpdate;

         
            drawingPanel = new  DrawingPanel(gController.world);
            drawingPanel.Location = new Point(0, menuSize);
            drawingPanel.Size = new Size(viewSize, viewSize);
            this.Controls.Add(drawingPanel);

            //Handling Inputs
            //this.KeyDown += gController.HandleMoveRequest;
            drawingPanel.KeyDown += TranslateKeyPress;
            //this.KeyUp += gController.CancelMoveRequest;

     
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
    }
}
