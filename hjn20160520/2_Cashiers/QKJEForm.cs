using Common;
using hjn20160520.Common;
using hjn20160520.Models;
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
    /// 欠款挂账窗口
    /// </summary>
    public partial class QKJEForm : Form
    {
        ClosingEntries ce;
        TipForm tipForm;

        public delegate void QKJEFormHandle(decimal qkje);
        public event QKJEFormHandle changed;

        InputBoxForm passwordForm = new InputBoxForm();  //会员密码检证窗口
        string VipPW = ""; //会员密码验证

        public QKJEForm()
        {
            InitializeComponent();
        }


        private void QKJEForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    OKFunc();

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
                    var res = db.hd_sys_qx.AsNoTracking().Where(t => (t.usr_id == qxid_temp) && (t.zm == qxzm_temp)).FirstOrDefault();

                    if (res != null)
                    {
                        decimal qk = ce.QKjs;
                        decimal temp = res.mje.HasValue ? res.mje.Value : 0;
                        if ((qk + QK_temp) <= temp)
                        {

                            int vip = HandoverModel.GetInstance.VipID;
                            //会员信息
                            var vipInfo = db.hd_vip_info.Where(t => t.vipcode == vip).FirstOrDefault();
                            decimal? qk_temp = Convert.ToDecimal(vipInfo.other4) + QK_temp;
                            vipInfo.other4 = qk_temp.ToString();
                            db.SaveChanges();

                            changed(QK_temp.Value);

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
            ce = this.Owner as ClosingEntries;
            VipPW = "";
            passwordForm.changed += passwordForm_changed;
            textBox3.Text = "";
        }

        void passwordForm_changed(string PW)
        {
            this.VipPW = PW;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OKFunc();
        }

        private void OKFunc()
        {
            int vipid = HandoverModel.GetInstance.VipID;
            if (vipid != 0)
            {
                using (var db = new hjnbhEntities())
                {
                    var Vippasswordinfo = db.hd_vip_info.Where(t => t.vipcode == vipid).Select(t => t.password).FirstOrDefault();

                    string vippw = Vippasswordinfo.HasValue ? Vippasswordinfo.Value.ToString() : "0";
                    //先验证密码
                    passwordForm.ShowDialog();
                    if (VipPW != vippw)
                    {
                        MessageBox.Show("会员密码检验失败！请输入正确的会员密码！可尝试使用默认密码 0 。", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else
                    {
                        QKFunc();

                        VipPW = ""; //会员密码验证，用完要清空，防止被盗用
                    }
                }

            }
            else
            {
                MessageBox.Show("您还没有登记会员，请先在按F3查询会员！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }



        private void QKJEForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            passwordForm.changed -= passwordForm_changed;

        }


    }
}
