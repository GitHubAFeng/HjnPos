using Common;
using hjn20160520.Common;
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
    public partial class QKJEForm : Form
    {
        TipForm tipForm;

        public QKJEForm()
        {
            InitializeComponent();
        }


        private void QKJEForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    QKFunc();
                    break;

                case Keys.Escape:
                    this.Close();
                    break;
            }
        }


        private void QKFunc()
        {
            try
            {
                decimal? QK_temp = Convert.ToDecimal(textBox1.Text.Trim());
                int qxid_temp = 0;
                int qxzm_temp = 0;
                if (!string.IsNullOrEmpty(textBox2.Text.Trim()))
                {
                    int.TryParse(textBox2.Text.Trim(), out qxid_temp);
                }

                if (!string.IsNullOrEmpty(textBox3.Text.Trim()))
                {
                    int.TryParse(textBox3.Text.Trim(), out qxzm_temp);
                }
                //检证用户    
                using (var db = new hjnbhEntities())
                {
                    //目前数据库中没有挂账上限字段，先用挂零的字段实现功能先……
                    var res = db.hd_sys_qx.Where(t => (t.usr_id == qxid_temp) && (t.zm == qxzm_temp)).FirstOrDefault();

                    if (res != null)
                    {
                        decimal? temp = res.mje;
                        if (QK_temp <= temp)
                        {
                            string vip = Cashiers.GetInstance.VipID.ToString();
                            //会员信息
                            var vipInfo = db.hd_vip_info.Where(t => t.vipcard == vip).FirstOrDefault();
                            decimal? qk_temp = Convert.ToDecimal(vipInfo.other4) + QK_temp;
                            vipInfo.other4 = qk_temp.ToString();
                            db.SaveChanges();

                            ClosingEntries.GetInstance.QKjs = QK_temp;  //挂账金额
                            ClosingEntries.GetInstance.CETotalMoney -= QK_temp; //总金额减去挂账
                            ClosingEntries.GetInstance.label6.Text = QK_temp.ToString(); //已挂金额
                            ClosingEntries.GetInstance.getMoney = ClosingEntries.GetInstance.CETotalMoney;
                            this.Close();
                        }
                        else
                        {
                            tipForm = new TipForm();
                            tipForm.Tiplabel.Text = "挂账金额太大，您权限不足！";
                            tipForm.ShowDialog();
                            textBox1.Focus();
                            textBox1.SelectAll();
                        }

                    }
                    else
                    {
                        tipForm = new TipForm();
                        tipForm.Tiplabel.Text = "工号或者密码错误！";
                        tipForm.ShowDialog();
                        textBox2.Focus();
                        textBox2.SelectAll();
                    }

                }
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("会员挂账窗口进行挂账时出现异常:", e);
                MessageBox.Show("数据库连接出错！");
                string tip = ConnectionHelper.ToDo();
                if (!string.IsNullOrEmpty(tip))
                {
                    MessageBox.Show(tip);
                }
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' && e.KeyChar != '.' || e.KeyChar > '9' && e.KeyChar != '.' || ((TextBox)(sender)).Text.IndexOf('.') >= 0 && e.KeyChar == '.') && e.KeyChar != (char)13 && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }

        private void QKJEForm_Load(object sender, EventArgs e)
        {

        }


    }
}
