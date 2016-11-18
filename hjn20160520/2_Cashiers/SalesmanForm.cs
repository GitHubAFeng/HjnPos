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
    public partial class SalesmanForm : Form
    {
        //用于其它窗口传值给本窗口控件
        //这是委托与事件的第一步  
        public delegate void YWYDPHandle(string s , int id);
        public event YWYDPHandle changed;

        public delegate void YWYZDHandle(string s, int id);
        public event YWYZDHandle ZDchanged;  

        public SalesmanForm()
        {
            InitializeComponent();
        }

        private void SalesmanForm_Load(object sender, EventArgs e)
        {
            //默认全选
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                textBox2.SelectAll();
            }

        }



        //快捷键
        private void SalesmanForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    SalesFun();
                    //changed(ywyname, ywyid2);
                    this.Close();
                    break;
                case Keys.Escape:
                    this.Close();
                    break;
                case Keys.F1:
                    changed("空", -2);
                    this.Close();
                    break;
                case Keys.F2:
                    ZDchanged("空", -2);
                    this.Close();
                    break;

            }
        }

        #region 查询业务员工
        //int ywyid2 = -1;
        //string ywyname = string.Empty;

        //处理业务员录入逻辑
        private void SalesFun()
        {
            try
            {
            if (string.IsNullOrEmpty(textBox1.Text.Trim()) && string.IsNullOrEmpty(textBox2.Text.Trim())) return;

            using (var db = new hjnbhEntities())
            {
                //整单业务员工
                if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
                {

                    int ywyid = -1;
                    if (int.TryParse(textBox1.Text.Trim(), out ywyid))
                    {

                        //查用户视图
                        var ywyInfo = db.user_role_view.AsNoTracking().Where(t => t.usr_id == ywyid).FirstOrDefault();
                        if (ywyInfo != null)
                        {
                            ZDchanged(ywyInfo.usr_name, ywyid);
                        }
                        else
                        {
                            MessageBox.Show("（整单）查询不到该员工号!");
                        }

                    }
                }


                int ywyid2 = -1;
                //单品业务员工
                if (!string.IsNullOrEmpty(textBox2.Text.Trim()))
                {
                    if (int.TryParse(textBox2.Text.Trim(), out ywyid2))
                    {

                        //查用户视图
                        var ywyInfo2 = db.user_role_view.AsNoTracking().Where(t => t.usr_id == ywyid2).FirstOrDefault();
                        if (ywyInfo2 != null)
                        {
                            //ywyname = ywyInfo2.usr_name;
                            //ywyid2 = ywyInfo2.usr_id;
                            changed(ywyInfo2.usr_name, ywyid2);

                        }
                        else
                        {
                            MessageBox.Show("(单品)查询不到该员工号!");
                        }

                    }
                }
            }


            }
            catch (Exception e)
            {
                LogHelper.WriteLog("业务员录入窗口登记时出现异常:", e);
                MessageBox.Show("业务员录入时出现异常！请检查业务员资料是否正确，必要时请联系管理员！");
                //string tip = ConnectionHelper.ToDo();
                //if (!string.IsNullOrEmpty(tip))
                //{
                //    MessageBox.Show(tip);
                //}
            }
        }

        #endregion

        //限制只能输入数字
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
            SalesFun();
            //changed(ywyname, ywyid2);
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            changed("空", -2);
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ZDchanged("空", -2);
            this.Close();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
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





    }
}
