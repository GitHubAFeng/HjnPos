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

        BindingList<JiaoBanModel> jiaoBanList = new BindingList<JiaoBanModel>();


        public exchangeForm()
        {
            InitializeComponent();
        }

        private void exchangeForm_Load(object sender, EventArgs e)
        {
            this.ActiveControl = this.dataGridView1;

            //计时器有延迟，防止用户快速交班而计时器还未开始运行时会发重新错误
            label16.Text = System.DateTime.Now.ToString();
            HandoverModel.GetInstance.ClosedTime = System.DateTime.Now;

            ShowUI();

            PCname = HandoverModel.GetInstance.pc_name;

            this.dataGridView1.DataSource = jiaoBanList;

            if (jiaoBanList.Count == 0)
            {
                loadForJianbanFunc();
            }

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

                case Keys.F7:
                    reprintFunc();
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

                    if (HandoverModel.GetInstance.workid != 0)
                    {
                        var jbta = db.hd_dborjb.Where(t => t.id == HandoverModel.GetInstance.workid).FirstOrDefault();
                        if (jbta != null)
                        {
                            jbta.jbje = HandoverModel.GetInstance.QianxiangMoney;
                            jbta.jcount = HandoverModel.GetInstance.OrderCount;
                            jbta.tkje = HandoverModel.GetInstance.RefundMoney;//退款
                            jbta.qkje = HandoverModel.GetInstance.DrawMoney;//中途提款
                            jbta.jtime = time_temp;
                            jbta.item_count = HandoverModel.GetInstance.AllCount;
                            jbta.all_je = HandoverModel.GetInstance.AllJe;

                            jbta.yinlian_je = HandoverModel.GetInstance.paycardMoney;
                            jbta.xianjin_je = HandoverModel.GetInstance.CashMoney;
                            jbta.czk_je = HandoverModel.GetInstance.VipCardMoney;
                            jbta.liquan_je = HandoverModel.GetInstance.LiQuanMoney;
                            jbta.mobile_je = HandoverModel.GetInstance.ModbilePayMoney;
                            jbta.vipcz_je = HandoverModel.GetInstance.CZVipJE;
                            jbta.viphk_je = HandoverModel.GetInstance.HKVipJE;


                        }
                        else
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
                                jbje = HandoverModel.GetInstance.QianxiangMoney,
                                jcount = HandoverModel.GetInstance.OrderCount,
                                tkje = HandoverModel.GetInstance.RefundMoney,//退款
                                qkje = HandoverModel.GetInstance.DrawMoney,//中途提款
                                jtime = time_temp,
                                item_count = HandoverModel.GetInstance.AllCount,
                                all_je = HandoverModel.GetInstance.AllJe,

                                yinlian_je = HandoverModel.GetInstance.paycardMoney,
                                xianjin_je = HandoverModel.GetInstance.CashMoney,
                                czk_je = HandoverModel.GetInstance.VipCardMoney,
                                liquan_je = HandoverModel.GetInstance.LiQuanMoney,
                                mobile_je = HandoverModel.GetInstance.ModbilePayMoney,
                                vipcz_je = HandoverModel.GetInstance.CZVipJE,
                                viphk_je = HandoverModel.GetInstance.HKVipJE

                            };
                            db.hd_dborjb.Add(JBInfo);
                        }

                    }

                    var re = db.SaveChanges();
                    if (re > 0)
                    {
                        timer1.Enabled = false;  //停止计时
                        HandoverModel.GetInstance.ClosedTime = time_temp;
                        HandoverModel.GetInstance.isWorking = false;
                        JiaoBanPrinter jbprint = new JiaoBanPrinter();
                        jbprint.StartPrint();

                        //if (DialogResult.Yes == MessageBox.Show("交班成功！以防他人冒用帐号，请及时退出登陆，是否现在退出本软件？", "提醒", MessageBoxButtons.YesNo))
                        //{
                        //    Application.Exit();
                        //}
                        //else
                        //{
                        //    InitData();  //重置
                        //    UIChanged();  //通知交班成功
                        //}

                        InitData();  //重置
                        UIChanged();  //通知交班成功

                        if (DialogResult.Yes == MessageBox.Show("交班成功！以防他人冒用帐号，请及时退出登陆，是否现在退出本软件？", "提醒", MessageBoxButtons.YesNo))
                        {
                            Application.Exit();
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("交班界面进行交班时发生异常:", ex);
                //MessageBox.Show("交班时出现异常，请先检查数据是否正常，必要时请联系管理员！");
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
            label18.Text = HandoverModel.GetInstance.isWorking ? HandoverModel.GetInstance.workTime.ToString("yyyy/MM/dd HH:mm:ss") : "未当班";
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
            //交班金额
            label37.Text = HandoverModel.GetInstance.QianxiangMoney.ToString("0.00");
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
            //交班金额
            HandoverModel.GetInstance.QianxiangMoney = 0.00m;

            HandoverModel.GetInstance.AllCount =0.00m;  //当班期间总金额
            HandoverModel.GetInstance.AllJe = 0.00m;  //当班期间总售出商品数量

            HandoverModel.GetInstance.workid = 0;
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



        //读取交班表
        private void loadForJianbanFunc()
        {

            jiaoBanList.Clear();

            using (var db = new hjnbhEntities())
            {
                var jiaobanInfo = db.hd_dborjb.AsNoTracking().Where(t => t.usr_id == HandoverModel.GetInstance.userID)
                    .OrderByDescending(t => t.dtime.Value).ToList();

                if (jiaobanInfo.Count > 0)
                {
                    foreach (var item in jiaobanInfo)
                    {
                        jiaoBanList.Add(new JiaoBanModel
                        {
                            workid = item.id,
                            scode = item.scode.HasValue ? item.scode.Value : 0,
                            bcode = item.bcode.HasValue ? item.bcode.Value : 0,
                            userID = item.usr_id.HasValue ? item.usr_id.Value : 0,
                            //PCname = item.pc_name,
                            //pc_code = item.pc_code,
                            userName = item.cname,
                            SaveMoney = item.dbje.HasValue ? item.dbje.Value : 0.00m,
                            workTime = item.dtime.HasValue ? item.dtime.Value.ToString("yyyy/MM/dd HH:mm") : "未知",
                            QianxiangMoney = item.jbje.HasValue ? item.jbje.Value : 0.00m,
                            OrderCount = item.jcount.HasValue ? item.jcount.Value : 0,
                            RefundMoney = item.tkje.HasValue ? item.tkje.Value : 0.00m,//退款
                            DrawMoney = item.qkje.HasValue ? item.qkje.Value : 0.00m,//中途提款
                            JTime = item.jtime.HasValue ? item.jtime.Value.ToString("yyyy/MM/dd HH:mm") : "未知",
                            //AllCount = item.item_count.HasValue ? item.item_count.Value : 0.00m,
                            //AllJe = item.all_je.HasValue ? item.all_je.Value : 0.00m,
                            paycardMoney = item.yinlian_je.HasValue ? item.yinlian_je.Value : 0.00m,
                            CashMoney = item.xianjin_je.HasValue ? item.xianjin_je.Value : 0.00m,
                            VipCardMoney = item.czk_je.HasValue ? item.czk_je.Value : 0.00m,
                            LiQuanMoney = item.liquan_je.HasValue ? item.liquan_je.Value : 0.00m,
                            ModbilePayMoney = item.mobile_je.HasValue ? item.mobile_je.Value : 0.00m,
                            CZVipJE = item.vipcz_je.HasValue ? item.vipcz_je.Value : 0.00m,
                            HKVipJE = item.viphk_je.HasValue ? item.viphk_je.Value : 0.00m

                        });
                    }

                    dataGridView1.Refresh();
                }
                //else
                //{
                //    MessageBox.Show("Test");
                //}


            }


        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                //列名
                dataGridView1.Columns[0].HeaderText = "内部序号";
                dataGridView1.Columns[1].HeaderText = "员工号";
                dataGridView1.Columns[2].HeaderText = "员工姓名";
                dataGridView1.Columns[3].HeaderText = "分店号";
                dataGridView1.Columns[4].HeaderText = "终端号";
                dataGridView1.Columns[5].HeaderText = "交易单数";
                dataGridView1.Columns[6].HeaderText = "当班金额";
                dataGridView1.Columns[7].HeaderText = "退货金额";
                dataGridView1.Columns[8].HeaderText = "中途提款";
                dataGridView1.Columns[9].HeaderText = "应交金额";
                dataGridView1.Columns[10].HeaderText = "当班时间";
                dataGridView1.Columns[11].HeaderText = "交班时间";


                //隐藏   
                dataGridView1.Columns[5].Visible = false;
                dataGridView1.Columns[6].Visible = false;
                dataGridView1.Columns[7].Visible = false;
                dataGridView1.Columns[8].Visible = false;

                dataGridView1.Columns[12].Visible = false;
                dataGridView1.Columns[13].Visible = false;
                dataGridView1.Columns[14].Visible = false;
                dataGridView1.Columns[15].Visible = false;
                dataGridView1.Columns[16].Visible = false;
                dataGridView1.Columns[17].Visible = false;
                dataGridView1.Columns[18].Visible = false;
                dataGridView1.Columns[19].Visible = false;

                //列宽
                dataGridView1.Columns[10].Width = 140;
                dataGridView1.Columns[11].Width = 140;


            }
            catch
            {
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            reprintFunc();
        }


        //重打凭证
        private void reprintFunc()
        {
            if (jiaoBanList.Count > 0 && dataGridView1.Rows.Count > 0)
            {
                int index_ = dataGridView1.SelectedRows[0].Index;
                var printinfo = jiaoBanList[index_];
                if (printinfo != null)
                {
                    if (DialogResult.No == MessageBox.Show("是否确认重新打印此交班凭证？", "提醒", MessageBoxButtons.YesNo))
                    {
                        return;
                    }

                    JiaoBanPrinter jbprint = new JiaoBanPrinter(printinfo, true);
                    jbprint.StartPrint();

                }
            }
            else
            {
                MessageBox.Show("没有可打印信息！");
            }

        }
















    }
}
