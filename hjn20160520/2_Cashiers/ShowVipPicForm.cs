using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hjn20160520._2_Cashiers
{
    public partial class ShowVipPicForm : Form
    {
        public ShowVipPicForm()
        {

            InitializeComponent();
        }

        private void ShowVipPicForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    this.Close();
                    break;
                case Keys.Escape:
                    this.Close();
                    break;
            }
        }

        private void ShowVipPicForm_Load(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image = CashiersFormXP.GetInstance.pic;
                pictureBox1.Show();
            }
            
        }







    }
}
