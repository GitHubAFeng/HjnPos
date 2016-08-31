using Common;
using hjn20160520.Common;
using hjn20160520.Login;
using hjn20160520.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace hjn20160520
{
    /// <summary>
    /// 为了兼容XP 的登陆窗口
    /// </summary>
    public partial class Test : Form
    {
        public Test()
        {
            InitializeComponent();
        }

        private void Test_Load(object sender, EventArgs e)
        {
            GetUserConfig(@"../UserConfig.xml");
            //TestFunc();
            GetDBConfig();   //服务器配置
        }



        private bool UserLoginById()
        {
            //try
            //{

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
                        //infos.last_ip = innerIP;  //登录IP
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

                        MainFormXP mainForm = new MainFormXP();
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
            //}
            //catch (Exception)
            //{

            //    MessageBox.Show("数据库访问出错！");
            //    return false;
            //}
        }

        private void Test_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:

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



        //读取本地用户配置XML
        private void GetUserConfig(string logPath)
        {
            try
            {
                if (!File.Exists(logPath))
                {
                    SvaeConfigFunc(@"../");
                }
                else
                {
                    XElement el = XElement.Load(logPath);

                    var products = el.Elements("user").Where(e => e.Attribute("ID").Value == "1").FirstOrDefault();
                    if (products != null)
                    {
                        HandoverModel.GetInstance.scode = int.Parse(products.Element("scode").Value.Trim());
                        HandoverModel.GetInstance.bcode = int.Parse(products.Element("bcode").Value.Trim());
                        HandoverModel.GetInstance.scodeName = products.Element("cname").Value.Trim();
                        HandoverModel.GetInstance.istorePath = products.Element("istorepath").Value.Trim();
                        HandoverModel.GetInstance.Call = products.Element("call").Value;
                        HandoverModel.GetInstance.Address = products.Element("address").Value;
                        HandoverModel.GetInstance.Remark1 = products.Element("remark1").Value;
                        HandoverModel.GetInstance.Remark2 = products.Element("remark2").Value;
                    }
                }
            }
            catch
            {
                MessageBox.Show("读取本地用户配置失败！请尝试忽略此警告，继续运行软件后会自动纠错，必要时请联系管理员！");
            }

        }


        /// <summary>
        /// 保存XML配置文件 , 不存在就创建
        /// </summary>
        /// <param name="path">目录路径</param>
        private void SvaeConfigFunc(string path)
        {
            try
            {
                if (!System.IO.Directory.Exists((path)))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                string logPath = path + "UserConfig.xml";

                if (!File.Exists(logPath))
                {
                    XDocument doc = new XDocument
                    (
                        new XDeclaration("1.0", "utf-8", "yes"),
                        new XElement
                        (
                            "setting",
                            new XElement
                            (
                                "user",
                                new XAttribute("ID", 1),
                                new XElement("scode", 1),  //分店
                                new XElement("cname", "黄金牛儿童百货"),  //分店名字
                                new XElement("index", 0),  //下拉下标，方便下次自动选中此下标位置
                                new XElement("bcode", 1),  //机号
                                new XElement("istorepath", ""),  //库存报表路径
                                new XElement("ctime", System.DateTime.Now.ToShortDateString())
                            )
                        )
                    );
                    // 保存为XML文件
                    doc.Save(logPath);
                }
                else
                {
                    XElement el = XElement.Load(logPath);

                    var products = el.Elements("user").Where(e => e.Attribute("ID").Value == "1").FirstOrDefault();
                    if (products != null)
                    {
                        products.SetAttributeValue("ID", 1);
                        products.ReplaceNodes
                        (
                            new XElement("scode", 1),
                            new XElement("cname", "黄金牛儿童百货"),  //分店名字
                            new XElement("index", 0),  //下拉下标，方便下次自动选中此下标位置
                            new XElement("bcode", 1),  //机号
                            new XElement("istorepath", ""),  //库存报表路径
                            new XElement("ctime", System.DateTime.Now.ToShortDateString())
                        );

                        el.Save(logPath);
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("登录时默认生成分店信息时发生异常:", ex);

            }
        }


        //读取本地用户配置XML,读取网络配置
        private void GetDBConfig(string logPath = @"../DBConfig.xml")
        {
            try
            {
                if (!File.Exists(logPath)) return;

                XElement el = XElement.Load(logPath);

                var products = el.Elements("DB").Where(e => e.Attribute("ID").Value == "1").FirstOrDefault();
                if (products != null)
                {

                    MyEFDB.serverAdd = products.Element("server").Value;
                    MyEFDB.dadaBaseName = products.Element("dadaBase").Value;
                    MyEFDB.usrid = products.Element("usr").Value;
                    MyEFDB.wd = products.Element("wd").Value;

                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("登陆读取DB网络配置时发生异常:", ex);
                MessageBox.Show("网络配置读取失败！");

            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            NetForm nf = new NetForm();

            nf.ShowDialog();
        }









    }
}
