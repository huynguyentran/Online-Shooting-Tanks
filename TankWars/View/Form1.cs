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
        private const int viewSize = 500;

        public Form1(GameController _gController)
        {
            InitializeComponent();

            gController = _gController;
            gController.AddErrorHandler(MessageBoxForError);

            gController.updateView += WorldUpdate;

            DrawingPanel drawingPanel;
            drawingPanel = new  DrawingPanel(gController.world);
            drawingPanel.Location = new Point(0, menuSize);
            drawingPanel.Size = new Size(viewSize, viewSize);
            this.Controls.Add(drawingPanel);

        }



        private void MessageBoxForError(string str)
        {
            MessageBox.Show(str);
        }

        private void WorldUpdate()
        {
            Invoke(new MethodInvoker(() => this.Invalidate(true)));
        }

        class DrawingPanel : Panel
        {
            private readonly Model theMap;
            public DrawingPanel(Model w)
            {
                DoubleBuffered = true;
                theMap = w;
            }


        }

        private void serverTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && (serverTextBox.Text !="" && nameTextBox.Text != ""))
            {
                //if both of them have something in it.
                gController.ConnectToServer(serverTextBox.Text, nameTextBox.Text);

            }
        }

        private void nameTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && (serverTextBox.Text != "" && nameTextBox.Text != ""))
            {
                gController.ConnectToServer(serverTextBox.Text, nameTextBox.Text);
            }
        }
    }
}
