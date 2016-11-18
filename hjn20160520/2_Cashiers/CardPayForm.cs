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
    /// 银联卡付款
    /// </summary>
    public partial class CardPayForm : Form
    {
        ClosingEntries CE;
        //这是委托与事件的第一步  ,卡号，支付金额,返现，折扣
        public delegate void CardPayFormHandle(string card,decimal payje,decimal reje,decimal rezk);
        public event CardPayFormHandle changed;

        decimal CEJE = 0.00m; //应付金额
        string tempStr = "";

        public CardPayForm()
        {
            InitializeComponent();
        }

        private void CardPayForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    //CE.label5.Text = tempStr;

                    this.Close();
                    break;

                case Keys.Enter:
                    OKFunc();
                    break;
            }
        }

        private void CardPayForm_Load(object sender, EventArgs e)
        {
            CE = this.Owner as ClosingEntries;
            tempStr = CE.label5.Text;
            this.CEJE = CE.CETotalMoney;
            this.label10.Text = CEJE.ToString("0.00");
            CE.label5.Text = "银联卡";
            textBox1.Focus();
            textBox1.SelectAll();

            this.textBox1.Text = "";
            this.textBox2.Text = "";
            this.textBox3.Text = "";
            this.textBox4.Text = "";
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 8 && !Char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            } 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OKFunc();
        }

        //提交
        private void OKFunc()
        {
            string cardtemp = "";
            if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                cardtemp = textBox1.Text.Trim();
            }
            //else
            //{
            //    MessageBox.Show("银行卡号不能为空，请重新输入！");
            //    return;
            //}

            decimal rejetemp = 0.00m;
            if (!string.IsNullOrEmpty(textBox2.Text.Trim()))
            {
                if (!decimal.TryParse(textBox2.Text.Trim(), out rejetemp))
                {
                    MessageBox.Show("银行返现金额输入有误，请重新输入！");
                    return;
                }
            }

            decimal rezktemp = 0.00m;
            if (!string.IsNullOrEmpty(textBox3.Text.Trim()))
            {
                if (!decimal.TryParse(textBox3.Text.Trim(), out rezktemp))
                {
                    MessageBox.Show("银行折扣值输入有误，请重新输入！");
                    return;
                }
            }

            decimal payjetemp = 0.00m;
            if (!string.IsNullOrEmpty(textBox4.Text.Trim()))
            {
                if (!decimal.TryParse(textBox4.Text.Trim(), out payjetemp))
                {
                    MessageBox.Show("支付金额输入有误，请重新输入！");
                    return;
                }
            }
            else
            {
                payjetemp = -1;
            }


            changed(cardtemp, payjetemp, rejetemp, rezktemp);

            this.Close();
        }



        private void CardPayForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CE.label5.Text = tempStr;
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' && e.KeyChar != '.' || e.KeyChar > '9' && e.KeyChar != '.' || ((TextBox)(sender)).Text.IndexOf('.') >= 0 && e.KeyChar == '.') && e.KeyChar != (char)13 && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' && e.KeyChar != '.' || e.KeyChar > '9' && e.KeyChar != '.' || ((TextBox)(sender)).Text.IndexOf('.') >= 0 && e.KeyChar == '.') && e.KeyChar != (char)13 && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                decimal cetemp = CEJE;

                if (!string.IsNullOrEmpty(textBox2.Text.Trim()) && !string.IsNullOrEmpty(textBox3.Text.Trim()))
                {
                    decimal temp = 0;
                    if (decimal.TryParse(textBox2.Text.Trim(), out temp))
                    {
                        decimal temp2 = 0;
                        if (decimal.TryParse(textBox3.Text.Trim(), out temp2))
                        {
                            cetemp -= cetemp * temp2 / 100;
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
                else if (!string.IsNullOrEmpty(textBox2.Text.Trim()))
                {
                    decimal temp2 = 0;
                    if (decimal.TryParse(textBox2.Text.Trim(), out temp2))
                    {
                        cetemp -= temp2;
                        if (cetemp < 0)
                        {
                            MessageBox.Show("应付金额不允许小于0，请重新输入！");
                            textBox2.Text = "";
                            return;
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(textBox3.Text.Trim()))
                {
                    decimal temp2 = 0;
                    if (decimal.TryParse(textBox3.Text.Trim(), out temp2))
                    {
                        cetemp -= cetemp * temp2 / 100;

                    }
                }

                label10.Text = cetemp.ToString("0.00");
            }
            catch
            {

            }
        }



        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                decimal cetemp = CEJE;

                if (!string.IsNullOrEmpty(textBox3.Text.Trim()) && !string.IsNullOrEmpty(textBox2.Text.Trim()))
                {
                    decimal temp = 0;
                    if (decimal.TryParse(textBox3.Text.Trim(), out temp))
                    {
                        decimal temp2 = 0;
                        if (decimal.TryParse(textBox2.Text.Trim(), out temp2))
                        {
                            cetemp -= temp2;
                        }

                        cetemp -= cetemp * temp / 100;
                        if (cetemp < 0)
                        {
                            MessageBox.Show("应付金额不允许小于0，请重新输入！");
                            textBox3.Text = "";
                            return;
                        }
                    }
                }

                else if (!string.IsNullOrEmpty(textBox3.Text.Trim()))
                {
                    decimal temp2 = 0;
                    if (decimal.TryParse(textBox3.Text.Trim(), out temp2))
                    {
                        cetemp -= cetemp * temp2 / 100;
                        if (cetemp < 0)
                        {
                            MessageBox.Show("应付金额不允许小于0，请重新输入！");
                            textBox3.Text = "";
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

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' && e.KeyChar != '.' || e.KeyChar > '9' && e.KeyChar != '.' || ((TextBox)(sender)).Text.IndexOf('.') >= 0 && e.KeyChar == '.') && e.KeyChar != (char)13 && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }





    }
}
