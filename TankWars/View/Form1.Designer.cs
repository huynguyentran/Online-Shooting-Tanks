
namespace View
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.serverTextBox = new System.Windows.Forms.TextBox();
            this.serverLable = new System.Windows.Forms.Label();
            this.nameLable = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(63, 9);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(100, 22);
            this.nameTextBox.TabIndex = 0;
            this.nameTextBox.TextChanged += new System.EventHandler(this.nameTextBox_TextChanged);
            // 
            // serverTextBox
            // 
            this.serverTextBox.Location = new System.Drawing.Point(241, 9);
            this.serverTextBox.Name = "serverTextBox";
            this.serverTextBox.Size = new System.Drawing.Size(100, 22);
            this.serverTextBox.TabIndex = 1;
            this.serverTextBox.Text = "localhost";
            this.serverTextBox.TextChanged += new System.EventHandler(this.serverTextBox_TextChanged);
            // 
            // serverLable
            // 
            this.serverLable.AutoSize = true;
            this.serverLable.Location = new System.Drawing.Point(169, 12);
            this.serverLable.Name = "serverLable";
            this.serverLable.Size = new System.Drawing.Size(66, 17);
            this.serverLable.TabIndex = 2;
            this.serverLable.Text = "Server/IP";
            // 
            // nameLable
            // 
            this.nameLable.AutoSize = true;
            this.nameLable.Location = new System.Drawing.Point(12, 12);
            this.nameLable.Name = "nameLable";
            this.nameLable.Size = new System.Drawing.Size(45, 17);
            this.nameLable.TabIndex = 3;
            this.nameLable.Text = "Name";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.nameLable);
            this.Controls.Add(this.serverLable);
            this.Controls.Add(this.serverTextBox);
            this.Controls.Add(this.nameTextBox);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.TextBox serverTextBox;
        private System.Windows.Forms.Label serverLable;
        private System.Windows.Forms.Label nameLable;
    }
}

