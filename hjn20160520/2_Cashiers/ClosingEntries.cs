using Common;
using hjn20160520.Common;
using hjn20160520.Models;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Forms;

namespace hjn20160520._2_Cashiers
{
    public partial class ClosingEntries : Form
    {
        //单例
        //public static ClosingEntries GetInstance { get; private set; }

        CashiersFormXP CFXPForm;  //收银窗口
        //信息提示窗口
        TipForm tipForm;
        //结算方式，默认为现金
        JSType jstype = JSType.Cash;

        //用于记录付款方式的列表
        List<CEJStype> CEJStypeList = new List<CEJStype>();

        //抹零窗口
        MoLingForm MLForm = new MoLingForm();
        CardPayForm CPFrom = new CardPayForm();  //银联卡窗口
        MobilePayForm MoPayform = new MobilePayForm();  //移动支付窗口
        VipShopForm vipform = new VipShopForm();

        VipCZKForm czkform = new VipCZKForm();  //会员储值卡消费
        QKJEForm qkform = new QKJEForm();  //挂账窗口
        InputBoxForm passwordForm = new InputBoxForm(); //会员密码输入框
        string VipPW = ""; //会员密码验证

        decimal AllCzkJe = 0; //全部使用的储卡金额，包括定金与分期
        decimal DJJe = 0;  //使用的定金
        decimal FQJe = 0;  //使用的分期金额
        decimal CzkJe = 0; //本单储值卡使用金额
        BindingList<VipFQModel> FqList = new BindingList<VipFQModel>();  //用过的分期列表

        decimal weixun = 0;  //本单使用微信支付金额
        decimal zfb = 0;  //本单使用支付宝金额
        public string weixunStr = "", zfbStr = "";  //微信与支付宝账号

        private decimal vipToJe = 0;  //会员抵额退款转存入储值金额
        private decimal CurrentTuihuJF = 0.00m; //本次抵额退货需要扣减的积分

        //用于其它窗口传值给本窗口控件
        //这是委托与事件的第一步  
        public delegate void ClosingEntriesHandle();
        public event ClosingEntriesHandle changed;  //取消结算事件

        public delegate void CEFormHandle(decimal getje, decimal toje, decimal zlje, string jsdh, string vip);
        public event CEFormHandle UIChanged;  //UI更新事件

        public BindingList<GoodsBuy> goodList = new BindingList<GoodsBuy>();

        //抹零
        public decimal? MoLing { get; set; }
        //此单所得会员积分
        private decimal vipJF;  //本次积分
        private string jsdh; //结算单号，方便打印
        //找零
        public decimal GiveChange
        {
            get { return (getMoney - CETotalMoney); }

        }

        //收取金额 ， 最终入账的结算金额（现金） , 收取金额  =  应收金额-欠款金额
        public decimal JE { get { return (CETotalMoney - QKjs); } }

        //应收金额 ， 是商品总金额
        public decimal CETotalMoney { get; set; }

        //实收金额,从客户手中收取的金额，有多就找零的，不够就是欠款。要实时得到此值前，必须执行一次 UpdataJEUI();方法
        public decimal getMoney = 0.00m;


        private decimal qkje;
        //欠款挂帐
        public decimal QKjs
        {
            get
            {
                return ((CETotalMoney - getMoney) > 0) ? (CETotalMoney - getMoney) : qkje;
            }
            set { qkje = value; }
        }

        //标志是否一性次全额付款
        private bool isCEOK = false;

        //银行卡号
        public string payCard { get; set; }
        //银行优惠金额
        private decimal payAllje = 0.00m;

        public ClosingEntries()
        {
            InitializeComponent();
        }

        private void ClosingEntries_Load(object sender, EventArgs e)
        {
            CFXPForm = this.Owner as CashiersFormXP;

            CE_textBox1.Focus();
            label5.Text = "现金";
            ShowUI();

            tipForm = new TipForm();
            CEJStypeList.Clear();
            MoLing = 0.00m;
            this.vipToJe = 0.00m;
            vipJF = 0.00m;
            CurrentTuihuJF = 0.00m;

            OnCouponFunc();  //计算礼券

            MLForm.changed += MLForm_changed;

            vipform.changed += vipform_changed;
            czkform.changed += czkform_changed;
            CPFrom.changed += CPFrom_changed;
            qkform.changed += qkform_changed;
            MoPayform.changed += MoPayform_changed;
            passwordForm.changed += passwordForm_changed;
        }


        /// <summary>
        /// 会员密码检证
        /// </summary>
        /// <param name="PW"></param>
        void passwordForm_changed(string PW)
        {
            VipPW = PW;
        }

        /// <summary>
        /// 移动支付处理
        /// </summary>
        /// <param name="weixun">微信</param>
        /// <param name="zfb">支付宝</param>
        void MoPayform_changed(decimal weixun, decimal zfb, string weixunStr, string zfbStr)
        {
            if (weixun == 0 && zfb == 0) return;
            this.weixun += weixun;
            this.zfb += zfb;
            this.weixunStr = weixunStr;
            this.zfbStr = zfbStr;

            decimal alltemp = weixun + zfb;

            if (alltemp > 0)
            {
                if (this.CETotalMoney - alltemp > 0)
                {
                    this.CETotalMoney -= alltemp;
                    UpdataJEUI();
                    CEJEFunc(4, alltemp);

                }
                else
                {
                    this.label5.Text = "移动支付";
                    CEJEFunc(4, CETotalMoney);
                    this.getMoney = CETotalMoney;
                    this.isCEOK = true;
                    //立即全款支付
                    OnEnterClick();
                }

            }


        }

        /// <summary>
        /// 处理挂帐
        /// </summary>
        /// <param name="qkje">欠款</param>
        void qkform_changed(decimal qkje)
        {
            this.QKjs += qkje;  //累计的挂账金额
            this.CETotalMoney -= qkje;  //应付金额减去此次挂账
            label6.Text = QKjs.ToString() + " 元";  //已挂金额
            this.getMoney = CETotalMoney;
            CE_label7.Text = CETotalMoney.ToString();  //总金额
            CE_textBox1.Text = getMoney.ToString();  //客人要付金额
            this.label16.Text = CETotalMoney.ToString() + " 元";  //本单合计
            CE_textBox1.SelectAll();
        }


        /// <summary>
        /// 处理银联卡消费
        /// </summary>
        /// <param name="card">银行卡</param>
        /// <param name="reje">银行返现金额</param>
        /// <param name="rezk">银行折扣</param>
        void CPFrom_changed(string card, decimal reje, decimal rezk)
        {
            if (string.IsNullOrEmpty(card)) return;
            this.payCard = card;

            //不管怎么优惠，记录付款的总额是不变的，优惠的钱是从银行返回的。

            if (reje > 0)
            {
                payAllje += reje;
            }

            if (rezk > 0)
            {
                decimal tempje = this.CETotalMoney * rezk / 100;
                payAllje += tempje;
            }

            if (payAllje > 0)
            {
                this.CETotalMoney -= payAllje;
                if (this.CETotalMoney < 0) this.CETotalMoney = 0.00m;
                UpdataJEUI();
            }

            CEJEFunc(1, CETotalMoney);
            this.getMoney = CETotalMoney;
            this.isCEOK = true;
            //立即全款支付
            OnEnterClick();
        }


        /// <summary>
        /// 处理储值卡消费
        /// </summary>
        /// <param name="CzkJe">使用的储卡金额</param>
        /// <param name="DJJe">使用定金</param>
        /// <param name="FQJe">使用分期</param>
        /// <param name="FqList">分期列表，用于判断使用了哪些分期</param>
        void czkform_changed(decimal CzkJe, decimal DJJe, decimal FQJe, BindingList<VipFQModel> FqList)
        {
            this.CzkJe = CzkJe;
            this.DJJe = DJJe;
            this.FQJe = FQJe;
            this.FqList = FqList;
            this.AllCzkJe = CzkJe + DJJe + FQJe;


            CETotalMoney -= DJJe;
            CETotalMoney -= FQJe;

            UpdataJEUI();
            if (CETotalMoney < 0) CETotalMoney = 0.00m;

            //使用储值卡
            if (CzkJe > 0 && CETotalMoney > 0)
            {
                if (CzkJe < CETotalMoney)
                {
                    if (DialogResult.Yes == MessageBox.Show("此储值卡金额不足以全额付款，是否抵消部分应付金额？", "提醒", MessageBoxButtons.YesNo))
                    {
                        CETotalMoney -= CzkJe;

                        UpdataJEUI();
                    }

                }
                else
                {
                    UpdataJEUI();
                    this.label5.Text = "储值卡";
                    this.getMoney = this.CETotalMoney;
                    this.isCEOK = true;
                    //增加储值卡的金额
                    CEJEFunc(3, AllCzkJe);
                    //立即全款支付
                    OnEnterClick();
                }
            }

            //增加储值卡的金额
            CEJEFunc(3, AllCzkJe);

        }

