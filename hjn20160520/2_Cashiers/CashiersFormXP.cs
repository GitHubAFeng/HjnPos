﻿using Common;
using ExcelData;
using hjn20160520._4_Detail;
using hjn20160520.Common;
using hjn20160520.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;

namespace hjn20160520._2_Cashiers
{

    /// <summary>
    /// 结算方式
    /// </summary>
    //public enum JSType
    //{
    //    Cash = 0, //现金
    //    UnionPay = 1, //银联卡
    //    Coupon = 2,  //购物劵
    //    Others = 3  //其它
    //}

    /// <summary>
    /// 主窗口状态类型
    /// </summary>
    public enum StateType
    {
        Cash,  //收银
        Back,  //退货
        Offline   //离线
    }

    /// <summary>
    /// 收银模块主逻辑
    /// </summary>
    public partial class CashiersFormXP : Form
    {
        //单例
        //public static CashiersFormXP GetInstance { get; private set; }
        MemberPointsForm MPForm = new MemberPointsForm(); //会员积分冲减窗口
        ClosingEntries CEform = new ClosingEntries();
        //ChoiceGoods solo9_PP = new ChoiceGoods(); //活动9的赠品选择
        SalesmanForm SMFormSMForm = new SalesmanForm(); //业务员录入窗口 
        //ChoiceGoods choice;  // 商品选择窗口
        GoodsNote GNform = new GoodsNote(); //挂单窗口
        MainFormXP mainForm;  //主菜单
        LockScreenForm LSForm;  //锁屏窗口
        RefundForm RDForm;  //退货窗口
        ChoiceGoods CGForm = new ChoiceGoods(); //商品选择窗口

        VipShopForm vipForm = new VipShopForm();//会员消费窗口
        ZKForm zkform = new ZKForm();
        ZKZDForm zkzdform = new ZKZDForm();
        VipMemoForm vipmemo = new VipMemoForm();  //会员备注消息

        //public bool isVipBirthday = false;
        ////用于其它窗口传值给本窗口控件 (VIP赠品信息)
        ////这是委托与事件的第一步  
        //public delegate void VIPZSHandle(int vipid );
        //public event VIPZSHandle changed;  

        public bool isLianXi { get; set; }  //是否练习模式
        //公共提示信息窗口
        TipForm tipForm;
        KeyboardHook kh;  //全局快捷键封装
        //记录购物车内的商品
        public BindingList<GoodsBuy> goodsBuyList = new BindingList<GoodsBuy>();
        //记录从数据库查到的商品
        //public BindingList<GoodsBuy> goodsChooseList = new BindingList<GoodsBuy>();
        //挂单取单窗口的挂单列表
        public BindingList<GoodsNoteModel> noteList = new BindingList<GoodsNoteModel>();
        //挂单窗口中订单号与订单商品清单对应的字典列表
        public Dictionary<int, BindingList<GoodsBuy>> noteDict = new Dictionary<int, BindingList<GoodsBuy>>();

        #region 收银属性

        //整单折扣率
        public decimal ZKZD { get; set; }
        //单品折扣率临时变量
        //public decimal? ZKDP_temp { get; set; }

        //单号，临时，以后要放上数据库读取
        public int OrderNo = 0;

        //标志一单交易,是否新单
        public bool isNewItem = false;

        //应收总金额
        public decimal totalMoney { get; set; }

        public bool isVipDate = false;  //是否会员日
        public decimal vipDateZkl = 0; //会员日折扣率
        public decimal vipDtaeJF = 0; //会员日积分倍数
        //进行消费的会员ID
        //public int VipID { get; set; }
        //进行消费的会员卡号
        //public string VipCARD { get; set; }

        public int lastvipid;  //记录上单会员id
        //会员备注消息
        public string VipMdemo { get; set; }
        //会员等级
        //public int viplv { get; set; }
        //会员名字
        //public string VipName { get; set; }

        //public PrintHelper printer;  //小票打印

        public BindingList<GoodsBuy> lastGoodsList = new BindingList<GoodsBuy>();  //上单购物清单

        #endregion
        public CashiersFormXP()
        {
            InitializeComponent();
        }

        private void CashiersFormXP_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                //删除DEL键
                case Keys.Delete:

                    Dele();

                    break;

                //F1 提款
                case Keys.F1:
                    this.textBox1.Focus();
                    this.textBox1.SelectAll();
                    break;

                case Keys.F2:

                    HandoverModel.GetInstance.isPrint = !HandoverModel.GetInstance.isPrint;
                    if (HandoverModel.GetInstance.isPrint)
                    {
                        label47.Text = "开";
                    }
                    else
                    {
                        label47.Text = "关";
                    }

                    break;

                //存货
                case Keys.F5:
                    VipSaveItemForm vipsave = new VipSaveItemForm();
                    vipsave.ShowDialog(this);
                    break;

                //取货
                case Keys.F6:
                    VipGetItemForm vipget = new VipGetItemForm();
                    vipget.ShowDialog(this);
                    break;

                //F3键登记业务员
                case Keys.F3:
                    //SalesmanForm SMFormSMForm = new SalesmanForm(); //业务员录入窗口 
                    //SMFormSMForm.changed += showYWYuiFunc;
                    //SMFormSMForm.ZDchanged += SMFormSMForm_ZDchanged;
                    SMFormSMForm.ShowDialog();
                    break;
                //F4键重打小票
                case Keys.F4:
                    //窗口打印
                    //PrintForm pr = new PrintForm(lastGoodsList, jf, ysje, ssje, jsdh, jstype, zhaoling);
                    //pr.ShowDialog();
                    //文本打印
                    PrintHelper ph = new PrintHelper(lastGoodsList, jf, ysje, ssje, jsdh, vipCradXF, payXF, LQXF, zhaoling, lastVipcard, dateStr, true);
                    ph.StartPrint();

                    break;

                //锁屏
                case Keys.Home:
                    //LSForm = new LockScreenForm();
                    LSForm.ShowDialog();
                    //this.Hide();
                    break;
                //打开会员查询窗口
                case Keys.F7:
                    VIPForm();
                    break;
                //查商品
                case Keys.F8:
                    ItemInfoForm iiform = new ItemInfoForm();
                    iiform.ShowDialog();
                    break;
                //退货
                case Keys.F9:
                    Refund();
                    break;
                //整单打折
                case Keys.F10:
                    //ZKZDForm zkzdform = new ZKZDForm();
                    //zkzdform.changed += zkzdform_changed;
                    zkzdform.ShowDialog();
                    //if (ZKZD != null)
                    //{
                    //    ZKZDFunc();
                    //}
                    break;
                //单品打折
                case Keys.F11:
                    //ZKForm zkform = new ZKForm();
                    //zkform.changed += zkform_changed;
                    zkform.ShowDialog();

                    //if (ZKDP_temp != null)
                    //{
                    //    ZKDPFunc();
                    //}
                    break;

                //打开会员卡窗口
                case Keys.F12:
                    //VipShopForm vipForm = new VipShopForm();//会员消费窗口
                    //vipForm.changed += showVIPuiFunc;
                    //vipForm.VIPchanged += vipForm_VIPchanged;
                    vipForm.ShowDialog();
                    break;

            }
            //中途提款ctrl+T
            if ((e.KeyCode == Keys.T) && e.Control)
            {
                TiKuanForm tikuanFrom = new TiKuanForm();
                tikuanFrom.ShowDialog();
            }

            //销售明细ctrl+S
            if ((e.KeyCode == Keys.S) && e.Control)
            {
                var xsmxform = new detailForm();
                xsmxform.ShowDialog();
            }
            //会员消息ctrl+L
            if ((e.KeyCode == Keys.L) && e.Control)
            {
                vipmemo.ShowDialog();
            }
            //会员图像ctrl+P
            if ((e.KeyCode == Keys.P) && e.Control)
            {
                ShowVipImaFunc();
            }
            ////导入会员图像ctrl+O
            //if ((e.KeyCode == Keys.O) && e.Control)
            //{
            //    VipPicWriteFunc();
            //}

        }

        private void CashiersFormXP_Load(object sender, EventArgs e)
        {
            //单例赋值
            //if (GetInstance == null) GetInstance = this;

            Init();
        }

        //初始化窗口
        private void Init()
        {
            // 窗口全屏设置全屏

            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            //this.TopMost = true;  //窗口顶置

            //会员日
            if (VipDateFunc() == false)
            {
                isVipDate = false;
                label2.Visible = false;
            }

            HandoverModel.GetInstance.isVipBirthday = false;
            HandoverModel.GetInstance.VipCard = string.Empty;
            HandoverModel.GetInstance.VipID = 0;
            HandoverModel.GetInstance.VipLv = 0;
            HandoverModel.GetInstance.VipName = string.Empty;


            //isVipBirthday = false;  //重置生日
            label4.Visible = false;
            //打印开关
            if (HandoverModel.GetInstance.isPrint)
            {
                label47.Text = "开";
            }
            else
            {
                label47.Text = "关";
            }

            if (isLianXi)
            {
                label25.Visible = true;
                label96.Text = "练习";
            }
            else
            {
                label25.Visible = false;
                label96.Text = "收银";
            }

            //时间开始
            timer1.Start();
            //窗口赋值

            //choice = new ChoiceGoods();
            //GNform = new GoodsNote();
            mainForm = new MainFormXP();
            //MPForm = new MemberPointsForm();
            tipForm = new TipForm();
            LSForm = new LockScreenForm();
            //CGForm = new ChoiceGoods();

            dataGridView_Cashiers.DataSource = goodsBuyList;

            //初始化购物车
            if (goodsBuyList.Count > 0)
            {
                goodsBuyList.Clear();
            }

            //合计等文本初始化，完工后直接设置为空
            label84.Text = "";
            label83.Text = "";
            label81.Text = "";
            label82.Text = "";
            label26.Text = HandoverModel.GetInstance.bcode.ToString();  //机号
            label100.Text = HandoverModel.GetInstance.userName;  //员工名字
            label6.Text = HandoverModel.GetInstance.scodeName;  //分店名字

            notifyIcon1.Visible = true;//默认图标不可见，托盘图标可见,以防出现多个托盘图标
            //全局快捷键
            kh = new KeyboardHook();
            kh.SetHook();
            kh.OnKeyDownEvent += kh_OnKeyDownEvent;

            CGForm.changed += CGForm_changed;

            //solo9_SP.changed += solo_changed;  //活动9
            //solo9_LB.changed += solo_changed;
            //solo9_PP.changed += solo_changed;
            cho5.changed += cho5_changed;

            //业务员事件
            SMFormSMForm.changed += showYWYuiFunc;
            SMFormSMForm.ZDchanged += SMFormSMForm_ZDchanged;
            //结算传递小票信息
            CEform.changed += CEform_changed;
            CEform.UIChanged += CEform_UIChanged;

            //会员
            vipForm.changed += showVIPuiFunc;
            //vipForm.VIPchanged += vipForm_VIPchanged;

            //打折
            zkform.changed += zkform_changed;
            zkzdform.changed += zkzdform_changed;

            //会员备注
            vipmemo.changed += vipmemo_changed;

            //挂单
            GNform.changed += GNform_changed;

        }

        //传递挂单
        void GNform_changed(int va, int inde)
        {
            GetNoteByorder(va);
            noteList.RemoveAt(inde);
        }

        /// <summary>
        /// 传递会员备注
        /// </summary>
        /// <param name="mo"></param>
        void vipmemo_changed(string mo)
        {
            VipMdemo = mo;
        }

        /// <summary>
        /// 处理结算后的UI更新
        /// </summary>
        void CEform_UIChanged(decimal getje, decimal toje, decimal zlje)
        {
            label85.Visible = true;
            label86.Visible = true;
            label87.Visible = true;
            label88.Visible = true;
            label92.Visible = true;
            label91.Visible = true;

            label87.Text = getje.ToString() + " 元";  //收款
            label88.Text = zlje.ToString() + " 元";  //找零
            //上单实收
            label92.Text = getje.ToString() + " 元";
            label91.Text = toje.ToString() + " 元";  //上单合计

            isNewItems(true);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label_timer.Text = " 当前时间：" + System.DateTime.Now.ToString();

        }

        void kh_OnKeyDownEvent(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Pause)
            {
                if (WindowState == FormWindowState.Minimized)
                {
                    notifyIcon1_MouseDoubleClick(null, null);
                }
                else
                {
                    this.WindowState = FormWindowState.Minimized;
                }
            }
        }


        /*换个思路处理优惠活动：
1、每次添加商品到购物车时 ， 先进行促销活动判断，判断结束后添加商品。
2、遍历购物车里所有商品，判断是否有优惠活动的商品，再判断是否满足优惠条件来进行优惠操作。*/

        #region 商品查询
        //根据条码通过EF进行模糊查询
        private void EFSelectByBarCode()
        {
            try
            {
                #region 查询操作
                int VipID = HandoverModel.GetInstance.VipID;

                string temptxt = textBox1.Text.Trim();
                if (string.IsNullOrEmpty(temptxt))
                {
                    tipForm.Tiplabel.Text = "请输入需要查找的商品条码!";
                    tipForm.ShowDialog();
                    return;
                }
                int itemid_temp = -1;
                int.TryParse(temptxt, out itemid_temp);

                using (hjnbhEntities db = new hjnbhEntities())
                {

                    var rules = db.hd_item_info.AsNoTracking().Where(t => t.tm.Contains(temptxt) || t.cname.Contains(temptxt) || t.item_id == itemid_temp)
                            .Select(t => new
                            {
                                noCode = t.item_id,
                                BarCode = t.tm,
                                Goods = t.cname,
                                unit = t.unit,
                                spec = t.spec,
                                retails = t.ls_price,
                                hyprice = t.hy_price,
                                JJprice = t.jj_price,
                                pinyin = t.py,
                                goodsDes = t.manufactory,
                                hpsize = t.hpack_size,
                                Status = t.status,
                                PFprice = t.pf_price
                            })
                        //.OrderBy(t => t.pinyin)

                            .ToList();

                    //如果查出数据不至一条就弹出选择窗口，否则直接显示出来

                    if (rules.Count == 0)
                    {

                        this.textBox1.SelectAll();
                        tipForm.Tiplabel.Text = "没有查找到该商品!";
                        tipForm.ShowDialog();
                        return;
                    }


                    #region 查到多条记录时
                    //查询到多条则弹出商品选择窗口，排除表格在正修改时发生判断
                    if (rules.Count > 1 && !dataGridView_Cashiers.IsCurrentCellInEditMode)
                    {
                        string tip_temp = Tipslabel.Text;
                        Tipslabel.Text = "商品正在查询中，请稍等！";

                        if (rules.Count > 10)
                        {

                            if (DialogResult.No == MessageBox.Show("查询到多个类似的商品，数据量较大时可能造成几秒的卡顿，是否继续查询？", "提醒", MessageBoxButtons.YesNo))
                            {
                                return;
                            }
                        }

                        foreach (var item in rules)
                        {
                            #region 商品单位查询
                            //需要把单位编号转换为中文以便UI显示
                            int unitID = item.unit.HasValue ? (int)item.unit : 1;
                            string dw = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == unitID).Select(t => t.txt1).FirstOrDefault();
                            #endregion

                            #region 商品类别与品牌查询
                            var ppbl = db.v_xs_item_info.AsNoTracking().Where(t => t.item_id == item.noCode).FirstOrDefault();

                            #endregion

                            CGForm.ChooseList.Add(new GoodsBuy
                            {
                                noCode = item.noCode,
                                barCodeTM = item.BarCode,
                                goods = item.Goods,
                                unit = unitID,
                                unitStr = dw,
                                spec = item.spec,
                                lsPrice = Math.Round(item.retails, 2),
                                pinYin = item.pinyin,
                                salesClerk = HandoverModel.GetInstance.YWYStr,
                                ywy = HandoverModel.GetInstance.YWYid,
                                goodsDes = item.goodsDes,
                                hpackSize = item.hpsize,
                                jjPrice = item.JJprice,
                                hyPrice = Math.Round(item.hyprice, 2),
                                status = item.Status,
                                pfPrice = item.PFprice,
                                isVip = VipID == 0 ? false : true,
                                PP = ppbl != null ? ppbl.pp : "",
                                LB = ppbl != null ? ppbl.lb_code : 0
                            });
                        }

                        CGForm.ShowDialog();
                        Tipslabel.Text = tip_temp;  //重置提示

                    }



                    #endregion
                    #region 只查到一条记录时

                    //只查到一条如果没有重复的就直接上屏，除非表格正在修改数量
                    if (rules.Count == 1 && !dataGridView_Cashiers.IsCurrentCellInEditMode)
                    {
                        //先判断该商品状态是否允许销售
                        if (rules[0].Status.Value == 2)
                        {
                            tipForm.Tiplabel.Text = "此商品目前处于停止销售状态！";
                            tipForm.ShowDialog();
                            return;
                        }

                        //选择商品时才去促销与优惠视图里找找该商品有没有搞活动

                        #region 按普通流程走

                        GoodsBuy newGoods_temp = new GoodsBuy();
                        foreach (var item in rules)
                        {
                            var ppbl = db.v_xs_item_info.AsNoTracking().Where(t => t.item_id == item.noCode).FirstOrDefault();
                            #region 商品单位查询
                            //需要把单位编号转换为中文以便UI显示
                            int unitID = item.unit.HasValue ? (int)item.unit : 1;
                            string dw = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == unitID).Select(t => t.txt1).FirstOrDefault();
                            #endregion
                            newGoods_temp = new GoodsBuy
                            {
                                noCode = item.noCode,
                                barCodeTM = item.BarCode,
                                goods = item.Goods,
                                unit = unitID,
                                unitStr = dw,
                                spec = item.spec,
                                lsPrice = Math.Round(item.retails, 2),
                                pinYin = item.pinyin,
                                salesClerk = HandoverModel.GetInstance.YWYStr,
                                ywy = HandoverModel.GetInstance.YWYid,
                                goodsDes = item.goodsDes,
                                hpackSize = item.hpsize,
                                jjPrice = item.JJprice,
                                hyPrice = Math.Round(item.hyprice, 2),
                                status = item.Status,
                                pfPrice = item.PFprice,
                                isVip = VipID == 0 ? false : true,
                                PP = ppbl != null ? ppbl.pp : "",
                                LB = ppbl != null ? ppbl.lb_code : 0
                            };

                        }

                        if (goodsBuyList.Count == 0)
                        {
                            goodsBuyList.Add(newGoods_temp);

                        }
                        else
                        {

                            //if (goodsBuyList.Any(n => n.noCode == newGoods_temp.noCode))
                            //{
                            var Has = goodsBuyList.Where(p => p.noCode == newGoods_temp.noCode).FirstOrDefault();
                            if (Has != null)
                            {
                                if (Has.isXG == false)
                                {
                                    Has.countNum++;
                                    dataGridView_Cashiers.Refresh();
                                }
                                else
                                {
                                    if (DialogResult.OK == MessageBox.Show("此单 " + Has.goods + " 超出限购的部分将不再享受活动优惠，是否确认购买？", "活动提醒", MessageBoxButtons.OKCancel))
                                    {
                                        //另起的这一组也要能数量叠加
                                        var reXG = goodsBuyList.Where(t => t.noCode == newGoods_temp.noCode && t.isXG == false).FirstOrDefault();
                                        if (reXG != null)
                                        {
                                            reXG.countNum++;
                                            dataGridView_Cashiers.Refresh();
                                        }
                                        else
                                        {
                                            goodsBuyList.Add(newGoods_temp);
                                        }

                                    }
                                }

                            }
                            else
                            {
                                goodsBuyList.Add(newGoods_temp);
                            }


                            //}
                            //else
                            //{
                            //    goodsBuyList.Add(newGoods_temp);
                            //}

                        }
                        #endregion


                    #endregion
                #endregion

                        //促销活动
                        //XSHDFunc(db);

                        //优惠活动
                        //YHHDFunc(db);

                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("收银主窗口查询商品时出现异常:", e);
                MessageBox.Show("查询商品时出现异常,请联系管理员！");

            }
        }
        #endregion


        /// <summary>
        /// 从商品选择窗口传递回的商品
        /// </summary>
        /// <param name="goods"></param>
        void CGForm_changed(GoodsBuy goods)
        {
            var re = goodsBuyList.Where(t => t.noCode == goods.noCode).FirstOrDefault();
            //如果存在还要判定是否数量限购封顶，如果封顶了再另起一组
            if (re != null)
            {
                if (re.isXG == false)
                {
                    re.countNum++;
                    dataGridView_Cashiers.Refresh();
                    //temptxt_choTM = goods.barCodeTM;  //传递给活动3 / 活动4 / 活动1 /活动9
                    //ischoose = true;
                }
                else
                {
                    if (DialogResult.OK == MessageBox.Show("此单 " + goods.goods + " 超出限购的部分将不再享受活动优惠，是否确认购买？", "活动提醒", MessageBoxButtons.OKCancel))
                    {
                        //另起的这一组也要能数量叠加
                        var reXG = goodsBuyList.Where(t => t.noCode == goods.noCode && t.isXG == false).FirstOrDefault();
                        if (reXG != null)
                        {
                            reXG.countNum++;
                            dataGridView_Cashiers.Refresh();
                        }
                        else
                        {
                            goodsBuyList.Add(goods);
                        }

                    }
                }

            }
            else
            {
                goodsBuyList.Add(goods);
                //temptxt_choTM = goods.barCodeTM;  //传递给活动3
            }

            //需要再判断当前购物车是否满足优惠活动条件
            using (var db = new hjnbhEntities())
            {

                XSHDFunc(db);  //处理促销
                //YHHDFunc(db);  //处理优惠
            }

            textBox1.Text = "";
        }




        //促销活动处理逻辑(如果在促销视图中找到商品就会调整会员价)
        public void XSHDFunc(hjnbhEntities db)
        {
            ////if (string.IsNullOrEmpty(VipID.ToString()) || VipID == 0) return;  //目前设置非会员不享受促销价
            ////暂时取消这个活动试试效果
            return;
            //该活动视图数据太乱

            try
            {
                int scode_temp = HandoverModel.GetInstance.scode;
                //foreach (var item in goodsBuyList)
                //{
                for (int i = 0; i < goodsBuyList.Count; i++)
                {
                    int tempid = goodsBuyList[i].noCode;

                    //判断优惠视图是否有此同码的活动，因以优惠为优先，如果有优惠的话就不再判断促销了
                    var YhInfo_ = db.v_yh_detail.AsNoTracking().Where(t => t.item_id == tempid && t.scode == scode_temp).FirstOrDefault();
                    if (YhInfo_ != null) continue;
                    //判断分店与货号是否符合活动条件
                    var xsinfo = db.v_xs_item_info.AsNoTracking().Where(t => t.item_id == tempid && t.scode == scode_temp).FirstOrDefault();
                    if (xsinfo != null)
                    {
                        if (!string.IsNullOrEmpty(xsinfo.hy_price))
                        {
                            decimal temp = Convert.ToDecimal(xsinfo.hy_price);

                            if (temp <= 0)
                            {
                                decimal temp_ls = Convert.ToDecimal(xsinfo.ls_price);
                                goodsBuyList[i].hyPrice = temp_ls;
                            }
                            else
                            {
                                goodsBuyList[i].hyPrice = temp;
                            }

                        }

                        goodsBuyList[i].goodsDes = xsinfo.memo;
                        //限购
                        if (xsinfo.xg_amount > 0)
                        {
                            if (goodsBuyList[i].countNum > xsinfo.xg_amount)
                            {
                                goodsBuyList[i].countNum = xsinfo.xg_amount;
                                MessageBox.Show(xsinfo.cname + "  已达最大限购数量！");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                LogHelper.WriteLog("收银主界面处理优惠活动时发生异常:", ex);
                MessageBox.Show("促销活动处理出错！");

            }
        }



        //优惠活动处理逻辑
        //bool isMEZS = false;   //用来判断满额加价购是否已经赠送过来赠品
        //string temptxt_choTM = string.Empty;   //用于防止重复的活动 当通过选择窗口添加商品时触发活动的条件
        //decimal addtemp3 = 1;  //活动3的赠品数量 
        //decimal addtemp9 = 1;  //活动9的赠品数量 
        //decimal addtemp1 = 1;  //活动1的赠品数量 
        //bool ischoose = false;  //活动1判断商品是不是从商品选择中添加
        public void YHHDFunc(hjnbhEntities db)
        {
            //try
            //{
            #region 处理优惠活动
            //int scode_te = HandoverModel.GetInstance.scode;
            //int VipID = HandoverModel.GetInstance.VipID;
            //int viplv = HandoverModel.GetInstance.VipLv;
            ////2遍历购物车中每个商品看是否有优惠活动的商品
            //for (int i = 0; i < goodsBuyList.Count; i++)
            //{

            //    //这种写法如果同时有两个活动商品时他就分不清了，导致商品活动重复判断
            //    int itemid = goodsBuyList[i].noCode;
            //    string itemTM = goodsBuyList[i].barCodeTM;

            //    //var vipLV = db.hd_vip_info.AsNoTracking().Where(t => t.vipcode == VipID).Select(t => t.viptype).FirstOrDefault();
            //    //判断分店与货号是否符合活动条件
            //    var YhInfo = db.v_yh_detail.AsNoTracking().Where(t => t.item_id == itemid && t.scode == scode_te).FirstOrDefault();
            //    //如果有优惠表中的商品则判断面向对象，是否会员专享
            //    if (YhInfo != null)
            //    {
            //        //判断活动时间 (时间改由后台自行判定)
            //        //if (System.DateTime.Now > YhInfo.sendtime) continue;
            //        //查询会员等级,如果为空则不是会员消费
            //        //var vipLV = db.hd_vip_info.AsNoTracking().Where(t => t.vipcode == VipID).Select(t => t.viptype).FirstOrDefault();

            //        ////特定对象
            //        //if ((int)YhInfo.dx_type == 0 || (int)YhInfo.dx_type == 1)
            //        //{
            //        //再判断商品的优惠类型
            //        switch (YhInfo.vtype)
            //        {
            //            //case 1:
            //                ////限赠活动
            //                #region 活动视图里只有赠品(0806,不好用)
            //                ////加了这一段就相当于只有输入条码才能判断活动
            //                //string temptxt_1 = textBox1.Text.Trim();
            //                //if (string.IsNullOrEmpty(temptxt_1)) temptxt_1 = temptxt_choTM;
            //                //int itemid_temp_1 = -1;
            //                //int.TryParse(temptxt_1, out itemid_temp_1);
            //                //if (goodsBuyList[i].noCode == itemid_temp_1 || goodsBuyList[i].barCodeTM == temptxt_1)
            //                //{
            //                //    goodsBuyList[i].vtype = 0;
            //                //}
            //                //if (goodsBuyList[i].vtype == 1) continue;

            //                //if (YhInfo.dx_type == 1)  //限定会员
            //                //{
            //                //    if (VipID == 0) continue;
            //                //    int viplvInt = vipLV.HasValue ? (int)vipLV.Value : 0;
            //                //    int viplvInfo = YhInfo.viptype;
            //                //    //如果会员等级条件满足
            //                //    if (viplvInt >= viplvInfo)
            //                //    {
            //                //        //查询该会员赠品领取记录
            //                //        var zshis = db.hd_vip_zs_history.Where(e => e.vipcode == VipID && e.item_id == itemid && e.zstime > YhInfo.sbegintime && e.zstime < YhInfo.sendtime).ToList();
            //                //        if (zshis.Count > 0)
            //                //        {
            //                //            //有领取记录
            //                //            decimal numed = 0.00m; //已经领取的数量
            //                //            foreach (var item in zshis)
            //                //            {
            //                //                decimal temp = item.zscount.HasValue ? item.zscount.Value : 0;
            //                //                numed += temp;
            //                //            }
            //                //            if (numed >= YhInfo.xg_amount) continue;

            //                //            //还可以领取的数量
            //                //            decimal numtemp = YhInfo.xg_amount - numed;

            //                //            //是否有赠品在购物车里
            //                //            var zsitem = goodsBuyList.Where(t => t.noCode == goodsBuyList[i].noCode && goodsBuyList[i].vtype == 1 && goodsBuyList[i].isZS).FirstOrDefault();
            //                //            if (zsitem != null)
            //                //            {
            //                //                //有的话，如果不超过限购就数量++好了
            //                //                if (zsitem.countNum < numtemp)
            //                //                {
            //                //                    zsitem.countNum++;
            //                //                }
            //                //                else
            //                //                {
            //                //                    zsitem.isXG = true;
            //                //                }
            //                //            }
            //                //            else
            //                //            {
            //                //                //没有



            //                //                //if (!goodsBuyList[i].isZS)
            //                //                //{

            //                //                if (goodsBuyList[i].countNum < numtemp)
            //                //                {

            //                //                    //验证是否加价了
            //                //                    if (YhInfo.ls_price.Value > 0)
            //                //                    {
            //                //                        if (DialogResult.OK == MessageBox.Show("此赠品" + YhInfo.cname + "价值" + Math.Round(YhInfo.ls_price.Value, 2) + "元，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.OKCancel))
            //                //                        {
            //                //                            goodsBuyList[i].hyPrice = Math.Round(YhInfo.ls_price.Value, 2);  //捆绑商品的特价
            //                //                            goodsBuyList[i].goodsDes = YhInfo.memo; //备注
            //                //                            goodsBuyList[i].isVip = true;
            //                //                            goodsBuyList[i].vtype = 1;
            //                //                            goodsBuyList[i].isZS = true;
            //                //                        }
            //                //                    }
            //                //                    else
            //                //                    {
            //                //                        if (DialogResult.OK == MessageBox.Show("此赠品" + YhInfo.cname + "为免费赠送，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.OKCancel))
            //                //                        {
            //                //                            goodsBuyList[i].hyPrice = 0.00m;
            //                //                            goodsBuyList[i].goodsDes = YhInfo.memo; //备注
            //                //                            goodsBuyList[i].isVip = true;
            //                //                            goodsBuyList[i].vtype = 1;
            //                //                            goodsBuyList[i].isZS = true;

            //                //                        }
            //                //                    }
            //                //                }
            //                //                else if (goodsBuyList[i].countNum == numtemp)
            //                //                {
            //                //                    //验证是否加价了
            //                //                    if (YhInfo.ls_price.Value > 0)
            //                //                    {
            //                //                        if (DialogResult.OK == MessageBox.Show("此赠品" + YhInfo.cname + "价值" + Math.Round(YhInfo.ls_price.Value, 2) + "元，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.OKCancel))
            //                //                        {
            //                //                            goodsBuyList[i].hyPrice = Math.Round(YhInfo.ls_price.Value, 2);  //捆绑商品的特价
            //                //                            goodsBuyList[i].goodsDes = YhInfo.memo; //备注
            //                //                            goodsBuyList[i].isVip = true;
            //                //                            goodsBuyList[i].vtype = 1;
            //                //                            goodsBuyList[i].isZS = true;
            //                //                            goodsBuyList[i].isXG = true;
            //                //                        }
            //                //                    }
            //                //                    else
            //                //                    {
            //                //                        if (DialogResult.OK == MessageBox.Show("此赠品" + YhInfo.cname + "为免费赠送，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.OKCancel))
            //                //                        {
            //                //                            goodsBuyList[i].hyPrice = 0.00m;
            //                //                            goodsBuyList[i].goodsDes = YhInfo.memo; //备注
            //                //                            goodsBuyList[i].isVip = true;
            //                //                            goodsBuyList[i].vtype = 1;
            //                //                            goodsBuyList[i].isZS = true;
            //                //                            goodsBuyList[i].isXG = true;
            //                //                        }
            //                //                    }
            //                //                }
            //                //                else
            //                //                {
            //                //                    goodsBuyList[i].countNum = Convert.ToInt32(numtemp);
            //                //                    goodsBuyList[i].isXG = true;
            //                //                }

            //                //            }

            //                //        }
            //                //        else
            //                //        {
            //                //            //没有领取记录
            //                //            //商品不能超过限赠数量
            //                //            //是否有赠品在购物车里
            //                //            var zsitem = goodsBuyList.Where(t => t.noCode == goodsBuyList[i].noCode && goodsBuyList[i].vtype == 1 && goodsBuyList[i].isZS).FirstOrDefault();
            //                //            if (zsitem != null)
            //                //            {
            //                //                //有的话，如果不超过限购就数量++好了
            //                //                if (zsitem.countNum < YhInfo.xg_amount)
            //                //                {
            //                //                    zsitem.countNum++;
            //                //                }
            //                //                else
            //                //                {
            //                //                    zsitem.isXG = true;
            //                //                }
            //                //            }
            //                //            else
            //                //            {
            //                //                //没有
            //                //                if (goodsBuyList[i].countNum < YhInfo.xg_amount)
            //                //                {

            //                //                    //验证是否加价了
            //                //                    if (YhInfo.ls_price.Value > 0)
            //                //                    {
            //                //                        if (DialogResult.OK == MessageBox.Show("此赠品" + YhInfo.cname + "价值" + Math.Round(YhInfo.ls_price.Value, 2) + "元，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.OKCancel))
            //                //                        {
            //                //                            goodsBuyList[i].hyPrice = Math.Round(YhInfo.ls_price.Value, 2);  //捆绑商品的特价
            //                //                            goodsBuyList[i].goodsDes = YhInfo.memo; //备注
            //                //                            goodsBuyList[i].isVip = true;
            //                //                            goodsBuyList[i].vtype = 1;
            //                //                            goodsBuyList[i].isZS = true;
            //                //                        }
            //                //                    }
            //                //                    else
            //                //                    {
            //                //                        if (DialogResult.OK == MessageBox.Show("此赠品" + YhInfo.cname + "为免费赠送，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.OKCancel))
            //                //                        {
            //                //                            goodsBuyList[i].hyPrice = 0.00m;
            //                //                            goodsBuyList[i].goodsDes = YhInfo.memo; //备注
            //                //                            goodsBuyList[i].isVip = true;
            //                //                            goodsBuyList[i].vtype = 1;
            //                //                            goodsBuyList[i].isZS = true;

            //                //                        }
            //                //                    }
            //                //                }
            //                //                else if (goodsBuyList[i].countNum == YhInfo.xg_amount)
            //                //                {
            //                //                    //验证是否加价了
            //                //                    if (YhInfo.ls_price.Value > 0)
            //                //                    {
            //                //                        if (DialogResult.OK == MessageBox.Show("此赠品" + YhInfo.cname + "价值" + Math.Round(YhInfo.ls_price.Value, 2) + "元，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.OKCancel))
            //                //                        {
            //                //                            goodsBuyList[i].hyPrice = Math.Round(YhInfo.ls_price.Value, 2);  //捆绑商品的特价
            //                //                            goodsBuyList[i].goodsDes = YhInfo.memo; //备注
            //                //                            goodsBuyList[i].isVip = true;
            //                //                            goodsBuyList[i].vtype = 1;
            //                //                            goodsBuyList[i].isZS = true;
            //                //                            goodsBuyList[i].isXG = true;
            //                //                        }
            //                //                    }
            //                //                    else
            //                //                    {
            //                //                        if (DialogResult.OK == MessageBox.Show("此赠品" + YhInfo.cname + "为免费赠送，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.OKCancel))
            //                //                        {
            //                //                            goodsBuyList[i].hyPrice = 0.00m;
            //                //                            goodsBuyList[i].goodsDes = YhInfo.memo; //备注
            //                //                            goodsBuyList[i].isVip = true;
            //                //                            goodsBuyList[i].vtype = 1;
            //                //                            goodsBuyList[i].isZS = true;
            //                //                            goodsBuyList[i].isXG = true;
            //                //                        }
            //                //                    }
            //                //                }
            //                //                else
            //                //                {
            //                //                    goodsBuyList[i].countNum = Convert.ToInt32(YhInfo.xg_amount);
            //                //                    goodsBuyList[i].isXG = true;
            //                //                }

            //                //            }

            //                //        }
            //                //        }

            //                //    }



            //                #endregion

            //                #region 0804重构

            //                ////加了这一段就相当于只有输入条码才能判断活动
            //                //string temptxt_1 = textBox1.Text.Trim();
            //                //if (string.IsNullOrEmpty(temptxt_1)) temptxt_1 = temptxt_choTM;
            //                //int itemid_temp_1 = -1;
            //                //int.TryParse(temptxt_1, out itemid_temp_1);
            //                //if (goodsBuyList[i].noCode == itemid_temp_1 || goodsBuyList[i].barCodeTM == temptxt_1)
            //                //{
            //                //    goodsBuyList[i].vtype = 0;
            //                //}

            //                //if (goodsBuyList[i].vtype == 1) continue;

            //                //if (YhInfo.dx_type == 1)  //限定会员
            //                //{
            //                //    if (VipID == 0) continue;
            //                //    int viplvInt = vipLV.HasValue ? (int)vipLV.Value : 0;
            //                //    int viplvInfo = YhInfo.viptype;
            //                //    //如果会员等级条件满足
            //                //    if (viplvInt >= viplvInfo)
            //                //    {
            //                //        // 查询该会员赠品领取记录,因为这活动是送一个的，所以一条记录一个赠品
            //                //        var zshis = db.hd_vip_zs_history.Where(e => e.vipcode == VipID && e.item_id == YhInfo.item_id && e.zstime > YhInfo.sbegintime && e.zstime < YhInfo.sendtime).ToList();
            //                //        if (zshis.Count > 0)
            //                //        {
            //                //            //有赠送记录的情况
            //                //            decimal count_temp_1 = zshis.Count + goodsBuyList[i].countNum;
            //                //            //是否达到限购
            //                //            if (count_temp_1 < YhInfo.xg_amount)
            //                //            {

            //                //                //是否已经存在该赠品
            //                //                var zs = goodsBuyList.Where(t => t.vtype == 1 && t.isZS && t.noCode == YhInfo.item_id).FirstOrDefault();
            //                //                if (zs != null)
            //                //                {
            //                //                    decimal temp = zs.countNum + 1;
            //                //                    if (temp < YhInfo.xg_amount)
            //                //                    {
            //                //                        zs.countNum++;
            //                //                        continue;
            //                //                    }
            //                //                    else
            //                //                    {
            //                //                        zs.isXG = true;
            //                //                        continue;
            //                //                    }

            //                //                }

            //                //                if (DialogResult.OK == MessageBox.Show("此商品 " + goodsBuyList[i].goods + " 满足限量赠送活动（免费），是否确认参加此次活动？", "活动提醒", MessageBoxButtons.OKCancel))
            //                //                {
            //                //                    //那么开始更改价格
            //                //                    goodsBuyList[i].hyPrice = 0.00m;
            //                //                    goodsBuyList[i].lsPrice = Math.Round(YhInfo.yls_price, 2);
            //                //                    goodsBuyList[i].isVip = true;
            //                //                    goodsBuyList[i].isZS = true;
            //                //                    goodsBuyList[i].vtype = 1;
            //                //                }

            //                //            }

            //                //        }
            //                //        else
            //                //        {
            //                //            //没有送过的情况
            //                //            //小于限量
            //                //            if (goodsBuyList[i].countNum < YhInfo.xg_amount)
            //                //            {

            //                //                if (DialogResult.OK == MessageBox.Show("此商品 " + goodsBuyList[i].goods + " 满足限量赠送活动（免费），是否确认参加此次活动？", "活动提醒", MessageBoxButtons.OKCancel))
            //                //                {
            //                //                    //那么开始更改价格
            //                //                    goodsBuyList[i].hyPrice = 0.00m;
            //                //                    goodsBuyList[i].lsPrice = Math.Round(YhInfo.yls_price, 2);
            //                //                    goodsBuyList[i].isVip = true;
            //                //                    goodsBuyList[i].isZS = true;
            //                //                    goodsBuyList[i].vtype = 1;

            //                //                }
            //                //            }
            //                //                //大于限量不再参与活动
            //                //            else
            //                //            {
            //                //                goodsBuyList[i].vtype = 1;
            //                //                goodsBuyList[i].isXG = true;
            //                //            }
            //                //        }

            //                //    }
            //                //}



            //                #endregion

            //                #region 活动视图里只有赠品
            //                ////防止重复判断， 需要 在商品选择事件中添加活动类型传值(意思是只有全新的商品才会去判定)
            //                //string temptxt_1 = textBox1.Text.Trim();
            //                //if (string.IsNullOrEmpty(temptxt_1) || ischoose) temptxt_1 = temptxt_choTM;
            //                //int itemid_temp_1 = -1;
            //                //int.TryParse(temptxt_1, out itemid_temp_1);
            //                //if (goodsBuyList[i].barCodeTM != temptxt_1)
            //                //{
            //                //    //goodsBuyList[i].vtype = 0;
            //                //    continue;
            //                //}

            //                ////if (goodsBuyList[i].vtype == 1) continue;

            //                //if (YhInfo.dx_type == 1)  //限定会员
            //                //{
            //                //    if (VipID == 0) continue;
            //                //    //int viplvInt = vipLV.HasValue ? (int)vipLV.Value : 0;
            //                //    int viplvInfo = YhInfo.viptype;
            //                //    //如果会员等级条件满足
            //                //    if (viplv >= viplvInfo)
            //                //    {
            //                //        //购物车中的赠品
            //                //        var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.item_id).FirstOrDefault();
            //                //        if (zsitem == null) continue;
            //                //        //if(zsitem.countNum >= YhInfo.xg_amount)continue;
            //                //        //查询该会员赠品领取记录
            //                //        var zshis = db.hd_vip_zs_history.Where(e => e.vipcode == VipID && e.item_id == YhInfo.item_id && e.zstime > YhInfo.sbegintime && e.zstime < YhInfo.sendtime).ToList();
            //                //        if (zshis.Count > 0)
            //                //        {
            //                //            //有领取记录
            //                //            decimal numed = 0.00m; //已经领取的数量
            //                //            foreach (var item in zshis)
            //                //            {
            //                //                decimal temp = item.zscount.HasValue ? item.zscount.Value : 0;
            //                //                numed += temp;
            //                //            }
            //                //            if (numed >= YhInfo.xg_amount) continue;

            //                //            //还可以领取的数量
            //                //            decimal numtemp = YhInfo.xg_amount - numed;
            //                //            if (zsitem.vtype != 1)
            //                //            {

            //                //                if (zsitem.countNum <= numtemp)
            //                //                {

            //                //                    //验证是否加价了
            //                //                    if (YhInfo.ls_price.Value > 0)
            //                //                    {
            //                //                        if (DialogResult.OK == MessageBox.Show("此赠品" + YhInfo.cname + "价值" + Math.Round(YhInfo.ls_price.Value, 2) + "元，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.OKCancel))
            //                //                        {
            //                //                            zsitem.hyPrice = Math.Round(YhInfo.ls_price.HasValue ? YhInfo.ls_price.Value : (decimal)zsitem.hyPrice, 2);  //捆绑商品的特价
            //                //                            zsitem.goodsDes = YhInfo.memo; //备注
            //                //                            zsitem.pfPrice = Math.Round(YhInfo.yls_price, 2);
            //                //                            zsitem.isVip = true;
            //                //                            zsitem.vtype = 1;
            //                //                            zsitem.isZS = true;
            //                //                            zsitem.isXG = true;
            //                //                            break;
            //                //                        }
            //                //                    }
            //                //                    else
            //                //                    {
            //                //                        if (DialogResult.OK == MessageBox.Show("此赠品" + YhInfo.cname + "为免费赠送，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.OKCancel))
            //                //                        {
            //                //                            zsitem.hyPrice = 0.00m;
            //                //                            zsitem.pfPrice = Math.Round(YhInfo.yls_price, 2);
            //                //                            zsitem.goodsDes = YhInfo.memo; //备注
            //                //                            zsitem.isVip = true;
            //                //                            zsitem.vtype = 1;
            //                //                            zsitem.isZS = true;
            //                //                            zsitem.isXG = true;
            //                //                            break;
            //                //                        }
            //                //                    }
            //                //                }
            //                //                else
            //                //                {
            //                //                    zsitem.countNum = Convert.ToInt32(numtemp);
            //                //                    zsitem.isXG = true;
            //                //                }

            //                //            }

            //                //        }
            //                //        else
            //                //        {

            //                //            //没有领取记录
            //                //            //商品不能超过限赠数量
            //                //            if (zsitem.countNum >= YhInfo.xg_amount)
            //                //            {
            //                //                //当限购为1的情况
            //                //                if (YhInfo.xg_amount == 1)
            //                //                {
            //                //                    ////这样就不判断数量了……
            //                //                    //if (DialogResult.OK == MessageBox.Show("此赠品" + YhInfo.cname + "为免费赠送，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.OKCancel))
            //                //                    //{
            //                //                    zsitem.hyPrice = 0.00m;
            //                //                    zsitem.pfPrice = Math.Round(YhInfo.yls_price, 2);
            //                //                    zsitem.goodsDes = YhInfo.memo; //备注
            //                //                    zsitem.isVip = true;
            //                //                    zsitem.vtype = 1;
            //                //                    zsitem.isZS = true;
            //                //                    //zsitem.isXG = true;
            //                //                    dataGridView_Cashiers.Refresh();
            //                //                    //break;
            //                //                    //}
            //                //                }
            //                //                zsitem.isXG = true;
            //                //                continue;
            //                //            }
            //                //            else
            //                //            {
            //                //                //验证是否加价了
            //                //                if (YhInfo.ls_price.Value > 0)
            //                //                {
            //                //                    if (DialogResult.OK == MessageBox.Show("此赠品" + YhInfo.cname + "价值" + Math.Round(YhInfo.ls_price.Value, 2) + "元，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.OKCancel))
            //                //                    {
            //                //                        zsitem.hyPrice = Math.Round(YhInfo.ls_price.HasValue ? YhInfo.ls_price.Value : (decimal)zsitem.hyPrice, 2);  //捆绑商品的特价
            //                //                        zsitem.goodsDes = YhInfo.memo; //备注
            //                //                        zsitem.pfPrice = Math.Round(YhInfo.yls_price, 2);
            //                //                        zsitem.isVip = true;
            //                //                        zsitem.vtype = 1;
            //                //                        zsitem.isZS = true;
            //                //                        //zsitem.isXG = true;
            //                //                        break;
            //                //                    }
            //                //                }
            //                //                else
            //                //                {
            //                //                    if (DialogResult.OK == MessageBox.Show("此赠品" + YhInfo.cname + "为免费赠送，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.OKCancel))
            //                //                    {
            //                //                        zsitem.hyPrice = 0.00m;
            //                //                        zsitem.pfPrice = Math.Round(YhInfo.yls_price, 2);
            //                //                        zsitem.goodsDes = YhInfo.memo; //备注
            //                //                        zsitem.isVip = true;
            //                //                        zsitem.vtype = 1;
            //                //                        zsitem.isZS = true;
            //                //                        //zsitem.isXG = true;
            //                //                        break;
            //                //                    }
            //                //                }
            //                //            }

            //                //        }

            //                //    }
            //                //}

            //                //ischoose = false;  //重置

            //                #endregion

            //                //break;
            //            //case 2:
            //                //零售特价（不限购） , ----现在又要改为限购了，与7的区别是后台可以选择是否限购,但在前台没区别
            //                #region 目前0727新代码逻辑 , 限购

            //                //if (YhInfo.dx_type == 1)  //限定会员
            //                //{
            //                //    if (VipID == 0) continue;
            //                //    int viplvInfo = YhInfo.viptype;
            //                //    //如果会员等级条件满足
            //                //    if (viplv >= viplvInfo)
            //                //    {
            //                //        //购物车已经存在捆绑商品就修改组合活动的特价
            //                //        var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.item_id || t.barCodeTM == YhInfo.tm).FirstOrDefault();

            //                //        if (zsitem != null)
            //                //        {
            //                //            if (YhInfo.xg_amount > 0)
            //                //            {
            //                //                decimal temp2 = YhInfo.xg_amount;
            //                //                //限购条件
            //                //                if (zsitem.countNum < YhInfo.xg_amount)
            //                //                {
            //                //                    zsitem.hyPrice = Math.Round(YhInfo.ls_price.HasValue ? YhInfo.ls_price.Value : (decimal)zsitem.hyPrice, 2);  //捆绑商品的特价
            //                //                    zsitem.goodsDes = YhInfo.memo; //备注
            //                //                    zsitem.pfPrice = Math.Round(YhInfo.yls_price, 2);
            //                //                    zsitem.isVip = true;
            //                //                    zsitem.vtype = 2;

            //                //                }
            //                //                else if (zsitem.countNum == temp2)
            //                //                {
            //                //                    zsitem.hyPrice = Math.Round(YhInfo.ls_price.HasValue ? YhInfo.ls_price.Value : (decimal)zsitem.hyPrice, 2);  //捆绑商品的特价
            //                //                    zsitem.goodsDes = YhInfo.memo; //备注
            //                //                    zsitem.pfPrice = Math.Round(YhInfo.yls_price, 2);
            //                //                    zsitem.isVip = true;
            //                //                    zsitem.vtype = 2;
            //                //                    zsitem.isXG = true;
            //                //                }
            //                //                else
            //                //                {
            //                //                    //超出部分将按原价售卖
            //                //                    zsitem.isXG = true;
            //                //                    //商品另起一组
            //                //                    //商品查询会判定isXG
            //                //                }
            //                //            }
            //                //            else
            //                //            {
            //                //                zsitem.hyPrice = Math.Round(YhInfo.ls_price.HasValue ? YhInfo.ls_price.Value : (decimal)zsitem.hyPrice, 2);  //捆绑商品的特价
            //                //                zsitem.goodsDes = YhInfo.memo; //备注
            //                //                zsitem.pfPrice = Math.Round(YhInfo.yls_price, 2);
            //                //                zsitem.isVip = true;
            //                //                zsitem.vtype = 2;

            //                //            }
            //                //        }

            //                //    }
            //                //}
            //                //else if (YhInfo.dx_type == 0)   //所有对象
            //                //{
            //                //    //已经存在就数量++
            //                //    var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.item_id || t.barCodeTM == YhInfo.tm).FirstOrDefault();
            //                //    if (zsitem != null)
            //                //    {
            //                //        if (YhInfo.xg_amount > 0)
            //                //        {
            //                //            decimal temp2 = YhInfo.xg_amount;
            //                //            //限购条件
            //                //            if (zsitem.countNum < temp2)
            //                //            {
            //                //                zsitem.hyPrice = Math.Round(YhInfo.ls_price.HasValue ? YhInfo.ls_price.Value : (decimal)zsitem.hyPrice, 2);  //捆绑商品的特价
            //                //                zsitem.lsPrice = Math.Round(YhInfo.ls_price.HasValue ? YhInfo.ls_price.Value : (decimal)zsitem.lsPrice, 2);  //都是特价
            //                //                zsitem.goodsDes = YhInfo.memo; //备注
            //                //                zsitem.pfPrice = Math.Round(YhInfo.yls_price, 2);
            //                //                zsitem.vtype = 2;

            //                //            }
            //                //            else if (zsitem.countNum == temp2)
            //                //            {
            //                //                zsitem.hyPrice = Math.Round(YhInfo.ls_price.HasValue ? YhInfo.ls_price.Value : (decimal)zsitem.hyPrice, 2);  //捆绑商品的特价
            //                //                zsitem.lsPrice = Math.Round(YhInfo.ls_price.HasValue ? YhInfo.ls_price.Value : (decimal)zsitem.lsPrice, 2);  //都是特价
            //                //                zsitem.goodsDes = YhInfo.memo; //备注
            //                //                zsitem.pfPrice = Math.Round(YhInfo.yls_price, 2);
            //                //                zsitem.vtype = 2;
            //                //                zsitem.isXG = true;

            //                //            }
            //                //            else
            //                //            {
            //                //                //超出部分将按原价售卖
            //                //                zsitem.isXG = true;
            //                //                //商品另起一组
            //                //                //商品查询会判定isXG
            //                //            }
            //                //        }
            //                //        else
            //                //        {
            //                //            zsitem.hyPrice = Math.Round(YhInfo.ls_price.HasValue ? YhInfo.ls_price.Value : (decimal)zsitem.hyPrice, 2);  //捆绑商品的特价
            //                //            zsitem.lsPrice = Math.Round(YhInfo.ls_price.HasValue ? YhInfo.ls_price.Value : (decimal)zsitem.lsPrice, 2);  //都是特价
            //                //            zsitem.goodsDes = YhInfo.memo; //备注
            //                //            zsitem.pfPrice = Math.Round(YhInfo.yls_price, 2);
            //                //            zsitem.vtype = 2;

            //                //        }

            //                //    }
            //                //}

            //                #endregion

            //                //break;
            //            //case 3:
            //            #region 买1送1  (赠品的加价已经加在零售上了，所以赠品都是0元的)
            //            ////买1送1
            //            ////判定目前数量是否满足活动
            //            //if (goodsBuyList[i].countNum < YhInfo.amount) continue;

            //            //string temptxt_3 = textBox1.Text.Trim();
            //            //if (string.IsNullOrEmpty(temptxt_3)) temptxt_3 = temptxt_choTM;
            //            //int itemid_temp_ = -1;
            //            //int.TryParse(temptxt_3, out itemid_temp_);
            //            //if (goodsBuyList[i].noCode == itemid_temp_ || goodsBuyList[i].barCodeTM == temptxt_3)
            //            //{
            //            //    goodsBuyList[i].vtype = 0;
            //            //}
            //            //if (goodsBuyList[i].vtype == 3) continue;

            //            //if (YhInfo.xg_amount > 0)
            //            //{
            //            //    //限购条件
            //            //    if (goodsBuyList[i].countNum > YhInfo.xg_amount)
            //            //    {
            //            //        MessageBox.Show("该活动商品数量已经达到了最大限购额！");
            //            //        continue;
            //            //    }
            //            //}


            //            //#region 0729更新

            //            ////查赠品
            //            //var ZSITEM_3 = db.v_yh_detail.AsNoTracking().Where(e => e.tm == temptxt_3 && e.scode == scode_te && e.vtype == 3).ToList();
            //            //if (ZSITEM_3.Count > 0)
            //            //{
            //            //    //查赠品数量比例
            //            //    if (goodsBuyList[i].countNum % (int)YhInfo.amount == 0)
            //            //    {
            //            //        int numtemp = goodsBuyList[i].countNum / (int)YhInfo.amount;  //比例系数
            //            //        addtemp3 = Convert.ToInt32(YhInfo.zs_amount * numtemp);  //应该送的赠品数量

            //            //        if (DialogResult.Cancel == MessageBox.Show("此单满足买一送一活动，是否确认参加此次活动？（"
            //            //            + goodsBuyList[i].goods + "的单价将被修正为" + Math.Round(YhInfo.ls_price.Value, 2).ToString() + "元,并赠送" + YhInfo.zs_cname
            //            //            , "活动提醒", MessageBoxButtons.OKCancel))
            //            //        {
            //            //            continue;
            //            //        }
            //            //    }
            //            //    else
            //            //    {
            //            //        //MessageBox.Show("不满足条件");
            //            //        continue;
            //            //    }

            //            //    var cho3 = new ChoiceGoods();
            //            //    var cho3VIP = new ChoiceGoods();

            //            //    foreach (var item in ZSITEM_3)
            //            //    {
            //            //        #region 商品单位查询
            //            //        int zsunit5 = 0;
            //            //        var zsinfo100 = db.hd_item_info.AsNoTracking().Where(t => t.item_id == YhInfo.zs_item_id || t.tm == YhInfo.zstm).Select(t => t.unit).FirstOrDefault();
            //            //        if (zsinfo100 != null)
            //            //        {
            //            //            zsunit5 = zsinfo100.HasValue ? (int)zsinfo100.Value : 1;
            //            //        }
            //            //        //需要把单位编号转换为中文以便UI显示
            //            //        string dw_5 = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == zsunit5).Select(t => t.txt1).FirstOrDefault();
            //            //        #endregion

            //            //        if (item.dx_type == 1)  //限定会员
            //            //        {
            //            //            if (VipID == 0) continue;

            //            //            //int viplvInt = vipLV.HasValue ? (int)vipLV.Value : 0;
            //            //            int viplvInfo = item.viptype;
            //            //            //如果会员等级条件满足
            //            //            if (viplv >= viplvInfo)
            //            //            {
            //            //                cho3VIP.ChooseList.Add(new GoodsBuy
            //            //                {
            //            //                    unit = zsunit5,
            //            //                    unitStr = dw_5,
            //            //                    noCode = item.zs_item_id,
            //            //                    barCodeTM = item.zstm,
            //            //                    goods = item.zs_cname,
            //            //                    countNum = Convert.ToInt32(item.zs_amount.Value),
            //            //                    lsPrice = Math.Round(YhInfo.zs_yprice.Value, 2),
            //            //                    hyPrice = 0.00m,
            //            //                    goodsDes = item.memo,
            //            //                    isVip = true,
            //            //                    isZS = true,
            //            //                    isXG = true,
            //            //                    vtype =3
            //            //                });
            //            //                goodsBuyList[i].isXG = true;
            //            //                goodsBuyList[i].goodsDes = item.memo;
            //            //                goodsBuyList[i].vtype = 3;  //标志为活动商品
            //            //                goodsBuyList[i].hyPrice = Math.Round(YhInfo.ls_price.Value / YhInfo.amount, 2);   //活动商品的价格要改为活动价
            //            //                //goodsBuyList[i].lsPrice = Math.Round(YhInfo.ls_price.Value, 2);   //活动商品的价格要改为活动价
            //            //            }
            //            //        }
            //            //        //所有对象
            //            //        else if (YhInfo.dx_type == 0)
            //            //        {

            //            //            cho3.ChooseList.Add(new GoodsBuy
            //            //            {
            //            //                unit = zsunit5,
            //            //                unitStr = dw_5,
            //            //                noCode = item.zs_item_id,
            //            //                barCodeTM = item.zstm,
            //            //                goods = item.zs_cname,
            //            //                countNum = Convert.ToInt32(item.zs_amount.Value),
            //            //                lsPrice = 0.00m,
            //            //                hyPrice = 0.00m,
            //            //                goodsDes = item.memo,
            //            //                isZS = true,
            //            //                isXG = true,
            //            //                vtype=3
            //            //            });
            //            //            goodsBuyList[i].isXG = true;
            //            //            goodsBuyList[i].goodsDes = item.memo;
            //            //            goodsBuyList[i].vtype = 3;
            //            //            goodsBuyList[i].hyPrice = Math.Round(YhInfo.ls_price.Value / YhInfo.amount, 2);   //活动商品的价格要改为活动价
            //            //            goodsBuyList[i].lsPrice = Math.Round(YhInfo.ls_price.Value / YhInfo.amount, 2);   //活动商品的价格要改为活动价
            //            //        }
            //            //    }
            //            //    if (cho3VIP.ChooseList.Count > 0)
            //            //    {
            //            //        cho3VIP.changed += cho3VIP_changed;
            //            //        cho3VIP.ShowDialog();
            //            //    }
            //            //    else
            //            //    {
            //            //        cho3.changed += cho3_changed;
            //            //        cho3.ShowDialog();
            //            //    }
            //            //}
            //            //#endregion

            //            /////////////////////////////////////////////////////////////////////////////////////////////////
            //            #region 旧的

            //            /*
            //                        if (YhInfo.dx_type == 1)  //限定会员
            //                        {
            //                            int viplvInt = vipLV.HasValue ? (int)vipLV.Value : 0;
            //                            int viplvInfo = YhInfo.viptype.HasValue ? YhInfo.viptype.Value : 0;
            //                            //如果会员等级条件满足
            //                            if (viplvInt >= viplvInfo)
            //                            {

            //                                var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.zs_item_id || t.barCodeTM == YhInfo.zstm).FirstOrDefault();

            //                                //已经存在就数量++
            //                                if (zsitem != null)
            //                                {
            //                                    int addtemp = 1;

            //                                    if (YhInfo.xg_amount > 0)
            //                                    {
            //                                        //限购条件
            //                                        if (goodsBuyList[i].countNum < YhInfo.xg_amount)
            //                                        {
            //                                            if (goodsBuyList[i].countNum % (int)YhInfo.amount == 0)
            //                                            {
            //                                                int numtemp = goodsBuyList[i].countNum / (int)YhInfo.amount;
            //                                                addtemp = Convert.ToInt32(YhInfo.zs_amount * numtemp);
            //                                            }

            //                                            //赠品的限制数量
            //                                            if (zsitem.countNum < addtemp)
            //                                            {
            //                                                int temp_ = Convert.ToInt32(YhInfo.zs_amount);   //一次赠送的数量
            //                                                if (temp_ == 0) temp_ = 1;
            //                                                int counttemp = zsitem.countNum + temp_;    //原有数量与赠送数量相加后
            //                                                zsitem.countNum += temp_;
            //                                                zsitem.hyPrice = 0;
            //                                                zsitem.lsPrice = 0;
            //                                                zsitem.goodsDes = YhInfo.memo;
            //                                                zsitem.isZS = true;
            //                                                zsitem.isXG = true;
            //                                                zsitem.isVip = true;
            //                                                zsitem.vtype = 3;
            //                                                goodsBuyList[i].isVip = true;
            //                                                goodsBuyList[i].vtype = 3;
            //                                                goodsBuyList[i].hyPrice = Math.Round(YhInfo.ls_price.Value, 2);   //活动商品的价格要改为活动价
            //                                                goodsBuyList[i].lsPrice = Math.Round(YhInfo.ls_price.Value, 2);   //活动商品的价格要改为活动价
            //                                                goodsBuyList[i].goodsDes = YhInfo.memo;
            //                                            }
            //                                        }
            //                                        else
            //                                        {
            //                                            goodsBuyList[i].vtype = 3;
            //                                            goodsBuyList[i].isXG = true;
            //                                            goodsBuyList[i].isVip = true;
            //                                        }
            //                                    }
            //                                    //不限购的情况
            //                                    else
            //                                    {
            //                                        if (goodsBuyList[i].countNum % (int)YhInfo.amount == 0)
            //                                        {
            //                                            int numtemp = goodsBuyList[i].countNum / (int)YhInfo.amount;
            //                                            addtemp = Convert.ToInt32(YhInfo.zs_amount * numtemp);
            //                                        }

            //                                        //目前赠品的数量少于限制数量时
            //                                        if (zsitem.countNum < addtemp)
            //                                        {
            //                                            if (zsitem.isXG)
            //                                            {
            //                                                int temp_ = Convert.ToInt32(YhInfo.zs_amount);   //一次赠送的数量
            //                                                if (temp_ == 0) temp_ = 1;
            //                                                int counttemp = zsitem.countNum + temp_;    //原有数量与赠送数量相加后
            //                                                //那么数量++
            //                                                zsitem.countNum += temp_;
            //                                                zsitem.hyPrice = 0;
            //                                                zsitem.lsPrice = 0;
            //                                                zsitem.goodsDes = YhInfo.memo;
            //                                                zsitem.isZS = true;
            //                                                zsitem.isXG = true;
            //                                                zsitem.isVip = true;
            //                                                zsitem.vtype = 3;
            //                                                goodsBuyList[i].isVip = true;
            //                                                goodsBuyList[i].vtype = 3;
            //                                                goodsBuyList[i].goodsDes = YhInfo.memo;
            //                                                goodsBuyList[i].hyPrice = Math.Round(YhInfo.ls_price.Value, 2);   //活动商品的价格要改为活动价
            //                                                goodsBuyList[i].lsPrice = Math.Round(YhInfo.ls_price.Value, 2);   //活动商品的价格要改为活动价
            //                                            }

            //                                        }
            //                                        //    //分拆商品组，另起一组按原价出售
            //                                        //else
            //                                        //{

            //                                        //}
            //                                    }

            //                                }
            //                                else
            //                                {
            //                                    //赠送的商品
            //                                    var ZsGoods = new GoodsBuy
            //                                    {
            //                                        barCodeTM = YhInfo.zstm,
            //                                        noCode = YhInfo.zs_item_id,
            //                                        goods = YhInfo.zs_cname,
            //                                        unitStr = dw,
            //                                        countNum = Convert.ToInt32(YhInfo.zs_amount),
            //                                        jjPrice = YhInfo.yjj_price,
            //                                        lsPrice = 0,
            //                                        hyPrice = 0,
            //                                        goodsDes = YhInfo.memo,
            //                                        isVip = true,
            //                                        isZS = true,
            //                                        isXG = true,
            //                                        vtype = 3
            //                                    };
            //                                    goodsBuyList.Add(ZsGoods);
            //                                }
            //                                goodsBuyList[i].hyPrice = Math.Round(YhInfo.ls_price.Value, 2);   //活动商品的价格要改为活动价
            //                                goodsBuyList[i].lsPrice = Math.Round(YhInfo.ls_price.Value, 2);   //活动商品的价格要改为活动价
            //                                goodsBuyList[i].isVip = true;
            //                                goodsBuyList[i].vtype = 3;
            //                                goodsBuyList[i].goodsDes = YhInfo.memo;
            //                            }
            //                        }
            //                        else if (YhInfo.dx_type == 0)   //所有对象
            //                        {
            //                            var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.zs_item_id || t.barCodeTM == YhInfo.zstm).FirstOrDefault();
            //                            //var zsitemZS = goodsBuyList.Where(t => t.noCode == YhInfo.zs_item_id && t.isZS).FirstOrDefault();
            //                            //已经存在就数量++
            //                            if (zsitem != null)
            //                            {
            //                                int addtemp = 1;   //应该送的赠品的数量

            //                                if (YhInfo.xg_amount > 0)
            //                                {
            //                                    //限购条件
            //                                    if (goodsBuyList[i].countNum < YhInfo.xg_amount)
            //                                    {
            //                                        if (goodsBuyList[i].countNum % (int)YhInfo.amount == 0)
            //                                        {
            //                                            int numtemp = goodsBuyList[i].countNum / (int)YhInfo.amount;  //比例系数
            //                                            addtemp = Convert.ToInt32(YhInfo.zs_amount * numtemp);  //赠品的正确数量
            //                                        }

            //                                        //赠品的限制数量
            //                                        if (zsitem.countNum < addtemp)
            //                                        {
            //                                            int temp_ = Convert.ToInt32(YhInfo.zs_amount);   //一次赠送的数量
            //                                            if (temp_ == 0) temp_ = 1;
            //                                            int counttemp = zsitem.countNum + temp_;    //原有数量与赠送数量相加后
            //                                            zsitem.countNum += temp_;
            //                                            zsitem.hyPrice = 0;
            //                                            zsitem.lsPrice = 0;
            //                                            zsitem.goodsDes = YhInfo.memo;
            //                                            zsitem.isZS = true;
            //                                            zsitem.isXG = true;
            //                                            //zsitem.isVip = true;
            //                                            zsitem.vtype = 3;
            //                                            //goodsBuyList[i].isVip = true;
            //                                            goodsBuyList[i].vtype = 3;
            //                                            goodsBuyList[i].goodsDes = YhInfo.memo;
            //                                            goodsBuyList[i].hyPrice = Math.Round(YhInfo.ls_price.Value, 2);   //活动商品的价格要改为活动价
            //                                            goodsBuyList[i].lsPrice = Math.Round(YhInfo.ls_price.Value, 2);   //活动商品的价格要改为活动价

            //                                        }
            //                                    }
            //                                    else
            //                                    {
            //                                        goodsBuyList[i].vtype = 3;
            //                                        goodsBuyList[i].isXG = true;
            //                                    }
            //                                }
            //                                //不限购的情况
            //                                else
            //                                {
            //                                    //活动商品与赠品的数量比例
            //                                    if (goodsBuyList[i].countNum % (int)YhInfo.amount == 0)
            //                                    {
            //                                        int numtemp = goodsBuyList[i].countNum / (int)YhInfo.amount;
            //                                        addtemp = Convert.ToInt32(YhInfo.zs_amount * numtemp);
            //                                    }

            //                                    int temp_ = Convert.ToInt32(YhInfo.zs_amount);   //一次赠送的数量
            //                                    if (temp_ == 0) temp_ = 1;
            //                                    int counttemp = zsitem.countNum + temp_;    //原有数量与赠送数量相加后
            //                                    //赠品的限制数量
            //                                    if (zsitem.countNum < addtemp)  //赠品少则++
            //                                    {

            //                                        zsitem.countNum += temp_;
            //                                        zsitem.hyPrice = 0;
            //                                        zsitem.lsPrice = 0;
            //                                        zsitem.goodsDes = YhInfo.memo;
            //                                        zsitem.isZS = true;
            //                                        zsitem.isXG = true;
            //                                        //zsitem.isVip = true;
            //                                        zsitem.vtype = 3;
            //                                        //goodsBuyList[i].isVip = true;
            //                                        goodsBuyList[i].vtype = 3;
            //                                        goodsBuyList[i].goodsDes = YhInfo.memo;
            //                                        goodsBuyList[i].hyPrice = Math.Round(YhInfo.ls_price.Value, 2);   //活动商品的价格要改为活动价
            //                                        goodsBuyList[i].lsPrice = Math.Round(YhInfo.ls_price.Value, 2);   //活动商品的价格要改为活动价
            //                                    }

            //                                    #region 拆分
            //                                    //先判断活动商品与赠品的数量比例
            //                                    //分为正确比例、赠品比商品多、赠品比商品少   3种情况
            //                                    //if (goodsBuyList[i].countNum > addtemp)  //商品比赠品多
            //                                    //{

            //                                    //}

            //                                    #endregion
            //                                    #region 拆分赠品组的方法（目前没有好思路）

            //                                    ////活动商品与赠品的数量比例
            //                                    //if (goodsBuyList[i].countNum % (int)YhInfo.amount == 0)
            //                                    //{
            //                                    //    int numtemp = goodsBuyList[i].countNum / (int)YhInfo.amount;
            //                                    //    addtemp = Convert.ToInt32(YhInfo.zs_amount * numtemp);
            //                                    //}

            //                                    //int temp_ = Convert.ToInt32(YhInfo.zs_amount);   //一次赠送的数量
            //                                    //if (temp_ == 0) temp_ = 1;

            //                                    ////已经存在赠品组
            //                                    //if (zsitemZS != null)
            //                                    //{
            //                                    //    //int counttemp = zsitem.countNum + temp_;    //原有数量与赠送数量相加后
            //                                    //    //赠品的限制数量
            //                                    //    if (zsitemZS.countNum < addtemp)
            //                                    //    {
            //                                    //        zsitemZS.countNum += temp_;
            //                                    //        zsitemZS.hyPrice = 0;
            //                                    //        zsitemZS.lsPrice = 0;
            //                                    //        zsitemZS.goodsDes = YhInfo.memo;
            //                                    //        zsitemZS.isZS = true;
            //                                    //        zsitemZS.isXG = true;
            //                                    //        //zsitem.isVip = true;
            //                                    //        zsitemZS.vtype = 3;
            //                                    //        //goodsBuyList[i].isVip = true;
            //                                    //        goodsBuyList[i].vtype = 3;
            //                                    //        goodsBuyList[i].hyPrice = YhInfo.ls_price;   //活动商品的价格要改为活动价
            //                                    //        goodsBuyList[i].lsPrice = YhInfo.ls_price;   //活动商品的价格要改为活动价
            //                                    //    }
            //                                    //}
            //                                    ////没有赠品组
            //                                    //else
            //                                    //{
            //                                    //    //分拆的数量
            //                                    //    int fxcount = zsitem.countNum - addtemp;
            //                                    //    if (zsitem.countNum < addtemp)
            //                                    //    {
            //                                    //        zsitem.countNum += temp_;
            //                                    //        zsitem.hyPrice = 0;
            //                                    //        zsitem.lsPrice = 0;
            //                                    //        zsitem.goodsDes = YhInfo.memo;
            //                                    //        zsitem.isZS = true;
            //                                    //        zsitem.isXG = true;
            //                                    //        //zsitem.isVip = true;
            //                                    //        zsitem.vtype = 3;
            //                                    //        //goodsBuyList[i].isVip = true;
            //                                    //        goodsBuyList[i].vtype = 3;
            //                                    //        goodsBuyList[i].hyPrice = YhInfo.ls_price;   //活动商品的价格要改为活动价
            //                                    //        goodsBuyList[i].lsPrice = YhInfo.ls_price;   //活动商品的价格要改为活动价
            //                                    //    }


            //                                    //}



            //                                    //分拆商品组，另起一组按原价出售

            //                                    //else
            //                                    //{
            //                                    //    if (DialogResult.OK == MessageBox.Show("此单 " + zsitem.goods + " 超出限购的部分将不再享受活动优惠，是否确认购买？", "活动提醒", MessageBoxButtons.OKCancel))
            //                                    //    {
            //                                    //    //分拆的数量
            //                                    //    int fxcount = zsitem.countNum - addtemp;
            //                                    //    zsitem.countNum = temp_;
            //                                    //    zsitem.isXG = true;

            //                                    //    var lqitem = new GoodsBuy
            //                                    //    {
            //                                    //        barCodeTM = YhInfo.zstm,
            //                                    //        noCode = YhInfo.zs_item_id,
            //                                    //        goods = YhInfo.zs_cname,
            //                                    //        unitStr = dw,
            //                                    //        countNum = fxcount,
            //                                    //        jjPrice = YhInfo.yjj_price,
            //                                    //        lsPrice =Math.Round( YhInfo.yls_price,2),
            //                                    //        hyPrice = Math.Round( zshy,2),
            //                                    //        goodsDes = YhInfo.memo,

            //                                    //    };
            //                                    //    var Has_ = goodsBuyList.Where(p => p.noCode == lqitem.noCode).FirstOrDefault();
            //                                    //    if (Has_ != null)
            //                                    //    {
            //                                    //        if (Has_.isXG == false)
            //                                    //        {
            //                                    //            Has_.countNum += fxcount;
            //                                    //            dataGridView_Cashiers.Refresh();
            //                                    //        }
            //                                    //        else
            //                                    //        {

            //                                    //                //另起的这一组也要能数量叠加
            //                                    //                var reXG = goodsBuyList.Where(t => t.noCode == lqitem.noCode && t.isXG == false).FirstOrDefault();
            //                                    //                if (reXG != null)
            //                                    //                {
            //                                    //                    reXG.countNum += fxcount;
            //                                    //                    dataGridView_Cashiers.Refresh();
            //                                    //                }
            //                                    //                else
            //                                    //                {
            //                                    //                    goodsBuyList.Add(lqitem);
            //                                    //                }

            //                                    //        }

            //                                    //    }
            //                                    //    else
            //                                    //    {
            //                                    //        goodsBuyList.Add(lqitem);
            //                                    //    }
            //                                    //}
            //                                    //}
            //                                    #endregion

            //                                }

            //                            }
            //                            else
            //                            {
            //                                //赠送的商品
            //                                var ZsGoods = new GoodsBuy
            //                                {
            //                                    barCodeTM = YhInfo.zstm,
            //                                    noCode = YhInfo.zs_item_id,
            //                                    goods = YhInfo.zs_cname,
            //                                    unitStr = dw,
            //                                    countNum = Convert.ToInt32(YhInfo.zs_amount),
            //                                    jjPrice = YhInfo.yjj_price,
            //                                    lsPrice = 0,
            //                                    hyPrice = 0,
            //                                    goodsDes = YhInfo.memo,
            //                                    isZS = true,
            //                                    isXG = true,
            //                                    vtype = 3
            //                                };
            //                                goodsBuyList.Add(ZsGoods);

            //                            }
            //                            goodsBuyList[i].hyPrice = Math.Round(YhInfo.ls_price.Value, 2);   //活动商品的价格要改为活动价
            //                            goodsBuyList[i].lsPrice = Math.Round(YhInfo.ls_price.Value, 2);   //活动商品的价格要改为活动价
            //                            //goodsBuyList[i].isVip = true;
            //                            goodsBuyList[i].vtype = 3;
            //                            goodsBuyList[i].goodsDes = YhInfo.memo;

            //                        }
            //                        #endregion
            //                        */
            //            //}
            //            #endregion
            //#endregion
            //            //break;

            //            //case 4:
            //            #region 组合优惠,两种商品同时存在则产生优惠，同时修改为特价。 (不要了，有新的)

            //            //if (YhInfo.dx_type == 1)  //限定会员
            //            //{
            //            //    int viplvInt = vipLV.HasValue ? (int)vipLV.Value : 0;
            //            //    int viplvInfo = YhInfo.viptype.HasValue ? YhInfo.viptype.Value : 0;
            //            //    //如果会员等级条件满足
            //            //    if (viplvInt >= viplvInfo)
            //            //    {
            //            //        //购物车已经存在捆绑商品就修改组合活动的特价
            //            //        var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.zs_item_id).FirstOrDefault();
            //            //        if (zsitem != null)
            //            //        {
            //            //            //限购数量
            //            //            if (zsitem.countNum <= YhInfo.xg_amount)
            //            //            {
            //            //                zsitem.hyPrice = YhInfo.ls_price;  //捆绑商品的特价
            //            //                goodsBuyList[i].hyPrice = YhInfo.ls_price;  //组合商品的特价
            //            //                zsitem.isVip = true;
            //            //                goodsBuyList[i].isVip = true;
            //            //            }

            //            //        }
            //            //    }
            //            //}
            //            //else if (YhInfo.dx_type == 0)   //所有对象
            //            //{
            //            //    //已经存在就数量++
            //            //    var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.zs_item_id).FirstOrDefault();
            //            //    if (zsitem != null)
            //            //    {
            //            //        //限购数量
            //            //        if (zsitem.countNum <= YhInfo.xg_amount)
            //            //        {
            //            //            zsitem.hyPrice = YhInfo.ls_price;  //捆绑商品的特价
            //            //            zsitem.lsPrice = YhInfo.ls_price;  //都是特价
            //            //            goodsBuyList[i].hyPrice = YhInfo.ls_price;  //组合商品的特价
            //            //            goodsBuyList[i].lsPrice = YhInfo.ls_price;
            //            //        }
            //            //    }
            //            //}

            //            #endregion

            //            #region 目前0727新代码逻辑,购买数量决定优惠程度的(看了后台是没有限购设置的)


            //            ////判定目前数量是否满足活动
            //            //if (goodsBuyList[i].countNum < YhInfo.amount) continue;
            //            ////if (YhInfo.xg_amount > 0)
            //            ////{
            //            ////    if (goodsBuyList[i].countNum > YhInfo.xg_amount)
            //            ////    {
            //            ////        MessageBox.Show(goodsBuyList[i].goods + " 该商品已达最大限购额！");
            //            ////        continue;
            //            ////    }
            //            ////}

            //            //string temptxt_4 = textBox1.Text.Trim();
            //            //if (string.IsNullOrEmpty(temptxt_4)) temptxt_4 = temptxt_choTM;
            //            //int itemid_temp_4 = -1;
            //            //int.TryParse(temptxt_4, out itemid_temp_4);
            //            //if (goodsBuyList[i].noCode == itemid_temp_4 && goodsBuyList[i].barCodeTM == temptxt_4 && goodsBuyList[i].vtype != 4)
            //            //{
            //            //    goodsBuyList[i].vtype = 0;
            //            //}
            //            //if (goodsBuyList[i].vtype == 4) continue;

            //            ////购物车已经存在捆绑商品就修改组合活动的特价(这里是判定捆绑的商品)
            //            //var zsitem4 = goodsBuyList.Where(t => t.noCode == YhInfo.zs_item_id && t.barCodeTM == YhInfo.zstm && t.vtype != 4).FirstOrDefault();
            //            //if (zsitem4 == null) continue;

            //            ////正比例增长
            //            //decimal temp_4 = goodsBuyList[i].countNum * YhInfo.zs_amount.Value;
            //            //decimal tempppp_4 = zsitem4.countNum * YhInfo.amount;

            //            //if (temp_4 != tempppp_4)
            //            //{
            //            //    continue;
            //            //}

            //            //if (goodsBuyList[i].countNum % YhInfo.zs_amount.Value != 0 && zsitem4.countNum % YhInfo.amount != 0) continue;

            //            //if (YhInfo.dx_type == 1)  //限定会员
            //            //{
            //            //    if (VipID == 0) continue;

            //            //    //int viplvInt = vipLV.HasValue ? (int)vipLV.Value : 0;
            //            //    int viplvInfo = YhInfo.viptype;
            //            //    //如果会员等级条件满足
            //            //    if (viplv >= viplvInfo)
            //            //    {
            //            //        if (DialogResult.OK == MessageBox.Show(goodsBuyList[i].goods + " 与 " + zsitem4.goods + " 满足组合优惠活动(组合价：" + Math.Round(YhInfo.ls_price.Value, 2) + " 元，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.OKCancel))
            //            //        {
            //            //            zsitem4.hyPrice = 0.00m;
            //            //            goodsBuyList[i].hyPrice = Math.Round(YhInfo.ls_price.Value / YhInfo.amount, 2);  //商品的特价
            //            //            zsitem4.goodsDes = YhInfo.memo; //备注
            //            //            zsitem4.isXG = true;
            //            //            zsitem4.isVip = true;
            //            //            goodsBuyList[i].isVip = true;
            //            //            zsitem4.vtype = 4;
            //            //            goodsBuyList[i].vtype = 4;
            //            //            goodsBuyList[i].goodsDes = YhInfo.memo;
            //            //            goodsBuyList[i].isXG = true;
            //            //        }

            //            //    }
            //            //}
            //            //else if (YhInfo.dx_type == 0)   //所有对象
            //            //{
            //            //    if (DialogResult.OK == MessageBox.Show(goodsBuyList[i].goods + " 与 " + zsitem4.goods + " 满足组合优惠活动(组合价：" + Math.Round(YhInfo.ls_price.Value, 2) + " 元，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.OKCancel))
            //            //    {
            //            //        zsitem4.hyPrice = 0.00m;
            //            //        zsitem4.lsPrice = 0.00m;
            //            //        goodsBuyList[i].hyPrice = Math.Round(YhInfo.ls_price.Value / YhInfo.amount, 2);  //商品的特价
            //            //        goodsBuyList[i].lsPrice = Math.Round(YhInfo.ls_price.Value / YhInfo.amount, 2);  //商品的特价
            //            //        zsitem4.goodsDes = YhInfo.memo; //备注
            //            //        zsitem4.vtype = 4;
            //            //        goodsBuyList[i].vtype = 4;
            //            //        goodsBuyList[i].goodsDes = YhInfo.memo;
            //            //        zsitem4.isXG = true;
            //            //        goodsBuyList[i].isXG = true;
            //            //    }

            //            //}
            //            //dataGridView_Cashiers.Refresh();
            //            #endregion

            //            //break;
            //            #region 活动5不在此范围判断
            //            /*
            //                    case 5:
            //                        //if (goodsBuyList[i].isZS) continue;   //这个条件是防止重复判断活动的
            //                        if (isMEZS) continue;   //这个条件是防止重复判断活动的
            //                        #region 主要判断合计金额是否满额就行，不要判断买了什么


            //                        #region 商品单位查询、会员价
            //                        int zsunit100 = 0;
            //                        //赠品的默认会员价
            //                        //decimal zshy_100 = 0;
            //                        //var zsinfo100 = db.hd_item_info.AsNoTracking().Where(t => t.item_id == YhInfo.zs_item_id || t.tm == YhInfo.zstm).Select(t => new { t.unit, t.hy_price }).ToList();
            //                        var zsinfo100 = db.hd_item_info.AsNoTracking().Where(t => t.item_id == YhInfo.zs_item_id || t.tm == YhInfo.zstm).Select(t => t.unit).FirstOrDefault();
            //                        if (zsinfo100 != null)
            //                        {
            //                            //foreach (var item in zsinfo100)
            //                            //{
            //                            //    zsunit100 = item.unit.HasValue ? (int)item.unit : 1;
            //                            //    zshy_100 = item.hy_price;
            //                            //}
            //                            zsunit100 = zsinfo100.HasValue ? (int)zsinfo100 : 1;
            //                        }
            //                        //需要把单位编号转换为中文以便UI显示
            //                        string dw_100 = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == zsunit100).Select(t => t.txt1).FirstOrDefault();
            //                        #endregion

            //                        if (YhInfo.dx_type == 1)  //限定会员
            //                        {
            //                            int viplvInt = vipLV.HasValue ? (int)vipLV.Value : 0;
            //                            int viplvInfo = YhInfo.viptype.HasValue ? YhInfo.viptype.Value : 0;
            //                            //如果会员等级条件满足
            //                            if (viplvInt >= viplvInfo)
            //                            {
            //                                decimal sum_temp = totalMoney.HasValue ? totalMoney.Value : 0;  //目前总额
            //                                //活动商品列表
            //                                var YHInfoList = db.v_yh_detail.AsNoTracking().Where(t => t.item_id == itemid || t.tm == itemTM).ToList();
            //                                if (YHInfoList.Count == 1)
            //                                {
            //                                    //是否满额条件
            //                                    if (sum_temp >= YhInfo.zjmoney)
            //                                    {
            //                                        if (YhInfo.zsmoney > 0)
            //                                        {
            //                                            if (DialogResult.OK == MessageBox.Show("此单 " + goodsBuyList[i].goods + " 满足满额加价赠送活动，是否确认参加此次活动？（将加价" + Math.Round(YhInfo.zsmoney.Value, 2).ToString() + "元）", "活动提醒", MessageBoxButtons.OKCancel))
            //                                            {


            //                                                //已经存在就数量++
            //                                                var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.zs_item_id || t.barCodeTM == YhInfo.zstm).FirstOrDefault();
            //                                                if (zsitem != null)
            //                                                {
            //                                                    int temp_ = Convert.ToInt32(YhInfo.zs_amount);  //赠送的数量
            //                                                    if (temp_ == 0) temp_ = 1;
            //                                                    if (DialogResult.OK == MessageBox.Show("此单 " + goodsBuyList[i].goods + " 满足满额加价赠送活动，是否确认修正商品价格？（" + Math.Round(YhInfo.zsmoney.Value, 2).ToString() + "元）", "活动提醒", MessageBoxButtons.OKCancel))
            //                                                    {
            //                                                        if (temp_ != 1)
            //                                                        {
            //                                                            zsitem.countNum = temp_;
            //                                                        }
            //                                                        zsitem.hyPrice = Math.Round(YhInfo.zsmoney.Value, 2);
            //                                                        zsitem.goodsDes = YhInfo.memo;
            //                                                        zsitem.isVip = true;
            //                                                        zsitem.isZS = true;
            //                                                        zsitem.isXG = true;
            //                                                        zsitem.vtype = 5;
            //                                                        isMEZS = true;
            //                                                    }
            //                                                    //goodsBuyList[i].isZS = true;
            //                                                    //goodsBuyList[i].isVip = true;
            //                                                    //goodsBuyList[i].vtype = 5;
            //                                                }
            //                                                else
            //                                                {
            //                                                    int temp_count = Convert.ToInt32(YhInfo.zs_amount);
            //                                                    if (temp_count == 0) temp_count = 1;

            //                                                    //赠送的商品
            //                                                    var JJZsGoods = new GoodsBuy
            //                                                    {
            //                                                        unitStr = dw_100,
            //                                                        barCodeTM = YhInfo.zstm,
            //                                                        noCode = YhInfo.zs_item_id,
            //                                                        goods = YhInfo.zs_cname,
            //                                                        countNum = temp_count,
            //                                                        jjPrice = YhInfo.yjj_price,
            //                                                        lsPrice = Math.Round(YhInfo.yls_price, 2),
            //                                                        hyPrice = Math.Round(YhInfo.zsmoney.Value, 2),
            //                                                        goodsDes = YhInfo.memo,
            //                                                        isZS = true,
            //                                                        isVip = true,
            //                                                        isXG = true,
            //                                                        vtype = 5
            //                                                    };
            //                                                    if (DialogResult.OK == MessageBox.Show("此单 " + goodsBuyList[i].goods + " 满足满额加价赠送活动，是否自动添加赠送商品（" + Math.Round(YhInfo.zsmoney.Value, 2).ToString() + "元）", "活动提醒", MessageBoxButtons.OKCancel))
            //                                                    {
            //                                                        //MessageBox.Show("你点击了确定");
            //                                                        goodsBuyList.Add(JJZsGoods);
            //                                                        isMEZS = true;

            //                                                    }
            //                                                }
            //                                            }
            //                                        }
            //                                        //商品活动设置不止一个时
            //                                        if (YHInfoList.Count > 1)
            //                                        {
            //                                            if (DialogResult.OK == MessageBox.Show("此单 " + goodsBuyList[i].goods + " 满足满额加价赠送活动，是否选择商品？（" + Math.Round(YhInfo.zsmoney.Value, 2).ToString() + "元）", "活动提醒", MessageBoxButtons.OKCancel))
            //                                            {
            //                                                //MessageBox.Show("你点击了确定");
            //                                                //列出所有活动赠送商品
            //                                                foreach (var item in YHInfoList)
            //                                                {
            //                                                    int temp_count_ = Convert.ToInt32(item.zs_amount);
            //                                                    if (temp_count_ == 0) temp_count_ = 1;
            //                                                    CGForm.ChooseList.Add(new GoodsBuy
            //                                                    {
            //                                                        unitStr = dw_100,
            //                                                        barCodeTM = YhInfo.zstm,
            //                                                        noCode = YhInfo.zs_item_id,
            //                                                        goods = YhInfo.zs_cname,
            //                                                        countNum = temp_count_,
            //                                                        jjPrice = YhInfo.yjj_price,
            //                                                        lsPrice = Math.Round(YhInfo.yls_price, 2),
            //                                                        hyPrice = Math.Round(YhInfo.zsmoney.Value, 2),
            //                                                        goodsDes = YhInfo.memo,
            //                                                        isZS = true,
            //                                                        isVip = true,
            //                                                        isXG = true,
            //                                                        vtype = 5
            //                                                    });

            //                                                    CGForm.ShowDialog();
            //                                                    isMEZS = true;
            //                                                }

            //                                            }
            //                                        }
            //                                    }
            //                                }
            //                            }
            //                        }
            //                        else if (YhInfo.dx_type == 0)   //所有对象
            //                        {
            //                            //活动商品列表
            //                            var YHInfoList = db.v_yh_detail.AsNoTracking().Where(t => t.item_id == itemid || t.tm == itemTM).ToList();
            //                            if (YHInfoList.Count == 1)
            //                            {
            //                                decimal sum_temp = totalMoney.HasValue ? totalMoney.Value : 0;  //目前总额
            //                                if (sum_temp >= YhInfo.zjmoney)
            //                                {
            //                                    if (YhInfo.zsmoney > 0)
            //                                    {
            //                                        if (DialogResult.OK == MessageBox.Show("此单 " + goodsBuyList[i].goods + " 满足满额加价赠送活动，是否确认参加此次活动？（将加价" + Math.Round(YhInfo.zsmoney.Value, 2).ToString() + "元）", "活动提醒", MessageBoxButtons.OKCancel))
            //                                        {
            //                                            //已经存在就数量++
            //                                            var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.zs_item_id || t.barCodeTM == YhInfo.zstm).FirstOrDefault();
            //                                            if (zsitem != null)
            //                                            {
            //                                                int temp_ = Convert.ToInt32(YhInfo.zs_amount);
            //                                                if (temp_ == 0) temp_ = 1;
            //                                                if (DialogResult.OK == MessageBox.Show("此单 " + goodsBuyList[i].goods + " 满足满额加价赠送活动，是否确认修正商品价格？（" + Math.Round(YhInfo.zsmoney.Value, 2).ToString() + "元）", "活动提醒", MessageBoxButtons.OKCancel))
            //                                                {
            //                                                    if (temp_ != 1)
            //                                                    {
            //                                                        zsitem.countNum = temp_;
            //                                                    }
            //                                                    zsitem.hyPrice = Math.Round(YhInfo.zsmoney.Value, 2);
            //                                                    zsitem.lsPrice = Math.Round(YhInfo.zsmoney.Value, 2);
            //                                                    zsitem.goodsDes = YhInfo.memo;
            //                                                    zsitem.isXG = true;
            //                                                    zsitem.isZS = true;
            //                                                    zsitem.vtype = 5;

            //                                                }
            //                                                isMEZS = true;
            //                                                //goodsBuyList[i].isZS = true;
            //                                            }
            //                                            else
            //                                            {
            //                                                int temp_count_2 = Convert.ToInt32(YhInfo.zs_amount);
            //                                                if (temp_count_2 == 0) temp_count_2 = 1;
            //                                                //赠送的商品
            //                                                var JJZsGoods = new GoodsBuy
            //                                                {
            //                                                    unitStr = dw_100,
            //                                                    barCodeTM = YhInfo.zstm,
            //                                                    noCode = YhInfo.zs_item_id,
            //                                                    goods = YhInfo.zs_cname,
            //                                                    countNum = temp_count_2,
            //                                                    jjPrice = YhInfo.yjj_price,
            //                                                    lsPrice = Math.Round(YhInfo.zsmoney.Value, 2),
            //                                                    hyPrice = Math.Round(YhInfo.zsmoney.Value, 2),
            //                                                    goodsDes = YhInfo.memo,
            //                                                    isZS = true,
            //                                                    isXG = true,
            //                                                    vtype = 5
            //                                                };

            //                                                if (DialogResult.OK == MessageBox.Show("此单 " + goodsBuyList[i].goods + " 满足满额加价赠送活动，是否自动添加赠送商品（" + Math.Round(YhInfo.zsmoney.Value, 2).ToString() + "元）", "活动提醒", MessageBoxButtons.OKCancel))
            //                                                {
            //                                                    //MessageBox.Show("你点击了确定");
            //                                                    goodsBuyList.Add(JJZsGoods);
            //                                                    isMEZS = true;
            //                                                }
            //                                            }
            //                                        }

            //                                    }
            //                                    //商品活动设置不止一个时
            //                                    if (YHInfoList.Count > 1)
            //                                    {
            //                                        if (DialogResult.OK == MessageBox.Show("此单 " + goodsBuyList[i].goods + " 满足满额加价赠送活动，是否选择商品？（" + Math.Round(YhInfo.zsmoney.Value, 2).ToString() + "元）", "活动提醒", MessageBoxButtons.OKCancel))
            //                                        {
            //                                            //MessageBox.Show("你点击了确定");
            //                                            //列出所有活动赠送商品
            //                                            foreach (var item in YHInfoList)
            //                                            {
            //                                                int temp_count_ = Convert.ToInt32(item.zs_amount);
            //                                                if (temp_count_ == 0) temp_count_ = 1;

            //                                                CGForm.ChooseList.Add(new GoodsBuy
            //                                                {
            //                                                    unitStr = dw_100,
            //                                                    barCodeTM = YhInfo.zstm,
            //                                                    noCode = YhInfo.zs_item_id,
            //                                                    goods = YhInfo.zs_cname,
            //                                                    countNum = temp_count_,
            //                                                    jjPrice = YhInfo.yjj_price,
            //                                                    lsPrice = Math.Round(YhInfo.zsmoney.Value, 2),
            //                                                    hyPrice = Math.Round(YhInfo.zsmoney.Value, 2),
            //                                                    goodsDes = YhInfo.memo,
            //                                                    isZS = true,
            //                                                    isXG = true,
            //                                                    vtype = 5
            //                                                });
            //                                                CGForm.ShowDialog();
            //                                                isMEZS = true;
            //                                            }

            //                                        }
            //                                    }
            //                                }
            //                            }
            //                        }
            //                        #endregion

            //                        break;
            //                         */
            //            #endregion

            //            //case 6:
            //                #region 时段特价，这是单一商品降价（不用前台判定时间，有则出现在活动视图，否则消失）

            //                //if (YhInfo.dx_type == 1)  //限定会员
            //                //{
            //                //    if (VipID == 0) continue;

            //                //    //int viplvInt = vipLV.HasValue ? (int)vipLV.Value : 0;
            //                //    int viplvInfo = YhInfo.viptype;
            //                //    //如果会员等级条件满足
            //                //    if (viplv >= viplvInfo)
            //                //    {
            //                //        //购物车已经存在捆绑商品就修改组合活动的特价
            //                //        var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.item_id).FirstOrDefault();

            //                //        if (zsitem != null)
            //                //        {
            //                //            if (YhInfo.xg_amount > 0)
            //                //            {
            //                //                decimal temp6 = YhInfo.xg_amount;
            //                //                //限购条件
            //                //                if (zsitem.countNum < temp6)
            //                //                {
            //                //                    zsitem.hyPrice = Math.Round(YhInfo.ls_price.Value, 2);  //捆绑商品的特价
            //                //                    zsitem.goodsDes = YhInfo.memo; //备注
            //                //                    zsitem.pfPrice = Math.Round(YhInfo.yls_price, 2);
            //                //                    zsitem.isVip = true;
            //                //                    zsitem.vtype = 6;
            //                //                }
            //                //                else if (zsitem.countNum == temp6)
            //                //                {
            //                //                    zsitem.hyPrice = Math.Round(YhInfo.ls_price.Value, 2);  //捆绑商品的特价
            //                //                    zsitem.goodsDes = YhInfo.memo; //备注
            //                //                    zsitem.pfPrice = Math.Round(YhInfo.yls_price, 2);
            //                //                    zsitem.isVip = true;
            //                //                    zsitem.vtype = 6;
            //                //                    zsitem.isXG = true;
            //                //                }
            //                //                else
            //                //                {
            //                //                    //超出部分将按原价售卖
            //                //                    zsitem.isXG = true;
            //                //                    //商品另起一组
            //                //                    //商品查询会判定isXG
            //                //                }
            //                //            }
            //                //            else
            //                //            {
            //                //                zsitem.hyPrice = Math.Round(YhInfo.ls_price.Value, 2);  //捆绑商品的特价
            //                //                zsitem.goodsDes = YhInfo.memo; //备注
            //                //                zsitem.pfPrice = Math.Round(YhInfo.yls_price, 2);
            //                //                zsitem.isVip = true;
            //                //                zsitem.vtype = 6;

            //                //            }
            //                //        }
            //                //    }
            //                //}
            //                //else if (YhInfo.dx_type == 0)   //所有对象
            //                //{
            //                //    //已经存在就数量++
            //                //    var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.item_id).FirstOrDefault();
            //                //    if (zsitem != null)
            //                //    {
            //                //        if (YhInfo.xg_amount > 0)
            //                //        {
            //                //            decimal temp6 = YhInfo.xg_amount;
            //                //            //限购条件
            //                //            if (zsitem.countNum < temp6)
            //                //            {
            //                //                zsitem.hyPrice = Math.Round(YhInfo.ls_price.Value, 2);  //捆绑商品的特价
            //                //                zsitem.lsPrice = Math.Round(YhInfo.ls_price.Value, 2);  //都是特价
            //                //                zsitem.goodsDes = YhInfo.memo; //备注
            //                //                zsitem.pfPrice = Math.Round(YhInfo.yls_price, 2);
            //                //                zsitem.vtype = 6;

            //                //            }
            //                //            else if (zsitem.countNum == temp6)
            //                //            {
            //                //                zsitem.hyPrice = Math.Round(YhInfo.ls_price.Value, 2);  //捆绑商品的特价
            //                //                zsitem.lsPrice = Math.Round(YhInfo.ls_price.Value, 2);  //都是特价
            //                //                zsitem.goodsDes = YhInfo.memo; //备注
            //                //                zsitem.pfPrice = Math.Round(YhInfo.yls_price, 2);
            //                //                zsitem.vtype = 6;
            //                //                zsitem.isXG = true;
            //                //            }
            //                //            else
            //                //            {
            //                //                //超出部分将按原价售卖
            //                //                zsitem.isXG = true;
            //                //                //商品另起一组
            //                //                //商品查询会判定isXG
            //                //            }
            //                //        }
            //                //        else
            //                //        {
            //                //            zsitem.hyPrice = Math.Round(YhInfo.ls_price.Value, 2);  //捆绑商品的特价
            //                //            zsitem.lsPrice = Math.Round(YhInfo.ls_price.Value, 2);  //都是特价
            //                //            zsitem.goodsDes = YhInfo.memo; //备注
            //                //            zsitem.pfPrice = Math.Round(YhInfo.yls_price, 2);
            //                //            zsitem.vtype = 6;

            //                //        }


            //                //    }
            //                //}

            //                //break;

            //                #endregion
            //            //case 7:
            //            #region 零售特价（会员在时间段内限购,其实与时段6一样会自动消失）
            //            //#region 废旧代码

            //            ////零售特价（限购）
            //            ////var XGTJGoods = new GoodsBuy
            //            ////{
            //            ////    noCode = YhInfo.zs_item_id,
            //            ////    goods = YhInfo.zs_cname,
            //            ////    countNum = Convert.ToInt32(YhInfo.zs_amount),
            //            ////    jjPrice = YhInfo.yjj_price,
            //            ////    lsPrice = YhInfo.yls_price,
            //            ////    hyPrice = YhInfo.zs_yprice,
            //            ////    goodsDes = YhInfo.memo
            //            ////};
            //            //////判断是否满足赠送条件(限购)
            //            ////bool isXGTJed = false;
            //            ////foreach (var item in goodsBuyList)
            //            ////{
            //            ////    //防止重复赠送
            //            ////    if (item.noCode == YhInfo.zs_item_id && item.goodsDes == YhInfo.memo)
            //            ////    {
            //            ////        //数量限制
            //            ////        if (item.countNum >= YhInfo.xg_amount)
            //            ////        {
            //            ////            isXGTJed = true; //已经赠送过
            //            ////        }
            //            ////    }
            //            ////}
            //            ////if (isXGTJed == false)
            //            ////{
            //            ////    //已经存在就数量++
            //            ////    var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.zs_item_id).FirstOrDefault();
            //            ////    if (zsitem != null)
            //            ////    {
            //            ////        zsitem.countNum++;
            //            ////    }
            //            ////    else
            //            ////    {
            //            ////        goodsBuyList.Add(XGTJGoods);
            //            ////    }
            //            ////}
            //            //#endregion

            //            //#region 目前0727新代码逻辑

            //            ////if (goodsBuyList[i].isXG) continue;   //这个条件是防止重复判断活动的

            //            //if (YhInfo.dx_type == 1)  //限定会员
            //            //{
            //            //    if (VipID == 0) continue;
            //            //    int viplvInfo = YhInfo.viptype;
            //            //    //如果会员等级条件满足
            //            //    if (viplv >= viplvInfo)
            //            //    {
            //            //        //活动时间内该会员的消费记录
            //            //        var viplsList = db.hd_ls.AsNoTracking().Where(t => t.vip == VipID && t.ctime > YhInfo.sbegintime && t.ctime < YhInfo.sendtime).ToList();
            //            //        if (viplsList.Count > 0)
            //            //        {
            //            //            //消费的商品里是否符合条件的品牌
            //            //            decimal ppzsnum = 0; //符合的数量
            //            //            foreach (var itempp in viplsList)
            //            //            {
            //            //                //找到这件活动商品的购买记录
            //            //                var vipls = db.hd_ls_detail.AsNoTracking().Where(t => t.v_code == itempp.v_code && t.item_id == YhInfo.item_id).FirstOrDefault();
            //            //                if (vipls != null)
            //            //                {
            //            //                    ppzsnum += vipls.amount.Value;  //统计出已经购买数量
            //            //                }
            //            //            }
            //            //            //只需要再购买的数量 
            //            //            decimal numtemp = YhInfo.xg_amount - ppzsnum;
            //            //            if (numtemp <= 0) continue;  //超过限购就不再参与活动
            //            //            //购物车中的商品
            //            //            var item_temp = goodsBuyList.Where(e => e.noCode == YhInfo.item_id);
            //            //            var zsitem = item_temp.FirstOrDefault();
            //            //            if (zsitem != null)
            //            //            {
            //            //                if (YhInfo.xg_amount > 0)
            //            //                {
            //            //                    //购物车中的商品符合的购买数量
            //            //                    var lbnum = item_temp.Select(e => e.countNum).Sum();

            //            //                    if (lbnum < numtemp)
            //            //                    {

            //            //                        zsitem.hyPrice = Math.Round(YhInfo.ls_price.HasValue ? YhInfo.ls_price.Value : (decimal)zsitem.hyPrice, 2);  //捆绑商品的特价
            //            //                        zsitem.goodsDes = YhInfo.memo; //备注
            //            //                        zsitem.pfPrice = Math.Round(YhInfo.yls_price, 2);
            //            //                        goodsBuyList[i].pfPrice = Math.Round(YhInfo.yls_price, 2);
            //            //                        zsitem.isVip = true;
            //            //                        goodsBuyList[i].isVip = true;
            //            //                        zsitem.vtype = 7;
            //            //                        goodsBuyList[i].vtype = 7;

            //            //                    }
            //            //                    else
            //            //                    {
            //            //                        //超出部分将按原价售卖
            //            //                        zsitem.isXG = true;
            //            //                        //商品另起一组
            //            //                        //商品查询会判定isXG
            //            //                        zsitem.hyPrice = Math.Round(YhInfo.ls_price.HasValue ? YhInfo.ls_price.Value : (decimal)zsitem.hyPrice, 2);  //捆绑商品的特价
            //            //                        zsitem.goodsDes = YhInfo.memo; //备注
            //            //                        zsitem.pfPrice = Math.Round(YhInfo.yls_price, 2);
            //            //                        goodsBuyList[i].pfPrice = Math.Round(YhInfo.yls_price, 2);
            //            //                        zsitem.isVip = true;
            //            //                        goodsBuyList[i].isVip = true;
            //            //                        zsitem.vtype = 7;
            //            //                        goodsBuyList[i].vtype = 7;
            //            //                    }
            //            //                }
            //            //                else
            //            //                {
            //            //                    zsitem.hyPrice = Math.Round(YhInfo.ls_price.HasValue ? YhInfo.ls_price.Value : (decimal)zsitem.hyPrice, 2);  //捆绑商品的特价
            //            //                    zsitem.goodsDes = YhInfo.memo; //备注
            //            //                    zsitem.pfPrice = Math.Round(YhInfo.yls_price, 2);
            //            //                    goodsBuyList[i].pfPrice = Math.Round(YhInfo.yls_price, 2);
            //            //                    zsitem.isVip = true;
            //            //                    goodsBuyList[i].isVip = true;
            //            //                    zsitem.vtype = 7;
            //            //                    goodsBuyList[i].vtype = 7;
            //            //                }
            //            //            }

            //            //        }
            //            //        else
            //            //        {
            //            //            var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.item_id).FirstOrDefault();
            //            //            if (zsitem != null)
            //            //            {
            //            //                if (YhInfo.xg_amount > 0)
            //            //                {
            //            //                    //限购条件
            //            //                    if (zsitem.countNum < YhInfo.xg_amount)
            //            //                    {
            //            //                        zsitem.hyPrice = Math.Round(YhInfo.ls_price.HasValue ? YhInfo.ls_price.Value : (decimal)zsitem.hyPrice, 2);  //捆绑商品的特价
            //            //                        zsitem.goodsDes = YhInfo.memo; //备注
            //            //                        zsitem.pfPrice = Math.Round(YhInfo.yls_price, 2);
            //            //                        goodsBuyList[i].pfPrice = Math.Round(YhInfo.yls_price, 2);
            //            //                        zsitem.isVip = true;
            //            //                        goodsBuyList[i].isVip = true;
            //            //                        zsitem.vtype = 7;
            //            //                        goodsBuyList[i].vtype = 7;
            //            //                    }
            //            //                    else
            //            //                    {
            //            //                        //超出部分将按原价售卖
            //            //                        zsitem.isXG = true;
            //            //                        //商品另起一组
            //            //                        //商品查询会判定isXG
            //            //                        zsitem.hyPrice = Math.Round(YhInfo.ls_price.HasValue ? YhInfo.ls_price.Value : (decimal)zsitem.hyPrice, 2);  //捆绑商品的特价
            //            //                        zsitem.goodsDes = YhInfo.memo; //备注
            //            //                        zsitem.pfPrice = Math.Round(YhInfo.yls_price, 2);
            //            //                        goodsBuyList[i].pfPrice = Math.Round(YhInfo.yls_price, 2);
            //            //                        zsitem.isVip = true;
            //            //                        goodsBuyList[i].isVip = true;
            //            //                        zsitem.vtype = 7;
            //            //                        goodsBuyList[i].vtype = 7;
            //            //                    }
            //            //                }
            //            //                else
            //            //                {
            //            //                    zsitem.hyPrice = Math.Round(YhInfo.ls_price.HasValue ? YhInfo.ls_price.Value : (decimal)zsitem.hyPrice, 2);  //捆绑商品的特价
            //            //                    zsitem.goodsDes = YhInfo.memo; //备注
            //            //                    zsitem.pfPrice = Math.Round(YhInfo.yls_price, 2);
            //            //                    goodsBuyList[i].pfPrice = Math.Round(YhInfo.yls_price, 2);
            //            //                    zsitem.isVip = true;
            //            //                    goodsBuyList[i].isVip = true;
            //            //                    zsitem.vtype = 7;
            //            //                    goodsBuyList[i].vtype = 7;
            //            //                }
            //            //            }
            //            //        }
            //            //    }
            //            //}

            //            //#endregion
            //            //break;

            //            #endregion




            //        }
            //    }
            //    //}


            //    //这个活动应该放在提交结算时判断
            //    #region (活动5)主要判断合计金额是否满额就行，不要判断买了什么


            //    //if (isMEZS) continue;   //这个条件是防止重复判断活动的

            //    //decimal sum_temp5 = totalMoney.HasValue ? totalMoney.Value : 0;  //目前总额
            //    ////活动商品列表
            //    //var YHInfo5 = db.v_yh_detail.AsNoTracking().Where(t => t.zjmoney <= sum_temp5 && t.scode == scode_te && t.vtype == 5).ToList();
            //    //if (YHInfo5.Count > 0)
            //    //{

            //    //    if (DialogResult.OK == MessageBox.Show("此单满足满额加价赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.OKCancel))
            //    //    {
            //    //        var cho = new ChoiceGoods();
            //    //        var choVIP = new ChoiceGoods();

            //    //        foreach (var item in YHInfo5)
            //    //        {

            //    //            #region 商品单位查询
            //    //            int zsunit5 = 0;
            //    //            var zsinfo100 = db.hd_item_info.AsNoTracking().Where(t => t.item_id == YhInfo.zs_item_id || t.tm == YhInfo.zstm).Select(t => t.unit).FirstOrDefault();
            //    //            if (zsinfo100 != null)
            //    //            {
            //    //                zsunit5 = zsinfo100.HasValue ? (int)zsinfo100.Value : 1;
            //    //            }
            //    //            //需要把单位编号转换为中文以便UI显示
            //    //            string dw_5 = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == zsunit5).Select(t => t.txt1).FirstOrDefault();
            //    //            #endregion

            //    //            if (item.dx_type == 1)  //限定会员
            //    //            {
            //    //                if (VipID == 0) continue;

            //    //                //int viplvInt = vipLV.HasValue ? (int)vipLV.Value : 0;
            //    //                int viplvInfo = item.viptype;
            //    //                //如果会员等级条件满足
            //    //                if (viplv >= viplvInfo)
            //    //                {
            //    //                    choVIP.ChooseList.Add(new GoodsBuy
            //    //                     {
            //    //                         unit=zsunit5,
            //    //                         unitStr=dw_5,
            //    //                         noCode = item.zs_item_id,
            //    //                         barCodeTM = item.zstm,
            //    //                         goods = item.zs_cname,
            //    //                         countNum = Convert.ToInt32(item.zs_amount.Value),
            //    //                         lsPrice = Math.Round((item.zsmoney / item.zs_amount).Value, 2),
            //    //                         hyPrice = Math.Round((item.zsmoney / item.zs_amount).Value, 2),
            //    //                         goodsDes = item.memo,
            //    //                         isVip=true,
            //    //                         isZS=true,
            //    //                         isXG=true,
            //    //                         vtype=5
            //    //                     });
            //    //                }
            //    //            }
            //    //            //所有对象
            //    //            else if (YhInfo.dx_type == 0)
            //    //            {

            //    //                cho.ChooseList.Add(new GoodsBuy
            //    //                {
            //    //                    unit = zsunit5,
            //    //                    unitStr = dw_5,
            //    //                    noCode = item.zs_item_id,
            //    //                    barCodeTM = item.zstm,
            //    //                    goods = item.zs_cname,
            //    //                    countNum = Convert.ToInt32(item.zs_amount.Value),
            //    //                    lsPrice =Math.Round((item.zsmoney/item.zs_amount).Value,2),
            //    //                    hyPrice = Math.Round((item.zsmoney / item.zs_amount).Value, 2),
            //    //                    goodsDes = item.memo,
            //    //                    isZS=true,
            //    //                    isXG=true,
            //    //                    vtype=5
            //    //                });
            //    //            }
            //    //        }
            //    //        if (choVIP.ChooseList.Count > 0)
            //    //        {
            //    //            choVIP.changed += choVIP_changed;
            //    //            choVIP.ShowDialog();
            //    //        }
            //    //        else
            //    //        {
            //    //            cho.changed += cho_changed;
            //    //            cho.ShowDialog();
            //    //        }


            //    //    }
            //    //    else
            //    //    {
            //    //        //不接受活动
            //    //        isMEZS = true;
            //    //        MessageBox.Show("如果要重新参加活动需要重进收银");
            //    //    }

            //    ////var MEZS = goodsBuyList.Where()

            //    //}



            //    #endregion

            //    #region 活动9的类别赠与品牌赠
            //    ////先判定有没有这两个活动
            //    //var ppinfo = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 9 && t.tj_range == 2 && t.scode == scode_te).ToList();
            //    ////要判定购物车内是否购满该类别或者品牌的商品
            //    ////然后才送
            //    //if (ppinfo.Count > 0)
            //    //{
            //    //    var solo1 = new ChoiceGoods();
            //    //        foreach (var item in ppinfo)
            //    //        {
            //    //            if (item.dx_type == 1)  //限定会员
            //    //            {
            //    //                if (VipID == 0) continue;

            //    //                int viplvInt = vipLV.HasValue ? (int)vipLV.Value : 0;
            //    //                int viplvInfo = item.viptype;
            //    //                //如果会员等级条件满足
            //    //                if (viplvInt >= viplvInfo)
            //    //                {
            //    //                    //购物车中的商品类别符合的购买数量
            //    //                    //var lbnum = goodsBuyList.Where(e => e.LB == item.item_id).Select(e => e.countNum).Sum();
            //    //                    decimal lbnum = 0;
            //    //                    foreach (var itemGoods in goodsBuyList)
            //    //                    {
            //    //                        if (itemGoods.PP == item.tm)
            //    //                        {
            //    //                            lbnum += itemGoods.countNum;
            //    //                        }
            //    //                    }

            //    //                    if (lbnum >= item.amount)
            //    //                    {
            //    //                        var lbzs = new GoodsBuy
            //    //                        {
            //    //                            barCodeTM = item.zstm,
            //    //                            noCode = item.zs_item_id,
            //    //                            countNum = Convert.ToInt32(item.zs_amount),
            //    //                            goods = item.zs_cname,
            //    //                            goodsDes = item.memo,
            //    //                            lsPrice = Math.Round(item.zs_yprice.Value,2),
            //    //                            hyPrice = 0.00m,
            //    //                            isVip =true,
            //    //                            isZS =true,
            //    //                            vtype =9
            //    //                        };
            //    //                        solo1.ChooseList.Add(lbzs);
            //    //                    }
            //    //                }
            //    //            }
            //    //                //不限定会员
            //    //            else
            //    //            {
            //    //                //购物车中的商品类别符合的购买数量
            //    //                var lbnum = goodsBuyList.Where(e => e.PP == item.tm).Select(e => e.countNum).Sum();
            //    //                if (lbnum >= item.amount)
            //    //                {
            //    //                    var lbzs = new GoodsBuy
            //    //                    {
            //    //                        barCodeTM = item.zstm,
            //    //                        noCode = item.zs_item_id,
            //    //                        countNum = Convert.ToInt32(item.zs_amount),
            //    //                        goods = item.zs_cname,
            //    //                        goodsDes = item.memo,
            //    //                        lsPrice = 0.00m,
            //    //                        hyPrice = 0.00m,
            //    //                        isZS = true,
            //    //                        vtype = 9
            //    //                    };
            //    //                    solo1.ChooseList.Add(lbzs);
            //    //                }
            //    //            }

            //    //    }

            //    //        if (solo1.ChooseList.Count > 0)
            //    //        {
            //    //            solo1.changed += solo_changed;
            //    //            solo1.ShowDialog();
            //    //        }
            //    //}



            //    #endregion


            //    //dataGridView_Cashiers.InvalidateRow(i);  //强制刷新行数据


            //}
            //}
            //catch (Exception ex)
            //{

            //    LogHelper.WriteLog("收银主界面处理优惠活动时发生异常:", ex);
            //    MessageBox.Show("优惠活动处理出错！");

            //}


            #endregion

        }


        /// <summary>
        /// 活动6 时段特价
        /// </summary>
        /// <param name="db"></param>
        private void YH6TJFunc(hjnbhEntities db)
        {
            int scode_temp = HandoverModel.GetInstance.scode;
            int VipID = HandoverModel.GetInstance.VipID;
            int viplv = HandoverModel.GetInstance.VipLv;

            //活动商品列表
            var YHInfo6 = db.v_yh_detail.AsNoTracking().Where(t => t.scode == scode_temp && t.vtype == 6).ToList();
            if (YHInfo6.Count > 0)
            {

                foreach (var item in YHInfo6)
                {
                    //过滤一下时间
                    if (item.sendtime.HasValue)
                    {
                        if (item.sendtime.Value < System.DateTime.Now) continue;

                    }
                    //先过滤一下，看购物车中是否有符合活动的普通商品
                    var HasHdItem = goodsBuyList.Where(t => t.vtype == 0 && t.noCode == item.item_id).FirstOrDefault();
                    if (HasHdItem == null) continue;

                    if (item.dx_type >= 0)  //不限制对象
                    {

                        //购物车中符合活动的普通商品(不与其它活动重叠)
                        var goodsptList = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 0).ToList();
                        if (goodsptList.Count > 0)
                        {
                            for (int i = 0; i < goodsptList.Count; i++)
                            {
                                //同名活动10
                                var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.item_id).FirstOrDefault();
                                //得判断有没有同名的活动10存在的情况
                                if (YH10ZS != null)
                                {
                                    if (YH10ZS.amount <= 0) continue;  //有总量才有得搞
                                    //还可以购买的数量
                                    decimal ZScount = 1.00m;
                                    //关联的数量,要有数量才有得搞
                                    if (YH10ZS.amount < goodsptList[i].countNum)
                                    {
                                        ZScount = YH10ZS.amount;
                                    }
                                    else
                                    {
                                        ZScount = goodsptList[i].countNum;
                                    }

                                    //那么来吧，互相伤害
                                    if (goodsptList[i].countNum <= ZScount)
                                    {
                                        goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                        goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                        goodsptList[i].goodsDes = item.memo; //备注
                                        goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                        goodsptList[i].vtype = 6;
                                        goodsptList[i].isGL = true;

                                    }
                                    else
                                    {
                                        //那么分拆
                                        goodsptList[i].countNum -= ZScount;
                                        goodsBuyList.Add(new GoodsBuy
                                        {
                                            spec = goodsptList[i].spec,
                                            pinYin = goodsptList[i].pinYin,
                                            unit = goodsptList[i].unit,
                                            unitStr = goodsptList[i].unitStr,
                                            barCodeTM = goodsptList[i].barCodeTM,
                                            noCode = goodsptList[i].noCode,
                                            countNum = ZScount,
                                            goods = goodsptList[i].goods,
                                            goodsDes = item.memo,
                                            lsPrice = Math.Round(item.ls_price.Value, 2),
                                            hyPrice = Math.Round(item.ls_price.Value, 2),
                                            jjPrice = item.yjj_price,
                                            pfPrice = Math.Round(item.yls_price, 2),
                                            vtype = 6,
                                            isGL = true,
                                        });

                                    }

                                }
                                else
                                {
                                    //没有关联的情况
                                    //那么来吧，互相伤害
                                    goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                    goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                    goodsptList[i].goodsDes = item.memo; //备注
                                    goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                    goodsptList[i].vtype = 6;


                                }
                            }

                        }

                    }
                }
            }
        }


        /// <summary>
        /// 活动2 零售特价
        /// </summary>
        /// <param name="db"></param>
        private void YH2TJFunc(hjnbhEntities db)
        {
            int scode_temp = HandoverModel.GetInstance.scode;
            int VipID = HandoverModel.GetInstance.VipID;
            int viplv = HandoverModel.GetInstance.VipLv;

            //活动商品列表
            var YHInfo2 = db.v_yh_detail.AsNoTracking().Where(t => t.scode == scode_temp && t.vtype == 2).ToList();
            if (YHInfo2.Count > 0)
            {

                foreach (var item in YHInfo2)
                {
                    //先过滤一下，看购物车中是否有符合活动的普通商品
                    var HasHdItem = goodsBuyList.Where(t => t.vtype == 0 && t.noCode == item.item_id).FirstOrDefault();
                    if (HasHdItem == null) continue;

                    if (item.dx_type == 1)  //限定会员
                    {
                        if (VipID == 0) continue;
                        int viplvInfo = item.viptype;
                        //如果会员等级条件满足
                        if (viplv >= viplvInfo)
                        {
                            //同名活动10
                            var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.item_id).FirstOrDefault();
                            //得判断有没有同名的活动10存在的情况
                            if (YH10ZS != null)
                            {
                                if (YH10ZS.amount <= 0) continue;  //有总量才有得搞
                                //还可以购买的数量
                                decimal ZScount = 1.00m;
                                //关联的数量,要有数量才有得搞
                                if (YH10ZS.amount < item.xg_amount)
                                {
                                    ZScount = YH10ZS.amount;
                                }
                                else
                                {
                                    ZScount = item.xg_amount;
                                }


                                //购物车中符合活动的普通商品(不与其它活动重叠)
                                var goodsptList = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 0).ToList();
                                if (goodsptList.Count > 0)
                                {
                                    for (int i = 0; i < goodsptList.Count; i++)
                                    {
                                        //那么来吧，互相伤害
                                        //如果限购
                                        if (item.xg_amount > 0)
                                        {
                                            if (goodsptList[i].countNum <= ZScount)
                                            {
                                                goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                goodsptList[i].goodsDes = item.memo; //备注
                                                goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                goodsptList[i].isVip = true;
                                                goodsptList[i].vtype = 2;
                                                goodsptList[i].isGL = true;

                                            }
                                            else
                                            {
                                                //那么分拆
                                                goodsptList[i].countNum -= ZScount;
                                                goodsBuyList.Add(new GoodsBuy
                                                {
                                                    spec = goodsptList[i].spec,
                                                    pinYin = goodsptList[i].pinYin,
                                                    unit = goodsptList[i].unit,
                                                    unitStr = goodsptList[i].unitStr,
                                                    barCodeTM = goodsptList[i].barCodeTM,
                                                    noCode = goodsptList[i].noCode,
                                                    countNum = ZScount,
                                                    goods = goodsptList[i].goods,
                                                    goodsDes = item.memo,
                                                    lsPrice = goodsptList[i].lsPrice,
                                                    hyPrice = Math.Round(item.ls_price.Value, 2),
                                                    jjPrice = item.yjj_price,
                                                    pfPrice = Math.Round(item.yls_price, 2),
                                                    isVip = true,
                                                    vtype = 2,
                                                    isGL = true,

                                                });

                                            }

                                        }
                                        else
                                        {
                                            //不限购
                                            if (goodsptList[i].countNum > YH10ZS.amount)
                                            {
                                                ZScount = YH10ZS.amount;
                                            }
                                            else
                                            {
                                                ZScount = goodsptList[i].countNum;
                                            }

                                            if (goodsptList[i].countNum <= ZScount)
                                            {
                                                goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                goodsptList[i].goodsDes = item.memo; //备注
                                                goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                goodsptList[i].isVip = true;
                                                goodsptList[i].vtype = 2;
                                                goodsptList[i].isGL = true;

                                            }
                                            else
                                            {
                                                //那么分拆
                                                goodsptList[i].countNum -= ZScount;
                                                goodsBuyList.Add(new GoodsBuy
                                                {
                                                    spec = goodsptList[i].spec,
                                                    pinYin = goodsptList[i].pinYin,
                                                    unit = goodsptList[i].unit,
                                                    unitStr = goodsptList[i].unitStr,
                                                    barCodeTM = goodsptList[i].barCodeTM,
                                                    noCode = goodsptList[i].noCode,
                                                    countNum = ZScount,
                                                    goods = goodsptList[i].goods,
                                                    goodsDes = item.memo,
                                                    lsPrice = goodsptList[i].lsPrice,
                                                    hyPrice = Math.Round(item.ls_price.Value, 2),
                                                    jjPrice = item.yjj_price,
                                                    pfPrice = Math.Round(item.yls_price, 2),
                                                    isVip = true,
                                                    vtype = 2,
                                                    isGL = true,

                                                });

                                            }

                                        }
                                    }
                                }


                            }
                            else
                            {
                                //没有关联的情况
                                //购物车中符合活动的普通商品(不与其它活动重叠)
                                var goodsptList = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 0).ToList();
                                if (goodsptList.Count > 0)
                                {
                                    for (int i = 0; i < goodsptList.Count; i++)
                                    {
                                        //那么来吧，互相伤害
                                        //如果限购
                                        if (item.xg_amount > 0)
                                        {
                                            if (goodsptList[i].countNum <= item.xg_amount)
                                            {
                                                goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                goodsptList[i].goodsDes = item.memo; //备注
                                                goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                goodsptList[i].isVip = true;
                                                goodsptList[i].vtype = 2;

                                            }
                                            else
                                            {
                                                //那么分拆
                                                goodsptList[i].countNum -= item.xg_amount;
                                                goodsBuyList.Add(new GoodsBuy
                                                {
                                                    spec = goodsptList[i].spec,
                                                    pinYin = goodsptList[i].pinYin,
                                                    unit = goodsptList[i].unit,
                                                    unitStr = goodsptList[i].unitStr,
                                                    barCodeTM = goodsptList[i].barCodeTM,
                                                    noCode = goodsptList[i].noCode,
                                                    countNum = item.xg_amount,
                                                    goods = goodsptList[i].goods,
                                                    goodsDes = item.memo,
                                                    lsPrice = goodsptList[i].lsPrice,
                                                    hyPrice = Math.Round(item.ls_price.Value, 2),
                                                    jjPrice = item.yjj_price,
                                                    pfPrice = Math.Round(item.yls_price, 2),
                                                    isVip = true,
                                                    vtype = 2,

                                                });

                                            }

                                        }
                                        else
                                        {
                                            //不限购

                                            goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                            goodsptList[i].goodsDes = item.memo; //备注
                                            goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                            goodsptList[i].isVip = true;
                                            goodsptList[i].vtype = 2;

                                        }
                                    }
                                }

                            }

                        }
                    }
                    else
                    {
                        //不限定对象

                        //如果是会员消费的情况
                        if (VipID > 0)
                        {
                            //同名活动10
                            var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.item_id).FirstOrDefault();
                            //得判断有没有同名的活动10存在的情况
                            if (YH10ZS != null)
                            {
                                if (YH10ZS.amount <= 0) continue;  //有总量才有得搞
                                //还可以购买的数量
                                decimal ZScount = 1.00m;
                                //关联的数量,要有数量才有得搞
                                if (YH10ZS.amount < item.xg_amount)
                                {
                                    ZScount = YH10ZS.amount;
                                }
                                else
                                {
                                    ZScount = item.xg_amount;
                                }


                                //购物车中符合活动的普通商品(不与其它活动重叠)
                                var goodsptList = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 0).ToList();
                                if (goodsptList.Count > 0)
                                {
                                    for (int i = 0; i < goodsptList.Count; i++)
                                    {
                                        //那么来吧，互相伤害
                                        //如果限购
                                        if (item.xg_amount > 0)
                                        {
                                            if (goodsptList[i].countNum <= ZScount)
                                            {
                                                goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                goodsptList[i].goodsDes = item.memo; //备注
                                                goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                goodsptList[i].isVip = true;
                                                goodsptList[i].vtype = 2;
                                                goodsptList[i].isGL = true;

                                            }
                                            else
                                            {
                                                //那么分拆
                                                goodsptList[i].countNum -= ZScount;
                                                goodsBuyList.Add(new GoodsBuy
                                                {
                                                    spec = goodsptList[i].spec,
                                                    pinYin = goodsptList[i].pinYin,
                                                    unit = goodsptList[i].unit,
                                                    unitStr = goodsptList[i].unitStr,
                                                    barCodeTM = goodsptList[i].barCodeTM,
                                                    noCode = goodsptList[i].noCode,
                                                    countNum = ZScount,
                                                    goods = goodsptList[i].goods,
                                                    goodsDes = item.memo,
                                                    lsPrice = goodsptList[i].lsPrice,
                                                    hyPrice = Math.Round(item.ls_price.Value, 2),
                                                    jjPrice = item.yjj_price,
                                                    pfPrice = Math.Round(item.yls_price, 2),
                                                    isVip = true,
                                                    vtype = 2,
                                                    isGL = true,

                                                });

                                            }

                                        }
                                        else
                                        {
                                            //如果不限购
                                            //不限购
                                            if (goodsptList[i].countNum > YH10ZS.amount)
                                            {
                                                ZScount = YH10ZS.amount;
                                            }
                                            else
                                            {
                                                ZScount = goodsptList[i].countNum;
                                            }

                                            if (goodsptList[i].countNum <= ZScount)
                                            {
                                                goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                goodsptList[i].goodsDes = item.memo; //备注
                                                goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                goodsptList[i].isVip = true;
                                                goodsptList[i].vtype = 2;
                                                goodsptList[i].isGL = true;

                                            }
                                            else
                                            {
                                                //那么分拆
                                                goodsptList[i].countNum -= ZScount;
                                                goodsBuyList.Add(new GoodsBuy
                                                {
                                                    spec = goodsptList[i].spec,
                                                    pinYin = goodsptList[i].pinYin,
                                                    unit = goodsptList[i].unit,
                                                    unitStr = goodsptList[i].unitStr,
                                                    barCodeTM = goodsptList[i].barCodeTM,
                                                    noCode = goodsptList[i].noCode,
                                                    countNum = ZScount,
                                                    goods = goodsptList[i].goods,
                                                    goodsDes = item.memo,
                                                    lsPrice = goodsptList[i].lsPrice,
                                                    hyPrice = Math.Round(item.ls_price.Value, 2),
                                                    jjPrice = item.yjj_price,
                                                    pfPrice = Math.Round(item.yls_price, 2),
                                                    isVip = true,
                                                    vtype = 2,
                                                    isGL = true,

                                                });

                                            }



                                        }
                                    }
                                }


                            }
                            else
                            {
                                //没有关联的情况
                                //购物车中符合活动的普通商品(不与其它活动重叠)
                                var goodsptList = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 0).ToList();
                                if (goodsptList.Count > 0)
                                {
                                    for (int i = 0; i < goodsptList.Count; i++)
                                    {
                                        //那么来吧，互相伤害
                                        //如果限购
                                        if (item.xg_amount > 0)
                                        {
                                            if (goodsptList[i].countNum <= item.xg_amount)
                                            {
                                                goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                goodsptList[i].goodsDes = item.memo; //备注
                                                goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                goodsptList[i].isVip = true;
                                                goodsptList[i].vtype = 2;

                                            }
                                            else
                                            {
                                                //那么分拆
                                                goodsptList[i].countNum -= item.xg_amount;
                                                goodsBuyList.Add(new GoodsBuy
                                                {
                                                    spec = goodsptList[i].spec,
                                                    pinYin = goodsptList[i].pinYin,
                                                    unit = goodsptList[i].unit,
                                                    unitStr = goodsptList[i].unitStr,
                                                    barCodeTM = goodsptList[i].barCodeTM,
                                                    noCode = goodsptList[i].noCode,
                                                    countNum = item.xg_amount,
                                                    goods = goodsptList[i].goods,
                                                    goodsDes = item.memo,
                                                    lsPrice = goodsptList[i].lsPrice,
                                                    hyPrice = Math.Round(item.ls_price.Value, 2),
                                                    jjPrice = item.yjj_price,
                                                    pfPrice = Math.Round(item.yls_price, 2),
                                                    isVip = true,
                                                    vtype = 2,

                                                });

                                            }

                                        }
                                        else
                                        {
                                            //不限购
                                            goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                            goodsptList[i].goodsDes = item.memo; //备注
                                            goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                            goodsptList[i].isVip = true;
                                            goodsptList[i].vtype = 2;

                                        }
                                    }
                                }

                            }


                        }
                        else
                        {
                            //非会员消费
                            //同名活动10
                            var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.item_id).FirstOrDefault();
                            //得判断有没有同名的活动10存在的情况
                            if (YH10ZS != null)
                            {
                                if (YH10ZS.amount <= 0) continue;  //有总量才有得搞
                                //还可以购买的数量
                                decimal ZScount = 1.00m;
                                //关联的数量,要有数量才有得搞
                                if (YH10ZS.amount < item.xg_amount)
                                {
                                    ZScount = YH10ZS.amount;
                                }
                                else
                                {
                                    ZScount = item.xg_amount;
                                }


                                //购物车中符合活动的普通商品(不与其它活动重叠)
                                var goodsptList = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 0).ToList();
                                if (goodsptList.Count > 0)
                                {
                                    for (int i = 0; i < goodsptList.Count; i++)
                                    {
                                        //那么来吧，互相伤害
                                        //如果限购
                                        if (item.xg_amount > 0)
                                        {
                                            if (goodsptList[i].countNum <= ZScount)
                                            {
                                                goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                goodsptList[i].goodsDes = item.memo; //备注
                                                goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                goodsptList[i].vtype = 2;
                                                goodsptList[i].isGL = true;

                                            }
                                            else
                                            {
                                                //那么分拆
                                                goodsptList[i].countNum -= ZScount;
                                                goodsBuyList.Add(new GoodsBuy
                                                {
                                                    spec = goodsptList[i].spec,
                                                    pinYin = goodsptList[i].pinYin,
                                                    unit = goodsptList[i].unit,
                                                    unitStr = goodsptList[i].unitStr,
                                                    barCodeTM = goodsptList[i].barCodeTM,
                                                    noCode = goodsptList[i].noCode,
                                                    countNum = ZScount,
                                                    goods = goodsptList[i].goods,
                                                    goodsDes = item.memo,
                                                    lsPrice = Math.Round(item.ls_price.Value, 2),
                                                    hyPrice = Math.Round(item.ls_price.Value, 2),
                                                    jjPrice = item.yjj_price,
                                                    pfPrice = Math.Round(item.yls_price, 2),
                                                    vtype = 2,
                                                    isGL = true,

                                                });

                                            }

                                        }
                                        else
                                        {
                                            //不限购
                                            if (goodsptList[i].countNum > YH10ZS.amount)
                                            {
                                                ZScount = YH10ZS.amount;
                                            }
                                            else
                                            {
                                                ZScount = goodsptList[i].countNum;
                                            }

                                            if (goodsptList[i].countNum <= ZScount)
                                            {
                                                goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                goodsptList[i].goodsDes = item.memo; //备注
                                                goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                goodsptList[i].vtype = 2;
                                                goodsptList[i].isGL = true;

                                            }
                                            else
                                            {
                                                //那么分拆
                                                goodsptList[i].countNum -= ZScount;
                                                goodsBuyList.Add(new GoodsBuy
                                                {
                                                    spec = goodsptList[i].spec,
                                                    pinYin = goodsptList[i].pinYin,
                                                    unit = goodsptList[i].unit,
                                                    unitStr = goodsptList[i].unitStr,
                                                    barCodeTM = goodsptList[i].barCodeTM,
                                                    noCode = goodsptList[i].noCode,
                                                    countNum = ZScount,
                                                    goods = goodsptList[i].goods,
                                                    goodsDes = item.memo,
                                                    lsPrice = Math.Round(item.ls_price.Value, 2),
                                                    hyPrice = Math.Round(item.ls_price.Value, 2),
                                                    jjPrice = item.yjj_price,
                                                    pfPrice = Math.Round(item.yls_price, 2),
                                                    vtype = 2,
                                                    isGL = true,

                                                });

                                            }

                                        }
                                    }
                                }


                            }
                            else
                            {
                                //没有关联的情况
                                //购物车中符合活动的普通商品(不与其它活动重叠)
                                var goodsptList = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 0).ToList();
                                if (goodsptList.Count > 0)
                                {
                                    for (int i = 0; i < goodsptList.Count; i++)
                                    {
                                        //那么来吧，互相伤害
                                        //如果限购
                                        if (item.xg_amount > 0)
                                        {
                                            if (goodsptList[i].countNum <= item.xg_amount)
                                            {
                                                goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                goodsptList[i].goodsDes = item.memo; //备注
                                                goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                goodsptList[i].vtype = 2;

                                            }
                                            else
                                            {
                                                //那么分拆
                                                goodsptList[i].countNum -= item.xg_amount;
                                                goodsBuyList.Add(new GoodsBuy
                                                {
                                                    spec = goodsptList[i].spec,
                                                    pinYin = goodsptList[i].pinYin,
                                                    unit = goodsptList[i].unit,
                                                    unitStr = goodsptList[i].unitStr,
                                                    barCodeTM = goodsptList[i].barCodeTM,
                                                    noCode = goodsptList[i].noCode,
                                                    countNum = item.xg_amount,
                                                    goods = goodsptList[i].goods,
                                                    goodsDes = item.memo,
                                                    lsPrice = Math.Round(item.ls_price.Value, 2),
                                                    hyPrice = Math.Round(item.ls_price.Value, 2),
                                                    jjPrice = item.yjj_price,
                                                    pfPrice = Math.Round(item.yls_price, 2),         
                                                    vtype = 2,

                                                });

                                            }

                                        }
                                        else
                                        {
                                            //不限购
                                            goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                            goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                            goodsptList[i].goodsDes = item.memo; //备注
                                            goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                            goodsptList[i].vtype = 2;

                                        }
                                    }
                                }

                            }



                        }

                    }
                }
            }
        }

        /// <summary>
        /// 活动7  限量购买特价单
        /// </summary>
        /// <param name="db"></param>
        private void YH7SLFunc(hjnbhEntities db)
        {
            int scode_temp = HandoverModel.GetInstance.scode;
            int VipID = HandoverModel.GetInstance.VipID;
            int viplv = HandoverModel.GetInstance.VipLv;

            //活动商品列表
            var YHInfo7 = db.v_yh_detail.AsNoTracking().Where(t => t.scode == scode_temp && t.vtype == 7).ToList();
            if (YHInfo7.Count > 0)
            {

                foreach (var item in YHInfo7)
                {
                    //先过滤一下，看购物车中是否有符合活动的普通商品
                    var HasHdItem = goodsBuyList.Where(t => t.vtype == 0 && t.noCode == item.item_id).FirstOrDefault();
                    if (HasHdItem == null) continue;

                    if (item.dx_type == 1)  //限定会员
                    {
                        if (VipID == 0) continue;
                        int viplvInfo = item.viptype;
                        //如果会员等级条件满足
                        if (viplv >= viplvInfo)
                        {

                            //同名活动10
                            var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.item_id).FirstOrDefault();
                            //得判断有没有同名的活动10存在的情况
                            if (YH10ZS != null)
                            {
                                if (YH10ZS.amount <= 0) continue;  //有总量才有得搞
                                //还可以购买的数量
                                decimal ZScount = 1.00m;
                                //关联的数量,要有数量才有得搞
                                if (YH10ZS.amount < item.xg_amount)
                                {
                                    ZScount = YH10ZS.amount;
                                }
                                else
                                {
                                    ZScount = item.xg_amount;
                                }

                                //活动时间内该会员的消费记录
                                var viplsList = db.hd_ls.AsNoTracking().Where(t => t.vip == VipID && t.ctime > item.sbegintime && t.ctime < item.sendtime).ToList();
                                if (viplsList.Count > 0)
                                {
                                    decimal ppzsnum = 0; //已经购买的数量
                                    foreach (var itempp in viplsList)
                                    {
                                        //找到这件活动商品的购买记录
                                        var vipls = db.hd_ls_detail.AsNoTracking().Where(t => t.v_code == itempp.v_code && t.item_id == item.item_id).FirstOrDefault();
                                        if (vipls != null)
                                        {
                                            ppzsnum += vipls.amount.Value;  //统计出已经购买数量
                                        }
                                    }
                                    //还可以购买的数量 
                                    decimal numtemp = ZScount - ppzsnum;
                                    if (numtemp <= 0) continue;  //超过限购或者总量就不再参与活动
                                    //购物车中是否已经添加了活动商品，避免重复
                                    var ZSitems = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 7).FirstOrDefault();
                                    if (ZSitems != null)
                                    {
                                        if (ZSitems.countNum >= numtemp)
                                        {
                                            continue;
                                        }
                                    }

                                    //购物车中符合活动的普通商品(不与其它活动重叠)
                                    var goodsptList = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 0).ToList();
                                    if (goodsptList.Count > 0)
                                    {
                                        for (int i = 0; i < goodsptList.Count; i++)
                                        {
                                            //那么来吧，互相伤害
                                            //如果限购
                                            if (item.xg_amount > 0)
                                            {
                                                if (goodsptList[i].countNum <= numtemp)
                                                {
                                                    goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                    goodsptList[i].goodsDes = item.memo; //备注
                                                    goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                    goodsptList[i].isVip = true;
                                                    goodsptList[i].vtype = 7;
                                                    goodsptList[i].isGL = true;
                                                    //goodsptList[i].isXG = true;
                                                }
                                                else
                                                {
                                                    //那么分拆
                                                    goodsptList[i].countNum -= numtemp;
                                                    goodsBuyList.Add(new GoodsBuy
                                                    {
                                                        spec = goodsptList[i].spec,
                                                        pinYin = goodsptList[i].pinYin,
                                                        unit = goodsptList[i].unit,
                                                        unitStr = goodsptList[i].unitStr,
                                                        barCodeTM = goodsptList[i].barCodeTM,
                                                        noCode = goodsptList[i].noCode,
                                                        countNum = numtemp,
                                                        goods = goodsptList[i].goods,
                                                        goodsDes = item.memo,
                                                        lsPrice = goodsptList[i].lsPrice,
                                                        hyPrice = Math.Round(item.ls_price.Value, 2),
                                                        jjPrice = item.yjj_price,
                                                        pfPrice = Math.Round(item.yls_price, 2),
                                                        isVip = true,
                                                        vtype = 7,
                                                        isGL = true,
                                                        //isXG=true
                                                    });

                                                }


                                            }

                                        }
                                    }

                                }
                                else
                                {
                                    //该会员没有消费记录

                                    //购物车中是否已经添加了活动商品，避免重复
                                    var ZSitems = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 7).FirstOrDefault();
                                    if (ZSitems != null)
                                    {
                                        if (ZSitems.countNum >= ZScount)
                                        {
                                            continue;
                                        }
                                    }
                                    //购物车中符合活动的普通商品(不与其它活动重叠)
                                    var goodsptList = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 0).ToList();
                                    if (goodsptList.Count > 0)
                                    {
                                        for (int i = 0; i < goodsptList.Count; i++)
                                        {
                                            //那么来吧，互相伤害
                                            //如果限购
                                            if (item.xg_amount > 0)
                                            {
                                                if (goodsptList[i].countNum <= ZScount)
                                                {
                                                    goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                    goodsptList[i].goodsDes = item.memo; //备注
                                                    goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                    goodsptList[i].isVip = true;
                                                    goodsptList[i].vtype = 7;
                                                    goodsptList[i].isGL = true;
                                                    //goodsptList[i].isXG = true;

                                                }
                                                else
                                                {
                                                    //那么分拆
                                                    goodsptList[i].countNum -= ZScount;
                                                    goodsBuyList.Add(new GoodsBuy
                                                    {
                                                        spec = goodsptList[i].spec,
                                                        pinYin = goodsptList[i].pinYin,
                                                        unit = goodsptList[i].unit,
                                                        unitStr = goodsptList[i].unitStr,
                                                        barCodeTM = goodsptList[i].barCodeTM,
                                                        noCode = goodsptList[i].noCode,
                                                        countNum = ZScount,
                                                        goods = goodsptList[i].goods,
                                                        goodsDes = item.memo,
                                                        lsPrice = goodsptList[i].lsPrice,
                                                        hyPrice = Math.Round(item.ls_price.Value, 2),
                                                        jjPrice = item.yjj_price,
                                                        pfPrice = Math.Round(item.yls_price, 2),
                                                        isVip = true,
                                                        vtype = 7,
                                                        isGL = true,
                                                        //isXG=true
                                                    });

                                                }

                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //没有关联的情况

                                //活动时间内该会员的消费记录
                                var viplsList = db.hd_ls.AsNoTracking().Where(t => t.vip == VipID && t.ctime > item.sbegintime && t.ctime < item.sendtime).ToList();
                                if (viplsList.Count > 0)
                                {
                                    decimal ppzsnum = 0; //已经购买的数量
                                    foreach (var itempp in viplsList)
                                    {
                                        //找到这件活动商品的购买记录
                                        var vipls = db.hd_ls_detail.AsNoTracking().Where(t => t.v_code == itempp.v_code && t.item_id == item.item_id).FirstOrDefault();
                                        if (vipls != null)
                                        {
                                            ppzsnum += vipls.amount.Value;  //统计出已经购买数量
                                        }
                                    }
                                    //还可以购买的数量 
                                    decimal numtemp = item.xg_amount - ppzsnum;
                                    if (numtemp <= 0) continue;  //超过限购或者总量就不再参与活动

                                    //购物车中是否已经添加了活动商品，避免重复
                                    var ZSitems = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 7).FirstOrDefault();
                                    if (ZSitems != null)
                                    {
                                        if (ZSitems.countNum >= numtemp)
                                        {
                                            continue;
                                        }
                                    }
                                    //购物车中符合活动的普通商品(不与其它活动重叠)
                                    var goodsptList = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 0).ToList();
                                    if (goodsptList.Count > 0)
                                    {
                                        for (int i = 0; i < goodsptList.Count; i++)
                                        {
                                            //那么来吧，互相伤害
                                            //如果限购
                                            if (item.xg_amount > 0)
                                            {
                                                if (goodsptList[i].countNum <= numtemp)
                                                {
                                                    goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                    goodsptList[i].goodsDes = item.memo; //备注
                                                    goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                    goodsptList[i].isVip = true;
                                                    goodsptList[i].vtype = 7;
                                                    //goodsptList[i].isXG = true;

                                                }
                                                else
                                                {
                                                    //那么分拆
                                                    goodsptList[i].countNum -= numtemp;
                                                    goodsBuyList.Add(new GoodsBuy
                                                    {
                                                        spec = goodsptList[i].spec,
                                                        pinYin = goodsptList[i].pinYin,
                                                        unit = goodsptList[i].unit,
                                                        unitStr = goodsptList[i].unitStr,
                                                        barCodeTM = goodsptList[i].barCodeTM,
                                                        noCode = goodsptList[i].noCode,
                                                        countNum = numtemp,
                                                        goods = goodsptList[i].goods,
                                                        goodsDes = item.memo,
                                                        lsPrice = goodsptList[i].lsPrice,
                                                        hyPrice = Math.Round(item.ls_price.Value, 2),
                                                        jjPrice = item.yjj_price,
                                                        pfPrice = Math.Round(item.yls_price, 2),
                                                        isVip = true,
                                                        vtype = 7,
                                                    });

                                                }


                                            }

                                        }
                                    }

                                }
                                else
                                {
                                    //该会员没有消费记录

                                    //购物车中是否已经添加了活动商品，避免重复
                                    var ZSitems = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 7).FirstOrDefault();
                                    if (ZSitems != null)
                                    {
                                        if (ZSitems.countNum >= item.xg_amount)
                                        {
                                            continue;
                                        }
                                    }
                                    //购物车中符合活动的普通商品(不与其它活动重叠)
                                    var goodsptList = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 0).ToList();
                                    if (goodsptList.Count > 0)
                                    {
                                        for (int i = 0; i < goodsptList.Count; i++)
                                        {
                                            //那么来吧，互相伤害
                                            //如果限购
                                            if (item.xg_amount > 0)
                                            {
                                                if (goodsptList[i].countNum <= item.xg_amount)
                                                {
                                                    goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                    goodsptList[i].goodsDes = item.memo; //备注
                                                    goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                    goodsptList[i].isVip = true;
                                                    goodsptList[i].vtype = 7;

                                                }
                                                else
                                                {
                                                    //那么分拆
                                                    goodsptList[i].countNum -= item.xg_amount;
                                                    goodsBuyList.Add(new GoodsBuy
                                                    {
                                                        spec = goodsptList[i].spec,
                                                        pinYin = goodsptList[i].pinYin,
                                                        unit = goodsptList[i].unit,
                                                        unitStr = goodsptList[i].unitStr,
                                                        barCodeTM = goodsptList[i].barCodeTM,
                                                        noCode = goodsptList[i].noCode,
                                                        countNum = item.xg_amount,
                                                        goods = goodsptList[i].goods,
                                                        goodsDes = item.memo,
                                                        lsPrice = goodsptList[i].lsPrice,
                                                        hyPrice = Math.Round(item.ls_price.Value, 2),
                                                        jjPrice = item.yjj_price,
                                                        pfPrice = Math.Round(item.yls_price, 2),
                                                        isVip = true,
                                                        vtype = 7,

                                                    });

                                                }

                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 活动3 买多送多
        /// </summary>
        /// <param name="db"></param>
        private void YH3WSFunc(hjnbhEntities db)
        {

            int scode_temp = HandoverModel.GetInstance.scode;
            int VipID = HandoverModel.GetInstance.VipID;
            int viplv = HandoverModel.GetInstance.VipLv;

            //活动商品列表
            var YHInfo3 = db.v_yh_detail.AsNoTracking().Where(t => t.scode == scode_temp && t.vtype == 3).ToList();
            if (YHInfo3.Count > 0)
            {

                foreach (var item in YHInfo3)
                {
                    //先过滤一下，看购物车中是否有符合活动的普通商品
                    var HasHdItem = goodsBuyList.Where(t => t.vtype == 0 && t.noCode == item.item_id).FirstOrDefault();
                    if (HasHdItem == null) continue;

                    if (item.dx_type == 1)  //限定会员
                    {
                        if (VipID == 0) continue;
                        int viplvInfo = item.viptype;
                        //如果会员等级条件满足
                        if (viplv >= viplvInfo)
                        {
                            //符合活动的普通商品(不与其它活动重叠)
                            var goodsptList = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 0).ToList();
                            if (goodsptList.Count > 0)
                            {
                                for (int i = 0; i < goodsptList.Count; i++)
                                {
                                    //同名活动10
                                    var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).FirstOrDefault();
                                    //得判断有没有同名的活动10存在的情况
                                    if (YH10ZS != null)
                                    {
                                        //关联的数量,要有数量才有得搞
                                        if (YH10ZS.amount > 0)
                                        {


                                            //是否达到数量条件
                                            if (goodsptList[i].countNum < item.amount) continue;


                                            #region 商品单位、规格、拼音查询
                                            int zsunit5 = 0;
                                            var zsinfo100 = db.hd_item_info.AsNoTracking().Where(t => t.item_id == item.zs_item_id || t.tm == item.zstm).Select(t => new { t.unit, t.spec, t.py }).FirstOrDefault();
                                            if (zsinfo100 != null)
                                            {
                                                zsunit5 = zsinfo100.unit.HasValue ? (int)zsinfo100.unit.Value : 1;
                                            }
                                            //需要把单位编号转换为中文以便UI显示
                                            string dw_ = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == zsunit5).Select(t => t.txt1).FirstOrDefault();
                                            #endregion


                                            //判断活动商品数量 ， 进行分拆出来的保持原价的商品数量
                                            decimal sesu = goodsptList[i].countNum % item.amount;
                                            //赠送倍数
                                            decimal peisu = 1;
                                            if (item.amount != 0)
                                            {
                                                peisu = Math.Floor(goodsptList[i].countNum / item.amount);

                                            }

                                            if (YH10ZS.amount < item.zs_amount * peisu) continue;

                                            //没有的话直接送，并更正价格
                                            if (DialogResult.No == MessageBox.Show("此单 " + item.cname + " 满足买满赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                            {
                                                continue;
                                            }

                                            //分拆
                                            if (sesu == 0)
                                            {
                                                goodsptList[i].lsPrice = Math.Round(item.ls_price.Value / item.amount, 2);
                                                goodsptList[i].hyPrice = Math.Round(item.ls_price.Value / item.amount, 2);
                                                goodsptList[i].goodsDes = item.memo;
                                                goodsptList[i].jjPrice = Math.Round(item.yjj_price, 2);
                                                goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);  //记录原价
                                                goodsptList[i].isZS = true;
                                                goodsptList[i].isXG = true;
                                                goodsptList[i].vtype = 3;
                                            }
                                            else
                                            {
                                                //分拆
                                                goodsptList[i].countNum = sesu;
                                                goodsBuyList.Add(new GoodsBuy
                                                {
                                                    spec = goodsptList[i].spec,
                                                    pinYin = goodsptList[i].pinYin,
                                                    unit = goodsptList[i].unit,
                                                    unitStr = goodsptList[i].unitStr,
                                                    noCode = goodsptList[i].noCode,
                                                    barCodeTM = goodsptList[i].barCodeTM,
                                                    goods = goodsptList[i].goods,
                                                    countNum = item.amount * peisu,
                                                    lsPrice = Math.Round(item.ls_price.Value / item.amount, 2),
                                                    hyPrice = Math.Round(item.ls_price.Value / item.amount, 2),
                                                    goodsDes = item.memo,
                                                    jjPrice = Math.Round(item.yjj_price, 2),
                                                    pfPrice = Math.Round(item.yls_price, 2),  //记录原价
                                                    isZS = true,
                                                    isXG = true,
                                                    vtype = 3,
                                                    isVip = true

                                                });
                                            }




                                            //判断是否已存在赠品的商品
                                            var ZSitem = goodsBuyList.Where(t => t.noCode == item.zs_item_id && t.vtype == 0).FirstOrDefault();
                                            if (ZSitem != null)
                                            {
                                                //有的话要考虑分拆
                                                //处理赠品
                                                //赠品多，则原商品更正数量为余数，再新增活动赠品
                                                if (ZSitem.countNum > item.zs_amount * peisu)
                                                {
                                                    ZSitem.countNum -= item.zs_amount * peisu;
                                                    //添加赠品
                                                    goodsBuyList.Add(new GoodsBuy
                                                    {
                                                        spec = ZSitem.spec,
                                                        unit = ZSitem.unit,
                                                        unitStr = ZSitem.unitStr,
                                                        noCode = item.zs_item_id,
                                                        barCodeTM = item.zstm,
                                                        goods = item.zs_cname,
                                                        countNum = item.zs_amount * peisu,
                                                        lsPrice = Math.Round(item.zs_ylsprice, 2),
                                                        hyPrice = 0.00m,
                                                        goodsDes = item.memo,
                                                        jjPrice = Math.Round(item.zs_yjjprice, 2),
                                                        pfPrice = Math.Round(item.zs_ylsprice, 2),  //记录原价
                                                        isZS = true,
                                                        isXG = true,
                                                        vtype = 3,
                                                        isGL = true,
                                                        isVip = true

                                                    });

                                                }
                                                else
                                                {
                                                    ZSitem.countNum = item.zs_amount * peisu;
                                                    ZSitem.lsPrice = 0.00m;
                                                    ZSitem.hyPrice = 0.00m;
                                                    ZSitem.pfPrice = Math.Round(item.zs_ylsprice, 2);
                                                    ZSitem.vtype = 3;
                                                    ZSitem.isZS = true;
                                                    ZSitem.isXG = true;
                                                    ZSitem.isVip = true;
                                                    ZSitem.goodsDes = item.memo;
                                                    ZSitem.isGL = true;
                                                }



                                            }
                                            else
                                            {
                                                //添加赠品
                                                goodsBuyList.Add(new GoodsBuy
                                                {
                                                    unit = zsunit5,
                                                    unitStr = dw_,
                                                    noCode = item.zs_item_id,
                                                    barCodeTM = item.zstm,
                                                    goods = item.zs_cname,
                                                    countNum = item.zs_amount * peisu,
                                                    lsPrice = Math.Round(item.zs_ylsprice, 2),
                                                    hyPrice = 0.00m,
                                                    goodsDes = item.memo,
                                                    jjPrice = Math.Round(item.zs_yjjprice, 2),
                                                    pfPrice = Math.Round(item.zs_ylsprice, 2),  //记录原价
                                                    isZS = true,
                                                    isXG = true,
                                                    vtype = 3,
                                                    isGL = true,
                                                    isVip = true
                                                });
                                            }

                                        }


                                    }
                                    else
                                    {
                                        //没有关联

                                        //是否达到数量条件
                                        if (goodsptList[i].countNum < item.amount) continue;
                                        //没有的话直接送，并更正价格
                                        if (DialogResult.No == MessageBox.Show("此单 " + item.cname + " 满足买满赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            continue;
                                        }

                                        #region 商品单位、规格、拼音查询
                                        int zsunit5 = 0;
                                        var zsinfo100 = db.hd_item_info.AsNoTracking().Where(t => t.item_id == item.zs_item_id || t.tm == item.zstm).Select(t => new { t.unit, t.spec, t.py }).FirstOrDefault();
                                        if (zsinfo100 != null)
                                        {
                                            zsunit5 = zsinfo100.unit.HasValue ? (int)zsinfo100.unit.Value : 1;
                                        }
                                        //需要把单位编号转换为中文以便UI显示
                                        string dw_ = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == zsunit5).Select(t => t.txt1).FirstOrDefault();
                                        #endregion


                                        //判断活动商品数量 ， 进行分拆出来的保持原价的商品数量
                                        decimal sesu = goodsptList[i].countNum % item.amount;
                                        //赠送倍数
                                        decimal peisu = 1;
                                        if (item.amount != 0)
                                        {
                                            peisu = Math.Floor(goodsptList[i].countNum / item.amount);

                                        }

                                        if (sesu == 0)
                                        {
                                            goodsptList[i].lsPrice = Math.Round(item.ls_price.Value / item.amount, 2);
                                            goodsptList[i].hyPrice = Math.Round(item.ls_price.Value / item.amount, 2);
                                            goodsptList[i].goodsDes = item.memo;
                                            goodsptList[i].jjPrice = Math.Round(item.yjj_price, 2);
                                            goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);  //记录原价
                                            goodsptList[i].isZS = true;
                                            goodsptList[i].isXG = true;
                                            goodsptList[i].vtype = 3;
                                        }
                                        else
                                        {
                                            //分拆
                                            goodsptList[i].countNum = sesu;
                                            goodsBuyList.Add(new GoodsBuy
                                            {
                                                spec = goodsptList[i].spec,
                                                pinYin = goodsptList[i].pinYin,
                                                unit = goodsptList[i].unit,
                                                unitStr = goodsptList[i].unitStr,
                                                noCode = goodsptList[i].noCode,
                                                barCodeTM = goodsptList[i].barCodeTM,
                                                goods = goodsptList[i].goods,
                                                countNum = item.amount * peisu,
                                                lsPrice = Math.Round(item.ls_price.Value / item.amount, 2),
                                                hyPrice = Math.Round(item.ls_price.Value / item.amount, 2),
                                                goodsDes = item.memo,
                                                jjPrice = Math.Round(item.yjj_price, 2),
                                                pfPrice = Math.Round(item.yls_price, 2),  //记录原价
                                                isZS = true,
                                                isXG = true,
                                                vtype = 3,
                                                isVip = true
                                            });
                                        }


                                        //判断是否已存在赠品的商品
                                        var ZSitem = goodsBuyList.Where(t => t.noCode == item.zs_item_id && t.vtype == 0).FirstOrDefault();
                                        if (ZSitem != null)
                                        {
                                            //有的话要考虑分拆
                                            //处理赠品
                                            //赠品多，则原商品更正数量为余数，再新增活动赠品
                                            if (ZSitem.countNum > item.zs_amount * peisu)
                                            {
                                                ZSitem.countNum -= item.zs_amount * peisu;
                                                //添加赠品
                                                goodsBuyList.Add(new GoodsBuy
                                                {
                                                    spec = ZSitem.spec,
                                                    unit = ZSitem.unit,
                                                    unitStr = ZSitem.unitStr,
                                                    noCode = item.zs_item_id,
                                                    barCodeTM = item.zstm,
                                                    goods = item.zs_cname,
                                                    countNum = item.zs_amount * peisu,
                                                    lsPrice = Math.Round(item.zs_ylsprice, 2),
                                                    hyPrice = 0.00m,
                                                    goodsDes = item.memo,
                                                    jjPrice = Math.Round(item.zs_yjjprice, 2),
                                                    pfPrice = Math.Round(item.zs_ylsprice, 2),  //记录原价
                                                    isZS = true,
                                                    isXG = true,
                                                    vtype = 3,
                                                    isVip = true

                                                });

                                            }
                                            else
                                            {
                                                ZSitem.countNum = item.zs_amount * peisu;
                                                ZSitem.lsPrice = 0.00m;
                                                ZSitem.hyPrice = 0.00m;
                                                ZSitem.pfPrice = Math.Round(item.zs_ylsprice, 2);
                                                ZSitem.vtype = 3;
                                                ZSitem.isZS = true;
                                                ZSitem.isXG = true;
                                                ZSitem.isVip = true;
                                                ZSitem.goodsDes = item.memo;
                                            }



                                        }
                                        else
                                        {
                                            //添加赠品
                                            goodsBuyList.Add(new GoodsBuy
                                            {
                                                unit = zsunit5,
                                                unitStr = dw_,
                                                noCode = item.zs_item_id,
                                                barCodeTM = item.zstm,
                                                goods = item.zs_cname,
                                                countNum = item.zs_amount * peisu,
                                                lsPrice = Math.Round(item.zs_ylsprice, 2),
                                                hyPrice = 0.00m,
                                                goodsDes = item.memo,
                                                jjPrice = Math.Round(item.zs_yjjprice, 2),
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),  //记录原价
                                                isZS = true,
                                                isXG = true,
                                                vtype = 3,
                                                isVip = true
                                            });
                                        }
                                    }
                                }

                            }
                        }
                    }

                    #region 旧的
                    //        //判定购物车中是否有活动商品(未参与过活动的)
                    //        var HDitem = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype != 3).FirstOrDefault();
                    //        //如果有
                    //        if (HDitem != null)
                    //        {
                    //            //活动商品的数量是否足够
                    //            if (HDitem.countNum < item.amount) continue;

                    //            #region 商品单位、规格、拼音查询
                    //            int zsunit5 = 0;
                    //            var zsinfo100 = db.hd_item_info.AsNoTracking().Where(t => t.item_id == item.zs_item_id || t.tm == item.zstm).Select(t => new { t.unit, t.spec, t.py }).FirstOrDefault();
                    //            if (zsinfo100 != null)
                    //            {
                    //                zsunit5 = zsinfo100.unit.HasValue ? (int)zsinfo100.unit.Value : 1;
                    //            }
                    //            //需要把单位编号转换为中文以便UI显示
                    //            string dw_ = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == zsunit5).Select(t => t.txt1).FirstOrDefault();
                    //            #endregion

                    //            //没有的话直接送，并更正价格
                    //            if (DialogResult.OK == MessageBox.Show("此单 " + item.cname + " 满足买满赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.OKCancel))
                    //            {
                    //                //判断活动商品数量 ， 进行分拆出来的保持原价的商品数量
                    //                decimal sesu = HDitem.countNum % item.amount;
                    //                //赠送倍数
                    //                decimal peisu = 1;
                    //                if (item.amount != 0)
                    //                {
                    //                    peisu = Math.Floor(HDitem.countNum / item.amount);

                    //                }

                    //                //是否已经存在
                    //                var ZFitem = goodsBuyList.Where(t => t.vtype == 3 && t.noCode == item.item_id && t.isXG).FirstOrDefault();

                    //                if (HDitem.countNum > item.amount)
                    //                {


                    //                    //活动商品调整
                    //                    HDitem.countNum = sesu;

                    //                    //添加活动商品
                    //                    if (ZFitem != null)
                    //                    {
                    //                        ZFitem.countNum += Convert.ToInt32(item.amount * peisu);
                    //                    }
                    //                    else
                    //                    {
                    //                        var newhditem = new GoodsBuy
                    //                        {
                    //                            spec = HDitem.spec,
                    //                            pinYin = HDitem.pinYin,
                    //                            unit = HDitem.unit,
                    //                            unitStr = HDitem.unitStr,
                    //                            noCode = HDitem.noCode,
                    //                            barCodeTM = HDitem.barCodeTM,
                    //                            goods = HDitem.goods,
                    //                            countNum = Convert.ToInt32(item.amount * peisu),
                    //                            lsPrice = Math.Round(item.ls_price.Value / item.amount, 2),
                    //                            hyPrice = Math.Round(item.ls_price.Value / item.amount, 2),
                    //                            goodsDes = item.memo,
                    //                            jjPrice = HDitem.jjPrice,
                    //                            pfPrice = Math.Round(item.yls_price, 2),
                    //                            isXG = true,
                    //                            vtype = 3
                    //                        };
                    //                        goodsBuyList.Add(newhditem);
                    //                    }

                    //                }


                    //                else
                    //                {
                    //                    if (ZFitem != null)
                    //                    {
                    //                        ZFitem.countNum += Convert.ToInt32(item.amount * peisu);
                    //                        goodsBuyList.Remove(HDitem);
                    //                    }
                    //                    else
                    //                    {
                    //                        HDitem.lsPrice = Math.Round(item.ls_price.Value / item.amount, 2);
                    //                        HDitem.hyPrice = Math.Round(item.ls_price.Value / item.amount, 2);
                    //                        HDitem.pfPrice = Math.Round(item.yls_price, 2);
                    //                        HDitem.isXG = true;
                    //                        HDitem.vtype = 3;
                    //                        HDitem.goodsDes = item.memo;
                    //                    }
                    //                }


                    //                //再判断购物车中是否也存在赠品类型的商品
                    //                var ZSitem = goodsBuyList.Where(t => t.noCode == item.zs_item_id && t.vtype != 3).FirstOrDefault();
                    //                if (ZSitem != null)
                    //                {
                    //                    //有的话要考虑分拆
                    //                    //处理赠品

                    //                    //1、赠品少，则直接更正数量与价格
                    //                    if (ZSitem.countNum < item.zs_amount)
                    //                    {
                    //                        ZSitem.countNum = Convert.ToInt32(item.zs_amount * peisu);
                    //                        ZSitem.lsPrice = 0.00m;
                    //                        ZSitem.hyPrice = 0.00m;
                    //                        ZSitem.pfPrice = Math.Round(item.zs_ylsprice, 2);
                    //                        ZSitem.vtype = 3;
                    //                        ZSitem.isZS = true;
                    //                        ZSitem.isXG = true;

                    //                    }


                    //                    //2、赠品多，则原商品更正数量为余数，再新增活动赠品
                    //                    if (ZSitem.countNum > item.zs_amount)
                    //                    {
                    //                        //ZSitem.countNum = ZSitem.countNum % Convert.ToInt32(item.zs_amount.Value);
                    //                        ZSitem.countNum -= Convert.ToInt32(item.zs_amount * peisu);


                    //                        var ZFZSitem = goodsBuyList.Where(t => t.vtype == 3 && t.noCode == item.zs_item_id && t.isZS && t.isXG).FirstOrDefault();
                    //                        if (ZFZSitem != null)
                    //                        {
                    //                            ZFZSitem.countNum += Convert.ToInt32(item.zs_amount * peisu);
                    //                        }
                    //                        else
                    //                        {
                    //                            var zscho3 = new GoodsBuy
                    //                            {
                    //                                spec = zsinfo100.spec,
                    //                                pinYin = zsinfo100.py,
                    //                                unit = zsunit5,
                    //                                unitStr = dw_,
                    //                                noCode = item.zs_item_id,
                    //                                barCodeTM = item.zstm,
                    //                                goods = item.zs_cname,
                    //                                countNum = Convert.ToInt32(item.zs_amount * peisu),
                    //                                lsPrice = 0.00m,
                    //                                hyPrice = 0.00m,
                    //                                goodsDes = item.memo,
                    //                                jjPrice = item.zs_yjjprice,
                    //                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                    //                                isZS = true,
                    //                                isXG = true,
                    //                                vtype = 3
                    //                            };
                    //                            goodsBuyList.Add(zscho3);

                    //                        }
                    //                    }

                    //                    //3、赠品相等，则更正价格
                    //                    if (ZSitem.countNum == item.zs_amount)
                    //                    {
                    //                        ZSitem.lsPrice = 0.00m;
                    //                        ZSitem.hyPrice = 0.00m;
                    //                        ZSitem.pfPrice = Math.Round(item.zs_ylsprice, 2);
                    //                        ZSitem.vtype = 3;
                    //                        ZSitem.isZS = true;
                    //                        ZSitem.isXG = true;
                    //                    }

                    //                }
                    //                else
                    //                {

                    //                    var ZFZSitem = goodsBuyList.Where(t => t.vtype == 3 && t.noCode == item.zs_item_id && t.isZS && t.isXG).FirstOrDefault();
                    //                    if (ZFZSitem != null)
                    //                    {
                    //                        ZFZSitem.countNum += Convert.ToInt32(item.zs_amount * peisu);
                    //                    }
                    //                    else
                    //                    {
                    //                        var zscho3 = new GoodsBuy
                    //                        {
                    //                            spec = zsinfo100.spec,
                    //                            pinYin = zsinfo100.py,
                    //                            unit = zsunit5,
                    //                            unitStr = dw_,
                    //                            noCode = item.zs_item_id,
                    //                            barCodeTM = item.zstm,
                    //                            goods = item.zs_cname,
                    //                            countNum = Convert.ToInt32(item.zs_amount * peisu),
                    //                            lsPrice = 0.00m,
                    //                            hyPrice = 0.00m,
                    //                            goodsDes = item.memo,
                    //                            jjPrice = item.zs_yjjprice,
                    //                            pfPrice = Math.Round(item.zs_ylsprice, 2),
                    //                            isZS = true,
                    //                            isXG = true,
                    //                            vtype = 3
                    //                        };
                    //                        goodsBuyList.Add(zscho3);

                    //                    }


                    //                }

                    //            }

                    //        }
                    //    }
                    //}
                    #endregion


                    //所有对象
                    else if (item.dx_type == 0)
                    {
                        //符合活动的普通商品
                        var goodsptList = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 0).ToList();
                        if (goodsptList.Count > 0)
                        {
                            for (int i = 0; i < goodsptList.Count; i++)
                            {
                                //同名活动10
                                var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).FirstOrDefault();
                                //得判断有没有同名的活动10存在的情况
                                if (YH10ZS != null)
                                {
                                    //关联的数量,要有数量才有得搞
                                    if (YH10ZS.amount > 0)
                                    {

                                        //是否达到数量条件
                                        if (goodsptList[i].countNum < item.amount) continue;


                                        #region 商品单位、规格、拼音查询
                                        int zsunit5 = 0;
                                        var zsinfo100 = db.hd_item_info.AsNoTracking().Where(t => t.item_id == item.zs_item_id || t.tm == item.zstm).Select(t => new { t.unit, t.spec, t.py }).FirstOrDefault();
                                        if (zsinfo100 != null)
                                        {
                                            zsunit5 = zsinfo100.unit.HasValue ? (int)zsinfo100.unit.Value : 1;
                                        }
                                        //需要把单位编号转换为中文以便UI显示
                                        string dw_ = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == zsunit5).Select(t => t.txt1).FirstOrDefault();
                                        #endregion


                                        //判断活动商品数量 ， 进行分拆出来的保持原价的商品数量
                                        decimal sesu = goodsptList[i].countNum % item.amount;
                                        //赠送倍数
                                        decimal peisu = 1;
                                        if (item.amount != 0)
                                        {
                                            peisu = Math.Floor(goodsptList[i].countNum / item.amount);

                                        }

                                        if (YH10ZS.amount < item.zs_amount * peisu) continue;
                                        //没有的话直接送，并更正价格
                                        if (DialogResult.No == MessageBox.Show("此单 " + item.cname + " 满足买满赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            continue;
                                        }

                                        if (sesu == 0)
                                        {
                                            goodsptList[i].lsPrice = Math.Round(item.ls_price.Value / item.amount, 2);
                                            goodsptList[i].hyPrice = Math.Round(item.ls_price.Value / item.amount, 2);
                                            goodsptList[i].goodsDes = item.memo;
                                            goodsptList[i].jjPrice = Math.Round(item.yjj_price, 2);
                                            goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);  //记录原价
                                            goodsptList[i].isZS = true;
                                            goodsptList[i].isXG = true;
                                            goodsptList[i].vtype = 3;
                                        }
                                        else
                                        {
                                            //分拆
                                            goodsptList[i].countNum = sesu;
                                            goodsBuyList.Add(new GoodsBuy
                                            {
                                                spec = goodsptList[i].spec,
                                                pinYin = goodsptList[i].pinYin,
                                                unit = goodsptList[i].unit,
                                                unitStr = goodsptList[i].unitStr,
                                                noCode = goodsptList[i].noCode,
                                                barCodeTM = goodsptList[i].barCodeTM,
                                                goods = goodsptList[i].goods,
                                                countNum = item.amount * peisu,
                                                lsPrice = Math.Round(item.ls_price.Value / item.amount, 2),
                                                hyPrice = Math.Round(item.ls_price.Value / item.amount, 2),
                                                goodsDes = item.memo,
                                                jjPrice = Math.Round(item.yjj_price, 2),
                                                pfPrice = Math.Round(item.yls_price, 2),  //记录原价
                                                isZS = true,
                                                isXG = true,
                                                vtype = 3,
                                            });
                                        }


                                        //判断是否已存在赠品的商品
                                        var ZSitem = goodsBuyList.Where(t => t.noCode == item.zs_item_id && t.vtype == 0).FirstOrDefault();
                                        if (ZSitem != null)
                                        {
                                            //有的话要考虑分拆
                                            //处理赠品
                                            //赠品多，则原商品更正数量为余数，再新增活动赠品
                                            if (ZSitem.countNum > item.zs_amount * peisu)
                                            {
                                                ZSitem.countNum -= item.zs_amount * peisu;
                                                //添加赠品
                                                goodsBuyList.Add(new GoodsBuy
                                                {
                                                    spec = ZSitem.spec,
                                                    unit = ZSitem.unit,
                                                    unitStr = ZSitem.unitStr,
                                                    noCode = item.zs_item_id,
                                                    barCodeTM = item.zstm,
                                                    goods = item.zs_cname,
                                                    countNum = item.zs_amount * peisu,
                                                    lsPrice = 0.00m,
                                                    hyPrice = 0.00m,
                                                    goodsDes = item.memo,
                                                    jjPrice = Math.Round(item.zs_yjjprice, 2),
                                                    pfPrice = Math.Round(item.zs_ylsprice, 2),  //记录原价
                                                    isZS = true,
                                                    isXG = true,
                                                    vtype = 3,
                                                    isGL = true

                                                });

                                            }
                                            else
                                            {
                                                ZSitem.countNum = item.zs_amount * peisu;
                                                ZSitem.lsPrice = 0.00m;
                                                ZSitem.hyPrice = 0.00m;
                                                ZSitem.pfPrice = Math.Round(item.zs_ylsprice, 2);
                                                ZSitem.vtype = 3;
                                                ZSitem.isZS = true;
                                                ZSitem.isXG = true;
                                                ZSitem.goodsDes = item.memo;
                                                ZSitem.isGL = true;
                                            }



                                        }
                                        else
                                        {
                                            //添加赠品
                                            goodsBuyList.Add(new GoodsBuy
                                            {
                                                unit = zsunit5,
                                                unitStr = dw_,
                                                noCode = item.zs_item_id,
                                                barCodeTM = item.zstm,
                                                goods = item.zs_cname,
                                                countNum = item.zs_amount * peisu,
                                                lsPrice = 0.00m,
                                                hyPrice = 0.00m,
                                                goodsDes = item.memo,
                                                jjPrice = Math.Round(item.zs_yjjprice, 2),
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),  //记录原价
                                                isZS = true,
                                                isXG = true,
                                                vtype = 3,
                                                isGL = true
                                            });
                                        }

                                    }

                                }
                                else
                                {
                                    //没有活动关联

                                    //是否达到数量条件
                                    if (goodsptList[i].countNum < item.amount) continue;
                                    //没有的话直接送，并更正价格
                                    if (DialogResult.No == MessageBox.Show("此单 " + item.cname + " 满足买满赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                    {
                                        continue;
                                    }

                                    #region 商品单位、规格、拼音查询
                                    int zsunit5 = 0;
                                    var zsinfo100 = db.hd_item_info.AsNoTracking().Where(t => t.item_id == item.zs_item_id || t.tm == item.zstm).Select(t => new { t.unit, t.spec, t.py }).FirstOrDefault();
                                    if (zsinfo100 != null)
                                    {
                                        zsunit5 = zsinfo100.unit.HasValue ? (int)zsinfo100.unit.Value : 1;
                                    }
                                    //需要把单位编号转换为中文以便UI显示
                                    string dw_ = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == zsunit5).Select(t => t.txt1).FirstOrDefault();
                                    #endregion


                                    //判断活动商品数量 ， 进行分拆出来的保持原价的商品数量
                                    decimal sesu = goodsptList[i].countNum % item.amount;
                                    //赠送倍数
                                    decimal peisu = 1;
                                    if (item.amount != 0)
                                    {
                                        peisu = Math.Floor(goodsptList[i].countNum / item.amount);

                                    }

                                    if (sesu == 0)
                                    {
                                        goodsptList[i].lsPrice = Math.Round(item.ls_price.Value / item.amount, 2);
                                        goodsptList[i].hyPrice = Math.Round(item.ls_price.Value / item.amount, 2);
                                        goodsptList[i].goodsDes = item.memo;
                                        goodsptList[i].jjPrice = Math.Round(item.yjj_price, 2);
                                        goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);  //记录原价
                                        goodsptList[i].isZS = true;
                                        goodsptList[i].isXG = true;
                                        goodsptList[i].vtype = 3;
                                    }
                                    else
                                    {
                                        //分拆
                                        goodsptList[i].countNum = sesu;
                                        goodsBuyList.Add(new GoodsBuy
                                        {
                                            spec = goodsptList[i].spec,
                                            pinYin = goodsptList[i].pinYin,
                                            unit = goodsptList[i].unit,
                                            unitStr = goodsptList[i].unitStr,
                                            noCode = goodsptList[i].noCode,
                                            barCodeTM = goodsptList[i].barCodeTM,
                                            goods = goodsptList[i].goods,
                                            countNum = item.amount * peisu,
                                            lsPrice = Math.Round(item.ls_price.Value / item.amount, 2),
                                            hyPrice = Math.Round(item.ls_price.Value / item.amount, 2),
                                            goodsDes = item.memo,
                                            jjPrice = Math.Round(item.yjj_price, 2),
                                            pfPrice = Math.Round(item.yls_price, 2),  //记录原价
                                            isZS = true,
                                            isXG = true,
                                            vtype = 3,
                                        });
                                    }



                                    //判断是否已存在赠品的商品
                                    var ZSitem = goodsBuyList.Where(t => t.noCode == item.zs_item_id && t.vtype == 0).FirstOrDefault();
                                    if (ZSitem != null)
                                    {
                                        //有的话要考虑分拆
                                        //处理赠品
                                        //赠品多，则原商品更正数量为余数，再新增活动赠品
                                        if (ZSitem.countNum > item.zs_amount * peisu)
                                        {
                                            ZSitem.countNum -= item.zs_amount * peisu;
                                            //添加赠品
                                            goodsBuyList.Add(new GoodsBuy
                                            {
                                                spec = ZSitem.spec,
                                                unit = ZSitem.unit,
                                                unitStr = ZSitem.unitStr,
                                                noCode = item.zs_item_id,
                                                barCodeTM = item.zstm,
                                                goods = item.zs_cname,
                                                countNum = item.zs_amount * peisu,
                                                lsPrice = 0.00m,
                                                hyPrice = 0.00m,
                                                goodsDes = item.memo,
                                                jjPrice = Math.Round(item.zs_yjjprice, 2),
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),  //记录原价
                                                isZS = true,
                                                isXG = true,
                                                vtype = 3,
                                            });

                                        }
                                        else
                                        {
                                            ZSitem.countNum = item.zs_amount * peisu;
                                            ZSitem.lsPrice = 0.00m;
                                            ZSitem.hyPrice = 0.00m;
                                            ZSitem.pfPrice = Math.Round(item.zs_ylsprice, 2);
                                            ZSitem.vtype = 3;
                                            ZSitem.isZS = true;
                                            ZSitem.isXG = true;
                                            ZSitem.goodsDes = item.memo;

                                        }



                                    }
                                    else
                                    {
                                        //添加赠品
                                        goodsBuyList.Add(new GoodsBuy
                                        {
                                            unit = zsunit5,
                                            unitStr = dw_,
                                            noCode = item.zs_item_id,
                                            barCodeTM = item.zstm,
                                            goods = item.zs_cname,
                                            countNum = item.zs_amount * peisu,
                                            lsPrice = 0.00m,
                                            hyPrice = 0.00m,
                                            goodsDes = item.memo,
                                            jjPrice = Math.Round(item.zs_yjjprice, 2),
                                            pfPrice = Math.Round(item.zs_ylsprice, 2),  //记录原价
                                            isZS = true,
                                            isXG = true,
                                            vtype = 3,
                                        });
                                    }
                                }
                            }
                        }



                        #region 旧的
                        ////判定购物车中是否有活动商品(未参与过活动的)
                        //var HDitem = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype != 3).FirstOrDefault();
                        ////如果有
                        //if (HDitem != null)
                        //{
                        //    //活动商品的数量是否足够
                        //    if (HDitem.countNum < item.amount) continue;

                        //    #region 商品单位、规格、拼音查询
                        //    int zsunit5 = 0;
                        //    var zsinfo100 = db.hd_item_info.AsNoTracking().Where(t => t.item_id == item.zs_item_id || t.tm == item.zstm).Select(t => new { t.unit, t.spec, t.py }).FirstOrDefault();
                        //    if (zsinfo100 != null)
                        //    {
                        //        zsunit5 = zsinfo100.unit.HasValue ? (int)zsinfo100.unit.Value : 1;
                        //    }
                        //    //需要把单位编号转换为中文以便UI显示
                        //    string dw_ = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == zsunit5).Select(t => t.txt1).FirstOrDefault();
                        //    #endregion

                        //    //没有的话直接送，并更正价格
                        //    if (DialogResult.OK == MessageBox.Show("此单 " + item.cname + " 满足买满赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.OKCancel))
                        //    {
                        //        //判断活动商品数量 ， 进行分拆出来的保持原价的商品数量
                        //        decimal sesu = HDitem.countNum % item.amount;
                        //        //赠送倍数
                        //        decimal peisu = 1;
                        //        if (item.amount != 0)
                        //        {
                        //            peisu = Math.Floor(HDitem.countNum / item.amount);

                        //        }

                        //        //是否已经存在
                        //        var ZFitem = goodsBuyList.Where(t => t.vtype == 3 && t.noCode == item.item_id && t.isXG).FirstOrDefault();

                        //        if (HDitem.countNum > item.amount)
                        //        {

                        //            //活动商品调整
                        //            HDitem.countNum = sesu;

                        //            //添加活动商品
                        //            if (ZFitem != null)
                        //            {
                        //                ZFitem.countNum += Convert.ToInt32(item.amount * peisu);
                        //            }
                        //            else
                        //            {
                        //                var newhditem = new GoodsBuy
                        //                {
                        //                    spec = HDitem.spec,
                        //                    pinYin = HDitem.pinYin,
                        //                    unit = HDitem.unit,
                        //                    unitStr = HDitem.unitStr,
                        //                    noCode = HDitem.noCode,
                        //                    barCodeTM = HDitem.barCodeTM,
                        //                    goods = HDitem.goods,
                        //                    countNum = Convert.ToInt32(item.amount * peisu),
                        //                    lsPrice = Math.Round(item.ls_price.Value / item.amount, 2),
                        //                    hyPrice = Math.Round(item.ls_price.Value / item.amount, 2),
                        //                    goodsDes = item.memo,
                        //                    jjPrice = HDitem.jjPrice,
                        //                    pfPrice = Math.Round(item.yls_price, 2),
                        //                    isXG = true,
                        //                    vtype = 3
                        //                };
                        //                goodsBuyList.Add(newhditem);
                        //            }

                        //        }
                        //        else
                        //        {
                        //            if (ZFitem != null)
                        //            {
                        //                ZFitem.countNum += Convert.ToInt32(item.amount * peisu);
                        //                goodsBuyList.Remove(HDitem);
                        //            }
                        //            else
                        //            {
                        //                HDitem.lsPrice = Math.Round(item.ls_price.Value / item.amount, 2);
                        //                HDitem.hyPrice = Math.Round(item.ls_price.Value / item.amount, 2);
                        //                HDitem.pfPrice = Math.Round(item.yls_price, 2);
                        //                HDitem.isXG = true;
                        //                HDitem.vtype = 3;
                        //                HDitem.goodsDes = item.memo;
                        //            }
                        //        }


                        //        //再判断购物车中是否也存在赠品类型的商品
                        //        var ZSitem = goodsBuyList.Where(t => t.noCode == item.zs_item_id && t.vtype != 3).FirstOrDefault();
                        //        if (ZSitem != null)
                        //        {
                        //            //有的话要考虑分拆
                        //            //处理赠品

                        //            //1、赠品少，则直接更正数量与价格
                        //            if (ZSitem.countNum < item.zs_amount)
                        //            {
                        //                ZSitem.countNum = Convert.ToInt32(item.zs_amount * peisu);
                        //                ZSitem.lsPrice = 0.00m;
                        //                ZSitem.hyPrice = 0.00m;
                        //                ZSitem.pfPrice = Math.Round(item.zs_ylsprice, 2);
                        //                ZSitem.vtype = 3;
                        //                ZSitem.isZS = true;
                        //                ZSitem.isXG = true;

                        //            }


                        //            //2、赠品多，则原商品更正数量为余数，再新增活动赠品
                        //            if (ZSitem.countNum > item.zs_amount)
                        //            {
                        //                //ZSitem.countNum = ZSitem.countNum % Convert.ToInt32(item.zs_amount.Value);
                        //                ZSitem.countNum -= Convert.ToInt32(item.zs_amount * peisu);

                        //                var ZFZSitem = goodsBuyList.Where(t => t.vtype == 3 && t.noCode == item.zs_item_id && t.isZS && t.isXG).FirstOrDefault();
                        //                if (ZFZSitem != null)
                        //                {
                        //                    ZFZSitem.countNum += Convert.ToInt32(item.zs_amount * peisu);
                        //                }
                        //                else
                        //                {
                        //                    var zscho3 = new GoodsBuy
                        //                    {
                        //                        spec = zsinfo100.spec,
                        //                        pinYin = zsinfo100.py,
                        //                        unit = zsunit5,
                        //                        unitStr = dw_,
                        //                        noCode = item.zs_item_id,
                        //                        barCodeTM = item.zstm,
                        //                        goods = item.zs_cname,
                        //                        countNum = Convert.ToInt32(item.zs_amount * peisu),
                        //                        lsPrice = 0.00m,
                        //                        hyPrice = 0.00m,
                        //                        goodsDes = item.memo,
                        //                        jjPrice = item.zs_yjjprice,
                        //                        pfPrice = Math.Round(item.zs_ylsprice, 2),
                        //                        isZS = true,
                        //                        isXG = true,
                        //                        vtype = 3
                        //                    };
                        //                    goodsBuyList.Add(zscho3);

                        //                }
                        //            }

                        //            //3、赠品相等，则更正价格
                        //            if (ZSitem.countNum == item.zs_amount)
                        //            {
                        //                ZSitem.lsPrice = 0.00m;
                        //                ZSitem.hyPrice = 0.00m;
                        //                ZSitem.pfPrice = Math.Round(item.zs_ylsprice, 2);
                        //                ZSitem.vtype = 3;
                        //                ZSitem.isZS = true;
                        //                ZSitem.isXG = true;
                        //            }

                        //        }
                        //        else
                        //        {

                        //            var ZFZSitem = goodsBuyList.Where(t => t.vtype == 3 && t.noCode == item.zs_item_id && t.isZS && t.isXG).FirstOrDefault();
                        //            if (ZFZSitem != null)
                        //            {
                        //                ZFZSitem.countNum += Convert.ToInt32(item.zs_amount * peisu);
                        //            }
                        //            else
                        //            {
                        //                var zscho3 = new GoodsBuy
                        //                {
                        //                    spec = zsinfo100.spec,
                        //                    pinYin = zsinfo100.py,
                        //                    unit = zsunit5,
                        //                    unitStr = dw_,
                        //                    noCode = item.zs_item_id,
                        //                    barCodeTM = item.zstm,
                        //                    goods = item.zs_cname,
                        //                    countNum = Convert.ToInt32(item.zs_amount * peisu),
                        //                    lsPrice = 0.00m,
                        //                    hyPrice = 0.00m,
                        //                    goodsDes = item.memo,
                        //                    jjPrice = item.zs_yjjprice,
                        //                    pfPrice = Math.Round(item.zs_ylsprice, 2),
                        //                    isZS = true,
                        //                    isXG = true,
                        //                    vtype = 3
                        //                };
                        //                goodsBuyList.Add(zscho3);

                        //            }


                        //        }

                        //    }

                        //}


                        #endregion


                    }

                }

            }
        }

        /// <summary>
        /// 活动4 组合优惠
        /// </summary>
        /// <param name="db"></param>
        private void YH4ZHFunc(hjnbhEntities db)
        {
            int scode_temp = HandoverModel.GetInstance.scode;
            int VipID = HandoverModel.GetInstance.VipID;
            int viplv = HandoverModel.GetInstance.VipLv;
            //活动4商品列表
            var YHInfo4 = db.v_yh_detail.AsNoTracking().Where(t => t.scode == scode_temp && t.vtype == 4).ToList();
            if (YHInfo4.Count > 0)
            {
                foreach (var item in YHInfo4)
                {
                    //选判断是否有这两件商品
                    //判定购物车中是否有活动商品(未参与过活动的)
                    var HDitem = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 0).FirstOrDefault();
                    var ZSitem = goodsBuyList.Where(t => t.vtype == 0 && t.noCode == item.zs_item_id).FirstOrDefault();
                    if (HDitem == null || ZSitem == null) continue;
                    //数量是否到达最低要求
                    if (HDitem.countNum < item.amount || ZSitem.countNum < item.zs_amount) continue;

                    if (item.dx_type == 1)  //限定会员
                    {
                        if (VipID == 0) continue;
                        int viplvInfo = item.viptype;
                        //如果会员等级条件满足
                        if (viplv >= viplvInfo)
                        {

                            #region 商品单位、规格、拼音查询
                            int zsunit5 = 0;
                            var zsinfo100 = db.hd_item_info.AsNoTracking().Where(t => t.item_id == item.zs_item_id || t.tm == item.zstm).Select(t => new { t.unit, t.spec, t.py }).FirstOrDefault();
                            if (zsinfo100 != null)
                            {
                                zsunit5 = zsinfo100.unit.HasValue ? (int)zsinfo100.unit.Value : 1;
                            }
                            //需要把单位编号转换为中文以便UI显示
                            string dw_ = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == zsunit5).Select(t => t.txt1).FirstOrDefault();
                            #endregion

                            //没有的话直接送，并更正价格
                            if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 与 " + item.zs_cname + " 满足组合优惠活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                            {
                                //判断活动商品数量 ， 进行分拆出来的保持原价的商品数量
                                //decimal sesu = HDitem.countNum % item.amount;
                                //赠送倍数
                                //倍数
                                decimal PeiSu = 1;

                                //赠品的倍数
                                decimal ZSpeisu = 1;
                                if (item.zs_amount != 0)
                                {
                                    ZSpeisu = Math.Floor(ZSitem.countNum / item.zs_amount);
                                }


                                //商品倍数
                                decimal peisu = 1;
                                if (item.amount != 0)
                                {
                                    peisu = Math.Floor(HDitem.countNum / item.amount);
                                }

                                //需要赠品数量
                                decimal tempzsnum = item.zs_amount * peisu;
                                if (ZSitem.countNum < tempzsnum)
                                {
                                    PeiSu = ZSpeisu;
                                }
                                else
                                {
                                    PeiSu = peisu;
                                }


                                decimal HdtempNum = HDitem.countNum - item.amount * PeiSu;
                                //进行分拆
                                if (HdtempNum <= 0)
                                {
                                    HDitem.lsPrice = Math.Round(item.ls_price.Value / item.amount, 2);
                                    HDitem.hyPrice = Math.Round(item.ls_price.Value / item.amount, 2);
                                    HDitem.jjPrice = Math.Round(item.yjj_price, 2);
                                    HDitem.pfPrice = Math.Round(item.yls_price, 2);
                                    HDitem.goodsDes = item.memo;
                                    HDitem.vtype = 4;
                                    HDitem.isVip = true;
                                }
                                else
                                {
                                    HDitem.countNum -= item.amount * PeiSu;
                                    goodsBuyList.Add(new GoodsBuy
                                    {
                                        spec = HDitem.spec,
                                        pinYin = HDitem.pinYin,
                                        unit = HDitem.unit,
                                        unitStr = HDitem.unitStr,
                                        noCode = HDitem.noCode,
                                        barCodeTM = HDitem.barCodeTM,
                                        goods = HDitem.goods,
                                        countNum = item.amount * PeiSu,
                                        lsPrice = Math.Round(item.ls_price.Value / item.amount, 2),
                                        hyPrice = Math.Round(item.ls_price.Value / item.amount, 2),
                                        goodsDes = item.memo,
                                        jjPrice = HDitem.jjPrice,
                                        pfPrice = Math.Round(item.yls_price, 2),
                                        isXG = true,
                                        vtype = 4,
                                        isVip = true

                                    });
                                }

                                //判断活动商品数量 ， 进行分拆出来的保持原价的商品数量
                                decimal ZSsesu = ZSitem.countNum - item.zs_amount * PeiSu;

                                if (ZSsesu <= 0)
                                {
                                    ZSitem.lsPrice = 0.00m;
                                    ZSitem.hyPrice = 0.00m;
                                    ZSitem.jjPrice = Math.Round(item.zs_yjjprice, 2);
                                    ZSitem.pfPrice = Math.Round(item.zs_ylsprice, 2);
                                    ZSitem.goodsDes = item.memo;
                                    ZSitem.vtype = 4;
                                    ZSitem.isVip = true;
                                }
                                else
                                {
                                    ZSitem.countNum -= item.zs_amount * PeiSu;
                                    goodsBuyList.Add(new GoodsBuy
                                    {
                                        spec = ZSitem.spec,
                                        pinYin = ZSitem.pinYin,
                                        unit = ZSitem.unit,
                                        unitStr = ZSitem.unitStr,
                                        noCode = ZSitem.noCode,
                                        barCodeTM = ZSitem.barCodeTM,
                                        goods = ZSitem.goods,
                                        countNum = item.zs_amount * PeiSu,
                                        lsPrice = 0.00m,
                                        hyPrice = 0.00m,
                                        goodsDes = item.memo,
                                        jjPrice = ZSitem.jjPrice,
                                        pfPrice = Math.Round(item.zs_ylsprice, 2),
                                        isXG = true,
                                        vtype = 4,
                                        isVip = true

                                    });

                                }

                            }
                        }
                    }

                    #region 旧的

                    ////是否已经存在
                    //var ZFitem = goodsBuyList.Where(t => t.vtype == 4 && t.noCode == item.item_id && t.isXG).FirstOrDefault();

                                //if (HDitem.countNum > item.amount)
                    //{
                    //    if (sesu == 0)
                    //    {
                    //        //活动商品调整
                    //        HDitem.countNum = 1;
                    //    }
                    //    else
                    //    {
                    //        //活动商品调整
                    //        HDitem.countNum = sesu;
                    //    }


                                //    //添加活动商品
                    //    if (ZFitem != null)
                    //    {
                    //        ZFitem.countNum += Convert.ToInt32(item.amount * peisu);
                    //    }
                    //    else
                    //    {
                    //        var newhditem = new GoodsBuy
                    //        {
                    //            spec = HDitem.spec,
                    //            pinYin = HDitem.pinYin,
                    //            unit = HDitem.unit,
                    //            unitStr = HDitem.unitStr,
                    //            noCode = HDitem.noCode,
                    //            barCodeTM = HDitem.barCodeTM,
                    //            goods = HDitem.goods,
                    //            countNum = Convert.ToInt32(item.amount * peisu),
                    //            lsPrice = Math.Round(item.ls_price.Value / item.amount, 2),
                    //            hyPrice = Math.Round(item.ls_price.Value / item.amount, 2),
                    //            goodsDes = item.memo,
                    //            jjPrice = HDitem.jjPrice,
                    //            pfPrice = Math.Round(item.yls_price, 2),
                    //            isXG = true,
                    //            vtype = 4
                    //        };
                    //        goodsBuyList.Add(newhditem);
                    //    }

                                //}
                    //else
                    //{
                    //    if (ZFitem != null)
                    //    {
                    //        ZFitem.countNum += Convert.ToInt32(item.amount * peisu);
                    //        goodsBuyList.Remove(HDitem);
                    //    }
                    //    else
                    //    {
                    //        HDitem.lsPrice = Math.Round(item.ls_price.Value / item.amount, 2);
                    //        HDitem.hyPrice = Math.Round(item.ls_price.Value / item.amount, 2);
                    //        HDitem.pfPrice = Math.Round(item.yls_price, 2);
                    //        HDitem.isXG = true;
                    //        HDitem.vtype = 4;
                    //        HDitem.goodsDes = item.memo;
                    //    }
                    //}

                                ////判断活动商品数量 ， 进行分拆出来的保持原价的商品数量
                    //decimal ZSsesu = ZSitem.countNum % item.zs_amount;

                                //var ZSZFitem = goodsBuyList.Where(t => t.vtype == 4 && t.noCode == item.zs_item_id && t.isXG).FirstOrDefault();

                                //if (ZSitem.countNum > item.zs_amount)
                    //{
                    //    if (ZSsesu == 0)
                    //    {
                    //        //活动商品调整
                    //        ZSitem.countNum = 1;
                    //    }
                    //    else
                    //    {
                    //        //活动商品调整
                    //        ZSitem.countNum = ZSsesu;
                    //    }

                                //    //添加活动商品
                    //    if (ZSZFitem != null)
                    //    {
                    //        ZSZFitem.countNum += Convert.ToInt32(item.zs_amount * peisu);
                    //    }
                    //    else
                    //    {
                    //        var newhditem = new GoodsBuy
                    //        {
                    //            spec = ZSitem.spec,
                    //            pinYin = ZSitem.pinYin,
                    //            unit = ZSitem.unit,
                    //            unitStr = ZSitem.unitStr,
                    //            noCode = ZSitem.noCode,
                    //            barCodeTM = ZSitem.barCodeTM,
                    //            goods = ZSitem.goods,
                    //            countNum = Convert.ToInt32(item.zs_amount * peisu),
                    //            lsPrice = 0.00m,
                    //            hyPrice = 0.00m,
                    //            goodsDes = item.memo,
                    //            jjPrice = HDitem.jjPrice,
                    //            pfPrice = Math.Round(item.zs_ylsprice, 2),
                    //            isXG = true,
                    //            vtype = 4
                    //        };
                    //        goodsBuyList.Add(newhditem);
                    //    }

                                //}

                    //            else
                    //            {
                    //                if (ZSZFitem != null)
                    //                {
                    //                    ZSZFitem.countNum += Convert.ToInt32(item.zs_amount * peisu);
                    //                    goodsBuyList.Remove(ZSitem);
                    //                }
                    //                else
                    //                {
                    //                    ZSitem.lsPrice = 0.00m;
                    //                    ZSitem.hyPrice = 0.00m;
                    //                    ZSitem.pfPrice = Math.Round(item.zs_ylsprice, 2);
                    //                    ZSitem.isXG = true;
                    //                    ZSitem.vtype = 4;
                    //                    ZSitem.goodsDes = item.memo;
                    //                }
                    //            }

                    //        }
                    //    }
                    //}
                    #endregion

                    //所有对象
                    else if (item.dx_type == 0)
                    {

                        #region 商品单位、规格、拼音查询
                        int zsunit5 = 0;
                        var zsinfo100 = db.hd_item_info.AsNoTracking().Where(t => t.item_id == item.zs_item_id || t.tm == item.zstm).Select(t => new { t.unit, t.spec, t.py }).FirstOrDefault();
                        if (zsinfo100 != null)
                        {
                            zsunit5 = zsinfo100.unit.HasValue ? (int)zsinfo100.unit.Value : 1;
                        }
                        //需要把单位编号转换为中文以便UI显示
                        string dw_ = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == zsunit5).Select(t => t.txt1).FirstOrDefault();
                        #endregion

                        //没有的话直接送，并更正价格
                        if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 与 " + item.zs_cname + " 满足组合优惠活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                        {
                            //判断活动商品数量 ， 进行分拆出来的保持原价的商品数量
                            //decimal sesu = HDitem.countNum % item.amount;
                            //倍数
                            decimal PeiSu = 1;

                            //赠品的倍数
                            decimal ZSpeisu = 1;
                            if (item.zs_amount != 0)
                            {
                                ZSpeisu = Math.Floor(ZSitem.countNum / item.zs_amount);
                            }


                            //商品倍数
                            decimal peisu = 1;
                            if (item.amount != 0)
                            {
                                peisu = Math.Floor(HDitem.countNum / item.amount);
                            }

                            //需要赠品数量
                            decimal tempzsnum = item.zs_amount * peisu;
                            if (ZSitem.countNum < tempzsnum)
                            {
                                PeiSu = ZSpeisu;
                            }
                            else
                            {
                                PeiSu = peisu;
                            }


                            decimal HdtempNum = HDitem.countNum - item.amount * PeiSu;

                            //进行分拆
                            if (HdtempNum == 0)
                            {
                                HDitem.lsPrice = Math.Round(item.ls_price.Value / item.amount, 2);
                                HDitem.hyPrice = Math.Round(item.ls_price.Value / item.amount, 2);
                                HDitem.jjPrice = Math.Round(item.yjj_price, 2);
                                HDitem.pfPrice = Math.Round(item.yls_price, 2);
                                HDitem.goodsDes = item.memo;
                                HDitem.vtype = 4;
                            }
                            else if (HdtempNum > 0)
                            {
                                HDitem.countNum -= item.amount * PeiSu;
                                goodsBuyList.Add(new GoodsBuy
                                {
                                    spec = HDitem.spec,
                                    pinYin = HDitem.pinYin,
                                    unit = HDitem.unit,
                                    unitStr = HDitem.unitStr,
                                    noCode = HDitem.noCode,
                                    barCodeTM = HDitem.barCodeTM,
                                    goods = HDitem.goods,
                                    countNum = item.amount * PeiSu,
                                    lsPrice = Math.Round(item.ls_price.Value / item.amount, 2),
                                    hyPrice = Math.Round(item.ls_price.Value / item.amount, 2),
                                    goodsDes = item.memo,
                                    jjPrice = HDitem.jjPrice,
                                    pfPrice = Math.Round(item.yls_price, 2),
                                    isXG = true,
                                    vtype = 4,
                                });
                            }

                            //判断活动商品数量 ， 进行分拆出来的保持原价的商品数量
                            decimal ZStempNum = ZSitem.countNum - item.zs_amount * PeiSu;

                            if (ZStempNum == 0)
                            {
                                ZSitem.lsPrice = 0.00m;
                                ZSitem.hyPrice = 0.00m;
                                ZSitem.jjPrice = Math.Round(item.zs_yjjprice, 2);
                                ZSitem.pfPrice = Math.Round(item.zs_ylsprice, 2);
                                ZSitem.goodsDes = item.memo;
                                ZSitem.vtype = 4;
                                ZSitem.isVip = true;
                            }
                            else if (ZStempNum > 0)
                            {
                                ZSitem.countNum -= item.zs_amount * PeiSu;
                                goodsBuyList.Add(new GoodsBuy
                                {
                                    spec = ZSitem.spec,
                                    pinYin = ZSitem.pinYin,
                                    unit = ZSitem.unit,
                                    unitStr = ZSitem.unitStr,
                                    noCode = ZSitem.noCode,
                                    barCodeTM = ZSitem.barCodeTM,
                                    goods = ZSitem.goods,
                                    countNum = item.zs_amount * PeiSu,
                                    lsPrice = 0.00m,
                                    hyPrice = 0.00m,
                                    goodsDes = item.memo,
                                    jjPrice = ZSitem.jjPrice,
                                    pfPrice = Math.Round(item.zs_ylsprice, 2),
                                    isXG = true,
                                    vtype = 4,
                                });

                            }

                        }

                        #region 旧的


                        //#region 商品单位、规格、拼音查询
                        //int zsunit5 = 0;
                        //var zsinfo100 = db.hd_item_info.AsNoTracking().Where(t => t.item_id == item.zs_item_id || t.tm == item.zstm).Select(t => new { t.unit, t.spec, t.py }).FirstOrDefault();
                        //if (zsinfo100 != null)
                        //{
                        //    zsunit5 = zsinfo100.unit.HasValue ? (int)zsinfo100.unit.Value : 1;
                        //}
                        ////需要把单位编号转换为中文以便UI显示
                        //string dw_ = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == zsunit5).Select(t => t.txt1).FirstOrDefault();
                        //#endregion

                        ////没有的话直接送，并更正价格
                        //if (DialogResult.OK == MessageBox.Show("此单 " + item.cname + " 与 " + item.zs_cname + " 满足组合优惠活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.OKCancel))
                        //{
                        //    //判断活动商品数量 ， 进行分拆出来的保持原价的商品数量
                        //    decimal sesu = HDitem.countNum % item.amount;
                        //    //赠送倍数
                        //    decimal peisu = 1;
                        //    if (item.amount != 0)
                        //    {
                        //        peisu = Math.Floor(HDitem.countNum / item.amount);

                        //    }

                        //    //是否已经存在
                        //    var ZFitem = goodsBuyList.Where(t => t.vtype == 4 && t.noCode == item.item_id && t.isXG).FirstOrDefault();

                        //    if (HDitem.countNum > item.amount)
                        //    {
                        //        //if (sesu == 0)
                        //        //{
                        //        //    //活动商品调整
                        //        //    HDitem.countNum = 1;
                        //        //}
                        //        //else
                        //        //{
                        //        //    //活动商品调整
                        //        //    HDitem.countNum = sesu;
                        //        //}

                        //        //活动商品调整
                        //        HDitem.countNum = sesu;

                        //        //添加活动商品
                        //        if (ZFitem != null)
                        //        {
                        //            ZFitem.countNum += Convert.ToInt32(item.amount * peisu);
                        //        }
                        //        else
                        //        {
                        //            var newhditem = new GoodsBuy
                        //            {
                        //                spec = HDitem.spec,
                        //                pinYin = HDitem.pinYin,
                        //                unit = HDitem.unit,
                        //                unitStr = HDitem.unitStr,
                        //                noCode = HDitem.noCode,
                        //                barCodeTM = HDitem.barCodeTM,
                        //                goods = HDitem.goods,
                        //                countNum = Convert.ToInt32(item.amount * peisu),
                        //                lsPrice = Math.Round(item.ls_price.Value / item.amount, 2),
                        //                hyPrice = Math.Round(item.ls_price.Value / item.amount, 2),
                        //                goodsDes = item.memo,
                        //                jjPrice = HDitem.jjPrice,
                        //                pfPrice = Math.Round(item.yls_price, 2),
                        //                isXG = true,
                        //                vtype = 4
                        //            };
                        //            goodsBuyList.Add(newhditem);
                        //        }

                        //    }
                        //    else
                        //    {
                        //        if (ZFitem != null)
                        //        {
                        //            ZFitem.countNum += Convert.ToInt32(item.amount * peisu);
                        //            goodsBuyList.Remove(HDitem);
                        //        }
                        //        else
                        //        {
                        //            HDitem.lsPrice = Math.Round(item.ls_price.Value / item.amount, 2);
                        //            HDitem.hyPrice = Math.Round(item.ls_price.Value / item.amount, 2);
                        //            HDitem.pfPrice = Math.Round(item.yls_price, 2);
                        //            HDitem.isXG = true;
                        //            HDitem.vtype = 4;
                        //            HDitem.goodsDes = item.memo;
                        //        }
                        //    }


                        //    //判断活动商品数量 ， 进行分拆出来的保持原价的商品数量
                        //    decimal ZSsesu = ZSitem.countNum % item.zs_amount;

                        //    var ZSZFitem = goodsBuyList.Where(t => t.vtype == 4 && t.noCode == item.zs_item_id && t.isXG).FirstOrDefault();

                        //    if (ZSitem.countNum > item.zs_amount)
                        //    {
                        //        //if (ZSsesu == 0)
                        //        //{
                        //        //    //活动商品调整
                        //        //    ZSitem.countNum = 1;
                        //        //}
                        //        //else
                        //        //{
                        //        //    //活动商品调整
                        //        //    ZSitem.countNum = ZSsesu;
                        //        //}

                        //        //活动商品调整
                        //        ZSitem.countNum = ZSsesu;

                        //        //添加活动商品
                        //        if (ZSZFitem != null)
                        //        {
                        //            ZSZFitem.countNum += Convert.ToInt32(item.zs_amount * peisu);
                        //        }
                        //        else
                        //        {
                        //            var newhditem = new GoodsBuy
                        //            {
                        //                spec = ZSitem.spec,
                        //                pinYin = ZSitem.pinYin,
                        //                unit = ZSitem.unit,
                        //                unitStr = ZSitem.unitStr,
                        //                noCode = ZSitem.noCode,
                        //                barCodeTM = ZSitem.barCodeTM,
                        //                goods = ZSitem.goods,
                        //                countNum = Convert.ToInt32(item.zs_amount * peisu),
                        //                lsPrice = 0.00m,
                        //                hyPrice = 0.00m,
                        //                goodsDes = item.memo,
                        //                jjPrice = HDitem.jjPrice,
                        //                pfPrice = Math.Round(item.zs_ylsprice, 2),
                        //                isXG = true,
                        //                vtype = 4
                        //            };
                        //            goodsBuyList.Add(newhditem);
                        //        }

                        //    }
                        //    else
                        //    {
                        //        if (ZSZFitem != null)
                        //        {
                        //            ZSZFitem.countNum += Convert.ToInt32(item.zs_amount * peisu);
                        //            goodsBuyList.Remove(ZSitem);
                        //        }
                        //        else
                        //        {
                        //            ZSitem.lsPrice = 0.00m;
                        //            ZSitem.hyPrice = 0.00m;
                        //            ZSitem.pfPrice = Math.Round(item.zs_ylsprice, 2);
                        //            ZSitem.isXG = true;
                        //            ZSitem.vtype = 4;
                        //            ZSitem.goodsDes = item.memo;
                        //        }
                        //    }

                        //}

                        //    }
                        //}

                        //for (int i = 0; i < goodsBuyList.Count; i++)
                        //{
                        //    if (goodsBuyList[i].countNum == 0)
                        //    {
                        //        goodsBuyList.RemoveAt(i);
                        //        //goodsBuyList[i].countNum = 1;
                        //    }
                        #endregion

                    }
                }

            }

        }



        ChoiceGoods cho5 = new ChoiceGoods();
        /// <summary>
        /// 活动5 满额赠送
        /// </summary>
        /// <param name="db"></param>
        private void YH5WEFunc(hjnbhEntities db)
        {
            #region (活动5)主要判断合计金额是否满额就行，不要判断买了什么

            var item5 = goodsBuyList.Where(e => e.vtype == 5 && e.isZS).FirstOrDefault();
            if (item5 != null) return; //如果有赠品就不再参与
            bool istip = false; //防止重复提醒
            int scode_temp = HandoverModel.GetInstance.scode;
            int VipID = HandoverModel.GetInstance.VipID;
            int viplv = HandoverModel.GetInstance.VipLv;

            decimal sum_temp5 = goodsBuyList.Select(t => t.Sum.Value).Sum();  //目前总额
            //活动商品列表
            var YHInfo5 = db.v_yh_detail.AsNoTracking().Where(t => t.zjmoney <= sum_temp5 && t.scode == scode_temp && t.vtype == 5).ToList();
            if (YHInfo5.Count > 0)
            {
                foreach (var item in YHInfo5)
                {

                    #region 商品单位、规格、拼音查询
                    int zsunit5 = 0;
                    var zsinfo100 = db.hd_item_info.AsNoTracking().Where(t => t.item_id == item.zs_item_id || t.tm == item.zstm).Select(t => new { t.unit, t.spec, t.py }).FirstOrDefault();
                    if (zsinfo100 != null)
                    {
                        zsunit5 = zsinfo100.unit.HasValue ? (int)zsinfo100.unit.Value : 1;
                    }
                    //需要把单位编号转换为中文以便UI显示
                    string dw_ = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == zsunit5).Select(t => t.txt1).FirstOrDefault();
                    #endregion


                    if (item.dx_type == 1)  //限定会员
                    {
                        if (VipID == 0) continue;
                        int viplvInfo = item.viptype;
                        //如果会员等级条件满足
                        if (viplv >= viplvInfo)
                        {
                            //赠送的数量
                            decimal ZScount = 1.00m;
                            //同名活动10
                            var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).FirstOrDefault();
                            //得判断有没有同名的活动10存在的情况
                            if (YH10ZS != null)
                            {
                                if (YH10ZS.amount > 0)
                                {
                                    //关联的数量,要有数量才有得搞
                                    if (YH10ZS.amount < item.zs_amount)
                                    {
                                        ZScount = YH10ZS.amount;
                                    }
                                    else
                                    {
                                        ZScount = item.zs_amount;
                                    }

                                    if (istip)
                                    {
                                        cho5.ChooseList.Add(new GoodsBuy
                                        {
                                            spec = zsinfo100.spec,
                                            pinYin = zsinfo100.py,
                                            unit = zsunit5,
                                            unitStr = dw_,
                                            noCode = item.zs_item_id,
                                            barCodeTM = item.zstm,
                                            goods = item.zs_cname,
                                            countNum = ZScount,
                                            lsPrice = Math.Round(item.zs_ylsprice, 2),
                                            hyPrice = Math.Round((item.zsmoney / item.zs_amount).Value, 2),
                                            goodsDes = item.memo,
                                            jjPrice = Math.Round(item.zs_yjjprice, 2),
                                            pfPrice = Math.Round(item.zs_ylsprice, 2),  //记录原价
                                            isVip = true,
                                            isZS = true,
                                            isXG = true,
                                            vtype = 5,
                                            isGL = true
                                        });
                                    }
                                    else
                                    {
                                        if (DialogResult.Yes == MessageBox.Show("此单满足满额加价赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            cho5.ChooseList.Add(new GoodsBuy
                                            {
                                                spec = zsinfo100.spec,
                                                pinYin = zsinfo100.py,
                                                unit = zsunit5,
                                                unitStr = dw_,
                                                noCode = item.zs_item_id,
                                                barCodeTM = item.zstm,
                                                goods = item.zs_cname,
                                                countNum = ZScount,
                                                lsPrice = Math.Round(item.zs_ylsprice, 2),
                                                hyPrice = Math.Round((item.zsmoney / item.zs_amount).Value, 2),
                                                goodsDes = item.memo,
                                                jjPrice = Math.Round(item.zs_yjjprice, 2),
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),  //记录原价
                                                isVip = true,
                                                isZS = true,
                                                isXG = true,
                                                vtype = 5,
                                                isGL = true
                                            });

                                            istip = true;
                                        }
                                    }


                                }



                            }
                            else
                            {
                                //没有关联的活动

                                if (istip)
                                {
                                    cho5.ChooseList.Add(new GoodsBuy
                                    {
                                        spec = zsinfo100.spec,
                                        pinYin = zsinfo100.py,
                                        unit = zsunit5,
                                        unitStr = dw_,
                                        noCode = item.zs_item_id,
                                        barCodeTM = item.zstm,
                                        goods = item.zs_cname,
                                        countNum = item.zs_amount,
                                        lsPrice = Math.Round(item.zs_ylsprice, 2),
                                        hyPrice = Math.Round((item.zsmoney / item.zs_amount).Value, 2),
                                        goodsDes = item.memo,
                                        jjPrice = Math.Round(item.zs_yjjprice, 2),
                                        pfPrice = Math.Round(item.zs_ylsprice, 2),  //记录原价
                                        isVip = true,
                                        isZS = true,
                                        isXG = true,
                                        vtype = 5
                                    });
                                }
                                else
                                {
                                    if (DialogResult.Yes == MessageBox.Show("此单满足满额加价赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                    {
                                        cho5.ChooseList.Add(new GoodsBuy
                                        {
                                            spec = zsinfo100.spec,
                                            pinYin = zsinfo100.py,
                                            unit = zsunit5,
                                            unitStr = dw_,
                                            noCode = item.zs_item_id,
                                            barCodeTM = item.zstm,
                                            goods = item.zs_cname,
                                            countNum = item.zs_amount,
                                            lsPrice = Math.Round(item.zs_ylsprice, 2),
                                            hyPrice = Math.Round((item.zsmoney / item.zs_amount).Value, 2),
                                            goodsDes = item.memo,
                                            jjPrice = Math.Round(item.zs_yjjprice, 2),
                                            pfPrice = Math.Round(item.zs_ylsprice, 2),  //记录原价
                                            isVip = true,
                                            isZS = true,
                                            isXG = true,
                                            vtype = 5
                                        });

                                        istip = true;
                                    }
                                }


                            }
                        }

                    }    //所有对象
                    else if (item.dx_type == 0)
                    {
                        //赠送的数量
                        decimal ZScount = 1.00m;
                        //同名活动10
                        var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).FirstOrDefault();
                        //得判断有没有同名的活动10存在的情况
                        if (YH10ZS != null)
                        {
                            if (YH10ZS.amount > 0)
                            {
                                //关联的数量,要有数量才有得搞
                                if (YH10ZS.amount < item.zs_amount)
                                {
                                    ZScount = YH10ZS.amount;
                                }
                                else
                                {
                                    ZScount = item.zs_amount;
                                }

                                if (istip)
                                {
                                    cho5.ChooseList.Add(new GoodsBuy
                                    {
                                        spec = zsinfo100.spec,
                                        pinYin = zsinfo100.py,
                                        unit = zsunit5,
                                        unitStr = dw_,
                                        noCode = item.zs_item_id,
                                        barCodeTM = item.zstm,
                                        goods = item.zs_cname,
                                        countNum = ZScount,
                                        lsPrice = Math.Round((item.zsmoney / item.zs_amount).Value, 2),
                                        hyPrice = Math.Round((item.zsmoney / item.zs_amount).Value, 2),
                                        goodsDes = item.memo,
                                        jjPrice = Math.Round(item.zs_yjjprice, 2),
                                        pfPrice = Math.Round(item.zs_ylsprice, 2),  //记录原价
                                        isZS = true,
                                        isXG = true,
                                        vtype = 5,
                                        isGL = true
                                    });
                                }
                                else
                                {
                                    if (DialogResult.Yes == MessageBox.Show("此单满足满额加价赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                    {
                                        cho5.ChooseList.Add(new GoodsBuy
                                        {
                                            spec = zsinfo100.spec,
                                            pinYin = zsinfo100.py,
                                            unit = zsunit5,
                                            unitStr = dw_,
                                            noCode = item.zs_item_id,
                                            barCodeTM = item.zstm,
                                            goods = item.zs_cname,
                                            countNum = ZScount,
                                            lsPrice = Math.Round((item.zsmoney / item.zs_amount).Value, 2),
                                            hyPrice = Math.Round((item.zsmoney / item.zs_amount).Value, 2),
                                            goodsDes = item.memo,
                                            jjPrice = Math.Round(item.zs_yjjprice, 2),
                                            pfPrice = Math.Round(item.zs_ylsprice, 2),  //记录原价
                                            isZS = true,
                                            isXG = true,
                                            vtype = 5,
                                            isGL = true
                                        });

                                        istip = true;
                                    }
                                }


                            }

                        }
                        else
                        {
                            //没有关联的活动

                            if (istip)
                            {
                                cho5.ChooseList.Add(new GoodsBuy
                                {
                                    spec = zsinfo100.spec,
                                    pinYin = zsinfo100.py,
                                    unit = zsunit5,
                                    unitStr = dw_,
                                    noCode = item.zs_item_id,
                                    barCodeTM = item.zstm,
                                    goods = item.zs_cname,
                                    countNum = item.zs_amount,
                                    lsPrice = Math.Round((item.zsmoney / item.zs_amount).Value, 2),
                                    hyPrice = Math.Round((item.zsmoney / item.zs_amount).Value, 2),
                                    goodsDes = item.memo,
                                    jjPrice = Math.Round(item.zs_yjjprice, 2),
                                    pfPrice = Math.Round(item.zs_ylsprice, 2),  //记录原价
                                    isZS = true,
                                    isXG = true,
                                    vtype = 5
                                });
                            }
                            else
                            {
                                if (DialogResult.Yes == MessageBox.Show("此单满足满额加价赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                {
                                    cho5.ChooseList.Add(new GoodsBuy
                                    {
                                        spec = zsinfo100.spec,
                                        pinYin = zsinfo100.py,
                                        unit = zsunit5,
                                        unitStr = dw_,
                                        noCode = item.zs_item_id,
                                        barCodeTM = item.zstm,
                                        goods = item.zs_cname,
                                        countNum = item.zs_amount,
                                        lsPrice = Math.Round((item.zsmoney / item.zs_amount).Value, 2),
                                        hyPrice = Math.Round((item.zsmoney / item.zs_amount).Value, 2),
                                        goodsDes = item.memo,
                                        jjPrice = Math.Round(item.zs_yjjprice, 2),
                                        pfPrice = Math.Round(item.zs_ylsprice, 2),  //记录原价
                                        isZS = true,
                                        isXG = true,
                                        vtype = 5
                                    });

                                    istip = true;
                                }
                            }


                        }


                    }



                }

                if (cho5.ChooseList.Count > 0)
                {
                    cho5.ShowDialog();
                }
            }
            #endregion


        }




        //ChoiceGoods solo9_SP = new ChoiceGoods();
        /// <summary>
        /// 活动9的商品数量满赠送
        /// </summary>
        /// <param name="db"></param>
        private void YH9SPFunc(hjnbhEntities db)
        {
            #region 活动9的商品数量满赠送
            int scode_temp = HandoverModel.GetInstance.scode;
            int VipID = HandoverModel.GetInstance.VipID;
            int viplv = HandoverModel.GetInstance.VipLv;

            //先判定有没有这个活动
            var ppinfo = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 9 && t.tj_range == 0 && t.scode == scode_temp).ToList();
            //要判定购物车内是否购满该类别或者品牌的商品
            //然后才送
            if (ppinfo.Count > 0)
            {
                foreach (var item in ppinfo)
                {
                    //先过滤一下，看购物车中是否有符合活动的普通商品
                    var HasHdItem = goodsBuyList.Where(t => t.vtype == 0 && t.noCode == item.item_id).FirstOrDefault();
                    if (HasHdItem == null) continue;

                    #region 商品单位、规格、拼音查询
                    int zsunit5 = 0;
                    var zsinfo100 = db.hd_item_info.AsNoTracking().Where(t => t.item_id == item.zs_item_id || t.tm == item.zstm).Select(t => new { t.unit, t.spec, t.py }).FirstOrDefault();
                    if (zsinfo100 != null)
                    {
                        zsunit5 = zsinfo100.unit.HasValue ? (int)zsinfo100.unit.Value : 1;
                    }
                    //需要把单位编号转换为中文以便UI显示
                    string dw_ = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == zsunit5).Select(t => t.txt1).FirstOrDefault();
                    #endregion

                    if (item.dx_type == 1)  //限定会员
                    {
                        if (VipID == 0) continue;

                        int viplvInfo = item.viptype;
                        //如果会员等级条件满足
                        if (viplv >= viplvInfo)
                        {
                            List<hd_ls> viplsList; //查询活动时段内会员是否有消费
                            //先判定会员最后一次领取赠品的时间
                            var vipzstime = db.hd_vip_zs_history.AsNoTracking().Where(t => t.vipcode == VipID && t.item_id == item.zs_item_id && t.zstime > item.sbegintime && t.zstime < item.sendtime).OrderByDescending(t => t.zstime).Select(t => t.zstime).FirstOrDefault();
                            if (vipzstime != null)
                            {
                                viplsList = db.hd_ls.AsNoTracking().Where(t => t.vip == VipID && t.ctime > vipzstime && t.ctime < item.sendtime).ToList();

                            }
                            else
                            {
                                viplsList = db.hd_ls.AsNoTracking().Where(t => t.vip == VipID && t.ctime > item.sbegintime && t.ctime < item.sendtime).ToList();
                            }

                            if (viplsList.Count > 0)
                            {

                                //消费的商品里是否符合条件的品牌
                                decimal ppzsnum = 0; //符合的数量
                                foreach (var itempp in viplsList)
                                {
                                    //找到这件活动商品的购买记录
                                    var vipls = db.hd_ls_detail.AsNoTracking().Where(t => t.v_code == itempp.v_code && t.item_id == item.item_id).FirstOrDefault();
                                    if (vipls != null)
                                    {
                                        ppzsnum += vipls.amount.Value;  //统计出该品牌已经购买数量
                                    }
                                }
                                //只需要再购买的数量 
                                decimal numtemp = item.amount - ppzsnum;
                                //购物车中的商品类别符合的购买数量(不与其它活动重叠)
                                var lbnum = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).Select(e => e.countNum).Sum();
                                if (lbnum >= numtemp)
                                {
                                    //符合条件，可以赠送
                                    //判断是否有活动10关联
                                    //同名活动10
                                    var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp  && t.item_id == item.zs_item_id).FirstOrDefault();
                                    //得判断有没有同名的活动10存在的情况
                                    if (YH10ZS != null)
                                    {
                                        //赠品数量
                                        decimal ZScount = 1.00m;
                                        //关联的数量,要有数量才有得搞
                                        if (YH10ZS.amount > 0)
                                        {
                                            if (YH10ZS.amount < item.zs_amount)
                                            {
                                                ZScount = YH10ZS.amount;
                                            }
                                            else
                                            {
                                                ZScount = item.zs_amount;
                                            }


                                            //判断购物车中有没有这个赠品
                                            var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                            if (ZSgood != null)
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足商品满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                {


                                                    goodsBuyList.Add(new GoodsBuy
                                                    {
                                                        spec = zsinfo100.spec,
                                                        pinYin = zsinfo100.py,
                                                        unit = zsunit5,
                                                        unitStr = dw_,
                                                        barCodeTM = item.zstm,
                                                        noCode = item.zs_item_id,
                                                        countNum = ZScount,
                                                        goods = item.zs_cname,
                                                        goodsDes = item.memo,
                                                        lsPrice = Math.Round(item.zs_ylsprice, 2),
                                                        hyPrice = 0.00m,
                                                        jjPrice = item.yjj_price,
                                                        pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                        isVip = true,
                                                        isZS = true,
                                                        vtype = 9,
                                                        isGL = true

                                                    });
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //没有关联
                                        //判断购物车中有没有这个赠品
                                        var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                        if (ZSgood != null)
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足商品满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                            {

                                                goodsBuyList.Add(new GoodsBuy
                                                {
                                                    spec = zsinfo100.spec,
                                                    pinYin = zsinfo100.py,
                                                    unit = zsunit5,
                                                    unitStr = dw_,
                                                    barCodeTM = item.zstm,
                                                    noCode = item.zs_item_id,
                                                    countNum = item.zs_amount,
                                                    goods = item.zs_cname,
                                                    goodsDes = item.memo,
                                                    lsPrice = Math.Round(item.zs_ylsprice, 2),
                                                    hyPrice = 0.00m,
                                                    jjPrice = item.yjj_price,
                                                    pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                    isVip = true,
                                                    isZS = true,
                                                    vtype = 9,
                                                });
                                            }
                                        }
                                    }

                                }
                            }
                            else
                            {
                                //如果会员在指定时间内没有消费，但是现在一次性购满的情况

                                //购物车中的商品类别符合的购买数量(不与其它活动重叠)
                                var lbnum = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).Select(e => e.countNum).Sum();
                                if (lbnum >= item.amount)
                                {

                                    //符合条件，可以赠送
                                    //判断是否有活动10关联
                                    //同名活动10
                                    var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp  && t.item_id == item.zs_item_id).FirstOrDefault();
                                    //得判断有没有同名的活动10存在的情况
                                    if (YH10ZS != null)
                                    {
                                        //赠品数量
                                        decimal ZScount = 1.00m;
                                        //关联的数量,要有数量才有得搞
                                        if (YH10ZS.amount > 0)
                                        {
                                            if (YH10ZS.amount < item.zs_amount)
                                            {
                                                ZScount = YH10ZS.amount;
                                            }
                                            else
                                            {
                                                ZScount = item.zs_amount;
                                            }


                                            //判断购物车中有没有这个赠品
                                            var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                            if (ZSgood != null)
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足商品满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                {

                                                    goodsBuyList.Add(new GoodsBuy
                                                    {
                                                        spec = zsinfo100.spec,
                                                        pinYin = zsinfo100.py,
                                                        unit = zsunit5,
                                                        unitStr = dw_,
                                                        barCodeTM = item.zstm,
                                                        noCode = item.zs_item_id,
                                                        countNum = ZScount,
                                                        jjPrice = item.yjj_price,
                                                        pfPrice = Math.Round(item.yls_price, 2),
                                                        goods = item.zs_cname,
                                                        goodsDes = item.memo,
                                                        lsPrice = 0.00m,
                                                        hyPrice = 0.00m,
                                                        isZS = true,
                                                        vtype = 9,
                                                        isVip = true,
                                                        isGL = true

                                                    });
                                                }
                                            }

                                        }
                                    }
                                    else
                                    {
                                        //没有关联活动
                                        //判断购物车中有没有这个赠品
                                        var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                        if (ZSgood != null)
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足商品满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                            {

                                                goodsBuyList.Add(new GoodsBuy
                                                {
                                                    spec = zsinfo100.spec,
                                                    pinYin = zsinfo100.py,
                                                    unit = zsunit5,
                                                    unitStr = dw_,
                                                    barCodeTM = item.zstm,
                                                    noCode = item.zs_item_id,
                                                    countNum = item.zs_amount,
                                                    jjPrice = item.yjj_price,
                                                    pfPrice = Math.Round(item.yls_price, 2),
                                                    goods = item.zs_cname,
                                                    goodsDes = item.memo,
                                                    lsPrice = 0.00m,
                                                    hyPrice = 0.00m,
                                                    isZS = true,
                                                    vtype = 9,
                                                    isVip = true,

                                                });
                                            }
                                        }


                                    }

                                }
                            }
                        }
                        else
                        {
                            //不符合资格的会员

                            //符合条件，可以赠送
                            //判断是否有活动10关联
                            //同名活动10
                            var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).FirstOrDefault();
                            //得判断有没有同名的活动10存在的情况
                            if (YH10ZS != null)
                            {
                                if (YH10ZS.amount <= 0) continue;  //有总量才有得搞

                                //赠品数量
                                decimal ZScount = 1.00m;
                                //关联的数量,要有数量才有得搞
                                if (YH10ZS.amount < item.zs_amount)
                                {
                                    ZScount = YH10ZS.amount;
                                }
                                else
                                {
                                    ZScount = item.zs_amount;
                                }

                                //购物车中的商品类别符合的购买数量(不与其它活动重叠)
                                var lbnum = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).Select(e => e.countNum).Sum();
                                if (lbnum >= item.amount)
                                {
                                    //判断购物车中有没有这个赠品
                                    var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                    if (ZSgood != null)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        goodsBuyList.Add(new GoodsBuy
                                        {
                                            spec = zsinfo100.spec,
                                            pinYin = zsinfo100.py,
                                            unit = zsunit5,
                                            unitStr = dw_,
                                            barCodeTM = item.zstm,
                                            noCode = item.zs_item_id,
                                            countNum = ZScount,
                                            jjPrice = item.yjj_price,
                                            pfPrice = Math.Round(item.yls_price, 2),
                                            goods = item.zs_cname,
                                            goodsDes = item.memo,
                                            lsPrice = 0.00m,
                                            hyPrice = 0.00m,
                                            isZS = true,
                                            isVip = true,
                                            isGL = true,
                                            vtype = 9
                                        });
                                    }
                                }


                            }
                            else
                            {
                                //没关联
                                //购物车中的商品类别符合的购买数量(不与其它活动重叠)
                                var lbnum = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).Select(e => e.countNum).Sum();
                                if (lbnum >= item.amount)
                                {
                                    //判断购物车中有没有这个赠品
                                    var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                    if (ZSgood != null)
                                    {
                                        continue;
                                    }
                                    else
                                    {

                                        goodsBuyList.Add(new GoodsBuy
                                        {
                                            spec = zsinfo100.spec,
                                            pinYin = zsinfo100.py,
                                            unit = zsunit5,
                                            unitStr = dw_,
                                            barCodeTM = item.zstm,
                                            noCode = item.zs_item_id,
                                            countNum = item.zs_amount,
                                            jjPrice = item.yjj_price,
                                            pfPrice = Math.Round(item.yls_price, 2),
                                            goods = item.zs_cname,
                                            goodsDes = item.memo,
                                            lsPrice = 0.00m,
                                            hyPrice = 0.00m,
                                            isZS = true,
                                            isVip = true,
                                            vtype = 9
                                        });
                                    }
                                }

                            }

                        }
                    }
                    //不限定会员
                    else
                    {
                        //是会员的情况
                        if (VipID > 0)
                        {
                            List<hd_ls> viplsList; //查询活动时段内会员是否有消费
                            //先判定会员最后一次领取赠品的时间
                            var vipzstime = db.hd_vip_zs_history.AsNoTracking().Where(t => t.vipcode == VipID && t.item_id == item.zs_item_id && t.zstime > item.sbegintime && t.zstime < item.sendtime).OrderByDescending(t => t.zstime).Select(t => t.zstime).FirstOrDefault();
                            if (vipzstime != null)
                            {
                                viplsList = db.hd_ls.AsNoTracking().Where(t => t.vip == VipID && t.ctime > vipzstime && t.ctime < item.sendtime).ToList();

                            }
                            else
                            {
                                viplsList = db.hd_ls.AsNoTracking().Where(t => t.vip == VipID && t.ctime > item.sbegintime && t.ctime < item.sendtime).ToList();
                            }

                            if (viplsList.Count > 0)
                            {

                                //消费的商品里是否符合条件的品牌
                                decimal ppzsnum = 0; //符合的数量
                                foreach (var itempp in viplsList)
                                {
                                    //找到这件活动商品的购买记录
                                    var vipls = db.hd_ls_detail.AsNoTracking().Where(t => t.v_code == itempp.v_code && t.item_id == item.item_id).FirstOrDefault();
                                    if (vipls != null)
                                    {
                                        ppzsnum += vipls.amount.Value;  //统计出该品牌已经购买数量
                                    }
                                }
                                //只需要再购买的数量 
                                decimal numtemp = item.amount - ppzsnum;
                                //购物车中的商品类别符合的购买数量(不与其它活动重叠)
                                var lbnum = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).Select(e => e.countNum).Sum();
                                if (lbnum >= numtemp)
                                {

                                    //符合条件，可以赠送
                                    //判断是否有活动10关联
                                    //同名活动10
                                    var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).FirstOrDefault();
                                    //得判断有没有同名的活动10存在的情况
                                    if (YH10ZS != null)
                                    {
                                        //赠品数量
                                        decimal ZScount = 1.00m;
                                        //关联的数量,要有数量才有得搞
                                        if (YH10ZS.amount > 0)
                                        {
                                            if (YH10ZS.amount < item.zs_amount)
                                            {
                                                ZScount = YH10ZS.amount;
                                            }
                                            else
                                            {
                                                ZScount = item.zs_amount;
                                            }

                                            //判断购物车中有没有这个赠品
                                            var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                            if (ZSgood != null)
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足商品满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                {


                                                    goodsBuyList.Add(new GoodsBuy
                                                    {
                                                        spec = zsinfo100.spec,
                                                        pinYin = zsinfo100.py,
                                                        unit = zsunit5,
                                                        unitStr = dw_,
                                                        barCodeTM = item.zstm,
                                                        noCode = item.zs_item_id,
                                                        countNum = ZScount,
                                                        goods = item.zs_cname,
                                                        goodsDes = item.memo,
                                                        lsPrice = Math.Round(item.zs_ylsprice, 2),
                                                        hyPrice = 0.00m,
                                                        jjPrice = item.yjj_price,
                                                        pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                        isVip = true,
                                                        isZS = true,
                                                        vtype = 9,
                                                        isGL = true

                                                    });
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //没有关联
                                        //判断购物车中有没有这个赠品
                                        var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                        if (ZSgood != null)
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足商品满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                            {

                                                goodsBuyList.Add(new GoodsBuy
                                                {
                                                    spec = zsinfo100.spec,
                                                    pinYin = zsinfo100.py,
                                                    unit = zsunit5,
                                                    unitStr = dw_,
                                                    barCodeTM = item.zstm,
                                                    noCode = item.zs_item_id,
                                                    countNum = item.zs_amount,
                                                    goods = item.zs_cname,
                                                    goodsDes = item.memo,
                                                    lsPrice = Math.Round(item.zs_ylsprice, 2),
                                                    hyPrice = 0.00m,
                                                    jjPrice = item.yjj_price,
                                                    pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                    isVip = true,
                                                    isZS = true,
                                                    vtype = 9,
                                                });
                                            }
                                        }
                                    }

                                }
                            }
                            else
                            {
                                //如果会员在指定时间内没有消费，但是现在一次性购满的情况
                                //购物车中的商品类别符合的购买数量(不与其它活动重叠)
                                var lbnum = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).Select(e => e.countNum).Sum();
                                if (lbnum >= item.amount)
                                {

                                    //符合条件，可以赠送
                                    //判断是否有活动10关联
                                    //同名活动10
                                    var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).FirstOrDefault();
                                    //得判断有没有同名的活动10存在的情况
                                    if (YH10ZS != null)
                                    {
                                        //赠品数量
                                        decimal ZScount = 1.00m;
                                        //关联的数量,要有数量才有得搞
                                        if (YH10ZS.amount > 0)
                                        {
                                            if (YH10ZS.amount < item.zs_amount)
                                            {
                                                ZScount = YH10ZS.amount;
                                            }
                                            else
                                            {
                                                ZScount = item.zs_amount;
                                            }


                                            //判断购物车中有没有这个赠品
                                            var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                            if (ZSgood != null)
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足商品满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                {

                                                    goodsBuyList.Add(new GoodsBuy
                                                    {
                                                        spec = zsinfo100.spec,
                                                        pinYin = zsinfo100.py,
                                                        unit = zsunit5,
                                                        unitStr = dw_,
                                                        barCodeTM = item.zstm,
                                                        noCode = item.zs_item_id,
                                                        countNum = ZScount,
                                                        jjPrice = item.yjj_price,
                                                        pfPrice = Math.Round(item.yls_price, 2),
                                                        goods = item.zs_cname,
                                                        goodsDes = item.memo,
                                                        lsPrice = 0.00m,
                                                        hyPrice = 0.00m,
                                                        isZS = true,
                                                        vtype = 9,
                                                        isVip = true,
                                                        isGL = true

                                                    });
                                                }
                                            }

                                        }
                                    }
                                    else
                                    {
                                        //没有关联活动
                                        //判断购物车中有没有这个赠品
                                        var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                        if (ZSgood != null)
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足商品满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                            {

                                                goodsBuyList.Add(new GoodsBuy
                                                {
                                                    spec = zsinfo100.spec,
                                                    pinYin = zsinfo100.py,
                                                    unit = zsunit5,
                                                    unitStr = dw_,
                                                    barCodeTM = item.zstm,
                                                    noCode = item.zs_item_id,
                                                    countNum = item.zs_amount,
                                                    jjPrice = item.yjj_price,
                                                    pfPrice = Math.Round(item.yls_price, 2),
                                                    goods = item.zs_cname,
                                                    goodsDes = item.memo,
                                                    lsPrice = 0.00m,
                                                    hyPrice = 0.00m,
                                                    isZS = true,
                                                    vtype = 9,
                                                    isVip = true,

                                                });
                                            }
                                        }


                                    }

                                }
                            }
                        }
                        else
                        {
                            //非会员消费

                            //符合条件，可以赠送
                            //判断是否有活动10关联
                            //同名活动10
                            var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp  && t.item_id == item.zs_item_id).FirstOrDefault();
                            //得判断有没有同名的活动10存在的情况
                            if (YH10ZS != null)
                            {
                                if (YH10ZS.amount <= 0) continue;  //有总量才有得搞

                                //赠品数量
                                decimal ZScount = 1.00m;
                                //关联的数量,要有数量才有得搞
                                if (YH10ZS.amount < item.zs_amount)
                                {
                                    ZScount = YH10ZS.amount;
                                }
                                else
                                {
                                    ZScount = item.zs_amount;
                                }

                                //购物车中的商品类别符合的购买数量(不与其它活动重叠)
                                var lbnum = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).Select(e => e.countNum).Sum();
                                if (lbnum >= item.amount)
                                {
                                    //判断购物车中有没有这个赠品
                                    var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                    if (ZSgood != null)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        goodsBuyList.Add(new GoodsBuy
                                        {
                                            spec = zsinfo100.spec,
                                            pinYin = zsinfo100.py,
                                            unit = zsunit5,
                                            unitStr = dw_,
                                            barCodeTM = item.zstm,
                                            noCode = item.zs_item_id,
                                            countNum = ZScount,
                                            jjPrice = item.yjj_price,
                                            pfPrice = Math.Round(item.yls_price, 2),
                                            goods = item.zs_cname,
                                            goodsDes = item.memo,
                                            lsPrice = 0.00m,
                                            hyPrice = 0.00m,
                                            isZS = true,
                                            isGL = true,
                                            vtype = 9
                                        });
                                    }
                                }

                            }
                            else
                            {
                                //没关联
                                //购物车中的商品类别符合的购买数量(不与其它活动重叠)
                                var lbnum = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).Select(e => e.countNum).Sum();
                                if (lbnum >= item.amount)
                                {
                                    //判断购物车中有没有这个赠品
                                    var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                    if (ZSgood != null)
                                    {
                                        continue;
                                    }
                                    else
                                    {

                                        goodsBuyList.Add(new GoodsBuy
                                        {
                                            spec = zsinfo100.spec,
                                            pinYin = zsinfo100.py,
                                            unit = zsunit5,
                                            unitStr = dw_,
                                            barCodeTM = item.zstm,
                                            noCode = item.zs_item_id,
                                            countNum = item.zs_amount,
                                            jjPrice = item.yjj_price,
                                            pfPrice = Math.Round(item.yls_price, 2),
                                            goods = item.zs_cname,
                                            goodsDes = item.memo,
                                            lsPrice = 0.00m,
                                            hyPrice = 0.00m,
                                            isZS = true,
                                            vtype = 9
                                        });
                                    }
                                }

                            }
                        }


                    }

                }

            }



            #endregion

        }


        //ChoiceGoods solo9_LB = new ChoiceGoods();
        /// <summary>
        /// 活动9的类别赠送
        /// </summary>
        /// <param name="db"></param>
        private void YH9LBFunc(hjnbhEntities db)
        {
            #region 活动9的类别赠
            int scode_temp = HandoverModel.GetInstance.scode;
            int VipID = HandoverModel.GetInstance.VipID;
            int viplv = HandoverModel.GetInstance.VipLv;
            //先判定有没有这两个活动
            var ppinfo = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 9 && t.tj_range == 1 && t.scode == scode_temp).ToList();
            //要判定购物车内是否购满该类别或者品牌的商品
            //然后才送
            if (ppinfo.Count > 0)
            {

                foreach (var item in ppinfo)
                {
                    //先过滤一下，看购物车中是否有符合活动的普通商品
                    var HasHdItem = goodsBuyList.Where(t => t.vtype == 0 && t.LB == item.item_id).FirstOrDefault();
                    if (HasHdItem == null) continue;

                    #region 商品单位、规格、拼音查询
                    int zsunit5 = 0;
                    var zsinfo100 = db.hd_item_info.AsNoTracking().Where(t => t.item_id == item.zs_item_id || t.tm == item.zstm).Select(t => new { t.unit, t.spec, t.py }).FirstOrDefault();
                    if (zsinfo100 != null)
                    {
                        zsunit5 = zsinfo100.unit.HasValue ? (int)zsinfo100.unit.Value : 1;
                    }
                    //需要把单位编号转换为中文以便UI显示
                    string dw_ = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == zsunit5).Select(t => t.txt1).FirstOrDefault();
                    #endregion

                    if (item.dx_type == 1)  //限定会员
                    {
                        if (VipID == 0) continue;

                        int viplvInfo = item.viptype;
                        //如果会员等级条件满足
                        if (viplv >= viplvInfo)
                        {
                            List<hd_ls> viplsList; //查询活动时段内会员是否有消费
                            //先判定会员最后一次领取赠品的时间
                            var vipzstime = db.hd_vip_zs_history.AsNoTracking().Where(t => t.vipcode == VipID && t.item_id == item.zs_item_id && t.zstime > item.sbegintime && t.zstime < item.sendtime).OrderByDescending(t => t.zstime).Select(t => t.zstime).FirstOrDefault();
                            if (vipzstime != null)
                            {
                                viplsList = db.hd_ls.AsNoTracking().Where(t => t.vip == VipID && t.ctime > vipzstime && t.ctime < item.sendtime).ToList();

                            }
                            else
                            {
                                viplsList = db.hd_ls.AsNoTracking().Where(t => t.vip == VipID && t.ctime > item.sbegintime && t.ctime < item.sendtime).ToList();
                            }

                            if (viplsList.Count > 0)
                            {

                                //消费的商品里是否符合条件的品牌
                                decimal ppzsnum = 0; //符合的数量
                                foreach (var itempp in viplsList)
                                {
                                    //找到这件活动商品的购买记录
                                    var vipls = db.hd_ls_detail.AsNoTracking().Where(t => t.v_code == itempp.v_code).FirstOrDefault();
                                    if (vipls != null)
                                    {
                                        var pplb = db.v_xs_item_info.AsNoTracking().Where(t => t.item_id == vipls.item_id).Select(t => t.lb_code).FirstOrDefault();
                                        if (pplb == item.item_id)
                                        {
                                            ppzsnum += vipls.amount.Value;  //统计出该品牌已经购买数量
                                        }
                                    }
                                }
                                //只需要再购买的数量 
                                decimal numtemp = item.amount - ppzsnum;
                                //购物车中的商品类别符合的购买数量(不与其它活动重叠)
                                var lbnum = goodsBuyList.Where(e => e.LB == item.item_id && e.vtype == 0).Select(e => e.countNum).Sum();
                                if (lbnum >= numtemp)
                                {

                                    //符合条件，可以赠送
                                    //判断是否有活动10关联
                                    //同名活动10
                                    var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).FirstOrDefault();
                                    //得判断有没有同名的活动10存在的情况
                                    if (YH10ZS != null)
                                    {
                                        //赠品数量
                                        decimal ZScount = 1.00m;
                                        //关联的数量,要有数量才有得搞
                                        if (YH10ZS.amount > 0)
                                        {
                                            if (YH10ZS.amount < item.zs_amount)
                                            {
                                                ZScount = YH10ZS.amount;
                                            }
                                            else
                                            {
                                                ZScount = item.zs_amount;
                                            }


                                            //判断购物车中有没有这个赠品
                                            var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                            if (ZSgood != null)
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                {
                                                    goodsBuyList.Add(new GoodsBuy
                                                    {
                                                        spec = zsinfo100.spec,
                                                        pinYin = zsinfo100.py,
                                                        unit = zsunit5,
                                                        unitStr = dw_,
                                                        barCodeTM = item.zstm,
                                                        noCode = item.zs_item_id,
                                                        countNum = ZScount,
                                                        goods = item.zs_cname,
                                                        goodsDes = item.memo,
                                                        lsPrice = Math.Round(item.zs_ylsprice, 2),
                                                        hyPrice = 0.00m,
                                                        jjPrice = item.yjj_price,
                                                        pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                        isVip = true,
                                                        isZS = true,
                                                        vtype = 9,
                                                        isGL = true

                                                    });

                                                }

                                            }

                                        }
                                    }
                                    else
                                    {
                                        //没有关联活动
                                        //判断购物车中有没有这个赠品
                                        var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                        if (ZSgood != null)
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                            {
                                                goodsBuyList.Add(new GoodsBuy
                                                {
                                                    spec = zsinfo100.spec,
                                                    pinYin = zsinfo100.py,
                                                    unit = zsunit5,
                                                    unitStr = dw_,
                                                    barCodeTM = item.zstm,
                                                    noCode = item.zs_item_id,
                                                    countNum = item.zs_amount,
                                                    goods = item.zs_cname,
                                                    goodsDes = item.memo,
                                                    lsPrice = Math.Round(item.zs_ylsprice, 2),
                                                    hyPrice = 0.00m,
                                                    jjPrice = item.yjj_price,
                                                    pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                    isVip = true,
                                                    isZS = true,
                                                    vtype = 9,
                                                });

                                            }

                                        }

                                    }

                                }
                            }
                            else
                            {
                                //如果会员在指定时间内没有消费，但是现在一次性购满的情况
                                //购物车中的商品类别符合的购买数量(不与其它活动重叠)
                                var lbnum = goodsBuyList.Where(e => e.LB == item.item_id && e.vtype == 0).Select(e => e.countNum).Sum();
                                if (lbnum >= item.amount)
                                {
                                    //没有关联活动
                                    //判断购物车中有没有这个赠品
                                    var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                    if (ZSgood != null)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            goodsBuyList.Add(new GoodsBuy
                                            {
                                                spec = zsinfo100.spec,
                                                pinYin = zsinfo100.py,
                                                unit = zsunit5,
                                                unitStr = dw_,
                                                barCodeTM = item.zstm,
                                                noCode = item.zs_item_id,
                                                countNum = item.zs_amount,
                                                goods = item.zs_cname,
                                                goodsDes = item.memo,
                                                lsPrice = 0.00m,
                                                hyPrice = 0.00m,
                                                jjPrice = item.yjj_price,
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                isVip = true,
                                                isZS = true,
                                                vtype = 9,
                                            });

                                        }

                                    }
                                }
                            }
                        }
                        else
                        {
                            //不符合资格的会员

                            //符合条件，可以赠送
                            //判断是否有活动10关联
                            //同名活动10
                            var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).FirstOrDefault();
                            //得判断有没有同名的活动10存在的情况
                            if (YH10ZS != null)
                            {
                                if (YH10ZS.amount <= 0) continue;  //有总量才有得搞

                                //赠品数量
                                decimal ZScount = 1.00m;
                                //关联的数量,要有数量才有得搞
                                if (YH10ZS.amount < item.zs_amount)
                                {
                                    ZScount = YH10ZS.amount;
                                }
                                else
                                {
                                    ZScount = item.zs_amount;
                                }

                                //购物车中的商品类别符合的购买数量
                                var lbnum = goodsBuyList.Where(e => e.LB == item.item_id).Select(e => e.countNum).Sum();
                                if (lbnum >= item.amount)
                                {

                                    //没有关联活动
                                    //判断购物车中有没有这个赠品
                                    var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                    if (ZSgood != null)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            goodsBuyList.Add(new GoodsBuy
                                            {
                                                spec = zsinfo100.spec,
                                                pinYin = zsinfo100.py,
                                                unit = zsunit5,
                                                unitStr = dw_,
                                                barCodeTM = item.zstm,
                                                noCode = item.zs_item_id,
                                                countNum = ZScount,
                                                goods = item.zs_cname,
                                                goodsDes = item.memo,
                                                lsPrice = 0.00m,
                                                hyPrice = 0.00m,
                                                jjPrice = item.yjj_price,
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                isZS = true,
                                                isGL = true,
                                                isVip = true,
                                                vtype = 9,
                                            });

                                        }

                                    }
                                }
                            }
                            else
                            {
                                //没有关联

                                //购物车中的商品类别符合的购买数量
                                var lbnum = goodsBuyList.Where(e => e.LB == item.item_id).Select(e => e.countNum).Sum();
                                if (lbnum >= item.amount)
                                {

                                    //没有关联活动
                                    //判断购物车中有没有这个赠品
                                    var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                    if (ZSgood != null)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            goodsBuyList.Add(new GoodsBuy
                                            {
                                                spec = zsinfo100.spec,
                                                pinYin = zsinfo100.py,
                                                unit = zsunit5,
                                                unitStr = dw_,
                                                barCodeTM = item.zstm,
                                                noCode = item.zs_item_id,
                                                countNum = item.zs_amount,
                                                goods = item.zs_cname,
                                                goodsDes = item.memo,
                                                lsPrice = 0.00m,
                                                hyPrice = 0.00m,
                                                jjPrice = item.yjj_price,
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                isZS = true,
                                                isVip = true,
                                                vtype = 9,
                                            });

                                        }

                                    }


                                }


                            }
                        }
                    }
                    //不限定会员
                    else
                    {
                        //是会员消费的情况
                        if (VipID > 0)
                        {
                            List<hd_ls> viplsList; //查询活动时段内会员是否有消费
                            //先判定会员最后一次领取赠品的时间
                            var vipzstime = db.hd_vip_zs_history.AsNoTracking().Where(t => t.vipcode == VipID && t.item_id == item.zs_item_id && t.zstime > item.sbegintime && t.zstime < item.sendtime).OrderByDescending(t => t.zstime).Select(t => t.zstime).FirstOrDefault();
                            if (vipzstime != null)
                            {
                                viplsList = db.hd_ls.AsNoTracking().Where(t => t.vip == VipID && t.ctime > vipzstime && t.ctime < item.sendtime).ToList();

                            }
                            else
                            {
                                viplsList = db.hd_ls.AsNoTracking().Where(t => t.vip == VipID && t.ctime > item.sbegintime && t.ctime < item.sendtime).ToList();
                            }

                            if (viplsList.Count > 0)
                            {

                                //消费的商品里是否符合条件的品牌
                                decimal ppzsnum = 0; //符合的数量
                                foreach (var itempp in viplsList)
                                {
                                    //找到这件活动商品的购买记录
                                    var vipls = db.hd_ls_detail.AsNoTracking().Where(t => t.v_code == itempp.v_code).FirstOrDefault();
                                    if (vipls != null)
                                    {
                                        var pplb = db.v_xs_item_info.AsNoTracking().Where(t => t.item_id == vipls.item_id).Select(t => t.lb_code).FirstOrDefault();
                                        if (pplb == item.item_id)
                                        {
                                            ppzsnum += vipls.amount.Value;  //统计出该品牌已经购买数量
                                        }
                                    }
                                }
                                //只需要再购买的数量 
                                decimal numtemp = item.amount - ppzsnum;
                                //购物车中的商品类别符合的购买数量(不与其它活动重叠)
                                var lbnum = goodsBuyList.Where(e => e.LB == item.item_id && e.vtype == 0).Select(e => e.countNum).Sum();
                                if (lbnum >= numtemp)
                                {

                                    //符合条件，可以赠送
                                    //判断是否有活动10关联
                                    //同名活动10
                                    var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp  && t.item_id == item.zs_item_id).FirstOrDefault();
                                    //得判断有没有同名的活动10存在的情况
                                    if (YH10ZS != null)
                                    {
                                        //赠品数量
                                        decimal ZScount = 1.00m;
                                        //关联的数量,要有数量才有得搞
                                        if (YH10ZS.amount > 0)
                                        {
                                            if (YH10ZS.amount < item.zs_amount)
                                            {
                                                ZScount = YH10ZS.amount;
                                            }
                                            else
                                            {
                                                ZScount = item.zs_amount;
                                            }


                                            //判断购物车中有没有这个赠品
                                            var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                            if (ZSgood != null)
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                {
                                                    goodsBuyList.Add(new GoodsBuy
                                                    {
                                                        spec = zsinfo100.spec,
                                                        pinYin = zsinfo100.py,
                                                        unit = zsunit5,
                                                        unitStr = dw_,
                                                        barCodeTM = item.zstm,
                                                        noCode = item.zs_item_id,
                                                        countNum = ZScount,
                                                        goods = item.zs_cname,
                                                        goodsDes = item.memo,
                                                        lsPrice = Math.Round(item.zs_ylsprice, 2),
                                                        hyPrice = 0.00m,
                                                        jjPrice = item.yjj_price,
                                                        pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                        isVip = true,
                                                        isZS = true,
                                                        vtype = 9,
                                                        isGL = true

                                                    });

                                                }

                                            }

                                        }
                                    }
                                    else
                                    {
                                        //没有关联活动
                                        //判断购物车中有没有这个赠品
                                        var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                        if (ZSgood != null)
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                            {
                                                goodsBuyList.Add(new GoodsBuy
                                                {
                                                    spec = zsinfo100.spec,
                                                    pinYin = zsinfo100.py,
                                                    unit = zsunit5,
                                                    unitStr = dw_,
                                                    barCodeTM = item.zstm,
                                                    noCode = item.zs_item_id,
                                                    countNum = item.zs_amount,
                                                    goods = item.zs_cname,
                                                    goodsDes = item.memo,
                                                    lsPrice = Math.Round(item.zs_ylsprice, 2),
                                                    hyPrice = 0.00m,
                                                    jjPrice = item.yjj_price,
                                                    pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                    isVip = true,
                                                    isZS = true,
                                                    vtype = 9,
                                                });

                                            }

                                        }

                                    }

                                }
                            }
                            else
                            {
                                //如果会员在指定时间内没有消费，但是现在一次性购满的情况
                                //购物车中的商品类别符合的购买数量(不与其它活动重叠)
                                var lbnum = goodsBuyList.Where(e => e.LB == item.item_id && e.vtype == 0).Select(e => e.countNum).Sum();
                                if (lbnum >= item.amount)
                                {
                                    //没有关联活动
                                    //判断购物车中有没有这个赠品
                                    var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                    if (ZSgood != null)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            goodsBuyList.Add(new GoodsBuy
                                            {
                                                spec = zsinfo100.spec,
                                                pinYin = zsinfo100.py,
                                                unit = zsunit5,
                                                unitStr = dw_,
                                                barCodeTM = item.zstm,
                                                noCode = item.zs_item_id,
                                                countNum = item.zs_amount,
                                                goods = item.zs_cname,
                                                goodsDes = item.memo,
                                                lsPrice = 0.00m,
                                                hyPrice = 0.00m,
                                                jjPrice = item.yjj_price,
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                isVip = true,
                                                isZS = true,
                                                vtype = 9,
                                            });

                                        }

                                    }
                                }
                            }
                        }
                        else
                        {
                            //不是会员

                            //符合条件，可以赠送
                            //判断是否有活动10关联
                            //同名活动10
                            var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp  && t.item_id == item.zs_item_id).FirstOrDefault();
                            //得判断有没有同名的活动10存在的情况
                            if (YH10ZS != null)
                            {
                                if (YH10ZS.amount <= 0) continue;  //有总量才有得搞

                                //赠品数量
                                decimal ZScount = 1.00m;
                                //关联的数量,要有数量才有得搞
                                if (YH10ZS.amount < item.zs_amount)
                                {
                                    ZScount = YH10ZS.amount;
                                }
                                else
                                {
                                    ZScount = item.zs_amount;
                                }



                                //购物车中的商品类别符合的购买数量
                                var lbnum = goodsBuyList.Where(e => e.LB == item.item_id).Select(e => e.countNum).Sum();
                                if (lbnum >= item.amount)
                                {

                                    //没有关联活动
                                    //判断购物车中有没有这个赠品
                                    var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                    if (ZSgood != null)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            goodsBuyList.Add(new GoodsBuy
                                            {
                                                spec = zsinfo100.spec,
                                                pinYin = zsinfo100.py,
                                                unit = zsunit5,
                                                unitStr = dw_,
                                                barCodeTM = item.zstm,
                                                noCode = item.zs_item_id,
                                                countNum = ZScount,
                                                goods = item.zs_cname,
                                                goodsDes = item.memo,
                                                lsPrice = 0.00m,
                                                hyPrice = 0.00m,
                                                jjPrice = item.yjj_price,
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                isZS = true,
                                                isGL = true,
                                                vtype = 9,
                                            });

                                        }

                                    }
                                }
                            }
                            else
                            {
                                //没有关联

                                //购物车中的商品类别符合的购买数量
                                var lbnum = goodsBuyList.Where(e => e.LB == item.item_id).Select(e => e.countNum).Sum();
                                if (lbnum >= item.amount)
                                {

                                    //没有关联活动
                                    //判断购物车中有没有这个赠品
                                    var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                    if (ZSgood != null)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            goodsBuyList.Add(new GoodsBuy
                                            {
                                                spec = zsinfo100.spec,
                                                pinYin = zsinfo100.py,
                                                unit = zsunit5,
                                                unitStr = dw_,
                                                barCodeTM = item.zstm,
                                                noCode = item.zs_item_id,
                                                countNum = item.zs_amount,
                                                goods = item.zs_cname,
                                                goodsDes = item.memo,
                                                lsPrice = 0.00m,
                                                hyPrice = 0.00m,
                                                jjPrice = item.yjj_price,
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                isZS = true,
                                                vtype = 9,
                                            });

                                        }

                                    }


                                }


                            }


                        }


                    }

                }

            }



            #endregion

        }
        private void YH9PPFunc(hjnbhEntities db)
        {
            #region 活动9的品牌赠
            int scode_temp = HandoverModel.GetInstance.scode;
            int VipID = HandoverModel.GetInstance.VipID;
            int viplv = HandoverModel.GetInstance.VipLv;
            //先判定有没有这两个活动
            var ppinfo = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 9 && t.tj_range == 2 && t.scode == scode_temp).ToList();
            //要判定购物车内是否购满该类别或者品牌的商品
            //然后才送
            if (ppinfo.Count > 0)
            {
                foreach (var item in ppinfo)
                {
                    //先过滤一下，看购物车中是否有符合活动的普通商品
                    var HasHdItem = goodsBuyList.Where(t => t.vtype == 0 && t.PP == item.tm).FirstOrDefault();
                    if (HasHdItem == null) continue;

                    #region 商品单位、规格、拼音查询
                    int zsunit5 = 0;
                    var zsinfo100 = db.hd_item_info.AsNoTracking().Where(t => t.item_id == item.zs_item_id || t.tm == item.zstm).Select(t => new { t.unit, t.spec, t.py }).FirstOrDefault();
                    if (zsinfo100 != null)
                    {
                        zsunit5 = zsinfo100.unit.HasValue ? (int)zsinfo100.unit.Value : 1;
                    }
                    //需要把单位编号转换为中文以便UI显示
                    string dw_ = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == zsunit5).Select(t => t.txt1).FirstOrDefault();
                    #endregion

                    if (item.dx_type == 1)  //限定会员
                    {
                        if (VipID == 0) continue;

                        int viplvInfo = item.viptype;
                        //如果会员等级条件满足
                        if (viplv >= viplvInfo)
                        {
                            List<hd_ls> viplsList; //查询活动时段内会员是否有消费
                            //先判定会员最后一次领取赠品的时间
                            var vipzstime = db.hd_vip_zs_history.AsNoTracking().Where(t => t.vipcode == VipID && t.item_id == item.zs_item_id && t.zstime > item.sbegintime && t.zstime < item.sendtime).OrderByDescending(t => t.zstime).Select(t => t.zstime).FirstOrDefault();
                            if (vipzstime != null)
                            {
                                viplsList = db.hd_ls.AsNoTracking().Where(t => t.vip == VipID && t.ctime > vipzstime && t.ctime < item.sendtime).ToList();

                            }
                            else
                            {
                                viplsList = db.hd_ls.AsNoTracking().Where(t => t.vip == VipID && t.ctime > item.sbegintime && t.ctime < item.sendtime).ToList();
                            }

                            if (viplsList.Count > 0)
                            {

                                //消费的商品里是否符合条件的品牌
                                decimal ppzsnum = 0; //符合的数量
                                foreach (var itempp in viplsList)
                                {
                                    //找到这件活动商品的购买记录
                                    var vipls = db.hd_ls_detail.AsNoTracking().Where(t => t.v_code == itempp.v_code).FirstOrDefault();
                                    if (vipls != null)
                                    {
                                        var ppxs = db.v_xs_item_info.AsNoTracking().Where(t => t.item_id == vipls.item_id).Select(t => t.pp).FirstOrDefault();
                                        if (ppxs == item.tm)
                                        {
                                            ppzsnum += vipls.amount.Value;  //统计出该品牌已经购买数量
                                        }
                                    }
                                }
                                //只需要再购买的数量 
                                decimal numtemp = item.amount - ppzsnum;
                                //购物车中的商品类别符合的购买数量(不与其它活动重叠)
                                var lbnum = goodsBuyList.Where(e => e.PP == item.tm && e.vtype == 0).Select(e => e.countNum).Sum();
                                if (lbnum >= numtemp)
                                {

                                    //符合条件，可以赠送
                                    //判断是否有活动10关联
                                    //同名活动10
                                    var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).FirstOrDefault();
                                    //得判断有没有同名的活动10存在的情况
                                    if (YH10ZS != null)
                                    {
                                        //赠品数量
                                        decimal ZScount = 1.00m;
                                        //关联的数量,要有数量才有得搞
                                        if (YH10ZS.amount > 0)
                                        {
                                            if (YH10ZS.amount < item.zs_amount)
                                            {
                                                ZScount = YH10ZS.amount;
                                            }
                                            else
                                            {
                                                ZScount = item.zs_amount;
                                            }


                                            //判断购物车中有没有这个赠品
                                            var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                            if (ZSgood != null)
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                {
                                                    goodsBuyList.Add(new GoodsBuy
                                                    {
                                                        spec = zsinfo100.spec,
                                                        pinYin = zsinfo100.py,
                                                        unit = zsunit5,
                                                        unitStr = dw_,
                                                        barCodeTM = item.zstm,
                                                        noCode = item.zs_item_id,
                                                        countNum = ZScount,
                                                        goods = item.zs_cname,
                                                        goodsDes = item.memo,
                                                        lsPrice = Math.Round(item.zs_ylsprice, 2),
                                                        hyPrice = 0.00m,
                                                        jjPrice = item.yjj_price,
                                                        pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                        isVip = true,
                                                        isZS = true,
                                                        vtype = 9,
                                                        isGL = true

                                                    });

                                                }

                                            }

                                        }
                                    }
                                    else
                                    {
                                        //没有关联活动
                                        //判断购物车中有没有这个赠品
                                        var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                        if (ZSgood != null)
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                            {
                                                goodsBuyList.Add(new GoodsBuy
                                                {
                                                    spec = zsinfo100.spec,
                                                    pinYin = zsinfo100.py,
                                                    unit = zsunit5,
                                                    unitStr = dw_,
                                                    barCodeTM = item.zstm,
                                                    noCode = item.zs_item_id,
                                                    countNum = item.zs_amount,
                                                    goods = item.zs_cname,
                                                    goodsDes = item.memo,
                                                    lsPrice = Math.Round(item.zs_ylsprice, 2),
                                                    hyPrice = 0.00m,
                                                    jjPrice = item.yjj_price,
                                                    pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                    isVip = true,
                                                    isZS = true,
                                                    vtype = 9,
                                                });

                                            }

                                        }

                                    }

                                }
                            }
                            else
                            {
                                //如果会员在指定时间内没有消费，但是现在一次性购满的情况
                                //购物车中的商品类别符合的购买数量
                                var lbnum = goodsBuyList.Where(e => e.PP == item.tm && e.vtype == 0).Select(e => e.countNum).Sum();
                                if (lbnum >= item.amount)
                                {
                                    //没有关联活动
                                    //判断购物车中有没有这个赠品
                                    var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                    if (ZSgood != null)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            goodsBuyList.Add(new GoodsBuy
                                            {
                                                spec = zsinfo100.spec,
                                                pinYin = zsinfo100.py,
                                                unit = zsunit5,
                                                unitStr = dw_,
                                                barCodeTM = item.zstm,
                                                noCode = item.zs_item_id,
                                                countNum = item.zs_amount,
                                                goods = item.zs_cname,
                                                goodsDes = item.memo,
                                                lsPrice = 0.00m,
                                                hyPrice = 0.00m,
                                                jjPrice = item.yjj_price,
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                isVip = true,
                                                isZS = true,
                                                vtype = 9,
                                            });

                                        }

                                    }

                                }
                            }
                        }
                        else
                        {
                            //不满足等级的会员

                            //符合条件，可以赠送
                            //判断是否有活动10关联
                            //同名活动10
                            var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).FirstOrDefault();
                            //得判断有没有同名的活动10存在的情况
                            if (YH10ZS != null)
                            {
                                if (YH10ZS.amount <= 0) continue;  //有总量才有得搞

                                //赠品数量
                                decimal ZScount = 1.00m;
                                if (YH10ZS.amount < item.zs_amount)
                                {
                                    ZScount = YH10ZS.amount;
                                }
                                else
                                {
                                    ZScount = item.zs_amount;
                                }

                                //购物车中的商品类别符合的购买数量
                                var lbnum = goodsBuyList.Where(e => e.PP == item.tm && e.vtype == 0).Select(e => e.countNum).Sum();
                                if (lbnum >= item.amount)
                                {
                                    //判断购物车中有没有这个赠品
                                    var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                    if (ZSgood != null)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            goodsBuyList.Add(new GoodsBuy
                                            {
                                                spec = zsinfo100.spec,
                                                pinYin = zsinfo100.py,
                                                unit = zsunit5,
                                                unitStr = dw_,
                                                barCodeTM = item.zstm,
                                                noCode = item.zs_item_id,
                                                countNum = ZScount,
                                                goods = item.zs_cname,
                                                goodsDes = item.memo,
                                                lsPrice = 0.00m,
                                                hyPrice = 0.00m,
                                                jjPrice = item.yjj_price,
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                isVip = true,
                                                isGL = true,
                                                isZS = true,
                                                vtype = 9,
                                            });

                                        }

                                    }
                                }



                            }
                            else
                            {
                                //没有关联
                                //购物车中的商品类别符合的购买数量
                                var lbnum = goodsBuyList.Where(e => e.PP == item.tm && e.vtype == 0).Select(e => e.countNum).Sum();
                                if (lbnum >= item.amount)
                                {
                                    //判断购物车中有没有这个赠品
                                    var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                    if (ZSgood != null)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            goodsBuyList.Add(new GoodsBuy
                                            {
                                                spec = zsinfo100.spec,
                                                pinYin = zsinfo100.py,
                                                unit = zsunit5,
                                                unitStr = dw_,
                                                barCodeTM = item.zstm,
                                                noCode = item.zs_item_id,
                                                countNum = item.zs_amount,
                                                goods = item.zs_cname,
                                                goodsDes = item.memo,
                                                lsPrice = 0.00m,
                                                hyPrice = 0.00m,
                                                jjPrice = item.yjj_price,
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                isZS = true,
                                                isVip = true,
                                                vtype = 9,
                                            });

                                        }

                                    }
                                }
                            }


                        }
                    }
                    //不限定会员
                    else
                    {
                        //如果是会员消费
                        if (VipID > 0)
                        {
                            List<hd_ls> viplsList; //查询活动时段内会员是否有消费
                            //先判定会员最后一次领取赠品的时间
                            var vipzstime = db.hd_vip_zs_history.AsNoTracking().Where(t => t.vipcode == VipID && t.item_id == item.zs_item_id && t.zstime > item.sbegintime && t.zstime < item.sendtime).OrderByDescending(t => t.zstime).Select(t => t.zstime).FirstOrDefault();
                            if (vipzstime != null)
                            {
                                viplsList = db.hd_ls.AsNoTracking().Where(t => t.vip == VipID && t.ctime > vipzstime && t.ctime < item.sendtime).ToList();

                            }
                            else
                            {
                                viplsList = db.hd_ls.AsNoTracking().Where(t => t.vip == VipID && t.ctime > item.sbegintime && t.ctime < item.sendtime).ToList();
                            }

                            if (viplsList.Count > 0)
                            {

                                //消费的商品里是否符合条件的品牌
                                decimal ppzsnum = 0; //符合的数量
                                foreach (var itempp in viplsList)
                                {
                                    //找到这件活动商品的购买记录
                                    var vipls = db.hd_ls_detail.AsNoTracking().Where(t => t.v_code == itempp.v_code).FirstOrDefault();
                                    if (vipls != null)
                                    {
                                        var ppxs = db.v_xs_item_info.AsNoTracking().Where(t => t.item_id == vipls.item_id).Select(t => t.pp).FirstOrDefault();
                                        if (ppxs == item.tm)
                                        {
                                            ppzsnum += vipls.amount.Value;  //统计出该品牌已经购买数量
                                        }
                                    }
                                }
                                //只需要再购买的数量 
                                decimal numtemp = item.amount - ppzsnum;
                                //购物车中的商品类别符合的购买数量(不与其它活动重叠)
                                var lbnum = goodsBuyList.Where(e => e.PP == item.tm && e.vtype == 0).Select(e => e.countNum).Sum();
                                if (lbnum >= numtemp)
                                {

                                    //符合条件，可以赠送
                                    //判断是否有活动10关联
                                    //同名活动10
                                    var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).FirstOrDefault();
                                    //得判断有没有同名的活动10存在的情况
                                    if (YH10ZS != null)
                                    {
                                        //赠品数量
                                        decimal ZScount = 1.00m;
                                        //关联的数量,要有数量才有得搞
                                        if (YH10ZS.amount > 0)
                                        {
                                            if (YH10ZS.amount < item.zs_amount)
                                            {
                                                ZScount = YH10ZS.amount;
                                            }
                                            else
                                            {
                                                ZScount = item.zs_amount;
                                            }


                                            //判断购物车中有没有这个赠品
                                            var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                            if (ZSgood != null)
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                {
                                                    goodsBuyList.Add(new GoodsBuy
                                                    {
                                                        spec = zsinfo100.spec,
                                                        pinYin = zsinfo100.py,
                                                        unit = zsunit5,
                                                        unitStr = dw_,
                                                        barCodeTM = item.zstm,
                                                        noCode = item.zs_item_id,
                                                        countNum = ZScount,
                                                        goods = item.zs_cname,
                                                        goodsDes = item.memo,
                                                        lsPrice = Math.Round(item.zs_ylsprice, 2),
                                                        hyPrice = 0.00m,
                                                        jjPrice = item.yjj_price,
                                                        pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                        isVip = true,
                                                        isZS = true,
                                                        vtype = 9,
                                                        isGL = true

                                                    });

                                                }

                                            }

                                        }
                                    }
                                    else
                                    {
                                        //没有关联活动
                                        //判断购物车中有没有这个赠品
                                        var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                        if (ZSgood != null)
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                            {
                                                goodsBuyList.Add(new GoodsBuy
                                                {
                                                    spec = zsinfo100.spec,
                                                    pinYin = zsinfo100.py,
                                                    unit = zsunit5,
                                                    unitStr = dw_,
                                                    barCodeTM = item.zstm,
                                                    noCode = item.zs_item_id,
                                                    countNum = item.zs_amount,
                                                    goods = item.zs_cname,
                                                    goodsDes = item.memo,
                                                    lsPrice = Math.Round(item.zs_ylsprice, 2),
                                                    hyPrice = 0.00m,
                                                    jjPrice = item.yjj_price,
                                                    pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                    isVip = true,
                                                    isZS = true,
                                                    vtype = 9,
                                                });

                                            }

                                        }

                                    }

                                }
                            }
                            else
                            {
                                //如果会员在指定时间内没有消费，但是现在一次性购满的情况
                                //购物车中的商品类别符合的购买数量
                                var lbnum = goodsBuyList.Where(e => e.PP == item.tm && e.vtype == 0).Select(e => e.countNum).Sum();
                                if (lbnum >= item.amount)
                                {
                                    //没有关联活动
                                    //判断购物车中有没有这个赠品
                                    var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                    if (ZSgood != null)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            goodsBuyList.Add(new GoodsBuy
                                            {
                                                spec = zsinfo100.spec,
                                                pinYin = zsinfo100.py,
                                                unit = zsunit5,
                                                unitStr = dw_,
                                                barCodeTM = item.zstm,
                                                noCode = item.zs_item_id,
                                                countNum = item.zs_amount,
                                                goods = item.zs_cname,
                                                goodsDes = item.memo,
                                                lsPrice = 0.00m,
                                                hyPrice = 0.00m,
                                                jjPrice = item.yjj_price,
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                isVip = true,
                                                isZS = true,
                                                vtype = 9,
                                            });

                                        }

                                    }

                                }
                            }

                        }
                        else
                        {
                            //不是会员消费

                            //符合条件，可以赠送
                            //判断是否有活动10关联
                            //同名活动10
                            var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).FirstOrDefault();
                            //得判断有没有同名的活动10存在的情况
                            if (YH10ZS != null)
                            {
                                if (YH10ZS.amount <= 0) continue;  //有总量才有得搞

                                //赠品数量
                                decimal ZScount = 1.00m;
                                if (YH10ZS.amount < item.zs_amount)
                                {
                                    ZScount = YH10ZS.amount;
                                }
                                else
                                {
                                    ZScount = item.zs_amount;
                                }

                                //购物车中的商品类别符合的购买数量
                                var lbnum = goodsBuyList.Where(e => e.PP == item.tm && e.vtype == 0).Select(e => e.countNum).Sum();
                                if (lbnum >= item.amount)
                                {
                                    //判断购物车中有没有这个赠品
                                    var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                    if (ZSgood != null)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            goodsBuyList.Add(new GoodsBuy
                                            {
                                                spec = zsinfo100.spec,
                                                pinYin = zsinfo100.py,
                                                unit = zsunit5,
                                                unitStr = dw_,
                                                barCodeTM = item.zstm,
                                                noCode = item.zs_item_id,
                                                countNum = ZScount,
                                                goods = item.zs_cname,
                                                goodsDes = item.memo,
                                                lsPrice = 0.00m,
                                                hyPrice = 0.00m,
                                                jjPrice = item.yjj_price,
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                isGL = true,
                                                isZS = true,
                                                vtype = 9,
                                            });

                                        }

                                    }
                                }



                            }
                            else
                            {
                                //没有关联
                                //购物车中的商品类别符合的购买数量
                                var lbnum = goodsBuyList.Where(e => e.PP == item.tm && e.vtype == 0).Select(e => e.countNum).Sum();
                                if (lbnum >= item.amount)
                                {
                                    //判断购物车中有没有这个赠品
                                    var ZSgood = goodsBuyList.Where(t => t.vtype == 9 && t.isZS && t.noCode == item.zs_item_id).FirstOrDefault();
                                    if (ZSgood != null)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (DialogResult.Yes == MessageBox.Show("此单 " + item.cname + " 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            goodsBuyList.Add(new GoodsBuy
                                            {
                                                spec = zsinfo100.spec,
                                                pinYin = zsinfo100.py,
                                                unit = zsunit5,
                                                unitStr = dw_,
                                                barCodeTM = item.zstm,
                                                noCode = item.zs_item_id,
                                                countNum = item.zs_amount,
                                                goods = item.zs_cname,
                                                goodsDes = item.memo,
                                                lsPrice = 0.00m,
                                                hyPrice = 0.00m,
                                                jjPrice = item.yjj_price,
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                isZS = true,
                                                vtype = 9,
                                            });

                                        }

                                    }
                                }
                            }

                        }



                    }

                }

            }



            #endregion

        }

        /// <summary>
        /// 活动10
        /// </summary>
        /// <param name="db"></param>
        private void YH10ZSFunc(hjnbhEntities db)
        {
            #region 关联其它活动的赠送、优惠总量，也可以是独立活动
            //如果判断过就不再重复
            var ishashhd10 = goodsBuyList.Where(t => t.vtype == 10 && t.isZS).FirstOrDefault();
            if (ishashhd10 != null) return;

            int scode_temp = HandoverModel.GetInstance.scode;
            int VipID = HandoverModel.GetInstance.VipID;
            int viplv = HandoverModel.GetInstance.VipLv;
            //所有活动10
            var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.amount > 0).ToList();
            if (YH10ZS.Count > 0)
            {
                foreach (var item in YH10ZS)
                {
                    //先过滤一下，看购物车中是否有符合活动的普通商品
                    var HasHdItem = goodsBuyList.Where(t => t.vtype == 0 && t.noCode == item.item_id).FirstOrDefault();
                    if (HasHdItem == null) continue;
                    /////////////////////////////
                    //判断是否要求会员
                    if (item.dx_type == 1)
                    {
                        if (VipID != 0)
                        {
                            //判断是否要求会员等级
                            if (viplv >= item.viptype)
                            {
                                //符合活动的普通商品
                                var goodsptList = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 0).ToList();
                                if (goodsptList.Count > 0)
                                {
                                    //在赠送之前先判断还有没有赠品
                                    if (item.amount < 1) continue;
                                    //还要判断在此活动前是否还有赠品赠送出去
                                    var num = goodsptList.Select(t => t.countNum).Sum();
                                    if (item.amount - num <= 0) continue;


                                    if (item.ls_price > 0)
                                    {
                                        if (DialogResult.No == MessageBox.Show("此单 " + item.cname + "价值" + Math.Round(item.ls_price.Value, 2) + " 元，满足限量赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            continue;
                                        }
                                    }

                                    for (int i = 0; i < goodsptList.Count; i++)
                                    {
                                        if (goodsptList[i].countNum > 1)
                                        {
                                            //那么分拆
                                            goodsptList[i].countNum--;
                                            //送一个
                                            goodsBuyList.Add(new GoodsBuy
                                            {
                                                spec = goodsptList[i].spec,
                                                pinYin = goodsptList[i].pinYin,
                                                unit = goodsptList[i].unit,
                                                unitStr = goodsptList[i].unitStr,
                                                noCode = goodsptList[i].noCode,
                                                barCodeTM = goodsptList[i].barCodeTM,
                                                goods = goodsptList[i].goods,
                                                countNum = 1.00m,
                                                lsPrice = Math.Round(item.ls_price.Value, 2),
                                                hyPrice = Math.Round(item.ls_price.Value, 2),
                                                goodsDes = "限量赠品",
                                                jjPrice = Math.Round(item.yjj_price, 2),
                                                pfPrice = Math.Round(item.yls_price, 2),  //记录原价
                                                isVip = true,
                                                isZS = true,
                                                isXG = true,
                                                vtype = 10,
                                                isGL = true

                                            });
                                        }
                                        else if (goodsptList[i].countNum == 1)
                                        {

                                            goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);
                                            goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);
                                            goodsptList[i].goodsDes = "限量赠品";
                                            goodsptList[i].jjPrice = Math.Round(item.yjj_price, 2);
                                            goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);  //记录原价
                                            goodsptList[i].isVip = true;
                                            goodsptList[i].isZS = true;
                                            goodsptList[i].isXG = true;
                                            goodsptList[i].vtype = 10;
                                            goodsptList[i].isGL = true;
                                        }
                                    }
                                }

                            }
                        }
                    }
                    else
                    {
                        //不要求会员的情况
                        //符合活动的普通商品
                        var goodsptList = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 0).ToList();
                        if (goodsptList.Count > 0)
                        {
                            //在赠送之前先判断还有没有赠品
                            if (item.amount < 1) continue;
                            //还要判断在此活动前是否还有赠品赠送出去
                            var num = goodsptList.Select(t => t.countNum).Sum();
                            if (item.amount - num <= 0) continue;

                            if (item.ls_price > 0)
                            {
                                if (DialogResult.No == MessageBox.Show("此单 " + item.cname + "价值" + Math.Round(item.ls_price.Value, 2) + " 元，满足限量赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                {
                                    continue;
                                }
                            }

                            for (int i = 0; i < goodsptList.Count; i++)
                            {
                                if (goodsptList[i].countNum > 1)
                                {
                                    //那么分拆
                                    goodsptList[i].countNum--;
                                    //送一个
                                    goodsBuyList.Add(new GoodsBuy
                                    {
                                        spec = goodsptList[i].spec,
                                        pinYin = goodsptList[i].pinYin,
                                        unit = goodsptList[i].unit,
                                        unitStr = goodsptList[i].unitStr,
                                        noCode = goodsptList[i].noCode,
                                        barCodeTM = goodsptList[i].barCodeTM,
                                        goods = goodsptList[i].goods,
                                        countNum = 1.00m,
                                        lsPrice = Math.Round(item.ls_price.Value, 2),
                                        hyPrice = Math.Round(item.ls_price.Value, 2),
                                        goodsDes = "限量赠品",
                                        jjPrice = Math.Round(item.yjj_price, 2),
                                        pfPrice = Math.Round(item.yls_price, 2),  //记录原价
                                        isZS = true,
                                        isXG = true,
                                        vtype = 10,
                                        isGL = true

                                    });
                                }
                                else if (goodsptList[i].countNum == 1)
                                {

                                    goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);
                                    goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);
                                    goodsptList[i].goodsDes = "限量赠品";
                                    goodsptList[i].jjPrice = Math.Round(item.yjj_price, 2);
                                    goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);  //记录原价
                                    goodsptList[i].isZS = true;
                                    goodsptList[i].isXG = true;
                                    goodsptList[i].vtype = 10;
                                    goodsptList[i].isGL = true;
                                }
                            }
                        }

                    }

                }

            }




            #endregion
        }


        /// <summary>
        /// 活动1 限赠
        /// </summary>
        /// <param name="db"></param>
        private void YH1ZSFunc(hjnbhEntities db)
        {
            //如果判断过就不再重复
            var ishashhd1 = goodsBuyList.Where(t => t.vtype == 1 && t.isZS).FirstOrDefault();
            if (ishashhd1 != null) return;

            int scode_temp = HandoverModel.GetInstance.scode;
            int VipID = HandoverModel.GetInstance.VipID;
            int viplv = HandoverModel.GetInstance.VipLv;

            //所有活动1
            var YH1ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 1 && t.scode == scode_temp).ToList();
            if (YH1ZS.Count > 0)
            {
                foreach (var item in YH1ZS)
                {
                    //先过滤一下，看购物车中是否有符合活动的普通商品
                    var HasHdItem = goodsBuyList.Where(t => t.vtype == 0 && t.noCode == item.item_id).FirstOrDefault();
                    if (HasHdItem == null) continue;

                    //判断是否要求会员
                    if (item.dx_type == 1)
                    {
                        if (VipID != 0)
                        {
                            //判断是否要求会员等级
                            if (viplv >= item.viptype)
                            {
                                //符合活动的普通商品
                                var goodsptList = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 0).ToList();
                                if (goodsptList.Count > 0)
                                {
                                    for (int i = 0; i < goodsptList.Count; i++)
                                    {
                                        int itemid = goodsptList[i].noCode;
                                        //同名活动10
                                        var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == itemid).FirstOrDefault();
                                        //得判断有没有同名的活动10存在的情况
                                        if (YH10ZS != null)
                                        {
                                            //关联的数量,要有数量才有得搞
                                            if (YH10ZS.amount > 0)
                                            {
                                                //查询该会员赠品领取记录
                                                var zshis = db.hd_vip_zs_history.Where(e => e.vipcode == VipID && e.item_id == itemid && e.zstime > item.sbegintime && e.zstime < item.sendtime).ToList();
                                                if (zshis.Count > 0)
                                                {
                                                    //有领取记录
                                                    decimal numed = 0.00m; //已经领取的数量
                                                    foreach (var itemzs in zshis)
                                                    {
                                                        decimal temp = itemzs.zscount.HasValue ? itemzs.zscount.Value : 0;
                                                        numed += temp;
                                                    }
                                                    if (numed >= item.xg_amount) continue;
                                                    //可以领取数量
                                                    decimal numtemp = item.xg_amount - numed;
                                                    //可以领取的总数量
                                                    decimal counttemp = YH10ZS.amount > numtemp ? numtemp : YH10ZS.amount;
                                                    if (goodsptList[i].countNum > counttemp)
                                                    {
                                                        //验证是否加价了
                                                        if (item.ls_price.Value > 0)
                                                        {
                                                            if (DialogResult.No == MessageBox.Show("此赠品" + item.cname + "价值" + Math.Round(item.ls_price.Value, 2) + "元，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                            {
                                                                continue;
                                                            }
                                                        }

                                                        if (goodsptList[i].countNum - counttemp > 0)
                                                        {
                                                            //那么分拆
                                                            goodsptList[i].countNum -= counttemp;
                                                            //送几个
                                                            goodsBuyList.Add(new GoodsBuy
                                                            {
                                                                spec = goodsptList[i].spec,
                                                                pinYin = goodsptList[i].pinYin,
                                                                unit = goodsptList[i].unit,
                                                                unitStr = goodsptList[i].unitStr,
                                                                noCode = goodsptList[i].noCode,
                                                                barCodeTM = goodsptList[i].barCodeTM,
                                                                goods = goodsptList[i].goods,
                                                                countNum = numtemp,
                                                                lsPrice = Math.Round(item.ls_price.Value, 2),
                                                                hyPrice = Math.Round(item.ls_price.Value, 2),
                                                                goodsDes = item.memo,
                                                                jjPrice = Math.Round(item.yjj_price, 2),
                                                                pfPrice = Math.Round(item.yls_price, 2),  //记录原价
                                                                isZS = true,
                                                                isXG = true,
                                                                vtype = 1,
                                                                isGL = true

                                                            });
                                                        }
                                                        else
                                                        {
                                                            goodsptList[i].isGL = true;
                                                            goodsptList[i].isZS = true;
                                                            goodsptList[i].vtype = 1;
                                                            goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);
                                                            goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);
                                                            goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                        }



                                                    }
                                                    else
                                                    {
                                                        goodsptList[i].isGL = true;
                                                        goodsptList[i].isZS = true;
                                                        goodsptList[i].vtype = 1;
                                                        goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);
                                                        goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);
                                                        goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);

                                                    }


                                                }
                                                else
                                                {
                                                    decimal tempnum = item.xg_amount > YH10ZS.amount ? YH10ZS.amount : item.xg_amount;

                                                    //没有领取记录
                                                    //商品不能超过限赠数量
                                                    if (goodsBuyList[i].countNum > tempnum)
                                                    {

                                                        //验证是否加价了
                                                        if (item.ls_price.Value > 0)
                                                        {
                                                            if (DialogResult.No == MessageBox.Show("此赠品" + item.cname + "价值" + Math.Round(item.ls_price.Value, 2) + "元，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                            {
                                                                continue;
                                                            }
                                                        }
                                                        //那么分拆
                                                        if (goodsptList[i].countNum - tempnum > 0)
                                                        {
                                                            goodsptList[i].countNum -= tempnum;
                                                            //送几个
                                                            goodsBuyList.Add(new GoodsBuy
                                                            {
                                                                spec = goodsptList[i].spec,
                                                                pinYin = goodsptList[i].pinYin,
                                                                unit = goodsptList[i].unit,
                                                                unitStr = goodsptList[i].unitStr,
                                                                noCode = goodsptList[i].noCode,
                                                                barCodeTM = goodsptList[i].barCodeTM,
                                                                goods = goodsptList[i].goods,
                                                                countNum = tempnum,
                                                                lsPrice = Math.Round(item.ls_price.Value, 2),
                                                                hyPrice = Math.Round(item.ls_price.Value, 2),
                                                                goodsDes = item.memo,
                                                                jjPrice = Math.Round(item.yjj_price, 2),
                                                                pfPrice = Math.Round(item.yls_price, 2),  //记录原价
                                                                isZS = true,
                                                                isXG = true,
                                                                vtype = 1,
                                                                isGL = true
                                                            });
                                                        }
                                                        else
                                                        {
                                                            goodsptList[i].isGL = true;
                                                            goodsptList[i].isZS = true;
                                                            goodsptList[i].vtype = 1;
                                                            goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);
                                                            goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);
                                                            goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);

                                                        }



                                                    }
                                                    else
                                                    {
                                                        goodsptList[i].isGL = true;
                                                        goodsptList[i].isZS = true;
                                                        goodsptList[i].vtype = 1;
                                                        goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);
                                                        goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);
                                                        goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);

                                                    }

                                                }
                                            }

                                        }
                                        else
                                        {

                                            //没有关联时，不用关联数量
                                            //查询该会员赠品领取记录
                                            var zshis = db.hd_vip_zs_history.Where(e => e.vipcode == VipID && e.item_id == itemid && e.zstime > item.sbegintime && e.zstime < item.sendtime).ToList();
                                            if (zshis.Count > 0)
                                            {
                                                //有领取记录
                                                decimal numed = 0.00m; //已经领取的数量
                                                foreach (var itemzs in zshis)
                                                {
                                                    decimal temp = itemzs.zscount.HasValue ? itemzs.zscount.Value : 0;
                                                    numed += temp;
                                                }
                                                if (numed >= item.xg_amount) continue;
                                                //可以领取数量
                                                decimal numtemp = item.xg_amount - numed;

                                                if (goodsptList[i].countNum > numtemp)
                                                {
                                                    //验证是否加价了
                                                    if (item.ls_price.Value > 0)
                                                    {
                                                        if (DialogResult.No == MessageBox.Show("此赠品" + item.cname + "价值" + Math.Round(item.ls_price.Value, 2) + "元，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                        {
                                                            continue;
                                                        }
                                                    }
                                                    //那么分拆
                                                    if (goodsptList[i].countNum - numtemp > 0)
                                                    {
                                                        goodsptList[i].countNum -= numtemp;
                                                        //送几个
                                                        goodsBuyList.Add(new GoodsBuy
                                                        {
                                                            spec = goodsptList[i].spec,
                                                            pinYin = goodsptList[i].pinYin,
                                                            unit = goodsptList[i].unit,
                                                            unitStr = goodsptList[i].unitStr,
                                                            noCode = goodsptList[i].noCode,
                                                            barCodeTM = goodsptList[i].barCodeTM,
                                                            goods = goodsptList[i].goods,
                                                            countNum = numtemp,
                                                            lsPrice = Math.Round(item.ls_price.Value, 2),
                                                            hyPrice = Math.Round(item.ls_price.Value, 2),
                                                            goodsDes = item.memo,
                                                            jjPrice = Math.Round(item.yjj_price, 2),
                                                            pfPrice = Math.Round(item.yls_price, 2),  //记录原价
                                                            isZS = true,
                                                            isXG = true,
                                                            vtype = 1,
                                                        });
                                                    }
                                                    else
                                                    {
                                                        goodsptList[i].isZS = true;
                                                        goodsptList[i].vtype = 1;
                                                        goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);
                                                        goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);
                                                        goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);

                                                    }

                                                }
                                                else
                                                {
                                                    goodsptList[i].isZS = true;
                                                    goodsptList[i].vtype = 1;
                                                    goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);
                                                    goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);
                                                    goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);

                                                }


                                            }
                                            else
                                            {

                                                //没有领取记录
                                                //商品不能超过限赠数量
                                                if (goodsBuyList[i].countNum > item.xg_amount)
                                                {
                                                    //验证是否加价了
                                                    if (item.ls_price.Value > 0)
                                                    {
                                                        if (DialogResult.No == MessageBox.Show("此赠品" + item.cname + "价值" + Math.Round(item.ls_price.Value, 2) + "元，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                        {
                                                            continue;
                                                        }
                                                    }
                                                    //那么分拆
                                                    if (goodsptList[i].countNum - item.xg_amount > 0)
                                                    {
                                                        goodsptList[i].countNum -= item.xg_amount;
                                                        //送几个
                                                        goodsBuyList.Add(new GoodsBuy
                                                        {
                                                            spec = goodsptList[i].spec,
                                                            pinYin = goodsptList[i].pinYin,
                                                            unit = goodsptList[i].unit,
                                                            unitStr = goodsptList[i].unitStr,
                                                            noCode = goodsptList[i].noCode,
                                                            barCodeTM = goodsptList[i].barCodeTM,
                                                            goods = goodsptList[i].goods,
                                                            countNum = item.xg_amount,
                                                            lsPrice = Math.Round(item.ls_price.Value, 2),
                                                            hyPrice = Math.Round(item.ls_price.Value, 2),
                                                            goodsDes = item.memo,
                                                            jjPrice = Math.Round(item.yjj_price, 2),
                                                            pfPrice = Math.Round(item.yls_price, 2),  //记录原价
                                                            isZS = true,
                                                            isXG = true,
                                                            vtype = 1,
                                                        });
                                                    }
                                                    else
                                                    {
                                                        goodsptList[i].isZS = true;
                                                        goodsptList[i].vtype = 1;
                                                        goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);
                                                        goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);
                                                        goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);

                                                    }

                                                }
                                                else
                                                {
                                                    goodsptList[i].isZS = true;
                                                    goodsptList[i].vtype = 1;
                                                    goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);
                                                    goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);
                                                    goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);

                                                }

                                            }


                                        }

                                    }

                                }

                            }

                        }
                    }
                }
            }


        }



        //活动9 
        void solo_changed(GoodsBuy goods)
        {
            var item3 = goodsBuyList.Where(e => e.vtype == 9 && e.noCode == goods.noCode && e.isZS).FirstOrDefault();
            if (item3 != null)
            {

                MessageBox.Show("赠品数量已经超额，不再累加");
            }
            else
            {
                goodsBuyList.Add(goods);

            }

            textBox1.Text = "";
        }


        //活动5
        void cho5_changed(GoodsBuy goods)
        {
            //var item3 = goodsBuyList.Where(e => e.vtype == 5 && e.noCode == goods.noCode && e.isZS).FirstOrDefault();  //都可以选
            var item3 = goodsBuyList.Where(e => e.vtype == 5 && e.isZS).FirstOrDefault();  //任选其一
            if (item3 != null)
            {

                MessageBox.Show("赠品数量已经超额，不再累加");
            }
            else
            {
                goodsBuyList.Add(goods);

            }

            textBox1.Text = "";
        }





        //处理当会员登记后及时刷新活动调整后的UI
        public void HDUIFunc()
        {

            try
            {
                decimal temp_r = 0;
                for (int i = 0; i < goodsBuyList.Count; i++)
                {
                    if (HandoverModel.GetInstance.VipID == 0)
                    {
                        goodsBuyList[i].isVip = false;
                        temp_r += (goodsBuyList[i].lsPrice.Value * goodsBuyList[i].countNum);
                    }
                    else
                    {
                        goodsBuyList[i].isVip = true;
                        temp_r += (goodsBuyList[i].hyPrice.Value * goodsBuyList[i].countNum);
                    }
                    dataGridView_Cashiers.InvalidateRow(i);  //强制刷新行数据
                }

                label81.Text = temp_r.ToString() + "  元";  //合计金额
                totalMoney = temp_r;
                this.label101.Text = HandoverModel.GetInstance.VipName;

                label3.Visible = true;  //你有新消息……
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("收银主界面下方UI显示异常:", e);
                MessageBox.Show("会员信息刷新异常");
            }

        }

        private void CashiersFormXP_Activated(object sender, EventArgs e)
        {
            if (!textBox1.Focused)
            {
                textBox1.Focus();
                textBox1.SelectAll();
            }

            if (label103.Text == "未登记")
            {
                Tipslabel.Text = "业务员还未登记，请按F3键录入";
            }

            //if (label23.Text == "未登记")
            //{
            //    Tipslabel.Text = "如果是会员消费，请按F12键录入";
            //}

            if (label103.Text == "未登记" && label103.Text == "未登记")
            {
                Tipslabel.Text = "请按F3键录入营业员。如果是会员消费，请按F12键录入";
            }

            //ColumnWidthFunc();
            label6.Text = HandoverModel.GetInstance.scodeName;  //分店名字
            label26.Text = HandoverModel.GetInstance.bcode.ToString();  //机号
        }

        private void dataGridView_Cashiers_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView_Cashiers.Refresh();

            ShowDown();

            textBox1.Focus();  //焦点回到条码输入框
        }

        private void dataGridView_Cashiers_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            try
            {
                //验证第5列，数量
                if (e.ColumnIndex == 5)
                {
                    decimal temp_int = 0.00m;
                    if (decimal.TryParse(e.FormattedValue.ToString(), out temp_int))
                    {
                        e.Cancel = false;
                        dataGridView_Cashiers.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = temp_int.ToString("0.00");
                        dataGridView_Cashiers.InvalidateCell(e.ColumnIndex, e.RowIndex);
                        //dataGridView_Cashiers.InvalidateRow(e.RowIndex);
                        //dataGridView_Cashiers.Refresh();
                    }
                    else
                    {
                        e.Cancel = true;
                        dataGridView_Cashiers.CancelEdit();
                        //MessageBox.Show("数量请输入整数");
                        tipForm.Tiplabel.Text = "商品数量只能输入数字!";
                        tipForm.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("收银主界面修改数量出现异常:", ex);
                MessageBox.Show("提示：表格数量修改验证错误！");
            }
        }


        //显示UI，购物车商品总额，总数量
        public void ShowDown()
        {
            if (goodsBuyList.Count > 0)
            {
                try
                {
                    decimal temp_r = 0;
                    decimal temp_c = 0;
                    foreach (var item in goodsBuyList)
                    {
                        temp_r += item.Sum.Value;
                        temp_c += item.countNum;
                    }

                    label81.Text = temp_r.ToString() + "  元";  //合计金额
                    label82.Text = temp_c.ToString();  //合计数量
                    totalMoney = temp_r;  //获取总金额
                    //changed(totalMoney);  //事件传值给结算
                }
                catch (Exception e)
                {
                    LogHelper.WriteLog("收银主界面下方UI显示异常:", e);
                    MessageBox.Show("收银主界面下方UI显示异常");
                }
            }
            else
            {
                label81.Text = "";
                label82.Text = "";
            }
        }

        private void dataGridView_Cashiers_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView_Cashiers.Rows.Count > 0)
            {
                try
                {
                    if (dataGridView_Cashiers.SelectedRows.Count == 0) return;

                    if (dataGridView_Cashiers.SelectedRows[0].Cells[3].Value != null)
                    {
                        label84.Text = dataGridView_Cashiers.SelectedRows[0].Cells[3].Value.ToString();
                    }
                    if (dataGridView_Cashiers.SelectedRows[0].Cells[9].Value != null)
                    {
                        label83.Text = dataGridView_Cashiers.SelectedRows[0].Cells[9].Value.ToString() + "  元";

                    }
                    if (dataGridView_Cashiers.SelectedRows[0].Cells[17].Value != null)
                    {
                        label31.Text = dataGridView_Cashiers.SelectedRows[0].Cells[17].Value.ToString();
                    }

                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("收银主界面根据选择自动显示商品信息发生异常:", ex);
                }
            }
            else
            {
                label84.Text = "";
                label83.Text = "";
            }

        }



        //登记会员，记录会员ID与姓名、会员等级
        //void vipForm_VIPchanged(int vipid, string vipcrad, int viplv)
        //{
        //    this.VipCARD = vipcrad;
        //    this.VipID = vipid;
        //    this.viplv = viplv;
        //    ReaderVipInfoFunc();
        //    this.lastvipid = vipid;

        //    VipBirthdayFunc();
        //}

        //整单业务员
        void SMFormSMForm_ZDchanged(string s, int id)
        {
            if (s != string.Empty)
            {
                this.label103.Text = s;   //这个显示整单
                for (int i = 0; i < goodsBuyList.Count; i++)
                {
                    goodsBuyList[i].salesClerk = s;
                    goodsBuyList[i].ywy = id;

                }
            }
        }

        /// <summary>
        /// 处理整单打折
        /// </summary>
        /// <param name="d"></param>
        void zkzdform_changed(decimal d)
        {
            if (goodsBuyList.Count == 0) return;
            this.ZKZD = d;
            for (int i = 0; i < goodsBuyList.Count; i++)
            {
                if (goodsBuyList[i].ZKDP != 0) continue;   //忽略已经打折的
                int itemid = goodsBuyList[i].noCode;
                using (var db = new hjnbhEntities())
                {
                    //能否打折 0 选中 1000不选中
                    var zkinfo = db.v_hd_item_info.AsNoTracking().Where(t => t.item_id == itemid).Select(t => t.isale).FirstOrDefault();
                    if (zkinfo == 1)
                    {
                        goodsBuyList[i].hyPrice = Math.Round(goodsBuyList[i].hyPrice.Value * (d / 100), 2);
                        goodsBuyList[i].lsPrice = Math.Round(goodsBuyList[i].lsPrice.Value * (d / 100), 2);
                        goodsBuyList[i].goodsDes += "(" + (d / 10).ToString() + "折" + ")";
                        goodsBuyList[i].ZKDP = d / 100;  //单品折扣
                        dataGridView_Cashiers.InvalidateRow(i);
                    }
                }
            }
            ShowDown();  //刷新合计金额UI
            label32.Text = d.ToString() + "%";  //显示折扣额
        }


        /// <summary>
        /// 处理单品折扣
        /// </summary>
        /// <param name="d"></param>
        void zkform_changed(decimal d)
        {

            int itemid = 0;
            int index = 0;
            if (dataGridView_Cashiers.Rows.Count > 0)
            {
                index = dataGridView_Cashiers.SelectedRows[0].Index;
            }
            if (goodsBuyList[index].ZKDP != 0)
            {
                MessageBox.Show("不允许重复打折！");
                return;  //不允许重复折扣
            }
            itemid = goodsBuyList[index].noCode;
            using (var db = new hjnbhEntities())
            {
                //能否打折 0 选中 1000不选中
                var zkinfo = db.v_hd_item_info.AsNoTracking().Where(t => t.item_id == itemid).Select(t => t.isale).FirstOrDefault();
                if (zkinfo == 1)
                {
                    goodsBuyList[index].hyPrice = Math.Round(goodsBuyList[index].hyPrice.Value * (d / 100), 2);
                    goodsBuyList[index].lsPrice = Math.Round(goodsBuyList[index].lsPrice.Value * (d / 100), 2);
                    goodsBuyList[index].goodsDes += "(" + (d / 10).ToString() + "折" + ")";
                    goodsBuyList[index].ZKDP = d / 100;  //单品折扣
                    dataGridView_Cashiers.InvalidateRow(index);
                    ShowDown();  //刷新合计金额UI
                    label31.Text = d.ToString() + "%";  //显示折扣额
                }
                else
                {
                    MessageBox.Show("该商品不允许打折！");
                }
            }

        }


        int timer_temp = 0;  // 临时变量，解决结算后无法显示结算信息的BUG，因为按回车关闭结算窗口后会同时触发该窗口的回车事件……
        //重写热键方法，这个优先级最高
        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
        {
            int WM_KEYDOWN = 256;
            int WM_SYSKEYDOWN = 260;
            if (msg.Msg == WM_KEYDOWN | msg.Msg == WM_SYSKEYDOWN)
            {
                switch (keyData)
                {
                    ////删除DEL键
                    //case Keys.Delete:

                    //    Dele();

                    //    break;

                    //清空购物车
                    case Keys.Insert:

                        InsertFun();

                        break;


                    //回车
                    case Keys.Enter:

                        EnterFun();
                        if (!textBox1.Focused)
                        {
                            textBox1.Focus();
                        }
                        //textBox1.SelectAll();
                        textBox1.Text = "";  //清空方便下次读码
                        ShowDown(); //刷新UI

                        break;

                    //小键盘+号
                    case Keys.Add:

                        UpdataCount();
                        break;

                    //向上键表格换行
                    case Keys.Up:

                        UpFun();

                        break;

                    //向下键表格换行
                    case Keys.Down:

                        DownFun();

                        break;


                    case Keys.PageUp:

                        UpNote();

                        break;

                    case Keys.PageDown:

                        GetNote();

                        break;
                    //退出
                    case Keys.Escape:
                        OnCashFormESC();

                        break;

                }

            }
            return false;
        }



        //删除单行
        private void Dele()
        {
            try
            {
                //当前行数大于1行时删除选中行后把往上一行设置为选中状态
                if (dataGridView_Cashiers.Rows.Count > 0)
                {
                    DialogResult RSS = MessageBox.Show(this, "确定要删除选中的商品？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    switch (RSS)
                    {
                        case DialogResult.Yes:

                            int DELindex_temp = dataGridView_Cashiers.SelectedRows[0].Index;
                            //dataGridView_Cashiers.Rows.RemoveAt(DELindex_temp);

                            //string de_temp = dataGridView_Cashiers.CurrentRow.Cells[2].Value.ToString();

                            goodsBuyList.RemoveAt(DELindex_temp);
                            dataGridView_Cashiers.Refresh();

                            if (DELindex_temp - 1 >= 0)
                            {
                                dataGridView_Cashiers.Rows[DELindex_temp - 1].Selected = true;
                            }
                            break;
                    }



                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("收银主界面删除选中行发生异常:", ex);
            }
        }

        //删除全部，清空购物车
        private void InsertFun()
        {
            DialogResult RSS = MessageBox.Show(this, "您是否确定清空所有商品？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            switch (RSS)
            {
                case DialogResult.Yes:

                    //if (dataGridView_Cashiers.Rows.Count > 0)
                    //{
                    //    goodsBuyList.Clear();
                    //    initData();   //相当于重置
                    //}

                    goodsBuyList.Clear();
                    initData();   //相当于重置
                    break;
            }

        }


        //回车功能
        private void EnterFun()
        {

            //如果输入框为空且购物车有商品时，则弹出结算窗口
            if (string.IsNullOrEmpty(textBox1.Text) && goodsBuyList.Count > 0 && !dataGridView_Cashiers.IsCurrentCellInEditMode)
            {
                using (var db = new hjnbhEntities())
                {

                    YH3WSFunc(db);   //活动3  买送
                    YH4ZHFunc(db);   //活动4  组合
                    YH2TJFunc(db);   //活动2  商品特价
                    YH7SLFunc(db);   //活动7  限量特价
                    YH6TJFunc(db);   //活动6  时段特价
                    YH9SPFunc(db);   //活动9  商品数量赠送
                    YH9PPFunc(db);   //活动9  品牌赠送
                    YH9LBFunc(db);   //活动9  类别赠送

                    YH1ZSFunc(db);   //活动1  限量赠送
                    YH10ZSFunc(db);  //活动10   赠品限量

                    YH5WEFunc(db);   //活动5  满额送

                    VipDateHDFunc();  //会员日  对VIP商品打折

                    IstoreFunc(db);   //库存提醒



                }



                dataGridView_Cashiers.Refresh();

                foreach (var item in goodsBuyList)
                {
                    lastGoodsList.Add(item);
                }

                ShowDown();

                CEform.CETotalMoney = totalMoney;
                CEform.goodList = goodsBuyList;
                CEform.ShowDialog(this);
            }
            //如果输入框有内容或者购物车没有商品，则进行商品查询
            if (!string.IsNullOrEmpty(textBox1.Text) || goodsBuyList.Count == 0)
            {
                if (isNewItem == false)
                {
                    EFSelectByBarCode();  // 查数据库

                }

            }

            //是否新单
            if (isNewItem)
            {
                //人为地造成需要按两次回车才触发
                timer_temp++;
                if (timer_temp == 2)
                {

                    initData();

                }


            }
        }

        #region 重打小票赋值
        decimal? jf, ysje, ssje, zhaoling;
        decimal vipCradXF, payXF, LQXF;
        public string jsdh, lastVipcard, dateStr;  //上单的
        JSType jstype;

        void CEform_changed(decimal? jf, decimal? ysje, decimal? ssje, string jsdh, JSType jstype, decimal vipCradXF, decimal payXF, decimal LQXF, decimal? zhaoling, string vip, string dateStr)
        {
            this.LQXF = LQXF;
            this.payXF = payXF;
            this.vipCradXF = vipCradXF;
            this.dateStr = dateStr;
            this.jf = jf;
            this.ysje = ysje;
            this.ssje = ssje;
            this.jsdh = jsdh;
            this.jstype = jstype;
            this.zhaoling = zhaoling;
            this.lastVipcard = vip;

            label8.Text = jsdh;  //上单单据
        }
        #endregion

        //小键盘向上
        private void UpFun()
        {
            try
            {
                //当前行数大于1行才生效
                if (dataGridView_Cashiers.Rows.Count > 1)
                {
                    int rowindex_temp = dataGridView_Cashiers.SelectedRows[0].Index;
                    if (rowindex_temp == 0)
                    {
                        dataGridView_Cashiers.Rows[dataGridView_Cashiers.Rows.Count - 1].Selected = true;
                        dataGridView_Cashiers.Rows[rowindex_temp].Selected = false;

                    }
                    else
                    {
                        dataGridView_Cashiers.Rows[rowindex_temp - 1].Selected = true;
                        dataGridView_Cashiers.Rows[rowindex_temp].Selected = false;
                    }

                }

            }
            catch (Exception ex)
            {

                LogHelper.WriteLog("收银窗口小键盘向上时发生异常：" + ex);

            }
        }

        //小键盘向下
        private void DownFun()
        {
            try
            {
                //当前行数大于1行才生效
                if (dataGridView_Cashiers.Rows.Count > 1)
                {
                    int rowindexDown_temp = dataGridView_Cashiers.SelectedRows[0].Index;
                    if (rowindexDown_temp == dataGridView_Cashiers.Rows.Count - 1)
                    {
                        dataGridView_Cashiers.Rows[0].Selected = true;
                        dataGridView_Cashiers.Rows[rowindexDown_temp].Selected = false;

                    }
                    else
                    {
                        dataGridView_Cashiers.Rows[rowindexDown_temp + 1].Selected = true;
                        dataGridView_Cashiers.Rows[rowindexDown_temp].Selected = false;
                    }

                }

            }
            catch (Exception ex)
            {

                LogHelper.WriteLog("收银窗口小键盘向下时发生异常：" + ex);

            }
        }

        //负责挂单的逻辑
        private void UpNote()
        {
            if (goodsBuyList.Count > 0)
            {

                string date_temp = System.DateTime.Now.ToString();

                OrderNo++;

                noteList.Add(
                    new GoodsNoteModel
                    {
                        noNote = OrderNo,
                        upDate = date_temp,
                        cashier = HandoverModel.GetInstance.YWYStr,
                        totalM = totalMoney
                    });

                GNform.dataGridViewGN1.DataSource = noteList;

                var list_temp = goodsBuyList.Select(t => t).ToList<GoodsBuy>();
                var list_2 = new BindingList<GoodsBuy>();

                foreach (var item in list_temp)
                {
                    list_2.Add(item);
                }

                if (!noteDict.ContainsKey(OrderNo))
                {
                    noteDict.Add(OrderNo, list_2);
                }
                //挂单后清屏
                goodsBuyList.Clear();

            }
            else
            {
                tipForm.Tiplabel.Text = "当前无销售商品，空单不能挂起！";
                tipForm.ShowDialog();

            }
            //更新挂单数量
            label98.Text = noteList.Count.ToString();

        }

        //负责取出挂单
        private void GetNote()
        {

            if (goodsBuyList.Count > 0)
            {
                tipForm.Tiplabel.Text = "此单已有商品明细，请先结算或者清除此单后再进行取单操作！";
                tipForm.ShowDialog();
            }
            else
            {
                GNform.ShowDialog(this);
            }
            //更新挂单数量
            label98.Text = noteList.Count.ToString();

        }


        //收银窗口退出，重置所有字段属性
        private void OnCashFormESC()
        {
            if (noteList.Count > 0)
            {
                tipForm.Tiplabel.Text = "您有挂单或未结单，请先取消后再退出前台收银！";
                tipForm.ShowDialog();
            }
            else if (goodsBuyList.Count > 0)
            {
                tipForm.Tiplabel.Text = "请先清空当前商品清单后再退出前台收银！";
                tipForm.ShowDialog();
            }
            else
            {
                initData();
                mainForm.Show();
                this.Close();

                //if (isLianXi)
                //{
                //    isLianXi = false;  //退出练习
                //    this.Close();
                //}
                //else
                //{
                //    isLianXi = false;  //退出练习
                //    this.Hide();
                //    mainForm.Show();

                //this.Close();
                //}


            }


        }

        //每次重置窗口都要重置的数据 
        private void initData()
        {
            if (isLianXi == false)
            {
                label25.Visible = false;
            }

            ZKZD = 0;
            totalMoney = 0;
            isNewItem = false;
            VipMdemo = string.Empty;
            label3.Visible = false;  //你有新消息……
            label4.Visible = false;
            this.tableLayoutPanel2.Visible = false;  //隐藏结算结果

            //this.VipID = 0;  //把会员消费重置为普通消费
            //this.VipCARD = string.Empty;
            //this.viplv = 0;
            //this.VipName = string.Empty;
            HandoverModel.GetInstance.VipLv = 0;
            HandoverModel.GetInstance.VipID = 0;
            HandoverModel.GetInstance.VipName = string.Empty;
            HandoverModel.GetInstance.VipCard = string.Empty;
            HandoverModel.GetInstance.isVipBirthday = false;

            this.label101.Text = "按F12登记会员";
            //this.label99.Text = "未登记";
            label31.Text = "0";  //折扣额
            label32.Text = "0";   //整单折扣
            richTextBox1.Visible = false;  //默认不显示会员信息
            //isMEZS = false;  //重置满额赠送
            //业务员重置
            HandoverModel.GetInstance.YWYid = 0;
            HandoverModel.GetInstance.YWYStr = "";
            this.label103.Text = "未登记";

            timer_temp = 0;  //用于计数

        }


        //按+号修改数量
        private void UpdataCount()
        {
            if (goodsBuyList.Count > 0)
            {
                try
                {
                    dataGridView_Cashiers.CurrentCell = dataGridView_Cashiers.SelectedRows[0].Cells[5];
                    dataGridView_Cashiers.BeginEdit(true);
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("收银主界面按+号修改商品数量发生异常:", ex);
                }
            }

        }

        //会员管理窗口
        private void VIPForm()
        {

            MPForm.ShowDialog(this);

        }

        //负责退货逻辑
        private void Refund()
        {
            //暂时不做整单退货
            //this.label96.Text = "退货";
            //tipForm.code = 1;
            //tipForm.Tiplabel.Text = "按Shift键进行单品退货，按回车键进行整单退货";
            //tipForm.ShowDialog();

            //根据商品条码查询零售明细单
            RDForm = new RefundForm();
            RDForm.ShowDialog();

        }



        //#endregion


        #region 自动在数据表格首列绘制序号


        //表格绘制事件
        private void dataGridView_Cashiers_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            try
            {
                SetDataGridViewRowXh(e, dataGridView_Cashiers);

            }
            catch (Exception)
            {
                MessageBox.Show("提示：表格首列序号绘制出错！");
            }
        }

        //在首列绘制序号，如果首列原有内容，会出现重叠，所以首列手动添加一个空列
        private void SetDataGridViewRowXh(DataGridViewRowPostPaintEventArgs e, DataGridView dataGridView)
        {
            //SolidBrush solidBrush = new SolidBrush(Color.Black); //更改序号样式
            //int xh = e.RowIndex + 1;
            //e.Graphics.DrawString(xh.ToString(CultureInfo.CurrentUICulture), e.InheritedRowStyle.Font, solidBrush, e.RowBounds.Location.X + 5, e.RowBounds.Location.Y + 5);

            SolidBrush b = new SolidBrush(this.dataGridView_Cashiers.RowHeadersDefaultCellStyle.ForeColor);
            e.Graphics.DrawString((e.RowIndex + 1).ToString(System.Globalization.CultureInfo.CurrentUICulture), this.dataGridView_Cashiers.DefaultCellStyle.Font, b, e.RowBounds.Location.X + 5, e.RowBounds.Location.Y + 4);
        }
        #endregion





        //调整表格的列宽、同时隐藏不需要显示的列、禁止编辑、修改列名
        private void UpdateNameFunc()
        {
            try
            {
                //列名
                dataGridView_Cashiers.Columns[1].HeaderText = "货号";
                dataGridView_Cashiers.Columns[2].HeaderText = "条码";
                dataGridView_Cashiers.Columns[3].HeaderText = "品名";
                dataGridView_Cashiers.Columns[4].HeaderText = "规格";
                dataGridView_Cashiers.Columns[5].HeaderText = "数量";
                dataGridView_Cashiers.Columns[7].HeaderText = "单位";
                dataGridView_Cashiers.Columns[9].HeaderText = "零售价";
                dataGridView_Cashiers.Columns[10].HeaderText = "会员价";
                dataGridView_Cashiers.Columns[11].HeaderText = "金额";
                dataGridView_Cashiers.Columns[12].HeaderText = "拼音";
                dataGridView_Cashiers.Columns[13].HeaderText = "备注";
                dataGridView_Cashiers.Columns[14].HeaderText = "营业员";

                //隐藏
                dataGridView_Cashiers.Columns[6].Visible = false;  //单位编码
                dataGridView_Cashiers.Columns[8].Visible = false;  //进价
                dataGridView_Cashiers.Columns[12].Visible = false;  //拼音
                dataGridView_Cashiers.Columns[15].Visible = false;
                dataGridView_Cashiers.Columns[16].Visible = false;
                dataGridView_Cashiers.Columns[17].Visible = false; //折扣
                dataGridView_Cashiers.Columns[18].Visible = false; //批发价
                dataGridView_Cashiers.Columns[19].Visible = false; //活动商品标志
                dataGridView_Cashiers.Columns[20].Visible = false; //VIP标志
                dataGridView_Cashiers.Columns[21].Visible = false; //限购标志
                dataGridView_Cashiers.Columns[22].Visible = false; //活动类型
                dataGridView_Cashiers.Columns[23].Visible = false;  //业务
                dataGridView_Cashiers.Columns[24].Visible = false; //品牌
                dataGridView_Cashiers.Columns[25].Visible = false; //类别
                dataGridView_Cashiers.Columns[26].Visible = false; //是否关联活动10

                //列宽
                dataGridView_Cashiers.Columns[0].Width = 30;
                dataGridView_Cashiers.Columns[1].Width = 80;
                dataGridView_Cashiers.Columns[2].Width = 130;  //条码
                dataGridView_Cashiers.Columns[3].Width = 260;  //品名


                //禁止编辑单元格
                //设置单元格是否可以编辑
                for (int i = 0; i < dataGridView_Cashiers.Columns.Count; i++)
                {
                    if (dataGridView_Cashiers.Columns[i].Index != 5)
                    {
                        dataGridView_Cashiers.Columns[i].ReadOnly = true;
                    }
                }
            }
            catch
            {
            }
        }

        //结单，完成了一单，全部还原重新开始
        public void isNewItems(bool NewOrOld = false)
        {
            if (NewOrOld)
            {
                goodsBuyList.Clear();
                label81.Text = "";
                label82.Text = "";
                label83.Text = "";
                label84.Text = "";
                this.tableLayoutPanel2.Visible = true; //显示结算UI
                isNewItem = true;
                //练习模式下不累计
                if (isLianXi == false)
                {
                    HandoverModel.GetInstance.OrderCount++; //交易单数

                }


            }

        }

        //根据挂单窗口中选择的挂单来显示商品清单
        public BindingList<GoodsBuy> NoteSeleOrder(int order)
        {


            BindingList<GoodsBuy> list_temp = new BindingList<GoodsBuy>();
            noteDict.TryGetValue(order, out list_temp);


            return list_temp;

        }


        //从挂单窗口中取单
        public void GetNoteByorder(int order)
        {

            var list_temp = new BindingList<GoodsBuy>();
            var list_2 = new BindingList<GoodsBuy>();

            noteDict.TryGetValue(order, out list_temp);

            var list = list_temp.Select(t => t).ToList<GoodsBuy>();

            foreach (var item in list)
            {
                goodsBuyList.Add(item);
            }

        }


        #region 显示会员图像
        public Image pic;
        private void ShowVipImaFunc()
        {
            int VipID = HandoverModel.GetInstance.VipID;

            if (VipID == 0) return;
            try
            {
                using (var db = new hjnbhEntities())
                {
                    var ima = db.hd_vip_info.AsNoTracking().Where(t => t.vipcode == VipID).Select(t => t.picture).FirstOrDefault();
                    if (ima != null)
                    {
                        if (ima is byte[])
                        {
                            using (var ms = new MemoryStream(ima, 0, ima.Length))
                            {
                                pic = Image.FromStream(ms);
                                ShowVipPicForm picform = new ShowVipPicForm();
                                picform.ShowDialog();
                            }
                        }
                        else
                        {
                            var pic_temp = ima as byte[];
                            if (pic_temp != null)
                            {
                                using (var ms = new MemoryStream(pic_temp, 0, ima.Length))
                                {
                                    pic = Image.FromStream(ms);
                                    ShowVipPicForm picform = new ShowVipPicForm();
                                    picform.ShowDialog();
                                }
                            }

                        }

                    }
                    else
                    {
                        MessageBox.Show("此会员没有图像数据！");
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("此会员没有图像数据！");
                LogHelper.WriteLog("收银主界面打开会员图像时发生异常:", ex);
            }
        }
        #endregion

        #region 存入会员图像

        //存入会员图像
        private void VipPicWriteFunc()
        {
            int VipID = HandoverModel.GetInstance.VipID;

            if (VipID == 0) return;
            using (var db = new hjnbhEntities())
            {
                var ima = db.hd_vip_info.Where(t => t.vipcode == VipID).Select(t => t.picture).FirstOrDefault();
                if (ima != null)
                {
                    ima = SetImageToByteArray(@"E:\0.png");
                    int re = db.SaveChanges();
                    if (re > 0)
                    {
                        MessageBox.Show("会员图像保存成功！");
                    }
                    else
                    {
                        MessageBox.Show("会员图像保存失败！");

                    }
                }

            }
        }

        //根据文件名(完全路径)
        public byte[] SetImageToByteArray(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open, System.IO.FileAccess.Read, FileShare.ReadWrite);
            int streamLength = (int)fs.Length;
            byte[] image = new byte[streamLength];
            fs.Read(image, 0, streamLength);
            fs.Close();
            return image;
        }
        #endregion

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                //还原窗体显示    
                //this.FormBorderStyle = FormBorderStyle.None;
                this.Show();
                //this.WindowState = FormWindowState.Maximized;
                //激活窗体并给予它焦点
                //this.Activate();
                //this.textBox1.Focus();
                //任务栏区显示图标
                this.ShowInTaskbar = true;
                //托盘区图标隐藏
                notifyIcon1.Visible = false;
            }
        }

        private void CashiersFormXP_SizeChanged(object sender, EventArgs e)
        {
            //判断是否选择的是最小化按钮
            if (WindowState == FormWindowState.Minimized)
            {
                //this.FormBorderStyle = FormBorderStyle.Sizable;
                //隐藏任务栏区图标
                this.ShowInTaskbar = false;
                //图标显示在托盘区
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(500, "提示", "双击或者Pause键可以恢复窗口", ToolTipIcon.Info);
            }

            ////if (WindowState == FormWindowState.Maximized)
            ////{
            ////    激活窗体并给予它焦点
            ////    this.Activate();
            ////    任务栏区显示图标
            ////    this.ShowInTaskbar = true;
            ////    托盘区图标隐藏
            ////    notifyIcon1.Visible = false;
            ////}
        }


        //接受事件的值更新UI 业务员
        private void showYWYuiFunc(string ywy_temp, int id)
        {
            if (id == -1) return;
            if (dataGridView_Cashiers.RowCount > 0)
            {
                int index_temp = dataGridView_Cashiers.SelectedRows[0].Index;

                if (!string.IsNullOrEmpty(ywy_temp))
                {
                    goodsBuyList[index_temp].salesClerk = ywy_temp;
                }

                if (id != -1)
                {
                    goodsBuyList[index_temp].ywy = id;
                }


                dataGridView_Cashiers.Refresh();
            }
            else
            {
                MessageBox.Show("当前购物车中没有商品！");
            }


        }

        public string lastVipName;  //记录为上单的会员名字
        //接受活动事件,会员变动，刷新UI
        private void showVIPuiFunc()
        {
            //this.label99.Text = VIP_temp;
            //this.label101.Text = VIP_temp;
            //this.lastVipName = VIP_temp;
            //this.VipName = VIP_temp;

            //刷新活动
            using (var db = new hjnbhEntities())
            {
                XSHDFunc(db);
                YHHDFunc(db);
            }

            HDUIFunc();  //刷新价格与UI
            ReaderVipInfoFunc(); //显示备注
            VipBirthdayFunc();  //显示会员生日
        }

        private void CashiersFormXP_FormClosing(object sender, FormClosingEventArgs e)
        {
            kh.UnHook();  //快捷键注销


            kh.OnKeyDownEvent -= kh_OnKeyDownEvent;

            CGForm.changed -= CGForm_changed;

            //solo9_SP.changed -= solo_changed;  //活动9
            //solo9_LB.changed -= solo_changed;
            //solo9_PP.changed -= solo_changed;
            cho5.changed -= cho5_changed;


            //业务员事件
            SMFormSMForm.changed -= showYWYuiFunc;
            SMFormSMForm.ZDchanged -= SMFormSMForm_ZDchanged;
            //回车选择商品
            CEform.changed -= CEform_changed;
            CEform.UIChanged -= CEform_UIChanged;


            //会员
            vipForm.changed -= showVIPuiFunc;
            //vipForm.VIPchanged -= vipForm_VIPchanged;

            //打折
            zkform.changed -= zkform_changed;
            zkzdform.changed -= zkzdform_changed;

            //会员备注
            vipmemo.changed -= vipmemo_changed;

        }

        //直接在收银UI上显示会员备注信息(如果登陆会员的话)
        #region 显示会员备注
        //读取会员消息
        private void ReaderVipInfoFunc()
        {
            //try
            //{
            int VipID = HandoverModel.GetInstance.VipID;

            int vipid = VipID;
            if (vipid == 0)
            {
                //MessageBox.Show("请先在收银窗口登记会员卡号");
                richTextBox1.Visible = false;
                return;
            }

            using (var db = new hjnbhEntities())
            {
                var vipInfo = db.hd_vip_info.AsNoTracking().Where(t => t.vipcode == vipid).Select(t => t.sVipMemo).FirstOrDefault();
                if (!string.IsNullOrEmpty(vipInfo))
                {
                    StringBuilder StrB = new StringBuilder();
                    StrB.Append(TextByDateFunc(vipInfo));
                    //richTextBox1.AppendText(StrB.ToString());
                    richTextBox1.Text = StrB.ToString();
                    richTextBox1.Visible = true;
                }
                else
                {
                    richTextBox1.Visible = false;
                }

            }
            //}
            //catch (Exception e)
            //{
            //    LogHelper.WriteLog("会员消息备注窗口读取会员消息时出现异常:", e);
            //    MessageBox.Show("会员信息显示出错！");
            //    //string tip = ConnectionHelper.ToDo();
            //    //if (!string.IsNullOrEmpty(tip))
            //    //{
            //    //    MessageBox.Show(tip);
            //    //}
            //}
        }


        //正则匹配日期
        string strEx = @"^((((1[6-9]|[2-9]\d)\d{2})-(0?[13578]|1[02])-(0?[1-9]|[12]\d|3[01]))|(((1[6-9]|[2-9]\d)\d{2})-(0?[13456789]|1[012])-(0?[1-9]|[12]\d|30))|(((1[6-9]|[2-9]\d)\d{2})-0?2-(0?[1-9]|1\d|2[0-9]))|(((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))-0?2-29-))$";
        //按时间分割文本
        private string TextByDateFunc(string text)
        {
            return (Regex.Replace(text, strEx, "$1\r\n\t"));

        }
        #endregion

        /// <summary>
        /// 结算时判断商品库存
        /// </summary>
        /// <param name="db"></param>
        private void IstoreFunc(hjnbhEntities db)
        {
            bool istip = false; //是否提醒
            int scode_temp = HandoverModel.GetInstance.scode;
            StringBuilder temp = new StringBuilder(); //提醒
            temp.Append("下列商品目前库存不足，请及时补货:" + "\n");

            //创建库存报表
            using (DataTable dt = new DataTable("库存提醒"))
            {
                //创建列
                DataColumn dtc = new DataColumn("品名", typeof(string));
                dt.Columns.Add(dtc);

                dtc = new DataColumn("条码", typeof(string));
                dt.Columns.Add(dtc);

                dtc = new DataColumn("货号", typeof(string));
                dt.Columns.Add(dtc);

                dtc = new DataColumn("库存", typeof(decimal));
                dt.Columns.Add(dtc);

                dtc = new DataColumn("仓库", typeof(int));
                dt.Columns.Add(dtc);

                dtc = new DataColumn("提醒时间", typeof(DateTime));
                dt.Columns.Add(dtc);

                //去掉购物车中重复条码的商品
                var inedx_temp = goodsBuyList.Select(t => t.noCode).Distinct().ToList();
                foreach (var item_index in inedx_temp)
                {
                    var info = goodsBuyList.Where(t => t.noCode == item_index).FirstOrDefault();
                    if (info != null)
                    {

                        ////遍历购物车
                        //foreach (var item in goodsBuyList)
                        //{


                        //查询库存
                        var istoreInfo = db.hd_istore.AsNoTracking().Where(e => e.item_id == info.noCode && e.scode == scode_temp).Select(e => e.amount).FirstOrDefault();
                        if (istoreInfo <= 0)
                        {

                            temp.Append("\t" + info.goods + "(" + info.barCodeTM + ")" + "\n");

                            //添加数据到DataTable
                            DataRow dr = dt.NewRow();

                            dr["品名"] = info.goods;
                            dr["条码"] = info.barCodeTM;
                            dr["货号"] = info.noCode;
                            dr["库存"] = istoreInfo;
                            dr["仓库"] = scode_temp;
                            dr["提醒时间"] = System.DateTime.Now;
                            dt.Rows.Add(dr);

                            istip = true;
                        }

                    }
                }
                temp.Append("\n");
                temp.Append("是否保存为报表？");
                string temp_ = temp.ToString();

                if (istip)
                {
                    if (DialogResult.Yes == MessageBox.Show(temp_, "库存提醒", MessageBoxButtons.YesNo))
                    {
                        if (string.IsNullOrEmpty(HandoverModel.GetInstance.istorePath))
                        {
                            var re = NPOIForExcel.ToExcelWrite(dt, "库存提醒报表");
                            if (re != "")
                            {
                                SvaeConfigFunc(re);
                                HandoverModel.GetInstance.istorePath = re;
                            }
                        }
                        else
                        {
                            var re = NPOIForExcel.ToExcelWrite(dt, "库存提醒报表", "MySheet", HandoverModel.GetInstance.istorePath);

                        }


                    }

                }


            }
        }



        /// <summary>
        /// 保存XML配置文件 , 不存在就创建
        /// </summary>
        /// <param name="path">目录路径</param>
        private void SvaeConfigFunc(string istorepath, string path = @"../")
        {
            try
            {
                if (!System.IO.Directory.Exists((path)))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                string logPath = path + "UserConfig.xml";

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
                                new XElement("scode", 1),  //分店
                                new XElement("cname", "黄金牛儿童百货"),  //分店名字
                                new XElement("index", 0),  //下拉下标，方便下次自动选中此下标位置
                                new XElement("bcode", 1),  //机号
                                new XElement("istorepath", istorepath),  //库存报表路径
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
                    //查询
                    var products = el.Elements("user").Where(e => e.Attribute("ID").Value == "1").FirstOrDefault();
                    if (products != null)
                    {
                        //更改
                        products.SetElementValue("istorepath", istorepath);
                        products.SetElementValue("ctime", System.DateTime.Now.ToShortDateString());

                        el.Save(logPath);
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("保存库存XML时发生异常:", ex);

            }
        }

        private void dataGridView_Cashiers_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            ShowDown();
            UpdateNameFunc();
        }




        //会员日提醒
        private bool VipDateFunc()
        {
            //查询会员日
            using (var db = new hjnbhEntities())
            {
                //会员日
                string vipdate = db.setting.AsNoTracking().Where(t => t.id == 111).Select(t => t.value).FirstOrDefault();
                if (!string.IsNullOrEmpty(vipdate))
                {
                    //分割
                    string[] strInfo = vipdate.Split('|');

                    var dayList = new List<int>();

                    for (int i = 0; i < strInfo[0].Length; i++)
                    {
                        if (strInfo[0][i] == '1')
                        {
                            dayList.Add(i + 1);
                        }
                    }


                    //长度小于17是星期,否则是按月
                    if (vipdate.Length > 17)
                    {
                        foreach (var item in dayList)
                        {
                            if (System.DateTime.Today.Day == item)
                            {
                                //会员日
                                isVipDate = true;
                                vipDateZkl = Convert.ToDecimal(strInfo[1]) / 100;
                                vipDtaeJF = Convert.ToDecimal(strInfo[2]);
                                label2.Text = " 会员日：当天会员消费可以享受原价 " + vipDateZkl * 100 + "% 的优惠，" + vipDtaeJF + "倍的积分增长";
                                label2.Visible = true;
                                return true;
                            }

                        }

                    }
                    else
                    {
                        //按周
                        foreach (var item in dayList)
                        {
                            int temp = (int)System.DateTime.Today.DayOfWeek;
                            if (temp == item)
                            {
                                //会员日
                                isVipDate = true;
                                vipDateZkl = Convert.ToDecimal(strInfo[1]) / 100;
                                vipDtaeJF = Convert.ToDecimal(strInfo[2]);
                                label2.Text = " 会员日：当天会员消费可以享受原价 " + vipDateZkl * 100 + "% 的优惠，" + vipDtaeJF + "倍的积分增长";
                                label2.Visible = true;
                                return true;
                            }

                        }
                    }
                }
            }
            return false;

        }

        //会员日打折
        private void VipDateHDFunc()
        {
            int VipID = HandoverModel.GetInstance.VipID;

            if (isVipDate && VipID != 0)
            {
                for (int i = 0; i < goodsBuyList.Count; i++)
                {
                    if (goodsBuyList[i].vtype == 0 && goodsBuyList[i].isZS == false && goodsBuyList[i].isVip)
                    {
                        decimal temp = goodsBuyList[i].hyPrice.Value * vipDateZkl;
                        goodsBuyList[i].hyPrice = Math.Round(temp, 2);
                        string strtemp = goodsBuyList[i].goodsDes;
                        if (!string.IsNullOrEmpty(strtemp))
                        {
                            if (!strtemp.Contains("会员日优惠"))
                            {
                                goodsBuyList[i].goodsDes += "会员日优惠";
                            }
                        }
                        else
                        {
                            goodsBuyList[i].goodsDes += "会员日优惠";
                        }

                    }


                }
            }




        }

        //更改dataGridView的备注列的文字颜色
        private void dataGridView_Cashiers_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            for (int i = 0; i < goodsBuyList.Count; i++)
            {
                if (goodsBuyList[i].vtype != 0)
                {
                    if (e.ColumnIndex == 13 && e.RowIndex == i)
                    {
                        e.CellStyle.ForeColor = Color.Red;
                    }
                }
                else if (goodsBuyList[i].isVip && goodsBuyList[i].vtype == 0)
                {
                    if (e.ColumnIndex == 13 && e.RowIndex == i)
                    {
                        e.CellStyle.ForeColor = Color.Gold;
                    }
                }

            }

        }


        /// <summary>
        /// 判断是否会员生日
        /// </summary>
        private void VipBirthdayFunc()
        {
            if (HandoverModel.GetInstance.isVipBirthday)
            {
                label4.Visible = true;
            }
            else
            {
                label4.Visible = false;
            }
        }









    }
}
