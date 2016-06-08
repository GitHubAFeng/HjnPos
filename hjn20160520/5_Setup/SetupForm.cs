using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
        {
            int WM_KEYDOWN = 256;
            int WM_SYSKEYDOWN = 260;
            if (msg.Msg == WM_KEYDOWN | msg.Msg == WM_SYSKEYDOWN)
            {
                switch (keyData)
                {
                    //删除DEL键
                    case Keys.Escape:

                        mainForm.Show();
                        this.Close();

                        break;


                    //回车
                    case Keys.Enter:


                        break;

                }

            }
            return false;
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










    }
}
