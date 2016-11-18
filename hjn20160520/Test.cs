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
        MainFormXP mainForm = new MainFormXP();

        //public string usrName = "";  //用户名

        public Test()
        {
            InitializeComponent();
        }

        private void Test_Load(object sender, EventArgs e)
        {
            GetUserConfig(@"../UserConfig.xml");
            GetDBConfig();   //服务器配置

            HandoverModel.GetInstance.pc_code = HardwareHandler.GetNetworkAdpaterID();  //机械码
            HandoverModel.GetInstance.pc_name = HardwareHandler.GetMachineName(); //计算机名字
        }



        private bool UserLoginById()
        {
            try
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
                        var sta = infos.state.HasValue ? infos.state.Value : 0;
                        if (sta == 1)
                        {
                            //infos.last_ip = innerIP;  //登录IP
                            infos.last_login = System.DateTime.Now;  //登录时间
                            db.SaveChanges();
                            //查询员工信息
                            int usrid = infos.usr_id;
                            var userInfos = db.user_role_view.AsNoTracking().Where(t => t.usr_id == usrid).FirstOrDefault();
                            //usrName = userInfos.usr_name;
                            HandoverModel.GetInstance.userID = userInfos.usr_id;  //员工ID
                            HandoverModel.GetInstance.userName = userInfos.usr_name; ;  //员工名字
                            HandoverModel.GetInstance.RoleID = userInfos.role_id.HasValue ? (int)userInfos.role_id : 0; //角色ID
                            HandoverModel.GetInstance.RoleName = userInfos.role_name;  //角色

                            //mainForm.Hellolabel.Text = "您好，" + _name;
                            mainForm.Show();
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("该员工为不可用状态！请重新登录");
                            return false;
                        }

                    }
                    else
                    {
                        MessageBox.Show("员工号或者密码错误！");
                    }

                }
                return false;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("登录连接查询员工时发生异常:", ex);
                MessageBox.Show("服务器访问出错！请检查网络是否正常，服务器配置是否正确，必要时请联系管理员！");
                return false;
            }
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

                    XElement el = XElement.Load(logPath);

                    var products = el.Elements("user").Where(e => e.Attribute("ID").Value == "1").FirstOrDefault();
                    if (products != null)
                    {
                        HandoverModel.GetInstance.scodeIndex = Convert.ToInt32(products.Element("index").Value.Trim());
                        HandoverModel.GetInstance.scode = Convert.ToInt32(products.Element("scode").Value.Trim());
                        HandoverModel.GetInstance.bcode = Convert.ToInt32(products.Element("bcode").Value.Trim());
                        HandoverModel.GetInstance.scodeName = products.Element("cname").Value.Trim();
                        HandoverModel.GetInstance.istorePath = products.Element("istorepath").Value.Trim();
                        HandoverModel.GetInstance.Call = products.Element("call").Value;
                        HandoverModel.GetInstance.Address = products.Element("address").Value;
                        HandoverModel.GetInstance.Remark1 = products.Element("remark1").Value;
                        HandoverModel.GetInstance.Remark2 = products.Element("remark2").Value;
                        HandoverModel.GetInstance.Remark3 = products.Element("remark3").Value;
                        HandoverModel.GetInstance.Remark4 = products.Element("remark4").Value;
                        HandoverModel.GetInstance.PageHeight = Convert.ToInt32(products.Element("pageheight").Value);
                        HandoverModel.GetInstance.PageWidth = Convert.ToInt32(products.Element("pagewidth").Value);
                        HandoverModel.GetInstance.FontSize = Convert.ToInt32(products.Element("fontsize").Value);
                        HandoverModel.GetInstance.PrintFont = products.Element("printfont").Value;
                        HandoverModel.GetInstance.PrintTitle = products.Element("printtitle").Value;
                        HandoverModel.GetInstance.PrintCopies = Convert.ToInt16(products.Element("printcopies").Value);

                    }
                
            }
            catch
            {
                SvaeConfigFunc(@"../");
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

                                new XElement("call", ""),  //客服专线
                                new XElement("address", ""),  //地址
                                new XElement("remark1", ""),  //备注1
                                new XElement("remark2", ""),  //备注2
                                new XElement("remark3", ""),  //备注3
                                new XElement("remark4", ""),  //备注4
                                new XElement("printtitle", ""),  //打印标题

                                new XElement("printfont", ""),  //打印字体
                                new XElement("fontsize", ""),  //字体大小
                                new XElement("pagewidth", ""),  //打印页面宽度
                                new XElement("pageheight", ""),  //打印页面高度
                                 new XElement("printcopies", "1"),  //打印份数
                                new XElement("ctime", System.DateTime.Now.ToShortDateString())
                            )
                        )
                    );
                    // 保存为XML文件
                    doc.Save(logPath);
                }
                else
                {
                    //如果有
                    XElement el = XElement.Load(logPath);

                    var products = el.Elements("user").Where(e => e.Attribute("ID").Value == "1").FirstOrDefault();
                    if (products != null)
                    {
                        products.SetAttributeValue("ID", 1);
                        products.ReplaceNodes
                        (
                            new XElement("scode", HandoverModel.GetInstance.scode.ToString()),
                            new XElement("cname", HandoverModel.GetInstance.scodeName),  //分店名字
                            new XElement("index", HandoverModel.GetInstance.scodeIndex),  //下拉下标，方便下次自动选中此下标位置
                            new XElement("bcode", HandoverModel.GetInstance.bcode.ToString()),  //机号
                            new XElement("istorepath", HandoverModel.GetInstance.istorePath),  //库存报表路径
                            new XElement("call", HandoverModel.GetInstance.Call),  //客服专线
                            new XElement("address", HandoverModel.GetInstance.Address),  //地址
                            new XElement("remark1", HandoverModel.GetInstance.Remark1),  //备注1
                            new XElement("remark2", HandoverModel.GetInstance.Remark2),  //备注2
                            new XElement("remark3", HandoverModel.GetInstance.Remark3),  //备注3
                            new XElement("remark4", HandoverModel.GetInstance.Remark4),  //备注4
                            new XElement("printtitle", HandoverModel.GetInstance.PrintTitle),  //打印标题

                            new XElement("printfont", HandoverModel.GetInstance.PrintFont),  //打印字体
                            new XElement("fontsize", HandoverModel.GetInstance.FontSize),  //字体大小
                            new XElement("pagewidth", HandoverModel.GetInstance.PageWidth),  //打印页面宽度
                            new XElement("pageheight", HandoverModel.GetInstance.PageHeight),  //打印页面高度
                            new XElement("printcopies", HandoverModel.GetInstance.PrintCopies),  //打印份数
                            new XElement("ctime", System.DateTime.Now.ToShortDateString())
                        );

                        el.Save(logPath);
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("登录时读取用户配置信息时发生异常:", ex);
                MessageBox.Show("读取本地用户配置失败！请尝试忽略此警告，继续运行软件后进入系统设置并保存一次配置，必要时请联系管理员！");

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

                    MyEFDB.serverAdd = EncodeAndDecode.DecodeBase64(products.Element("server").Value);
                    MyEFDB.dadaBaseName = EncodeAndDecode.DecodeBase64(products.Element("dadaBase").Value);
                    MyEFDB.usrid = EncodeAndDecode.DecodeBase64(products.Element("usr").Value);
                    MyEFDB.wd = EncodeAndDecode.DecodeBase64(products.Element("wd").Value);

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

        private void button2_Click(object sender, EventArgs e)
        {
            if (UserLoginById())
            {
                this.Hide();

            }
        }









    }
}
