using Common;
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
    /// 修改商品金额
    /// </summary>
    public partial class UpdataJEForm : Form
    {
        public delegate void UpdataJEFormHandle(decimal je);
        public event UpdataJEFormHandle changed;  //传递修改金额到购物车

        public UpdataJEForm()
        {
            InitializeComponent();
        }

        private void UpdataJEForm_Load(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox3.Text = "";
            textBox1.Focus();
        }

        private void UpdataJEForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.Close();
                    break;
                case Keys.Enter:
                    updataJEFunc();
                    break;
            }
        }

        private void updataJEFunc()
        {
            try
            {
                decimal updataje = 0.00m; //修改金额
                int qxid_temp = 0; //权限工号
                int qxzm_temp = 0;  //权限密码

                if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
                {
                    decimal.TryParse(textBox1.Text.Trim(), out updataje);
                }

                if (!string.IsNullOrEmpty(textBox2.Text.Trim()))
                {
                    int.TryParse(textBox2.Text.Trim(), out qxid_temp);
                }

                if (!string.IsNullOrEmpty(textBox3.Text.Trim()))
                {
                    int.TryParse(textBox3.Text.Trim(), out qxzm_temp);
                }

                if (string.IsNullOrEmpty(textBox1.Text.Trim())) return;

                //检证用户权限    
                using (var db = new hjnbhEntities())
                {
                    var res = db.hd_sys_qx.AsNoTracking().Where(t => (t.usr_id == qxid_temp) && (t.zm == qxzm_temp)).FirstOrDefault();

                    if (res != null)
                    {

                        if (updataje > 0)
                        {
                            changed(updataje);
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("修改金额小于零！此操作无效！");
                        }

                    }
                    else
                    {

                        MessageBox.Show("权限工号或者密码错误！");
                        textBox2.Focus();
                        textBox2.SelectAll();
                    }

                }
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("收银修改商品金额窗口进行提款时出现异常:", e);
                MessageBox.Show("收银修改商品金额出现异常！请检查数值输入是否正常，必要时候请联系管理员！");

            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' && e.KeyChar != '.' || e.KeyChar > '9' && e.KeyChar != '.' || ((TextBox)(sender)).Text.IndexOf('.') >= 0 && e.KeyChar == '.') && e.KeyChar != (char)13 && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            updataJEFunc();
        }















    }
}
