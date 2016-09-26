using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace hjn20160520.Common
{
    /// <summary>
    /// 密码输入窗口
    /// </summary>
    public partial class InputBoxForm : Form
    {

        public delegate void InputBoxFormHandle(string PW);
        public event InputBoxFormHandle changed;
        

        public InputBoxForm()
        {
            InitializeComponent();
        }

        private void InputBoxForm_Load(object sender, EventArgs e)
        {
            this.textBox1.Focus();
            this.textBox1.Text = "";
        }

        private void InputBoxForm_KeyDown(object sender, KeyEventArgs e)
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
            if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                changed(textBox1.Text.Trim());
                this.Close();
            }

        }



        private void button1_Click(object sender, EventArgs e)
        {
            OKFunc();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsNumber(e.KeyChar)) && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }









    }
}
