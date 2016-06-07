using hjn20160520._1_Exchange;
using hjn20160520._4_Detail;
using hjn20160520._7_Attend;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hjn20160520
{
    public partial class MainForm : Form
    {

        //1-收银交班窗口
        exchangeForm exForm;

        //2-前台收银窗口
        Cashiers cashierForm;

        //4-销售明细查询窗口
        detailForm DLForm;

        //7-员工考勤窗口
        attendForm attForm;



        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            //全屏
            //if (this.WindowState == FormWindowState.Maximized)
            //{
            //    this.WindowState = FormWindowState.Normal;
            //}
            //else
            //{
            //    this.FormBorderStyle = FormBorderStyle.None;
            //    this.WindowState = FormWindowState.Maximized;
            //    this.TopMost = true;  //窗口顶置
            //}

            DLForm = new detailForm();
            cashierForm = new Cashiers();
            exForm = new exchangeForm();
            attForm = new attendForm();


        }


        #region 2前台收银
        private void button2_Enter(object sender, EventArgs e)
        {
            this.label2.Visible = true;
            //((Button)sender).ForeColor = Color.White;
        }

        private void button2_Leave(object sender, EventArgs e)
        {
            this.label2.Visible = false;
            //((Button)sender).ForeColor = Color.Black;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            cashierForm.Show();
            this.Hide();

        }

        #endregion

        #region 1前台交班
        private void button1_Click(object sender, EventArgs e)
        {
            exForm.ShowDialog();
            this.Hide();
        }

        private void button1_Enter(object sender, EventArgs e)
        {
            this.label1.Visible = true;
            //this.button1.ForeColor = Color.White;
        }

        private void button1_Leave(object sender, EventArgs e)
        {
            this.label1.Visible = false;
            //this.button1.ForeColor = Color.Black;
        }
        #endregion

        #region 3前台当班
        private void button3_Click(object sender, EventArgs e)
        {

        }


        private void button3_Leave(object sender, EventArgs e)
        {
            this.label3.Visible = false;
            //this.button3.ForeColor = Color.Black;
        }

        private void button3_Enter(object sender, EventArgs e)
        {
            this.label3.Visible = true;
            //this.button3.ForeColor = Color.White;
        }
        #endregion


        #region 4销售明细
        private void button4_Click(object sender, EventArgs e)
        {
            DLForm.ShowDialog();
            this.Hide();
        }

        private void button4_Leave(object sender, EventArgs e)
        {
            this.label4.Visible = false;
            //this.button4.ForeColor = Color.Black;


        }

        private void button4_Enter(object sender, EventArgs e)
        {
            this.label4.Visible = true;
            //this.button4.ForeColor = Color.White;
        }

        #endregion


        #region 5参数设置
        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void button5_Enter(object sender, EventArgs e)
        {
            this.label5.Visible = true;
            //this.button5.ForeColor = Color.White;
        }

        private void button5_Leave(object sender, EventArgs e)
        {
            this.label5.Visible = false;
            //this.button5.ForeColor = Color.Black;
        }
           #endregion


        #region 6练习收银
        private void button6_Click(object sender, EventArgs e)
        {

        }

        private void button6_Enter(object sender, EventArgs e)
        {
            this.label6.Visible = true;
            //this.button6.ForeColor = Color.White;
        }

        private void button6_Leave(object sender, EventArgs e)
        {
            this.label6.Visible = false;
            //this.button6.ForeColor = Color.Black;
        }
           #endregion

        #region 7员工考勤
        private void button7_Click(object sender, EventArgs e)
        {
            attForm.ShowDialog();
            this.Hide();
        }

        private void button7_Enter(object sender, EventArgs e)
        {
            this.label7.Visible = true;
            //this.button7.ForeColor = Color.White;
        }

        private void button7_Leave(object sender, EventArgs e)
        {
            this.label7.Visible = false;
            //this.button7.ForeColor = Color.Black;
        }
           #endregion

        #region 8补货申请
        private void button8_Click(object sender, EventArgs e)
        {

        }

        private void button8_Enter(object sender, EventArgs e)
        {
            this.label8.Visible = true;
            //this.button8.ForeColor = Color.White;
        }

        private void button8_Leave(object sender, EventArgs e)
        {
            this.label8.Visible = false;
            //this.button8.ForeColor = Color.Black;
        }
        #endregion

        #region 9办理会员
        private void button9_Click(object sender, EventArgs e)
        {

        }

        private void button9_Enter(object sender, EventArgs e)
        {
            this.label9.Visible = true;
            //this.button9.ForeColor = Color.White;
        }

        private void button9_Leave(object sender, EventArgs e)
        {
            this.label9.Visible = false;
            //this.button9.ForeColor = Color.Black;
        }
        #endregion

        #region 10退出系统
        private void button10_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button10_Enter(object sender, EventArgs e)
        {
            this.label10.Visible = true;
            //this.button10.ForeColor = Color.White;
        }

        private void button10_Leave(object sender, EventArgs e)
        {
            this.label10.Visible = false;
            //this.button10.ForeColor = Color.Black;
        }
        #endregion




        //热键
        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
        {
            int WM_KEYDOWN = 256;
            int WM_SYSKEYDOWN = 260;
            if (msg.Msg == WM_KEYDOWN | msg.Msg == WM_SYSKEYDOWN)
            {
                switch (keyData)
                {

                    //回车
                    case Keys.Enter:


                        break;
                    //退出
                    case Keys.Escape:

                        this.button10_Click(null, null);
                        break;

                }

            }
            return false;
        }







    }
}
