using Common;
using hjn20160520.Common;
using hjn20160520.Models;
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

        decimal AllCzkJe = 0; //全部使用的储卡金额，包括定金与分期
        decimal DJJe = 0;  //使用的定金
        decimal FQJe = 0;  //使用的分期金额
        decimal CzkJe = 0; //本单储值卡使用金额
        BindingList<VipFQModel> FqList = new BindingList<VipFQModel>();  //用过的分期列表

        decimal weixun = 0;  //本单使用微信支付金额
        decimal zfb = 0;  //本单使用支付宝金额
        public string weixunStr = "", zfbStr = "";  //微信与支付宝账号

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

            vipJF = 0.00m;

            OnCouponFunc();  //计算礼券

            MLForm.changed += MLForm_changed;

            vipform.changed += vipform_changed;
            czkform.changed += czkform_changed;
            CPFrom.changed += CPFrom_changed;
            qkform.changed += qkform_changed;
            MoPayform.changed += MoPayform_changed;

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
                    OnUnionPayFunc();
                    break;
                //移动支付
                case Keys.F4:
                    MoPayform.ShowDialog(this);

                    break;
                //储值卡
                case Keys.F6:
                    OnOthersFunc();
                    break;
                //抹零
                case Keys.F7:
                    //MLForm.changed += MLForm_changed;
                    MLForm.ShowDialog(this);
                    break;

                //挂账
                case Keys.F8:
                    QKFunc();

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
            //getMoney = Convert.ToDecimal(CE_textBox1.Text);
            if (!decimal.TryParse(CE_textBox1.Text.Trim(), out getMoney))
            {
                MessageBox.Show("现收金额输入有误，请重新输入！");
            }
            if (GiveChange < 0)
            {
                CE_label5.Text = GiveChange.ToString();  //找零
                //MessageBox.Show("所收取金额不足以全额付款，请重新确认");
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

            //默认
            CEJEFunc(0, CETotalMoney);
        }

        //银联卡支付
        private void OnUnionPayFunc()
        {

            CPFrom.ShowDialog(this);
        }

        //购物劵支付
        private void OnCouponFunc()
        {

            var Coupontemp = goodList.Where(t => t.hyPrice < 0 || t.lsPrice < 0).ToList();
            if (Coupontemp.Count > 0)
            {
                decimal temp = 0;
                foreach (var item in Coupontemp)
                {
                    if (item.lsPrice < 0)
                    {
                        temp += item.lsPrice.Value;
                    }
                    else
                    {
                        temp += item.hyPrice.Value;
                    }
                }

                decimal aass = Math.Abs(temp);
                if (CETotalMoney > 0)
                {
                    CEJEFunc(0, CETotalMoney);
                    CEJEFunc(2, aass);
                }
                else
                {
                    CEJEFunc(2, aass);
                    this.label5.Text = "礼券";
                }


            }



        }

        //其它方式支付,会员储值卡
        private void OnOthersFunc()
        {
            int vipid = HandoverModel.GetInstance.VipID;
            if (vipid != 0)
            {
                using (var db = new hjnbhEntities())
                {
                    var Vipinfo = db.hd_vip_info.Where(t => t.vipcode == vipid).FirstOrDefault();
                    if (Vipinfo != null)
                    {
                        decimal czk = Vipinfo.czk_ye.HasValue ? Vipinfo.czk_ye.Value : 0;
                        if (czk <= 0)
                        {
                            MessageBox.Show("会员储值余额为 0 元，不可使用储值卡消费！");
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
                tipForm.Tiplabel.Text = "您还没有登记会员，请先在收银窗口按F12登记会员后再进行储值卡消费！";
                tipForm.ShowDialog();
                //VipShopForm vipform = new VipShopForm();
                //vipform.ShowDialog();
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

        //单号计算方式，当前时间+00000+id
        long no_temp = Convert.ToInt64(System.DateTime.Now.ToString("yyyyMMdd") + "000000");
        DateTime timer = System.DateTime.Now; //统一成单时间
        string dateStr = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm");
        //向数据库中存储单据
        private void DBFunc()
        {
            try
            {
                if (CFXPForm.isLianXi) return;
                decimal total = goodList.Select(t => t.Sum.Value).Sum();  //实际上商品价格总额

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





                    //结算单
                    var HDJS = new hd_js
                    {
                        v_code = jsNoteNO,
                        ls_code = lsNoteNO,
                        js_type = (byte)jstype, //结算类型
                        ysje = CETotalMoney, //应收金额
                        //je = JE,  //收取金额 ，入账金额
                        je = total,
                        ssje = getMoney, //实收金额
                        qkje = QKjs,  //还有个欠款字段
                        status = 1, //状态，1为确认结算
                        //del_flag = 0, //删除标记
                        moling = MoLing, //抹零
                        remark = CFXPForm.ZKZD != 0 ? ("整单打折" + (CFXPForm.ZKZD / 100).ToString()) : string.Empty, // 备注
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

                                var vipczk = new hd_vip_cz
                                {
                                    ckh = vipNo.ToString(), //会员编号
                                    rq = timer, //时间
                                    fs = (byte)3, //类型
                                    srvoucher = jsNoteNO, //单号
                                    je = -vipce.ceJE,
                                    czr = HandoverModel.GetInstance.userID,
                                    ctype = (byte)0,
                                    lsh = HandoverModel.GetInstance.scode
                                };
                                db.hd_vip_cz.Add(vipczk);


                                //会员储卡消费自动备注
                                string temp4 = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ： " + " 会员储卡余额 -" + vipce.ceJE.ToString() + ";";
                                VipAutoMemoFunc(db, vipNo, HandoverModel.GetInstance.VipCard, HandoverModel.GetInstance.VipName, temp4, 4);

                            }

                            decimal jftemp = total / 10;  //记录积分方便打印

                            if (CFXPForm.isVipDate)
                            {
                                jftemp *= CFXPForm.vipDtaeJF;
                            }

                            if (HandoverModel.GetInstance.isVipBirthday)
                            {
                                jftemp *= 2;
                            }

                            vipJF = jftemp;

                            decimal tempJF = Vipinfo.jfnum.HasValue ? Vipinfo.jfnum.Value : 0;
                            tempJF += jftemp;
                            Vipinfo.jfnum = tempJF;  //10元换1分

                            decimal tempLJJE = Vipinfo.ljxfje.HasValue ? Vipinfo.ljxfje.Value : 0;
                            tempLJJE += total;
                            Vipinfo.ljxfje = tempLJJE; //累计积分金额
                            Vipinfo.dtMaxChanged = timer;  //最近消费时间


                            //会员与消费的零售订单关联
                            var vip = new hd_vip_cz
                            {
                                ckh = vipNo.ToString(), //会员编号
                                rq = timer, //时间
                                fs = (byte)7, //消费类型
                                srvoucher = jsNoteNO, //单号
                                je = total,
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


                    #endregion

                    #region 活动相关、明细

                    string goodsmemos = ""; //领取的赠品
                    foreach (var item in goodList)
                    {
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
                            iszs = (byte)zs_temp,//是否赠送
                            cid = HandoverModel.GetInstance.userID,//零售员ID
                            ctime = timer, //出单时间
                            vtype = (byte)item.vtype,  //活动类型
                            ywy = item.ywy
                        };

                        db.hd_ls_detail.Add(HDLSMX);


                        //会员赠送记录,就是会产生赠送行为的活动
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

                    if (!string.IsNullOrEmpty(goodsmemos))
                    {
                        string temp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ： " + " 会员赠品领取 " + goodsmemos + ";";
                        VipAutoMemoFunc(db, vipNo, HandoverModel.GetInstance.VipCard, HandoverModel.GetInstance.VipName, temp, 1);

                    }

                    db.SaveChanges();
                    #endregion


                    #region 库存管理



                    #region EF新增出库单(没用)

                    ////生成出库主单
                    //var outNote = new hd_out
                    //{
                    //    v_code = outNoteNo, //此主键不能为null，看来要在这里就传入单号
                    //    vtype = (byte)201, //零售类型
                    //    scode = HandoverModel.GetInstance.scode,//仓库号
                    //    hs_code = CashiersFormXP.GetInstance.VipID,//货商号/客户号/对象仓库
                    //    ywy = HandoverModel.GetInstance.YWYid,//业务员
                    //    //remark = "", //备注
                    //    srvoucher = lsNoteNO,//源单号
                    //    cid = HandoverModel.GetInstance.userID,//制单人
                    //    ctime = timer
                    //};
                    //db.hd_out.Add(outNote);


                    ////生成出库明细单
                    //foreach (var item in CashiersFormXP.GetInstance.goodsBuyList)
                    //{
                    //    var outMXnote = new hd_out_detail
                    //    {
                    //        v_code = outNoteNo,//单号
                    //        scode = HandoverModel.GetInstance.scode,  //分店
                    //        item_id = item.noCode,//货号
                    //        tm = item.barCodeTM,//条码
                    //        cname = item.goods,//名称
                    //        spec = item.spec,//规格
                    //        hpack_size = item.hpackSize,//装数
                    //        unit = item.unit,//单位
                    //        amount = item.countNum,//数量
                    //        jj_price = item.jjPrice,//进价
                    //        yjj_price = item.jjPrice, //原进价，不知什么鬼
                    //        ls_price = item.lsPrice,//零售价
                    //        yls_price = item.lsPrice, //原零售价，不知什么鬼
                    //        remark = item.goodsDes,//其他说明
                    //        ctime = timer
                    //    };
                    //    db.hd_out_detail.Add(outMXnote);

                    //}
                    #endregion
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


                    #region EF减库存 (没用)
                    /*
                        int scode = HandoverModel.GetInstance.scode;  //分店号
                        //库存商品数量扣减
                        for (int i = 0; i < CashiersFormXP.GetInstance.goodsBuyList.Count; i++)
                        {
                            int itemid = CashiersFormXP.GetInstance.goodsBuyList[i].noCode;  //每个商品货号
                            var count = CashiersFormXP.GetInstance.goodsBuyList[i].countNum;
                            var info = db.hd_istore.Where(t => t.scode == scode && t.item_id == itemid).FirstOrDefault();
                            var mxinfo = db.hd_istore_detail.Where(t => t.scode == scode && t.item_id == itemid).FirstOrDefault();
                            //减主库存
                            if (info != null)
                            {
                                info.amount -= count;
                                info.uid = HandoverModel.GetInstance.userID;
                                info.utime = timer;
                            }
                            else
                            {
                                //如果没有就新增
                                var addistore = new hd_istore
                                {
                                    scode = scode,
                                    item_id = CashiersFormXP.GetInstance.goodsBuyList[i].noCode,
                                    spec = CashiersFormXP.GetInstance.goodsBuyList[i].spec,
                                    amount = -CashiersFormXP.GetInstance.goodsBuyList[i].countNum,  //负库存
                                    jj_price = CashiersFormXP.GetInstance.goodsBuyList[i].jjPrice,
                                    ls_price = CashiersFormXP.GetInstance.goodsBuyList[i].lsPrice,
                                    cid = HandoverModel.GetInstance.userID,
                                    ctime = timer,
                                    uid = HandoverModel.GetInstance.userID,
                                    utime = timer,
                                    status = 0,
                                    hy_price = CashiersFormXP.GetInstance.goodsBuyList[i].hyPrice
                                };
                                db.hd_istore.Add(addistore);
                            }

                            //减主库存明细
                            if (mxinfo != null)
                            {
                                mxinfo.amount -= count;
                                mxinfo.uid = HandoverModel.GetInstance.userID;
                                mxinfo.utime = timer;

                            }
                            else
                            {
                                //如果没有就新增
                                var addistoreMx = new hd_istore_detail
                                {
                                    scode = scode,
                                    item_id = CashiersFormXP.GetInstance.goodsBuyList[i].noCode,
                                    spec = CashiersFormXP.GetInstance.goodsBuyList[i].spec,
                                    amount = -CashiersFormXP.GetInstance.goodsBuyList[i].countNum,  //负库存
                                    jj_price = CashiersFormXP.GetInstance.goodsBuyList[i].jjPrice,
                                    ls_price = CashiersFormXP.GetInstance.goodsBuyList[i].lsPrice,
                                    cid = HandoverModel.GetInstance.userID,
                                    ctime = timer,
                                    uid = HandoverModel.GetInstance.userID,
                                    utime = timer,
                                    batch_no = outNoteNo,
                                    srvoucher = lsNoteNO,
                                    unit = CashiersFormXP.GetInstance.goodsBuyList[i].unit,
                                    hpack_size = CashiersFormXP.GetInstance.goodsBuyList[i].hpackSize,
                                    cname = CashiersFormXP.GetInstance.goodsBuyList[i].goods

                                };
                                db.hd_istore_detail.Add(addistoreMx);
                            }

                            //减去打包商品的库存
                            //查找有没有打包商品
                            int id = CashiersFormXP.GetInstance.goodsBuyList[i].noCode;
                            var itemdb = db.hd_item_db.Where(t => t.item_id == id).FirstOrDefault();
                            if (itemdb != null)
                            {
                                itemdb.amount -= CashiersFormXP.GetInstance.goodsBuyList[i].countNum;
                            }

                        }
                    */

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

                    //使用文本排版打印
                    PrintHelper print = new PrintHelper(goodList, vipJF, CETotalMoney, getMoney, jsdh,weixun,zfb, vipXF, payXF, payAllje, LQXF, GiveChange, vipcard, dateStr);
                    print.StartPrint();

                    ////使用窗口打印
                    //PrintForm pr = new PrintForm(goodList, vipJF, CETotalMoney, getMoney, jsdh, jstype, GiveChange, vipcard);
                    ////pr.StartPrint();
                    //pr.ShowDialog();


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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OnUnionPayFunc();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            OnOthersFunc();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            MLForm.ShowDialog(this);

        }

        private void button4_Click(object sender, EventArgs e)
        {
            QKFunc();

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

            MoPayform.ShowDialog(this);
        }







    }
}
