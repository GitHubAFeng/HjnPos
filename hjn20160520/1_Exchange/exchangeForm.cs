using Common;
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

namespace hjn20160520._1_Exchange
{
    /// <summary>
    /// 交班
    /// </summary>
    public partial class exchangeForm : Form
    {

        public delegate void exchangeFormHandle();
        public event exchangeFormHandle UIChanged;  //UI更新事件
        string PCname = "";  //计算机名字


        public exchangeForm()
        {
            InitializeComponent();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void exchangeForm_Load(object sender, EventArgs e)
        {

            //计时器有延迟，防止用户快速交班而计时器还未开始运行时会发重新错误
            label16.Text = System.DateTime.Now.ToString();
            HandoverModel.GetInstance.ClosedTime = System.DateTime.Now;

            ShowUI();

            PCname = HandoverModel.GetInstance.pc_name;
        }

        //热键
        private void exchangeForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    EXFunc();
                    break;

                case Keys.Escape:
                    this.Close();
                    break;
            }
        }


        DateTime time_temp = new DateTime();  //交班时间

        //处理交班逻辑
        private void EXFunc()
        {
            try
            {
                if (HandoverModel.GetInstance.isWorking == false)
                {
                    MessageBox.Show("您还未当班，不能进行交班操作！");
                    return;
                }
                if (time_temp == DateTime.MinValue)
                {
                    time_temp = System.DateTime.Now;
                }

                using (var db = new hjnbhEntities())
                {
                    var JBInfo = new hd_dborjb
                    {
                        scode = HandoverModel.GetInstance.scode,
                        bcode = HandoverModel.GetInstance.bcode,
                        usr_id = HandoverModel.GetInstance.userID,
                        pc_name = PCname,
                        pc_code = HandoverModel.GetInstance.pc_code,
                        cname = HandoverModel.GetInstance.userName,
                        dbje = HandoverModel.GetInstance.SaveMoney,
                        dtime = HandoverModel.GetInstance.workTime,
                        jbje = HandoverModel.GetInstance.Money,
                        jcount = HandoverModel.GetInstance.OrderCount,
                        tkje = HandoverModel.GetInstance.RefundMoney,//退款
                        qkje = HandoverModel.GetInstance.DrawMoney,//中途提款
                        jtime = time_temp,
                        item_count = HandoverModel.GetInstance.AllCount,
                        all_je = HandoverModel.GetInstance.AllJe
                    };
                    db.hd_dborjb.Add(JBInfo);
                    var re = db.SaveChanges();
                    if (re > 0)
                    {
                        timer1.Enabled = false;  //停止计时
                        HandoverModel.GetInstance.ClosedTime = time_temp;
                        HandoverModel.GetInstance.isWorking = false;
                        JiaoBanPrinter jbprint = new JiaoBanPrinter(JBInfo.id.ToString());
                        jbprint.StartPrint();

                        if (DialogResult.Yes == MessageBox.Show("交班成功！以防他人冒用帐号，请及时退出登陆，是否现在退出本软件？", "提醒", MessageBoxButtons.YesNo))
                        {
                            Application.Exit();
                        }
                        else
                        {
                            InitData();  //重置
                            UIChanged();  //通知交班成功
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("交班界面进行交班时发生异常:", ex);
                MessageBox.Show("交班时出现异常，请先检查数据是否正常，必要时请联系管理员！");
            }
        }
        //刷新UI
        private void ShowUI()
        {
            //收银员工号
            label20.Text = HandoverModel.GetInstance.userID.ToString("0.00");
            //收银机号
            label19.Text = HandoverModel.GetInstance.bcode.ToString("0.00");
            //当班时间
            label18.Text = HandoverModel.GetInstance.workTime.ToString("0.00");
            //当班金额
            label17.Text = HandoverModel.GetInstance.SaveMoney.ToString("0.00");

            //交易单数
            label15.Text = HandoverModel.GetInstance.OrderCount.ToString("0.00");
            //退款金额
            label14.Text = HandoverModel.GetInstance.RefundMoney.ToString("0.00");
            //中途提款
            label13.Text = HandoverModel.GetInstance.DrawMoney.ToString("0.00");
            //现金
            label4.Text = HandoverModel.GetInstance.CashMoney.ToString("0.00");
            //银联卡
            label23.Text = HandoverModel.GetInstance.paycardMoney.ToString("0.00");
            //礼券
            label26.Text = HandoverModel.GetInstance.LiQuanMoney.ToString("0.00");
            //储值卡
            label27.Text = HandoverModel.GetInstance.VipCardMoney.ToString("0.00");
            //移动支付
            label32.Text = HandoverModel.GetInstance.ModbilePayMoney.ToString("0.00");
            //储卡充值
            label35.Text = HandoverModel.GetInstance.CZVipJE.ToString("0.00");
            //会员还款
            label36.Text = HandoverModel.GetInstance.HKVipJE.ToString("0.00");
            //应交总金额
            label30.Text = HandoverModel.GetInstance.Money.ToString("0.00");
        }


        //交班后重置基本数据
        private void InitData()
        {
            //当班金额
            HandoverModel.GetInstance.SaveMoney = 0.00m;

            //交易单数
            HandoverModel.GetInstance.OrderCount = 0;
            //退款金额
            HandoverModel.GetInstance.RefundMoney = 0.00m;
            //中途提款
            HandoverModel.GetInstance.DrawMoney = 0.00m;
            //现金
            HandoverModel.GetInstance.CashMoney = 0.00m;
            //银联卡
            HandoverModel.GetInstance.paycardMoney = 0.00m;
            //礼券
            HandoverModel.GetInstance.LiQuanMoney = 0.00m;
            //储值卡
            HandoverModel.GetInstance.VipCardMoney = 0.00m;
            //移动支付
            HandoverModel.GetInstance.ModbilePayMoney = 0.00m;
            //储卡充值
            HandoverModel.GetInstance.CZVipJE = 0.00m;
            //会员还款
            HandoverModel.GetInstance.HKVipJE = 0.00m;
            //应交总金额
            HandoverModel.GetInstance.Money = 0.00m;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            time_temp = System.DateTime.Now;
            label16.Text = time_temp.ToString();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            EXFunc();

        }





    }
}
