using hjn20160520._1_Exchange;
using hjn20160520._2_Cashiers;
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
using System.Windows.Forms;

namespace hjn20160520
{
    public partial class MainFormXP : Form
    {
       
        //1-收银交班窗口
        //exchangeForm exForm;

        //3-当班窗口
        //DutyWorkForm DWForm;
        //4-销售明细查询窗口
        detailForm DLForm;

        //参数设置窗口
        SetupForm setupForm;

        //7-员工考勤窗口
        attendForm attForm;
        //8-补货申请窗口
        ReplenishRequestForm RRForm;


        //信息提示窗口
        TipForm tipForm;



        public MainFormXP()
        {
            InitializeComponent();

        }

        private void MainFormXP_KeyDown(object sender, KeyEventArgs e)
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

        private void MainFormXP_Load(object sender, EventArgs e)
        {

            this.Hellolabel.Text = "您好，" + HandoverModel.GetInstance.userName;

            //全屏
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            //this.TopMost = true;  //窗口顶置
            HandoverModel.GetInstance.isPrint = true;   //默认打印为开


            DLForm = new detailForm();
            //exForm = new exchangeForm();
            attForm = new attendForm();
            RRForm = new ReplenishRequestForm();
            setupForm = new SetupForm();
            //VIPForm = new VIPCardForm();

            this.VisibleChanged += MainFormXP_VisibleChanged;
        }

        void MainFormXP_VisibleChanged(object sender, EventArgs e)
        {
            if (HandoverModel.GetInstance.isWorking == true)
            {
                label11.Text = "正在当班中…";
                //button2.Focus();
            }
            else
            {
                label11.Text = "您还未当班";
            }
        }

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
                DutyWorkForm DWForm = new DutyWorkForm();
                DWForm.ShowDialog(this);
                //自动输入向下
                //SendKeys.Send("{NumPad2}");
                button2.Focus();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (HandoverModel.GetInstance.isWorking)
            {
                //2-前台收银窗口
                CashiersFormXP cashierForm = new CashiersFormXP();

                cashierForm.Show();
                this.Hide();

            }
            else
            {
                tipForm = new TipForm();
                tipForm.Tiplabel.Text = "您还没有当班，请先当班后才可以开始收银！";
                tipForm.ShowDialog();
                //自动输入向上
                //SendKeys.Send("{UP}");
                //SendKeys.Send("{NumPad1}");
                button1.Focus();
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (HandoverModel.GetInstance.isWorking)
            {
                exchangeForm exForm = new exchangeForm();

                exForm.ShowDialog(this);
            }
            else
            {
                tipForm = new TipForm();
                tipForm.Tiplabel.Text = "您当前还没有当班，不能进行交班操作！";
                tipForm.ShowDialog();
                //自动输入向上
                //SendKeys.Send("{NumPad1}");
                //SendKeys.Send("{UP}");
                button1.Focus();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DLForm.ShowDialog();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            setupForm.ShowDialog();

        }

        private void button6_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("暂不可用");
            //var LX = new Cashiers();
            //LX.isLianXi = true;
            //LX.ShowDialog();
            //开启练习模式(该模式下不允许进行新增与修改数据的操作)
            CashiersFormXP LXFrom = new CashiersFormXP();
            LXFrom.isLianXi = true;
            LXFrom.Show();
            this.Hide();

        }

        private void button7_Click(object sender, EventArgs e)
        {
            attForm.ShowDialog();

        }

        private void button8_Click(object sender, EventArgs e)
        {
            RRForm.ShowDialog();

        }

        private void button9_Click(object sender, EventArgs e)
        {
            //9-会员办理窗口
            VIPCardForm VIPForm = new VIPCardForm();
            VIPForm.ShowDialog();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (HandoverModel.GetInstance.isWorking)
            {
                MessageBox.Show("您目前当班中，请先交班后再退出软件！");
            }
            else
            {
                if (DialogResult.Yes == MessageBox.Show("是否确认退出软件？", "提醒", MessageBoxButtons.YesNo))
                {
                    Application.Exit();                
                }
            }
        }


        //当班
        private void button1_Enter(object sender, EventArgs e)
        {
            this.label1.Visible = true;

        }

        private void button1_Leave(object sender, EventArgs e)
        {
            this.label1.Visible = false;

        }

        private void button2_Enter(object sender, EventArgs e)
        {
            this.label2.Visible = true;

        }

        private void button2_Leave(object sender, EventArgs e)
        {
            this.label2.Visible = false;

        }

        private void button3_Enter(object sender, EventArgs e)
        {
            this.label3.Visible = true;

        }

        private void button3_Leave(object sender, EventArgs e)
        {
            this.label3.Visible = false;

        }

        private void button4_Enter(object sender, EventArgs e)
        {
            this.label4.Visible = true;

        }

        private void button4_Leave(object sender, EventArgs e)
        {
            this.label4.Visible = false;

        }

        private void button5_Enter(object sender, EventArgs e)
        {
            this.label5.Visible = true;

        }

        private void button5_Leave(object sender, EventArgs e)
        {
            this.label5.Visible = false;

        }

        private void button6_Enter(object sender, EventArgs e)
        {
            this.label6.Visible = true;

        }

        private void button6_Leave(object sender, EventArgs e)
        {
            this.label6.Visible = false;

        }

        private void button7_Enter(object sender, EventArgs e)
        {
            this.label7.Visible = true;

        }

        private void button7_Leave(object sender, EventArgs e)
        {
            this.label7.Visible = false;

        }

        private void button8_Enter(object sender, EventArgs e)
        {
            this.label8.Visible = true;

        }

        private void button8_Leave(object sender, EventArgs e)
        {
            this.label8.Visible = false;

        }

        private void button9_Enter(object sender, EventArgs e)
        {
            this.label9.Visible = true;

        }

        private void button9_Leave(object sender, EventArgs e)
        {
            this.label9.Visible = false;

        }

        private void button10_Enter(object sender, EventArgs e)
        {
            this.label10.Visible = true;

        }

        private void button10_Leave(object sender, EventArgs e)
        {
            this.label10.Visible = false;

        }

        private void MainFormXP_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.VisibleChanged -= MainFormXP_VisibleChanged;

        }























    }
}
