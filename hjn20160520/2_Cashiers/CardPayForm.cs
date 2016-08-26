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
        //这是委托与事件的第一步  
        public delegate void CardPayFormHandle(string card);
        public event CardPayFormHandle changed;

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
                    CE.label5.Text = tempStr;
                    this.Close();
                    break;

                case Keys.Enter:
                    if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
                    {
                        string temp = textBox1.Text.Trim();
                        changed(temp);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("卡号为空，请重新输入！");
                    }
                    break;
            }
        }

        private void CardPayForm_Load(object sender, EventArgs e)
        {
            CE = this.Owner as ClosingEntries;
            tempStr = CE.label5.Text;

            CE.label5.Text = "银联卡";
            textBox1.Focus();
            textBox1.SelectAll();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 8 && !Char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            } 
        }
    }
}
