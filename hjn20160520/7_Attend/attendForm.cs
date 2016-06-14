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

namespace hjn20160520._7_Attend
{
    public partial class attendForm : Form
    {

        //主菜单窗口
        MainForm mainForm;


        public attendForm()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label19.Text = System.DateTime.Now.ToString();
        }

        private void attendForm_Load(object sender, EventArgs e)
        {
            mainForm = new MainForm();
            //this.FormBorderStyle = FormBorderStyle.None;
            this.timer1.Enabled = true;
            this.textBox1.Focus();
            this.textBox1.SelectAll();
        }


        //热键
        private void attendForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {

                //回车
                case Keys.Enter:

                    break;
                //退出
                case Keys.Escape:

                    mainForm.Show();
                    this.Close();

                    break;
                    //上班刷卡
                case Keys.F2:
                    var roleInfo = ShowUserByID();
                    ShowInfo(roleInfo);

                    break;
                //下班刷卡
                case Keys.F3:

                    break;


            }
        }


        //根据员工ID查询员工信息
        private RolesModel ShowUserByID()
        {
            RolesModel role = new RolesModel();  // 拿这个容器装一下

            string temp_text = textBox1.Text.Trim();
            using (var db = new hjnbhEntities())
            {
                var userInfos = db.users.Where(t => t.login_id == temp_text).Select(t => new { t.usr_id, t.usr_name, t.ctime, t.user_type, t.sex }).ToList();
                if (userInfos != null)
                {
                    foreach (var item in userInfos)
                    {
                        role.cTime = item.ctime;
                        role.id = item.usr_id;
                        role.name = item.usr_name;
                        role.roleType = item.user_type;
                        role.sex = item.sex.ToString();
                    }
                }
                return role;
            }
        }

        //UI信息赋值
        private void ShowInfo(RolesModel role)
        {
            label13.Text = role.id.ToString();
            label12.Text = role.name;
            label11.Text = role.cTime.ToString();
            label10.Text = System.DateTime.Now.ToString();
            label14.Text = role.roleTypeStr;
            label15.Text = role.sex;
            label16.Text = role.roleTypeStr;

            panel4.Visible = panel5.Visible = true;
        }









    }
}