        /// <summary>
        /// 汇总结算方式
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="fsje"></param>
        private void CEJEFunc(int fs, decimal fsje)
        {
            var cetemp = CEJStypeList.Where(t => t.cetype == fs).FirstOrDefault();
            if (cetemp != null)
            {
                cetemp.ceJE += fsje;
            }
            else
            {
                CEJStypeList.Add(new CEJStype
                {
                    cetype = fs,
                    ceJE = fsje

                });
            }
        }



        void vipform_changed()
        {

            VIPShowUI();
        }




        //给登录会员时刷新
        public void VIPShowUI()
        {
            CETotalMoney = CFXPForm.totalMoney;
            this.label16.Text = CETotalMoney.ToString() + " 元";  //本单合计
            CE_label7.Text = CETotalMoney.ToString();  //应收金额
            //默认设置实收金额等于应收总金额，并默认全选状态
            getMoney = CETotalMoney;
            CE_textBox1.Text = getMoney.ToString();  //现收，实收金额
            CE_textBox1.SelectAll();
        }

        public void ShowUI()
        {
            this.label16.Text = CETotalMoney.ToString() + " 元";  //本单合计
            CE_label7.Text = CETotalMoney.ToString();  //应收金额
            //默认设置实收金额等于应收总金额，并默认全选状态
            getMoney = CETotalMoney;
            CE_textBox1.Text = getMoney.ToString();  //现收，实收金额
            CE_textBox1.SelectAll();
            label19.Text = "0";
            label6.Text = "0";
        }

        //快捷键
        private void ClosingEntries_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    OnEnterClick();
                    break;
                case Keys.Escape:
                    changed();
                    this.Close();
                    break;
                //现金
                case Keys.F3:
                    //OnCashFunc();
                    break;
                //银联
                case Keys.F5:
                    if (CETotalMoney > 0)
                    {
                        OnUnionPayFunc();

                    }
                    else
                    {
                        MessageBox.Show("退货或者赠送的情况下不可使用此结算方式！请直接按回车完成结算。");
                    }
                    break;
                //移动支付
                case Keys.F4:
                    if (CETotalMoney > 0)
                    {
                        MoPayform.ShowDialog(this);
                    }
                    else
                    {
                        MessageBox.Show("退货或者赠送的情况下不可使用此结算方式！请直接按回车完成结算。");
                    }

                    break;
                //储值卡
                case Keys.F6:
                    if (CETotalMoney > 0)
                    {
                        OnOthersFunc();

                    }
                    else
                    {
                        MessageBox.Show("退货或者赠送的情况下不可使用此结算方式！请直接按回车完成结算。");
                    }
                    break;
                //抹零
                case Keys.F7:
                    if (CETotalMoney > 0)
                    {
                        MLForm.ShowDialog(this);

                    }
                    else
                    {
                        MessageBox.Show("应付金额已小于或者等于零！请直接按回车完成结算。");
                    }
                    break;

                //挂账
                case Keys.F8:
                    if (CETotalMoney > 0)
                    {
                        QKFunc();
                    }
                    else
                    {
                        MessageBox.Show("应付金额已小于或者等于零！请直接按回车完成结算。");
                    }
                    break;

                //转储值
                case Keys.F9:
                    ToVipSaveCK();
                    break;


                //会员登记
                //case Keys.F12:
                //    //vipform = new VipShopForm();
                //    vipform.ShowDialog();

