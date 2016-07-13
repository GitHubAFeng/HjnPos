using hjn20160520.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace hjn20160520.Login
{
    public partial class LoginForm : Form
    {

        MainForm mainForm; //主窗口
        //string outerIP = "";  //外网IP
        string innerIP = "";  //内网IP

        public LoginForm()
        {
            InitializeComponent();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            innerIP = GetHostIP();

            GetUserConfig(@"../UserConfig.xml");

        }

        //快捷键
        private void LoginForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    //if (string.IsNullOrEmpty(outerIP = isCanConnetIE()))
                    //{
                    //    MessageBox.Show("无法连接网络！");
                    //    return;
                    //}

                    if (UserLoginById())
                    {
                        this.Hide();
                    }

                    break;

                case Keys.Escape:
                    Application.Exit();
                    break;
            }
        }


        private bool UserLoginById()
        {
            string loginID;
            string passWord;
            if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("员工号或者密码不能为空！");
                return false;
            }

            loginID = textBox1.Text.Trim();
            passWord = textBox2.Text.Trim();

            using (var db = new hjnbhEntities())
            {
                var infos = db.users.AsNoTracking().Where(t => t.login_id == loginID && t.password == passWord).FirstOrDefault();
                if (infos != null)
                {
                    infos.last_ip = innerIP;  //登录IP
                    infos.last_login = System.DateTime.Now;  //登录时间
                    db.SaveChanges();
                    //查询员工信息
                    int usrid = infos.usr_id;
                    var userInfos = db.user_role_view.AsNoTracking().Where(t => t.usr_id == usrid).FirstOrDefault();
                    string _name = userInfos.usr_name;
                    HandoverModel.GetInstance.userID = userInfos.usr_id;  //员工ID
                    HandoverModel.GetInstance.userName = _name;  //员工名字
                    HandoverModel.GetInstance.RoleID = userInfos.role_id.HasValue ? (int)userInfos.role_id : 0; //角色ID
                    HandoverModel.GetInstance.RoleName = userInfos.role_name;  //角色

                    mainForm = new MainForm();
                    mainForm.Hellolabel.Text = "您好，" + _name;
                    mainForm.Show();
                    return true;
                }
                else
                {
                    MessageBox.Show("员工号或者密码错误！");
                }

            }
            return false;
        }


        //判断是否连接数据库，没用
        private bool isCanconnetDB()
        {
            string connStr = "Server=192.168.16.88;initial catalog=hjnbh;user id=sa;password=123456";

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                if (conn.State == ConnectionState.Open)
                {
                    MessageBox.Show("数据库连接打开");
                    return true;
                }
                else
                {
                    MessageBox.Show("数据库连接失败");

                }
            }

            return false;
        }

        //判断本机是否联网，也是获取外网IP
        private string isCanConnetIE()
        {
            string tempip = "";
            try
            {
                WebRequest wr = WebRequest.Create("http://www.ip138.com/ips138.asp");
                Stream s = wr.GetResponse().GetResponseStream();
                StreamReader sr = new StreamReader(s, Encoding.Default);
                string all = sr.ReadToEnd(); //读取网站的数据

                int start = all.IndexOf("您的IP地址是：[") + 9;
                int end = all.IndexOf("]", start);
                tempip = all.Substring(start, end - start);
                sr.Close();
                s.Close();
            }
            catch
            {
            }
            return tempip;

        }

        //获取本机内网IP
        private string GetHostIP()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }


        //读取本地用户配置XML
        private void GetUserConfig(string logPath)
        {
            if (!File.Exists(logPath)) return;
            XElement el = XElement.Load(logPath);

            var products = el.Elements("user").Where(e => e.Attribute("ID").Value == "1").Select(e => e.Element("scode").Value).FirstOrDefault();
            if (products != null)
            {
                HandoverModel.GetInstance.scode = int.Parse(products.Trim());
                //MessageBox.Show(products);
            }
            //else
            //{
            //    MessageBox.Show("Test");
            //}
        }



    }
}
