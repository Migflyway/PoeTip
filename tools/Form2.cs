using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tools
{
    public partial class Form2 : Form
    {
        static int ScreenWidth;
        static int ScreenHeight;

        public Form2()
        {
            InitializeComponent();

            this.TopMost = true;
            ScreenWidth = Screen.PrimaryScreen.Bounds.Width;
            ScreenHeight = Screen.PrimaryScreen.Bounds.Height;
        }

        public void Show(string text)
        {
            this.label1.Text = text;
            //this.label1.ForeColor = Color.White;

            this.Width = this.label1.Width;
            this.Height = this.label1.Height+6;
            int x = (this.Width + MousePosition.X) > ScreenWidth? MousePosition.X - this.Width : MousePosition.X;
            int y = (this.Height + MousePosition.Y+30) > ScreenHeight ? MousePosition.Y - this.Height - 10: MousePosition.Y + 30;

            this.Location = new Point(x, y);
            this.Show();
        }

        private void Form2_MouseEnter(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void label1_MouseEnter(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
