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
    public partial class MobilePayForm : Form
    {
        //微信与支付宝 传递金额
        public delegate void MobilePayFormHandle(decimal weixun, decimal zfb, string weixunStr, string zfbStr);
        public event MobilePayFormHandle changed;

        ClosingEntries CE;
        decimal CEJE = 0.00m; //应付金额


        public MobilePayForm()
        {
            InitializeComponent();
        }

        private void MobilePayForm_Load(object sender, EventArgs e)
        {
            CE = this.Owner as ClosingEntries;
            this.CEJE = CE.CETotalMoney;
            this.label10.Text = CEJE.ToString("0.00");

            this.ActiveControl = this.textBox1;
            this.textBox1.Focus();
            this.textBox1.Text = "";
            this.textBox2.Text = "";

            if (string.IsNullOrEmpty(CE.weixunStr))
            {
                this.textBox3.Text = CE.weixunStr;

            }
            else
            {
                this.textBox3.Text = "";
            }

            if (string.IsNullOrEmpty(CE.zfbStr))
            {
                this.textBox4.Text = CE.zfbStr;

            }
            else
            {
                this.textBox4.Text = "";
            }

        }

        private void MobilePayForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    OKFunc();
                    break;

                case Keys.Escape:
                    this.Close();
                    break;

                case Keys.F1:
                    textBox2.Text = "";

                    textBox1.Text = CEJE.ToString();
                    break;

                case Keys.F2:
                    textBox1.Text = "";

                    textBox2.Text = CEJE.ToString();
                    break;
            }
        }


        //提交
        private void OKFunc()
        {
            //支付宝
            decimal zfbtemp = 0.00m;
            if (!string.IsNullOrEmpty(textBox2.Text.Trim()))
            {
                if (!decimal.TryParse(textBox2.Text.Trim(), out zfbtemp))
                {
                    MessageBox.Show("银行返现金额输入有误，请重新输入！");
                    return;
                }
            }

            //微信 
            decimal weixuntemp = 0.00m;
            if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                if (!decimal.TryParse(textBox1.Text.Trim(), out weixuntemp))
                {
                    MessageBox.Show("银行返现金额输入有误，请重新输入！");
                    return;
                }
            }


            //微信账号
            string weixunstr = "没有录入";
            if (!string.IsNullOrEmpty(textBox3.Text.Trim()))
            {
                weixunstr = textBox3.Text.Trim();
            }

            //支付宝账号
            string zfbstr = "没有录入";
            if (!string.IsNullOrEmpty(textBox4.Text.Trim()))
            {
                zfbstr = textBox4.Text.Trim();
            }

            changed(weixuntemp, zfbtemp, weixunstr, zfbstr);

            this.Close();


        }



        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                decimal cetemp = CEJE;

                if (!string.IsNullOrEmpty(textBox1.Text.Trim()) && !string.IsNullOrEmpty(textBox2.Text.Trim()))
                {
                    decimal temp = 0;
                    if (decimal.TryParse(textBox1.Text.Trim(), out temp))
                    {
                        decimal temp2 = 0;
                        if (decimal.TryParse(textBox2.Text.Trim(), out temp2))
                        {
                            cetemp -= temp2;
                        }

                        cetemp -= temp;

                        if (cetemp < 0)
                        {
                            MessageBox.Show("应付金额不允许小于0，请重新输入！");
                            textBox1.Text = "";
                            return;
                        }
                    }
                }

                else if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
                {
                    decimal temp2 = 0;
                    if (decimal.TryParse(textBox1.Text.Trim(), out temp2))
                    {
                        cetemp -= temp2;
                        if (cetemp < 0)
                        {
                            MessageBox.Show("应付金额不允许小于0，请重新输入！");
                            textBox1.Text = "";
                            return;
                        }
                    }
                }

                else if (!string.IsNullOrEmpty(textBox2.Text.Trim()))
                {
                    decimal temp2 = 0;
                    if (decimal.TryParse(textBox2.Text.Trim(), out temp2))
                    {
                        cetemp -= temp2;
                    }
                }

                label10.Text = cetemp.ToString("0.00");
            }
            catch
            {

            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                decimal cetemp = CEJE;

                if (!string.IsNullOrEmpty(textBox1.Text.Trim()) && !string.IsNullOrEmpty(textBox2.Text.Trim()))
                {
                    decimal temp = 0;
                    if (decimal.TryParse(textBox2.Text.Trim(), out temp))
                    {
                        decimal temp2 = 0;
                        if (decimal.TryParse(textBox1.Text.Trim(), out temp2))
                        {
                            cetemp -= temp2;
                        }

                        cetemp -= temp;

                        if (cetemp < 0)
                        {
                            MessageBox.Show("应付金额不允许小于0，请重新输入！");
                            textBox2.Text = "";
                            return;
                        }
                    }
                }

                else if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
                {
                    decimal temp2 = 0;
                    if (decimal.TryParse(textBox1.Text.Trim(), out temp2))
                    {
                        cetemp -= temp2;

                    }
                }

                else if (!string.IsNullOrEmpty(textBox2.Text.Trim()))
                {
                    decimal temp2 = 0;
                    if (decimal.TryParse(textBox2.Text.Trim(), out temp2))
                    {
                        cetemp -= temp2;
                    }

                    if (cetemp < 0)
                    {
                        MessageBox.Show("应付金额不允许小于0，请重新输入！");
                        textBox2.Text = "";
                        return;
                    }
                }

                label10.Text = cetemp.ToString("0.00");
            }
            catch
            {

            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' && e.KeyChar != '.' || e.KeyChar > '9' && e.KeyChar != '.' || ((TextBox)(sender)).Text.IndexOf('.') >= 0 && e.KeyChar == '.') && e.KeyChar != (char)13 && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' && e.KeyChar != '.' || e.KeyChar > '9' && e.KeyChar != '.' || ((TextBox)(sender)).Text.IndexOf('.') >= 0 && e.KeyChar == '.') && e.KeyChar != (char)13 && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OKFunc();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";

            textBox1.Text = CEJE.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";

            textBox2.Text = CEJE.ToString();
        }













    }
}