                //    break;
            }
        }

        //重置
        private void InitData()
        {
            this.MoLing = 0;  //清空抹零 //退出结算时清空抹零
            qkje = 0;
            QKjs = 0; //欠款清零
            label6.Text = "0";
            label19.Text = "0";
            weixunStr = "";
            zfbStr = "";
            AllCzkJe = 0; //全部使用的储卡金额，包括定金与分期
            DJJe = 0;  //使用的定金
            FQJe = 0;  //使用的分期金额
            CzkJe = 0; //本单储值卡使用金额
            weixun = 0;  //本单使用微信支付金额
            zfb = 0;  //本单使用支付宝金额
            FqList.Clear();  //分期列表

            VipPW = ""; //会员密码是否通过验证

        }


        //抹零传值
        void MLForm_changed(decimal s)
        {

            MoLing += s;
            CETotalMoney -= s;

            this.label16.Text = CETotalMoney.ToString() + " 元";  //本单合计
            CE_label7.Text = CETotalMoney.ToString();  //应收
            CE_textBox1.Text = CETotalMoney.ToString();  //应收
            this.label19.Text = MoLing.ToString() + " 元";  //已抹
        }

        //挂账逻辑
        private void QKFunc()
        {
            if (HandoverModel.GetInstance.VipID != 0)
            {
                qkform.ShowDialog(this);
            }
            else
            {
                tipForm.Tiplabel.Text = "您还没有登记会员，请先在收银窗口按F12登记会员后再进行结算!";
                tipForm.ShowDialog();
            }

        }

        //回车逻辑
        private void OnEnterClick()
        {

            if (!decimal.TryParse(CE_textBox1.Text.Trim(), out getMoney))
            {
                MessageBox.Show("现收金额输入有误，请重新输入！");
            }
            if (GiveChange < 0)
            {
                CE_label5.Text = GiveChange.ToString();  //找零
                tipForm.Tiplabel.Text = "所收取金额不足以全额付款，请重新确认!";
                tipForm.ShowDialog();
            }
            else
            {
                if (CEJStypeList.Count == 0 || !isCEOK)
                {
                    //默认现金消费
                    OnCashFunc();
                }

                DBFunc();

                //结算完成事件
                UIChanged(this.getMoney, this.CETotalMoney, this.GiveChange, jsdh, HandoverModel.GetInstance.VipCard);

                CE_textBox1.Text = "";
                CE_label5.Text = "0.00";

                this.MoLing = 0;  //结单后把上单抹零纪录清空
                this.Close();

            }
        }

        //现金支付
        private void OnCashFunc()
        {
            this.label5.Text = "现金";

            if (CETotalMoney >= 0)
            {
                //默认
                CEJEFunc(0, CETotalMoney);
            }
            else
            {
                decimal alljeTemp = goodList.Where(t => t.isTuiHuo == false).Select(t => t.Sum).Sum();
                //退款就不要计入应交
                CEJEFunc(0, alljeTemp);
            }

        }

        //银联卡支付
        private void OnUnionPayFunc()
        {

            CPFrom.ShowDialog(this);
        }

        //购物劵支付
        private void OnCouponFunc()
        {
            decimal total = goodList.Where(t => t.Sum > 0 ).Select(t => t.Sum).Sum(); //实际商品金额
            decimal lqtotal = goodList.Where(t => t.Sum < 0 && t.isTuiHuo == false).Select(t => t.Sum).Sum(); //礼券金额
            decimal aass = Math.Abs(lqtotal);
            if (CETotalMoney > 0)
            {
                CEJEFunc(2, aass);
                this.label5.Text = "礼券";
            }
            else
            {
                CEJEFunc(2, total);
                this.label5.Text = "礼券";
                CETotalMoney = 0.00m;
                getMoney = 0.00m;
                UpdataJEUI();
            }

            //var Coupontemp = goodList.Where(t => t.Sum < 0 && t.isTuiHuo == false).ToList();
            //if (Coupontemp.Count > 0)
            //{
            //    decimal temp = 0;
            //    foreach (var item in Coupontemp)
            //    {
            //        if (item.lsPrice < 0)
            //        {
            //            temp += item.lsPrice.Value;
            //        }
            //        else
            //        {
            //            temp += item.hyPrice.Value;
            //        }
            //    }

            //    decimal aass = Math.Abs(temp);
            //    //if (CETotalMoney > 0)
            //    //{
            //    //    CEJEFunc(0, CETotalMoney);
            //    //    CEJEFunc(2, aass);
            //    //}
            //    //else
            //    //{
            //    //    CEJEFunc(2, aass);
            //    //    this.label5.Text = "礼券";
            //    //}

            //    CEJEFunc(2, aass);
            //    this.label5.Text = "礼券";
            //}



        }

        //其它方式支付,会员储值卡
        private void OnOthersFunc()
        {
            //条件验证要在子窗口中做限制了
            int vipid = HandoverModel.GetInstance.VipID;
            if (vipid != 0)
            {
                using (var db = new hjnbhEntities())
                {
                    var Vippasswordinfo = db.hd_vip_info.Where(t => t.vipcode == vipid).Select(t => t.password).FirstOrDefault();

                    string vippw = Vippasswordinfo.HasValue ? Vippasswordinfo.Value.ToString() : "0";
                    //先验证密码
                    passwordForm.ShowDialog();
                    if (!string.IsNullOrEmpty(VipPW))
                    {
                        if (VipPW != vippw)
                        {
                            MessageBox.Show("会员密码检验失败！请输入正确的会员密码！可尝试使用默认密码 0 。", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        else
                        {
                            czkform.ShowDialog(this);
                        }
                    }

                }

            }
            else
            {
                tipForm.Tiplabel.Text = "您还没有登记会员，请先按ESC键取消结算，并在收银窗口按F12键登记会员，以便调整商品价格！";
                tipForm.ShowDialog();
            }

        }

        //刷新金额相关UI显示
        private void UpdataJEUI()
        {
            CE_label7.Text = CETotalMoney.ToString();
            CE_textBox1.Text = CETotalMoney.ToString();
            label16.Text = CETotalMoney.ToString();
        }



        //限制只能输入数字与小数.
        private void CE_textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' && e.KeyChar != '.' || e.KeyChar > '9' && e.KeyChar != '.' || ((TextBox)(sender)).Text.IndexOf('.') >= 0 && e.KeyChar == '.') && e.KeyChar != (char)13 && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }


        #region 向数据库提交单据包括结算单、零售单、零售明细与出库单、出库明细、库存扣减


        //向数据库中存储单据
        private void DBFunc()
        {
            try
            {
                //单号计算方式，当前时间+00000+id
                long no_temp = Convert.ToInt64(System.DateTime.Now.ToString("yyyyMMdd") + "000000");
                DateTime timer = System.DateTime.Now; //统一成单时间
                string dateStr = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");

                if (HandoverModel.GetInstance.isLianxi)
                {
                    MessageBox.Show("练习模式下该操作无效！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                decimal total = goodList.Where(t => t.Sum > 0).Select(t => t.Sum).Sum();  //实际上商品价格总额，过滤退货的、礼券

                string lsNoteNO = string.Empty;
                string jsNoteNO = string.Empty;
                string outNoteNo = string.Empty;
                decimal zktemp = CFXPForm.ZKZD / 100;  //整单折扣

                using (var db = new hjnbhEntities())
                {


                    #region 零售单与结算单
                    //后台需要上传会员编号
                    int vipNo = HandoverModel.GetInstance.VipID;
                    string vipcard = HandoverModel.GetInstance.VipCard;

                    //主单
                    var HDLS = new hd_ls
                    {
                        vip = vipNo,  //是会员则记录会员ID，否则记得0
                        zzk = zktemp,  //总折扣
                        del_flag = 0,  //删除标记
                        cid = HandoverModel.GetInstance.userID,//零售员
                        ywy = HandoverModel.GetInstance.YWYid,//业务员工
                        scode = HandoverModel.GetInstance.scode,//分店
                        ctime = timer
                    };
                    db.hd_ls.Add(HDLS);

                    db.SaveChanges(); //保存一次才能进行获取


                    jsNoteNO = "JS" + (no_temp + HDLS.id).ToString();  //获取ID并生成结算单号
                    lsNoteNO = "LS" + (no_temp + HDLS.id).ToString();  //获取ID并生成主单号
                    outNoteNo = "LSC" + (no_temp + HDLS.id).ToString();//获取出库单号

                    HDLS.v_code = lsNoteNO; //零售单号
                    jsdh = jsNoteNO;



                    string remarktemp = !string.IsNullOrEmpty(HandoverModel.GetInstance.TuiHuoJSDH) ? "前台抵额退货|" + HandoverModel.GetInstance.TuiHuoJSDH : "";

                    //结算单
                    var HDJS = new hd_js
                    {
                        v_code = jsNoteNO,
                        ls_code = lsNoteNO,
                        js_type = (byte)jstype, //结算类型
                        ysje = CETotalMoney, //应收金额
                        //je = JE,  //收取金额 ，入账金额
                        je = total,
                        pc_code = HandoverModel.GetInstance.pc_code,  //机械码
                        ssje = getMoney, //实收金额
                        qkje = QKjs,  //还有个欠款字段
                        status = 1, //状态，1为确认结算
                        //del_flag = 0, //删除标记
                        moling = MoLing, //抹零
                        remark = CFXPForm.ZKZD != 0 ? ("整单打折" + (CFXPForm.ZKZD / 100).ToString()) : remarktemp, // 备注
                        bankcode = payCard,//银行卡号
                        cid = HandoverModel.GetInstance.userID, //收银员工
                        ctime = timer //成单时间

                    };
                    db.hd_js.Add(HDJS);


                    #endregion



                    #region VIP单据与积分

                    if (vipNo != 0)
                    {
                        var Vipinfo = db.hd_vip_info.Where(t => t.vipcode == vipNo).FirstOrDefault();
                        if (Vipinfo != null)
                        {
                            //如果是会员储值卡消费
                            var vipce = CEJStypeList.Where(t => t.cetype == 3).FirstOrDefault();
                            if (vipce != null)
                            {
                                if (CzkJe > 0)
                                {
                                    Vipinfo.czk_ye -= CzkJe;
                                }
                                //记录储卡消费，余额扣减
                                var vipczk = new hd_vip_cz
                                {
                                    ckh = vipNo.ToString(), //会员编号
                                    rq = timer, //时间
                                    fs = (byte)3, //类型
                                    srvoucher = jsNoteNO, //单号
                                    je = -vipce.ceJE, //使用余额
                                    czr = HandoverModel.GetInstance.userID,
                                    ctype = (byte)0,
                                    lsh = HandoverModel.GetInstance.scode
                                };
                                db.hd_vip_cz.Add(vipczk);


                                //会员储卡消费自动备注
                                string temp4 = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ： " + " 会员储卡余额 -" + vipce.ceJE.ToString() + ";";
                                VipAutoMemoFunc(db, vipNo, HandoverModel.GetInstance.VipCard, HandoverModel.GetInstance.VipName, temp4, 4);

                            }

                            decimal jftemp = 0.00m;  //记录积分方便打印
                            //计算积分
                            foreach (var item in goodList)
                            {
                                if (item.isCyjf)
                                {
                                    decimal jfbltemp = item.jfbl > 0 ? item.jfbl : 0.1m;  //默认0.1
                                    decimal itemjf = item.Sum * jfbltemp;
                                    jftemp += itemjf;
                                }
                            }


                            if (CFXPForm.isVipDate)
                            {
                                jftemp *= CFXPForm.vipDtaeJF;
                            }

                            if (HandoverModel.GetInstance.isVipBirthday)
                            {
                                jftemp *= 2;
                            }

                            jftemp = Math.Round(jftemp, 2);
                            vipJF = jftemp;

                            decimal tempJF = Vipinfo.jfnum.HasValue ? Vipinfo.jfnum.Value : 0;
                            tempJF += jftemp;
                            Vipinfo.jfnum = tempJF;  

                            decimal tempLJJE = Vipinfo.ljxfje.HasValue ? Vipinfo.ljxfje.Value : 0;
                            tempLJJE += total;
                            Vipinfo.ljxfje = tempLJJE; //累计积分金额
                            Vipinfo.dtMaxChanged = timer;  //最近消费时间

                            //消费金额大于0才能使用定金、分期，积会才会增长
                            if (total > 0)
                            {
                                //会员与消费的零售订单关联，会员积分增长
                                var vip = new hd_vip_cz
                                {
                                    ckh = vipNo.ToString(), //会员编号
                                    rq = timer, //时间
                                    fs = (byte)7, //消费类型
                                    srvoucher = jsNoteNO, //单号
                                    je = total, //记录消费金额
                                    ctype = (byte)0,
                                    czr = HandoverModel.GetInstance.userID,
                                    jf = jftemp,
                                    lsh = HandoverModel.GetInstance.scode
                                };
                                db.hd_vip_cz.Add(vip);

                                //会员积分增长自动备注
                                string temp3 = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ： " + " 会员积分增长 +" + jftemp.ToString() + ";";
                                VipAutoMemoFunc(db, vipNo, HandoverModel.GetInstance.VipCard, HandoverModel.GetInstance.VipName, temp3, 3);


                                if (FQJe > 0)
                                {
                                    //会员使用的分期
                                    var usedFqInfo = FqList.Where(t => t.Used).ToList();
                                    if (usedFqInfo.Count > 0)
                                    {
                                        foreach (var item in usedFqInfo)
                                        {
                                            var Fqinfo = db.hd_vip_fq.Where(t => t.id == item.id && t.vipcode == item.vipCode).FirstOrDefault();
                                            if (Fqinfo != null)
                                            {
                                                decimal fqnumtemp = Fqinfo.amount.HasValue ? Fqinfo.amount.Value : 0;
                                                decimal tempcount = usedFqInfo.Where(t => t.id == item.id).Count();
                                                fqnumtemp -= tempcount;
                                                Fqinfo.amount = fqnumtemp;
                                            }
                                        }

                                        string temp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ： " + " 使用了储卡分期金额 " + FQJe.ToString() + ";";
                                        VipAutoMemoFunc(db, vipNo, HandoverModel.GetInstance.VipCard, HandoverModel.GetInstance.VipName, temp, 4);
                                    }
                                }

                                if (DJJe > 0)
                                {
                                    //使用定金
                                    decimal djtemp = Vipinfo.ydje.HasValue ? Vipinfo.ydje.Value : 0;
                                    djtemp -= DJJe;
                                    Vipinfo.ydje = djtemp;
                                    string temp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ： " + " 使用了定金 " + DJJe.ToString() + ";";
                                    VipAutoMemoFunc(db, vipNo, HandoverModel.GetInstance.VipCard, HandoverModel.GetInstance.VipName, temp, 5);

                                }
                            }


                        }

                    }


                    #endregion

                    #region 活动相关、抵额退货等特殊情况、明细

                    string goodsmemos = ""; //领取的赠品
                    string Tuihuomemotemp = "";  //退货的商品
                    decimal TuihuoJEtemp = 0; //退货金额
                    string THNoteID = ""; //退货单号
                    //返回 0 为 该退货商品购买时候的会员 ， 1 为退货单号 
                    int TuiHuoVipid = 0; //此退货商品当时的购买会员
                    string vipcard_temp = "";  //退货会员卡号
                    string vipname_temp = "";  //退货会员名字
                    string ZJFstr = "";  //总积分

                    string TuiHuoJSDH = HandoverModel.GetInstance.TuiHuoJSDH; //需要退货的小票单
                    int jsid = db.hd_js.Where(t => t.v_code == TuiHuoJSDH).Select(t => t.id).FirstOrDefault();
                    if (jsid > 0)
                    {
                        //退货单号计算方式，当前时间+00000+id
                        long TuiHuono_temp = Convert.ToInt64(System.DateTime.Now.ToString("yyyyMMdd") + "000000");
                        THNoteID = "LSR" + (TuiHuono_temp + jsid).ToString();//获取退货入库单号
                    }

                    foreach (var item in goodList)
                    {
                        //过滤掉抵额退货的商品，此商品不放入零售明细，避免后台统计错乱
                        if (item.isTuiHuo)
                        {
                            try
                            {
                                //这个还是要执行退货处理，放入退货明细
                                TuiHuoVipid = TuiHuoFunc(db, item, jsNoteNO, THNoteID);
                                Tuihuomemotemp += "[" + item.noCode + "/" + item.goods + "*" + item.countNum.ToString() + "] ";
                                TuihuoJEtemp += item.Sum;
                            }
                            catch (Exception)
                            {

                                MessageBox.Show("零售抵额退货出现异常，请联系管理员处理！", "异常提醒", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            }
                        }
                        else
                        {
                            //每件商品所得积分
                            decimal itemjf = 0.00m;
                            if (item.isCyjf)
                            {
                                decimal jfbltemp = item.jfbl > 0 ? item.jfbl : 0.1m;  //默认0.1
                                itemjf = Math.Round(item.Sum * jfbltemp, 2);
                            }


                            int zs_temp = item.isZS ? 1 : 0;

                            //明细单
                            var HDLSMX = new hd_ls_detail
                            {
                                v_code = lsNoteNO, //标识单号
                                item_id = item.noCode,//商品货号
                                tm = item.barCodeTM,//条码
                                cname = item.goods,//名称
                                spec = item.spec,//规格
                                hpack_size = (decimal?)item.hpackSize,//不知是什么,包装规格
                                unit = item.unit,  //单位
                                amount = item.countNum, //数量
                                jj_price = item.jjPrice, //进价
                                ls_price = HandoverModel.GetInstance.VipID == 0 ? item.lsPrice : item.hyPrice,//零售价
                                yls_price = item.pfPrice,//原零售价
                                zk = item.ZKDP,//折扣
                                jf = itemjf,
                                iszs = (byte)zs_temp,//是否赠送
                                cid = HandoverModel.GetInstance.userID,//零售员ID
                                ctime = timer, //出单时间
                                vtype = (byte)item.vtype,  //活动类型
                                ywy = item.ywy
                            };

                            db.hd_ls_detail.Add(HDLSMX);
                        }


                        //会员赠送记录,就是会产生赠送行为的活动，主动赠送的不计入此列
                        if (item.vtype == 1 || item.vtype == 9 || item.vtype == 3 || item.vtype == 5 || item.vtype == 10)
                        {
                            if (item.isZS)
                            {
                                ////存入领取记录
                                var lqinfo = new hd_vip_zs_history
                                {
                                    vipcode = HandoverModel.GetInstance.VipID,
                                    scode = HandoverModel.GetInstance.scode,
                                    zstime = System.DateTime.Now,
                                    item_id = item.noCode,
                                    tm = item.barCodeTM,
                                    cname = item.goods,
                                    zscount = item.countNum,
                                    price = item.hyPrice
                                };
                                db.hd_vip_zs_history.Add(lqinfo);

                                goodsmemos += "[" + item.noCode + "/" + item.goods + "*" + item.countNum.ToString() + "] ";
                            }

                        }

                        //与活动10关联的商品的限量总数量更新（减去）
                        if (item.isGL)
                        {
                            //要先查活动时间，把时间作为过滤条件
                            var hdtimeinfo = db.v_yh_detail.AsNoTracking().Where(t => t.item_id == item.noCode && t.vtype == 10).Select(t => new { t.sbegintime, t.sendtime }).FirstOrDefault();
                            if (hdtimeinfo != null)
                            {
                                var hd10info = db.hd_yh_detail.Where(t => t.item_id == item.noCode && t.sbegintime == hdtimeinfo.sbegintime && t.sendtime == hdtimeinfo.sendtime).FirstOrDefault();
                                if (hd10info != null)
                                {
                                    string temp = hd10info.v_code.Substring(0, 3);
                                    if (temp == "XGL")
                                    {
                                        decimal tempnum = hd10info.amount.Value - item.countNum;
                                        hd10info.amount = tempnum;
                                        //改为修改状态，否则修改不生效。原因是查询时使用了AsNoTracking()加速查询，这种查询状态是不能修改值的，所以得手动更改状态
                                        //db.Entry<hd_yh_detail>(hd10info).State = System.Data.Entity.EntityState.Modified;

                                    }
                                }
                            }
                        }

                    }

                    //当有抵额退货时，管理会员积分扣减等情况
                    if (TuihuoJEtemp != 0)
                    {
                        //管理会员积分
                        if (TuiHuoVipid > 0)
                        {
                            //查会员
                            var vipinfo = db.hd_vip_info.Where(e => e.vipcode == TuiHuoVipid).FirstOrDefault();
                            if (vipinfo != null)
                            {
                                vipcard_temp = vipinfo.vipcard;
                                vipname_temp = vipinfo.vipname;


                                vipinfo.jfnum -= CurrentTuihuJF;
                                ZJFstr = vipinfo.jfnum.ToString();


                                vipinfo.ljxfje -= TuihuoJEtemp;  //减去累计消费

                                string vipidStr = vipinfo.vipcode.ToString();
                                //记录会员积分扣减
                                var vipcz = new hd_vip_cz
                                {
                                    ckh = vipidStr, //会员id
                                    czr = HandoverModel.GetInstance.userID,
                                    rq = System.DateTime.Now, //时间
                                    jf = -CurrentTuihuJF,//积分
                                    fs = (byte)7, //类型
                                    ctype = (byte)3, //抵额退货
                                    srvoucher = jsNoteNO, //单号(0923说要用小票单号)
                                    je = -TuihuoJEtemp, //退款金额
                                    lsh = HandoverModel.GetInstance.scode
                                };

                                db.hd_vip_cz.Add(vipcz);


                                //自动备注积分扣减
                                string memotemp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ： " + " 会员退货扣减积分 " + (-CurrentTuihuJF).ToString() + ";";
                                VipAutoMemoFunc(db, vipNo, vipcard_temp, vipname_temp, memotemp, 3);
                            }
                        }

                        //处理退货后结算信息
                        var jstypeinfo = db.hd_js_type.AsNoTracking().Where(t => t.v_code == HandoverModel.GetInstance.TuiHuoJSDH).Select(t => new { t.js_type, t.bankcode }).FirstOrDefault();
                        string bankcodetemp = jstypeinfo == null ? THNoteID : jstypeinfo.bankcode;
                        int jstypetemp = jstypeinfo == null ? 0 : jstypeinfo.js_type.Value;

                        //更新结算上的金额等，并新增这条退货结算记录
                        db.hd_js_type.Add(new hd_js_type
                        {
                            v_code = HandoverModel.GetInstance.TuiHuoJSDH,
                            cid = HandoverModel.GetInstance.userID,
                            ctime = System.DateTime.Now,
                            je = TuihuoJEtemp,
                            bankcode = bankcodetemp,
                            status = -1,
                            js_type = jstypetemp
                        });


                        //扣减结算单的收入金额
                        var jsInfo = db.hd_js.Where(t => t.v_code == HandoverModel.GetInstance.TuiHuoJSDH).FirstOrDefault();
                        if (jsInfo != null)
                        {
                            jsInfo.je -= TuihuoJEtemp;

                        }
                        //获取办理退货的商品零售单信息
                        var THinfo = db.hd_ls.Where(t => t.v_code == HandoverModel.GetInstance.TuiHuoLSDH).FirstOrDefault();

                        if (THinfo != null)
                        {
                            //不要放在循环体里，防止重复主单
                            //判断是否已经存在
                            var ininfo = db.hd_in.Where(t => t.v_code == THNoteID).FirstOrDefault();
                            if (ininfo != null)
                            {
                                //存在的话更新时间、分店、员工
                                ininfo.scode = HandoverModel.GetInstance.scode;
                                ininfo.cid = HandoverModel.GetInstance.userID;
                                ininfo.ctime = System.DateTime.Now;
                            }
                            else
                            {
                                var sqlTh = new SqlParameter[]
                            {
                                new SqlParameter("@v_code", THNoteID), 
                                new SqlParameter("@scode", HandoverModel.GetInstance.scode),
                                new SqlParameter("@vtype", 109),
                                new SqlParameter("@hs_code",  TuiHuoVipid), 
                                new SqlParameter("@ywy",  THinfo.ywy),
                                new SqlParameter("@srvoucher", THinfo.v_code),
                                new SqlParameter("@remark", "前台抵额退货|" + jsdh),
                                new SqlParameter("@cid", HandoverModel.GetInstance.userID)

                            };

                                db.Database.ExecuteSqlCommand("EXEC [dbo].[Create_in] @v_code,@scode,@vtype,@hs_code,@ywy,@srvoucher,@remark,@cid,1,0", sqlTh);
                            }

                        }

                    }

                    if (!string.IsNullOrEmpty(goodsmemos))
                    {
                        string temp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ： " + " 会员赠品领取 " + goodsmemos + ";";
                        VipAutoMemoFunc(db, vipNo, HandoverModel.GetInstance.VipCard, HandoverModel.GetInstance.VipName, temp, 1);

                    }

                    if (!string.IsNullOrEmpty(Tuihuomemotemp))
                    {
                        //自动备注退货
                        string memotemp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ： " + " 会员进行抵额退货 " + "，退货金额 " + TuihuoJEtemp + "元，" + Tuihuomemotemp + ";";
                        VipAutoMemoFunc(db, vipNo, vipcard_temp, vipname_temp, memotemp, 0);
                    }

                    //退款转储值
                    if (vipToJe > 0)
                    {
                        //储值记录
                        var vip = new hd_vip_cz
                        {
                            ckh = HandoverModel.GetInstance.VipID.ToString(), //会员编号
                            rq = System.DateTime.Now, //时间
                            fs = (byte)2, //储值类型
                            je = vipToJe, //记录储值金额
                            ctype = (byte)4, //此类型为退款转储值
                            srvoucher = jsNoteNO, //单号(0923说要用小票单号)
                            czr = HandoverModel.GetInstance.userID,
                            lsh = HandoverModel.GetInstance.scode
                        };
                        db.hd_vip_cz.Add(vip);
                    }


                    db.SaveChanges();
                    #endregion


                    #region 库存管理

                    #region SQL出库单
                    //可以
                    var sqlOut = new SqlParameter[]
                        {
                            new SqlParameter("@v_code", outNoteNo), new SqlParameter("@scode", HandoverModel.GetInstance.scode),
                            new SqlParameter("@vtype", 209), new SqlParameter("@hs_code", HandoverModel.GetInstance.VipID),
                            new SqlParameter("@ywy", HandoverModel.GetInstance.YWYid), new SqlParameter("@srvoucher", lsNoteNO),
                            new SqlParameter("@remark", "零售出库"), new SqlParameter("@cid", HandoverModel.GetInstance.userID)

                        };

                    db.Database.ExecuteSqlCommand("EXEC [dbo].[Create_out] @v_code,@scode,@vtype,@hs_code,@ywy,@srvoucher,@remark,@cid,1,0", sqlOut);

                    #region SQL商品结算明细单
                    //可以
                    var sqlJS = new SqlParameter[]
                        {
                            new SqlParameter("@j_code", jsNoteNO), 
                            new SqlParameter("@v_code", lsNoteNO),
                            new SqlParameter("@je", CETotalMoney),
                            new SqlParameter("@s_je", JE), 
                            new SqlParameter("@je_je", getMoney),
                            new SqlParameter("@smemo", "零售"),
                            new SqlParameter("@cid", HandoverModel.GetInstance.userID)

                        };

                    db.Database.ExecuteSqlCommand("EXEC [dbo].[Create_js_detail] @j_code,@v_code,@je,@s_je,@je_je,@smemo,@cid,0,0", sqlJS);


                    #endregion

                    //生成出库明细单
                    foreach (var item in goodList)
                    {
                        #region SQL减库存 (没用)
                        //    //没反应，不可以  ,不用了。只用in  和 out  就算了
                        //    var sqlStore = new SqlParameter[]
                        //{
                        //    new SqlParameter("@v_code", outNoteNo),
                        //    new SqlParameter("@cid", HandoverModel.GetInstance.userID)

                        //};

                        //    db.Database.ExecuteSqlCommand("EXEC [dbo].[create_store] @v_code,109,@cid,1", sqlStore);
                        //没用了，现在会自动调整库存
                        //    var sqlStoreMX = new SqlParameter[]
                        //{
                        //    new SqlParameter("@v_code", outNoteNo),
                        //    new SqlParameter("@scode", HandoverModel.GetInstance.scode),
                        //    new SqlParameter("@vtype", 109),
                        //    new SqlParameter("@item_id", item.noCode),
                        //    new SqlParameter("@tm", item.barCodeTM),
                        //    new SqlParameter("@amount", item.countNum),
                        //    new SqlParameter("@jj_price", item.jjPrice),
                        //    new SqlParameter("@ls_price", item.lsPrice),
                        //    new SqlParameter("@pf_price", item.pfPrice),
                        //    new SqlParameter("@batchno", outNoteNo),
                        //    new SqlParameter("@cid", HandoverModel.GetInstance.userID)
                        //};

                        //    db.Database.ExecuteSqlCommand("EXEC [dbo].[create_store_detail] @v_code,@scode,@vtype,@item_id,@tm,@amount,@jj_price,@ls_price,@pf_price,@batchno,@cid,1", sqlStoreMX);


                        #endregion

                        //查询是否有打包的商品，如果有，要分拆后分别减去库存
                        if (item.isDbItem)
                        {
                            var itemdbInfo = db.hd_item_db.AsNoTracking().Where(t => t.sitem_id == item.noCode).ToList();
                            if (itemdbInfo.Count > 0)
                            {
                                foreach (var itemdb in itemdbInfo)
                                {
                                    var item1 = db.hd_item_info.AsNoTracking().Where(t => t.item_id == itemdb.item_id).FirstOrDefault();
                                    if (item1 != null)
                                    {

                                        var itemDblOutMX = new SqlParameter[]
                                    {
                                        new SqlParameter("@v_code", outNoteNo), 
                                        new SqlParameter("@scode", HandoverModel.GetInstance.scode),
                                        new SqlParameter("@vtype", 209),
                                        new SqlParameter("@lid", DBNull.Value.ToString()),
                                        new SqlParameter("@item_id", item1.item_id),
                                        new SqlParameter("@tm", item1.tm),
                                        new SqlParameter("@amount", itemdb.amount.HasValue? itemdb.amount.Value:1),
                                        //new SqlParameter("@cur_store", DBNull.Value.ToString()),  //这个不知是什么，有默认值0.00，如果打开他记得下面加上SQL字段
                                        new SqlParameter("@jj_price", item1.jj_price),
                                        new SqlParameter("@ls_price", item1.ls_price),
                                        new SqlParameter("@pf_price", item1.pf_price.HasValue?item1.pf_price.Value:0),
                                        new SqlParameter("@batchno", outNoteNo),
                                        new SqlParameter("@remark", "零售出库"),
                                        new SqlParameter("@cid", HandoverModel.GetInstance.userID)

                                    };

                                        db.Database.ExecuteSqlCommand("EXEC [dbo].[create_out_detail] @v_code,@scode,@vtype,@lid,@item_id,@tm,@amount,0.00,@jj_price,@ls_price,@pf_price,@batchno,@remark,@cid,1,0", itemDblOutMX);

                                    }

                                }

                            }
                        }
                        else
                        {
                            //出库明细  ,零售出库用209
                            var sqlOutMX = new SqlParameter[]
                            {
                                new SqlParameter("@v_code", outNoteNo), 
                                new SqlParameter("@scode", HandoverModel.GetInstance.scode),
                                new SqlParameter("@vtype", 209),
                                new SqlParameter("@lid", DBNull.Value.ToString()),
                                new SqlParameter("@item_id", item.noCode),
                                new SqlParameter("@tm", item.barCodeTM),
                                new SqlParameter("@amount", item.countNum), 
                                //new SqlParameter("@cur_store", DBNull.Value.ToString()),  //这个不知是什么，有默认值0.00，如果打开他记得下面加上SQL字段
                                new SqlParameter("@jj_price", item.jjPrice),
                                new SqlParameter("@ls_price", item.lsPrice),
                                new SqlParameter("@pf_price", item.pfPrice.HasValue?item.pfPrice.Value:0),
                                new SqlParameter("@batchno", outNoteNo),
                                new SqlParameter("@remark", "零售出库"),
                                new SqlParameter("@cid", HandoverModel.GetInstance.userID)

                            };

                            db.Database.ExecuteSqlCommand("EXEC [dbo].[create_out_detail] @v_code,@scode,@vtype,@lid,@item_id,@tm,@amount,0.00,@jj_price,@ls_price,@pf_price,@batchno,@remark,@cid,1,0", sqlOutMX);
                        }



                    }
                    #endregion


                    #endregion

                    #region 总结结算方式，金额汇总

                    if (CEJStypeList.Count > 0)
                    {
                        foreach (var itemfs in CEJStypeList)
                        {
                            switch (itemfs.cetype)
                            {
                                case 0:
                                    HandoverModel.GetInstance.CashMoney += itemfs.ceJE; //现金
                                    db.hd_js_type.Add(new hd_js_type
                                    {
                                        v_code = jsNoteNO,
                                        cid = HandoverModel.GetInstance.userID,
                                        ctime = timer,
                                        je = itemfs.ceJE,
                                        status = 0,
                                        js_type = 0
                                    });

                                    break;
                                case 1:
                                    HandoverModel.GetInstance.paycardMoney += itemfs.ceJE; //银联
                                    db.hd_js_type.Add(new hd_js_type
                                    {
                                        v_code = jsNoteNO,
                                        cid = HandoverModel.GetInstance.userID,
                                        ctime = timer,
                                        je = itemfs.ceJE,
                                        status = 0,
                                        js_type = 1,
                                        bankcode = payCard
                                    });

                                    //如果有银行优惠
                                    if (payAllje > 0)
                                    {
                                        db.hd_js_type.Add(new hd_js_type
                                        {
                                            v_code = jsNoteNO,
                                            cid = HandoverModel.GetInstance.userID,
                                            ctime = timer,
                                            je = payAllje,
                                            status = 0,
                                            js_type = 9,
                                            bankcode = payCard
                                        });
                                    }

                                    break;
                                case 2:
                                    HandoverModel.GetInstance.LiQuanMoney += itemfs.ceJE; //礼券
                                    db.hd_js_type.Add(new hd_js_type
                                    {
                                        v_code = jsNoteNO,
                                        cid = HandoverModel.GetInstance.userID,
                                        ctime = timer,
                                        je = itemfs.ceJE,
                                        status = 0,
                                        js_type = 2
                                    });

                                    break;
                                case 3:
                                    HandoverModel.GetInstance.VipCardMoney += itemfs.ceJE; //储值卡
                                    db.hd_js_type.Add(new hd_js_type
                                    {
                                        v_code = jsNoteNO,
                                        cid = HandoverModel.GetInstance.userID,
                                        ctime = timer,
                                        je = itemfs.ceJE,
                                        status = 0,
                                        js_type = 3
                                    });

                                    break;

                                case 4:
                                    HandoverModel.GetInstance.ModbilePayMoney += itemfs.ceJE; //移动支付
                                    //使用微信 
                                    if (weixun > 0)
                                    {
                                        db.hd_js_type.Add(new hd_js_type
                                        {
                                            v_code = jsNoteNO,
                                            cid = HandoverModel.GetInstance.userID,
                                            ctime = timer,
                                            je = weixun,
                                            status = 0,
                                            bankcode = weixunStr,
                                            js_type = 8
                                        });
                                    }

                                    //使用支付宝 
                                    if (zfb > 0)
                                    {
                                        db.hd_js_type.Add(new hd_js_type
                                        {
                                            v_code = jsNoteNO,
                                            cid = HandoverModel.GetInstance.userID,
                                            ctime = timer,
                                            je = zfb,
                                            status = 0,
                                            bankcode = zfbStr,
                                            js_type = 7
                                        });
                                    }

                                    break;
                            }
                        }
                    }
                    else
                    {
                        //如果只有一种结算方式，那就是默认的现金了
                        HandoverModel.GetInstance.CashMoney += CEJStypeList[0].ceJE; //现金
                        db.hd_js_type.Add(new hd_js_type
                        {
                            v_code = jsNoteNO,
                            cid = HandoverModel.GetInstance.userID,
                            ctime = timer,
                            je = CEJStypeList[0].ceJE,
                            status = 0,
                            js_type = 0
                        });
                    }



                    db.SaveChanges();
                    #endregion


                    //会员储值消费总额 
                    decimal vipXF = CEJStypeList.Where(t => t.cetype == 3).Select(t => t.ceJE).FirstOrDefault();
                    //银行卡消费总额
                    decimal payXF = CEJStypeList.Where(t => t.cetype == 1).Select(t => t.ceJE).FirstOrDefault();
                    //礼券消费总额
                    decimal LQXF = CEJStypeList.Where(t => t.cetype == 2).Select(t => t.ceJE).FirstOrDefault();
                    //移动消费总额
                    //decimal MoXF = CEJStypeList.Where(t => t.cetype == 4).Select(t => t.ceJE).FirstOrDefault();
                    try
                    {
                        //使用文本排版打印
                        PrintHelper print = new PrintHelper(goodList, vipJF, CETotalMoney, getMoney, jsdh, weixun, zfb, vipXF, payXF, payAllje, LQXF, GiveChange, vipcard, dateStr, false, "", "", vipToJe);
                        print.StartPrint();
                    }
                    catch (Exception)
                    {

                        MessageBox.Show("打印机出现异常，结算仍可进行但无法打印单据，请检查打印机，可尝试重启电脑，必要时候请联系管理员！", "异常提醒", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    }
                    ////使用文本排版打印
                    //PrintHelper print = new PrintHelper(goodList, vipJF, CETotalMoney, getMoney, jsdh, weixun, zfb, vipXF, payXF, payAllje, LQXF, GiveChange, vipcard, dateStr, false, "", "", vipToJe);
                    //print.StartPrint();

                    ////使用窗口打印
                    //PrintForm pr = new PrintForm(goodList, vipJF, CETotalMoney, getMoney, jsdh, jstype, GiveChange, vipcard);
                    ////pr.StartPrint();
                    //pr.ShowDialog();

                    HandoverModel.GetInstance.TuiHuoJSDH = "";  //清空退货结算单号
                    HandoverModel.GetInstance.TuiHuoLSDH = "";  //清空退货零售单号
                }

            }
            catch (Exception e)
            {
                LogHelper.WriteLog("结算窗口保存上传客户订单时出现异常:", e);
                MessageBox.Show("结算出现异常，请联系管理员！");
            }
        }


        #endregion


        //0其它，1活动，2存取货，3积分，4储卡，5定金
        //会员自动备注
        private void VipAutoMemoFunc(hjnbhEntities db, int vipid, string vipCard, string vipName, string Memo, int vtype)
        {
            var VipMemoinfo4 = db.hd_vip_memo.Where(t => t.vipcode == vipid && t.type == vtype).FirstOrDefault();
            if (VipMemoinfo4 != null)
            {
                VipMemoinfo4.memo += Memo;
            }
            else
            {
                //没有就新建
                var newinfo4 = new hd_vip_memo
                {
                    vipcard = vipCard,
                    vipcode = vipid,
                    vipname = vipName,
                    scode = HandoverModel.GetInstance.scode,
                    cid = HandoverModel.GetInstance.userID,
                    memo = Memo,
                    type = vtype,
                    ctime = System.DateTime.Now
                };

                db.hd_vip_memo.Add(newinfo4);
            }
        }


        private void ClosingEntries_Activated(object sender, EventArgs e)
        {
            //this.label16.Text = CETotalMoney.ToString() + " 元";  //本单合计
            //CE_label7.Text = CETotalMoney.ToString();  //应收
            //this.label19.Text = MoLing.ToString();  //已抹
        }

        //退出结算事件
        private void ClosingEntries_FormClosing(object sender, FormClosingEventArgs e)
        {

            CEJStypeList.Clear();
            payCard = "";
            InitData();

            MoPayform.changed -= MoPayform_changed;
            MLForm.changed -= MLForm_changed;
            vipform.changed -= vipform_changed;
            czkform.changed -= czkform_changed;
            CPFrom.changed -= CPFrom_changed;
            qkform.changed -= qkform_changed;
            passwordForm.changed -= passwordForm_changed;

        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (CETotalMoney > 0)
            {
                OnUnionPayFunc();

            }
            else
            {
                MessageBox.Show("退货或者赠送的情况下不可使用此结算方式！请直接按回车完成结算。", "结算提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (CETotalMoney > 0)
            {
                OnOthersFunc();

            }
            else
            {
                MessageBox.Show("退货或者赠送的情况下不可使用此结算方式！请直接按回车完成结算。", "结算提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }


        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (CETotalMoney > 0)
            {
                MLForm.ShowDialog(this);

            }
            else
            {
                MessageBox.Show("应付金额已小于零或已为零！请直接按回车键完成结算。", "结算提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }


        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (CETotalMoney > 0)
            {
                QKFunc();
            }
            else
            {
                MessageBox.Show("应付金额已小于零或已为零！请直接按回车键完成结算。", "结算提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            OnEnterClick();

        }




        protected override void WndProc(ref Message msg)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_CLOSE = 0xF060;

            if (msg.Msg == WM_SYSCOMMAND && ((int)msg.WParam == SC_CLOSE))
            {
                // 点击winform右上关闭按钮 
                // 加入想要的逻辑处理

                changed();

                //return;
            }
            base.WndProc(ref msg);
        }

        //移动支付
        private void button6_Click(object sender, EventArgs e)
        {
            if (CETotalMoney > 0)
            {
                MoPayform.ShowDialog(this);
            }
            else
            {
                MessageBox.Show("退货或者赠送的情况下不可使用此结算方式！请直接按回车完成结算。", "结算提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }



        /// <summary>
        /// 保存退货,提交数据
        /// </summary>
        private int TuiHuoFunc(hjnbhEntities db, GoodsBuy Tuiitem, string jsdh, string THNoteID)
        {

            string JSDH = HandoverModel.GetInstance.TuiHuoJSDH;  //退货的结算单号
            string LSDH = HandoverModel.GetInstance.TuiHuoLSDH;  //退货的零售单号
            //返回 该退货商品购买时候的会员
            int TuiHuovipid = 0; //退货会员

            Tuiitem.Sum = Math.Abs(Tuiitem.Sum);  //把售价取正数，统一
            Tuiitem.lsPrice = Math.Abs(Tuiitem.lsPrice.Value);  //把售价取正数，统一
            Tuiitem.hyPrice = Math.Abs(Tuiitem.hyPrice.Value);  //把售价取正数，统一

            //1、根据条码查相应的零售明细单
            //2、查到此明细单后在th_flag字段做退货标志
            //3、以此退货商品新建入库主单与入库明细单

            string rkStr = "前台抵额退货|" + jsdh;  //退货备注
            //存储过程返回状态码
            int re_temp = 0;

            bool isvip = false;  //是否会员
            var vip = db.hd_ls.AsNoTracking().Where(t => t.v_code == LSDH).Select(t => t.vip).FirstOrDefault();
            if (vip.HasValue)
            {
                if (vip.Value > 0)
                {
                    TuiHuovipid = vip.Value;  //取会员
                    isvip = true;
                }
            }


            //商品信息
            var jsInfo = db.hd_js.Where(t => t.v_code == JSDH).FirstOrDefault();
            if (jsInfo != null)
            {
                //找到零售明细 (注意不要加上 t.th_flag != 1 这个判断，因为目前设定，虽然标志为退货，但数量不一定退清了)
                var mxinfo = db.hd_ls_detail.Where(t => t.item_id == Tuiitem.noCode && t.v_code == LSDH && t.amount > 0 && t.vtype == Tuiitem.vtype).FirstOrDefault();
                if (mxinfo != null)
                {
                    //取得批发价
                    var pf = db.hd_item_info.AsNoTracking().Where(t => t.item_id == Tuiitem.noCode).Select(t => t.pf_price).FirstOrDefault();
                    //mxinfo.amount -= item.countNum;  //目前不减去退货数量，是新增一行数量为负的，原来的标志退货
                    mxinfo.th_flag = 1;  //退货标志
                    mxinfo.th_date = System.DateTime.Now;  //退货时间

                    //该退货商品总积分
                    decimal tuihuojftemp = mxinfo.jf.HasValue ? mxinfo.jf.Value : 0.00m;
                    //该退货商品单品积分
                    decimal dpjetemp = tuihuojftemp / mxinfo.amount.Value;
                    //此次退货单品扣减积分
                    decimal currjf = dpjetemp * Tuiitem.countNum;
                    //此次退货总共扣减积分
                    CurrentTuihuJF += currjf;

                    //新增退货的明细,处理零售明细,减去退货的数量，新增这行记录
                    var tuihuoMX = new hd_ls_detail
                    {
                        v_code = LSDH, //标识单号
                        item_id = Tuiitem.noCode,//商品货号
                        tm = Tuiitem.barCodeTM,//条码
                        cname = Tuiitem.goods,//名称
                        spec = Tuiitem.spec,//规格
                        //hpack_size = (decimal?)item.hpackSize,//不知是什么,包装规格
                        unit = mxinfo.unit,  //单位
                        amount = -Tuiitem.countNum, //数量
                        jj_price = mxinfo.jj_price, //进价
                        ls_price = isvip ? Tuiitem.hyPrice : Tuiitem.lsPrice,
                        yls_price = mxinfo.yls_price,//原零售价
                        zk = mxinfo.zk,//折扣
                        iszs = mxinfo.iszs,//是否赠送
                        cid = HandoverModel.GetInstance.userID,//零售员ID
                        ctime = System.DateTime.Now, //出单时间
                        vtype = mxinfo.vtype,  //活动类型
                        ywy = mxinfo.ywy,
                        jf = -currjf,
                        th_date = System.DateTime.Now,
                        dzth_code = jsdh  //此次小票上结算单号
                    };

                    db.hd_ls_detail.Add(tuihuoMX);

                    jsInfo.remark += rkStr;  //备注

                    db.SaveChanges();
                }

                //要判断是否打包商品
                var dbinfo = db.hd_item_db.AsNoTracking().Where(t => t.sitem_id == Tuiitem.noCode).ToList();
                if (dbinfo.Count > 0)
                {
                    foreach (var itemdb in dbinfo)
                    {
                        var itemdbinfo = db.hd_item_info.AsNoTracking().Where(t => t.item_id == itemdb.item_id).FirstOrDefault();
                        decimal templs = 0;
                        if (vip > 0)
                        {
                            templs = itemdb.vip_bj.Value;
                        }
                        else
                        {
                            templs = itemdb.bj.Value;

                        }

                        var sqlThdbMX = new SqlParameter[]
                                    {
                                        new SqlParameter("@v_code", THNoteID), 
                                        new SqlParameter("@scode", HandoverModel.GetInstance.scode),
                                        new SqlParameter("@vtype", 109),
                                        //new SqlParameter("@lid", 0),  这个不知是什么
                                        new SqlParameter("@item_id",  itemdbinfo.item_id),
                                        new SqlParameter("@tm", itemdbinfo.tm),
                                        new SqlParameter("@amount", itemdb.amount.Value*Tuiitem.countNum),
                                        new SqlParameter("@jj_price", itemdbinfo.jj_price),
                                        new SqlParameter("@ls_price",  templs),
                                        new SqlParameter("@pf_price", itemdbinfo.pf_price.HasValue?itemdbinfo.pf_price.Value:0),
                                        new SqlParameter("@remark", "零售退货"),
                                        new SqlParameter("@cid", HandoverModel.GetInstance.userID)
                                    };

                        re_temp = db.Database.ExecuteSqlCommand("EXEC [dbo].[create_in_detail] @v_code,@scode,@vtype,0,@item_id,@tm,@amount,@jj_price,@ls_price,@pf_price,@remark,@cid,1,0", sqlThdbMX);


                    }
                }
                else
                {
                    if (mxinfo != null)
                    {
                        #region SQL操作退货
                        //之前 退货vtype 是103， 8月18号改为109
                        var sqlThMX = new SqlParameter[]
                                {
                                    new SqlParameter("@v_code", THNoteID), 
                                    new SqlParameter("@scode", HandoverModel.GetInstance.scode),
                                    new SqlParameter("@vtype", 109),
                                    //new SqlParameter("@lid", 0),  这个不知是什么
                                    new SqlParameter("@item_id",  mxinfo.item_id),
                                    new SqlParameter("@tm", mxinfo.tm),
                                    new SqlParameter("@amount", Tuiitem.countNum),
                                    new SqlParameter("@jj_price", mxinfo.jj_price),
                                    new SqlParameter("@ls_price",  Tuiitem.Sum),
                                    new SqlParameter("@pf_price", Tuiitem.pfPrice),
                                    new SqlParameter("@remark", "零售退货"),
                                    new SqlParameter("@cid", HandoverModel.GetInstance.userID)
                                };

                        re_temp = db.Database.ExecuteSqlCommand("EXEC [dbo].[create_in_detail] @v_code,@scode,@vtype,0,@item_id,@tm,@amount,@jj_price,@ls_price,@pf_price,@remark,@cid,1,0", sqlThMX);

                        #endregion
                    }

                }


                if (re_temp > 0)
                {
                    //退货成功
                    //退货金额
                    HandoverModel.GetInstance.RefundMoney += Tuiitem.Sum;
                }
                else
                {
                    MessageBox.Show("退货数据登记失败！请核实此单据真实性！");
                }
            }


            return TuiHuovipid;

        }



        private void button7_Click(object sender, EventArgs e)
        {

            ToVipSaveCK();
        }


        //存入储值逻辑处理
        private void ToVipSaveCK()
        {
            //练习模式下不允许
            if (HandoverModel.GetInstance.isLianxi)
            {
                MessageBox.Show("练习模式下该操作无效！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (CETotalMoney >= 0)
            {
                MessageBox.Show("此功能只限于需要给客人退款（金额小于零）时使用！");
                return;
            }

            if (HandoverModel.GetInstance.VipID > 0)
            {
                decimal temp = Math.Abs(CETotalMoney);

                string vipstr = "[" + HandoverModel.GetInstance.VipCard + "]" + HandoverModel.GetInstance.VipName;
                if (DialogResult.Yes == MessageBox.Show("您已经登录了会员：" + vipstr + "，是否自动把退货金额" + temp.ToString() + "元存入储值卡并完成结算？", "提醒", MessageBoxButtons.YesNo))
                {
                    ToSaveCZKFunc();
                }

            }
            else
            {
                MessageBox.Show("您还没有登记会员，请先按ESC键取消结算，并在收银窗口按F12键登记会员，以便调整商品价格！", "会员登入提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
        }



        //转入储值- 直接储值
        private void ToSaveCZKFunc()
        {
            //条件验证要在子窗口中做限制了
            int vipid = HandoverModel.GetInstance.VipID;
            if (vipid > 0)
            {
                using (var db = new hjnbhEntities())
                {
                    var Vipinfo = db.hd_vip_info.Where(t => t.vipcode == vipid).FirstOrDefault();
                    if (Vipinfo != null)
                    {
                        decimal temp = Math.Abs(CETotalMoney);
                        decimal yetemp = Vipinfo.czk_ye.HasValue ? Vipinfo.czk_ye.Value : 0;
                        decimal alltemp = yetemp + temp;
                        Vipinfo.czk_ye = alltemp;

                        //储值记录我放在结算逻辑里了，因为需要单号

                        //自动备注
                        string memotemp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ： " + " 会员储卡充减 " + temp + "元;";
                        VipAutoMemoFunc(db, vipid, HandoverModel.GetInstance.VipCard, HandoverModel.GetInstance.VipName, memotemp, 4);

                        if (db.SaveChanges() > 0)
                        {
                            //如果要结算金额为0就开启，目前想保留正常出售的商品价格
                            //CETotalMoney = 0.00m;
                            //getMoney = 0.00m;

                            decimal sumTotal = goodList.Where(t => t.isTuiHuo == false).Select(t => t.Sum).Sum();  //实际上商品价格总额，过滤退货的
                            CETotalMoney = sumTotal;
                            getMoney = sumTotal;
                            UpdataJEUI();
                            this.vipToJe = temp;
                            HandoverModel.GetInstance.CZVipJE += temp;
                            MessageBox.Show("存入储值卡余额成功！本次存入金额：" + temp.ToString() + "元，目前总余额为：" + alltemp.ToString() + "元");
                            OnEnterClick();  //结算

                        }
                    }

                }

            }
        }









    }
}
