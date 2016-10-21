using System;
using System.Drawing;
using System.Windows.Forms;

namespace hjn20160520._2_Cashiers
{
    public partial class VipCZYEForm : Form
    {
        public delegate void VipCZYEFormHandle(decimal CZYE, decimal KJYE, decimal FQJE, decimal FQSU, decimal YFDJ, bool ZSCZ = false);
        public event VipCZYEFormHandle changed;  //传递储值事件，充值，扣减，分期，定金

        private decimal FQJE = 0;  //分期金额

        SysQxValiForm ValiForm = new SysQxValiForm();  //权限检证窗口
        bool isVali = false; //是否验证通过

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

            checkBox1.Checked = false;
            isVali = false;
            ValiForm.Changed += ValiForm_Changed;
        }

        void ValiForm_Changed(bool isValied)
        {
            isVali = isValied;
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
                bool isZS = checkBox1.Checked;
                //如果是赠送要验证权限
                if (isZS)
                {
                    ValiForm.ShowDialog();
                    if (isVali == false) return;
                }

                decimal CZYE = 0.00m;    //充值
                decimal KJYE = 0.00m;    //扣减
                decimal FQJE = 0.00m;    //分期
                decimal FQSU = 0.00m;    //分期
                decimal YFDJ = 0.00m;    //定金

                if (!string.IsNullOrEmpty(this.textBox1.Text.Trim()))
                {
                    CZYE = Convert.ToDecimal(this.textBox1.Text.Trim());
                }

                if (!string.IsNullOrEmpty(this.textBox2.Text.Trim()))
                {
                    KJYE = Convert.ToDecimal(this.textBox2.Text.Trim());
                }

                if (!string.IsNullOrEmpty(this.textBox3.Text.Trim()))
                {
                    FQJE = Convert.ToDecimal(this.textBox3.Text.Trim());
                }

                if (!string.IsNullOrEmpty(this.textBox4.Text.Trim()))
                {
                    FQSU = Convert.ToDecimal(this.textBox4.Text.Trim());
                }

                if (!string.IsNullOrEmpty(this.textBox5.Text.Trim()))
                {
                    YFDJ = Convert.ToDecimal(this.textBox5.Text.Trim());
                }

                changed(CZYE, KJYE, FQJE, FQSU, YFDJ, isZS);
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
                    //不允许负数
                    if (jetemp < 0)
                    {
                        textBox3.Text = "0";
                        jetemp = 0;
                    }

                    //不允许少于1
                    if (FQ < 1)
                    {
                        textBox4.Text = "1";
                        FQ = 1;
                    }

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

        //只能输入数字且正数
        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 0x20) e.KeyChar = (char)0;  //禁止空格键  
            if ((e.KeyChar == 0x2D) && (((TextBox)sender).Text.Length == 0)) return;   //处理负数  
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

        //禁止输入无意义的负数与0
        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            decimal temp = 0;
            decimal.TryParse(textBox5.Text.Trim(), out temp);
            if (temp <= 0)
            {
                textBox5.Text = "";
            }
        }

        //禁止输入无意义的负数与0
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            decimal temp = 0;
            decimal.TryParse(textBox2.Text.Trim(), out temp);
            if (temp <= 0)
            {
                textBox2.Text = "";
            }

            if (string.IsNullOrEmpty(textBox2.Text.Trim()))
            {
                textBox1.Enabled = true;
                textBox1.BackColor = Color.WhiteSmoke;

            }
            else
            {
                textBox1.BackColor = Color.LightGray;
                textBox1.Enabled = false;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            decimal temp = 0;
            decimal.TryParse(textBox1.Text.Trim(), out temp);
            if (temp <= 0)
            {
                textBox1.Text = "";
            }

            if (string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                textBox2.Enabled = true;
                textBox2.BackColor = Color.WhiteSmoke;

            }
            else
            {
                textBox2.BackColor = Color.LightGray;
                textBox2.Enabled = false;

            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                textBox3.Text = "";
                textBox5.Text = "";
                label7.Text = "";
                textBox3.BackColor = Color.LightGray;
                textBox4.BackColor = Color.LightGray;
                textBox5.BackColor = Color.LightGray;
                textBox3.Enabled = false;
                textBox4.Enabled = false;
                textBox5.Enabled = false;
            }
            else
            {
                textBox3.Enabled = true;
                textBox4.Enabled = true;
                textBox5.Enabled = true;
                textBox3.BackColor = Color.WhiteSmoke;
                textBox4.BackColor = Color.WhiteSmoke;
                textBox5.BackColor = Color.WhiteSmoke;
            }
        }

        private void VipCZYEForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ValiForm.Changed -= ValiForm_Changed;

        }




    }
}
