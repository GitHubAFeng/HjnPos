﻿using hjn20160520.Models;
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
        MainForm mainForm;

        public SetupForm()
        {
            InitializeComponent();
            this.ControlBox = false;
        }

        private void SetupForm_Load(object sender, EventArgs e)
        {
            //this.FormBorderStyle = FormBorderStyle.None;
            mainForm = new MainForm();
        }

        private void label20_Click(object sender, EventArgs e)
        {

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
                //删除DEL键
                case Keys.Escape:

                    mainForm.Show();
                    this.Close();

                    break;


                //回车
                case Keys.Enter:
                    SvaeConfigFunc(@"E:\text");
                    SaveScodeFunc();
                    this.Close();
                    break;

            }
        }


        //保存分店号 、机号
        private void SaveScodeFunc()
        {
            if (!string.IsNullOrEmpty(textBox11.Text.Trim()))
            {
                HandoverModel.GetInstance.scode = int.Parse(textBox11.Text.Trim());
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

            if (!System.IO.Directory.Exists((path)))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            string logPath = path + "Log.xml";

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
                            new XElement("scode", textBox11.Text),
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
                        new XElement("scode", textBox11.Text),
                        new XElement("ctime", System.DateTime.Now.ToShortDateString())
                    );

                    el.Save(logPath);
                }
                //else
                //{
                //    MessageBox.Show("没找到");
                //}

            }
        }






    }
}
