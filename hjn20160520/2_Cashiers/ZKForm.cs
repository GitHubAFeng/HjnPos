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

        CashiersFormXP CSFrom;

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
                    VialeFunc();
                    break;
                case Keys.F1:
                    changed(-1);
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
            if (zkl > 0)
            {
                changed(zkl);
                this.Close();
            }
            else
            {
                MessageBox.Show("折扣率输入错误！请输入0~100之间的数值！");
            }
        }

        private void ZKForm_Load(object sender, EventArgs e)
        {
            CSFrom = this.Owner as CashiersFormXP;
            this.ActiveControl = this.textBox1;
            textBox3.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            VialeFunc();
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


        //权限查询
        private void VialeFunc()
        {
            int user = 0;  //用户
            if (!string.IsNullOrEmpty(textBox2.Text.Trim()))
            {
                int.TryParse(textBox2.Text.Trim(), out user);
            }

            int pw = 0;  //密码
            if (!string.IsNullOrEmpty(textBox3.Text.Trim()))
            {
                int.TryParse(textBox3.Text.Trim(), out pw);
            }

            int zk = 0;  //折扣
            if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                int.TryParse(textBox1.Text.Trim(), out zk);
            }

            using (var db = new hjnbhEntities())
            {
                var qxinfo = db.hd_sys_qx.AsNoTracking().Where(t => t.usr_id == user && t.zm == pw).FirstOrDefault();
                if (qxinfo != null)
                {
                    if (zk < qxinfo.zk)
                    {
                        MessageBox.Show("该权限工号折扣率不能低于 " + qxinfo.zk.ToString() + "，请重新输入折扣率！", "权限检证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    }
                    else
                    {
                        ZKLFunc();
                    }
                }
                else
                {
                    MessageBox.Show("权限工号或者密码错误！请重新确认。", "权限检证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

            }


        }

        private void button2_Click(object sender, EventArgs e)
        {
            changed(-1);
            this.Close();
        }



    }
}
