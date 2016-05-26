using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hjn20160520
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
           
        }


        #region 2前台收银
        private void button2_Enter(object sender, EventArgs e)
        {
            this.label2.Visible = true;
            ((Button)sender).BackColor = Color.Gold;
        }

        private void button2_Leave(object sender, EventArgs e)
        {
            this.label2.Visible = false;
            ((Button)sender).BackColor = Color.White;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form1 f1 = new Form1();
            f1.Show();
            this.Hide();

        }

        #endregion

        #region 1前台交班
        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Enter(object sender, EventArgs e)
        {
            this.label1.Visible = true;
            this.button1.BackColor = Color.Gold;
        }

        private void button1_Leave(object sender, EventArgs e)
        {
            this.label1.Visible = false;
            this.button1.BackColor = Color.White;
        }
        #endregion

        #region 3前台当班
        private void button3_Click(object sender, EventArgs e)
        {

        }


        private void button3_Leave(object sender, EventArgs e)
        {
            this.label3.Visible = false;
            this.button3.BackColor = Color.White;
        }

        private void button3_Enter(object sender, EventArgs e)
        {
            this.label3.Visible = true;
            this.button3.BackColor = Color.Gold;
        }
        #endregion









    }
}
