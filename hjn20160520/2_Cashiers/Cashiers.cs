using Common;
using hjn20160520._2_Cashiers;
using hjn20160520._4_Detail;
using hjn20160520._9_VIPCard;
using hjn20160520.Common;
using hjn20160520.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Configuration;

namespace hjn20160520
{
    /// <summary>
    /// 结算方式
    /// </summary>
    public enum JSType
    {
        Cash = 0, //现金
        UnionPay = 1, //银联卡
        Coupon = 2,  //购物劵
        Others = 3  //其它
    }


    /// <summary>
    /// 收银模块主逻辑
    /// </summary>
    public partial class Cashiers : Form
    {
        //单例
        public static Cashiers GetInstance { get; private set; }

        ChoiceGoods choice;  // 商品选择窗口
        ClosingEntries CEform;  //  商品结算窗口
        GoodsNote GNform; //挂单窗口
        MainForm mainForm;  //主菜单
        MemberPointsForm MPForm;  //会员积分冲减窗口
        SalesmanForm SMForm; //业务员录入窗口
        LockScreenForm LSForm;  //锁屏窗口
        RefundForm RDForm;  //退货窗口
        VipShopForm vipForm; //会员消费窗口

        public bool isLianXi { get; set; }  //是否练习模式
        //公共提示信息窗口
        TipForm tipForm;

        //记录购物车内的商品
        public BindingList<GoodsBuy> goodsBuyList = new BindingList<GoodsBuy>();
        //记录从数据库查到的商品
        public BindingList<GoodsBuy> goodsChooseList = new BindingList<GoodsBuy>();
        //挂单取单窗口的挂单列表
        public BindingList<GoodsNoteModel> noteList = new BindingList<GoodsNoteModel>();
        //挂单窗口中订单号与订单商品清单对应的字典列表
        public Dictionary<int, BindingList<GoodsBuy>> noteDict = new Dictionary<int, BindingList<GoodsBuy>>();

        #region 收银属性

        //整单折扣率
        public decimal? ZKZD { get; set; }
        //单品折扣率临时变量
        public decimal? ZKDP_temp { get; set; }

        //单号，临时，以后要放上数据库读取
        public int OrderNo = 0;

        //标志一单交易,是否新单
        public bool isNewItem = false;

        //应收总金额
        public decimal? totalMoney { get; set; }


        //进行消费的会员卡号
        public int VipID { get; set; }
        //会员备注消息
        public string VipMdemo { get; set; }

        public PrintHelper printer;  //小票打印

        public BindingList<GoodsBuy> lastGoodsList = new BindingList<GoodsBuy>();  //上单购物清单

        #endregion

        public Cashiers()
        {
            InitializeComponent();
        }


        //右下方当前时间
        private void timer1_Tick(object sender, EventArgs e)
        {
            label_timer.Text = " 当前时间：" + System.DateTime.Now.ToString();
        }

        //窗口初始化
        private void Form1_Load(object sender, EventArgs e)
        {
            //单例赋值
            if (GetInstance == null) GetInstance = this;

            Init();
        }

        //初始化窗口
        private void Init()
        {
            // 窗口全屏设置全屏

            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            //this.TopMost = true;  //窗口顶置

            if (isLianXi)
            {
                label25.Visible = true;
            }

            //时间开始
            timer1.Start();
            //窗口赋值
            CEform = new ClosingEntries();
            choice = new ChoiceGoods();
            GNform = new GoodsNote();
            mainForm = new MainForm();
            MPForm = new MemberPointsForm();
            tipForm = new TipForm();
            SMForm = new SalesmanForm();
            LSForm = new LockScreenForm();
            vipForm = new VipShopForm();

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
            label26.Text = HandoverModel.GetInstance.bcode.ToString(); //机号
        }




        //计时器点击事件（没用到）
        private void label_timer_Click(object sender, EventArgs e)
        {

        }


        /*换个思路处理优惠活动：
1、每次添加商品到购物车时 ， 先进行促销活动判断，判断结束后添加商品。
2、遍历购物车里所有商品，判断是否有优惠活动的商品，再判断是否满足优惠条件来进行优惠操作。*/

        #region 商品查询
        //根据条码通过EF进行模糊查询
        private void EFSelectByBarCode()
        {
            //try
            //{
                #region 查询操作

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
                        //查打包商品
                        var itemdb = db.hd_item_db.AsNoTracking().Where(t => t.item_id.ToString() == temptxt || t.sitem_id.ToString().Contains(temptxt)).ToList();

                        if (itemdb.Count == 0)
                        {
                            this.textBox1.SelectAll();
                            tipForm.Tiplabel.Text = "没有查找到该商品!";
                            tipForm.ShowDialog();
                            return;
                        }

                        #region 查到多条记录时
                        //查询到多条则弹出商品选择窗口，排除表格在正修改时发生判断
                        if (itemdb.Count > 1 && !dataGridView_Cashiers.IsCurrentCellInEditMode)
                        {
                            string tip_temp = Tipslabel.Text;
                            Tipslabel.Text = "商品正在查询中，请稍等！";
                            var form1 = new ChoiceGoods();
                            foreach (var item in itemdb)
                            {

                                goodsChooseList.Add(new GoodsBuy
                                {
                                    noCode = (int)item.item_id.Value,
                                    barCodeTM = "",
                                    goods = "",
                                    unit = 0,
                                    unitStr = "",
                                    spec = "",
                                    lsPrice = item.bj,
                                    pinYin = "",
                                    salesClerk = HandoverModel.GetInstance.YWYStr,
                                    goodsDes = "",
                                    //hpackSize = item.hpsize,
                                    //jjPrice = item.JJprice,
                                    hyPrice = item.vip_bj,
                                    //status = item.Status,
                                    //pfPrice = item.PFprice
                                });
                            }

                            Tipslabel.Text = tip_temp;

                            form1.dataGridView1.DataSource = goodsChooseList;
                            form1.ShowDialog();
                        }


                        #endregion

                        if (itemdb.Count == 1 && !dataGridView_Cashiers.IsCurrentCellInEditMode)
                        {
                            GoodsBuy newGoods_temp = new GoodsBuy();
                            foreach (var item in itemdb)
                            {

                                newGoods_temp = new GoodsBuy
                                {
                                    noCode = (int)item.item_id.Value,
                                    barCodeTM = "",
                                    goods = "",
                                    unit = 0,
                                    unitStr = "",
                                    spec = "",
                                    lsPrice = item.bj,
                                    pinYin = "",
                                    salesClerk = HandoverModel.GetInstance.YWYStr,
                                    goodsDes = "",
                                    //hpackSize = item.hpsize,
                                    //jjPrice = item.JJprice,
                                    hyPrice = item.vip_bj,
                                    //status = item.Status,
                                    //pfPrice = item.PFprice

                                };

                            }

                            if (goodsBuyList.Count == 0)
                            {
                                goodsBuyList.Add(newGoods_temp);

                            }
                            else
                            {

                                if (goodsBuyList.Any(n => n.noCode == newGoods_temp.noCode))
                                {
                                    var o = goodsBuyList.Where(p => p.noCode == newGoods_temp.noCode).FirstOrDefault();
                                    o.countNum++;
                                    dataGridView_Cashiers.Refresh();
                                }
                                else
                                {
                                    goodsBuyList.Add(newGoods_temp);
                                }

                            }
                        }


                    }

                #endregion

                    #region 查到多条记录时
                    //查询到多条则弹出商品选择窗口，排除表格在正修改时发生判断
                    if (rules.Count > 1 && !dataGridView_Cashiers.IsCurrentCellInEditMode)
                    {
                        string tip_temp = Tipslabel.Text;
                        Tipslabel.Text = "商品正在查询中，请稍等！";
                        var form1 = new ChoiceGoods();
                        foreach (var item in rules)
                        {
                            #region 商品单位查询
                            //需要把单位编号转换为中文以便UI显示
                            int unitID = item.unit.HasValue ? (int)item.unit : 1;
                            string dw = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == unitID).Select(t => t.txt1).FirstOrDefault();
                            #endregion

                            goodsChooseList.Add(new GoodsBuy
                            {
                                noCode = item.noCode,
                                barCodeTM = item.BarCode,
                                goods = item.Goods,
                                unit = unitID,
                                unitStr = dw,
                                spec = item.spec,
                                lsPrice = item.retails,
                                pinYin = item.pinyin,
                                salesClerk = HandoverModel.GetInstance.YWYStr,
                                goodsDes = item.goodsDes,
                                hpackSize = item.hpsize,
                                jjPrice = item.JJprice,
                                hyPrice = item.hyprice,
                                status = item.Status,
                                pfPrice = item.PFprice
                            });
                        }

