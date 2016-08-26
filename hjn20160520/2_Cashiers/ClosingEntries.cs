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
        //信息提示窗口
        TipForm tipForm;
        //结算方式，默认为现金
        JSType jstype = JSType.Cash;

        //用于记录付款方式的列表
        List<CEJStype> CEJStypeList = new List<CEJStype>();

        //抹零窗口
        MoLingForm MLForm = new MoLingForm();
        CardPayForm CPFrom = new CardPayForm();  //银联卡窗口

        VipShopForm vipform = new VipShopForm();

        VipCZKForm czkform = new VipCZKForm();  //会员储值卡消费
        QKJEForm qkform = new QKJEForm();  //挂账窗口

        //public PrintHelper printer;  //小票打印
        //decimal vipCradXF = 0.00m;  //此单储卡消费额

        //用于其它窗口传值给本窗口控件
        //这是委托与事件的第一步  
        public delegate void ClosingEntriesHandle(decimal? jf, decimal? ysje, decimal? ssje, string jsdh, JSType jstype, decimal vipcardXF, decimal payXF, decimal LQXF, decimal? zhaoling, string vip, string date);
        public event ClosingEntriesHandle changed;

        public BindingList<GoodsBuy> goodList = new BindingList<GoodsBuy>();

        //抹零
        public decimal? MoLing { get; set; }
        //此单所得会员积分
        private decimal? vipJF;  //本次积分
        private string jsdh; //结算单号，方便打印
        //找零
        public decimal? GiveChange
        {
            get { return (getMoney - CETotalMoney); }

        }

        //收取金额 ， 最终入账的结算金额 , 收取金额  =  应收金额-欠款金额
        public decimal JE { get { return (CETotalMoney - QKjs); } }

        //应收金额 ， 是商品总金额
        public decimal CETotalMoney { get; set; }

        //实收金额,从客户手中收取的金额，有多就找零的，不够就是欠款
        public decimal getMoney { get; set; }


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


        //银行卡号
        public string payCard { get; set; }

        public ClosingEntries()
        {
            InitializeComponent();
        }

        private void ClosingEntries_Load(object sender, EventArgs e)
        {
            //if (GetInstance == null) GetInstance = this;
            CE_textBox1.Focus();

            //MessageBox.Show(CETotalMoney.ToString());
            //CETotalMoney = CashiersFormXP.GetInstance.totalMoney;

            //CashiersFormXP.GetInstance.changed += GetInstance_changed;
            ShowUI();

            tipForm = new TipForm();
            CEJStypeList.Clear();
            MoLing = 0.00m;
            //vipCradXF = 0.00m;
            vipJF = 0.00m;
            //默认现金消费
            OnCashFunc();
            OnCouponFunc();  //计算礼券


            MLForm.changed += MLForm_changed;
            vipform.VIPchanged += vipform_VIPchanged;
            vipform.changed += vipform_changed;
            czkform.changed += czkform_changed;
            CPFrom.changed += CPFrom_changed;
            qkform.changed += qkform_changed;
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
        /// <param name="card"></param>
        void CPFrom_changed(string card)
        {
            this.payCard = card;
            CEJEFunc(1, CETotalMoney);
            //立即全款支付
            OnEnterClick();
        }

        /// <summary>
        /// 处理储值卡消费
        /// </summary>
        /// <param name="s"></param>
        void czkform_changed(decimal s)
        {
            if (s < CETotalMoney)
            {
                if (DialogResult.Yes == MessageBox.Show("此储值卡金额不足以全额付款，是否抵消部分应付金额？", "提醒", MessageBoxButtons.YesNo))
                {
                    CETotalMoney -= s;
                    UpdataJEUI();
                    //增加储值卡的金额
                    CEJEFunc(3, s);


                    //增加现金的金额,支付上面不足的部分
                    CEJEFunc(0, CETotalMoney);

                }

            }
            else
            {
                CETotalMoney -= s;
                UpdataJEUI();

                this.label5.Text = "储值卡";
                //全款支付
                CEJEFunc(3, s);
            }


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



        void vipform_changed(string s)
        {
            CashiersFormXP.GetInstance.VipName = s;
            CashiersFormXP.GetInstance.HDUIFunc();
            VIPShowUI();        
        }

        void vipform_VIPchanged(int vipid, string vipcrad, int viplv)
        {
            CashiersFormXP.GetInstance.VipID = vipid;
            CashiersFormXP.GetInstance.VipCARD = vipcrad;
            CashiersFormXP.GetInstance.viplv = viplv;

        }



        //给登录会员时刷新
        public void VIPShowUI()
        {
            CETotalMoney = CashiersFormXP.GetInstance.totalMoney;
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
                    //InitData();
                    this.Close();
                    break;
                //现金
                case Keys.F3:
                    //OnCashFunc();
                    break;
                //银联
                case Keys.F4:
                    OnUnionPayFunc();
                    break;
                //购物劵
                case Keys.F5:
                    //OnCouponFunc();
                    break;
                //其它
                case Keys.F6:
                    OnOthersFunc();
                    break;
                //抹零
                case Keys.F10:
                    //MLForm.changed += MLForm_changed;
                    MLForm.ShowDialog(this);
                    break;

                //挂账
                case Keys.F11:
                    QKFunc();

                    break;
                //会员登记
                case Keys.F12:
                    //vipform = new VipShopForm();
                    vipform.ShowDialog();

                    break;
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
            //isCardPay = false;
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
            if (CashiersFormXP.GetInstance.VipID != 0)
            {
                qkform.ShowDialog(this);
            }
            else
            {
                tipForm.Tiplabel.Text = "您还没有登记会员，请先登记会员!";
                tipForm.ShowDialog();
                //VipShopForm vipform = new VipShopForm();
                vipform.ShowDialog();
            }

        }

        //回车逻辑
        private void OnEnterClick()
        {
            getMoney = Convert.ToDecimal(CE_textBox1.Text);
            if (GiveChange < 0)
            {
                CE_label5.Text = GiveChange.ToString();  //找零
                //MessageBox.Show("所收取金额不足以全额付款，请重新确认");
                tipForm.Tiplabel.Text = "所收取金额不足以全额付款，请重新确认!";
                tipForm.ShowDialog();
            }
            else
            {
                DBFunc();
                CashiersFormXP.GetInstance.label85.Visible = true;
                CashiersFormXP.GetInstance.label86.Visible = true;
                CashiersFormXP.GetInstance.label87.Visible = true;
                CashiersFormXP.GetInstance.label88.Visible = true;
                CashiersFormXP.GetInstance.label92.Visible = true;
                CashiersFormXP.GetInstance.label91.Visible = true;

                CashiersFormXP.GetInstance.label87.Text = this.getMoney.ToString() + " 元";  //收款
                CashiersFormXP.GetInstance.label88.Text = this.GiveChange.ToString() + " 元";  //找零

                //上单实收
                CashiersFormXP.GetInstance.label92.Text = this.getMoney.ToString() + " 元";
                CashiersFormXP.GetInstance.label91.Text = this.CETotalMoney.ToString() + " 元";  //上单合计

                CE_textBox1.Text = "";
                CE_label5.Text = "0.00";

                CashiersFormXP.GetInstance.isNewItems(true);
                this.MoLing = 0;  //结单后把上单抹零纪录清空
                this.Close();

            }
        }

        //现金支付
        private void OnCashFunc()
        {
            this.label5.Text = "现金";
            //jstype = JSType.Cash;
            //默认
            CEJEFunc(-1, CETotalMoney);
        }

        //银联卡支付
        private void OnUnionPayFunc()
        {

            CPFrom.ShowDialog(this);
        }

        //购物劵支付
        private void OnCouponFunc()
        {
            //this.label5.Text = "礼劵";
            //jstype = JSType.Coupon;
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
            int vipid = CashiersFormXP.GetInstance.VipID;
            if (vipid != 0)
            {
                using (var db = new hjnbhEntities())
                {
                    var Vipinfo = db.hd_vip_info.Where(t => t.vipcode == vipid).FirstOrDefault();
                    if (Vipinfo != null)
                    {
                        decimal czk = Vipinfo.czk_ye.HasValue ? Vipinfo.czk_ye.Value : 0;
                        if (czk <= 0 )
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
                tipForm.Tiplabel.Text = "您还没有登记会员，请先登记会员后再进行储值卡消费！";
                tipForm.ShowDialog();
                //VipShopForm vipform = new VipShopForm();
                vipform.ShowDialog();
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
            //try
            //{
            if (CashiersFormXP.GetInstance.isLianXi) return;
            decimal total = goodList.Select(t => t.Sum.Value).Sum();  //实际上商品价格总额

            string lsNoteNO = string.Empty;
            string jsNoteNO = string.Empty;
            string outNoteNo = string.Empty;
            decimal zktemp = CashiersFormXP.GetInstance.ZKZD / 100;  //整单折扣

            using (var db = new hjnbhEntities())
            {


                    #region 零售单与结算单
                    //后台需要上传会员编号
                    int vipNo = CashiersFormXP.GetInstance.VipID;
                    string vipcard = CashiersFormXP.GetInstance.VipCARD;

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
                    //jsNoteNO = "JS" + (no_temp + HDJS.id).ToString();  //获取ID并生成结算单号
                    outNoteNo = "LSC" + (no_temp + HDLS.id).ToString();//获取出库单号

                    HDLS.v_code = lsNoteNO; //零售单号
                    //HDJS.ls_code = lsNoteNO;  //零售单号
                    //HDJS.v_code = jsNoteNO;  //结算单号
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
                        remark = CashiersFormXP.GetInstance.ZKZD !=0 ? ("整单打折" + (CashiersFormXP.GetInstance.ZKZD / 100).ToString()) : string.Empty, // 备注
                        bankcode = payCard,//银行卡号
                        cid = HandoverModel.GetInstance.userID, //收银员工
                        ctime = timer //成单时间

                    };
                    db.hd_js.Add(HDJS);





                    #region VIP单据与积分

                    if (vipNo != 0)
                    {
                        var Vipinfo = db.hd_vip_info.Where(t => t.vipcode == vipNo).FirstOrDefault();
                        if (Vipinfo != null)
                        {
                            decimal jftemp = total / 10;  //记录积分方便打印

                            if (CashiersFormXP.GetInstance.isVipDate)
                            {
                                jftemp *= CashiersFormXP.GetInstance.vipDtaeJF;
                            }

                            if (CashiersFormXP.GetInstance.isVipBirthday)
                            {
                                jftemp *= 2;
                            }

                            vipJF = jftemp;
                            //Vipinfo.jfnum += (HDJS.ysje / 10);  //10元换1分
                            Vipinfo.jfnum += jftemp;  //10元换1分

                            Vipinfo.ljxfje += JE; //累计积分金额
                            //vipJF = HDJS.ysje / 10;  //记录积分方便打印
                            Vipinfo.sVipMemo += CashiersFormXP.GetInstance.VipMdemo;
                            Vipinfo.dtMaxChanged = timer;  //最近消费时间
                            //会员与消费的零售订单关联
                            var vip = new hd_vip_cz
                            {
                                ckh = vipNo.ToString(), //会员编号
                                rq = timer, //时间
                                fs = (byte)7, //类型
                                srvoucher = jsNoteNO, //单号
                                je = total,
                                //je = JE,
                                //jf = HDJS.ysje / 10,//积分
                                //jf = HDJS.ysje.Value / 10,
                                jf = jftemp,
                                lsh = HandoverModel.GetInstance.scode
                            };
                            db.hd_vip_cz.Add(vip);

                            //如果是会员储值卡消费
                            var vipce = CEJStypeList.Where(t => t.cetype == 3).FirstOrDefault();
                            if (vipce != null)
                            {
                                Vipinfo.czk_ye -= vipce.ceJE;
                            }

                            //if (jstype == JSType.Others)
                            //{
                            //    Vipinfo.czk_ye -= vipCradXF;
                            //    HandoverModel.GetInstance.VipCardMoney += vipCradXF;
                            //}
                        }

                    }


                    #endregion

                    #endregion
                    foreach (var item in goodList)
                    {
                        int zs_temp = item.isZS ? 1 : 0;

                        //因为活动中我用了pfPrice来记录原零售价，其它是没有的
                        if (item.vtype !=0 )
                        {
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
                                ls_price = CashiersFormXP.GetInstance.VipID == 0 ? item.lsPrice : item.hyPrice,//零售价
                                yls_price = item.pfPrice,//原零售价
                                zk = item.ZKDP,//折扣
                                iszs = (byte)zs_temp,//是否赠送
                                cid = HandoverModel.GetInstance.userID,//零售员ID
                                ctime = timer, //出单时间
                                vtype = (byte)item.vtype,  //活动类型
                                ywy = item.ywy
                            };

                            db.hd_ls_detail.Add(HDLSMX);

                        }
                        else
                        {
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
                                ls_price = CashiersFormXP.GetInstance.VipID == 0 ? item.lsPrice : item.hyPrice,//零售价
                                yls_price = item.lsPrice,//原零售价
                                zk = item.ZKDP,//折扣
                                iszs = (byte)zs_temp,//是否赠送
                                cid = HandoverModel.GetInstance.userID,//零售员ID
                                ctime = timer, //出单时间
                                vtype = (byte)item.vtype,  //活动类型
                                ywy = item.ywy
                            };

                            db.hd_ls_detail.Add(HDLSMX);

                        }


                        //会员赠送记录
                        if (item.vtype == 1 || item.vtype == 9 || item.vtype == 3)
                        {
                            if (item.isZS)
                            {
                                ////存入领取记录
                                var lqinfo = new hd_vip_zs_history
                                {
                                    vipcode = CashiersFormXP.GetInstance.VipID,
                                    scode = HandoverModel.GetInstance.scode,
                                    zstime = System.DateTime.Now,
                                    item_id = item.noCode,
                                    tm = item.barCodeTM,
                                    cname = item.goods,
                                    zscount = item.countNum,
                                    price = item.hyPrice
                                };
                                db.hd_vip_zs_history.Add(lqinfo);
                            }

                        }

                    }

                    db.SaveChanges();


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
                            new SqlParameter("@vtype", 209), new SqlParameter("@hs_code", CashiersFormXP.GetInstance.VipID),
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

                    if (CEJStypeList.Count > 1)
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
             
                    //使用文本排版打印
                    PrintHelper print = new PrintHelper(goodList, vipJF, CETotalMoney, getMoney, jsdh, vipXF,payXF,LQXF, GiveChange, vipcard, dateStr);
                    print.StartPrint();

                    ////使用窗口打印
                    //PrintForm pr = new PrintForm(goodList, vipJF, CETotalMoney, getMoney, jsdh, jstype, GiveChange, vipcard);
                    ////pr.StartPrint();
                    //pr.ShowDialog();

                    //传递给重打小票
                    changed(vipJF, CETotalMoney, getMoney, jsdh, jstype, vipXF,payXF,LQXF, GiveChange, vipcard, dateStr);


            }

            //}
            //catch (Exception e)
            //{
            //    LogHelper.WriteLog("结算窗口保存上传客户订单时出现异常:", e);
            //    MessageBox.Show("数据库连接出错！");
            //}
        }


        #endregion

        private void ClosingEntries_Activated(object sender, EventArgs e)
        {
            //this.label16.Text = CETotalMoney.ToString() + " 元";  //本单合计
            //CE_label7.Text = CETotalMoney.ToString();  //应收
            //this.label19.Text = MoLing.ToString();  //已抹
        }

        private void ClosingEntries_FormClosing(object sender, FormClosingEventArgs e)
        {
            CEJStypeList.Clear();
            payCard = "";
            InitData();


            MLForm.changed -= MLForm_changed;
            vipform.VIPchanged -= vipform_VIPchanged;
            vipform.changed -= vipform_changed;
            czkform.changed -= czkform_changed;
            CPFrom.changed -= CPFrom_changed;
            CPFrom.changed -= CPFrom_changed;
            qkform.changed -= qkform_changed;
        }








    }
}
