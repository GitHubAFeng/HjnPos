using hjn20160520._1_Exchange;
using hjn20160520._3_DutyWork;
using hjn20160520._4_Detail;
using hjn20160520._5_Setup;
using hjn20160520._7_Attend;
using hjn20160520._8_ReplenishRequest;
using hjn20160520._9_VIPCard;
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

namespace hjn20160520
{
    public partial class MainForm : Form
    {

        //1-收银交班窗口
        exchangeForm exForm;

        //2-前台收银窗口
        Cashiers cashierForm;
        //3-当班窗口
        DutyWorkForm DWForm;
        //4-销售明细查询窗口
        detailForm DLForm;

        //参数设置窗口
        SetupForm setupForm;

        //7-员工考勤窗口
        attendForm attForm;
        //8-补货申请窗口
        ReplenishRequestForm RRForm;
        //9-会员办理窗口
        VIPCardForm VIPForm;
      
        //信息提示窗口
        TipForm tipForm;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //全屏
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            //this.TopMost = true;  //窗口顶置


            DLForm = new detailForm();
            cashierForm = new Cashiers();
            exForm = new exchangeForm();
            attForm = new attendForm();
            RRForm = new ReplenishRequestForm();
            setupForm = new SetupForm();
            VIPForm = new VIPCardForm();

            label11.Text = "";
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
            if (HandoverModel.GetInstance.isWorking)
            {
                cashierForm.ShowDialog();
                //this.Hide();
            }
            else
            {
                tipForm = new TipForm();
                tipForm.Tiplabel.Text = "您还没有当班，请先当班后才可以开始收银！";
                tipForm.ShowDialog();
            }


        }

        #endregion

        #region 1前台当班
        private void button1_Click(object sender, EventArgs e)
        {
            if (HandoverModel.GetInstance.isWorking)
            {
                tipForm = new TipForm();
                tipForm.Tiplabel.Text = HandoverModel.GetInstance.userID.ToString() + "员工已在当班中，请先交班后才可当班！";
                tipForm.ShowDialog();
            }
            else
            {
                DWForm = new DutyWorkForm();
                DWForm.ShowDialog();
            }

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

        #region 3前台交班
        private void button3_Click(object sender, EventArgs e)
        {
            if (HandoverModel.GetInstance.isWorking)
            {
                exForm.ShowDialog();
            }
            else
            {
                tipForm = new TipForm();
                tipForm.Tiplabel.Text = "您当前还没有当班，不能进行交班操作！";
                tipForm.ShowDialog();
            }
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
            setupForm.ShowDialog();
            this.Hide();
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
            RRForm.ShowDialog();
            this.Hide();
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
            VIPForm.ShowDialog();
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
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:

                    break;

                case Keys.Escape:
                    this.button10_Click(null, null);

                    break;

                case Keys.NumPad1:
                    button1_Click(null, null);
                    break;
                case Keys.NumPad2:
                    button2_Click(null, null);
                    break;
                case Keys.NumPad3:
                    button3_Click(null, null);
                    break;
                case Keys.NumPad4:
                    button4_Click(null, null);
                    break;
                case Keys.NumPad5:
                    button5_Click(null, null);
                    break;
                case Keys.NumPad6:
                    button6_Click(null, null);
                    break;
                case Keys.NumPad7:
                    button7_Click(null, null);
                    break;
                case Keys.NumPad8:
                    button8_Click(null, null);
                    break;
                case Keys.NumPad9:
                    button9_Click(null, null);
                    break;
                case Keys.NumPad0:
                    button10_Click(null, null);
                    break;
            }
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            if (HandoverModel.GetInstance.isWorking == true)
            {
                label11.Text = "当班中";
                //button2.Focus();
            }
        }







    }
}
