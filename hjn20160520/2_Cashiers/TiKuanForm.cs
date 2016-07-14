using hjn20160520.Models;
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
    public partial class TiKuanForm : Form
    {
        public TiKuanForm()
        {
            InitializeComponent();
        }

        private void TiKuanForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.Close();
                    break;
                case Keys.Enter:
                    TiKuanFunc();
                    break;
            }
        }


        private void TiKuanFunc()
        {
            if (string.IsNullOrEmpty(textBox1.Text.Trim())) return;
            decimal tikuan = Convert.ToDecimal(textBox1.Text.Trim());
            if (tikuan > HandoverModel.GetInstance.Money)
            {
                MessageBox.Show("钱箱余额不足！");
                textBox1.Text = HandoverModel.GetInstance.Money.ToString();
                textBox1.SelectAll();
            }
            else
            {
                HandoverModel.GetInstance.Money -= tikuan;
                HandoverModel.GetInstance.DrawMoney += tikuan;
                MessageBox.Show("提款成功，已提" + tikuan.ToString() + "元");
            }

        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' && e.KeyChar != '.' || e.KeyChar > '9' && e.KeyChar != '.' || ((TextBox)(sender)).Text.IndexOf('.') >= 0 && e.KeyChar == '.') && e.KeyChar != (char)13 && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }

    }
}
