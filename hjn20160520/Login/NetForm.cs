using Common;
using hjn20160520.Common;
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

namespace hjn20160520.Login
{
    public partial class NetForm : Form
    {

        //用于其它窗口传值给本窗口控件
        //这是委托与事件的第一步  
        //public delegate void NetFormHandle(string server, string dadaBase, string usr, string wd);
        //public event NetFormHandle changed; 

        public NetForm()
        {
            InitializeComponent();
        }



        private void NetForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.Close();
                    break;

                case Keys.F1:
                    TestConnFunc();
                    break;

                case Keys.F2:
                    SvaeConfigFunc();
                    break;



            }
        }


        //传值
        //private void CFunc()
        //{


        //    changed(textBox1.Text.Trim(), textBox2.Text.Trim(), textBox3.Text.Trim(), textBox4.Text.Trim());

        //}

        /// <summary>
        /// 保存XML配置文件
        /// </summary>
        /// <param name="path">目录路径</param>
        private void SvaeConfigFunc(string path = @"../")
        {
            try
            {

            if (!System.IO.Directory.Exists((path)))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            string logPath = path + "DBConfig.xml";

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
                            "DB",
                            new XAttribute("ID", 1),
                            new XElement("server", textBox1.Text.Trim()),
                            new XElement("usr", textBox3.Text.Trim()),
                            new XElement("wd", textBox4.Text.Trim()),
                            new XElement("dadaBase", textBox2.Text.Trim()),
                            new XElement("ctime", System.DateTime.Now.ToShortDateString())
                        )
                    )
                );
                // 保存为XML文件
                doc.Save(logPath);

                MyEFDB.serverAdd = textBox1.Text.Trim();
                MyEFDB.usrid = textBox3.Text.Trim();
                MyEFDB.wd = textBox4.Text.Trim();
                MyEFDB.dadaBaseName = textBox2.Text.Trim();
                MessageBox.Show("保存成功！");
            }
            else
            {
                XElement el = XElement.Load(logPath);

                var products = el.Elements("DB").Where(e => e.Attribute("ID").Value == "1").FirstOrDefault();
                if (products != null)
                {
                    products.SetAttributeValue("ID", 1);
                    products.ReplaceNodes
                    (
                            new XElement("server", textBox1.Text.Trim()),
                            new XElement("usr", textBox3.Text.Trim()),
                            new XElement("wd", textBox4.Text.Trim()),
                            new XElement("dadaBase", textBox2.Text.Trim()),
                            new XElement("ctime", System.DateTime.Now.ToShortDateString())
                    );

                    el.Save(logPath);

                    MyEFDB.serverAdd = textBox1.Text.Trim();
                    MyEFDB.usrid = textBox3.Text.Trim();
                    MyEFDB.wd = textBox4.Text.Trim();
                    MyEFDB.dadaBaseName = textBox2.Text.Trim();
                    MessageBox.Show("保存成功！");
                }

            }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("保存网络服务器配置时发生异常:", ex);
                MessageBox.Show("保存失败！");

            }
        }



        //测试连接
        private void TestConnFunc()
        {
            try
            {

                MyEFDB.serverAdd = textBox1.Text.Trim();
                MyEFDB.usrid = textBox3.Text.Trim();
                MyEFDB.wd = textBox4.Text.Trim();
                MyEFDB.dadaBaseName = textBox2.Text.Trim();

                using (var db = new hjnbhEntities())
                {
                    var re = db.dept.AsNoTracking().Take(10).FirstOrDefault();
                    MessageBox.Show("连接成功!");
                    //MessageBox.Show(re.id.ToString());
                }
            }
            catch (Exception)
            {

                MessageBox.Show("连接失败!");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TestConnFunc();
        }

        private void NetForm_Load(object sender, EventArgs e)
        {
            try
            {
                textBox1.Text = MyEFDB.serverAdd;
                textBox2.Text = MyEFDB.dadaBaseName;
                textBox3.Text = MyEFDB.usrid;
                textBox4.Text = MyEFDB.wd;
            }
            catch 
            {
               
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SvaeConfigFunc();
        }





    }
}
