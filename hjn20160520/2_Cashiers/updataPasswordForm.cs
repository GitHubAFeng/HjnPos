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
    /// 会员密码修改窗口
    /// </summary>
    public partial class updataPasswordForm : Form
    {
        //传递原密码做验证，与新密码
        public delegate void updataPasswordFormHandle(string oldPW, string updataPW);
        public event updataPasswordFormHandle changed;


        public updataPasswordForm()
        {
            InitializeComponent();
        }

        private void updataPasswordForm_Load(object sender, EventArgs e)
        {
            this.textBox1.Focus();
            this.textBox1.Text = "";
            this.textBox2.Text = "";
        }

        private void updataPasswordForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.Close();
                    break;

                case Keys.Enter:
                    OKFunc();
                    break;
            }
        }

        private void OKFunc()
        {
            changed(textBox1.Text.Trim(),textBox2.Text.Trim());

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OKFunc();
        }

        /// <summary>
        /// 只能输入数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsNumber(e.KeyChar)) && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsNumber(e.KeyChar)) && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }











    }
}
