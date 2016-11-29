using Common;
using hjn20160520.Models;
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
    public partial class ScreenSaverForm : Form
    {

        public delegate void ScreenSaverFormHandle(bool isValied = false);
        public event ScreenSaverFormHandle Changed;  //通知是否验证通过

        public ScreenSaverForm()
        {
            InitializeComponent();
        }

        private void ScreenSaverForm_Load(object sender, EventArgs e)
        {
            label3.Text = HandoverModel.GetInstance.userName + "(" + HandoverModel.GetInstance.userID + ")";
            textBox1.Text = "";
        }

        private void ScreenSaverForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.Close();
                    break;
                case Keys.Enter:

                    UserLoginById();
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UserLoginById();
        }


        private void UserLoginById()
        {
            try
            {

                string loginID;
                string passWord;
                if (string.IsNullOrEmpty(textBox1.Text))
                {
                    MessageBox.Show("密码不能为空！");
                    return;
                }

                loginID = HandoverModel.GetInstance.userID.ToString();
                passWord = textBox1.Text.Trim();

                using (var db = new hjnbhEntities())
                {
                    var infos = db.users.AsNoTracking().Where(t => t.login_id == loginID && t.password == passWord).FirstOrDefault();
                    if (infos != null)
                    {
                        var sta = infos.state.HasValue ? infos.state.Value : 0;
                        if (sta == 1)
                        {

                            Changed(true);
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("该员工为不可用状态！请重新登录");

                        }

                    }
                    else
                    {
                        MessageBox.Show("登录密码错误！");

                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("屏保重新查询员工时发生异常:", ex);
                MessageBox.Show("服务器访问出错！请检查网络是否正常，服务器配置是否正确，必要时请联系管理员！");
            }
        }








    }
}
