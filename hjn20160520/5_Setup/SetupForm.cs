using Common;
using hjn20160520.Common;
using hjn20160520.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace hjn20160520._5_Setup
{
    public partial class SetupForm : Form
    {

        public SetupForm()
        {
            InitializeComponent();
            this.ControlBox = false;
        }

        private void SetupForm_Load(object sender, EventArgs e)
        {

            ShowScodeFunc();

        }


        //更换标签标题的背景色（目前效果不好，暂弃）
        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            //try
            //{
            //    //修改为自定义绘制
            //    this.tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
            //    //色泽
            //    var black = new SolidBrush(Color.Black);
            //    var white = new SolidBrush(Color.White);

            //    //文字格式，居中对齐
            //    var strFormat = new StringFormat();
            //    strFormat.Alignment = StringAlignment.Center;

            //    //设置文字的背景色
            //    Rectangle rec1 = tabControl1.GetTabRect(0);
            //    e.Graphics.FillRectangle(black, rec1);
            //    Rectangle rec2 = tabControl1.GetTabRect(1);
            //    e.Graphics.FillRectangle(black, rec2);

            //    //开始绘制
            //    for (int i = 0; i < tabControl1.TabPages.Count; i++)
            //    {
            //        Rectangle rec = tabControl1.GetTabRect(i);
            //        e.Graphics.DrawString(tabControl1.TabPages[i].Text, new Font("宋体", 10), white, rec, strFormat);
            //    }
            //}
            //catch
            //{

            //}
        }


        #region 热键注册

        //重写热键方法
        private void SetupForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:

                    this.Close();

                    break;


                //回车
                case Keys.Enter:
                    SvaeConfigFunc(@"../");
                    SaveScodeFunc();
                    this.Close();
                    break;

            }
        }


        //保存分店号 、机号
        private void SaveScodeFunc()
        {
            try
            {
                //int? scode2 = comboBox1.SelectedValue as int?;   //分店号
                //HandoverModel.GetInstance.scode = scode2.HasValue ? scode2.Value : 0;
                HandoverModel.GetInstance.scode = int.Parse(comboBox1.SelectedValue.ToString());  //取值分店号
                HandoverModel.GetInstance.scodeName = comboBox1.SelectedText;
                int bcode2 = 0; //机号
                if (int.TryParse(textBox12.Text.Trim(), out bcode2))
                {
                    HandoverModel.GetInstance.bcode = bcode2;
                    //HandoverModel.GetInstance.isSetCode = true;
                }

            }
            catch
            {
                MessageBox.Show("分店保存失败");
            }

        }


        #endregion

        private void label25_Click(object sender, EventArgs e)
        {

        }

        //是否有打印机
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.panel2.Visible = !this.panel2.Visible;
        }
        //是否有顾客显示屏
        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            this.panel3.Visible = !this.panel3.Visible;

        }
        //是否有钱箱
        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            this.panel4.Visible = !this.panel4.Visible;

        }
        //设置单尾脚注
        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            this.panel5.Visible = !this.panel5.Visible;

        }
        //是否双屏
        private void checkBox13_CheckedChanged(object sender, EventArgs e)
        {
            this.panel7.Visible = !this.panel7.Visible;

        }
        //是否有串口直连电子称
        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            this.panel8.Visible = !this.panel8.Visible;

        }

        private void textBox11_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b' && !Char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }



        /// <summary>
        /// 保存XML配置文件
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
                                new XElement("scode", comboBox1.SelectedValue),  //分店
                                new XElement("cname", comboBox1.SelectedText),  //分店名字
                                new XElement("index", comboBox1.SelectedIndex),  //下拉下标，方便下次自动选中此下标位置
                                new XElement("bcode", textBox12.Text),  //机号
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
                            new XElement("scode", comboBox1.SelectedValue),
                            new XElement("cname", comboBox1.SelectedText),  //分店名字
                            new XElement("index", comboBox1.SelectedIndex),  //下拉下标，方便下次自动选中此下标位置
                            new XElement("bcode", textBox12.Text),  //机号
                            new XElement("ctime", System.DateTime.Now.ToShortDateString())
                        );

                        el.Save(logPath);
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("系统设置保存分店信息时发生异常:", ex);

            }
        }


        //读取本地用户配置XML
        private void GetUserConfig(string logPath)
        {
            try
            {
                if (!File.Exists(logPath)) return;

                XElement el = XElement.Load(logPath);

                var products = el.Elements("user").Where(e => e.Attribute("ID").Value == "1").FirstOrDefault();
                if (products != null)
                {
                    textBox12.Text = products.Element("bcode").Value;
                    int index_temp = 0;
                    int.TryParse(products.Element("index").Value, out index_temp);
                    comboBox1.SelectedIndex = index_temp;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("系统设置读取XML分店信息时发生异常:", ex);
            }
        }


        //从数据库读取所有分店信息给下拉框
        private void ShowScodeFunc()
        {
            try
            {

                using (var db = new hjnbhEntities())
                {
                    var infos = db.hd_dept_info.AsNoTracking().Select(t => new { t.scode, t.cname }).ToList();
                    if (infos.Count > 0)
                    {
                        DataTable dt = new DataTable();
                        DataColumn dc1 = new DataColumn("id");
                        DataColumn dc2 = new DataColumn("name");
                        dt.Columns.Add(dc1);
                        dt.Columns.Add(dc2);

                        foreach (var item in infos)
                        {
                            DataRow dr1 = dt.NewRow();
                            dr1["id"] = item.scode;
                            dr1["name"] = item.cname;
                            dt.Rows.Add(dr1);
                        }

                        comboBox1.DataSource = dt;
                        comboBox1.ValueMember = "id";  //值字段
                        comboBox1.DisplayMember = "name";   //显示的字段
                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("系统设置在线读取分店信息时发生异常:", ex);
                MessageBox.Show("数据库连接出错！");
                string tip = ConnectionHelper.ToDo();
                if (!string.IsNullOrEmpty(tip))
                {
                    MessageBox.Show(tip);
                }
            }
            finally
            {
                GetUserConfig(@"../UserConfig.xml");
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            RegistryKey rk2 = null;
            try
            {
                if (checkBox4.Checked) //设置开机自启动  
                {
                    MessageBox.Show("设置开机自启动，需要修改注册表", "提示");
                    string path = Application.ExecutablePath; //本程序路径

                    rk2 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                    if (rk2 == null)
                    {
                        rk2 = Registry.LocalMachine.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                    }

                    rk2.SetValue("HJNPos", path);


                }
                else //取消开机自启动  
                {
                    MessageBox.Show("取消开机自启动，需要修改注册表", "提示");
                    string path = Application.ExecutablePath;

                    rk2 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                    if (rk2 == null)
                    {
                        rk2 = Registry.LocalMachine.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                    }

                    rk2.DeleteValue("HJNPos", false);

                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("系统设置软件开机启动时发生异常:", ex);
                MessageBox.Show("设置开机自启动出错，请确认是否被安全软件拦截？");
            }
            finally
            {
                if (rk2 != null)
                {
                    rk2.Close();
                }
            }
        }

        //初始化默认设置
        private void button1_Click(object sender, EventArgs e)
        {

        }






    }
}
