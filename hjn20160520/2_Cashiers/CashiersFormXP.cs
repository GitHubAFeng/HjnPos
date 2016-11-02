using Common;
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

        MemberPointsForm MPForm = new MemberPointsForm(); //会员管理窗口
        ClosingEntries CEform = new ClosingEntries();  //结算窗口
        SalesmanForm SMFormSMForm = new SalesmanForm(); //业务员录入窗口 
        GoodsNote GNform = new GoodsNote(); //挂单窗口
        MainFormXP mainForm;  //主菜单
        LockScreenForm LSForm;  //锁屏窗口
        RefundForm RDForm = new RefundForm();  //退货窗口
        ChoiceGoods CGForm = new ChoiceGoods(); //商品选择窗口
        VipShopForm vipShopForm = new VipShopForm();//会员消费窗口
        UpdataJEForm JEForm = new UpdataJEForm();  //修改商品金额窗口
        ZKForm zkform = new ZKForm(); //单品折扣
        ZKZDForm zkzdform = new ZKZDForm();  //整单折扣
        VipMemoForm vipmemo = new VipMemoForm();  //会员备注消息
        SysQxValiForm QXForm = new SysQxValiForm();  //权限验证窗口
        bool isQxValied = false;  //权限是否验证通过

        public string jsdh, lastVipcard;  //上单的单据与会员

        //公共提示信息窗口
        TipForm tipForm;
        KeyboardHook kh;  //全局快捷键封装
        //记录购物车内的商品
        public BindingList<GoodsBuy> goodsBuyList = new BindingList<GoodsBuy>();
        //记录购物车中的商品作备份，以便取消结算时还原到活动前
        public BindingList<GoodsBuy> saveGoodsBuyList = new BindingList<GoodsBuy>();
        //记录购物车中的商品作备份，以便取消打折时还原
        //public BindingList<GoodsBuy> saveItemForZKlist = new BindingList<GoodsBuy>();

        //挂单取单窗口的挂单列表
        public BindingList<GoodsNoteModel> noteList = new BindingList<GoodsNoteModel>();
        //挂单窗口中订单号与订单商品清单对应的字典列表
        public Dictionary<int, BindingList<GoodsBuy>> noteDict = new Dictionary<int, BindingList<GoodsBuy>>();

        #region 收银属性

        //整单折扣率
        public decimal ZKZD { get; set; }

        //单号，临时，以后要放上数据库读取
        public int OrderNo = 0;

        //标志一单交易,是否新单
        public bool isNewItem = false;

        //应收总金额
        public decimal totalMoney { get; set; }

        public bool isVipDate = false;  //是否会员日
        public decimal vipDateZkl = 0; //会员日折扣率
        public decimal vipDtaeJF = 0; //会员日积分倍数

        //public int lastvipid;  //记录上单会员id

        private decimal jeToUpdata = 0; //记录修改商品金额

        #endregion
        public CashiersFormXP()
        {
            InitializeComponent();
        }

        //快捷键
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
                    SMFormSMForm.ShowDialog();
                    break;
                //F4键重打小票
                case Keys.F4:

                    RePrintForm reprint = new RePrintForm();
                    reprint.ShowDialog(this);


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

                    zkzdform.ShowDialog(this);

                    break;
                //单品打折
                case Keys.F11:

                    zkform.ShowDialog(this);

                    break;

                //打开会员卡窗口
                case Keys.F12:

                    vipShopForm.ShowDialog();
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

            //会员图像ctrl+P
            //if ((e.KeyCode == Keys.P) && e.Control)
            //{
            //    ShowVipImaFunc();
            //}
            ////导入会员图像ctrl+O
            //if ((e.KeyCode == Keys.O) && e.Control)
            //{
            //    VipPicWriteFunc();
            //}

            //开关库存提醒 ctrl+U
            if ((e.KeyCode == Keys.U) && e.Control)
            {
                isStoreTip = !isStoreTip;
            }


            //刷新会员消息ctrl+Y
            if ((e.KeyCode == Keys.Y) && e.Control)
            {
                ReaderVipInfoFunc();
            }

            //主动刷新活动ctrl+H
            if ((e.KeyCode == Keys.H) && e.Control)
            {
                try
                {
                    HDTipFunc();  //活动详情
                    if (dataGridView1.RowCount > 0)
                    {
                        tabControl1.SelectedIndex = 1;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("收银主窗口主动查询活动详情时出现异常:", ex);
                    MessageBox.Show("查询活动详情时出现异常,请联系管理员！");
                }
            }

            //打开会员备注
            if ((e.KeyCode == Keys.P) && e.Control)
            {
                vipmemo.ShowDialog();
            }

        }

        private void CashiersFormXP_Load(object sender, EventArgs e)
        {

            Init();
            this.ActiveControl = this.textBox1;
            textBox1.Focus();
            vipShopForm.ShowDialog(); //默认使用会员
        }

        //初始化窗口
        private void Init()
        {
            // 窗口全屏设置全屏

            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            //this.TopMost = true;  //窗口顶置

            try
            {
                HDTipFunc();  //活动详情
                if (dataGridView1.RowCount > 0)
                {
                    tabControl1.SelectedIndex = 1;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("收银主窗口查询活动详情时出现异常:", ex);
                MessageBox.Show("查询活动详情时出现异常,请联系管理员！");
            }

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

            if (HandoverModel.GetInstance.isLianxi)
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


            mainForm = new MainFormXP();

            tipForm = new TipForm();
            LSForm = new LockScreenForm();

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

            cho5.changed += cho5_changed;

            //业务员事件
            SMFormSMForm.changed += showYWYuiFunc;
            SMFormSMForm.ZDchanged += SMFormSMForm_ZDchanged;
            //结算事件
            CEform.UIChanged += CEform_UIChanged;
            CEform.changed += CEform_FormESC;

            //会员
            vipShopForm.changed += showVIPuiFunc;

            //会员管理转会员消费
            MPForm.changed += showVIPuiFunc;

            //打折
            zkform.changed += zkform_changed;
            zkzdform.changed += zkzdform_changed;

            //权限验证
            QXForm.Changed += QXForm_Changed;

            //挂单
            GNform.changed += GNform_changed;

            //退货
            RDForm.changed += RDForm_changed;

            //修改金额
            JEForm.changed += JEForm_changed;

        }

        //修改金额逻辑
        void JEForm_changed(decimal je)
        {
            this.jeToUpdata = je;
        }


        //抵额退货的逻辑处理,参数为退货窗口传递过来的退货商品
        void RDForm_changed(BindingList<TuiHuoItemModel> THlist)
        {

            if (THlist.Count > 0)
            {
                foreach (var item in THlist)
                {
                    //先看看有没有重复的
                    var ishasTui = goodsBuyList.Where(t => t.isTuiHuo && t.noCode == item.noCode && t.vtype == item.vtype).FirstOrDefault();
                    if (ishasTui != null)
                    {
                        if (item.MaxTuiCount <= ishasTui.countNum)
                        {
                            MessageBox.Show("退货商品[" + item.goods + "]数量不允许大于已购数量[" + item.MaxTuiCount + "]，请确认您输入的数据是否正确？", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            continue;
                        }
                        else
                        {
                            if (DialogResult.Yes == MessageBox.Show("购物车中已存在该退货商品[" + item.goods + "]，是否需要重复添加？", "提醒", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
                            {
                                ishasTui.countNum += item.countNum;
                                ishasTui.Sum = Math.Round(ishasTui.countNum * ishasTui.hyPrice.Value, 2);
                            }
                        }

                    }
                    else
                    {
                        goodsBuyList.Add(new GoodsBuy
                        {
                            noCode = item.noCode,
                            barCodeTM = item.barCodeTM,
                            countNum = item.countNum, 
                            pfPrice = item.ylsPrice,
                            jjPrice = item.jjPrice,
                            lsPrice = -item.Price,//价格这些是负数
                            hyPrice = -item.Price,
                            Sum = Math.Round(item.countNum * -item.Price,2), 
                            unit = item.unit,
                            unitStr = item.unitStr,
                            spec = item.spec,
                            goods = item.goods,
                            goodsDes = "退货",
                            isXG = true,
                            vtype = item.vtype,
                            isTuiHuo = true
                        });
                    }

                }

                dataGridView_Cashiers.Refresh();
                ShowDown();

            }
        }


        //验证权限
        void QXForm_Changed(bool isValied)
        {
            this.isQxValied = isValied;
        }


        //取消结算事件，把购物车还原到未判断活动的状态
        void CEform_FormESC()
        {
            goodsBuyList.Clear();

            for (int i = 0; i < saveGoodsBuyList.Count; i++)
            {
                goodsBuyList.Add(new GoodsBuy
                {
                    countNum = saveGoodsBuyList[i].countNum,
                    noCode = saveGoodsBuyList[i].noCode,
                    barCodeTM = saveGoodsBuyList[i].barCodeTM,
                    goods = saveGoodsBuyList[i].goods,
                    unit = saveGoodsBuyList[i].unit,
                    unitStr = saveGoodsBuyList[i].unitStr,
                    spec = saveGoodsBuyList[i].spec,
                    lsPrice = saveGoodsBuyList[i].lsPrice,
                    pinYin = saveGoodsBuyList[i].pinYin,
                    salesClerk = saveGoodsBuyList[i].salesClerk,
                    ywy = saveGoodsBuyList[i].ywy,
                    goodsDes = saveGoodsBuyList[i].goodsDes,
                    hpackSize = saveGoodsBuyList[i].hpackSize,
                    jjPrice = saveGoodsBuyList[i].jjPrice,
                    hyPrice = saveGoodsBuyList[i].hyPrice,
                    status = saveGoodsBuyList[i].status,
                    pfPrice = saveGoodsBuyList[i].pfPrice,
                    Sum = saveGoodsBuyList[i].Sum,
                    PP = saveGoodsBuyList[i].PP,
                    LB = saveGoodsBuyList[i].LB,
                    isDbItem = saveGoodsBuyList[i].isDbItem,
                    vtype = saveGoodsBuyList[i].vtype,
                    isTuiHuo = saveGoodsBuyList[i].isTuiHuo,
                    isXG = saveGoodsBuyList[i].isXG,
                    isGL = saveGoodsBuyList[i].isGL,
                    isZS = saveGoodsBuyList[i].isZS,
                    isCyjf = saveGoodsBuyList[i].isCyjf,
                    jfbl = saveGoodsBuyList[i].jfbl
                });


            }

            dataGridView_Cashiers.Refresh();
        }

        //传递挂单
        void GNform_changed(int va, int inde)
        {
            GetNoteByorder(va);
            noteList.RemoveAt(inde);
        }

        /// <summary>
        /// 刷新会员备注 (目前不用，因为内容不一样)
        /// </summary>
        /// <param name="mo"></param>
        void vipmemo_changed()
        {
            ReaderVipInfoFunc();
        }

        /// <summary>
        /// 处理结算后的UI更新
        /// </summary>
        void CEform_UIChanged(decimal getje, decimal toje, decimal zlje, string jsdh, string vip)
        {
            this.jsdh = jsdh;

            this.lastVipcard = vip;

            label8.Text = jsdh;  //上单单据

            isNewItems(true);


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

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label_timer.Text = " 当前时间：" + System.DateTime.Now.ToString();

        }
        //最小化
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



        //根据条码通过EF进行模糊查询
        private void EFSelectByBarCode()
        {
            try
            {
                var BuyListTemp = new BindingList<GoodsBuy>();   //缓存购物车
                string tip_temp = Tipslabel.Text;  //提示文字

                #region 查询操作 0902更新
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

                    var itemsInfo = db.v_xs_item_info.AsNoTracking().Where(t => t.tm.Contains(temptxt) || t.ftm.Contains(temptxt) || t.cname.Contains(temptxt) || t.item_id == itemid_temp)
                        .Where(t => t.scode == HandoverModel.GetInstance.scode || t.scode == 0)
                        .Select(t => new
                        {
                            t.item_id,
                            t.tm,
                            t.cname,
                            t.dw,
                            t.spec,
                            t.scode,
                            t.jj_price,
                            t.ls_price,
                            t.hy_price,
                            t.lb_code,
                            t.pp,
                            t.cyjf,
                            t.jfbl
                        })
                        .ToList();

                    //全部放入缓存先
                    if (itemsInfo.Count > 0)
                    {
                        Tipslabel.Text = "商品正在查询中，请稍等！";

                        if (itemsInfo.Count > 30)
                        {

                            if (DialogResult.No == MessageBox.Show("查询到多个类似的商品，数据量较大时可能造成几秒的卡顿，是否继续查询？", "提醒", MessageBoxButtons.YesNo))
                            {
                                return;
                            }
                        }

                        foreach (var item in itemsInfo)
                        {
                            #region 商品状态、批发价、厂家、拼音、单位编号
                            var itemsOtherInfo = db.hd_item_info.AsNoTracking().Where(t => t.item_id == item.item_id)
                                    .Select(t => new
                                    {
                                        t.unit,
                                        t.py,
                                        t.manufactory,
                                        t.hpack_size,
                                        t.status,
                                        t.pf_price,
                                    })
                                    .FirstOrDefault();

                            #endregion

                            //查询是否打包商品
                            var dbitemifno = db.hd_item_db.AsNoTracking().Where(t => t.sitem_id == item.item_id && t.db_flag == 1 && t.del_flag == 0).FirstOrDefault();
                            if (dbitemifno != null)
                            {
                                BuyListTemp.Add(new GoodsBuy
                                {
                                    noCode = item.item_id,
                                    barCodeTM = item.tm,
                                    goods = item.cname,
                                    unit = itemsOtherInfo.unit.HasValue ? (int)itemsOtherInfo.unit : 1,
                                    unitStr = item.dw,
                                    spec = item.spec,
                                    lsPrice = Convert.ToDecimal(item.ls_price),
                                    pinYin = itemsOtherInfo.py,
                                    salesClerk = HandoverModel.GetInstance.YWYStr,
                                    ywy = HandoverModel.GetInstance.YWYid,
                                    goodsDes = "打包商品",
                                    hpackSize = itemsOtherInfo.hpack_size,
                                    jjPrice = Convert.ToDecimal(item.jj_price),
                                    hyPrice = Convert.ToDecimal(item.hy_price),
                                    status = itemsOtherInfo.status,
                                    pfPrice = Convert.ToDecimal(item.ls_price),
                                    PP = item != null ? item.pp : "",
                                    LB = item != null ? item.lb_code : 0,
                                    isCyjf = item.cyjf==1?true:false,
                                    jfbl = item.jfbl,
                                    isDbItem = true
                                });

                            }
                            else
                            {
                                BuyListTemp.Add(new GoodsBuy
                                {
                                    noCode = item.item_id,
                                    barCodeTM = item.tm,
                                    goods = item.cname,
                                    unit = itemsOtherInfo.unit.HasValue ? (int)itemsOtherInfo.unit : 1,
                                    unitStr = item.dw,
                                    spec = item.spec,
                                    lsPrice = Convert.ToDecimal(item.ls_price),
                                    pinYin = itemsOtherInfo.py,
                                    salesClerk = HandoverModel.GetInstance.YWYStr,
                                    ywy = HandoverModel.GetInstance.YWYid,
                                    goodsDes = itemsOtherInfo.manufactory,
                                    hpackSize = itemsOtherInfo.hpack_size,
                                    jjPrice = Convert.ToDecimal(item.jj_price),
                                    hyPrice = Convert.ToDecimal(item.hy_price),
                                    status = itemsOtherInfo.status,
                                    pfPrice = Convert.ToDecimal(item.ls_price),
                                    isCyjf = item.cyjf == 1 ? true : false,
                                    jfbl = item.jfbl,
                                    PP = item != null ? item.pp : "",
                                    LB = item != null ? item.lb_code : 0
                                });

                            }
                        }
                    }

                    //更新商品售价
                    for (int i = 0; i < BuyListTemp.Count; i++)
                    {
                        BuyListTemp[i].Sum = VipID == 0 ? Math.Round(BuyListTemp[i].countNum * BuyListTemp[i].lsPrice.Value, 2) : Math.Round(BuyListTemp[i].countNum * BuyListTemp[i].hyPrice.Value, 2);
                    }


                    //再判断缓存里面的商品个数
                    if (BuyListTemp.Count > 0)
                    {
                        //把商品转到购物车

                        if (BuyListTemp.Count > 1)
                        {

                            //放上选择列表
                            foreach (var item in BuyListTemp)
                            {
                                decimal hy = item.hyPrice.HasValue ? item.hyPrice.Value : 0.00m;

                                CGForm.ChooseList.Add(new GoodsBuy
                                {
                                    noCode = item.noCode,
                                    barCodeTM = item.barCodeTM,
                                    goods = item.goods,
                                    unit = item.unit,
                                    unitStr = item.unitStr,
                                    spec = item.spec,
                                    lsPrice = item.lsPrice,
                                    pinYin = item.pinYin,
                                    salesClerk = item.salesClerk,
                                    ywy = item.ywy,
                                    goodsDes = item.goodsDes,
                                    hpackSize = item.hpackSize,
                                    jjPrice = item.jjPrice,
                                    hyPrice = hy > 0 ? item.hyPrice : item.lsPrice,
                                    status = item.status,
                                    pfPrice = item.pfPrice,
                                    Sum = item.Sum,
                                    PP = item.PP,
                                    LB = item.LB,
                                    isCyjf = item.isCyjf,
                                    jfbl = item.jfbl,
                                    isDbItem = item.isDbItem,
                                });
                            }

                            CGForm.ShowDialog();

                        }

                        else if (BuyListTemp.Count == 1)
                        {
                            //一件就直接上屏

                            //先判断该商品状态是否允许销售
                            if (BuyListTemp[0].status.Value == 2)
                            {
                                tipForm.Tiplabel.Text = "此商品目前处于停止销售状态！";
                                tipForm.ShowDialog();
                                return;
                            }

                            decimal hy = BuyListTemp[0].hyPrice.HasValue ? BuyListTemp[0].hyPrice.Value : 0.00m;

                            var newGoods_temp = new GoodsBuy()
                            {

                                noCode = BuyListTemp[0].noCode,
                                barCodeTM = BuyListTemp[0].barCodeTM,
                                goods = BuyListTemp[0].goods,
                                unit = BuyListTemp[0].unit,
                                unitStr = BuyListTemp[0].unitStr,
                                spec = BuyListTemp[0].spec,
                                lsPrice = BuyListTemp[0].lsPrice,
                                pinYin = BuyListTemp[0].pinYin,
                                salesClerk = BuyListTemp[0].salesClerk,
                                ywy = BuyListTemp[0].ywy,
                                goodsDes = BuyListTemp[0].goodsDes,
                                hpackSize = BuyListTemp[0].hpackSize,
                                jjPrice = BuyListTemp[0].jjPrice,
                                hyPrice = hy > 0 ? BuyListTemp[0].hyPrice : BuyListTemp[0].lsPrice,
                                status = BuyListTemp[0].status,
                                pfPrice = BuyListTemp[0].pfPrice,
                                PP = BuyListTemp[0].PP,
                                LB = BuyListTemp[0].LB,
                                isDbItem = BuyListTemp[0].isDbItem,
                                isCyjf = BuyListTemp[0].isCyjf,
                                jfbl = BuyListTemp[0].jfbl,
                                Sum = BuyListTemp[0].Sum
                            };

                            if (goodsBuyList.Count == 0)
                            {
                                goodsBuyList.Add(newGoods_temp);

                            }
                            else
                            {

                                CGForm_changed(newGoods_temp);

                            }

                        }
                       

                    }
                    else
                    {
                        //一件也没有找到

                        this.textBox1.SelectAll();
                        tipForm.Tiplabel.Text = "没有查找到该商品!";
                        tipForm.ShowDialog();

                    }

                    Tipslabel.Text = tip_temp;  //重置提示

                #endregion

                }
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("收银主窗口查询商品时出现异常:", e);
                MessageBox.Show("查询商品时出现异常,请联系管理员！");
            }
        }


        // 从商品选择窗口传递回的商品
        void CGForm_changed(GoodsBuy goods)
        {
            var re = goodsBuyList.Where(t => t.noCode == goods.noCode).FirstOrDefault();
            //如果存在还要判定是否数量限购封顶，如果封顶了再另起一组
            if (re != null)
            {
                if (re.isXG == false)
                {
                    re.countNum++;
                    re.Sum = HandoverModel.GetInstance.VipID > 0 ? Math.Round(re.hyPrice.Value * re.countNum, 2) : Math.Round(re.lsPrice.Value * re.countNum, 2);
                    dataGridView_Cashiers.Refresh();

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
                            reXG.Sum = HandoverModel.GetInstance.VipID > 0 ? Math.Round(reXG.hyPrice.Value * reXG.countNum, 2) : Math.Round(reXG.lsPrice.Value * reXG.countNum, 2);

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
            }

            textBox1.Clear();
        }


        #region 活动逻辑


        // 活动6 时段特价
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
                                var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.item_id).ToList();

                                //得判断有没有同名的活动10存在的情况
                                if (YH10ZS.Count > 0)
                                {
                                    //所有符合条件的活动10数量
                                    decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                    if (YH10count <= 0) continue;  //有总量才有得搞
                                    //还可以购买的数量
                                    decimal ZScount = 1.00m;
                                    //关联的数量,要有数量才有得搞
                                    if (YH10count < goodsptList[i].countNum)
                                    {
                                        ZScount = YH10count;
                                    }
                                    else
                                    {
                                        ZScount = goodsptList[i].countNum;
                                    }

                                    //if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足时段特价活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                    //{
                                    //    continue;
                                    //}

                                    //那么来吧，互相伤害
                                    if (goodsptList[i].countNum <= ZScount)
                                    {
                                        goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                        goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                        goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);  //捆绑商品的特价
                                        goodsptList[i].goodsDes = item.memo; //备注
                                        goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                        goodsptList[i].vtype = 6;
                                        goodsptList[i].isGL = true;
                                        goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                    }
                                    else
                                    {
                                        //那么分拆
                                        goodsptList[i].countNum -= ZScount;
                                        goodsptList[i].Sum = VipID > 0 ? Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2) : Math.Round(goodsptList[i].countNum * goodsptList[i].lsPrice.Value, 2);

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
                                            Sum = Math.Round(item.ls_price.Value * ZScount, 2),
                                            jjPrice = item.yjj_price,
                                            pfPrice = Math.Round(item.yls_price, 2),
                                            vtype = 6,
                                            isCyjf = item.isjf == 0 ? true : false,
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
                                    goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                    goodsptList[i].goodsDes = item.memo; //备注
                                    goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                    goodsptList[i].vtype = 6;
                                    goodsptList[i].isCyjf = item.isjf == 0 ? true : false;

                                }
                            }

                        }

                    }
                }
            }
        }


        // 活动2 零售特价  tj_range 0商品 ，1类别 ，2品牌
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
                    #region 商品特价

                    //特价商品
                    if (item.tj_range == 0)
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
                            var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.item_id).ToList();

                            //得判断有没有同名的活动10存在的情况
                            if (YH10ZS.Count > 0)
                            {
                                //所有符合条件的活动10数量
                                decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                if (YH10count <= 0) continue;  //有总量才有得搞
                                //还可以购买的数量
                                decimal ZScount = 1.00m;
                                //关联的数量,要有数量才有得搞
                                if (YH10count < item.xg_amount)
                                {
                                    ZScount = YH10count;
                                }
                                else
                                {
                                    ZScount = item.xg_amount;
                                }


                                //购物车中符合活动的普通商品(不与其它活动重叠)
                                var goodsptList = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 0).ToList();
                                if (goodsptList.Count > 0)
                                {
                                    //if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足购买特价活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                    //{
                                    //    continue;
                                    //}

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
                                                goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                                goodsptList[i].vtype = 2;
                                                goodsptList[i].isGL = true;
                                                goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                            }
                                            else
                                            {
                                                //那么分拆
                                                goodsptList[i].countNum -= ZScount;
                                                goodsptList[i].Sum = VipID > 0 ? Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2) : Math.Round(goodsptList[i].countNum * goodsptList[i].lsPrice.Value, 2);

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
                                                    Sum = Math.Round(item.ls_price.Value * ZScount, 2),
                                                    vtype = 2,
                                                    isCyjf = item.isjf == 0 ? true : false,
                                                    isGL = true,

                                                });

                                            }

                                        }
                                        else
                                        {
                                            //不限购
                                            if (goodsptList[i].countNum > YH10count)
                                            {
                                                ZScount = YH10count;
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
                                                goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                                goodsptList[i].vtype = 2;
                                                goodsptList[i].isGL = true;
                                                goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                            }
                                            else
                                            {
                                                //那么分拆
                                                goodsptList[i].countNum -= ZScount;
                                                goodsptList[i].Sum = VipID > 0 ? Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2) : Math.Round(goodsptList[i].countNum * goodsptList[i].lsPrice.Value, 2);

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
                                                    Sum = Math.Round(item.ls_price.Value * ZScount, 2),
                                                    vtype = 2,
                                                    isCyjf = item.isjf == 0 ? true : false,
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
                                    //if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足购买特价活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                    //{
                                    //    continue;
                                    //}

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
                                                goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                                goodsptList[i].vtype = 2;
                                                goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                            }
                                            else
                                            {
                                                //那么分拆
                                                goodsptList[i].countNum -= item.xg_amount;
                                                goodsptList[i].Sum = VipID > 0 ? Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2) : Math.Round(goodsptList[i].countNum * goodsptList[i].lsPrice.Value, 2);

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
                                                    Sum = Math.Round(item.ls_price.Value * item.xg_amount, 2),
                                                    vtype = 2,
                                                    isCyjf = item.isjf == 0 ? true : false,
                                                });

                                            }

                                        }
                                        else
                                        {
                                            //不限购

                                            goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                            goodsptList[i].goodsDes = item.memo; //备注
                                            goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                            goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                            goodsptList[i].vtype = 2;
                                            goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
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
                            var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.item_id).ToList();

                            //得判断有没有同名的活动10存在的情况
                            if (YH10ZS.Count > 0)
                            {
                                //所有符合条件的活动10数量
                                decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                if (YH10count <= 0) continue;  //有总量才有得搞
                                //还可以购买的数量
                                decimal ZScount = 1.00m;
                                //关联的数量,要有数量才有得搞
                                if (YH10count < item.xg_amount)
                                {
                                    ZScount = YH10count;
                                }
                                else
                                {
                                    ZScount = item.xg_amount;
                                }


                                //购物车中符合活动的普通商品(不与其它活动重叠)
                                var goodsptList = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 0).ToList();
                                if (goodsptList.Count > 0)
                                {
                                    //if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足购买特价活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                    //{
                                    //    continue;
                                    //}

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
                                                goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                                goodsptList[i].vtype = 2;
                                                goodsptList[i].isGL = true;
                                                goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                            }
                                            else
                                            {
                                                //那么分拆
                                                goodsptList[i].countNum -= ZScount;
                                                goodsptList[i].Sum = VipID > 0 ? Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2) : Math.Round(goodsptList[i].countNum * goodsptList[i].lsPrice.Value, 2);

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
                                                    Sum = Math.Round(item.ls_price.Value * ZScount, 2),
                                                    vtype = 2,
                                                    isGL = true,
                                                    isCyjf = item.isjf == 0 ? true : false,
                                                });

                                            }

                                        }
                                        else
                                        {
                                            //如果不限购
                                            //不限购
                                            if (goodsptList[i].countNum > YH10count)
                                            {
                                                ZScount = YH10count;
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
                                                goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                                goodsptList[i].vtype = 2;
                                                goodsptList[i].isGL = true;
                                                goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                            }
                                            else
                                            {
                                                //那么分拆
                                                goodsptList[i].countNum -= ZScount;
                                                goodsptList[i].Sum = VipID > 0 ? Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2) : Math.Round(goodsptList[i].countNum * goodsptList[i].lsPrice.Value, 2);

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
                                                    Sum = Math.Round(item.ls_price.Value * ZScount, 2),
                                                    vtype = 2,
                                                    isGL = true,
                                                    isCyjf = item.isjf == 0 ? true : false,
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
                                    //if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足购买特价活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                    //{
                                    //    continue;
                                    //}

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
                                                goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                                goodsptList[i].vtype = 2;
                                                goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                            }
                                            else
                                            {
                                                //那么分拆
                                                goodsptList[i].countNum -= item.xg_amount;
                                                goodsptList[i].Sum = VipID > 0 ? Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2) : Math.Round(goodsptList[i].countNum * goodsptList[i].lsPrice.Value, 2);

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
                                                    Sum = Math.Round(item.ls_price.Value * item.xg_amount, 2),
                                                    vtype = 2,
                                                    isCyjf = item.isjf == 0 ? true : false,
                                                });

                                            }

                                        }
                                        else
                                        {
                                            //不限购
                                            goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                            goodsptList[i].goodsDes = item.memo; //备注
                                            goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                            goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                            goodsptList[i].vtype = 2;
                                            goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
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
                                    //if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足购买特价活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                    //{
                                    //    continue;
                                    //}

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
                                                goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2); 
                                                goodsptList[i].goodsDes = item.memo; //备注
                                                goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                goodsptList[i].vtype = 2;
                                                goodsptList[i].isGL = true;
                                                goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                            }
                                            else
                                            {
                                                //那么分拆
                                                goodsptList[i].countNum -= ZScount;
                                                goodsptList[i].Sum = VipID > 0 ? Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2) : Math.Round(goodsptList[i].countNum * goodsptList[i].lsPrice.Value, 2);

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
                                                    Sum = Math.Round(item.ls_price.Value * ZScount, 2),
                                                    jjPrice = item.yjj_price,
                                                    pfPrice = Math.Round(item.yls_price, 2),
                                                    vtype = 2,
                                                    isGL = true,
                                                    isCyjf = item.isjf == 0 ? true : false,
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
                                                goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2); 
                                                goodsptList[i].goodsDes = item.memo; //备注
                                                goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                goodsptList[i].vtype = 2;
                                                goodsptList[i].isGL = true;
                                                goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                            }
                                            else
                                            {
                                                //那么分拆
                                                goodsptList[i].countNum -= ZScount;
                                                goodsptList[i].Sum = VipID > 0 ? Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2) : Math.Round(goodsptList[i].countNum * goodsptList[i].lsPrice.Value, 2);

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
                                                    Sum = Math.Round(item.ls_price.Value * ZScount, 2),
                                                    jjPrice = item.yjj_price,
                                                    pfPrice = Math.Round(item.yls_price, 2),
                                                    vtype = 2,
                                                    isGL = true,
                                                    isCyjf = item.isjf == 0 ? true : false,
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
                                    //if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足购买特价活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                    //{
                                    //    continue;
                                    //}

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
                                                goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);  //捆绑商品的特价
                                                goodsptList[i].goodsDes = item.memo; //备注
                                                goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                goodsptList[i].vtype = 2;
                                                goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                            }
                                            else
                                            {
                                                //那么分拆
                                                goodsptList[i].countNum -= item.xg_amount;
                                                goodsptList[i].Sum = VipID > 0 ? Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2) : Math.Round(goodsptList[i].countNum * goodsptList[i].lsPrice.Value, 2);

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
                                                    Sum = Math.Round(item.ls_price.Value * item.xg_amount, 2),
                                                    jjPrice = item.yjj_price,
                                                    pfPrice = Math.Round(item.yls_price, 2),
                                                    vtype = 2,
                                                    isCyjf = item.isjf == 0 ? true : false,
                                                });

                                            }

                                        }
                                        else
                                        {
                                            //不限购
                                            goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                            goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                            goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2); 
                                            goodsptList[i].goodsDes = item.memo; //备注
                                            goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                            goodsptList[i].vtype = 2;
                                            goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                        }
                                    }
                                }

                            }



                        }

                    }
                }
                    
                    #endregion


                    #region 类别特价与品牌特价
                    //类别特价
                    if (item.tj_range == 1 || item.tj_range == 2)
                    {
                        int hdlb = -1; //活动类别编号
                        int.TryParse(item.tm, out hdlb);
                        if (hdlb == -1) continue;

                        if (item.dx_type == 1)  //限定会员
                        {
                            if (VipID == 0) continue;
                            int viplvInfo = item.viptype;
                            //如果会员等级条件满足
                            if (viplv >= viplvInfo)
                            {
                                //同名活动10
                                //var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.item_id).ToList();

                                ////得判断有没有同名的活动10存在的情况
                                //if (YH10ZS.Count > 0)
                                //{
                                //    //所有符合条件的活动10数量
                                //    decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                //    if (YH10count <= 0) continue;  //有总量才有得搞
                                //    //还可以购买的数量
                                //    decimal ZScount = 1.00m;
                                //    //关联的数量,要有数量才有得搞
                                //    if (YH10count < item.xg_amount)
                                //    {
                                //        ZScount = YH10count;
                                //    }
                                //    else
                                //    {
                                //        ZScount = item.xg_amount;
                                //    }


                                //    //购物车中符合活动的普通商品(不与其它活动重叠)
                                //    var goodsptList = goodsBuyList.Where(t => t.vtype == 0).ToList();
                                //    if (goodsptList.Count > 0)
                                //    {
                                //        //if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足购买特价活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                //        //{
                                //        //    continue;
                                //        //}

                                //        for (int i = 0; i < goodsptList.Count; i++)
                                //        {
                                //            //过滤不是此类别的商品
                                //            if (item.tj_range == 1)
                                //            {
                                //                if (isBLFunc(db, goodsptList[i].LB, hdlb) == false) continue;

                                //            }

                                //            //过滤不是此品牌的商品
                                //            if (item.tj_range == 2)
                                //            {
                                //                if (isPPFunc(db, goodsptList[i].noCode, item.tm) == false) continue;

                                //            }


                                //            //那么来吧，互相伤害
                                //            //如果限购
                                //            if (item.xg_amount > 0)
                                //            {
                                //                if (goodsptList[i].countNum <= ZScount)
                                //                {
                                //                    goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                //                    goodsptList[i].goodsDes = item.memo; //备注
                                //                    //goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                //                    goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                //                    goodsptList[i].vtype = 2;
                                //                    goodsptList[i].isGL = true;
                                //                    goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                //                }
                                //                else
                                //                {
                                //                    //那么分拆
                                //                    goodsptList[i].countNum -= ZScount;
                                //                    goodsptList[i].Sum = VipID > 0 ? Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2) : Math.Round(goodsptList[i].countNum * goodsptList[i].lsPrice.Value, 2);

                                //                    goodsBuyList.Add(new GoodsBuy
                                //                    {
                                //                        spec = goodsptList[i].spec,
                                //                        pinYin = goodsptList[i].pinYin,
                                //                        unit = goodsptList[i].unit,
                                //                        unitStr = goodsptList[i].unitStr,
                                //                        barCodeTM = goodsptList[i].barCodeTM,
                                //                        noCode = goodsptList[i].noCode,
                                //                        countNum = ZScount,
                                //                        goods = goodsptList[i].goods,
                                //                        goodsDes = item.memo,
                                //                        lsPrice = goodsptList[i].lsPrice,
                                //                        hyPrice = Math.Round(item.ls_price.Value, 2),
                                //                        jjPrice = goodsptList[i].jjPrice,
                                //                        pfPrice = goodsptList[i].pfPrice,
                                //                        Sum = Math.Round(item.ls_price.Value * ZScount, 2),
                                //                        vtype = 2,
                                //                        isCyjf = item.isjf == 0 ? true : false,
                                //                        isGL = true,

                                //                    });

                                //                }

                                //            }
                                //            else
                                //            {
                                //                //不限购
                                //                if (goodsptList[i].countNum > YH10count)
                                //                {
                                //                    ZScount = YH10count;
                                //                }
                                //                else
                                //                {
                                //                    ZScount = goodsptList[i].countNum;
                                //                }

                                //                if (goodsptList[i].countNum <= ZScount)
                                //                {
                                //                    goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                //                    goodsptList[i].goodsDes = item.memo; //备注
                                //                    //goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                //                    goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                //                    goodsptList[i].vtype = 2;
                                //                    goodsptList[i].isGL = true;
                                //                    goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                //                }
                                //                else
                                //                {
                                //                    //那么分拆
                                //                    goodsptList[i].countNum -= ZScount;
                                //                    goodsptList[i].Sum = VipID > 0 ? Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2) : Math.Round(goodsptList[i].countNum * goodsptList[i].lsPrice.Value, 2);

                                //                    goodsBuyList.Add(new GoodsBuy
                                //                    {
                                //                        spec = goodsptList[i].spec,
                                //                        pinYin = goodsptList[i].pinYin,
                                //                        unit = goodsptList[i].unit,
                                //                        unitStr = goodsptList[i].unitStr,
                                //                        barCodeTM = goodsptList[i].barCodeTM,
                                //                        noCode = goodsptList[i].noCode,
                                //                        countNum = ZScount,
                                //                        goods = goodsptList[i].goods,
                                //                        goodsDes = item.memo,
                                //                        lsPrice = goodsptList[i].lsPrice,
                                //                        hyPrice = Math.Round(item.ls_price.Value, 2),
                                //                        jjPrice = goodsptList[i].jjPrice,
                                //                        pfPrice = goodsptList[i].pfPrice,
                                //                        Sum = Math.Round(item.ls_price.Value * ZScount, 2),
                                //                        vtype = 2,
                                //                        isCyjf = item.isjf == 0 ? true : false,
                                //                        isGL = true,

                                //                    });

                                //                }

                                //            }
                                //        }
                                //    }


                                //}
                                //else
                                //{
                                    //没有关联的情况
                                    //购物车中符合活动的普通商品(不与其它活动重叠)
                                    var goodsptList = goodsBuyList.Where(t =>t.vtype == 0).ToList();
                                    if (goodsptList.Count > 0)
                                    {
                                        //if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足购买特价活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        //{
                                        //    continue;
                                        //}

                                        for (int i = 0; i < goodsptList.Count; i++)
                                        {
                                            //过滤不是此类别的商品
                                            if (item.tj_range == 1)
                                            {
                                                if (isBLFunc(db, goodsptList[i].LB, hdlb) == false) continue;

                                            }

                                            //过滤不是此品牌的商品
                                            if (item.tj_range == 2)
                                            {
                                                if (isPPFunc(db, goodsptList[i].noCode, item.tm) == false) continue;

                                            }

                                            //那么来吧，互相伤害
                                            //如果限购
                                            if (item.xg_amount > 0)
                                            {
                                                if (goodsptList[i].countNum <= item.xg_amount)
                                                {
                                                    goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                    goodsptList[i].goodsDes = item.memo; //备注
                                                    //goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                    goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                                    goodsptList[i].vtype = 2;
                                                    goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                                }
                                                else
                                                {
                                                    //那么分拆
                                                    goodsptList[i].countNum -= item.xg_amount;
                                                    goodsptList[i].Sum = VipID > 0 ? Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2) : Math.Round(goodsptList[i].countNum * goodsptList[i].lsPrice.Value, 2);

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
                                                        jjPrice = goodsptList[i].jjPrice,
                                                        pfPrice = goodsptList[i].pfPrice,
                                                        Sum = Math.Round(item.ls_price.Value * item.xg_amount, 2),
                                                        vtype = 2,
                                                        isCyjf = item.isjf == 0 ? true : false,
                                                    });

                                                }

                                            }
                                            else
                                            {
                                                //不限购

                                                goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                goodsptList[i].goodsDes = item.memo; //备注
                                                //goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                                goodsptList[i].vtype = 2;
                                                goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                            }
                                        }
                                    }

                                //}

                            }
                        }
                        else
                        {
                            //不限定对象

                            //如果是会员消费的情况
                            if (VipID > 0)
                            {
                                ////同名活动10
                                //var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.item_id).ToList();

                                ////得判断有没有同名的活动10存在的情况
                                //if (YH10ZS.Count > 0)
                                //{
                                //    //所有符合条件的活动10数量
                                //    decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                //    if (YH10count <= 0) continue;  //有总量才有得搞
                                //    //还可以购买的数量
                                //    decimal ZScount = 1.00m;
                                //    //关联的数量,要有数量才有得搞
                                //    if (YH10count < item.xg_amount)
                                //    {
                                //        ZScount = YH10count;
                                //    }
                                //    else
                                //    {
                                //        ZScount = item.xg_amount;
                                //    }


                                //    //购物车中符合活动的普通商品(不与其它活动重叠)
                                //    var goodsptList = goodsBuyList.Where(t => t.vtype == 0).ToList();
                                //    if (goodsptList.Count > 0)
                                //    {
                                //        //if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足购买特价活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                //        //{
                                //        //    continue;
                                //        //}

                                //        for (int i = 0; i < goodsptList.Count; i++)
                                //        {
                                //            //过滤不是此类别的商品
                                //            if (item.tj_range == 1)
                                //            {
                                //                if (isBLFunc(db, goodsptList[i].LB, hdlb) == false) continue;

                                //            }

                                //            //过滤不是此品牌的商品
                                //            if (item.tj_range == 2)
                                //            {
                                //                if (isPPFunc(db, goodsptList[i].noCode, item.tm) == false) continue;

                                //            }

                                //            //那么来吧，互相伤害
                                //            //如果限购
                                //            if (item.xg_amount > 0)
                                //            {
                                //                if (goodsptList[i].countNum <= ZScount)
                                //                {
                                //                    goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                //                    goodsptList[i].goodsDes = item.memo; //备注
                                //                    //goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                //                    goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                //                    goodsptList[i].vtype = 2;
                                //                    goodsptList[i].isGL = true;
                                //                    goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                //                }
                                //                else
                                //                {
                                //                    //那么分拆
                                //                    goodsptList[i].countNum -= ZScount;
                                //                    goodsptList[i].Sum = VipID > 0 ? Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2) : Math.Round(goodsptList[i].countNum * goodsptList[i].lsPrice.Value, 2);

                                //                    goodsBuyList.Add(new GoodsBuy
                                //                    {
                                //                        spec = goodsptList[i].spec,
                                //                        pinYin = goodsptList[i].pinYin,
                                //                        unit = goodsptList[i].unit,
                                //                        unitStr = goodsptList[i].unitStr,
                                //                        barCodeTM = goodsptList[i].barCodeTM,
                                //                        noCode = goodsptList[i].noCode,
                                //                        countNum = ZScount,
                                //                        goods = goodsptList[i].goods,
                                //                        goodsDes = item.memo,
                                //                        lsPrice = goodsptList[i].lsPrice,
                                //                        hyPrice = Math.Round(item.ls_price.Value, 2),
                                //                        jjPrice = goodsptList[i].jjPrice,
                                //                        pfPrice = goodsptList[i].pfPrice,
                                //                        Sum = Math.Round(item.ls_price.Value * ZScount, 2),
                                //                        vtype = 2,
                                //                        isGL = true,
                                //                        isCyjf = item.isjf == 0 ? true : false,
                                //                    });

                                //                }

                                //            }
                                //            else
                                //            {
                                //                //如果不限购
                                //                //不限购
                                //                if (goodsptList[i].countNum > YH10count)
                                //                {
                                //                    ZScount = YH10count;
                                //                }
                                //                else
                                //                {
                                //                    ZScount = goodsptList[i].countNum;
                                //                }

                                //                if (goodsptList[i].countNum <= ZScount)
                                //                {
                                //                    goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                //                    goodsptList[i].goodsDes = item.memo; //备注
                                //                    //goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                //                    goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                //                    goodsptList[i].vtype = 2;
                                //                    goodsptList[i].isGL = true;
                                //                    goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                //                }
                                //                else
                                //                {
                                //                    //那么分拆
                                //                    goodsptList[i].countNum -= ZScount;
                                //                    goodsptList[i].Sum = VipID > 0 ? Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2) : Math.Round(goodsptList[i].countNum * goodsptList[i].lsPrice.Value, 2);

                                //                    goodsBuyList.Add(new GoodsBuy
                                //                    {
                                //                        spec = goodsptList[i].spec,
                                //                        pinYin = goodsptList[i].pinYin,
                                //                        unit = goodsptList[i].unit,
                                //                        unitStr = goodsptList[i].unitStr,
                                //                        barCodeTM = goodsptList[i].barCodeTM,
                                //                        noCode = goodsptList[i].noCode,
                                //                        countNum = ZScount,
                                //                        goods = goodsptList[i].goods,
                                //                        goodsDes = item.memo,
                                //                        lsPrice = goodsptList[i].lsPrice,
                                //                        hyPrice = Math.Round(item.ls_price.Value, 2),
                                //                        jjPrice = goodsptList[i].jjPrice,
                                //                        pfPrice = goodsptList[i].pfPrice,
                                //                        Sum = Math.Round(item.ls_price.Value * ZScount, 2),
                                //                        vtype = 2,
                                //                        isGL = true,
                                //                        isCyjf = item.isjf == 0 ? true : false,
                                //                    });

                                //                }



                                //            }
                                //        }
                                //    }


                                //}
                                //else
                                //{
                                    //没有关联的情况
                                    //购物车中符合活动的普通商品(不与其它活动重叠)
                                    var goodsptList = goodsBuyList.Where(t => t.vtype == 0).ToList();
                                    if (goodsptList.Count > 0)
                                    {
                                        //if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足购买特价活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        //{
                                        //    continue;
                                        //}

                                        for (int i = 0; i < goodsptList.Count; i++)
                                        {
                                            //过滤不是此类别的商品
                                            if (item.tj_range == 1)
                                            {
                                                if (isBLFunc(db, goodsptList[i].LB, hdlb) == false) continue;

                                            }

                                            //过滤不是此品牌的商品
                                            if (item.tj_range == 2)
                                            {
                                                if (isPPFunc(db, goodsptList[i].noCode, item.tm) == false) continue;

                                            }

                                            //那么来吧，互相伤害
                                            //如果限购
                                            if (item.xg_amount > 0)
                                            {
                                                if (goodsptList[i].countNum <= item.xg_amount)
                                                {
                                                    goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                    goodsptList[i].goodsDes = item.memo; //备注
                                                    //goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                    goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                                    goodsptList[i].vtype = 2;
                                                    goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                                }
                                                else
                                                {
                                                    //那么分拆
                                                    goodsptList[i].countNum -= item.xg_amount;
                                                    goodsptList[i].Sum = VipID > 0 ? Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2) : Math.Round(goodsptList[i].countNum * goodsptList[i].lsPrice.Value, 2);

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
                                                        jjPrice = goodsptList[i].jjPrice,
                                                        pfPrice = goodsptList[i].pfPrice,
                                                        Sum = Math.Round(item.ls_price.Value * item.xg_amount, 2),
                                                        vtype = 2,
                                                        isCyjf = item.isjf == 0 ? true : false,
                                                    });

                                                }

                                            }
                                            else
                                            {
                                                //不限购
                                                goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                goodsptList[i].goodsDes = item.memo; //备注
                                                //goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                                goodsptList[i].vtype = 2;
                                                goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                            }
                                        }
                                    }

                                //}


                            }
                            else
                            {
                                //非会员消费
                                //同名活动10
                                //var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.item_id).FirstOrDefault();
                                ////得判断有没有同名的活动10存在的情况
                                //if (YH10ZS != null)
                                //{
                                //    if (YH10ZS.amount <= 0) continue;  //有总量才有得搞
                                //    //还可以购买的数量
                                //    decimal ZScount = 1.00m;
                                //    //关联的数量,要有数量才有得搞
                                //    if (YH10ZS.amount < item.xg_amount)
                                //    {
                                //        ZScount = YH10ZS.amount;
                                //    }
                                //    else
                                //    {
                                //        ZScount = item.xg_amount;
                                //    }


                                //    //购物车中符合活动的普通商品(不与其它活动重叠)
                                //    var goodsptList = goodsBuyList.Where(t => t.vtype == 0).ToList();
                                //    if (goodsptList.Count > 0)
                                //    {
                                //        //if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足购买特价活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                //        //{
                                //        //    continue;
                                //        //}

                                //        for (int i = 0; i < goodsptList.Count; i++)
                                //        {
                                //            //过滤不是此类别的商品
                                //            if (item.tj_range == 1)
                                //            {
                                //                if (isBLFunc(db, goodsptList[i].LB, hdlb) == false) continue;

                                //            }

                                //            //过滤不是此品牌的商品
                                //            if (item.tj_range == 2)
                                //            {
                                //                if (isPPFunc(db, goodsptList[i].noCode, item.tm) == false) continue;

                                //            }

                                //            //那么来吧，互相伤害
                                //            //如果限购
                                //            if (item.xg_amount > 0)
                                //            {
                                //                if (goodsptList[i].countNum <= ZScount)
                                //                {
                                //                    goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                //                    goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                //                    goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                //                    goodsptList[i].goodsDes = item.memo; //备注
                                //                    //goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                //                    goodsptList[i].vtype = 2;
                                //                    goodsptList[i].isGL = true;
                                //                    goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                //                }
                                //                else
                                //                {
                                //                    //那么分拆
                                //                    goodsptList[i].countNum -= ZScount;
                                //                    goodsptList[i].Sum = VipID > 0 ? Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2) : Math.Round(goodsptList[i].countNum * goodsptList[i].lsPrice.Value, 2);

                                //                    goodsBuyList.Add(new GoodsBuy
                                //                    {
                                //                        spec = goodsptList[i].spec,
                                //                        pinYin = goodsptList[i].pinYin,
                                //                        unit = goodsptList[i].unit,
                                //                        unitStr = goodsptList[i].unitStr,
                                //                        barCodeTM = goodsptList[i].barCodeTM,
                                //                        noCode = goodsptList[i].noCode,
                                //                        countNum = ZScount,
                                //                        goods = goodsptList[i].goods,
                                //                        goodsDes = item.memo,
                                //                        lsPrice = Math.Round(item.ls_price.Value, 2),
                                //                        hyPrice = Math.Round(item.ls_price.Value, 2),
                                //                        Sum = Math.Round(item.ls_price.Value * ZScount, 2),
                                //                        jjPrice = goodsptList[i].jjPrice,
                                //                        pfPrice = goodsptList[i].pfPrice,
                                //                        vtype = 2,
                                //                        isGL = true,
                                //                        isCyjf = item.isjf == 0 ? true : false,
                                //                    });

                                //                }

                                //            }
                                //            else
                                //            {
                                //                //不限购
                                //                if (goodsptList[i].countNum > YH10ZS.amount)
                                //                {
                                //                    ZScount = YH10ZS.amount;
                                //                }
                                //                else
                                //                {
                                //                    ZScount = goodsptList[i].countNum;
                                //                }

                                //                if (goodsptList[i].countNum <= ZScount)
                                //                {
                                //                    goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                //                    goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                //                    goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                //                    goodsptList[i].goodsDes = item.memo; //备注
                                //                    //goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                //                    goodsptList[i].vtype = 2;
                                //                    goodsptList[i].isGL = true;
                                //                    goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                //                }
                                //                else
                                //                {
                                //                    //那么分拆
                                //                    goodsptList[i].countNum -= ZScount;
                                //                    goodsptList[i].Sum = VipID > 0 ? Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2) : Math.Round(goodsptList[i].countNum * goodsptList[i].lsPrice.Value, 2);

                                //                    goodsBuyList.Add(new GoodsBuy
                                //                    {
                                //                        spec = goodsptList[i].spec,
                                //                        pinYin = goodsptList[i].pinYin,
                                //                        unit = goodsptList[i].unit,
                                //                        unitStr = goodsptList[i].unitStr,
                                //                        barCodeTM = goodsptList[i].barCodeTM,
                                //                        noCode = goodsptList[i].noCode,
                                //                        countNum = ZScount,
                                //                        goods = goodsptList[i].goods,
                                //                        goodsDes = item.memo,
                                //                        lsPrice = Math.Round(item.ls_price.Value, 2),
                                //                        hyPrice = Math.Round(item.ls_price.Value, 2),
                                //                        Sum = Math.Round(item.ls_price.Value * ZScount, 2),
                                //                        jjPrice = goodsptList[i].jjPrice,
                                //                        pfPrice = goodsptList[i].pfPrice,
                                //                        vtype = 2,
                                //                        isGL = true,
                                //                        isCyjf = item.isjf == 0 ? true : false,
                                //                    });

                                //                }

                                //            }
                                //        }
                                //    }


                                //}
                                //else
                                //{
                                    //没有关联的情况
                                    //购物车中符合活动的普通商品(不与其它活动重叠)
                                    var goodsptList = goodsBuyList.Where(t =>t.vtype == 0).ToList();
                                    if (goodsptList.Count > 0)
                                    {
                                        //if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足购买特价活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        //{
                                        //    continue;
                                        //}

                                        for (int i = 0; i < goodsptList.Count; i++)
                                        {
                                            //过滤不是此类别的商品
                                            if (item.tj_range == 1)
                                            {
                                                if (isBLFunc(db, goodsptList[i].LB, hdlb) == false) continue;

                                            }

                                            //过滤不是此品牌的商品
                                            if (item.tj_range == 2)
                                            {
                                                if (isPPFunc(db, goodsptList[i].noCode, item.tm) == false) continue;

                                            }

                                            //那么来吧，互相伤害
                                            //如果限购
                                            if (item.xg_amount > 0)
                                            {
                                                if (goodsptList[i].countNum <= item.xg_amount)
                                                {
                                                    goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                    goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                    goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);  //捆绑商品的特价
                                                    goodsptList[i].goodsDes = item.memo; //备注
                                                    //goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                    goodsptList[i].vtype = 2;
                                                    goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                                }
                                                else
                                                {
                                                    //那么分拆
                                                    goodsptList[i].countNum -= item.xg_amount;
                                                    goodsptList[i].Sum = VipID > 0 ? Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2) : Math.Round(goodsptList[i].countNum * goodsptList[i].lsPrice.Value, 2);

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
                                                        Sum = Math.Round(item.ls_price.Value * item.xg_amount, 2),
                                                        jjPrice = goodsptList[i].jjPrice,
                                                        pfPrice = goodsptList[i].pfPrice,
                                                        vtype = 2,
                                                        isCyjf = item.isjf == 0 ? true : false,
                                                    });

                                                }

                                            }
                                            else
                                            {
                                                //不限购
                                                goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                                goodsptList[i].goodsDes = item.memo; //备注
                                                //goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                goodsptList[i].vtype = 2;
                                                goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                            }
                                        }
                                    }

                                //}



                            }

                        }
                    
                    
                    }
                    #endregion

                }
            }
        }

        // 活动7  限量购买特价单 (目前只限于会员)
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
                            //验证此活动是否有逻辑矛盾
                            if (item.dxg_amount > item.txg_amount)
                            {
                                MessageBox.Show("限量购买特价活动设置错误，每单限购数量不可大于每天限购数量，此活动将不生效，请检查后台活动设置！", "活动提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                continue;
                            }

                            if (item.dxg_amount > item.xg_amount || item.txg_amount > item.xg_amount)
                            {
                                MessageBox.Show("限量购买特价活动设置错误，每单限购或者每天限购数量不可大于总限购数量，此活动将不生效，请检查后台活动设置！", "活动提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                continue;
                            }

                            if (item.dxg_amount <= 0 && item.txg_amount <= 0 && item.xg_amount <= 0)
                            {
                                MessageBox.Show("限量购买特价活动设置错误，没有设置限购数量，此活动将不生效，请检查后台活动设置！", "活动提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                continue;
                            }


                            //同名活动10
                            var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.item_id).ToList();

                            //得判断有没有同名的活动10存在的情况
                            if (YH10ZS.Count > 0)
                            {
                                //所有符合条件的活动10数量
                                decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                if (YH10count <= 0) continue;  //有总量才有得搞
                                //还可以购买的数量
                                decimal ZScount = 1.00m;
                                //关联的数量,要有数量才有得搞
                                if (YH10count < item.xg_amount)
                                {
                                    ZScount = YH10count;
                                }
                                else
                                {
                                    ZScount = item.xg_amount;
                                }

                                //活动时间内该会员的消费记录
                                var viplsList = db.hd_ls.AsNoTracking().Where(t => t.vip == VipID && t.ctime > item.sbegintime && t.ctime < item.sendtime).ToList();
                                if (viplsList.Count > 0)
                                {
                                    decimal ppzsnum = 0; //总共已经购买的数量
                                    decimal daybuynum = 0; //当天已经购买数量
                                    foreach (var itempp in viplsList)
                                    {
                                        //找到这件活动商品的购买记录
                                        var vipls = db.hd_ls_detail.AsNoTracking().Where(t => t.v_code == itempp.v_code && t.item_id == item.item_id && t.vtype == 7).FirstOrDefault();
                                        if (vipls != null)
                                        {
                                            ppzsnum += vipls.amount.Value;  //统计出已经购买数量

                                            if (vipls.ctime.Value.Date == System.DateTime.Now.Date)
                                            {
                                                daybuynum += vipls.amount.Value;
                                            }
                                        }
                                    }

                                    decimal numtemp = 0;  //还可以购买的数量 
                                    decimal sycount = ZScount - ppzsnum; //剩余数量
                                    numtemp = sycount; //默认数量

                                    //每天限购
                                    if (item.txg_amount > 0)
                                    {
                                        decimal daysycount = item.txg_amount - daybuynum; //今天还能购买的数量

                                        if (daysycount <= 0)
                                        {
                                            continue;  //超过每天限购总量就不再参与活动
                                        }
                                        else
                                        {
                                            numtemp = sycount > daysycount ? daysycount : sycount;

                                        }
                                    }

                                    //每单限购
                                    if (item.dxg_amount > 0)
                                    {
                                        numtemp = HasHdItem.countNum - item.dxg_amount > 0 ? item.dxg_amount : HasHdItem.countNum;

                                    }

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
                                        //if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足限量购买特价活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        //{
                                        //    continue;
                                        //}

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
                                                    goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                                    goodsptList[i].vtype = 7;
                                                    goodsptList[i].isGL = true;
                                                    goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                                }
                                                else
                                                {
                                                    //那么分拆
                                                    goodsptList[i].countNum -= numtemp;
                                                    goodsptList[i].Sum = Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2);

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
                                                        Sum = Math.Round(item.ls_price.Value * numtemp, 2),
                                                        vtype = 7,
                                                        isGL = true,
                                                        isCyjf = item.isjf == 0 ? true : false,
                                                    });

                                                }


                                            }

                                        }
                                    }

                                }
                                else
                                {
                                    //该会员没有消费记录


                                    decimal numtemp = 0;  //还可以购买的数量 
                                    if (item.xg_amount > 0) numtemp = item.xg_amount;
                                    if (item.txg_amount > 0) numtemp = item.xg_amount > item.txg_amount ? item.txg_amount : item.xg_amount;
                                    if (item.dxg_amount > 0) numtemp = item.xg_amount > item.dxg_amount ? item.dxg_amount : item.xg_amount;
                                    numtemp = ZScount > numtemp ? numtemp : ZScount;

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
                                        //if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足限量购买特价活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        //{
                                        //    continue;
                                        //}

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
                                                    goodsptList[i].Sum = Math.Round(goodsptList[i].hyPrice.Value * goodsptList[i].countNum, 2);
                                                    goodsptList[i].vtype = 7;
                                                    goodsptList[i].isGL = true;
                                                    goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                                }
                                                else
                                                {
                                                    //那么分拆
                                                    goodsptList[i].countNum -= numtemp;
                                                    goodsptList[i].Sum = Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2);

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
                                                        Sum = Math.Round(item.ls_price.Value * numtemp, 2),
                                                        vtype = 7,
                                                        isGL = true,
                                                        isCyjf = item.isjf == 0 ? true : false,
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
                                    decimal daybuynum = 0; //当天已经购买数量

                                    foreach (var itempp in viplsList)
                                    {
                                        //找到这件活动商品的购买记录
                                        var vipls = db.hd_ls_detail.AsNoTracking().Where(t => t.v_code == itempp.v_code && t.item_id == item.item_id && t.vtype == 7).FirstOrDefault();
                                        if (vipls != null)
                                        {
                                            ppzsnum += vipls.amount.Value;  //统计出已经购买数量


                                            if (vipls.ctime.Value.Date == System.DateTime.Now.Date)
                                            {
                                                daybuynum += vipls.amount.Value;
                                            }
                                        }
                                    }

                                    decimal numtemp = 0;  //还可以购买的数量 
                                    decimal sycount = item.xg_amount - ppzsnum; //剩余数量
                                    numtemp = sycount; //默认数量

                                    //每天限购
                                    if (item.txg_amount > 0)
                                    {
                                        decimal daysycount = item.txg_amount - daybuynum; //今天还能购买的数量

                                        if (daysycount <= 0)
                                        {
                                            continue;  //超过每天限购总量就不再参与活动
                                        }
                                        else
                                        {
                                            numtemp = sycount > daysycount ? daysycount : sycount;

                                        }
                                    }

                                    //每单限购
                                    if (item.dxg_amount > 0)
                                    {
                                        numtemp = HasHdItem.countNum - item.dxg_amount > 0 ? item.dxg_amount : HasHdItem.countNum;
                                    }

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
                                        //if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足限量购买特价活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        //{
                                        //    continue;
                                        //}

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
                                                    goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                                    goodsptList[i].vtype = 7;
                                                    goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                                }
                                                else
                                                {
                                                    //那么分拆
                                                    goodsptList[i].countNum -= numtemp;
                                                    goodsptList[i].Sum = Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2);

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
                                                        Sum = Math.Round(item.ls_price.Value * numtemp, 2),
                                                        vtype = 7,
                                                        isCyjf = item.isjf == 0 ? true : false,
                                                    });

                                                }


                                            }

                                        }
                                    }

                                }
                                else
                                {
                                    //该会员没有消费记录

                                    decimal numtemp = 0;  //还可以购买的数量 
                                    if (item.xg_amount > 0) numtemp = item.xg_amount;
                                    if (item.txg_amount > 0) numtemp = item.xg_amount > item.txg_amount ? item.txg_amount : item.xg_amount;
                                    if (item.dxg_amount > 0) numtemp = item.xg_amount > item.dxg_amount ? item.dxg_amount : item.xg_amount;


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

                                        //if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足限量购买特价活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        //{
                                        //    continue;
                                        //}

                                        for (int i = 0; i < goodsptList.Count; i++)
                                        {
                                            //那么来吧，互相伤害
                                            if (goodsptList[i].countNum <= numtemp)
                                            {
                                                goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);  //捆绑商品的特价
                                                goodsptList[i].goodsDes = item.memo; //备注
                                                goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                                goodsptList[i].vtype = 7;
                                                goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                            }
                                            else
                                            {
                                                //那么分拆
                                                goodsptList[i].countNum -= numtemp;
                                                goodsptList[i].Sum = Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2);

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
                                                    Sum = Math.Round(item.ls_price.Value * numtemp, 2),
                                                    vtype = 7,
                                                    isCyjf = item.isjf == 0 ? true : false,

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

        // 活动3 买多送多
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
                                    var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).ToList();

                                    //得判断有没有同名的活动10存在的情况
                                    if (YH10ZS.Count > 0)
                                    {
                                        //所有符合条件的活动10数量
                                        decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                        //关联的数量,要有数量才有得搞
                                        if (YH10count > 0)
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
                                            //if (YH10ZS.amount < item.zs_amount * peisu) continue;

                                            if (YH10count < item.zs_amount * peisu)
                                            {
                                                peisu = Math.Floor(YH10count / item.zs_amount);
                                                sesu = goodsptList[i].countNum - item.amount * peisu;

                                            }

                                            if (item.ls_price.Value > 0)
                                            {
                                                //没有的话直接送，并更正价格
                                                if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足买满赠送活动，此零售价将调整为：" + Math.Round(item.ls_price.Value, 2) + " 元，可领取赠品：[" + item.zs_cname + "]，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                {
                                                    continue;
                                                }
                                            }


                                            //分拆
                                            if (sesu == 0)
                                            {
                                                goodsptList[i].lsPrice = Math.Round(item.ls_price.Value / item.amount, 2);
                                                goodsptList[i].hyPrice = Math.Round(item.ls_price.Value / item.amount, 2);
                                                goodsptList[i].goodsDes = item.memo;
                                                goodsptList[i].jjPrice = Math.Round(item.yjj_price, 2);
                                                goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);  //记录原价
                                                goodsptList[i].Sum = Math.Round((item.ls_price.Value / item.amount) * goodsptList[i].countNum, 2);
                                                goodsptList[i].isXG = true;
                                                goodsptList[i].vtype = 3;
                                                goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                            }
                                            else
                                            {
                                                //分拆
                                                goodsptList[i].countNum = sesu;
                                                goodsptList[i].Sum = VipID > 0 ? Math.Round(goodsptList[i].hyPrice.Value * goodsptList[i].countNum, 2) : Math.Round(goodsptList[i].lsPrice.Value * goodsptList[i].countNum, 2);

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
                                                    isCyjf = item.isjf == 0 ? true : false,
                                                    isXG = true,
                                                    vtype = 3,
                                                    Sum = Math.Round((item.ls_price.Value / item.amount) * (item.amount * peisu), 2),

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
                                                    ZSitem.Sum = VipID > 0 ? Math.Round(ZSitem.hyPrice.Value * ZSitem.countNum, 2) : Math.Round(ZSitem.lsPrice.Value * ZSitem.countNum, 2);

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
                                                        isCyjf = item.isjf == 0 ? true : false,
                                                        Sum = 0.00m

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
                                                    ZSitem.Sum = 0.00m;
                                                    ZSitem.goodsDes = item.memo;
                                                    ZSitem.isGL = true;
                                                    ZSitem.isCyjf = item.isjf == 0 ? true : false;
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
                                                    isCyjf = item.isjf == 0 ? true : false,
                                                    isGL = true,
                                                    Sum = 0.00m
                                                });
                                            }

                                        }


                                    }
                                    else
                                    {
                                        //没有关联

                                        //是否达到数量条件
                                        if (goodsptList[i].countNum < item.amount) continue;
                                        if (item.ls_price.Value > 0)
                                        {
                                            //没有的话直接送，并更正价格
                                            if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足买满赠送活动，此零售价将调整为：" + Math.Round(item.ls_price.Value, 2) + " 元，可领取赠品：[" + item.zs_cname + "]，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                            {
                                                continue;
                                            }
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
                                            goodsptList[i].Sum = Math.Round((item.ls_price.Value / item.amount) * goodsptList[i].countNum, 2);
                                            goodsptList[i].goodsDes = item.memo;
                                            goodsptList[i].jjPrice = Math.Round(item.yjj_price, 2);
                                            goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);  //记录原价
                                            goodsptList[i].isXG = true;
                                            goodsptList[i].vtype = 3;
                                            goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                        }
                                        else
                                        {
                                            //分拆
                                            goodsptList[i].countNum = sesu;
                                            goodsptList[i].Sum = VipID > 0 ? Math.Round(goodsptList[i].hyPrice.Value * goodsptList[i].countNum, 2) : Math.Round(goodsptList[i].lsPrice.Value * goodsptList[i].countNum, 2);

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
                                                isCyjf = item.isjf == 0 ? true : false,
                                                isXG = true,
                                                vtype = 3,
                                                Sum = Math.Round((item.ls_price.Value / item.amount) * (item.amount * peisu), 2),
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
                                                ZSitem.Sum = VipID > 0 ? Math.Round(ZSitem.hyPrice.Value * ZSitem.countNum, 2) : Math.Round(ZSitem.lsPrice.Value * ZSitem.countNum, 2);

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
                                                    isCyjf = item.isjf == 0 ? true : false,
                                                    Sum = 0.00m

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
                                                ZSitem.Sum = 0.00m;
                                                ZSitem.goodsDes = item.memo;
                                                ZSitem.isCyjf = item.isjf == 0 ? true : false;
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
                                                isCyjf = item.isjf == 0 ? true : false,
                                                Sum = 0.00m
                                            });
                                        }
                                    }
                                }

                            }
                        }
                    }

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
                                var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).ToList();

                                //得判断有没有同名的活动10存在的情况
                                if (YH10ZS.Count > 0)
                                {
                                    //所有符合条件的活动10数量
                                    decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                    //关联的数量,要有数量才有得搞
                                    if (YH10count > 0)
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

                                        //if (YH10ZS.amount < item.zs_amount * peisu) continue;
                                        //如果活动10的赠品不够就用送这活动10还剩下的
                                        if (YH10count < item.zs_amount * peisu)
                                        {
                                            peisu = Math.Floor(YH10count / item.zs_amount);
                                            sesu = goodsptList[i].countNum - item.amount * peisu;

                                        }

                                        if (item.ls_price.Value > 0)
                                        {
                                            //没有的话直接送，并更正价格
                                            if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足买满赠送活动，此零售价将调整为：" + Math.Round(item.ls_price.Value, 2) + " 元，可领取赠品：[" + item.zs_cname + "]，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                            {
                                                continue;
                                            }
                                        }


                                        if (sesu == 0)
                                        {
                                            goodsptList[i].lsPrice = Math.Round(item.ls_price.Value / item.amount, 2);
                                            goodsptList[i].hyPrice = Math.Round(item.ls_price.Value / item.amount, 2);
                                            goodsptList[i].Sum = Math.Round((item.ls_price.Value / item.amount) * goodsptList[i].countNum, 2);
                                            goodsptList[i].goodsDes = item.memo;
                                            goodsptList[i].jjPrice = Math.Round(item.yjj_price, 2);
                                            goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);  //记录原价
                                            goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                            goodsptList[i].isXG = true;
                                            goodsptList[i].vtype = 3;
                                        }
                                        else
                                        {
                                            //分拆
                                            goodsptList[i].countNum = sesu;
                                            goodsptList[i].Sum = VipID > 0 ? Math.Round(goodsptList[i].hyPrice.Value * goodsptList[i].countNum, 2) : Math.Round(goodsptList[i].lsPrice.Value * goodsptList[i].countNum, 2);

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
                                                Sum = Math.Round((item.ls_price.Value / item.amount) * item.amount * peisu, 2),
                                                goodsDes = item.memo,
                                                jjPrice = Math.Round(item.yjj_price, 2),
                                                pfPrice = Math.Round(item.yls_price, 2),  //记录原价
                                                isCyjf = item.isjf == 0 ? true : false,
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
                                                ZSitem.Sum = VipID > 0 ? Math.Round(ZSitem.hyPrice.Value * ZSitem.countNum, 2) : Math.Round(ZSitem.lsPrice.Value * ZSitem.countNum, 2);

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
                                                    Sum = 0.00m,
                                                    goodsDes = item.memo,
                                                    jjPrice = Math.Round(item.zs_yjjprice, 2),
                                                    pfPrice = Math.Round(item.zs_ylsprice, 2),  //记录原价
                                                    isZS = true,
                                                    isXG = true,
                                                    vtype = 3,
                                                    isCyjf = item.isjf == 0 ? true : false,
                                                    isGL = true

                                                });

                                            }
                                            else
                                            {
                                                ZSitem.countNum = item.zs_amount * peisu;
                                                ZSitem.lsPrice = 0.00m;
                                                ZSitem.hyPrice = 0.00m;
                                                ZSitem.Sum = 0.00m;
                                                ZSitem.pfPrice = Math.Round(item.zs_ylsprice, 2);
                                                ZSitem.vtype = 3;
                                                ZSitem.isZS = true;
                                                ZSitem.isXG = true;
                                                ZSitem.goodsDes = item.memo;
                                                ZSitem.isGL = true;
                                                ZSitem.isCyjf = item.isjf == 0 ? true : false;
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
                                                Sum = 0.00m,
                                                goodsDes = item.memo,
                                                jjPrice = Math.Round(item.zs_yjjprice, 2),
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),  //记录原价
                                                isZS = true,
                                                isXG = true,
                                                vtype = 3,
                                                isCyjf = item.isjf == 0 ? true : false,
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

                                    if (item.ls_price.Value > 0)
                                    {
                                        //没有的话直接送，并更正价格
                                        if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足买满赠送活动，此零售价将调整为：" + Math.Round(item.ls_price.Value, 2) + " 元，可领取赠品：[" + item.zs_cname + "]，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            continue;
                                        }
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
                                        goodsptList[i].Sum = Math.Round((item.ls_price.Value / item.amount) * goodsptList[i].countNum, 2);
                                        goodsptList[i].goodsDes = item.memo;
                                        goodsptList[i].jjPrice = Math.Round(item.yjj_price, 2);
                                        goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);  //记录原价
                                        goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                        goodsptList[i].isXG = true;
                                        goodsptList[i].vtype = 3;
                                    }
                                    else
                                    {
                                        //分拆
                                        goodsptList[i].countNum = sesu;
                                        goodsptList[i].Sum = VipID > 0 ? Math.Round(goodsptList[i].hyPrice.Value * goodsptList[i].countNum, 2) : Math.Round(goodsptList[i].lsPrice.Value * goodsptList[i].countNum, 2);

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
                                            Sum = Math.Round((item.ls_price.Value / item.amount) * item.amount * peisu, 2),
                                            goodsDes = item.memo,
                                            jjPrice = Math.Round(item.yjj_price, 2),
                                            pfPrice = Math.Round(item.yls_price, 2),  //记录原价
                                            isCyjf = item.isjf == 0 ? true : false,
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
                                            ZSitem.Sum = VipID > 0 ? Math.Round(ZSitem.hyPrice.Value * ZSitem.countNum, 2) : Math.Round(ZSitem.lsPrice.Value * ZSitem.countNum, 2);

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
                                                Sum = 0.00m,
                                                goodsDes = item.memo,
                                                jjPrice = Math.Round(item.zs_yjjprice, 2),
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),  //记录原价
                                                isZS = true,
                                                isXG = true,
                                                vtype = 3,
                                                isCyjf = item.isjf == 0 ? true : false,
                                            });

                                        }
                                        else
                                        {
                                            ZSitem.countNum = item.zs_amount * peisu;
                                            ZSitem.lsPrice = 0.00m;
                                            ZSitem.hyPrice = 0.00m;
                                            ZSitem.Sum = 0.00m;
                                            ZSitem.pfPrice = Math.Round(item.zs_ylsprice, 2);
                                            ZSitem.vtype = 3;
                                            ZSitem.isZS = true;
                                            ZSitem.isXG = true;
                                            ZSitem.goodsDes = item.memo;
                                            ZSitem.isCyjf = item.isjf == 0 ? true : false;
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
                                            Sum = 0.00m,
                                            goodsDes = item.memo,
                                            jjPrice = Math.Round(item.zs_yjjprice, 2),
                                            pfPrice = Math.Round(item.zs_ylsprice, 2),  //记录原价
                                            isZS = true,
                                            isXG = true,
                                            vtype = 3,
                                            isCyjf = item.isjf == 0 ? true : false,
                                        });
                                    }
                                }
                            }
                        }

                    }

                }

            }
        }

        // 活动4 组合优惠
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
                    if (item.item_id == item.zs_item_id) continue;  //禁止组合A与B是同一个商品
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
                            //if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 与 [" + item.zs_cname + "] 满足组合优惠活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                            //{
                            //    continue;
                            //}
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
                                    HDitem.isCyjf = item.isjf == 0 ? true : false;
                                    HDitem.Sum = Math.Round((item.ls_price.Value / item.amount) * (HDitem.countNum), 2);
                                }
                                else
                                {
                                    HDitem.countNum -= item.amount * PeiSu;
                                    HDitem.Sum = VipID > 0 ? Math.Round(HDitem.hyPrice.Value * HDitem.countNum, 2) : Math.Round(HDitem.lsPrice.Value * HDitem.countNum, 2);

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
                                        isCyjf = item.isjf == 0 ? true : false,
                                        Sum = Math.Round((item.ls_price.Value / item.amount) * (item.amount * PeiSu), 2),

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
                                    ZSitem.isCyjf = item.isjf == 0 ? true : false;
                                    ZSitem.Sum = 0.00m;
                                }
                                else
                                {
                                    ZSitem.countNum -= item.zs_amount * PeiSu;
                                    ZSitem.Sum = VipID > 0 ? Math.Round(ZSitem.hyPrice.Value * ZSitem.countNum, 2) : Math.Round(ZSitem.lsPrice.Value * ZSitem.countNum, 2);

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
                                        isCyjf = item.isjf == 0 ? true : false,
                                        Sum = 0.00m

                                    });

                                }

                        }
                    }

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
                        //if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 与 [" + item.zs_cname + "] 满足组合优惠活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                        //{
                        //    continue;
                        //}
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
                                HDitem.Sum = Math.Round((item.ls_price.Value / item.amount) * HDitem.countNum, 2);
                                HDitem.jjPrice = Math.Round(item.yjj_price, 2);
                                HDitem.pfPrice = Math.Round(item.yls_price, 2);
                                HDitem.goodsDes = item.memo;
                                HDitem.vtype = 4;
                                HDitem.isCyjf = item.isjf == 0 ? true : false;
                            }
                            else if (HdtempNum > 0)
                            {
                                HDitem.countNum -= item.amount * PeiSu;
                                HDitem.Sum = VipID > 0 ? Math.Round(HDitem.hyPrice.Value * HDitem.countNum, 2) : Math.Round(HDitem.lsPrice.Value * HDitem.countNum, 2);

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
                                    Sum = Math.Round((item.ls_price.Value / item.amount) * item.amount * PeiSu, 2),
                                    goodsDes = item.memo,
                                    jjPrice = HDitem.jjPrice,
                                    pfPrice = Math.Round(item.yls_price, 2),
                                    isXG = true,
                                    vtype = 4,
                                    isCyjf = item.isjf == 0 ? true : false,
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
                                ZSitem.isCyjf = item.isjf == 0 ? true : false;
                                ZSitem.Sum = 0.00m;
                            }
                            else if (ZStempNum > 0)
                            {
                                ZSitem.countNum -= item.zs_amount * PeiSu;
                                ZSitem.Sum = VipID > 0 ? Math.Round(ZSitem.hyPrice.Value * ZSitem.countNum, 2) : Math.Round(ZSitem.lsPrice.Value * ZSitem.countNum, 2);

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
                                    Sum = 0.00m,
                                    goodsDes = item.memo,
                                    jjPrice = ZSitem.jjPrice,
                                    pfPrice = Math.Round(item.zs_ylsprice, 2),
                                    isXG = true,
                                    vtype = 4,
                                    isCyjf = item.isjf == 0 ? true : false,
                                });

                            }


                    }
                }

            }

        }



        ChoiceGoods cho5 = new ChoiceGoods();
        // 活动5 满额赠送
        private void YH5WEFunc(hjnbhEntities db)
        {
            #region (活动5)主要判断合计金额是否满额就行，不要判断买了什么

            var item5 = goodsBuyList.Where(e => e.vtype == 5 && e.isZS).FirstOrDefault();
            if (item5 != null) return; //如果有赠品就不再参与
            //bool istip = false; //防止重复提醒  现在似乎不需要了
            int scode_temp = HandoverModel.GetInstance.scode;
            int VipID = HandoverModel.GetInstance.VipID;
            int viplv = HandoverModel.GetInstance.VipLv;

            decimal sum_temp5 = goodsBuyList.Select(t => t.Sum).Sum();  //目前总额
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
                            var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).ToList();

                            //得判断有没有同名的活动10存在的情况
                            if (YH10ZS.Count > 0)
                            {
                                //所有符合条件的活动10数量
                                decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();

                                if (YH10count > 0)
                                {
                                    //关联的数量,要有数量才有得搞
                                    if (YH10count < item.zs_amount)
                                    {
                                        ZScount = YH10count;
                                    }
                                    else
                                    {
                                        ZScount = item.zs_amount;
                                    }

                                    //if (item.zs_amount > 0)
                                    //{
                                    //    if (DialogResult.No == MessageBox.Show("此单满足满额加价赠送活动，可加价领取赠品： [" + item.zs_cname + "]，价值：" + Math.Round(item.zsmoney.Value, 2) + " 元，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                    //    {
                                    //        continue;
                                    //    }
                                    //}
                                    //else
                                    //{
                                    //    if (DialogResult.No == MessageBox.Show("此单满足满额加价赠送活动，可免费领取：[" + item.zs_cname + "]，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                    //    {
                                    //        continue;
                                    //    }
                                    //}

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
                                        //goodsDes = item.memo,
                                        goodsDes = "加价：" + Math.Round(item.zsmoney.Value, 2) + " 元",
                                        jjPrice = Math.Round(item.zs_yjjprice, 2),
                                        pfPrice = Math.Round(item.zs_ylsprice, 2),  //记录原价
                                        Sum = Math.Round(((item.zsmoney / item.zs_amount).Value) * (ZScount), 2),
                                        isZS = true,
                                        isXG = true,
                                        vtype = 5,
                                        isCyjf = item.isjf == 0 ? true : false,
                                        isGL = true
                                    });

                                }



                            }
                            else
                            {

                                //if (item.zs_amount > 0)
                                //{
                                //    if (DialogResult.No == MessageBox.Show("此单满足满额加价赠送活动，可加价领取赠品： [" + item.zs_cname + "]，价值：" + Math.Round(item.zsmoney.Value, 2) + " 元，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                //    {
                                //        continue;
                                //    }
                                //}
                                //else
                                //{
                                //    if (DialogResult.No == MessageBox.Show("此单满足满额加价赠送活动，可免费领取：[" + item.zs_cname + "]，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                //    {
                                //        continue;
                                //    }
                                //}

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
                                    //goodsDes = item.memo,
                                    goodsDes = "加价：" + Math.Round(item.zsmoney.Value, 2) + " 元",
                                    jjPrice = Math.Round(item.zs_yjjprice, 2),
                                    pfPrice = Math.Round(item.zs_ylsprice, 2),  //记录原价
                                    Sum = Math.Round(((item.zsmoney / item.zs_amount).Value) * (item.zs_amount), 2),
                                    isZS = true,
                                    isXG = true,
                                    isCyjf = item.isjf == 0 ? true : false,
                                    vtype = 5
                                });

                            }
                        }

                    }    //所有对象
                    else if (item.dx_type == 0)
                    {
                        //赠送的数量
                        decimal ZScount = 1.00m;
                        //同名活动10
                        var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).ToList();

                        //得判断有没有同名的活动10存在的情况
                        if (YH10ZS.Count > 0)
                        {
                            //所有符合条件的活动10数量
                            decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();

                            if (YH10count > 0)
                            {
                                //关联的数量,要有数量才有得搞
                                if (YH10count < item.zs_amount)
                                {
                                    ZScount = YH10count;
                                }
                                else
                                {
                                    ZScount = item.zs_amount;
                                }

                                //if (item.zs_amount > 0)
                                //{
                                //    if (DialogResult.No == MessageBox.Show("此单满足满额加价赠送活动，可加价领取赠品： [" + item.zs_cname + "]，价值：" + Math.Round(item.zsmoney.Value, 2) + " 元，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                //    {
                                //        continue;
                                //    }
                                //}
                                //else
                                //{
                                //    if (DialogResult.No == MessageBox.Show("此单满足满额加价赠送活动，可免费领取：[" + item.zs_cname + "]，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                //    {
                                //        continue;
                                //    }
                                //}

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
                                    Sum = Math.Round(((item.zsmoney / item.zs_amount).Value) * ZScount, 2),
                                    //goodsDes = item.memo,
                                    goodsDes = "加价：" + Math.Round(item.zsmoney.Value, 2) + " 元",
                                    jjPrice = Math.Round(item.zs_yjjprice, 2),
                                    pfPrice = Math.Round(item.zs_ylsprice, 2),  //记录原价
                                    isZS = true,
                                    isXG = true,
                                    isCyjf = item.isjf == 0 ? true : false,
                                    vtype = 5,
                                    isGL = true
                                });

                            }

                        }
                        else
                        {
                            //没有关联的活动

                            //if (item.zs_amount > 0)
                            //{
                            //    if (DialogResult.No == MessageBox.Show("此单满足满额加价赠送活动，可加价领取赠品： [" + item.zs_cname + "]，价值：" + Math.Round(item.zsmoney.Value, 2) + " 元，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                            //    {
                            //        continue;
                            //    }
                            //}
                            //else
                            //{
                            //    if (DialogResult.No == MessageBox.Show("此单满足满额加价赠送活动，可免费领取：[" + item.zs_cname + "]，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                            //    {
                            //        continue;
                            //    }
                            //}

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
                                Sum = Math.Round(((item.zsmoney / item.zs_amount).Value) * item.zs_amount, 2),
                                //goodsDes = item.memo,
                                goodsDes = "加价：" + Math.Round(item.zsmoney.Value, 2) + " 元",
                                jjPrice = Math.Round(item.zs_yjjprice, 2),
                                pfPrice = Math.Round(item.zs_ylsprice, 2),  //记录原价
                                isZS = true,
                                isXG = true,
                                isCyjf = item.isjf == 0 ? true : false,
                                vtype = 5
                            });

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

        // 活动8，商品促销调价
        private void YH8XSFunc(hjnbhEntities db)
        {
            int scode_temp = HandoverModel.GetInstance.scode;
            //int VipID = HandoverModel.GetInstance.VipID;
            //int viplv = HandoverModel.GetInstance.VipLv;

            //活动商品列表
            var YHInfo8 = db.v_yh_detail.AsNoTracking().Where(t => t.scode == scode_temp && t.vtype == 8).ToList();
            if (YHInfo8.Count > 0)
            {

                foreach (var item in YHInfo8)
                {
                    //先过滤一下，看购物车中是否有符合活动的普通商品
                    var HasHdItem = goodsBuyList.Where(t => t.vtype == 0 && t.noCode == item.item_id).FirstOrDefault();
                    if (HasHdItem == null) continue;

                    HasHdItem.lsPrice = Math.Round(item.ls_price.Value, 2);
                    HasHdItem.hyPrice = Math.Round(item.ls_price.Value, 2);
                    HasHdItem.Sum = Math.Round(item.ls_price.Value * HasHdItem.countNum, 2);
                    HasHdItem.isCyjf = item.isjf == 0 ? true : false;
                }
            }
        }

        // 活动9的商品数量满赠送
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
                                    var vipls = db.hd_ls_detail.AsNoTracking().Where(t => t.v_code == itempp.v_code && t.item_id == item.item_id && t.vtype == 9 && t.iszs == 0 && t.ctime > item.sbegintime && t.ctime < item.sendtime).FirstOrDefault();
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
                                    var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).ToList();

                                    //得判断有没有同名的活动10存在的情况
                                    if (YH10ZS.Count > 0)
                                    {
                                        //所有符合条件的活动10数量
                                        decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                        //赠品数量
                                        decimal ZScount = 1.00m;
                                        //关联的数量,要有数量才有得搞
                                        if (YH10count > 0)
                                        {
                                            if (YH10count < item.zs_amount)
                                            {
                                                ZScount = YH10count;
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
                                                if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足商品满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                {
                                                    continue;

                                                }


                                                //修改活动商品价格为原价 活动价
                                                var hditem = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).FirstOrDefault();
                                                if (hditem != null)
                                                {
                                                    hditem.hyPrice = hditem.lsPrice.Value;
                                                    hditem.Sum = Math.Round(hditem.lsPrice.Value * hditem.countNum, 2);
                                                    hditem.vtype = 9;
                                                    hditem.isCyjf = item.isjf == 0 ? true : false;
                                                }

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
                                                        jjPrice = item.zs_yjjprice,
                                                        pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                        Sum = 0.00m,
                                                        isZS = true,
                                                        vtype = 9,
                                                        //isCyjf = item.isjf == 0 ? true : false,
                                                        isGL = true

                                                    });
                                                
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
                                            if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足商品满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                            {
                                                continue;
                                            }


                                            //修改活动商品价格为原价 活动价
                                            var hditem = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).FirstOrDefault();
                                            if (hditem != null)
                                            {
                                                hditem.hyPrice = hditem.lsPrice.Value;
                                                hditem.Sum = Math.Round(hditem.lsPrice.Value * hditem.countNum, 2);
                                                hditem.vtype = 9;
                                                hditem.isCyjf = item.isjf == 0 ? true : false;
                                            }


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
                                                    jjPrice = item.zs_yjjprice,
                                                    pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                    Sum = 0.00m,
                                                    isZS = true,
                                                    //isCyjf = item.isjf == 0 ? true : false,
                                                    vtype = 9,
                                                });
                                            
                                        }
                                    }

                                }
                                else
                                {
                                    if (DialogResult.Yes == MessageBox.Show("此单 [" + item.cname + "] 商品可以参与商品满购赠送活动，但未满足购买数量条件，是否确认参加此次活动？确定后此商品以原价出售并累计数量。", "活动提醒", MessageBoxButtons.YesNo))
                                    {


                                        //修改活动商品价格为原价 活动价
                                        var hditem = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).FirstOrDefault();
                                        if (hditem != null)
                                        {
                                            hditem.hyPrice = hditem.lsPrice.Value;
                                            hditem.Sum = Math.Round(hditem.lsPrice.Value * hditem.countNum, 2);
                                            hditem.vtype = 9;
                                            hditem.isCyjf = item.isjf == 0 ? true : false;
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
                                    var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).ToList();

                                    //得判断有没有同名的活动10存在的情况
                                    if (YH10ZS.Count > 0)
                                    {
                                        //所有符合条件的活动10数量
                                        decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                        //赠品数量
                                        decimal ZScount = 1.00m;
                                        //关联的数量,要有数量才有得搞
                                        if (YH10count > 0)
                                        {
                                            if (YH10count < item.zs_amount)
                                            {
                                                ZScount = YH10count;
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
                                                if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足商品满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                {
                                                    continue;

                                                }

                                                //修改活动商品价格为原价 活动价
                                                var hditem = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).FirstOrDefault();
                                                if (hditem != null)
                                                {
                                                    hditem.hyPrice = hditem.lsPrice.Value;
                                                    hditem.Sum = Math.Round(hditem.lsPrice.Value * hditem.countNum, 2);
                                                    hditem.vtype = 9;
                                                    hditem.isCyjf = item.isjf == 0 ? true : false;
                                                }

                                                    goodsBuyList.Add(new GoodsBuy
                                                    {
                                                        spec = zsinfo100.spec,
                                                        pinYin = zsinfo100.py,
                                                        unit = zsunit5,
                                                        unitStr = dw_,
                                                        barCodeTM = item.zstm,
                                                        noCode = item.zs_item_id,
                                                        countNum = ZScount,
                                                        jjPrice = item.zs_yjjprice,
                                                        pfPrice = Math.Round(item.yls_price, 2),
                                                        goods = item.zs_cname,
                                                        goodsDes = item.memo,
                                                        lsPrice = 0.00m,
                                                        hyPrice = 0.00m,
                                                        isZS = true,
                                                        vtype = 9,
                                                        //isCyjf = item.isjf == 0 ? true : false,
                                                        Sum = 0.00m,
                                                        isGL = true

                                                    });
                                                
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
                                            if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足商品满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                            {
                                                continue;

                                            }

                                            //修改活动商品价格为原价 活动价
                                            var hditem = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).FirstOrDefault();
                                            if (hditem != null)
                                            {
                                                hditem.hyPrice = hditem.lsPrice.Value;
                                                hditem.Sum = Math.Round(hditem.lsPrice.Value * hditem.countNum, 2);
                                                hditem.vtype = 9;
                                                hditem.isCyjf = item.isjf == 0 ? true : false;
                                            }

                                                goodsBuyList.Add(new GoodsBuy
                                                {
                                                    spec = zsinfo100.spec,
                                                    pinYin = zsinfo100.py,
                                                    unit = zsunit5,
                                                    unitStr = dw_,
                                                    barCodeTM = item.zstm,
                                                    noCode = item.zs_item_id,
                                                    countNum = item.zs_amount,
                                                    jjPrice = item.zs_yjjprice,
                                                    pfPrice = Math.Round(item.yls_price, 2),
                                                    goods = item.zs_cname,
                                                    goodsDes = item.memo,
                                                    lsPrice = 0.00m,
                                                    hyPrice = 0.00m,
                                                    isZS = true,
                                                    vtype = 9,
                                                    //isCyjf = item.isjf == 0 ? true : false,
                                                    Sum = 0.00m,

                                                });
                                            
                                        }


                                    }

                                }
                                else
                                {
                                    if (DialogResult.Yes == MessageBox.Show("此单 [" + item.cname + "] 商品可以参与商品满购赠送活动，但未满足购买数量条件，是否确认参加此次活动？确定后此商品以原价出售并累计数量。", "活动提醒", MessageBoxButtons.YesNo))
                                    {


                                        //修改活动商品价格为原价 活动价
                                        var hditem = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).FirstOrDefault();
                                        if (hditem != null)
                                        {
                                            hditem.hyPrice = hditem.lsPrice.Value;
                                            hditem.Sum = Math.Round(hditem.lsPrice.Value * hditem.countNum, 2);
                                            hditem.vtype = 9;
                                            hditem.isCyjf = item.isjf == 0 ? true : false;
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
                            var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).ToList();

                            //得判断有没有同名的活动10存在的情况
                            if (YH10ZS.Count > 0)
                            {
                                //所有符合条件的活动10数量
                                decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                if (YH10count <= 0) continue;  //有总量才有得搞

                                //赠品数量
                                decimal ZScount = 1.00m;
                                //关联的数量,要有数量才有得搞
                                if (YH10count < item.zs_amount)
                                {
                                    ZScount = YH10count;
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
                                        if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足商品满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            continue;

                                        }

                                        //修改活动商品价格为原价 活动价
                                        var hditem = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).FirstOrDefault();
                                        if (hditem != null)
                                        {
                                            hditem.hyPrice = hditem.lsPrice.Value;
                                            hditem.Sum = Math.Round(hditem.lsPrice.Value * hditem.countNum, 2);
                                            hditem.vtype = 9;
                                            hditem.isCyjf = item.isjf == 0 ? true : false;
                                        }

                                        goodsBuyList.Add(new GoodsBuy
                                        {
                                            spec = zsinfo100.spec,
                                            pinYin = zsinfo100.py,
                                            unit = zsunit5,
                                            unitStr = dw_,
                                            barCodeTM = item.zstm,
                                            noCode = item.zs_item_id,
                                            countNum = ZScount,
                                            jjPrice = item.zs_yjjprice,
                                            pfPrice = Math.Round(item.yls_price, 2),
                                            goods = item.zs_cname,
                                            goodsDes = item.memo,
                                            lsPrice = 0.00m,
                                            hyPrice = 0.00m,
                                            isZS = true,
                                            Sum = 0.00m,
                                            isGL = true,
                                            //isCyjf = item.isjf == 0 ? true : false,
                                            vtype = 9
                                        });
                                    }
                                }
                                else
                                {
                                    if (DialogResult.Yes == MessageBox.Show("此单 [" + item.cname + "] 商品可以参与商品满购赠送活动，但未满足购买数量条件，是否确认参加此次活动？确定后此商品以原价出售并累计数量。", "活动提醒", MessageBoxButtons.YesNo))
                                    {


                                        //修改活动商品价格为原价 活动价
                                        var hditem = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).FirstOrDefault();
                                        if (hditem != null)
                                        {
                                            hditem.hyPrice = hditem.lsPrice.Value;
                                            hditem.Sum = Math.Round(hditem.lsPrice.Value * hditem.countNum, 2);
                                            hditem.vtype = 9;
                                            hditem.isCyjf = item.isjf == 0 ? true : false;
                                        }

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
                                        if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足商品满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            continue;

                                        }
                                        //修改活动商品价格为原价 活动价
                                        var hditem = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).FirstOrDefault();
                                        if (hditem != null)
                                        {
                                            hditem.hyPrice = hditem.lsPrice.Value;
                                            hditem.Sum = Math.Round(hditem.lsPrice.Value * hditem.countNum, 2);
                                            hditem.vtype = 9;
                                            hditem.isCyjf = item.isjf == 0 ? true : false;
                                        }

                                        goodsBuyList.Add(new GoodsBuy
                                        {
                                            spec = zsinfo100.spec,
                                            pinYin = zsinfo100.py,
                                            unit = zsunit5,
                                            unitStr = dw_,
                                            barCodeTM = item.zstm,
                                            noCode = item.zs_item_id,
                                            countNum = item.zs_amount,
                                            jjPrice = item.zs_yjjprice,
                                            pfPrice = Math.Round(item.yls_price, 2),
                                            goods = item.zs_cname,
                                            goodsDes = item.memo,
                                            lsPrice = 0.00m,
                                            hyPrice = 0.00m,
                                            isZS = true,
                                            Sum = 0.00m,
                                            //isCyjf = item.isjf == 0 ? true : false,
                                            vtype = 9
                                        });
                                    }
                                }
                                else
                                {
                                    if (DialogResult.Yes == MessageBox.Show("此单 [" + item.cname + "] 商品可以参与商品满购赠送活动，但未满足购买数量条件，是否确认参加此次活动？确定后此商品以原价出售并累计数量。", "活动提醒", MessageBoxButtons.YesNo))
                                    {


                                        //修改活动商品价格为原价 活动价
                                        var hditem = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).FirstOrDefault();
                                        if (hditem != null)
                                        {
                                            hditem.hyPrice = hditem.lsPrice.Value;
                                            hditem.Sum = Math.Round(hditem.lsPrice.Value * hditem.countNum, 2);
                                            hditem.vtype = 9;
                                            hditem.isCyjf = item.isjf == 0 ? true : false;
                                        }

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
                                    var vipls = db.hd_ls_detail.AsNoTracking().Where(t => t.v_code == itempp.v_code && t.item_id == item.item_id && t.vtype == 9 &&t.iszs == 0 && t.ctime > item.sbegintime && t.ctime < item.sendtime).FirstOrDefault();
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
                                    var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).ToList();

                                    //得判断有没有同名的活动10存在的情况
                                    if (YH10ZS.Count > 0)
                                    {
                                        //所有符合条件的活动10数量
                                        decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                        //赠品数量
                                        decimal ZScount = 1.00m;
                                        //关联的数量,要有数量才有得搞
                                        if (YH10count > 0)
                                        {
                                            if (YH10count < item.zs_amount)
                                            {
                                                ZScount = YH10count;
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
                                                if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足商品满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                {
                                                    continue;

                                                }
                                                //修改活动商品价格为原价 活动价
                                                var hditem = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).FirstOrDefault();
                                                if (hditem != null)
                                                {
                                                    hditem.hyPrice = hditem.lsPrice.Value;
                                                    hditem.Sum = Math.Round(hditem.lsPrice.Value * hditem.countNum, 2);
                                                    hditem.vtype = 9;
                                                    hditem.isCyjf = item.isjf == 0 ? true : false;
                                                }

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
                                                        jjPrice = item.zs_yjjprice,
                                                        pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                        Sum = 0.00m,
                                                        isZS = true,
                                                        vtype = 9,
                                                        //isCyjf = item.isjf == 0 ? true : false,
                                                        isGL = true

                                                    });
                                                
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
                                            if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足商品满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                            {
                                                continue;

                                            }

                                            //修改活动商品价格为原价 活动价
                                            var hditem = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).FirstOrDefault();
                                            if (hditem != null)
                                            {
                                                hditem.hyPrice = hditem.lsPrice.Value;
                                                hditem.Sum = Math.Round(hditem.lsPrice.Value * hditem.countNum, 2);
                                                hditem.vtype = 9;
                                                hditem.isCyjf = item.isjf == 0 ? true : false;
                                            }

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
                                                    jjPrice = item.zs_yjjprice,
                                                    pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                    Sum = 0.00m,
                                                    isZS = true,
                                                    vtype = 9,
                                                    //isCyjf = item.isjf == 0 ? true : false,
                                                });
                                            
                                        }
                                    }

                                }
                                else
                                {
                                    if (DialogResult.Yes == MessageBox.Show("此单 [" + item.cname + "] 商品可以参与商品满购赠送活动，但未满足购买数量条件，是否确认参加此次活动？确定后此商品以原价出售并累计数量。", "活动提醒", MessageBoxButtons.YesNo))
                                    {


                                        //修改活动商品价格为原价 活动价
                                        var hditem = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).FirstOrDefault();
                                        if (hditem != null)
                                        {
                                            hditem.hyPrice = hditem.lsPrice.Value;
                                            hditem.Sum = Math.Round(hditem.lsPrice.Value * hditem.countNum, 2);
                                            hditem.vtype = 9;
                                            hditem.isCyjf = item.isjf == 0 ? true : false;
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
                                    var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).ToList();

                                    //得判断有没有同名的活动10存在的情况
                                    if (YH10ZS.Count > 0)
                                    {
                                        //所有符合条件的活动10数量
                                        decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                        //赠品数量
                                        decimal ZScount = 1.00m;
                                        //关联的数量,要有数量才有得搞
                                        if (YH10count > 0)
                                        {
                                            if (YH10count < item.zs_amount)
                                            {
                                                ZScount = YH10count;
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
                                                if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足商品满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                {
                                                    continue;

                                                }


                                                //修改活动商品价格为原价 活动价
                                                var hditem = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).FirstOrDefault();
                                                if (hditem != null)
                                                {
                                                    hditem.hyPrice = hditem.lsPrice.Value;
                                                    hditem.Sum = Math.Round(hditem.lsPrice.Value * hditem.countNum, 2);
                                                    hditem.vtype = 9;
                                                    hditem.isCyjf = item.isjf == 0 ? true : false;
                                                }

                                                    goodsBuyList.Add(new GoodsBuy
                                                    {
                                                        spec = zsinfo100.spec,
                                                        pinYin = zsinfo100.py,
                                                        unit = zsunit5,
                                                        unitStr = dw_,
                                                        barCodeTM = item.zstm,
                                                        noCode = item.zs_item_id,
                                                        countNum = ZScount,
                                                        jjPrice = item.zs_yjjprice,
                                                        pfPrice = Math.Round(item.yls_price, 2),
                                                        goods = item.zs_cname,
                                                        goodsDes = item.memo,
                                                        lsPrice = 0.00m,
                                                        hyPrice = 0.00m,
                                                        isZS = true,
                                                        vtype = 9,
                                                        //isCyjf = item.isjf == 0 ? true : false,
                                                        Sum = 0.00m,
                                                        isGL = true

                                                    });
                                                
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
                                            if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足商品满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                            {
                                                continue;

                                            }


                                            //修改活动商品价格为原价 活动价
                                            var hditem = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).FirstOrDefault();
                                            if (hditem != null)
                                            {
                                                hditem.hyPrice = hditem.lsPrice.Value;
                                                hditem.Sum = Math.Round(hditem.lsPrice.Value * hditem.countNum, 2);
                                                hditem.vtype = 9;
                                                hditem.isCyjf = item.isjf == 0 ? true : false;
                                            }

                                                goodsBuyList.Add(new GoodsBuy
                                                {
                                                    spec = zsinfo100.spec,
                                                    pinYin = zsinfo100.py,
                                                    unit = zsunit5,
                                                    unitStr = dw_,
                                                    barCodeTM = item.zstm,
                                                    noCode = item.zs_item_id,
                                                    countNum = item.zs_amount,
                                                    jjPrice = item.zs_yjjprice,
                                                    pfPrice = Math.Round(item.yls_price, 2),
                                                    goods = item.zs_cname,
                                                    goodsDes = item.memo,
                                                    lsPrice = 0.00m,
                                                    hyPrice = 0.00m,
                                                    isZS = true,
                                                    vtype = 9,
                                                    Sum = 0.00m,
                                                    //isCyjf = item.isjf == 0 ? true : false,
                                                });
                                            
                                        }


                                    }

                                }
                                else
                                {
                                    if (DialogResult.Yes == MessageBox.Show("此单 [" + item.cname + "] 商品可以参与商品满购赠送活动，但未满足购买数量条件，是否确认参加此次活动？确定后此商品以原价出售并累计数量。", "活动提醒", MessageBoxButtons.YesNo))
                                    {


                                        //修改活动商品价格为原价 活动价
                                        var hditem = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).FirstOrDefault();
                                        if (hditem != null)
                                        {
                                            hditem.hyPrice = hditem.lsPrice.Value;
                                            hditem.Sum = Math.Round(hditem.lsPrice.Value * hditem.countNum, 2);
                                            hditem.vtype = 9;
                                            hditem.isCyjf = item.isjf == 0 ? true : false;
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
                            var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).ToList();

                            //得判断有没有同名的活动10存在的情况
                            if (YH10ZS.Count > 0)
                            {
                                //所有符合条件的活动10数量
                                decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                if (YH10count <= 0) continue;  //有总量才有得搞

                                //赠品数量
                                decimal ZScount = 1.00m;
                                //关联的数量,要有数量才有得搞
                                if (YH10count < item.zs_amount)
                                {
                                    ZScount = YH10count;
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
                                        if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足商品满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            continue;

                                        }


                                        //修改活动商品价格为原价 活动价
                                        var hditem = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).FirstOrDefault();
                                        if (hditem != null)
                                        {
                                            hditem.hyPrice = hditem.lsPrice.Value;
                                            hditem.Sum = Math.Round(hditem.lsPrice.Value * hditem.countNum, 2);
                                            hditem.vtype = 9;
                                            hditem.isCyjf = item.isjf == 0 ? true : false;
                                        }


                                        goodsBuyList.Add(new GoodsBuy
                                        {
                                            spec = zsinfo100.spec,
                                            pinYin = zsinfo100.py,
                                            unit = zsunit5,
                                            unitStr = dw_,
                                            barCodeTM = item.zstm,
                                            noCode = item.zs_item_id,
                                            countNum = ZScount,
                                            jjPrice = item.zs_yjjprice,
                                            pfPrice = Math.Round(item.yls_price, 2),
                                            goods = item.zs_cname,
                                            goodsDes = item.memo,
                                            lsPrice = 0.00m,
                                            hyPrice = 0.00m,
                                            Sum = 0.00m,
                                            isZS = true,
                                            isGL = true,
                                            //isCyjf = item.isjf == 0 ? true : false,
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
                                        if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足商品满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            continue;

                                        }

                                        //修改活动商品价格为原价 活动价
                                        var hditem = goodsBuyList.Where(e => e.noCode == item.item_id && e.vtype == 0).FirstOrDefault();
                                        if (hditem != null)
                                        {
                                            hditem.hyPrice = hditem.lsPrice.Value;
                                            hditem.Sum = Math.Round(hditem.lsPrice.Value * hditem.countNum, 2);
                                            hditem.vtype = 9;
                                            hditem.isCyjf = item.isjf == 0 ? true : false;
                                        }

                                        goodsBuyList.Add(new GoodsBuy
                                        {
                                            spec = zsinfo100.spec,
                                            pinYin = zsinfo100.py,
                                            unit = zsunit5,
                                            unitStr = dw_,
                                            barCodeTM = item.zstm,
                                            noCode = item.zs_item_id,
                                            countNum = item.zs_amount,
                                            jjPrice = item.zs_yjjprice,
                                            pfPrice = Math.Round(item.yls_price, 2),
                                            goods = item.zs_cname,
                                            goodsDes = item.memo,
                                            lsPrice = 0.00m,
                                            hyPrice = 0.00m,
                                            Sum = 0.00m,
                                            isZS = true,
                                            //isCyjf = item.isjf == 0 ? true : false,
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


        // 活动9的类别赠送
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
                    decimal buysum = 0.00m; //符合的数量
                    //购物车中的商品类别符合的购买数量(不与其它活动重叠)
                    foreach (var itembuyed in goodsBuyList)
                    {
                        if (itembuyed.LB == item.item_id && itembuyed.vtype == 0)
                        {
                            buysum += itembuyed.countNum;
                        }
                        else
                        {
                            //如果不相等，再判断有没有可能是他的父类上级类别
                            var itemlbinfo = db.hd_item_lb.AsNoTracking().Where(t => t.lb_code == itembuyed.LB).FirstOrDefault();
                            if (itemlbinfo != null)
                            {
                                if (itemlbinfo.ilevel == 2)
                                {
                                    if (itemlbinfo.parent_id == item.item_id && itembuyed.vtype == 0)
                                    {
                                        buysum += itembuyed.countNum;
                                    }
                                }
                                else if (itemlbinfo.ilevel == 3)
                                {
                                    if (itemlbinfo.parent_id == item.item_id && itembuyed.vtype == 0)
                                    {
                                        buysum += itembuyed.countNum;
                                        //统计出该类别已经购买数量
                                    }
                                    else
                                    {
                                        var lbinfotemp = db.hd_item_lb.AsNoTracking().Where(t => t.lb_code == itemlbinfo.parent_id).FirstOrDefault();
                                        if (lbinfotemp != null)
                                        {
                                            if (lbinfotemp.parent_id == item.item_id && itembuyed.vtype == 0)
                                            {
                                                buysum += itembuyed.countNum;
                                                //统计出该类别已经购买数量
                                            }
                                        }

                                    }
                                }
                            }
                        }
                    }

                    if (buysum == 0) continue;

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

                                //消费的商品里是否符合条件的类别
                                decimal lbzsnum = 0; //符合的数量
                                foreach (var itempp in viplsList)
                                {
                                    //找到这件活动商品在活动期间的购买记录
                                    var vipls = db.hd_ls_detail.AsNoTracking().Where(t => t.v_code == itempp.v_code&&t.vtype==9&&t.iszs==0  && t.ctime > item.sbegintime && t.ctime < item.sendtime).FirstOrDefault();
                                    if (vipls != null)
                                    {
                                        //在销售视图中找出其类别
                                        var pplb = db.v_xs_item_info.AsNoTracking().Where(t => t.item_id == vipls.item_id ).Select(t => t.lb_code).FirstOrDefault();
                                        if (pplb == item.item_id)
                                        {
                                            lbzsnum += vipls.amount.Value;  //统计出该类别已经购买数量
                                        }
                                        else
                                        {
                                            //如果不相等，再判断有没有可能是他的父类上级类别
                                            var itemlbinfo = db.hd_item_lb.AsNoTracking().Where(t => t.lb_code == pplb).FirstOrDefault();
                                            if (itemlbinfo != null)
                                            {
                                                if (itemlbinfo.ilevel == 2)
                                                {
                                                    if (itemlbinfo.parent_id == item.item_id)
                                                    {
                                                        lbzsnum += vipls.amount.Value;  //统计出该类别已经购买数量
                                                    }
                                                }
                                                else if (itemlbinfo.ilevel == 3)
                                                {
                                                    if (itemlbinfo.parent_id == item.item_id)
                                                    {
                                                        lbzsnum += vipls.amount.Value;  //统计出该类别已经购买数量
                                                    }
                                                    else
                                                    {
                                                        var lbinfotemp = db.hd_item_lb.AsNoTracking().Where(t => t.lb_code == itemlbinfo.parent_id).FirstOrDefault();
                                                        if (lbinfotemp != null)
                                                        {
                                                            if (lbinfotemp.parent_id == item.item_id)
                                                            {
                                                                lbzsnum += vipls.amount.Value;  //统计出该类别已经购买数量
                                                            }
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                //只需要再购买的数量 
                                decimal numtemp = item.amount - lbzsnum;

                                if (buysum >= numtemp)
                                {

                                    //符合条件，可以赠送
                                    //判断是否有活动10关联
                                    //同名活动10
                                    var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).ToList();

                                    //得判断有没有同名的活动10存在的情况
                                    if (YH10ZS.Count > 0)
                                    {
                                        //所有符合条件的活动10数量
                                        decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                        //赠品数量
                                        decimal ZScount = 1.00m;
                                        //关联的数量,要有数量才有得搞
                                        if (YH10count > 0)
                                        {
                                            if (YH10count < item.zs_amount)
                                            {
                                                ZScount = YH10count;
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
                                                if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                {
                                                    continue;
                                                }

                                                //修改活动商品价格为原价 活动价
                                                foreach (var buyitem in goodsBuyList)
                                                {
                                                    if (isBLFunc(db, buyitem.LB, item.item_id))
                                                    {
                                                        buyitem.hyPrice = buyitem.lsPrice.Value;
                                                        buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                        buyitem.vtype = 9;
                                                        buyitem.isCyjf = item.isjf == 0 ? true : false;
                                                    }
                                                }

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
                                                        jjPrice = item.zs_yjjprice,
                                                        pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                        Sum = 0.00m,
                                                        isZS = true,
                                                        vtype = 9,
                                                        //isCyjf = item.isjf == 0 ? true : false,
                                                        isGL = true

                                                    });

                                                

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
                                            if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                            {
                                                continue;
                                            }

                                            //修改活动商品价格为原价 活动价
                                            foreach (var buyitem in goodsBuyList)
                                            {
                                                if (isBLFunc(db, buyitem.LB, item.item_id))
                                                {
                                                    buyitem.hyPrice = buyitem.lsPrice.Value;
                                                    buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                    buyitem.vtype = 9;
                                                    buyitem.isCyjf = item.isjf == 0 ? true : false;
                                                }
                                            }

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
                                                    jjPrice = item.zs_yjjprice,
                                                    pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                    Sum = 0.00m,
                                                    isZS = true,
                                                    //isCyjf = item.isjf == 0 ? true : false,
                                                    vtype = 9,
                                                });

                                            

                                        }

                                    }

                                }
                                else
                                {
                                    if (DialogResult.Yes == MessageBox.Show("此单属于 [" + item.cname + "] 类别的商品可以参与类别满购赠送活动，但未满足购买数量条件，是否确认参加此次活动？确定后此商品以原价出售并累计数量。", "活动提醒", MessageBoxButtons.YesNo))
                                    {

                                        //修改活动商品价格为原价 活动价
                                        foreach (var buyitem in goodsBuyList)
                                        {
                                            if (isBLFunc(db, buyitem.LB, item.item_id))
                                            {
                                                buyitem.hyPrice = buyitem.lsPrice.Value;
                                                buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                buyitem.vtype = 9;
                                                buyitem.isCyjf = item.isjf == 0 ? true : false;
                                            }
                                        }

                                    }

                                }
                            }
                            else
                            {
                                //如果会员在指定时间内没有消费，但是现在一次性购满的情况

                                //购物车中的商品类别符合的购买数量(不与其它活动重叠)
                                //var lbnum = goodsBuyList.Where(e => e.LB == item.item_id && e.vtype == 0).Select(e => e.countNum).Sum();
                                if (buysum >= item.amount)
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
                                        if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            continue;

                                        }

                                        //修改活动商品价格为原价 活动价
                                        foreach (var buyitem in goodsBuyList)
                                        {
                                            if (isBLFunc(db, buyitem.LB, item.item_id))
                                            {
                                                buyitem.hyPrice = buyitem.lsPrice.Value;
                                                buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                buyitem.vtype = 9;
                                                buyitem.isCyjf = item.isjf == 0 ? true : false;
                                            }
                                        }

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
                                                jjPrice = item.zs_yjjprice,
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                Sum = 0.00m,
                                                isZS = true,
                                                vtype = 9,
                                                //isCyjf = item.isjf == 0 ? true : false,
                                            });

                                        

                                    }
                                }
                                else
                                {
                                    if (DialogResult.Yes == MessageBox.Show("此单属于 [" + item.cname + "] 类别的商品可以参与类别满购赠送活动，但未满足购买数量条件，是否确认参加此次活动？确定后此商品以原价出售并累计数量。", "活动提醒", MessageBoxButtons.YesNo))
                                    {

                                        //修改活动商品价格为原价 活动价
                                        foreach (var buyitem in goodsBuyList)
                                        {
                                            if (isBLFunc(db, buyitem.LB, item.item_id))
                                            {
                                                buyitem.hyPrice = buyitem.lsPrice.Value;
                                                buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                buyitem.vtype = 9;
                                                buyitem.isCyjf = item.isjf == 0 ? true : false;
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
                            var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).ToList();

                            //得判断有没有同名的活动10存在的情况
                            if (YH10ZS.Count > 0)
                            {
                                //所有符合条件的活动10数量
                                decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                if (YH10count <= 0) continue;  //有总量才有得搞

                                //赠品数量
                                decimal ZScount = 1.00m;
                                //关联的数量,要有数量才有得搞
                                if (YH10count < item.zs_amount)
                                {
                                    ZScount = YH10count;
                                }
                                else
                                {
                                    ZScount = item.zs_amount;
                                }

                                //购物车中的商品类别符合的购买数量
                                //var lbnum = goodsBuyList.Where(e => e.LB == item.item_id).Select(e => e.countNum).Sum();
                                if (buysum >= item.amount)
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
                                        if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            continue;
                                        }

                                        //修改活动商品价格为原价 活动价
                                        foreach (var buyitem in goodsBuyList)
                                        {
                                            if (isBLFunc(db, buyitem.LB, item.item_id))
                                            {
                                                buyitem.hyPrice = buyitem.lsPrice.Value;
                                                buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                buyitem.vtype = 9;
                                                buyitem.isCyjf = item.isjf == 0 ? true : false;
                                            }
                                        }

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
                                                jjPrice = item.zs_yjjprice,
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                isZS = true,
                                                isGL = true,
                                                Sum = 0.00m,
                                                vtype = 9,
                                                //isCyjf = item.isjf == 0 ? true : false,
                                            });

                                        

                                    }
                                }
                                else
                                {
                                    if (DialogResult.Yes == MessageBox.Show("此单属于 [" + item.cname + "] 类别的商品可以参与类别满购赠送活动，但未满足购买数量条件，是否确认参加此次活动？确定后此商品以原价出售并累计数量。", "活动提醒", MessageBoxButtons.YesNo))
                                    {

                                        //修改活动商品价格为原价 活动价
                                        foreach (var buyitem in goodsBuyList)
                                        {
                                            if (isBLFunc(db, buyitem.LB, item.item_id))
                                            {
                                                buyitem.hyPrice = buyitem.lsPrice.Value;
                                                buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                buyitem.vtype = 9;
                                                buyitem.isCyjf = item.isjf == 0 ? true : false;
                                            }
                                        }

                                    }
                                }
                            }
                            else
                            {
                                //没有关联

                                //购物车中的商品类别符合的购买数量
                                //var lbnum = goodsBuyList.Where(e => e.LB == item.item_id).Select(e => e.countNum).Sum();
                                if (buysum >= item.amount)
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
                                        if (DialogResult.Yes == MessageBox.Show("此单 [" + item.cname + "] 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            //修改活动商品价格为原价 活动价
                                            foreach (var buyitem in goodsBuyList)
                                            {
                                                if (isBLFunc(db, buyitem.LB, item.item_id))
                                                {
                                                    buyitem.hyPrice = buyitem.lsPrice.Value;
                                                    buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                    buyitem.vtype = 9;
                                                    buyitem.isCyjf = item.isjf == 0 ? true : false;
                                                }
                                            }

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
                                                jjPrice = item.zs_yjjprice,
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                isZS = true,
                                                Sum = 0.00m,
                                                vtype = 9,
                                                //isCyjf = item.isjf == 0 ? true : false,
                                            });

                                        }

                                    }


                                }
                                else
                                {
                                    if (DialogResult.Yes == MessageBox.Show("此单属于 [" + item.cname + "] 类别的商品可以参与类别满购赠送活动，但未满足购买数量条件，是否确认参加此次活动？确定后此商品以原价出售并累计数量。", "活动提醒", MessageBoxButtons.YesNo))
                                    {

                                        //修改活动商品价格为原价 活动价
                                        foreach (var buyitem in goodsBuyList)
                                        {
                                            if (isBLFunc(db, buyitem.LB, item.item_id))
                                            {
                                                buyitem.hyPrice = buyitem.lsPrice.Value;
                                                buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                buyitem.vtype = 9;
                                                buyitem.isCyjf = item.isjf == 0 ? true : false;
                                            }
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
                                decimal lbzsnum = 0; //符合的数量
                                foreach (var itempp in viplsList)
                                {
                                    //找到这件活动商品的购买记录
                                    var vipls = db.hd_ls_detail.AsNoTracking().Where(t => t.v_code == itempp.v_code && t.vtype ==9 && t.iszs == 0  && t.ctime > item.sbegintime && t.ctime < item.sendtime).FirstOrDefault();
                                    if (vipls != null)
                                    {
                                        //在销售视图中找出其类别
                                        var pplb = db.v_xs_item_info.AsNoTracking().Where(t => t.item_id == vipls.item_id).Select(t => t.lb_code).FirstOrDefault();
                                        if (pplb == item.item_id)
                                        {
                                            lbzsnum += vipls.amount.Value;  //统计出该类别已经购买数量
                                        }
                                        else
                                        {
                                            //如果不相等，再判断有没有可能是他的父类上级类别
                                            var itemlbinfo = db.hd_item_lb.AsNoTracking().Where(t => t.lb_code == pplb).FirstOrDefault();
                                            if (itemlbinfo != null)
                                            {
                                                if (itemlbinfo.ilevel == 2)
                                                {
                                                    if (itemlbinfo.parent_id == item.item_id)
                                                    {
                                                        lbzsnum += vipls.amount.Value;  //统计出该类别已经购买数量
                                                    }
                                                }
                                                else if (itemlbinfo.ilevel == 3)
                                                {
                                                    if (itemlbinfo.parent_id == item.item_id)
                                                    {
                                                        lbzsnum += vipls.amount.Value;  //统计出该类别已经购买数量
                                                    }
                                                    else
                                                    {
                                                        var lbinfotemp = db.hd_item_lb.AsNoTracking().Where(t => t.lb_code == itemlbinfo.parent_id).FirstOrDefault();
                                                        if (lbinfotemp != null)
                                                        {
                                                            if (lbinfotemp.parent_id == item.item_id)
                                                            {
                                                                lbzsnum += vipls.amount.Value;  //统计出该类别已经购买数量
                                                            }
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                //只需要再购买的数量 
                                decimal numtemp = item.amount - lbzsnum;

                                //购物车中的商品类别符合的购买数量(不与其它活动重叠)
                                //var lbnum = goodsBuyList.Where(e => e.LB == item.item_id && e.vtype == 0).Select(e => e.countNum).Sum();
                                if (buysum >= numtemp)
                                {

                                    //符合条件，可以赠送
                                    //判断是否有活动10关联
                                    //同名活动10
                                    var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).ToList();

                                    //得判断有没有同名的活动10存在的情况
                                    if (YH10ZS.Count > 0)
                                    {
                                        //所有符合条件的活动10数量
                                        decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                        //赠品数量
                                        decimal ZScount = 1.00m;
                                        //关联的数量,要有数量才有得搞
                                        if (YH10count > 0)
                                        {
                                            if (YH10count < item.zs_amount)
                                            {
                                                ZScount = YH10count;
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
                                                if (DialogResult.Yes == MessageBox.Show("此单 [" + item.cname + "] 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                {
                                                    continue;
                                                }
                                                    //修改活动商品价格为原价 活动价
                                                foreach (var buyitem in goodsBuyList)
                                                {
                                                    if (isBLFunc(db, buyitem.LB, item.item_id))
                                                    {
                                                        buyitem.hyPrice = buyitem.lsPrice.Value;
                                                        buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                        buyitem.vtype = 9;
                                                        buyitem.isCyjf = item.isjf == 0 ? true : false;
                                                    }
                                                }

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
                                                        jjPrice = item.zs_yjjprice,
                                                        pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                        Sum = 0.00m,
                                                        isZS = true,
                                                        vtype = 9,
                                                        //isCyjf = item.isjf == 0 ? true : false,
                                                        isGL = true

                                                    });

                                                //}

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
                                            if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                            {
                                                continue;

                                            }
                                                //修改活动商品价格为原价 活动价
                                            foreach (var buyitem in goodsBuyList)
                                            {
                                                if (isBLFunc(db, buyitem.LB, item.item_id))
                                                {
                                                    buyitem.hyPrice = buyitem.lsPrice.Value;
                                                    buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                    buyitem.vtype = 9;
                                                    buyitem.isCyjf = item.isjf == 0 ? true : false;
                                                }
                                            }

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
                                                    jjPrice = item.zs_yjjprice,
                                                    pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                    Sum = 0.00m,
                                                    isZS = true,
                                                    vtype = 9,
                                                    //isCyjf = item.isjf == 0 ? true : false,
                                                });

                                            //}

                                        }

                                    }

                                }
                                else
                                {
                                    if (DialogResult.Yes == MessageBox.Show("此单属于 [" + item.cname + "] 类别的商品可以参与类别满购赠送活动，但未满足购买数量条件，是否确认参加此次活动？确定后此商品以原价出售并累计数量。", "活动提醒", MessageBoxButtons.YesNo))
                                    {

                                        //修改活动商品价格为原价 活动价
                                        foreach (var buyitem in goodsBuyList)
                                        {
                                            if (isBLFunc(db, buyitem.LB, item.item_id))
                                            {
                                                buyitem.hyPrice = buyitem.lsPrice.Value;
                                                buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                buyitem.vtype = 9;
                                                buyitem.isCyjf = item.isjf == 0 ? true : false;
                                            }
                                        }

                                    }
                                }
                            }
                            else
                            {
                                //如果会员在指定时间内没有消费，但是现在一次性购满的情况

                                //购物车中的商品类别符合的购买数量(不与其它活动重叠)
                                //var lbnum = goodsBuyList.Where(e => e.LB == item.item_id && e.vtype == 0).Select(e => e.countNum).Sum();
                                if (buysum >= item.amount)
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
                                        if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            continue;
                                        }
                                            ////修改活动商品价格为原价 活动价
                                        foreach (var buyitem in goodsBuyList)
                                        {
                                            if (isBLFunc(db, buyitem.LB, item.item_id))
                                            {
                                                buyitem.hyPrice = buyitem.lsPrice.Value;
                                                buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                buyitem.vtype = 9;
                                                buyitem.isCyjf = item.isjf == 0 ? true : false;
                                            }
                                        }

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
                                                jjPrice = item.zs_yjjprice,
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                Sum = 0.00m,
                                                isZS = true,
                                                vtype = 9,
                                                //isCyjf = item.isjf == 0 ? true : false,
                                            });

                                        //}

                                    }
                                }
                                else
                                {
                                    if (DialogResult.Yes == MessageBox.Show("此单属于 [" + item.cname + "] 类别的商品可以参与类别满购赠送活动，但未满足购买数量条件，是否确认参加此次活动？确定后此商品以原价出售并累计数量。", "活动提醒", MessageBoxButtons.YesNo))
                                    {

                                        //修改活动商品价格为原价 活动价
                                        foreach (var buyitem in goodsBuyList)
                                        {
                                            if (isBLFunc(db, buyitem.LB, item.item_id))
                                            {
                                                buyitem.hyPrice = buyitem.lsPrice.Value;
                                                buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                buyitem.vtype = 9;
                                                buyitem.isCyjf = item.isjf == 0 ? true : false;
                                            }
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
                            var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).ToList();

                            //得判断有没有同名的活动10存在的情况
                            if (YH10ZS.Count > 0)
                            {
                                //所有符合条件的活动10数量
                                decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                if (YH10count <= 0) continue;  //有总量才有得搞

                                //赠品数量
                                decimal ZScount = 1.00m;
                                //关联的数量,要有数量才有得搞
                                if (YH10count < item.zs_amount)
                                {
                                    ZScount = YH10count;
                                }
                                else
                                {
                                    ZScount = item.zs_amount;
                                }

                                //购物车中的商品类别符合的购买数量
                                //var lbnum = goodsBuyList.Where(e => e.LB == item.item_id).Select(e => e.countNum).Sum();
                                if (buysum >= item.amount)
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
                                        if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            continue;
                                        }
                                            ////修改活动商品价格为原价 活动价
                                        foreach (var buyitem in goodsBuyList)
                                        {
                                            if (isBLFunc(db, buyitem.LB, item.item_id))
                                            {
                                                buyitem.hyPrice = buyitem.lsPrice.Value;
                                                buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                buyitem.vtype = 9;
                                                buyitem.isCyjf = item.isjf == 0 ? true : false;
                                            }
                                        }

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
                                                Sum = 0.00m,
                                                jjPrice = item.zs_yjjprice,
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                isZS = true,
                                                isGL = true,
                                                vtype = 9,
                                                //isCyjf = item.isjf == 0 ? true : false,
                                            });

                                        //}

                                    }
                                }

                            }
                            else
                            {
                                //没有关联

                                //购物车中的商品类别符合的购买数量
                                //var lbnum = goodsBuyList.Where(e => e.LB == item.item_id).Select(e => e.countNum).Sum();
                                if (buysum >= item.amount)
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
                                        if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足类别满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            continue;
                                        }
                                            ////修改活动商品价格为原价 活动价
                                        foreach (var buyitem in goodsBuyList)
                                        {
                                            if (isBLFunc(db, buyitem.LB, item.item_id))
                                            {
                                                buyitem.hyPrice = buyitem.lsPrice.Value;
                                                buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                buyitem.vtype = 9;
                                                buyitem.isCyjf = item.isjf == 0 ? true : false;
                                            }
                                        }

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
                                                Sum = 0.00m,
                                                jjPrice = item.zs_yjjprice,
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                isZS = true,
                                                vtype = 9,
                                                //isCyjf = item.isjf == 0 ? true : false,
                                            });

                                        //}

                                    }


                                }

                            }


                        }


                    }

                }

            }







            #endregion

        }

        // 活动9的品牌赠送
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
                                    var vipls = db.hd_ls_detail.AsNoTracking().Where(t => t.v_code == itempp.v_code && t.vtype == 9&&t.iszs==0 && t.ctime > item.sbegintime && t.ctime < item.sendtime).FirstOrDefault();
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
                                    var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).ToList();

                                    //得判断有没有同名的活动10存在的情况
                                    if (YH10ZS.Count > 0)
                                    {
                                        //所有符合条件的活动10数量
                                        decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                        //赠品数量
                                        decimal ZScount = 1.00m;
                                        //关联的数量,要有数量才有得搞
                                        if (YH10count > 0)
                                        {
                                            if (YH10count < item.zs_amount)
                                            {
                                                ZScount = YH10count;
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
                                                if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足品牌满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                {
                                                    continue;
                                                 }
                                                ////修改活动商品价格为原价 活动价
                                                foreach (var buyitem in goodsBuyList)
                                                {
                                                    if (isPPFunc(db, buyitem.noCode, item.tm ))
                                                    {
                                                        buyitem.hyPrice = buyitem.lsPrice.Value;
                                                        buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                        buyitem.vtype = 9;
                                                        buyitem.isCyjf = item.isjf == 0 ? true : false;
                                                    }
                                                }

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
                                                        jjPrice = item.zs_yjjprice,
                                                        pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                        Sum = 0.00m,
                                                        isZS = true,
                                                        vtype = 9,
                                                        //isCyjf = item.isjf == 0 ? true : false,
                                                        isGL = true

                                                    });

                                                //}

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
                                            if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足品牌满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                            {
                                                continue;
                                            }
                                            ////修改活动商品价格为原价 活动价
                                            foreach (var buyitem in goodsBuyList)
                                            {
                                                if (isPPFunc(db, buyitem.noCode, item.tm))
                                                {
                                                    buyitem.hyPrice = buyitem.lsPrice.Value;
                                                    buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                    buyitem.vtype = 9;
                                                    buyitem.isCyjf = item.isjf == 0 ? true : false;
                                                }
                                            }

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
                                                    jjPrice = item.zs_yjjprice,
                                                    pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                    Sum = 0.00m,
                                                    isZS = true,
                                                    vtype = 9,
                                                    //isCyjf = item.isjf == 0 ? true : false,
                                                });

                                            //}

                                        }

                                    }

                                }
                                else
                                {
                                    if (DialogResult.Yes == MessageBox.Show("此单属于 [" + item.cname + "] 品牌的商品可以参与品牌满购赠送活动，但未满足购买数量条件，是否确认参加此次活动？确定后此商品以原价出售并累计数量。", "活动提醒", MessageBoxButtons.YesNo))
                                    {

                                        //修改活动商品价格为原价 活动价
                                        foreach (var buyitem in goodsBuyList)
                                        {
                                            if (isPPFunc(db, buyitem.noCode, item.tm))
                                            {
                                                buyitem.hyPrice = buyitem.lsPrice.Value;
                                                buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                buyitem.vtype = 9;
                                                buyitem.isCyjf = item.isjf == 0 ? true : false;
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
                                        if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足品牌满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            continue;
                                        }
                                        ////修改活动商品价格为原价 活动价
                                        foreach (var buyitem in goodsBuyList)
                                        {
                                            if (isPPFunc(db, buyitem.noCode, item.tm))
                                            {
                                                buyitem.hyPrice = buyitem.lsPrice.Value;
                                                buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                buyitem.vtype = 9;
                                                buyitem.isCyjf = item.isjf == 0 ? true : false;
                                            }
                                        }

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
                                                jjPrice = item.zs_yjjprice,
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                Sum = 0.00m,
                                                isZS = true,
                                                vtype = 9,
                                                //isCyjf = item.isjf == 0 ? true : false,
                                            });

                                        //}

                                    }

                                }
                                else
                                {
                                    if (DialogResult.Yes == MessageBox.Show("此单属于 [" + item.cname + "] 品牌的商品可以参与品牌满购赠送活动，但未满足购买数量条件，是否确认参加此次活动？确定后此商品以原价出售并累计数量。", "活动提醒", MessageBoxButtons.YesNo))
                                    {

                                        //修改活动商品价格为原价 活动价
                                        foreach (var buyitem in goodsBuyList)
                                        {
                                            if (isPPFunc(db, buyitem.noCode, item.tm))
                                            {
                                                buyitem.hyPrice = buyitem.lsPrice.Value;
                                                buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                buyitem.vtype = 9;
                                                buyitem.isCyjf = item.isjf == 0 ? true : false;
                                            }
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
                            var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).ToList();

                            //得判断有没有同名的活动10存在的情况
                            if (YH10ZS.Count > 0)
                            {
                                //所有符合条件的活动10数量
                                decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                if (YH10count <= 0) continue;  //有总量才有得搞

                                //赠品数量
                                decimal ZScount = 1.00m;
                                if (YH10count < item.zs_amount)
                                {
                                    ZScount = YH10count;
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
                                        if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足品牌满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            continue;
                                        }
                                        ////修改活动商品价格为原价 活动价
                                        foreach (var buyitem in goodsBuyList)
                                        {
                                            if (isPPFunc(db, buyitem.noCode, item.tm))
                                            {
                                                buyitem.hyPrice = buyitem.lsPrice.Value;
                                                buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                buyitem.vtype = 9;
                                                buyitem.isCyjf = item.isjf == 0 ? true : false;
                                            }
                                        }

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
                                                jjPrice = item.zs_yjjprice,
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                Sum = 0.00m,
                                                isGL = true,
                                                isZS = true,
                                                vtype = 9,
                                                //isCyjf = item.isjf == 0 ? true : false,
                                            });

                                        //}

                                    }
                                }
                                else
                                {
                                    if (DialogResult.Yes == MessageBox.Show("此单属于 [" + item.cname + "] 品牌的商品可以参与品牌满购赠送活动，但未满足购买数量条件，是否确认参加此次活动？确定后此商品以原价出售并累计数量。", "活动提醒", MessageBoxButtons.YesNo))
                                    {

                                        //修改活动商品价格为原价 活动价
                                        foreach (var buyitem in goodsBuyList)
                                        {
                                            if (isPPFunc(db, buyitem.noCode, item.tm))
                                            {
                                                buyitem.hyPrice = buyitem.lsPrice.Value;
                                                buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                buyitem.vtype = 9;
                                                buyitem.isCyjf = item.isjf == 0 ? true : false;
                                            }
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
                                        if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足品牌满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            continue;
                                        }
                                        ////修改活动商品价格为原价 活动价
                                        foreach (var buyitem in goodsBuyList)
                                        {
                                            if (isPPFunc(db, buyitem.noCode, item.tm))
                                            {
                                                buyitem.hyPrice = buyitem.lsPrice.Value;
                                                buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                buyitem.vtype = 9;
                                                buyitem.isCyjf = item.isjf == 0 ? true : false;
                                            }
                                        }

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
                                                jjPrice = item.zs_yjjprice,
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                isZS = true,
                                                Sum = 0.00m,
                                                vtype = 9,
                                                //isCyjf = item.isjf == 0 ? true : false,
                                            });

                                        //}

                                    }
                                }
                                else
                                {
                                    if (DialogResult.Yes == MessageBox.Show("此单属于 [" + item.cname + "] 品牌的商品可以参与品牌满购赠送活动，但未满足购买数量条件，是否确认参加此次活动？确定后此商品以原价出售并累计数量。", "活动提醒", MessageBoxButtons.YesNo))
                                    {

                                        //修改活动商品价格为原价 活动价
                                        foreach (var buyitem in goodsBuyList)
                                        {
                                            if (isPPFunc(db, buyitem.noCode, item.tm))
                                            {
                                                buyitem.hyPrice = buyitem.lsPrice.Value;
                                                buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                buyitem.vtype = 9;
                                                buyitem.isCyjf = item.isjf == 0 ? true : false;
                                            }
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
                                    var vipls = db.hd_ls_detail.AsNoTracking().Where(t => t.v_code == itempp.v_code && t.vtype == 9 &&t.iszs==0&& t.ctime > item.sbegintime && t.ctime < item.sendtime).FirstOrDefault();
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
                                    var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).ToList();

                                    //得判断有没有同名的活动10存在的情况
                                    if (YH10ZS.Count > 0)
                                    {
                                        //所有符合条件的活动10数量
                                        decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                        //赠品数量
                                        decimal ZScount = 1.00m;
                                        //关联的数量,要有数量才有得搞
                                        if (YH10count > 0)
                                        {
                                            if (YH10count < item.zs_amount)
                                            {
                                                ZScount = YH10count;
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
                                                if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足品牌满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                {
                                                    continue;
                                                }
                                                ////修改活动商品价格为原价 活动价
                                                foreach (var buyitem in goodsBuyList)
                                                {
                                                    if (isPPFunc(db, buyitem.noCode, item.tm))
                                                    {
                                                        buyitem.hyPrice = buyitem.lsPrice.Value;
                                                        buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                        buyitem.vtype = 9;
                                                        buyitem.isCyjf = item.isjf == 0 ? true : false;
                                                    }
                                                }

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
                                                        jjPrice = item.zs_yjjprice,
                                                        pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                        Sum = 0.00m,
                                                        isZS = true,
                                                        vtype = 9,
                                                        //isCyjf = item.isjf == 0 ? true : false,
                                                        isGL = true

                                                    });

                                                //}

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
                                            if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足品牌满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                            {
                                                continue;
                                            }
                                            ////修改活动商品价格为原价 活动价
                                            foreach (var buyitem in goodsBuyList)
                                            {
                                                if (isPPFunc(db, buyitem.noCode, item.tm))
                                                {
                                                    buyitem.hyPrice = buyitem.lsPrice.Value;
                                                    buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                    buyitem.vtype = 9;
                                                    buyitem.isCyjf = item.isjf == 0 ? true : false;
                                                }
                                            }

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
                                                    jjPrice = item.zs_yjjprice,
                                                    pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                    Sum = 0.00m,
                                                    isZS = true,
                                                    vtype = 9,
                                                    //isCyjf = item.isjf == 0 ? true : false,
                                                });

                                            //}

                                        }

                                    }

                                }
                                else
                                {
                                    if (DialogResult.Yes == MessageBox.Show("此单属于 [" + item.cname + "] 品牌的商品可以参与品牌满购赠送活动，但未满足购买数量条件，是否确认参加此次活动？确定后此商品以原价出售并累计数量。", "活动提醒", MessageBoxButtons.YesNo))
                                    {

                                        //修改活动商品价格为原价 活动价
                                        foreach (var buyitem in goodsBuyList)
                                        {
                                            if (isPPFunc(db, buyitem.noCode, item.tm))
                                            {
                                                buyitem.hyPrice = buyitem.lsPrice.Value;
                                                buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                buyitem.vtype = 9;
                                                buyitem.isCyjf = item.isjf == 0 ? true : false;
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
                                        if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足品牌满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            continue;
                                        }
                                        ////修改活动商品价格为原价 活动价
                                        foreach (var buyitem in goodsBuyList)
                                        {
                                            if (isPPFunc(db, buyitem.noCode, item.tm))
                                            {
                                                buyitem.hyPrice = buyitem.lsPrice.Value;
                                                buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                buyitem.vtype = 9;
                                                buyitem.isCyjf = item.isjf == 0 ? true : false;
                                            }
                                        }

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
                                                jjPrice = item.zs_yjjprice,
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                Sum = 0.00m,
                                                isZS = true,
                                                vtype = 9,
                                                //isCyjf = item.isjf == 0 ? true : false,
                                            });

                                        //}

                                    }

                                }
                                else
                                {
                                    if (DialogResult.Yes == MessageBox.Show("此单属于 [" + item.cname + "] 品牌的商品可以参与品牌满购赠送活动，但未满足购买数量条件，是否确认参加此次活动？确定后此商品以原价出售并累计数量。", "活动提醒", MessageBoxButtons.YesNo))
                                    {

                                        //修改活动商品价格为原价 活动价
                                        foreach (var buyitem in goodsBuyList)
                                        {
                                            if (isPPFunc(db, buyitem.noCode, item.tm))
                                            {
                                                buyitem.hyPrice = buyitem.lsPrice.Value;
                                                buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                buyitem.vtype = 9;
                                                buyitem.isCyjf = item.isjf == 0 ? true : false;
                                            }
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
                            var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == item.zs_item_id).ToList();

                            //得判断有没有同名的活动10存在的情况
                            if (YH10ZS.Count > 0)
                            {
                                //所有符合条件的活动10数量
                                decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                if (YH10count <= 0) continue;  //有总量才有得搞

                                //赠品数量
                                decimal ZScount = 1.00m;
                                if (YH10count < item.zs_amount)
                                {
                                    ZScount = YH10count;
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
                                        if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足品牌满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            continue;
                                        }
                                        ////修改活动商品价格为原价 活动价
                                        foreach (var buyitem in goodsBuyList)
                                        {
                                            if (isPPFunc(db, buyitem.noCode, item.tm))
                                            {
                                                buyitem.hyPrice = buyitem.lsPrice.Value;
                                                buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                buyitem.vtype = 9;
                                                buyitem.isCyjf = item.isjf == 0 ? true : false;
                                            }
                                        }

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
                                                Sum = 0.00m,
                                                jjPrice = item.zs_yjjprice,
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                isGL = true,
                                                isZS = true,
                                                vtype = 9,
                                                //isCyjf = item.isjf == 0 ? true : false,
                                            });

                                        //}

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
                                        if (DialogResult.No == MessageBox.Show("此单 [" + item.cname + "] 满足品牌满购赠送活动，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            continue;
                                        }
                                        ////修改活动商品价格为原价 活动价
                                        foreach (var buyitem in goodsBuyList)
                                        {
                                            if (isPPFunc(db, buyitem.noCode, item.tm))
                                            {
                                                buyitem.hyPrice = buyitem.lsPrice.Value;
                                                buyitem.Sum = Math.Round(buyitem.lsPrice.Value * buyitem.countNum, 2);
                                                buyitem.vtype = 9;
                                                buyitem.isCyjf = item.isjf == 0 ? true : false;
                                            }
                                        }

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
                                                Sum = 0.00m,
                                                jjPrice = item.zs_yjjprice,
                                                pfPrice = Math.Round(item.zs_ylsprice, 2),
                                                isZS = true,
                                                vtype = 9,
                                                //isCyjf = item.isjf == 0 ? true : false,
                                            });

                                        //}

                                    }
                                }

                            }

                        }



                    }

                }

            }



            #endregion

        }

        // 活动10
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
                                //判断购物车目前该赠品(活动商品)总数量，不能超过现存的数量
                                var zsitems = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype != 0).ToList();
                                if (zsitems.Count > 0)
                                {
                                    decimal zsitemcount = zsitems.Select(t => t.countNum).Sum();
                                    if (zsitemcount >= item.amount) continue;
                                }

                                //符合活动的普通商品
                                var goodsptList = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 0).ToList();
                                if (goodsptList.Count > 0)
                                {
                                    //在赠送之前先判断还有没有赠品
                                    if (item.amount < 1) continue;

                                    if (item.ls_price.Value > 0)
                                    {
                                        if (DialogResult.No == MessageBox.Show("此单商品：["+item.cname+"]满足限量促销活动，将更正零售价为活动价：" + Math.Round(item.ls_price.Value, 2) + " 元，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                        {
                                            continue;
                                        }
                                    }
                                    //else
                                    //{
                                    //    if (DialogResult.No == MessageBox.Show("此单商品：["+item.cname+"]满足限量促销活动，可免费领取此赠品，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                    //    {
                                    //        continue;
                                    //    }
                                    //}

                                    ////还要判断在此活动前是否还有赠品赠送出去
                                    var num = goodsptList.Select(t => t.countNum).Sum();
                                    if (item.amount >= num)
                                    {

                                        for (int i = 0; i < goodsptList.Count; i++)
                                        {

                                            goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);
                                            goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);
                                            goodsptList[i].goodsDes = string.IsNullOrEmpty(item.memo) ? "限量" : item.memo;
                                            goodsptList[i].jjPrice = Math.Round(item.yjj_price, 2);
                                            goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);  //记录原价
                                            goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                            goodsptList[i].isZS = true;
                                            goodsptList[i].isXG = true;
                                            goodsptList[i].vtype = 10;
                                            goodsptList[i].isGL = true;
                                            goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                        }
                                    }
                                    else
                                    {

                                        for (int i = 0; i < goodsptList.Count; i++)
                                        {
                                            //那么分拆
                                            goodsptList[i].countNum -= item.amount;
                                            goodsptList[i].Sum = Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2);

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
                                                countNum = item.amount,
                                                lsPrice = Math.Round(item.ls_price.Value, 2),
                                                hyPrice = Math.Round(item.ls_price.Value, 2),
                                                goodsDes = string.IsNullOrEmpty(item.memo) ? "限量" : item.memo,
                                                jjPrice = Math.Round(item.yjj_price, 2),
                                                pfPrice = Math.Round(item.yls_price, 2),  //记录原价
                                                Sum = Math.Round(item.ls_price.Value * item.amount, 2),
                                                isZS = true,
                                                isXG = true,
                                                vtype = 10,
                                                isCyjf = item.isjf == 0 ? true : false,
                                                isGL = true

                                            });

                                        }
                                    }

                                }

                            }
                        }
                    }
                    else
                    {
                        //不要求会员的情况
                        //判断购物车目前该赠品(活动商品)总数量，不能超过现存的数量
                        var zsitems = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype != 0).ToList();
                        if (zsitems.Count > 0)
                        {
                            decimal zsitemcount = zsitems.Select(t => t.countNum).Sum();
                            if (zsitemcount >= item.amount) continue;
                        }

                        //符合活动的普通商品
                        var goodsptList = goodsBuyList.Where(t => t.noCode == item.item_id && t.vtype == 0).ToList();
                        if (goodsptList.Count > 0)
                        {
                            //在赠送之前先判断还有没有赠品
                            if (item.amount < 1) continue;

                            if (item.ls_price > 0)
                            {
                                if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足限量赠送活动，将更正零售价为活动价：" + Math.Round(item.ls_price.Value, 2) + " 元，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                {
                                    continue;
                                }
                            }
                            //else
                            //{
                            //    if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足限量赠送活动，可免费领取此赠品，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                            //    {
                            //        continue;
                            //    }
                            //}

                            ////还要判断在此活动前是否还有赠品赠送出去
                            var num = goodsptList.Select(t => t.countNum).Sum();
                            if (item.amount >= num)
                            {


                                for (int i = 0; i < goodsptList.Count; i++)
                                {

                                    goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);
                                    goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);
                                    goodsptList[i].Sum = Math.Round((item.ls_price.Value) * goodsptList[i].countNum, 2);
                                    goodsptList[i].goodsDes = string.IsNullOrEmpty(item.memo) ? "限量" : item.memo;
                                    goodsptList[i].jjPrice = Math.Round(item.yjj_price, 2);
                                    goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);  //记录原价
                                    goodsptList[i].isZS = true;
                                    goodsptList[i].isXG = true;
                                    goodsptList[i].vtype = 10;
                                    goodsptList[i].isGL = true;
                                    goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                }
                            }
                            else
                            {

                                for (int i = 0; i < goodsptList.Count; i++)
                                {
                                    //那么分拆
                                    goodsptList[i].countNum -= item.amount;
                                    goodsptList[i].Sum = Math.Round(goodsptList[i].countNum * goodsptList[i].lsPrice.Value, 2);

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
                                        countNum = item.amount,
                                        lsPrice = Math.Round(item.ls_price.Value, 2),
                                        hyPrice = Math.Round(item.ls_price.Value, 2),
                                        Sum = Math.Round(item.ls_price.Value * item.amount, 2),
                                        goodsDes = string.IsNullOrEmpty(item.memo) ? "限量" : item.memo,
                                        jjPrice = Math.Round(item.yjj_price, 2),
                                        pfPrice = Math.Round(item.yls_price, 2),  //记录原价
                                        isZS = true,
                                        isXG = true,
                                        vtype = 10,
                                        isCyjf = item.isjf == 0 ? true : false,
                                        isGL = true

                                    });

                                }
                            }

                        }

                    }

                }

            }




            #endregion
        }


        // 活动1 限赠
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
                                        var YH10ZS = db.v_yh_detail.AsNoTracking().Where(t => t.vtype == 10 && t.scode == scode_temp && t.item_id == itemid).ToList();

                                        //得判断有没有同名的活动10存在的情况
                                        if (YH10ZS.Count > 0)
                                        {
                                            //所有符合条件的活动10数量
                                            decimal YH10count = YH10ZS.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                            //关联的数量,要有数量才有得搞
                                            if (YH10count > 0)
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
                                                    decimal counttemp = YH10count > numtemp ? numtemp : YH10count;

                                                    //验证是否加价了
                                                    if (item.ls_price.Value > 0)
                                                    {
                                                        if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足限量赠送活动，将更正零售价为活动价：" + Math.Round(item.ls_price.Value, 2) + " 元，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                        {
                                                            continue;
                                                        }
                                                    }
                                                    //else
                                                    //{
                                                    //    if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足限量赠送活动，可免费领取此赠品，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                    //    {
                                                    //        continue;
                                                    //    }
                                                    //}

                                                    if (goodsptList[i].countNum > counttemp)
                                                    {

                                                        if (goodsptList[i].countNum - counttemp > 0)
                                                        {
                                                            //那么分拆
                                                            goodsptList[i].countNum -= counttemp;
                                                            goodsptList[i].Sum = Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2);

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
                                                                Sum = Math.Round(item.ls_price.Value * numtemp, 2),
                                                                goodsDes = item.memo,
                                                                jjPrice = Math.Round(item.yjj_price, 2),
                                                                pfPrice = Math.Round(item.yls_price, 2),  //记录原价
                                                                isZS = true,
                                                                isXG = true,
                                                                vtype = 1,
                                                                isCyjf = item.isjf == 0 ? true : false,
                                                                isGL = true

                                                            });
                                                        }
                                                        else
                                                        {
                                                            goodsptList[i].goodsDes = item.memo;
                                                            goodsptList[i].isGL = true;
                                                            goodsptList[i].isZS = true;
                                                            goodsptList[i].vtype = 1;
                                                            goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);
                                                            goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);
                                                            goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                                            goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                            goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                                        }



                                                    }
                                                    else
                                                    {

                                                        goodsptList[i].goodsDes = item.memo;
                                                        goodsptList[i].isGL = true;
                                                        goodsptList[i].isZS = true;
                                                        goodsptList[i].vtype = 1;
                                                        goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);
                                                        goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);
                                                        goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                                        goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                        goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                                    }


                                                }
                                                else
                                                {
                                                    decimal tempnum = item.xg_amount > YH10count ? YH10count : item.xg_amount;

                                                    //验证是否加价了
                                                    if (item.ls_price.Value > 0)
                                                    {
                                                        if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足限量赠送活动，将更正零售价为活动价：" + Math.Round(item.ls_price.Value, 2) + " 元，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                        {
                                                            continue;
                                                        }
                                                    }
                                                    //else
                                                    //{
                                                    //    if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足限量赠送活动，可免费领取此赠品，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                    //    {
                                                    //        continue;
                                                    //    }
                                                    //}

                                                    //没有领取记录
                                                    //商品不能超过限赠数量
                                                    if (goodsptList[i].countNum > tempnum)
                                                    {

                                                        //那么分拆
                                                        if (goodsptList[i].countNum - tempnum > 0)
                                                        {
                                                            goodsptList[i].countNum -= tempnum;
                                                            goodsptList[i].Sum = Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2);

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
                                                                Sum = Math.Round(item.ls_price.Value * tempnum, 2),
                                                                goodsDes = item.memo,
                                                                jjPrice = Math.Round(item.yjj_price, 2),
                                                                pfPrice = Math.Round(item.yls_price, 2),  //记录原价
                                                                isZS = true,
                                                                isXG = true,
                                                                vtype = 1,
                                                                isCyjf = item.isjf == 0 ? true : false,
                                                                isGL = true
                                                            });
                                                        }
                                                        else
                                                        {
                                                            goodsptList[i].goodsDes = item.memo;
                                                            goodsptList[i].isGL = true;
                                                            goodsptList[i].isZS = true;
                                                            goodsptList[i].vtype = 1;
                                                            goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);
                                                            goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);
                                                            goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                                            goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                            goodsptList[i].isCyjf = item.isjf == 0 ? true : false;
                                                        }



                                                    }
                                                    else
                                                    {

                                                        goodsptList[i].goodsDes = item.memo;
                                                        goodsptList[i].isGL = true;
                                                        goodsptList[i].isZS = true;
                                                        goodsptList[i].vtype = 1;
                                                        goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);
                                                        goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);
                                                        goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                                        goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                        goodsptList[i].isCyjf = item.isjf == 0 ? true : false;

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

                                                //验证是否加价了
                                                if (item.ls_price.Value > 0)
                                                {
                                                    if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足限量赠送活动，将更正零售价为活动价：" + Math.Round(item.ls_price.Value, 2) + " 元，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                    {
                                                        continue;
                                                    }
                                                }
                                                //else
                                                //{
                                                //    if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足限量赠送活动，可免费领取此赠品，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                //    {
                                                //        continue;
                                                //    }
                                                //}

                                                if (goodsptList[i].countNum > numtemp)
                                                {

                                                    //那么分拆
                                                    if (goodsptList[i].countNum - numtemp > 0)
                                                    {
                                                        goodsptList[i].countNum -= numtemp;
                                                        goodsptList[i].Sum = Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2);

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
                                                            Sum = Math.Round(item.ls_price.Value * numtemp, 2),
                                                            goodsDes = item.memo,
                                                            jjPrice = Math.Round(item.yjj_price, 2),
                                                            pfPrice = Math.Round(item.yls_price, 2),  //记录原价
                                                            isZS = true,
                                                            isXG = true,
                                                            vtype = 1,
                                                            isCyjf = item.isjf == 0 ? true : false,
                                                        });
                                                    }
                                                    else
                                                    {
                                                        goodsptList[i].goodsDes = item.memo;
                                                        goodsptList[i].isZS = true;
                                                        goodsptList[i].vtype = 1;
                                                        goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);
                                                        goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);
                                                        goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                                        goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                        goodsptList[i].isCyjf = item.isjf == 0 ? true : false;

                                                    }

                                                }
                                                else
                                                {

                                                    goodsptList[i].goodsDes = item.memo;
                                                    goodsptList[i].isZS = true;
                                                    goodsptList[i].vtype = 1;
                                                    goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);
                                                    goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);
                                                    goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                                    goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                    goodsptList[i].isCyjf = item.isjf == 0 ? true : false;

                                                }


                                            }
                                            else
                                            {
                                                //验证是否加价了
                                                if (item.ls_price.Value > 0)
                                                {
                                                    if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足限量赠送活动，将更正零售价为活动价：" + Math.Round(item.ls_price.Value, 2) + " 元，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                    {
                                                        continue;
                                                    }
                                                }
                                                //else
                                                //{
                                                //    if (DialogResult.No == MessageBox.Show("此单商品：[" + item.cname + "]满足限量赠送活动，可免费领取此赠品，是否确认参加此次活动？", "活动提醒", MessageBoxButtons.YesNo))
                                                //    {
                                                //        continue;
                                                //    }
                                                //}

                                                //没有领取记录
                                                //商品不能超过限赠数量
                                                if (goodsptList[i].countNum > item.xg_amount)
                                                {

                                                    //那么分拆
                                                    if (goodsptList[i].countNum - item.xg_amount > 0)
                                                    {
                                                        goodsptList[i].countNum -= item.xg_amount;
                                                        goodsptList[i].Sum = Math.Round(goodsptList[i].countNum * goodsptList[i].hyPrice.Value, 2);

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
                                                            Sum = Math.Round(item.ls_price.Value * item.xg_amount, 2),
                                                            goodsDes = item.memo,
                                                            jjPrice = Math.Round(item.yjj_price, 2),
                                                            pfPrice = Math.Round(item.yls_price, 2),  //记录原价
                                                            isZS = true,
                                                            isXG = true,
                                                            vtype = 1,
                                                            isCyjf = item.isjf == 0 ? true : false,
                                                        });
                                                    }
                                                    else
                                                    {
                                                        goodsptList[i].goodsDes = item.memo;
                                                        goodsptList[i].isZS = true;
                                                        goodsptList[i].vtype = 1;
                                                        goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);
                                                        goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);
                                                        goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                                        goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                        goodsptList[i].isCyjf = item.isjf == 0 ? true : false;

                                                    }

                                                }
                                                else
                                                {
                                                    goodsptList[i].goodsDes = item.memo;
                                                    goodsptList[i].isZS = true;
                                                    goodsptList[i].vtype = 1;
                                                    goodsptList[i].hyPrice = Math.Round(item.ls_price.Value, 2);
                                                    goodsptList[i].lsPrice = Math.Round(item.ls_price.Value, 2);
                                                    goodsptList[i].Sum = Math.Round(item.ls_price.Value * goodsptList[i].countNum, 2);
                                                    goodsptList[i].pfPrice = Math.Round(item.yls_price, 2);
                                                    goodsptList[i].isCyjf = item.isjf == 0 ? true : false;

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


        //活动5赠品选择
        void cho5_changed(GoodsBuy goods)
        {
            var item3 = goodsBuyList.Where(e => e.vtype == 5 && e.isZS).FirstOrDefault();  //任选其一
            if (item3 != null)
            {

                MessageBox.Show("赠品数量已经超额，不再累加");
            }
            else
            {
                goodsBuyList.Add(goods);

            }

            textBox1.Clear();

        }


        //判断商品是否属于某类别下
        bool isBLFunc(hjnbhEntities db, int itemlb, int lbid)
        {

            if (itemlb == lbid)
                {
                    return true;
                }
                else
                {
                    //如果不相等，再判断有没有可能是他的父类上级类别
                    var itemlbinfo = db.hd_item_lb.AsNoTracking().Where(t => t.lb_code == itemlb).FirstOrDefault();
                    if (itemlbinfo != null)
                    {
                        if (itemlbinfo.ilevel == 2)
                        {
                            if (itemlbinfo.parent_id == lbid )
                            {
                                return true;
                            }
                        }
                        else if (itemlbinfo.ilevel == 3)
                        {
                            if (itemlbinfo.parent_id == lbid )
                            {
                                return true;
                            }
                            else
                            {
                                var lbinfotemp = db.hd_item_lb.AsNoTracking().Where(t => t.lb_code == itemlbinfo.parent_id).FirstOrDefault();
                                if (lbinfotemp != null)
                                {
                                    if (lbinfotemp.parent_id == lbid )
                                    {
                                        return true;
                                    }
                                }

                            }
                        }
                    }
                }
            
            return false;
        }

        //判断商品是否属于某品牌
        bool isPPFunc(hjnbhEntities db, int itemid, string ppid)
        {
            var ppxs = db.v_xs_item_info.AsNoTracking().Where(t => t.item_id == itemid).Select(t => t.pp).FirstOrDefault();
            if (ppxs == ppid)
            {
                return true;
            }

            return false;
        }
        #endregion



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
                        goodsBuyList[i].Sum = Math.Round(goodsBuyList[i].lsPrice.Value * goodsBuyList[i].countNum, 2);
                        temp_r += (goodsBuyList[i].lsPrice.Value * goodsBuyList[i].countNum);
                    }
                    else
                    {
                        goodsBuyList[i].Sum = Math.Round(goodsBuyList[i].hyPrice.Value * goodsBuyList[i].countNum, 2);
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

            if (label101.Text == "按F12登记会员" || label101.Text == "")
            {
                Tipslabel.Text = "如果是会员消费，请按F12键录入，享受丰富的会员活动。";
            }
            else if (label103.Text == "未登记")
            {
                Tipslabel.Text = "如果需要记录提成，请按F3键录入营业员。";
            }



            //ColumnWidthFunc();
            label6.Text = HandoverModel.GetInstance.scodeName;  //分店名字
            label26.Text = HandoverModel.GetInstance.bcode.ToString();  //机号
        }

        //表格结束编辑事件
        private void dataGridView_Cashiers_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            //dataGridView_Cashiers.Refresh();

            ShowDown();
            textBox1.Enabled = true;
            textBox1.Focus();  //焦点回到条码输入框
            textBox1.Clear();  //清空方便下次读码
        }

        //表格数据验证(目前是数量与总额修改后的验证)
        private void dataGridView_Cashiers_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            try
            {
                int vipidtemp = HandoverModel.GetInstance.VipID;
                //验证第5列，数量
                if (e.ColumnIndex == 5)
                {
                    decimal temp_int = 0.00m;
                    if (decimal.TryParse(e.FormattedValue.ToString(), out temp_int))
                    {
                        e.Cancel = false;
                        //dataGridView_Cashiers.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = temp_int.ToString("0.00"); //修改数量
                        //dataGridView_Cashiers.InvalidateCell(e.ColumnIndex, e.RowIndex);  //刷新该单元格

                        goodsBuyList[e.RowIndex].countNum = temp_int;
                        goodsBuyList[e.RowIndex].Sum = vipidtemp == 0 ? Math.Round(goodsBuyList[e.RowIndex].lsPrice.Value * temp_int, 2) : Math.Round(goodsBuyList[e.RowIndex].hyPrice.Value * temp_int, 2);
                        dataGridView_Cashiers.InvalidateRow(e.RowIndex);

                    }
                    else
                    {
                        e.Cancel = true;
                        dataGridView_Cashiers.CancelEdit();
                        tipForm.Tiplabel.Text = "商品数量只能输入数字!";
                        tipForm.ShowDialog();
                    }
                }

                //验证第11列，金额
                if (e.ColumnIndex == 11)
                {
                    decimal temp = 0.00m;
                    if (decimal.TryParse(e.FormattedValue.ToString(), out temp))
                    {
                        e.Cancel = false;

                        goodsBuyList[e.RowIndex].Sum = temp;
                        dataGridView_Cashiers.InvalidateRow(e.RowIndex);

                    }
                    else
                    {
                        e.Cancel = true;
                        dataGridView_Cashiers.CancelEdit();
                        tipForm.Tiplabel.Text = "商品金额只能输入数字!";
                        tipForm.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("收银主界面修改数量或者金额时出现异常:", ex);
                MessageBox.Show("提示：商品属性修改验证错误！");
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
                        temp_r += item.Sum;
                        temp_c += item.countNum;
                    }

                    label81.Text = temp_r.ToString() + "  元";  //合计金额
                    label82.Text = temp_c.ToString();  //合计数量
                    totalMoney = temp_r;  //获取总金额
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

        //表格行选择改变事件
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



        // 处理整单打折
        void zkzdform_changed(decimal d)
        {
            int vipidtemp = HandoverModel.GetInstance.VipID;
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
                        goodsBuyList[i].Sum = vipidtemp == 0 ? Math.Round(goodsBuyList[i].lsPrice.Value * goodsBuyList[i].countNum, 2) : Math.Round(goodsBuyList[i].hyPrice.Value * goodsBuyList[i].countNum, 2);
                        goodsBuyList[i].goodsDes += "(" + (d / 10).ToString() + "折" + ")";
                        goodsBuyList[i].ZKDP = d;  //单品折扣
                        dataGridView_Cashiers.InvalidateRow(i);
                    }
                }
            }
            ShowDown();  //刷新合计金额UI
            label32.Text = d.ToString() + "%";  //显示折扣额
        }


        // 处理单品折扣
        void zkform_changed(decimal d)
        {
            int vipidtemp = HandoverModel.GetInstance.VipID;

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
                    goodsBuyList[index].Sum = vipidtemp == 0 ? Math.Round(goodsBuyList[index].lsPrice.Value * goodsBuyList[index].countNum, 2) : Math.Round(goodsBuyList[index].hyPrice.Value * goodsBuyList[index].countNum, 2);

                    goodsBuyList[index].goodsDes += "(" + (d / 10).ToString() + "折" + ")";
                    goodsBuyList[index].ZKDP = d;  //单品折扣
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


        int timer_temp = 0;  // 临时变量，解决结算后无法显示结算信息的BUG，因为按回车关闭结算窗口后会同时触发该窗口的回车搜索商品事件……
        //重写热键方法，这个优先级最高
        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
        {
            int WM_KEYDOWN = 256;
            int WM_SYSKEYDOWN = 260;
            if (msg.Msg == WM_KEYDOWN | msg.Msg == WM_SYSKEYDOWN)
            {
                switch (keyData)
                {

                    //清空购物车
                    case Keys.Insert:

                        InsertFun();

                        break;


                    //回车
                    case Keys.Enter:
                        if (!dataGridView_Cashiers.IsCurrentCellInEditMode)
                        {
                            EnterFun();
                            if (!textBox1.Focused)
                            {
                                textBox1.Focus();
                            }

                            textBox1.Clear();  //清空方便下次读码
                            ShowDown(); //刷新UI
                        }

                        break;

                    //小键盘+号
                    case Keys.Add:

                        UpdataCount();
                        break;

                        //-号键修改金额
                    case Keys.Subtract:
                        UpdataSum();
                        break;
                        //*号键赠送
                    case Keys.Multiply:
                        ForZsFunc();
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

                //会员消息ctrl+L
                //if (keyData == (Keys.Control | Keys.L))
                //{
                //    vipmemo.ShowDialog();

                //}



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
                    if (DialogResult.Yes == MessageBox.Show("确定要删除选中的商品？", "移除商品", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
                    {

                        int DELindex_temp = dataGridView_Cashiers.SelectedRows[0].Index;
                        //goodsBuyList.RemoveAt(DELindex_temp);

                        //dataGridView_Cashiers.Refresh();

                        dataGridView_Cashiers.Rows.RemoveAt(DELindex_temp);

                        if (DELindex_temp - 1 >= 0)
                        {
                            dataGridView_Cashiers.Rows[DELindex_temp - 1].Selected = true;
                        }
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
                saveGoodsBuyList.Clear();
                //把原商品保存下来，以便取消结算时还原
                for (int i = 0; i < goodsBuyList.Count; i++)
                {
                    saveGoodsBuyList.Add(new GoodsBuy
                    {
                        countNum = goodsBuyList[i].countNum,
                        noCode = goodsBuyList[i].noCode,
                        barCodeTM = goodsBuyList[i].barCodeTM,
                        goods = goodsBuyList[i].goods,
                        unit = goodsBuyList[i].unit,
                        unitStr = goodsBuyList[i].unitStr,
                        spec = goodsBuyList[i].spec,
                        lsPrice = goodsBuyList[i].lsPrice,
                        pinYin = goodsBuyList[i].pinYin,
                        salesClerk = goodsBuyList[i].salesClerk,
                        ywy = goodsBuyList[i].ywy,
                        goodsDes = goodsBuyList[i].goodsDes,
                        hpackSize = goodsBuyList[i].hpackSize,
                        jjPrice = goodsBuyList[i].jjPrice,
                        hyPrice = goodsBuyList[i].hyPrice,
                        status = goodsBuyList[i].status,
                        pfPrice = goodsBuyList[i].pfPrice,
                        Sum = goodsBuyList[i].Sum,
                        PP = goodsBuyList[i].PP,
                        LB = goodsBuyList[i].LB,
                        isDbItem = goodsBuyList[i].isDbItem,
                        vtype = goodsBuyList[i].vtype,
                        isTuiHuo = goodsBuyList[i].isTuiHuo,
                        isXG = goodsBuyList[i].isXG,
                        isGL = goodsBuyList[i].isGL,
                        isZS = goodsBuyList[i].isZS,
                        isCyjf = goodsBuyList[i].isCyjf,
                        jfbl = goodsBuyList[i].jfbl
                    });


                }


                using (var db = new hjnbhEntities())
                {
                    YH8XSFunc(db);   //活动8  促销调价
                    YH3WSFunc(db);   //活动3  买送
                    YH4ZHFunc(db);   //活动4  组合
                    YH2TJFunc(db);   //活动2  商品特价
                    YH7SLFunc(db);   //活动7  限量特价
                    YH6TJFunc(db);   //活动6  时段特价
                    YH9SPFunc(db);   //活动9  商品数量赠送
                    YH9PPFunc(db);   //活动9  品牌赠送
                    YH9LBFunc(db);   //活动9  类别赠送

                    YH1ZSFunc(db);   //活动1  限量赠送
                    //YH10ZSFunc(db);  //活动10   赠品限量   //1026 厚爱 说不作单独活动

                    YH5WEFunc(db);   //活动5  满额送

                    VipDateHDFunc();  //会员日  对VIP商品打折

                    IstoreFunc(db);   //库存提醒



                }



                dataGridView_Cashiers.Refresh();

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
                    vipShopForm.ShowDialog(); //默认使用会员

                }


            }
        }

        #region 重打小票赋值


        ///// <summary>
        ///// 小票赋值，保存上单的信息。 同时也是通知结算完成事件
        ///// </summary>
        ///// <param name="jsdh"></param>
        ///// <param name="vip"></param>
        //void CEform_changed(string jsdh, string vip)
        //{

        //    this.jsdh = jsdh;

        //    this.lastVipcard = vip;

        //    label8.Text = jsdh;  //上单单据



        //}
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
                HandoverModel.GetInstance.isLianxi = false;
                initData();
                mainForm.Show();
                this.Close();
            }


        }

        //每次重置窗口都要重置的数据 
        private void initData()
        {
            if (HandoverModel.GetInstance.isLianxi == false)
            {
                label25.Visible = false;
            }

            ZKZD = 0;
            totalMoney = 0;
            isNewItem = false;

            label3.Visible = false;  //你有新消息……
            label4.Visible = false;
            this.tableLayoutPanel2.Visible = false;  //隐藏结算结果

            HandoverModel.GetInstance.VipLv = 0;
            HandoverModel.GetInstance.VipID = 0;
            HandoverModel.GetInstance.VipName = string.Empty;
            HandoverModel.GetInstance.VipCard = string.Empty;
            HandoverModel.GetInstance.isVipBirthday = false;

            this.label101.Text = "按F12登记会员";

            label31.Text = "0";  //折扣额
            label32.Text = "0";   //整单折扣

            //业务员重置
            HandoverModel.GetInstance.YWYid = 0;
            HandoverModel.GetInstance.YWYStr = "";
            this.label103.Text = "未登记";

            timer_temp = 0;  //用于取消结算显示面板的计数

            richTextBox1.Clear();  //清空会员备注显示
            tabControl1.SelectedIndex = 1; //默认显示活动详情


        }

        //按*号赠送
        private void ForZsFunc()
        {
            if (goodsBuyList.Count > 0)
            {
                try
                {
                    this.isQxValied = false;
                    QXForm.ShowDialog();
                    if (isQxValied)
                    {
                        if (dataGridView_Cashiers.Rows.Count > 0)
                        {
                            int temp = dataGridView_Cashiers.SelectedRows[0].Index;

                            //把金额修改为0元
                            goodsBuyList[temp].Sum = 0.00m;
                            goodsBuyList[temp].lsPrice = 0.00m;
                            goodsBuyList[temp].hyPrice = 0.00m;
                            //把商品属性修改为赠送
                            goodsBuyList[temp].isZS = true;
                            goodsBuyList[temp].vtype = 100;  //这个不能为负数，因为数据库是type类型
                            goodsBuyList[temp].isXG = true;  //不自动累计数量
                            //把备注修改为赠送
                            string tempstr = goodsBuyList[temp].goodsDes;
                            goodsBuyList[temp].goodsDes = string.IsNullOrEmpty(tempstr) ? "主动赠送" : "[主动赠送]" + tempstr;
                            //刷新表格
                            dataGridView_Cashiers.InvalidateRow(temp);

                        }
                    }

                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("收银主界面按*号主动赠送商品时发生异常:", ex);
                }
            }
        }

        //按-号修改金额
        private void UpdataSum()
        {
            if (goodsBuyList.Count > 0)
            {
                try
                {
                    this.jeToUpdata = 0.00m;

                    if (dataGridView_Cashiers.Rows.Count > 0)
                    {
                        JEForm.ShowDialog();

                        if (jeToUpdata > 0)
                        {
                            int temp = dataGridView_Cashiers.SelectedRows[0].Index;

                            //把金额修改为0元
                            goodsBuyList[temp].Sum = jeToUpdata;
                            goodsBuyList[temp].lsPrice = Math.Round(jeToUpdata / goodsBuyList[temp].countNum, 2);
                            goodsBuyList[temp].hyPrice = Math.Round(jeToUpdata / goodsBuyList[temp].countNum, 2);
                            //把商品属性改为修改金额
                            goodsBuyList[temp].vtype = 101;  //这个不能为负数，因为数据库是type类型
                            //把备注修改为赠送
                            string tempstr = goodsBuyList[temp].goodsDes;
                            goodsBuyList[temp].goodsDes = string.IsNullOrEmpty(tempstr) ? "金额修改" : "[金额修改]" + tempstr;
                            //刷新表格
                            dataGridView_Cashiers.InvalidateRow(temp);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("收银主界面按-号修改商品金额时发生异常:", ex);
                }
            }
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
                    textBox1.Enabled = false;

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
            RDForm.ShowDialog(this);
        }





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
                dataGridView_Cashiers.Columns[27].Visible = false; //是否打包
                dataGridView_Cashiers.Columns[28].Visible = false; //是否抵额退货
                dataGridView_Cashiers.Columns[29].Visible = false; //是否积分
                dataGridView_Cashiers.Columns[30].Visible = false; //积分比例

                //列宽
                dataGridView_Cashiers.Columns[0].Width = 30;
                dataGridView_Cashiers.Columns[1].Width = 80;
                dataGridView_Cashiers.Columns[2].Width = 130;  //条码
                dataGridView_Cashiers.Columns[3].Width = 260;  //品名


                //禁止编辑单元格
                //设置单元格是否可以编辑
                for (int i = 0; i < dataGridView_Cashiers.Columns.Count; i++)
                {
                    if (dataGridView_Cashiers.Columns[i].Index != 5 && dataGridView_Cashiers.Columns[i].Index != 11)
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
                if (HandoverModel.GetInstance.isLianxi == false)
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
        //窗口尺寸改变事件
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

        //public string lastVipName;  //记录为上单的会员名字
        //会员录入事件，接受活动事件,会员变动，刷新UI
        private void showVIPuiFunc()
        {

            HDUIFunc();  //刷新价格与UI
            ReaderVipInfoFunc(); //显示备注
            VipBirthdayFunc();  //显示会员生日

        }

        //窗口关闭事件
        private void CashiersFormXP_FormClosing(object sender, FormClosingEventArgs e)
        {
            kh.UnHook();  //快捷键注销

            kh.OnKeyDownEvent -= kh_OnKeyDownEvent;

            CGForm.changed -= CGForm_changed;
            cho5.changed -= cho5_changed;


            //业务员事件
            SMFormSMForm.changed -= showYWYuiFunc;
            SMFormSMForm.ZDchanged -= SMFormSMForm_ZDchanged;

            //结算
            CEform.UIChanged -= CEform_UIChanged;
            CEform.changed -= CEform_FormESC;

            //会员
            vipShopForm.changed -= showVIPuiFunc;

            MPForm.changed -= showVIPuiFunc;

            //打折
            zkform.changed -= zkform_changed;
            zkzdform.changed -= zkzdform_changed;

            //权限验证
            QXForm.Changed -= QXForm_Changed;

            //挂单
            GNform.changed -= GNform_changed;

            //退货
            RDForm.changed -= RDForm_changed;

            //修改金额
            JEForm.changed -= JEForm_changed;
        }

        //直接在收银UI上显示会员备注信息(如果登陆会员的话)
        #region 显示会员备注
        //读取会员消息
        private void ReaderVipInfoFunc()
        {
            try
            {
                decimal qk_temp = 0.00m;  //会员欠款

                richTextBox1.Text = "";
                int VipID = HandoverModel.GetInstance.VipID;
                StringBuilder StrB = new StringBuilder();

                if (VipID <= 0)
                {
                    //MessageBox.Show("请先录入会员！");
                    return;
                }

                using (var db = new hjnbhEntities())
                {

                    //会员目前已存的商品提醒
                    var vipsaveditem = db.hd_vip_item.AsNoTracking().Where(e => e.vipcode == VipID && e.amount > 0).ToList();
                    if (vipsaveditem.Count > 0)
                    {
                        string savetemp = "";
                        foreach (var item in vipsaveditem)
                        {
                            savetemp += "[" + item.item_id + "/" + item.cname + "*" + item.amount + "] ";
                        }

                        StrB.Append("目前已存商品：" + savetemp + "。\r\n");
                    }

                    //定金与余额提醒
                    //会员欠款
                    string vipmemo = ""; //旧系统会员备注
                    decimal djtemp = 0.00m, jetemp = 0.00m;
                    var vipinfo = db.hd_vip_info.AsNoTracking().Where(t => t.vipcode == VipID).Select(t => new { t.ydje, t.czk_ye ,t.qkje,t.sVipMemo}).FirstOrDefault();
                    if (vipinfo != null)
                    {
                        //目前可用定金
                        djtemp = vipinfo.ydje.HasValue ? vipinfo.ydje.Value : 0.00m;
                        //目前可用余额
                        jetemp = vipinfo.czk_ye.HasValue ? vipinfo.czk_ye.Value : 0.00m;

                        qk_temp = vipinfo.qkje.HasValue ? vipinfo.qkje.Value : 0.00m;

                        if (!string.IsNullOrEmpty(vipinfo.sVipMemo))
                        {
                            vipmemo = vipinfo.sVipMemo;
                        }
                    }

                    StrB.Append("目前已存定金：" + djtemp.ToString() + " 元。\r\n");
                    StrB.Append("目前可用储卡金额：" + jetemp.ToString() + " 元。\r\n");


                    //分期金额提醒
                    decimal Fqje = 0.00m; //分期总金额
                    var fqinfo = db.hd_vip_fq.AsNoTracking().Where(t => t.vipcode == VipID && t.amount > 0).ToList();
                    if (fqinfo.Count > 0)
                    {
                        foreach (var item in fqinfo)
                        {
                            decimal temp = item.mqje.HasValue ? item.mqje.Value : 0;
                            temp *= item.amount.Value;
                            Fqje += temp;
                        }
                    }

                    StrB.Append("目前已存分期金额：" + Fqje.ToString("0.00") + " 元。\r\n");

                    StrB.Append("会员欠款金额提醒：" + qk_temp.ToString("0.00") + " 元。\r\n");


                    //其它备注消息提醒
                    var otherinfo = db.hd_vip_memo.AsNoTracking().Where(t => t.vipcode == VipID && t.type == 0).Select(t => t.memo).FirstOrDefault();
                    if (otherinfo != null)
                    {
                        string memotemp = string.IsNullOrEmpty(otherinfo) ? "" : otherinfo;
                        StrB.Append("会员消息提醒：" + TextByDateFunc(memotemp) + "\r\n");
                        StrB.Append("会员历史备注信息：" + TextByDateFunc(vipmemo) + "\r\n");
                    }



                    richTextBox1.Text = StrB.ToString();

                }

                tabControl1.SelectedIndex = 0; //切换选项卡
                if (qk_temp > 0)
                {
                    MessageBox.Show("注意！该会员目前已欠款：" + qk_temp.ToString("0.00") + "元", "欠款提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("收银窗口读取会员消息时出现异常:", e);
                MessageBox.Show("读取会员消息时出现异常！请联系管理员");

            }
        }


        string strEx = @"((?<!\d)((\d{2,4}(\.|年|\/|\-))((((0?[13578]|1[02])(\.|月|\/|\-))((3[01])|([12][0-9])|(0?[1-9])))|(0?2(\.|月|\/|\-)((2[0-8])|(1[0-9])|(0?[1-9])))|(((0?[469]|11)(\.|月|\/|\-))((30)|([12][0-9])|(0?[1-9]))))|((([0-9]{2})((0[48]|[2468][048]|[13579][26])|((0[48]|[2468][048]|[3579][26])00))(\.|年|\/|\-))0?2(\.|月|\/|\-)29))日?(?!\d))";
        //正则匹配日期
        //string strEx = @"^((((1[6-9]|[2-9]\d)\d{2})-(0?[13578]|1[02])-(0?[1-9]|[12]\d|3[01]))|(((1[6-9]|[2-9]\d)\d{2})-(0?[13456789]|1[012])-(0?[1-9]|[12]\d|30))|(((1[6-9]|[2-9]\d)\d{2})-0?2-(0?[1-9]|1\d|2[0-9]))|(((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))-0?2-29-))$";
        //按时间分割文本
        private string TextByDateFunc(string text)
        {
            return (Regex.Replace(text, strEx, "\r\n$1"));

        }
        #endregion

        #region 活动提醒设置

        /// <summary>
        /// 活动提醒
        /// </summary>
        private void HDTipFunc()
        {
            int hdscode = HandoverModel.GetInstance.scode;
            var hdinfoList = new BindingList<HDTipModel>(); //活动信息缓存
            //读取目前有效的活动
            using (var db = new hjnbhEntities())
            {
                var hdinfo = db.v_yh_detail.AsNoTracking().Where(t => t.scode == hdscode).ToList();
                if (hdinfo.Count > 0)
                {

                    foreach (var item in hdinfo)
                    {
                        //活动1
                        if (item.vtype == 1)
                        {
                            //活动10的判断  
                            var hd10 = hdinfo.Where(t => t.vtype == 10 && t.item_id == item.item_id).ToList();
                            if (hd10.Count > 0)
                            {
                                var hd10Count = hd10.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                if (hd10Count > 0)
                                {
                                    //有效
                                    hdinfoList.Add(new HDTipModel
                                    {
                                        vtypeStr = HDtypeFunc(item.vtype),
                                        dxStr = HDdxFunc(item.dx_type),
                                        vipTypeStr = HDvipdxFunc(item.viptype),
                                        xgStr = item.xg_amount > 0 ? item.xg_amount.ToString("0.00") : "无",
                                        hdItemStr = item.cname,
                                        countStr = item.amount > 0 ? item.amount.ToString("0.00") : "1.00",
                                        lsStr = item.ls_price.HasValue ? item.ls_price.Value.ToString("0.00") : "空",
                                        zsStr = item.zs_cname,
                                        zsCountStr = item.zs_amount > 0 ? item.zs_amount.ToString("0.00") : "1.00",
                                        zsSaveCountStr = hd10Count.ToString("0.00"),
                                        beginTimeStr = item.sbegintime.HasValue ? item.sbegintime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空",
                                        endTimeStr = item.sendtime.HasValue ? item.sendtime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空"
                                    });
                                }
                            }
                            else
                            {
                                //有效,没有活动10的情况
                                hdinfoList.Add(new HDTipModel
                                {
                                    vtypeStr = HDtypeFunc(item.vtype),
                                    dxStr = HDdxFunc(item.dx_type),
                                    vipTypeStr = HDvipdxFunc(item.viptype),
                                    xgStr = item.xg_amount > 0 ? item.xg_amount.ToString("0.00") : "无",
                                    hdItemStr = item.cname,
                                    countStr = item.amount > 0 ? item.amount.ToString("0.00") : "1.00",
                                    lsStr = item.ls_price.HasValue ? item.ls_price.Value.ToString("0.00") : "空",
                                    zsStr = item.zs_cname,
                                    zsCountStr = item.zs_amount > 0 ? item.zs_amount.ToString("0.00") : "1.00",
                                    zsSaveCountStr = "不限",
                                    beginTimeStr = item.sbegintime.HasValue ? item.sbegintime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空",
                                    endTimeStr = item.sendtime.HasValue ? item.sendtime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空"
                                });
                            }
                        }

                        //活动2
                        if (item.vtype == 2)
                        {
                            //活动10的判断
                            var hd10 = hdinfo.Where(t => t.vtype == 10 && t.item_id == item.zs_item_id ).ToList();
                            if (hd10.Count > 0)
                            {
                                var hd10Count = hd10.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                if (hd10Count > 0)
                                {
                                    //有效
                                    hdinfoList.Add(new HDTipModel
                                    {
                                        vtypeStr = HDtypeFunc(item.vtype),
                                        dxStr = HDdxFunc(item.dx_type),
                                        vipTypeStr = HDvipdxFunc(item.viptype),
                                        xgStr = item.xg_amount > 0 ? item.xg_amount.ToString("0.00") : "无",
                                        hdItemStr = item.cname,
                                        countStr = item.amount > 0 ? item.amount.ToString("0.00") : "1.00",
                                        lsStr = item.ls_price.HasValue ? item.ls_price.Value.ToString("0.00") : "空",
                                        zsStr = item.zs_cname,
                                        zsCountStr = item.zs_amount > 0 ? item.zs_amount.ToString("0.00") : "1.00",
                                        zsSaveCountStr = hd10Count.ToString("0.00"),
                                        beginTimeStr = item.sbegintime.HasValue ? item.sbegintime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空",
                                        endTimeStr = item.sendtime.HasValue ? item.sendtime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空"
                                    });
                                }
                            }
                            else
                            {
                                //有效
                                hdinfoList.Add(new HDTipModel
                                {
                                    vtypeStr = HDtypeFunc(item.vtype),
                                    dxStr = HDdxFunc(item.dx_type),
                                    vipTypeStr = HDvipdxFunc(item.viptype),
                                    xgStr = item.xg_amount > 0 ? item.xg_amount.ToString("0.00") : "无",
                                    hdItemStr = item.cname,
                                    countStr = item.amount > 0 ? item.amount.ToString("0.00") : "1.00",
                                    lsStr = item.ls_price.HasValue ? item.ls_price.Value.ToString("0.00") : "空",
                                    zsStr = item.zs_cname,
                                    zsCountStr = item.zs_amount > 0 ? item.zs_amount.ToString("0.00") : "1.00",
                                    zsSaveCountStr = "不限",
                                    beginTimeStr = item.sbegintime.HasValue ? item.sbegintime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空",
                                    endTimeStr = item.sendtime.HasValue ? item.sendtime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空"
                                });
                            }
                        }

                        //活动3
                        if (item.vtype == 3)
                        {
                            //活动10的判断
                            var hd10 = hdinfo.Where(t => t.vtype == 10 && t.item_id == item.zs_item_id).ToList();
                            if (hd10.Count > 0)
                            {
                                var hd10Count = hd10.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                if (hd10Count > 0)
                                {
                                    //有效
                                    hdinfoList.Add(new HDTipModel
                                    {
                                        vtypeStr = HDtypeFunc(item.vtype),
                                        dxStr = HDdxFunc(item.dx_type),
                                        vipTypeStr = HDvipdxFunc(item.viptype),
                                        xgStr = item.xg_amount > 0 ? item.xg_amount.ToString("0.00") : "无",
                                        hdItemStr = item.cname,
                                        countStr = item.amount > 0 ? item.amount.ToString("0.00") : "1.00",
                                        lsStr = item.ls_price.HasValue ? item.ls_price.Value.ToString("0.00") : "空",
                                        zsStr = item.zs_cname,
                                        zsCountStr = item.zs_amount > 0 ? item.zs_amount.ToString("0.00") : "1.00",
                                        zsSaveCountStr = hd10Count.ToString("0.00"),
                                        beginTimeStr = item.sbegintime.HasValue ? item.sbegintime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空",
                                        endTimeStr = item.sendtime.HasValue ? item.sendtime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空"
                                    });
                                }
                            }
                            else
                            {
                                //有效
                                hdinfoList.Add(new HDTipModel
                                {
                                    vtypeStr = HDtypeFunc(item.vtype),
                                    dxStr = HDdxFunc(item.dx_type),
                                    vipTypeStr = HDvipdxFunc(item.viptype),
                                    xgStr = item.xg_amount > 0 ? item.xg_amount.ToString("0.00") : "无",
                                    hdItemStr = item.cname,
                                    countStr = item.amount > 0 ? item.amount.ToString("0.00") : "1.00",
                                    lsStr = item.ls_price.HasValue ? item.ls_price.Value.ToString("0.00") : "空",
                                    zsStr = item.zs_cname,
                                    zsCountStr = item.zs_amount > 0 ? item.zs_amount.ToString("0.00") : "1.00",
                                    zsSaveCountStr = "不限",
                                    beginTimeStr = item.sbegintime.HasValue ? item.sbegintime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空",
                                    endTimeStr = item.sendtime.HasValue ? item.sendtime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空"
                                });
                            }
                        }


                        //活动4
                        if (item.vtype == 4)
                        {
                            //有效
                            hdinfoList.Add(new HDTipModel
                            {
                                vtypeStr = HDtypeFunc(item.vtype),
                                dxStr = HDdxFunc(item.dx_type),
                                vipTypeStr = HDvipdxFunc(item.viptype),
                                xgStr = item.xg_amount > 0 ? item.xg_amount.ToString("0.00") : "无",
                                hdItemStr = item.cname,
                                countStr = item.amount > 0 ? item.amount.ToString("0.00") : "1.00",
                                lsStr = item.ls_price.HasValue ? item.ls_price.Value.ToString("0.00") : "空",
                                zsStr = item.zs_cname,
                                zsCountStr = item.zs_amount > 0 ? item.zs_amount.ToString("0.00") : "1.00",
                                zsSaveCountStr = "不限",
                                beginTimeStr = item.sbegintime.HasValue ? item.sbegintime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空",
                                endTimeStr = item.sendtime.HasValue ? item.sendtime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空"
                            });

                        }

                        //活动5
                        if (item.vtype == 5)
                        {
                            //活动10的判断
                            var hd10 = hdinfo.Where(t => t.vtype == 10 && t.item_id == item.zs_item_id ).ToList();
                            if (hd10.Count > 0)
                            {
                                var hd10Count = hd10.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                if (hd10Count > 0)
                                {
                                    //有效
                                    hdinfoList.Add(new HDTipModel
                                    {
                                        vtypeStr = HDtypeFunc(item.vtype),
                                        dxStr = HDdxFunc(item.dx_type),
                                        vipTypeStr = HDvipdxFunc(item.viptype),
                                        xgStr = item.xg_amount > 0 ? item.xg_amount.ToString("0.00") : "无",
                                        hdItemStr = item.cname,
                                        countStr = item.amount > 0 ? item.amount.ToString("0.00") : "1.00",
                                        lsStr = item.ls_price.HasValue ? item.ls_price.Value.ToString("0.00") : "空",
                                        zsStr = item.zs_cname,
                                        zsCountStr = item.zs_amount > 0 ? item.zs_amount.ToString("0.00") : "1.00",
                                        zsSaveCountStr = hd10Count.ToString("0.00"),
                                        beginTimeStr = item.sbegintime.HasValue ? item.sbegintime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空",
                                        endTimeStr = item.sendtime.HasValue ? item.sendtime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空"
                                    });
                                }
                            }
                            else
                            {
                                //有效
                                hdinfoList.Add(new HDTipModel
                                {
                                    vtypeStr = HDtypeFunc(item.vtype),
                                    dxStr = HDdxFunc(item.dx_type),
                                    vipTypeStr = HDvipdxFunc(item.viptype),
                                    xgStr = item.xg_amount > 0 ? item.xg_amount.ToString("0.00") : "无",
                                    hdItemStr = item.cname,
                                    countStr = item.amount > 0 ? item.amount.ToString("0.00") : "1.00",
                                    lsStr = item.ls_price.HasValue ? item.ls_price.Value.ToString("0.00") : "空",
                                    zsStr = item.zs_cname,
                                    zsCountStr = item.zs_amount > 0 ? item.zs_amount.ToString("0.00") : "1.00",
                                    zsSaveCountStr = "不限",
                                    beginTimeStr = item.sbegintime.HasValue ? item.sbegintime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空",
                                    endTimeStr = item.sendtime.HasValue ? item.sendtime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空"
                                });
                            }
                        }

                        //活动6
                        if (item.vtype == 6)
                        {
                            //活动10的判断
                            var hd10 = hdinfo.Where(t => t.vtype == 10 && t.item_id == item.zs_item_id ).ToList();
                            if (hd10.Count > 0)
                            {
                                var hd10Count = hd10.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                if (hd10Count > 0)
                                {
                                    //有效
                                    hdinfoList.Add(new HDTipModel
                                    {
                                        vtypeStr = HDtypeFunc(item.vtype),
                                        dxStr = HDdxFunc(item.dx_type),
                                        vipTypeStr = HDvipdxFunc(item.viptype),
                                        xgStr = item.xg_amount > 0 ? item.xg_amount.ToString("0.00") : "无",
                                        hdItemStr = item.cname,
                                        countStr = item.amount > 0 ? item.amount.ToString("0.00") : "1.00",
                                        lsStr = item.ls_price.HasValue ? item.ls_price.Value.ToString("0.00") : "空",
                                        zsStr = item.zs_cname,
                                        zsCountStr = item.zs_amount > 0 ? item.zs_amount.ToString("0.00") : "1.00",
                                        zsSaveCountStr = hd10Count.ToString("0.00"),
                                        beginTimeStr = item.sbegintime.HasValue ? item.sbegintime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空",
                                        endTimeStr = item.sendtime.HasValue ? item.sendtime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空"
                                    });
                                }
                            }
                            else
                            {
                                //有效
                                hdinfoList.Add(new HDTipModel
                                {
                                    vtypeStr = HDtypeFunc(item.vtype),
                                    dxStr = HDdxFunc(item.dx_type),
                                    vipTypeStr = HDvipdxFunc(item.viptype),
                                    xgStr = item.xg_amount > 0 ? item.xg_amount.ToString("0.00") : "无",
                                    hdItemStr = item.cname,
                                    countStr = item.amount > 0 ? item.amount.ToString("0.00") : "1.00",
                                    lsStr = item.ls_price.HasValue ? item.ls_price.Value.ToString("0.00") : "空",
                                    zsStr = item.zs_cname,
                                    zsCountStr = item.zs_amount > 0 ? item.zs_amount.ToString("0.00") : "1.00",
                                    zsSaveCountStr = "不限",
                                    beginTimeStr = item.sbegintime.HasValue ? item.sbegintime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空",
                                    endTimeStr = item.sendtime.HasValue ? item.sendtime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空"
                                });
                            }
                        }

                        //活动7
                        if (item.vtype == 7)
                        {
                            //活动10的判断
                            var hd10 = hdinfo.Where(t => t.vtype == 10 && t.item_id == item.zs_item_id ).ToList();
                            if (hd10.Count > 0)
                            {
                                var hd10Count = hd10.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                if (hd10Count > 0)
                                {
                                    //有效
                                    hdinfoList.Add(new HDTipModel
                                    {
                                        vtypeStr = HDtypeFunc(item.vtype),
                                        dxStr = HDdxFunc(item.dx_type),
                                        vipTypeStr = HDvipdxFunc(item.viptype),
                                        xgStr = item.xg_amount > 0 ? item.xg_amount.ToString("0.00") : "无",
                                        hdItemStr = item.cname,
                                        countStr = item.amount > 0 ? item.amount.ToString("0.00") : "1.00",
                                        lsStr = item.ls_price.HasValue ? item.ls_price.Value.ToString("0.00") : "空",
                                        zsStr = item.zs_cname,
                                        zsCountStr = item.zs_amount > 0 ? item.zs_amount.ToString("0.00") : "1.00",
                                        zsSaveCountStr = hd10Count.ToString("0.00"),
                                        beginTimeStr = item.sbegintime.HasValue ? item.sbegintime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空",
                                        endTimeStr = item.sendtime.HasValue ? item.sendtime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空"
                                    });
                                }
                            }
                            else
                            {
                                //有效
                                hdinfoList.Add(new HDTipModel
                                {
                                    vtypeStr = HDtypeFunc(item.vtype),
                                    dxStr = HDdxFunc(item.dx_type),
                                    vipTypeStr = HDvipdxFunc(item.viptype),
                                    xgStr = item.xg_amount > 0 ? item.xg_amount.ToString("0.00") : "无",
                                    hdItemStr = item.cname,
                                    countStr = item.amount > 0 ? item.amount.ToString("0.00") : "1.00",
                                    lsStr = item.ls_price.HasValue ? item.ls_price.Value.ToString("0.00") : "空",
                                    zsStr = item.zs_cname,
                                    zsCountStr = item.zs_amount > 0 ? item.zs_amount.ToString("0.00") : "1.00",
                                    zsSaveCountStr = "不限",
                                    beginTimeStr = item.sbegintime.HasValue ? item.sbegintime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空",
                                    endTimeStr = item.sendtime.HasValue ? item.sendtime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空"
                                });
                            }
                        }

                        //活动8
                        if (item.vtype == 8)
                        {
                            //有效
                            hdinfoList.Add(new HDTipModel
                            {
                                vtypeStr = HDtypeFunc(item.vtype),
                                dxStr = HDdxFunc(item.dx_type),
                                vipTypeStr = HDvipdxFunc(item.viptype),
                                xgStr = item.xg_amount > 0 ? item.xg_amount.ToString("0.00") : "无",
                                hdItemStr = item.cname,
                                countStr = item.amount > 0 ? item.amount.ToString("0.00") : "1.00",
                                lsStr = item.ls_price.HasValue ? item.ls_price.Value.ToString("0.00") : "空",
                                zsStr = item.zs_cname,
                                zsCountStr = item.zs_amount > 0 ? item.zs_amount.ToString("0.00") : "1.00",
                                zsSaveCountStr = "不限",
                                beginTimeStr = item.sbegintime.HasValue ? item.sbegintime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空",
                                endTimeStr = item.sendtime.HasValue ? item.sendtime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空"
                            });

                        }

                        //活动9
                        if (item.vtype == 9)
                        {
                            //活动10的判断
                            var hd10 = hdinfo.Where(t => t.vtype == 10 && t.item_id == item.zs_item_id ).ToList();
                            if (hd10.Count > 0)
                            {
                                var hd10Count = hd10.Where(t => t.amount > 0).Select(t => t.amount).Sum();
                                if (hd10Count > 0)
                                {
                                    //有效
                                    hdinfoList.Add(new HDTipModel
                                    {
                                        vtypeStr = HDtypeFunc(item.vtype, (int)item.tj_range.Value),
                                        dxStr = HDdxFunc(item.dx_type),
                                        vipTypeStr = HDvipdxFunc(item.viptype),
                                        xgStr = item.xg_amount > 0 ? item.xg_amount.ToString("0.00") : "无",
                                        hdItemStr = item.cname,
                                        countStr = item.amount > 0 ? item.amount.ToString("0.00") : "1.00",
                                        lsStr = item.ls_price.HasValue ? item.ls_price.Value.ToString("0.00") : "空",
                                        zsStr = item.zs_cname,
                                        zsCountStr = item.zs_amount > 0 ? item.zs_amount.ToString("0.00") : "1.00",
                                        zsSaveCountStr = hd10Count.ToString("0.00"),
                                        beginTimeStr = item.sbegintime.HasValue ? item.sbegintime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空",
                                        endTimeStr = item.sendtime.HasValue ? item.sendtime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空"
                                    });
                                }

                            }
                            else
                            {
                                //有效
                                hdinfoList.Add(new HDTipModel
                                {
                                    vtypeStr = HDtypeFunc(item.vtype, (int)item.tj_range.Value),
                                    dxStr = HDdxFunc(item.dx_type),
                                    vipTypeStr = HDvipdxFunc(item.viptype),
                                    xgStr = item.xg_amount > 0 ? item.xg_amount.ToString("0.00") : "无",
                                    hdItemStr = item.cname,
                                    countStr = item.amount > 0 ? item.amount.ToString("0.00") : "1.00",
                                    lsStr = item.ls_price.HasValue ? item.ls_price.Value.ToString("0.00") : "空",
                                    zsStr = item.zs_cname,
                                    zsCountStr = item.zs_amount > 0 ? item.zs_amount.ToString("0.00") : "1.00",
                                    zsSaveCountStr = "不限",
                                    beginTimeStr = item.sbegintime.HasValue ? item.sbegintime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空",
                                    endTimeStr = item.sendtime.HasValue ? item.sendtime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空"
                                });
                            }
                        }


                        //活动10
                        if (item.vtype == 10)
                        {
                            //活动10的判断
                            if (item.amount > 0)
                            {
                                //有效
                                hdinfoList.Add(new HDTipModel
                                {
                                    vtypeStr = HDtypeFunc(item.vtype),
                                    dxStr = HDdxFunc(item.dx_type),
                                    vipTypeStr = HDvipdxFunc(item.viptype),
                                    xgStr = item.xg_amount > 0 ? item.xg_amount.ToString("0.00") : "无",
                                    hdItemStr = item.cname,
                                    countStr = item.amount > 0 ? item.amount.ToString("0.00") : "1.00",
                                    lsStr = item.ls_price.HasValue ? item.ls_price.Value.ToString("0.00") : "空",
                                    zsStr = item.zs_cname,
                                    zsCountStr = item.zs_amount > 0 ? item.zs_amount.ToString("0.00") : "1.00",
                                    zsSaveCountStr = item.amount.ToString("0.00"),
                                    beginTimeStr = item.sbegintime.HasValue ? item.sbegintime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空",
                                    endTimeStr = item.sendtime.HasValue ? item.sendtime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "空"
                                });
                            }
                        }

                    }
                }
            }

            dataGridView1.DataSource = hdinfoList;
        }

        //返回活动名称
        private string HDtypeFunc(int hdtype, int tj = 0)
        {
            string temp = "";
            switch (hdtype)
            {
                case 1:
                    temp = "限量赠送";
                    break;
                case 2:
                    temp = "零售特价";
                    break;
                case 3:
                    temp = "买一送一";
                    break;
                case 4:
                    temp = "组合优惠";
                    break;
                case 5:
                    temp = "满购赠送";
                    break;
                case 6:
                    temp = "时段特价";
                    break;
                case 8:
                    temp = "促销降价";
                    break;
                case 7:
                    temp = "限量特价";
                    break;
                case 9:
                    if (tj == 0)
                    {
                        temp = "商品满赠";
                    }
                    if (tj == 1)
                    {
                        temp = "分类满赠";
                    }
                    if (tj == 2)
                    {
                        temp = "品牌满赠";
                    }
                    break;
                case 10:
                    temp = "限量商品";
                    break;
            }

            return temp;
        }


        //返回活动限定对象
        private string HDdxFunc(int dxtype)
        {
            string temp = "";
            switch (dxtype)
            {
                case 0:
                    temp = "所有顾客";
                    break;
                case 1:
                    temp = "限定会员";
                    break;
            }

            return temp;
        }

        //返回会员等级限定
        private string HDvipdxFunc(int vipdx)
        {
            string temp = "";
            switch (vipdx)
            {
                case -1:
                    temp = "所有顾客";
                    break;
                case 0:
                    temp = "所有会员";
                    break;
                case 1:
                    temp = "普通会员";
                    break;
                case 2:
                    temp = "黄金会员";
                    break;
                case 3:
                    temp = "钻石会员";
                    break;
            }

            return temp;
        }

        //列表改名
        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                dataGridView1.Columns[0].HeaderText = "活动类型";
                dataGridView1.Columns[1].HeaderText = "活动对象";
                dataGridView1.Columns[2].HeaderText = "会员类型";
                dataGridView1.Columns[3].HeaderText = "限购数量";
                dataGridView1.Columns[4].HeaderText = "活动商品";
                dataGridView1.Columns[5].HeaderText = "数量条件";
                dataGridView1.Columns[6].HeaderText = "活动价格";
                dataGridView1.Columns[7].HeaderText = "赠送商品";
                dataGridView1.Columns[8].HeaderText = "赠送数量";
                dataGridView1.Columns[9].HeaderText = "剩余数量";
                dataGridView1.Columns[10].HeaderText = "开始时间";
                dataGridView1.Columns[11].HeaderText = "结束时间";

            }
            catch
            {
            }
        }


        #endregion


        bool isStoreTip = true; //库存提醒开关
        // 结算时判断商品库存
        private void IstoreFunc(hjnbhEntities db)
        {
            if (isStoreTip == false) return;

            bool istip = false; //是否有低库存的商品需要提醒
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

                //去掉购物车中重复的商品
                var inedx_temp = goodsBuyList.Select(t => t.noCode).Distinct().ToList();
                foreach (var item_index in inedx_temp)
                {
                    //过滤掉退货商品的提示
                    var info = goodsBuyList.Where(t => t.noCode == item_index && t.isTuiHuo == false).FirstOrDefault();
                    if (info != null)
                    {

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

        //表格完成数据绑定事件
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
                            if (temp == 0) temp = 7;
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
                    //可以打折的商品
                    if (goodsBuyList[i].vtype == 0 && goodsBuyList[i].isZS == false && VipID != 0 && goodsBuyList[i].isTuiHuo == false)
                    {
                        decimal temp = goodsBuyList[i].hyPrice.Value * vipDateZkl;
                        goodsBuyList[i].hyPrice = Math.Round(temp, 2);
                        goodsBuyList[i].Sum = Math.Round(temp * goodsBuyList[i].countNum, 2);
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
            int vipidtemp = HandoverModel.GetInstance.VipID;
            for (int i = 0; i < goodsBuyList.Count; i++)
            {
                if (goodsBuyList[i].vtype != 0)
                {
                    if (e.ColumnIndex == 13 && e.RowIndex == i)
                    {
                        e.CellStyle.ForeColor = Color.Red;
                    }
                }
                else if (vipidtemp != 0 && goodsBuyList[i].vtype == 0)
                {
                    if (e.ColumnIndex == 13 && e.RowIndex == i)
                    {
                        e.CellStyle.ForeColor = Color.Gold;
                    }
                }

            }

        }


        // 判断是否会员生日
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


        //禁止输入+号与-号  *号
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)43 || (e.KeyChar == (char)42) || (e.KeyChar == (char)45))
            {
                e.Handled = true;
            }
        }











    }
}