                        Tipslabel.Text = tip_temp;

                        form1.dataGridView1.DataSource = goodsChooseList;
                        form1.ShowDialog();
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
                                lsPrice = item.retails,
                                pinYin = item.pinyin,
                                salesClerk = HandoverModel.GetInstance.YWYStr,
                                goodsDes = item.goodsDes,
                                hpackSize = item.hpsize,
                                jjPrice = item.JJprice,
                                hyPrice = item.hyprice,
                                status = item.Status,
                                pfPrice = item.PFprice

                            };

                        }

                        if (goodsBuyList.Count == 0)
                        {
                            goodsBuyList.Add(newGoods_temp);

                        }
                        else
                        {

                            if (goodsBuyList.Any(n => n.noCode == newGoods_temp.noCode))
                            {
                                var o = goodsBuyList.Where(p => p.noCode == newGoods_temp.noCode).FirstOrDefault();
                                o.countNum++;
                                dataGridView_Cashiers.Refresh();
                            }
                            else
                            {
                                goodsBuyList.Add(newGoods_temp);
                            }

                        }
                        #endregion


                    #endregion

                        //促销活动
                        XSHDFunc(db);

                        //优惠活动
                        YHHDFunc(db);

                    }
                }
            //}
            //catch (Exception e)
            //{
            //    LogHelper.WriteLog("收银主窗口查询商品时出现异常:", e);
            //    MessageBox.Show("数据库连接出错！");
            //    string tip = ConnectionHelper.ToDo();
            //    if (!string.IsNullOrEmpty(tip))
            //    {
            //        MessageBox.Show(tip);
            //    }
            //}
        }
        #endregion


        //打包的商品
        private void itemdbFunc(hjnbhEntities db, string temptxt)
        {
            var itemdb = db.hd_item_db.AsNoTracking().Where(t => t.item_id.ToString() == temptxt || t.sitem_id.ToString().Contains(temptxt)).FirstOrDefault();
            if (itemdb != null)
            {

            }
        }



        //促销活动处理逻辑(如果在促销视图中找到商品就会调整会员价)
        public void XSHDFunc(hjnbhEntities db)
        {
            //if (string.IsNullOrEmpty(VipID.ToString()) || VipID == 0) return;  //目前设置非会员不享受促销价
            try
            {
                int scode_temp = HandoverModel.GetInstance.scode;
                foreach (var item in goodsBuyList)
                {
                    //判断分店与货号是否符合活动条件
                    var xsinfo = db.v_xs_item_info.AsNoTracking().Where(t => t.item_id == item.noCode && t.scode == scode_temp).FirstOrDefault();
                    if (xsinfo != null)
                    {
                        if (!string.IsNullOrEmpty(xsinfo.hy_price))
                        {
                            item.hyPrice = Convert.ToDecimal(xsinfo.hy_price);
                        }

                        item.goodsDes = xsinfo.memo;
                        //限购
                        if (xsinfo.xg_amount > 0)
                        {
                            if (item.countNum > xsinfo.xg_amount)
                            {
                                item.countNum = Convert.ToInt32(xsinfo.xg_amount);
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
        public void YHHDFunc(hjnbhEntities db)
        {
            //try
            //{
                #region 处理优惠活动
                int scode_te = HandoverModel.GetInstance.scode;
                //2遍历购物车中每个商品看是否有优惠活动的商品
                for (int i = 0; i < goodsBuyList.Count; i++)
                {
                    
                    //这种写法如果同时有两个活动商品时他就分不清了，导致商品活动重复判断
                    int itemid = goodsBuyList[i].noCode;
                    string itemTM = goodsBuyList[i].barCodeTM;
                    //判断分店与货号是否符合活动条件
                    var YhInfo = db.v_yh_detail.AsNoTracking().Where(t => t.item_id == itemid && t.scode == scode_te).FirstOrDefault();
                    //如果有优惠表中的商品则判断面向对象，是否会员专享
                    if (YhInfo != null)
                    {
                        //判断活动时间
                        if (System.DateTime.Now > YhInfo.sendtime) continue;
                        //查询会员等级,如果为空则不是会员消费
                        var vipLV = db.hd_vip_info.AsNoTracking().Where(t => t.vipcard == VipID.ToString()).Select(t => t.viptype).FirstOrDefault();

                        //特定对象
                        if (YhInfo.dx_type == 0 || YhInfo.dx_type == 1)
                        {
                            //再判断商品的优惠类型
                            switch (YhInfo.vtype)
                            {
                                case 1:
                                    //优惠活动
                                    break;
                                case 2:
                                    //零售特价（不限购）
                                    #region 特价无限量(这是单一商品降价)

                                    //判断是否满足特价条件,不会自动添加捆绑的商品，只当两种商品同时出现时享受特价
                                    //if (YhInfo.tm == textBox1.Text.Trim())
                                    //{
                                    if (YhInfo.dx_type == 1)  //限定会员
                                    {
                                        int viplvInt = vipLV.HasValue ? (int)vipLV.Value : 0;
                                        int viplvInfo = YhInfo.viptype.HasValue ? YhInfo.viptype.Value : 0;
                                        //如果会员等级条件满足
                                        if (viplvInt >= viplvInfo)
                                        {
                                            //购物车已经存在捆绑商品就修改组合活动的特价
                                            var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.item_id).FirstOrDefault();
                                            if (zsitem != null)
                                            {
                                                zsitem.hyPrice = YhInfo.ls_price;  //捆绑商品的特价
                                                //goodsBuyList[i].hyPrice = YhInfo.ls_price;  //组合商品的特价
                                            }
                                        }
                                    }
                                    else if (YhInfo.dx_type == 0)   //所有对象
                                    {
                                        //已经存在就数量++
                                        var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.item_id).FirstOrDefault();
                                        if (zsitem != null)
                                        {
                                            zsitem.hyPrice = YhInfo.ls_price;  //捆绑商品的特价
                                            zsitem.lsPrice = YhInfo.ls_price;  //捆绑商品的特价
                                            //goodsBuyList[i].hyPrice = YhInfo.ls_price;  //组合商品的特价
                                        }
                                    }
                                    //}
                                    //dataGridView_Cashiers.InvalidateRow(i);  //强制刷新行数据
                                    #endregion
                                    break;
                                case 3:
                                    #region 买1送1
                                    //买1送1
                                    //判断是否满足赠送条件(有活动商品就送)
                                    //if (YhInfo.tm == textBox1.Text.Trim())
                                    //{

                                    string temptxt_ = textBox1.Text.Trim();
                                    int itemid_temp_ = -1;
                                    int.TryParse(temptxt_, out itemid_temp_);
                                    if (goodsBuyList[i].noCode == itemid_temp_ || goodsBuyList[i].barCodeTM == temptxt_)
                                    {
                                        goodsBuyList[i].isZS = false;
                                    }

                                    if (goodsBuyList[i].isZS) continue;   //这个条件是防止重复判断活动的

                                    if (YhInfo.dx_type == 1)  //限定会员
                                    {
                                        int viplvInt = vipLV.HasValue ? (int)vipLV.Value : 0;
                                        int viplvInfo = YhInfo.viptype.HasValue ? YhInfo.viptype.Value : 0;
                                        //如果会员等级条件满足
                                        if (viplvInt >= viplvInfo)
                                        {
                                            //已经存在就数量++
                                            var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.zs_item_id).FirstOrDefault();
                                            if (zsitem != null)
                                            {
                                                int temp_ = Convert.ToInt32(YhInfo.zs_amount);
                                                if (temp_ == 0) temp_ = 1;
                                                //MessageBox.Show("你点击了确定");
                                                zsitem.countNum += temp_;
                                                zsitem.hyPrice = 0;
                                                zsitem.goodsDes = YhInfo.memo;
                                                zsitem.isZS = true;

                                                goodsBuyList[i].isZS = true;
                                            }
                                            else
                                            {
                                                //赠送的商品
                                                var ZsGoods = new GoodsBuy
                                                {
                                                    barCodeTM = YhInfo.zstm,
                                                    noCode = YhInfo.zs_item_id,
                                                    goods = YhInfo.zs_cname,
                                                    countNum = Convert.ToInt32(YhInfo.zs_amount),
                                                    jjPrice = YhInfo.yjj_price,
                                                    lsPrice = YhInfo.yls_price,             
                                                    hyPrice = 0,
                                                    goodsDes = YhInfo.memo,
                                                    isZS =true
                                                };
                                                goodsBuyList.Add(ZsGoods);
                                            }
                                            goodsBuyList[i].hyPrice = YhInfo.ls_price;
                                            goodsBuyList[i].isZS = true;
                                        }
                                    }
                                    else if (YhInfo.dx_type == 0)   //所有对象
                                    {
                                        //已经存在就数量++
                                        var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.zs_item_id).FirstOrDefault();
                                        if (zsitem != null)
                                        {
                                            int temp_ = Convert.ToInt32(YhInfo.zs_amount);
                                            if (temp_ == 0) temp_ = 1;
                                            //MessageBox.Show("你点击了确定");
                                            zsitem.countNum += temp_;
                                            zsitem.hyPrice = 0;
                                            zsitem.goodsDes = YhInfo.memo;
                                            zsitem.isZS = true;

                                            goodsBuyList[i].isZS = true;
                                        }
                                        else
                                        {
                                            //赠送的商品
                                            var ZsGoods = new GoodsBuy
                                            {
                                                barCodeTM = YhInfo.zstm,
                                                noCode = YhInfo.zs_item_id,
                                                goods = YhInfo.zs_cname,
                                                countNum = Convert.ToInt32(YhInfo.zs_amount),
                                                jjPrice = YhInfo.yjj_price,
                                                lsPrice = 0,
                                                hyPrice = 0,
                                                goodsDes = YhInfo.memo,
                                                isZS = true
                                            };
                                            goodsBuyList.Add(ZsGoods);
                                        }
                                        goodsBuyList[i].hyPrice = YhInfo.ls_price;
                                        goodsBuyList[i].lsPrice = YhInfo.ls_price;
                                        goodsBuyList[i].isZS = true;
                                    }
                                    //}
                                    break;
                                    #endregion
                                case 4:
                                    #region 组合优惠,两种商品同时存在则产生优惠，同时修改为特价
                                    //组合优惠价
                                    //判断购物车中有没有捆绑商品
                                    //var ZHItem = goodsBuyList.Where(t => t.noCode == YhInfo.zs_item_id).FirstOrDefault();
                                    //if (ZHItem != null)
                                    //{
                                    //    ZHItem.lsPrice = YhInfo.yls_price;
                                    //    ZHItem.hyPrice = YhInfo.zs_yprice;
                                    //}
                                    ////同时活动商品的价格也要变动
                                    //var HDItem = goodsBuyList.Where(t => t.noCode == YhInfo.item_id).FirstOrDefault();
                                    //if (HDItem != null)
                                    //{
                                    //    HDItem.lsPrice = YhInfo.ls_price;
                                    //    HDItem.hyPrice = YhInfo.ls_price;
                                    //}
                                    ////dataGridView_Cashiers.Refresh();
                                    // dataGridView_Cashiers.InvalidateRow(i);  //强制刷新行数据

                                    if (YhInfo.dx_type == 1)  //限定会员
                                    {
                                        int viplvInt = vipLV.HasValue ? (int)vipLV.Value : 0;
                                        int viplvInfo = YhInfo.viptype.HasValue ? YhInfo.viptype.Value : 0;
                                        //如果会员等级条件满足
                                        if (viplvInt >= viplvInfo)
                                        {
                                            //购物车已经存在捆绑商品就修改组合活动的特价
                                            var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.zs_item_id).FirstOrDefault();
                                            if (zsitem != null)
                                            {
                                                //限购数量
                                                if (zsitem.countNum <= YhInfo.xg_amount)
                                                {
                                                    zsitem.hyPrice = YhInfo.ls_price;  //捆绑商品的特价
                                                    goodsBuyList[i].hyPrice = YhInfo.ls_price;  //组合商品的特价
                                                }

                                            }
                                        }
                                    }
                                    else if (YhInfo.dx_type == 0)   //所有对象
                                    {
                                        //已经存在就数量++
                                        var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.zs_item_id).FirstOrDefault();
                                        if (zsitem != null)
                                        {
                                            //限购数量
                                            if (zsitem.countNum <= YhInfo.xg_amount)
                                            {
                                                zsitem.hyPrice = YhInfo.ls_price;  //捆绑商品的特价
                                                zsitem.lsPrice = YhInfo.ls_price;  //都是特价
                                                goodsBuyList[i].hyPrice = YhInfo.ls_price;  //组合商品的特价
                                                goodsBuyList[i].lsPrice = YhInfo.ls_price;
                                            }
                                        }
                                    }
                                    //dataGridView_Cashiers.InvalidateRow(i);  //强制刷新行数据
                                    break;

                                    #endregion
                                case 5:
                                    if (goodsBuyList[i].isZS) continue;   //这个条件是防止重复判断活动的
                                    #region 主要判断合计金额是否满额就行，不要判断买了什么

                                    if (YhInfo.dx_type == 1)  //限定会员
                                    {
                                        int viplvInt = vipLV.HasValue ? (int)vipLV.Value : 0;
                                        int viplvInfo = YhInfo.viptype.HasValue ? YhInfo.viptype.Value : 0;
                                        //如果会员等级条件满足
                                        if (viplvInt >= viplvInfo)
                                        {
                                            decimal sum_temp = totalMoney.HasValue ? totalMoney.Value : 0;  //目前总额
                                            //活动商品列表
                                            var YHInfoList = db.v_yh_detail.AsNoTracking().Where(t => t.item_id == itemid || t.tm == itemTM).ToList();
                                            if (YHInfoList.Count == 1)
                                            {
                                                //是否满额条件
                                                if (sum_temp >= YhInfo.zjmoney)
                                                {
                                                    //已经存在就数量++
                                                    var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.zs_item_id).FirstOrDefault();
                                                    if (zsitem != null)
                                                    {
                                                        int temp_ = Convert.ToInt32(YhInfo.zs_amount);
                                                        if (temp_ == 0) temp_ = 1;
                                                        if (DialogResult.OK == MessageBox.Show("此单 " + goodsBuyList[i].goods + " 满足满额加价赠送活动，是否确认修正商品价格？（" + YhInfo.zsmoney.ToString() + "元*"+temp_.ToString()+"）", "活动提醒", MessageBoxButtons.OKCancel))
                                                        {
                                                            if (temp_ != 1)
                                                            {
                                                                zsitem.countNum += temp_;
                                                            }
                                                            zsitem.hyPrice = YhInfo.zsmoney;
                                                            zsitem.goodsDes = YhInfo.memo;
                                                            //zsitem.isZS = true;
                                                        }
                                                        goodsBuyList[i].isZS = true;
                                                    }
                                                    else
                                                    {
                                                        int temp_count = Convert.ToInt32(YhInfo.zs_amount);
                                                        if (temp_count == 0) temp_count = 1;

                                                        //赠送的商品
                                                        var JJZsGoods = new GoodsBuy
                                                        {
                                                            barCodeTM = YhInfo.zstm,
                                                            noCode = YhInfo.zs_item_id,
                                                            goods = YhInfo.zs_cname,
                                                            countNum = temp_count,
                                                            jjPrice = YhInfo.yjj_price,
                                                            lsPrice = YhInfo.yls_price,
                                                            hyPrice = YhInfo.zsmoney,
                                                            goodsDes = YhInfo.memo,
                                                            isZS = true
                                                        };
                                                        if (DialogResult.OK == MessageBox.Show("此单 " + goodsBuyList[i].goods + " 满足满额加价赠送活动，是否自动添加赠送商品（" + YhInfo.zsmoney.ToString() + "元）", "活动提醒", MessageBoxButtons.OKCancel))
                                                        {
                                                            //MessageBox.Show("你点击了确定");
                                                            goodsBuyList.Add(JJZsGoods);

                                                        }
                                                    }
                                                }
                                            }
                                            //商品活动设置不止一个时
                                            if (YHInfoList.Count > 1)
                                            {
                                                if (DialogResult.OK == MessageBox.Show("此单 " + goodsBuyList[i].goods + " 满足满额加价赠送活动，是否选择商品？（" + YhInfo.zsmoney.ToString() + "元）", "活动提醒", MessageBoxButtons.OKCancel))
                                                {
                                                    //MessageBox.Show("你点击了确定");
                                                    var form1 = new ChoiceGoods();
                                                    //列出所有活动赠送商品
                                                    foreach (var item in YHInfoList)
                                                    {
                                                        int temp_count_ = Convert.ToInt32(item.zs_amount);
                                                        if (temp_count_ == 0) temp_count_ = 1;
                                                        goodsChooseList.Add(new GoodsBuy
                                                        {
                                                            barCodeTM = YhInfo.zstm,
                                                            noCode = YhInfo.zs_item_id,
                                                            goods = YhInfo.zs_cname,
                                                            countNum = temp_count_,
                                                            jjPrice = YhInfo.yjj_price,
                                                            lsPrice = YhInfo.yls_price,
                                                            hyPrice = YhInfo.zsmoney,
                                                            goodsDes = YhInfo.memo,
                                                            isZS = true
                                                        });

                                                        form1.dataGridView1.DataSource = goodsChooseList;
                                                        form1.ShowDialog();

                                                    }

                                                }
                                            }
                                        }
                                    }
                                    else if (YhInfo.dx_type == 0)   //所有对象
                                    {
                                        //活动商品列表
                                        var YHInfoList = db.v_yh_detail.AsNoTracking().Where(t => t.item_id == itemid).ToList();
                                        if (YHInfoList.Count == 1)
                                        {
                                            decimal sum_temp = totalMoney.HasValue ? totalMoney.Value : 0;  //目前总额
                                            if (sum_temp >= YhInfo.zjmoney)
                                            {
                                                //已经存在就数量++
                                                var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.zs_item_id).FirstOrDefault();
                                                if (zsitem != null)
                                                {
                                                    int temp_ = Convert.ToInt32(YhInfo.zs_amount);
                                                    if (temp_ == 0) temp_ = 1;
                                                    if (DialogResult.OK == MessageBox.Show("此单 " + goodsBuyList[i].goods + " 满足满额加价赠送活动，是否确认修正商品价格？（" + YhInfo.zsmoney.ToString() + "元*" + temp_.ToString() + "）", "活动提醒", MessageBoxButtons.OKCancel))
                                                    {
                                                        if (temp_ != 1)
                                                        {
                                                            zsitem.countNum += temp_;
                                                        }
                                                        zsitem.hyPrice = YhInfo.zsmoney;
                                                        zsitem.lsPrice = YhInfo.zsmoney;
                                                        zsitem.goodsDes = YhInfo.memo;
                                                        //zsitem.isZS = true;
                                                    }
                                                    goodsBuyList[i].isZS = true;
                                                }
                                                else
                                                {
                                                    int temp_count_2 = Convert.ToInt32(YhInfo.zs_amount);
                                                    if (temp_count_2 == 0) temp_count_2 = 1;
                                                    //赠送的商品
                                                    var JJZsGoods = new GoodsBuy
                                                    {
                                                        barCodeTM = YhInfo.zstm,
                                                        noCode = YhInfo.zs_item_id,
                                                        goods = YhInfo.zs_cname,
                                                        countNum = temp_count_2,
                                                        jjPrice = YhInfo.yjj_price,
                                                        lsPrice = YhInfo.zsmoney,
                                                        hyPrice = YhInfo.zsmoney,
                                                        goodsDes = YhInfo.memo,
                                                        isZS=true
                                                    };

                                                    if (DialogResult.OK == MessageBox.Show("此单 " + goodsBuyList[i].goods + " 满足满额加价赠送活动，是否自动添加赠送商品（" + YhInfo.zsmoney.ToString() + "元）", "活动提醒", MessageBoxButtons.OKCancel))
                                                    {
                                                        //MessageBox.Show("你点击了确定");
                                                        goodsBuyList.Add(JJZsGoods);

                                                    }
                                                }
                                            }
                                            //goodsBuyList[i].isZS = true;
                                        }
                                        //商品活动设置不止一个时
                                        if (YHInfoList.Count > 1)
                                        {
                                            if (DialogResult.OK == MessageBox.Show("此单 " + goodsBuyList[i].goods + " 满足满额加价赠送活动，是否选择商品？（" + YhInfo.zsmoney.ToString() + "元）", "活动提醒", MessageBoxButtons.OKCancel))
                                            {
                                                //MessageBox.Show("你点击了确定");
                                                var form1 = new ChoiceGoods();
                                                //列出所有活动赠送商品
                                                foreach (var item in YHInfoList)
                                                {
                                                    int temp_count_ = Convert.ToInt32(item.zs_amount);
                                                    if (temp_count_ == 0) temp_count_ = 1;

                                                    goodsChooseList.Add(new GoodsBuy
                                                    {
                                                        barCodeTM = YhInfo.zstm,
                                                        noCode = YhInfo.zs_item_id,
                                                        goods = YhInfo.zs_cname,
                                                        countNum = temp_count_,
                                                        jjPrice = YhInfo.yjj_price,
                                                        lsPrice = YhInfo.zsmoney,
                                                        hyPrice = YhInfo.zsmoney,
                                                        goodsDes = YhInfo.memo,
                                                           isZS=true
                                                    });

                                                    form1.dataGridView1.DataSource = goodsChooseList;
                                                    form1.ShowDialog();

                                                }

                                            }
                                        }
                                    }
                                    #endregion

                                    #region 调整为满额条件就选任一商品加价购 这个不要，我重写了一个

                                    //if (YhInfo.dx_type == 1)  //限定会员
                                    //{
                                    //    int viplvInt = vipLV.HasValue ? (int)vipLV.Value : 0;
                                    //    int viplvInfo = YhInfo.viptype.HasValue ? YhInfo.viptype.Value : 0;
                                    //    //如果会员等级条件满足
                                    //    if (viplvInt >= viplvInfo)
                                    //    {
                                    //        decimal sum_temp = totalMoney.HasValue ? totalMoney.Value : 0;  //目前总额
                                    //        //活动商品列表
                                    //        var YHInfoList = db.v_yh_detail.AsNoTracking().Where(t => t.item_id == itemid).ToList();
                                    //        if (YHInfoList.Count == 1)
                                    //        {
                                    //            //是否满额条件
                                    //            if (sum_temp >= YhInfo.zjmoney)
                                    //            {
                                    //                //已经存在就数量++
                                    //                var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.zs_item_id).FirstOrDefault();
                                    //                if (zsitem != null)
                                    //                {
                                    //                    if (DialogResult.OK == MessageBox.Show("此单 " + goodsBuyList[i].goods + " 满足满额加价赠送活动，是否自动添加赠送商品（" + YhInfo.zsmoney.ToString() + "元）", "活动提醒", MessageBoxButtons.OKCancel))
                                    //                    {
                                    //                        int temp_ = Convert.ToInt32(YhInfo.zs_amount);
                                    //                        if (temp_ == 0) temp_ = 1;
                                    //                        //MessageBox.Show("你点击了确定");
                                    //                        zsitem.countNum += temp_;
                                    //                        zsitem.hyPrice = YhInfo.zsmoney;
                                    //                        zsitem.goodsDes = YhInfo.memo;

                                    //                    }

                                    //                }
                                    //                else
                                    //                {
                                    //                    int temp_count = Convert.ToInt32(YhInfo.zs_amount);
                                    //                    if (temp_count == 0) temp_count = 1;

                                    //                    //赠送的商品
                                    //                    var JJZsGoods = new GoodsBuy
                                    //                    {
                                    //                        barCodeTM = YhInfo.zstm,
                                    //                        noCode = YhInfo.zs_item_id,
                                    //                        goods = YhInfo.zs_cname,
                                    //                        countNum = temp_count,
                                    //                        jjPrice = YhInfo.yjj_price,
                                    //                        lsPrice = YhInfo.yls_price,
                                    //                        hyPrice = YhInfo.zsmoney,
                                    //                        goodsDes = YhInfo.memo
                                    //                    };
                                    //                    if (DialogResult.OK == MessageBox.Show("此单 " + goodsBuyList[i].goods + " 满足满额加价赠送活动，是否自动添加赠送商品（" + YhInfo.zsmoney.ToString() + "元）", "活动提醒", MessageBoxButtons.OKCancel))
                                    //                    {
                                    //                        //MessageBox.Show("你点击了确定");
                                    //                        goodsBuyList.Add(JJZsGoods);

                                    //                    }
                                    //                }
                                    //            }
                                    //        }
                                    //        //商品活动设置不止一个时
                                    //        if (YHInfoList.Count > 1)
                                    //        {
                                    //            if (DialogResult.OK == MessageBox.Show("此单 " + goodsBuyList[i].goods + " 满足满额加价赠送活动，是否选择商品？（" + YhInfo.zsmoney.ToString() + "元）", "活动提醒", MessageBoxButtons.OKCancel))
                                    //            {
                                    //                //MessageBox.Show("你点击了确定");
                                    //                var form1 = new ChoiceGoods();
                                    //                //列出所有活动赠送商品
                                    //                foreach (var item in YHInfoList)
                                    //                {
                                    //                    int temp_count_ = Convert.ToInt32(item.zs_amount);
                                    //                    if (temp_count_ == 0) temp_count_ = 1;
                                    //                    goodsChooseList.Add(new GoodsBuy
                                    //                    {
                                    //                        barCodeTM = YhInfo.zstm,
                                    //                        noCode = YhInfo.zs_item_id,
                                    //                        goods = YhInfo.zs_cname,
                                    //                        countNum = temp_count_,
                                    //                        jjPrice = YhInfo.yjj_price,
                                    //                        lsPrice = YhInfo.yls_price,
                                    //                        hyPrice = YhInfo.zsmoney,
                                    //                        goodsDes = YhInfo.memo
                                    //                    });

                                    //                    form1.dataGridView1.DataSource = goodsChooseList;
                                    //                    form1.ShowDialog();

                                    //                }

                                    //            }
                                    //        }
                                    //    }
                                    //}
                                    //else if (YhInfo.dx_type == 0)   //所有对象
                                    //{
                                    //    //活动商品列表
                                    //    var YHInfoList = db.v_yh_detail.AsNoTracking().Where(t => t.item_id == itemid).ToList();
                                    //    if (YHInfoList.Count == 1)
                                    //    {
                                    //        decimal sum_temp = totalMoney.HasValue ? totalMoney.Value : 0;  //目前总额
                                    //        if (sum_temp >= YhInfo.zjmoney)
                                    //        {
                                    //            //已经存在就数量++
                                    //            var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.zs_item_id).FirstOrDefault();
                                    //            if (zsitem != null)
                                    //            {
                                    //                if (DialogResult.OK == MessageBox.Show("此单 " + goodsBuyList[i].goods + " 满足满额加价赠送活动，是否自动添加赠送商品（" + YhInfo.zsmoney.ToString() + "元）", "活动提醒", MessageBoxButtons.OKCancel))
                                    //                {
                                    //                    int temp_ = Convert.ToInt32(YhInfo.zs_amount);
                                    //                    if (temp_ == 0) temp_ = 1;
                                    //                    //MessageBox.Show("你点击了确定");
                                    //                    zsitem.countNum += temp_;
                                    //                    zsitem.hyPrice = YhInfo.zsmoney;
                                    //                    zsitem.lsPrice = YhInfo.zsmoney;
                                    //                    zsitem.goodsDes = YhInfo.memo;
                                    //                }

                                    //            }
                                    //            else
                                    //            {
                                    //                int temp_count_2 = Convert.ToInt32(YhInfo.zs_amount);
                                    //                if (temp_count_2 == 0) temp_count_2 = 1;
                                    //                //赠送的商品
                                    //                var JJZsGoods = new GoodsBuy
                                    //                {
                                    //                    barCodeTM = YhInfo.zstm,
                                    //                    noCode = YhInfo.zs_item_id,
                                    //                    goods = YhInfo.zs_cname,
                                    //                    countNum = temp_count_2,
                                    //                    jjPrice = YhInfo.yjj_price,
                                    //                    lsPrice = YhInfo.zsmoney,
                                    //                    hyPrice = YhInfo.zsmoney,
                                    //                    goodsDes = YhInfo.memo
                                    //                };

                                    //                if (DialogResult.OK == MessageBox.Show("此单 " + goodsBuyList[i].goods + " 满足满额加价赠送活动，是否自动添加赠送商品（" + YhInfo.zsmoney.ToString() + "元）", "活动提醒", MessageBoxButtons.OKCancel))
                                    //                {
                                    //                    //MessageBox.Show("你点击了确定");
                                    //                    goodsBuyList.Add(JJZsGoods);

                                    //                }
                                    //            }
                                    //        }
                                    //    }
                                    //    //商品活动设置不止一个时
                                    //    if (YHInfoList.Count > 1)
                                    //    {
                                    //        if (DialogResult.OK == MessageBox.Show("此单 " + goodsBuyList[i].goods + " 满足满额加价赠送活动，是否选择商品？（" + YhInfo.zsmoney.ToString() + "元）", "活动提醒", MessageBoxButtons.OKCancel))
                                    //        {
                                    //            //MessageBox.Show("你点击了确定");
                                    //            var form1 = new ChoiceGoods();
                                    //            //列出所有活动赠送商品
                                    //            foreach (var item in YHInfoList)
                                    //            {
                                    //                int temp_count_ = Convert.ToInt32(item.zs_amount);
                                    //                if (temp_count_ == 0) temp_count_ = 1;
                           
                                    //                goodsChooseList.Add(new GoodsBuy
                                    //                {
                                    //                    barCodeTM = YhInfo.zstm,
                                    //                    noCode = YhInfo.zs_item_id,
                                    //                    goods = YhInfo.zs_cname,
                                    //                    countNum = temp_count_,
                                    //                    jjPrice = YhInfo.yjj_price,
                                    //                    lsPrice = YhInfo.zsmoney,
                                    //                    hyPrice = YhInfo.zsmoney,
                                    //                    goodsDes = YhInfo.memo
                                    //                });

                                    //                form1.dataGridView1.DataSource = goodsChooseList;
                                    //                form1.ShowDialog();

                                    //            }

                                    //        }
                                    //    }
                                    //}
                                    #endregion

                                    #region 活动商品满100元+1元赠送（这个不要了）
                                    ////满100元加1元赠送商品(加价购)
                                    //if (YhInfo.dx_type == 1)  //限定会员
                                    //{
                                    //    int viplvInt = vipLV.HasValue ? (int)vipLV.Value : 0;
                                    //    int viplvInfo = YhInfo.viptype.HasValue ? YhInfo.viptype.Value : 0;
                                    //    //如果会员等级条件满足
                                    //    if (viplvInt >= viplvInfo)
                                    //    {
                                    //        decimal sum_temp = totalMoney.HasValue ? totalMoney.Value : 0;  //目前总额
                                    //        //是否满额条件
                                    //        if (sum_temp >= YhInfo.zjmoney)
                                    //        {
                                    //            //已经存在就数量++
                                    //            var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.zs_item_id).FirstOrDefault();
                                    //            if (zsitem != null)
                                    //            {
                                    //                if (DialogResult.OK == MessageBox.Show("此单满足满100加1赠送活动，是否自动添加赠送商品（1元）", "活动提醒", MessageBoxButtons.OKCancel))
                                    //                {
                                    //                    //MessageBox.Show("你点击了确定");
                                    //                    zsitem.countNum++;
                                    //                    zsitem.hyPrice = 1.00m;
                                    //                    zsitem.goodsDes = YhInfo.memo;

                                    //                }

                                    //            }
                                    //            else
                                    //            {
                                    //                //赠送的商品
                                    //                var JJZsGoods = new GoodsBuy
                                    //                {
                                    //                    barCodeTM = YhInfo.zstm,
                                    //                    noCode = YhInfo.zs_item_id,
                                    //                    goods = YhInfo.zs_cname,
                                    //                    countNum = Convert.ToInt32(YhInfo.zs_amount),
                                    //                    jjPrice = YhInfo.yjj_price,
                                    //                    lsPrice = YhInfo.yls_price,
                                    //                    //hyPrice = YhInfo.zs_yprice, //这个价吧，赠送商品的价格为1元
                                    //                    hyPrice = 1.00m,
                                    //                    goodsDes = YhInfo.memo
                                    //                };
                                    //                if (DialogResult.OK == MessageBox.Show("此单满足满100加1赠送活动，是否自动添加赠送商品（1元）", "活动提醒", MessageBoxButtons.OKCancel))
                                    //                {
                                    //                    //MessageBox.Show("你点击了确定");
                                    //                    goodsBuyList.Add(JJZsGoods);

                                    //                }
                                    //            }
                                    //        }

                                    //    }
                                    //}
                                    //else if (YhInfo.dx_type == 0)   //所有对象
                                    //{
                                    //    decimal sum_temp = totalMoney.HasValue ? totalMoney.Value : 0;  //目前总额
                                    //    if (sum_temp >= 100)
                                    //    {
                                    //        //已经存在就数量++
                                    //        var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.zs_item_id).FirstOrDefault();
                                    //        if (zsitem != null)
                                    //        {
                                    //            if (DialogResult.OK == MessageBox.Show("此单满足满100加1赠送活动，是否自动添加赠送商品（1元）", "活动提醒", MessageBoxButtons.OKCancel))
                                    //            {
                                    //                //MessageBox.Show("你点击了确定");
                                    //                zsitem.countNum++;
                                    //                zsitem.hyPrice = 1.00m;
                                    //                zsitem.lsPrice = 1.00m;
                                    //                zsitem.goodsDes = YhInfo.memo;

                                    //            }

                                    //        }
                                    //        else
                                    //        {
                                    //            //赠送的商品
                                    //            var JJZsGoods = new GoodsBuy
                                    //            {
                                    //                barCodeTM = YhInfo.zstm,
                                    //                noCode = YhInfo.zs_item_id,
                                    //                goods = YhInfo.zs_cname,
                                    //                countNum = Convert.ToInt32(YhInfo.zs_amount),
                                    //                jjPrice = YhInfo.yjj_price,
                                    //                //lsPrice = YhInfo.yls_price,
                                    //                //hyPrice = YhInfo.zs_yprice, //这个价吧，赠送商品的价格为1元
                                    //                lsPrice = 1.00m,
                                    //                hyPrice = 1.00m,
                                    //                goodsDes = YhInfo.memo
                                    //            };

                                    //            if (DialogResult.OK == MessageBox.Show("此单满足满100加1赠送活动，是否自动添加赠送商品（1元）", "活动提醒", MessageBoxButtons.OKCancel))
                                    //            {
                                    //                //MessageBox.Show("你点击了确定");
                                    //                goodsBuyList.Add(JJZsGoods);

                                    //            }
                                    //            //else
                                    //            //{
                                    //            //    MessageBox.Show("你点击了取消");
                                    //            //}
                                    //        }
                                    //    }
                                    //}
                                    ////}
                                    ////dataGridView_Cashiers.InvalidateRow(i);  //强制刷新行数据
                                    #endregion
                                    break;

                                case 6:
                                    #region 时段特价，这是单一商品降价
                                    //按时段特价
                                    if (System.DateTime.Now < YhInfo.sendtime)
                                    {
                                        //判断是否满足特价条件,不会自动添加捆绑的商品，只当两种商品同时出现时享受特价
                                        //if (YhInfo.tm == textBox1.Text.Trim())
                                        //{
                                        if (YhInfo.dx_type == 1)  //限定会员
                                        {
                                            int viplvInt = vipLV.HasValue ? (int)vipLV.Value : 0;
                                            int viplvInfo = YhInfo.viptype.HasValue ? YhInfo.viptype.Value : 0;
                                            //如果会员等级条件满足
                                            if (viplvInt >= viplvInfo)
                                            {
                                                //购物车已经存在捆绑商品就修改组合活动的特价
                                                var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.item_id).FirstOrDefault();
                                                if (zsitem != null)
                                                {
                                                    zsitem.hyPrice = YhInfo.ls_price;  //捆绑商品的特价
                                                    //goodsBuyList[i].hyPrice = YhInfo.ls_price;  //组合商品的特价
                                                }
                                            }
                                        }
                                        else if (YhInfo.dx_type == 0)   //所有对象
                                        {
                                            //已经存在就数量++
                                            var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.item_id).FirstOrDefault();
                                            if (zsitem != null)
                                            {
                                                zsitem.hyPrice = YhInfo.ls_price;  //捆绑商品的特价
                                                zsitem.lsPrice = YhInfo.ls_price;  //都是特价

                                                //goodsBuyList[i].hyPrice = YhInfo.ls_price;  //组合商品的特价
                                            }
                                        }
                                        //}
                                        //dataGridView_Cashiers.InvalidateRow(i);  //强制刷新行数据
                                    }


                                    break;

                                    #endregion
                                case 7:
                                    #region 零售特价（限购，这是单一商品降价）
                                    //零售特价（限购）
                                    //var XGTJGoods = new GoodsBuy
                                    //{
                                    //    noCode = YhInfo.zs_item_id,
                                    //    goods = YhInfo.zs_cname,
                                    //    countNum = Convert.ToInt32(YhInfo.zs_amount),
                                    //    jjPrice = YhInfo.yjj_price,
                                    //    lsPrice = YhInfo.yls_price,
                                    //    hyPrice = YhInfo.zs_yprice,
                                    //    goodsDes = YhInfo.memo
                                    //};
                                    ////判断是否满足赠送条件(限购)
                                    //bool isXGTJed = false;
                                    //foreach (var item in goodsBuyList)
                                    //{
                                    //    //防止重复赠送
                                    //    if (item.noCode == YhInfo.zs_item_id && item.goodsDes == YhInfo.memo)
                                    //    {
                                    //        //数量限制
                                    //        if (item.countNum >= YhInfo.xg_amount)
                                    //        {
                                    //            isXGTJed = true; //已经赠送过
                                    //        }
                                    //    }
                                    //}
                                    //if (isXGTJed == false)
                                    //{
                                    //    //已经存在就数量++
                                    //    var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.zs_item_id).FirstOrDefault();
                                    //    if (zsitem != null)
                                    //    {
                                    //        zsitem.countNum++;
                                    //    }
                                    //    else
                                    //    {
                                    //        goodsBuyList.Add(XGTJGoods);
                                    //    }
                                    //}

                                    if (YhInfo.dx_type == 1)  //限定会员
                                    {
                                        int viplvInt = vipLV.HasValue ? (int)vipLV.Value : 0;
                                        int viplvInfo = YhInfo.viptype.HasValue ? YhInfo.viptype.Value : 0;
                                        //如果会员等级条件满足
                                        if (viplvInt >= viplvInfo)
                                        {
                                            //购物车已经存在捆绑商品就修改组合活动的特价
                                            var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.item_id).FirstOrDefault();
                                            if (zsitem != null)
                                            {
                                                //限购数量
                                                if (zsitem.countNum <= YhInfo.xg_amount)
                                                {
                                                    zsitem.hyPrice = YhInfo.ls_price;  //捆绑商品的特价
                                                    //goodsBuyList[i].hyPrice = YhInfo.ls_price;  //组合商品的特价
                                                }

                                            }
                                        }
                                    }
                                    else if (YhInfo.dx_type == 0)   //所有对象
                                    {
                                        //已经存在就数量++
                                        var zsitem = goodsBuyList.Where(t => t.noCode == YhInfo.item_id).FirstOrDefault();
                                        if (zsitem != null)
                                        {
                                            //限购数量
                                            if (zsitem.countNum <= YhInfo.xg_amount)
                                            {
                                                zsitem.hyPrice = YhInfo.ls_price;  //捆绑商品的特价
                                                zsitem.lsPrice = YhInfo.ls_price;   //都是特价
                                                //goodsBuyList[i].hyPrice = YhInfo.ls_price;  //组合商品的特价
                                                //MessageBox.Show(zsitem.goods + "  已达最大限购数量！");
                                            }
                                        }
                                    }
                                    //dataGridView_Cashiers.InvalidateRow(i);  //强制刷新行数据
                                    break;

                                    #endregion
                            }
                        }
                    }
                    dataGridView_Cashiers.InvalidateRow(i);  //强制刷新行数据
                }
            //}
            //catch (Exception ex)
            //{

            //    LogHelper.WriteLog("收银主界面处理优惠活动时发生异常:", ex);
            //    MessageBox.Show("优惠活动处理出错！");

            //}


                #endregion

        }

        //处理当会员登记后及时刷新活动调整后的UI
        public void HDUIFunc()
        {

            try
            {

                decimal? temp_r = 0;
                for (int i = 0; i < goodsBuyList.Count; i++)
                {
                    temp_r += (goodsBuyList[i].hyPrice * goodsBuyList[i].countNum);
                    dataGridView_Cashiers.InvalidateRow(i);  //强制刷新行数据
                }

                label81.Text = temp_r.ToString() + "  元";  //合计金额
                totalMoney = temp_r;

                label3.Visible = true;  //你有新消息……
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("收银主界面下方UI显示异常:", e);
            }

        }

        //条码文本输入框按键事件
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        //窗体在屏幕居中
        private void Cashiers_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.DarkOliveGreen, 0, 0, this.Width - 1, this.Height - 1);
        }
        //网格内容点击事件（没用到）
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        //清除数据显示的空格
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value is string)
                e.Value = e.Value.ToString().Trim();
        }


        //窗体激活事件
        private void Cashiers_Activated(object sender, EventArgs e)
        {

            if (!textBox1.Focused)
            {
                textBox1.Focus();
                textBox1.SelectAll();
            }

            if (label103.Text == "未登记")
            {
                Tipslabel.Text = "营业员还未登记，请按F4键录入";
            }

            if (label23.Text == "未登记")
            {
                Tipslabel.Text = "如果是会员消费，请按F12键录入";
            }

            if (label103.Text == "未登记" && label103.Text == "未登记")
            {
                Tipslabel.Text = "请按F4键录入营业员。如果是会员消费，请按F12键录入";
            }

            //ColumnWidthFunc();

        }


        //判断用户是否修改单元格
        private void dataGridView_Cashiers_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {


        }


        //当用户开始编辑数据网格时，保存修改前的值，方便返回操作
        private void dataGridView_Cashiers_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {

        }


        //当用户直接在UI上修改数据完毕时
        private void dataGridView_Cashiers_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

            dataGridView_Cashiers.Refresh();

            ShowDown();

            textBox1.Focus();  //焦点回到条码输入框
        }

        //datagridview单元格修改值提交时验证数据是否符合要求
        private void dataGridView_Cashiers_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            //验证第5列，数量
            if (e.ColumnIndex == 5)
            {
                int temp_int = 0;
                if (int.TryParse(e.FormattedValue.ToString(), out temp_int))
                {
                    e.Cancel = false;
                }
                else
                {
                    e.Cancel = true;
                    dataGridView_Cashiers.CancelEdit();
                    //MessageBox.Show("数量请输入整数");
                    tipForm.Tiplabel.Text = "数量请输入整数!";
                    tipForm.ShowDialog();
                }




            }
        }

        //显示UI，购物车商品总额，总数量
        public void ShowDown()
        {
            if (dataGridView_Cashiers.Rows.Count > 0)
            {
                try
                {
                    decimal? temp_r = 0;
                    int temp_c = 0;
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
                }
            }
            else
            {
                label81.Text = "";
                label82.Text = "";
            }
        }


        //当datagridview行选中时触发事件,显示当前选中的商品名字，商品零售价
        private void dataGridView_Cashiers_SelectionChanged(object sender, EventArgs e)
        {

            if (dataGridView_Cashiers.Rows.Count > 0)
            {
                try
                {
                    label84.Text = dataGridView_Cashiers.SelectedRows[0].Cells[3].Value.ToString();
                    label83.Text = dataGridView_Cashiers.SelectedRows[0].Cells[9].Value.ToString() + "  元";
                    label31.Text = dataGridView_Cashiers.SelectedRows[0].Cells[17].Value.ToString() + "  折";

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

        #region 热键注册

        //普通快捷键设置
        private void Cashiers_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                //F1 提款
                case Keys.F1:
                    TiKuanForm tikuanFrom = new TiKuanForm();
                    tikuanFrom.ShowDialog();
                    break;

                //F4键登记业务员
                case Keys.F4:
                    SMForm.ShowDialog();

                    break;
                //F5键重打小票
                case Keys.F5:
                    if (lastGoodsList.Count > 0)
                    {
                        printer.StartPrint(lastGoodsList);
                    }

                    break;
                //最小化
                case Keys.Pause:
                    this.WindowState = FormWindowState.Minimized;
                    //mainForm.Hide();
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
                case Keys.F2:
                    ItemInfoForm iiform = new ItemInfoForm();
                    iiform.ShowDialog();
                    break;
                //退货
                case Keys.F9:
                    Refund();
                    break;
                //整单打折
                case Keys.F10:
                    ZKZDForm zkzdform = new ZKZDForm();
                    zkzdform.ShowDialog();
                    if (ZKZD != null)
                    {
                        ZKZDFunc();
                    }
                    break;
                //单品打折
                case Keys.F11:
                    ZKForm zkform = new ZKForm();
                    zkform.ShowDialog();
                    if (ZKDP_temp != null)
                    {
                        ZKDPFunc();
                    }
                    break;

                //打开会员卡窗口
                case Keys.F12:
                    vipForm.ShowDialog();
                    break;


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
                VipMemoForm vipmemo = new VipMemoForm();
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
                    //删除DEL键
                    case Keys.Delete:

                        Dele();

                        break;

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
                        //ColumnWidthFunc(); //列宽
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
                //如果当前只有一行就直接清空
                if (dataGridView_Cashiers.Rows.Count == 1)
                {
                    int DELindex1_temp = dataGridView_Cashiers.SelectedRows[0].Index;
                    dataGridView_Cashiers.Rows.RemoveAt(DELindex1_temp);

                }
                //当前行数大于1行时删除选中行后把往上一行设置为选中状态
                if (dataGridView_Cashiers.Rows.Count > 1)
                {
                    int DELindex_temp = dataGridView_Cashiers.SelectedRows[0].Index;
                    dataGridView_Cashiers.Rows.RemoveAt(DELindex_temp);

                    string de_temp = dataGridView_Cashiers.CurrentRow.Cells[2].Value.ToString();

                    if (DELindex_temp - 1 >= 0)
                    {
                        dataGridView_Cashiers.Rows[DELindex_temp - 1].Selected = true;
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
            if (dataGridView_Cashiers.Rows.Count > 0)
            {
                goodsBuyList.Clear();
            }

        }


        //回车功能
        private void EnterFun()
        {
            //如果输入框为空且购物车有商品时，则弹出结算窗口
            if (string.IsNullOrEmpty(textBox1.Text) && goodsBuyList.Count > 0 && !dataGridView_Cashiers.IsCurrentCellInEditMode)
            {
                foreach (var item in goodsBuyList)
                {
                    lastGoodsList.Add(item);
                }
                CEform.ShowDialog();
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
                    label3.Visible = false;  //你有新消息……

                    this.VipID = 0;  //把会员消费重置为普通消费
                    ZKZD = null;  //清除折扣
                    this.label99.Text = "未登记";
                    this.label101.Text = "按F12登记会员";
                    this.tableLayoutPanel2.Visible = false;  //隐藏结算结果
                    isNewItem = false;
                    timer_temp = 0;
                }


            }
        }


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

                noteList.Add(new GoodsNoteModel { noNote = OrderNo, upDate = date_temp, cashier = HandoverModel.GetInstance.YWYStr, totalM = totalMoney });

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
                GNform.ShowDialog();
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
                //mainForm.Show();
                //this.Hide();

                isLianXi = false;  //退出练习
                label25.Visible = false;
                ZKZD = null;
                ZKDP_temp = null;
                totalMoney = null;
                isNewItem = false;
                VipMdemo = string.Empty;
                label3.Visible = false;  //你有新消息……

                this.tableLayoutPanel2.Visible = false;  //隐藏结算结果
                timer_temp = 0;

                this.VipID = 0;  //把会员消费重置为普通消费
                this.label101.Text = "按F12登记会员";
                this.label99.Text = "未登记";

                mainForm.Show();
                this.Close();
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

            MPForm.ShowDialog();

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



        #endregion

        #region 自动在数据表格首列绘制序号


        //表格绘制事件
        private void dataGridView_Cashiers_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SetDataGridViewRowXh(e, dataGridView_Cashiers);
        }
        //在首列绘制序号，如果首列原有内容，会出现重叠，所以首列手动添加一个空列
        private void SetDataGridViewRowXh(DataGridViewRowPostPaintEventArgs e, DataGridView dataGridView)
        {
            SolidBrush solidBrush = new SolidBrush(Color.White); //更改序号样式
            int xh = e.RowIndex + 1;
            e.Graphics.DrawString(xh.ToString(CultureInfo.CurrentUICulture), e.InheritedRowStyle.Font, solidBrush, e.RowBounds.Location.X + 5, e.RowBounds.Location.Y + 4);
        }
        #endregion



        //锁定窗口焦点始终在条码输入框上(目前与数据直接修改功能冲突)
        private void textBox1_Leave(object sender, EventArgs e)
        {
            //textBox1.Focus();
        }


        //用户从商品选择窗口选中的商品,如果购物车已存在该商品则数量加1，否则新增
        public void UserChooseGoods(int index)
        {
            try
            {
                if (goodsBuyList.Any(k => k.noCode == goodsChooseList[index].noCode))
                {
                    var se = goodsBuyList.Where(h => h.noCode == goodsChooseList[index].noCode).FirstOrDefault();

                    se.countNum++;
                    dataGridView_Cashiers.Refresh();
                    ShowDown();
                }
                else
                {

                    goodsBuyList.Add(goodsChooseList[index]);
                    //需要再判断当前购物车是否满足优惠活动条件
                    using (var db = new hjnbhEntities())
                    {
                        XSHDFunc(db);  //处理促销
                        YHHDFunc(db);  //处理优惠
                    }


                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("收银主界面重复商品数量自增时发生异常:", ex);
            }
        }

        //每当购物车内有商品列表增减时刷新UI
        private void dataGridView_Cashiers_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            ShowDown();
            UpdateNameFunc();
        }


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
                dataGridView_Cashiers.Columns[15].Visible = false;
                dataGridView_Cashiers.Columns[16].Visible = false;
                dataGridView_Cashiers.Columns[17].Visible = false; //折扣
                dataGridView_Cashiers.Columns[18].Visible = false; //批发价
                dataGridView_Cashiers.Columns[19].Visible = false; //活动商品标志

                //列宽
                dataGridView_Cashiers.Columns[0].Width = 30;
                dataGridView_Cashiers.Columns[1].Width = 80;
                dataGridView_Cashiers.Columns[2].Width = 130;  //条码
                dataGridView_Cashiers.Columns[3].Width = 260;  //品名
                //单元格文字色
                dataGridView_Cashiers.Columns[13].DefaultCellStyle.ForeColor = Color.Red;  //备注列

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
        //显示上单、结单实收款与总金额
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
                HandoverModel.GetInstance.OrderCount++; //交易单数
                if (totalMoney.HasValue)
                {
                    HandoverModel.GetInstance.Money += totalMoney.Value; //应收金额
                }
                //上单合计
                this.label91.Visible = true;
                this.label91.Text = ClosingEntries.GetInstance.CETotalMoney + " 元";

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

        // 从外部调用退货窗口的方法
        public void ShowRDForm()
        {
            this.RDForm.ShowDialog();
        }

        #region 处理打折

        //处理单品折扣
        private void ZKDPFunc()
        {
            if (ZKDP_temp == 0) return;
            int itemid = 0;
            int index = 0;
            if (dataGridView_Cashiers.Rows.Count > 0)
            {
                index = dataGridView_Cashiers.SelectedRows[0].Index;
            }
            itemid = goodsBuyList[index].noCode;
            using (var db = new hjnbhEntities())
            {
                //能否打折 0 选中 1000不选中
                var zkinfo = db.v_hd_item_info.AsNoTracking().Where(t => t.item_id == itemid).Select(t => t.isale).FirstOrDefault();
                if (zkinfo == 0)
                {
                    goodsBuyList[index].hyPrice *= (ZKDP_temp.HasValue ? ZKDP_temp.Value / 100 : 1);
                    goodsBuyList[index].lsPrice *= (ZKDP_temp.HasValue ? ZKDP_temp.Value / 100 : 1);
                    goodsBuyList[index].goodsDes += "(" + (ZKDP_temp / 10).ToString() + "折" + ")";
                    goodsBuyList[index].ZKDP = (ZKDP_temp.HasValue ? ZKDP_temp.Value / 100 : 0);
                    dataGridView_Cashiers.InvalidateRow(index);
                    ZKDP_temp = null;
                }
                else
                {
                    MessageBox.Show("该商品不允许打折！");
                }

            }
        }


        //整单打折
        private void ZKZDFunc()
        {
            if (ZKZD == 0) return;
            if (goodsBuyList.Count == 0) return;
            for (int i = 0; i < goodsBuyList.Count; i++)
            {
                int itemid = goodsBuyList[i].noCode;
                using (var db = new hjnbhEntities())
                {
                    //能否打折 0 选中 1000不选中
                    var zkinfo = db.v_hd_item_info.AsNoTracking().Where(t => t.item_id == itemid).Select(t => t.isale).FirstOrDefault();
                    if (zkinfo == 0)
                    {
                        goodsBuyList[i].hyPrice *= (ZKZD.HasValue ? ZKZD.Value / 100 : 1);
                        goodsBuyList[i].lsPrice *= (ZKZD.HasValue ? ZKZD.Value / 100 : 1);
                        goodsBuyList[i].goodsDes += "(" + (ZKZD / 10).ToString() + "折" + ")";
                        goodsBuyList[i].ZKDP = (ZKZD / 100) + ((goodsBuyList[i].ZKDP.HasValue ? goodsBuyList[i].ZKDP.Value : 0) / 100);
                        dataGridView_Cashiers.InvalidateRow(i);
                    }
                    //else
                    //{
                    //    MessageBox.Show("已忽略商品列表中不允许打折的商品！");
                    //}

                }
            }
            label32.Text = (ZKZD / 10).ToString() + "折";  // 折扣UI
        }
        #endregion

        #region 显示会员图像
        public Image pic;
        private void ShowVipImaFunc()
        {
            if (string.IsNullOrEmpty(VipID.ToString()) || VipID == 0) return;
            try
            {
                using (var db = new hjnbhEntities())
                {
                    var ima = db.hd_vip_info.AsNoTracking().Where(t => t.vipcard == VipID.ToString()).Select(t => t.picture).FirstOrDefault();
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
            if (string.IsNullOrEmpty(VipID.ToString()) || VipID == 0) return;
            using (var db = new hjnbhEntities())
            {
                var ima = db.hd_vip_info.Where(t => t.vipcard == VipID.ToString()).Select(t => t.picture).FirstOrDefault();
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
                //WindowState = FormWindowState.Maximized;
                //this.FormBorderStyle = FormBorderStyle.None;
                //this.FormBorderStyle = FormBorderStyle.None;
                //this.MaximumSize = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
                
                this.Show();
                //this.WindowState = FormWindowState.Maximized;
                //激活窗体并给予它焦点
                this.Activate();
                //任务栏区显示图标
                this.ShowInTaskbar = true;
                //托盘区图标隐藏
                notifyIcon1.Visible = false;
            }
        }

        private void Cashiers_SizeChanged(object sender, EventArgs e)
        {
            //判断是否选择的是最小化按钮
            if (WindowState == FormWindowState.Minimized)
            {
                //隐藏任务栏区图标
                this.ShowInTaskbar = false;
                //图标显示在托盘区
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(500, "提示", "双击可以回复窗口", ToolTipIcon.Info);
            }
        }





    }
}
