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
        MemberPointsForm MPFrom;
        decimal VipQKJE = 0; //变量的
        decimal qkje = 0; //不变的

        public delegate void VipHuangKuanFormHandle(decimal je);
        public event VipHuangKuanFormHandle changed;  //传递还款金额

        public VipHuangKuanForm()
        {
            InitializeComponent();
        }

        private void VipHuangKuanForm_Load(object sender, EventArgs e)
        {
            MPFrom = this.Owner as MemberPointsForm;
            this.VipQKJE = this.qkje = MPFrom.VipQKJE;
            this.label5.Text = this.VipQKJE.ToString("0.00");
            this.textBox1.Text = "";
            this.textBox1.Focus();

        }

        private void VipHuangKuanForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.Close();
                    break;

                case Keys.Enter:
                    OKFunc();
                    break;
                case Keys.F1:
                    alltorepay();
                    break;
            }
        }

        private void alltorepay()
        {
            this.textBox1.Text = qkje.ToString("0.00");
            this.textBox1.Focus();
            this.textBox1.SelectAll();
        }


        private void button2_Click(object sender, EventArgs e)
        {
            alltorepay();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OKFunc();
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
            this.VipQKJE = MPFrom.VipQKJE;
            if (string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                this.label5.Text = this.VipQKJE.ToString("0.00");
            }
            else
            {
                decimal temp = 0;
                if (decimal.TryParse(textBox1.Text.Trim(), out temp))
                {
                    this.VipQKJE -= temp;
                    this.label5.Text = this.VipQKJE.ToString("0.00");
                }
            }

        }


        private void OKFunc()
        {
            if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                decimal temp = 0;
                if (decimal.TryParse(textBox1.Text.Trim(), out temp))
                {
                    if (this.VipQKJE < 0)
                    {
                        MessageBox.Show("会员欠款金额不可以小于零！请检查是否输入正确？");
                    }
                    else
                    {
                        changed(temp);
                        this.Close();
                    }

                }
                else
                {
                    MessageBox.Show("您输入的金额不正确！请重新输入");
                }
            }
        }








    }
}
