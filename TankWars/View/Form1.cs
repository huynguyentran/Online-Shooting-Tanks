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

namespace View
{
    public partial class Form1 : Form
    {
        private GameController gController;
        public Form1()
        {
            InitializeComponent();

            gController = new GameController(messageBoxForError);

        }

        private void serverTextBox_TextChanged(object sender, EventArgs e)
        {
            if (KeyPress += Keys.Enter)
            {
                //if both of them have something in it.
                gController.connectToServer(serverTextBox.Text, nameTextBox.Text);

            }
        }

        private void nameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (KeyPress += Keys.Enter)
            {
                gController.connectToServer(serverTextBox.Text, nameTextBox.Text);
            }
        }

        private void messageBoxForError(string str)
        {
            MessageBox.Show(str);
        }
    }
}
