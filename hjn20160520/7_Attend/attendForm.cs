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
            tipslabel21.Text = "请签到";
        }


        //热键
        private void attendForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {

                //回车
                case Keys.Enter:
                    var roleInfo1 = ShowUserByID();
                    if (roleInfo1 != null)
                        ShowInfo(roleInfo1);

                    break;
                //退出
                case Keys.Escape:

                    //mainForm.Show();
                    this.Close();

                    break;
                //上班刷卡
                case Keys.F2:
                    var roleInfo2 = ShowUserByID();
                    if (roleInfo2 != null)
                        ShowInfo(roleInfo2);

                    break;
                //下班刷卡
                case Keys.F3:

                    break;


            }
        }


        //根据员工ID查询员工信息
        private RolesModel ShowUserByID()
        {
            try
            {
                string temp_text = string.Empty;
                RolesModel role = new RolesModel();  // 拿这个容器装一下
                if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
                {
                    temp_text = textBox1.Text.Trim();
                }
                else
                {
                    MessageBox.Show("员工号为空！");
                    return null;
                }
                using (var db = new hjnbhEntities())
                {
                    //查员工信息
                    var userInfos = db.users.AsNoTracking().Where(t => t.login_id == temp_text).Select(t => new { t.usr_id, t.usr_name, t.ctime, t.user_type, t.sex }).FirstOrDefault();
                    if (userInfos != null)
                    {
                        role.cTime = userInfos.ctime;
                        role.id = userInfos.usr_id;
                        role.name = userInfos.usr_name;
                        role.roleType = userInfos.user_type;
                        role.sex = userInfos.sex.ToString();

                        //增加签到记录
                        var kqnote = new hd_sys_kq
                        {
                            usr_id = userInfos.usr_id,
                            scode = HandoverModel.GetInstance.scode,
                            cname = userInfos.usr_name,
                            utime = System.DateTime.Now,
                            todaytime = System.DateTime.Now.Date
                            //还有一个考勤标识
                        };
                        db.hd_sys_kq.Add(kqnote);
                        var re = db.SaveChanges();
                        if (re <= 0)
                        {
                            MessageBox.Show("签到失败！");
                            return null;
                        }
                    }

                    return role;
                }
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("员工考勤窗口保存签到记录时出现异常:", e);
                MessageBox.Show("数据库连接出错！");
                string tip = ConnectionHelper.ToDo();
                if (!string.IsNullOrEmpty(tip))
                {
                    MessageBox.Show(tip);
                }
                return null;
            }
        }

        //UI信息赋值
        private void ShowInfo(RolesModel role)
        {
            textBox1.SelectAll();
            label13.Text = role.id.ToString();
            label12.Text = role.name;
            label11.Text = role.cTime.ToString();
            label10.Text = System.DateTime.Now.ToString();
            label14.Text = role.roleTypeStr;
            label15.Text = role.sex;
            label16.Text = role.roleTypeStr;
            tipslabel21.Text = role.name + "，签到成功！";

            panel4.Visible = panel5.Visible = true;
        }









    }
}
