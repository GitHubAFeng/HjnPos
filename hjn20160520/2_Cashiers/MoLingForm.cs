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
    public partial class MoLingForm : Form
    {
        ClosingEntries ce;
        TipForm tipForm;
        //用于其它窗口传值给本窗口控件
        //这是委托与事件的第一步  
        public delegate void MoLingFormHandle(decimal s);
        public event MoLingFormHandle changed;  

        public MoLingForm()
        {
            InitializeComponent();
        }

        private void MoLingForm_Load(object sender, EventArgs e)
        {
            ce = this.Owner as ClosingEntries;
            textBox1.Focus();
            textBox1.SelectAll();
        }
        //快捷键
        private void MoLingForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.Close();
                    break;
                case Keys.Enter:
                    MoLingFunc();
                    break;

            }
        }

        //处理抹零逻辑，抹零需要权限
        private void MoLingFunc()
        {
            try
            {
                decimal moling_temp = Convert.ToDecimal(textBox1.Text.Trim());
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
                    var res = db.hd_sys_qx.AsNoTracking().Where(t => (t.usr_id == qxid_temp) && (t.zm == qxzm_temp)).FirstOrDefault();

                    if (res != null)
                    {
                        //decimal mol = ClosingEntries.GetInstance.MoLing.HasValue ? ClosingEntries.GetInstance.MoLing.Value : 0;
                        decimal mol = ce.MoLing.HasValue ? ce.MoLing.Value : 0;
                        decimal temp = res.mje.HasValue ? res.mje.Value : 0;
                        if ((mol + moling_temp) <= temp)
                        {

                            //ClosingEntries.GetInstance.MoLing += moling_temp;  //抹零
                            //ClosingEntries.GetInstance.CETotalMoney -= moling_temp; //总金额减去抹零
                            changed(moling_temp);  //抹零传值
                            this.Close();


                        }
                        else
                        {
                            tipForm = new TipForm();
                            tipForm.Tiplabel.Text = "抹零金额太大，您权限不足！";
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
                LogHelper.WriteLog("抹零窗口进行抹零时出现异常:", e);
                MessageBox.Show("数据库连接出错！");
                string tip = ConnectionHelper.ToDo();
                if (!string.IsNullOrEmpty(tip))
                {
                    MessageBox.Show(tip);
                }
            }

        }

        //限制只能输入数字与小数点
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' && e.KeyChar != '.' || e.KeyChar > '9' && e.KeyChar != '.' || ((TextBox)(sender)).Text.IndexOf('.') >= 0 && e.KeyChar == '.') && e.KeyChar != (char)13 && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }

        //权限工号
        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            MoLingFunc();

        }




    }
}
