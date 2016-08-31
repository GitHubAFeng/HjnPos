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


                ////回车
                //case Keys.Enter:
                //    SvaeConfigFunc(@"../");
                //    SaveScodeFunc();
                //    this.Close();
                //    break;

            }


            //保存配置
            if ((e.KeyCode == Keys.S) && e.Control)
            {
                try
                {
                    SvaeConfigFunc(@"../");
                    SaveScodeFunc(); //立即保存分店号
                    SavePrintFunc(); //立即保存小票脚注
                    MessageBox.Show("系统设置保存成功！");
                }

                catch (Exception ex)
                {
                    LogHelper.WriteLog("系统设置保存分店信息时发生异常:", ex);
                    MessageBox.Show("系统设置保存失败，请尝试重启软件，必要时请联系管理员！");
                }
            }

        }


        //保存小票脚注
        private void SavePrintFunc()
        {
            try
            {
                HandoverModel.GetInstance.Call = textBox4.Text.Trim();
                HandoverModel.GetInstance.Address = textBox5.Text.Trim();
                HandoverModel.GetInstance.Remark1 = textBox7.Text.Trim();
                HandoverModel.GetInstance.Remark2 = textBox6.Text.Trim();
            }
            catch
            {
                MessageBox.Show("保存小票脚注失败，请尝试重启软件，必要时请联系管理员！");
            }
        }


        //保存分店号 、机号
        private void SaveScodeFunc()
        {
            try
            {
                HandoverModel.GetInstance.scode = int.Parse(comboBox1.SelectedValue.ToString());  //取值分店号
                HandoverModel.GetInstance.scodeName = comboBox1.Text;
                int bcode2 = 0; //机号
                if (int.TryParse(textBox12.Text.Trim(), out bcode2))
                {
                    HandoverModel.GetInstance.bcode = bcode2;
                }

            }
            catch
            {
                MessageBox.Show("分店保存失败，请尝试重启软件，必要时请联系管理员！");
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


                if (string.IsNullOrEmpty(istorepath))
                {
                    istorepath = textBox13.Text.Trim();
                }


                if (!File.Exists(logPath))
                {

                    //没有文档就新建
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
                                new XElement("cname", comboBox1.Text),  //分店名字
                                new XElement("index", comboBox1.SelectedIndex),  //下拉下标，方便下次自动选中此下标位置
                                new XElement("bcode", textBox12.Text.Trim()),  //机号
                                new XElement("istorepath", istorepath),  //库存报表路径
                                new XElement("call", textBox4.Text.Trim()),  //客服专线
                                new XElement("address", textBox5.Text.Trim()),  //地址
                                new XElement("remark1", textBox7.Text.Trim()),  //备注1
                                new XElement("remark2", textBox6.Text.Trim()),  //备注2
                                new XElement("ctime", System.DateTime.Now.ToShortDateString())
                            )
                        )
                    );
                    // 保存为XML文件
                    doc.Save(logPath);
                }
                else
                {

                    //如果已经有文档                
                    XElement el = XElement.Load(logPath);

                    var products = el.Elements("user").Where(e => e.Attribute("ID").Value == "1").FirstOrDefault();
                    if (products != null)
                    {
                        products.SetAttributeValue("ID", 1);
                        products.ReplaceNodes
                        (
                            new XElement("scode", comboBox1.SelectedValue),
                            new XElement("cname", comboBox1.Text),  //分店名字
                            new XElement("index", comboBox1.SelectedIndex),  //下拉下标，方便下次自动选中此下标位置
                            new XElement("bcode", textBox12.Text),  //机号
                            new XElement("istorepath", istorepath),  //库存报表路径
                            new XElement("call", textBox4.Text.Trim()),  //客服专线
                            new XElement("address", textBox5.Text.Trim()),  //地址
                            new XElement("remark1", textBox7.Text.Trim()),  //备注1
                            new XElement("remark2", textBox6.Text.Trim()),  //备注2
                            new XElement("ctime", System.DateTime.Now.ToShortDateString())
                        );

                        el.Save(logPath);
                    }

                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("系统设置保存分店信息时发生异常:", ex);
                MessageBox.Show("系统设置保存分店信息时发生异常，请尝试重启软件，必要时请联系管理员！");
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

                    textBox13.Text = products.Element("istorepath").Value;

                    textBox4.Text = products.Element("call").Value;
                    textBox5.Text = products.Element("address").Value;
                    textBox7.Text = products.Element("remark1").Value;
                    textBox6.Text = products.Element("remark2").Value;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("系统设置读取XML分店信息时发生异常:", ex);
                MessageBox.Show("系统设置读取失败，请尝试忽略此警告，进入系统设置后并保存一次配置，必要时请联系管理员！");
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
                MessageBox.Show("系统设置在线读取分店信息时发生异常，请尝试重启软件，必要时请联系管理员！");
                //string tip = ConnectionHelper.ToDo();
                //if (!string.IsNullOrEmpty(tip))
                //{
                //    MessageBox.Show(tip);
                //}
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

        //库存保存路径
        string istorepath = string.Empty;
        //库存提醒报表默认保存路径
        private void button2_Click(object sender, EventArgs e)
        {

            FolderBrowserDialog BrowDialog = new FolderBrowserDialog();
            BrowDialog.ShowNewFolderButton = true;
            BrowDialog.Description = "请选择保存位置";
            DialogResult result = BrowDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                HandoverModel.GetInstance.istorePath = BrowDialog.SelectedPath;
                textBox13.Text = BrowDialog.SelectedPath;
                istorepath = BrowDialog.SelectedPath;
            }


        }






    }
}
