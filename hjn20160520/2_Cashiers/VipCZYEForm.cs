using System;
using System.Windows.Forms;

namespace hjn20160520._2_Cashiers
{
    public partial class VipCZYEForm : Form
    {
        public delegate void VipCZYEFormHandle(string CZYE, string KJYE, string FQJE, string FQSU, string YFDJ);
        public event VipCZYEFormHandle changed;  //传递储值事件，充值，扣减，分期，定金

        private decimal FQJE = 0;  //分期金额

        public VipCZYEForm()
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

        private void VipCZYEForm_Load(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox5.Clear();
        }

        //只能输入小数点与数字
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' && e.KeyChar != '.' || e.KeyChar > '9' && e.KeyChar != '.' || ((TextBox)(sender)).Text.IndexOf('.') >= 0 && e.KeyChar == '.') && e.KeyChar != (char)13 && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }

        private void VipCZYEForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.Close();
                    break;

                case Keys.Enter:
                    EnterFunc();
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            EnterFunc();
        }


        //回车
        private void EnterFunc()
        {
            if (!string.IsNullOrEmpty(this.textBox1.Text.Trim()) && !string.IsNullOrEmpty(this.textBox2.Text.Trim()))
            {
                MessageBox.Show("不允许同时进行余额储值与扣减");
            }
            else
            {
                string CZYE = this.textBox1.Text.Trim();  //充值
                string KJYE = this.textBox2.Text.Trim();  //扣减
                string FQJE = this.textBox3.Text.Trim();  //分期金额
                string FQSU = this.textBox4.Text.Trim();  //分期数
                string YFDJ = this.textBox5.Text.Trim();  //定金

                changed(CZYE, KJYE, FQJE, FQSU, YFDJ);
                this.Close();
            }
        }

        //分期金额改变事件
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox3.Text.Trim()) && !string.IsNullOrEmpty(textBox4.Text.Trim()))
            {
                decimal jetemp = 0, FQ = 0;
                if (decimal.TryParse(textBox3.Text.Trim(), out jetemp) && decimal.TryParse(textBox4.Text.Trim(), out FQ))
                {
                    this.FQJE = Math.Round(jetemp / FQ, 2);
                    label7.Text = FQJE.ToString() + " 元";
                }
            }
        }

        //分期数改变事件
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox3.Text.Trim()) && !string.IsNullOrEmpty(textBox4.Text.Trim()))
            {
                decimal jetemp = 0, FQ = 0;
                if (decimal.TryParse(textBox3.Text.Trim(), out jetemp) && decimal.TryParse(textBox4.Text.Trim(), out FQ))
                {
                    this.FQJE = Math.Round(jetemp / FQ, 2);
                    label7.Text = FQJE.ToString() + " 元";
                }
            }
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

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' && e.KeyChar != '.' || e.KeyChar > '9' && e.KeyChar != '.' || ((TextBox)(sender)).Text.IndexOf('.') >= 0 && e.KeyChar == '.') && e.KeyChar != (char)13 && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }




    }
}
