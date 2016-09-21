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
    public partial class VipCZJFForm : Form
    {


        public VipCZJFForm()
        {
            InitializeComponent();
        }


        private void VipCZJFForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.Close();
                    break;

                case Keys.Enter:

                    if (!string.IsNullOrEmpty(this.textBox3.Text.Trim()) && !string.IsNullOrEmpty(this.textBox4.Text.Trim()))
                    {
                        MessageBox.Show("不允许同时进行积分储值与扣减");
                    }
                    else
                    {
                        MemberPointsForm frm1 = (MemberPointsForm)this.Owner;
                        frm1.CZJF = this.textBox3.Text.Trim();
                        frm1.KJJF = this.textBox4.Text.Trim();
                        frm1.VipJFFunc();
                        this.Close();
                    }

                    break;
            }
        }

        private void VipCZJFForm_Load(object sender, EventArgs e)
        {

        }


        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' && e.KeyChar != '.' || e.KeyChar > '9' && e.KeyChar != '.' || ((TextBox)(sender)).Text.IndexOf('.') >= 0 && e.KeyChar == '.') && e.KeyChar != (char)13 && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' && e.KeyChar != '.' || e.KeyChar > '9' && e.KeyChar != '.' || ((TextBox)(sender)).Text.IndexOf('.') >= 0 && e.KeyChar == '.') && e.KeyChar != (char)13 && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.textBox3.Text.Trim()) && !string.IsNullOrEmpty(this.textBox4.Text.Trim()))
            {
                MessageBox.Show("不允许同时进行积分储值与扣减");
            }
            else
            {
                MemberPointsForm frm1 = (MemberPointsForm)this.Owner;
                frm1.CZJF = this.textBox3.Text.Trim();
                frm1.KJJF = this.textBox4.Text.Trim();
                frm1.VipJFFunc();
                this.Close();
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            decimal temp = 0;
            decimal.TryParse(textBox3.Text.Trim(), out temp);
            if (temp <= 0)
            {
                textBox3.Text = "";
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            decimal temp = 0;
            decimal.TryParse(textBox4.Text.Trim(), out temp);
            if (temp <= 0)
            {
                textBox4.Text = "";
            }
        }






    }
}
