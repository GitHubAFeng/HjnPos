using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace hjn20160520._2_Cashiers
{

    /// <summary>
    /// 会员还款
    /// </summary>
    public partial class VipHuangKuanForm : Form
    {
        public delegate void VipHuangKuanFormHandle(decimal je);
        public event VipHuangKuanFormHandle changed;  //传递还款金额

        public VipHuangKuanForm()
        {
            InitializeComponent();
        }

        private void VipHuangKuanForm_Load(object sender, EventArgs e)
        {

        }

        private void VipHuangKuanForm_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' && e.KeyChar != '.' || e.KeyChar > '9' && e.KeyChar != '.' || ((TextBox)(sender)).Text.IndexOf('.') >= 0 && e.KeyChar == '.') && e.KeyChar != (char)13 && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }


        private void OKFunc()
        {
            if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                decimal temp = 0;
                if (decimal.TryParse(textBox1.Text.Trim(), out temp))
                {
                    changed(temp);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("您输入的金额不正确！请重新输入");
                }
            }
        }








    }
}
