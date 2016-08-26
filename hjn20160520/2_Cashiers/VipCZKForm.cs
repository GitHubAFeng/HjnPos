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
    /// 会员储值卡消费
    /// </summary>
    public partial class VipCZKForm : Form
    {
        ClosingEntries CE;
        //这是委托与事件的第一步  
        public delegate void VipCZKFormHandle(decimal s);
        public event VipCZKFormHandle changed;

        public VipCZKForm()
        {
            InitializeComponent();
        }

        private void VipCZKForm_Load(object sender, EventArgs e)
        {
            CE = this.Owner as ClosingEntries;
            ShowSE();
            if (setemp < CE.JE)
            {
                textBox1.Text = setemp.ToString();
            }
            else
            {
                textBox1.Text = CE.JE.ToString();
            }

            textBox1.Focus();
            textBox1.SelectAll();
        }

        private void VipCZKForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    KaFunc();

                    break;



                case Keys.Escape:
                    this.Close();

                    break;



            }
        }


        private void KaFunc()
        {
            decimal temp = 0;
            if (decimal.TryParse(textBox1.Text.Trim(), out temp))
            {
                if (temp > setemp)
                {
                    MessageBox.Show("输入的金额不能大于应付金额！");
                }
                else
                {
                    changed(temp);
                    this.Close();
                }

            }
            else
            {
                MessageBox.Show("请核对是否输入正确");
            }

        }

        decimal setemp = 0;
        //显示余额
        private void ShowSE()
        {
            using (var db = new hjnbhEntities())
            {
                var SE = db.hd_vip_info.AsNoTracking().Where(t => t.vipcode == CashiersFormXP.GetInstance.VipID).Select(t => t.czk_ye).FirstOrDefault();
                if (SE != null)
                {
                    setemp = SE.Value;
                    label3.Text = SE.ToString();
                }
            }
        }



    }
}
