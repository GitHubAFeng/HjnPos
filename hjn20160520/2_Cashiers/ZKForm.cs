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
    /// <summary>
    /// 单品折扣
    /// </summary>
    public partial class ZKForm : Form
    {
        //用于其它窗口传值给本窗口控件
        //这是委托与事件的第一步  
        public delegate void ZKDPHandle(decimal d);
        public event ZKDPHandle changed;  
        public ZKForm()
        {
            InitializeComponent();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' && e.KeyChar != '.' || e.KeyChar > '9' && e.KeyChar != '.' || ((TextBox)(sender)).Text.IndexOf('.') >= 0 && e.KeyChar == '.') && e.KeyChar != (char)13 && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }

        private void ZKForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.Close();
                    break;
                case Keys.Enter:
                    ZKLFunc();
                    this.Close();

                    break;
            }
        }


        //折扣
        private void ZKLFunc()
        {
            decimal zkl = 0.00m;
            if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                zkl = Convert.ToDecimal(textBox1.Text.Trim());
            }
            if (zkl != 0)
            {
                //Cashiers.GetInstance.ZKDP_temp = zkl;
                changed(zkl);
            }
        }

        private void ZKForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ZKLFunc();
            this.Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            decimal temp = 0;
            decimal.TryParse(textBox1.Text.Trim(), out temp);
            if (temp <= 0)
            {
                textBox1.Text = "";
            }
        }


    }
}
