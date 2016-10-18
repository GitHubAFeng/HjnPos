using Common;
using hjn20160520.Common;
using hjn20160520.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Forms;

namespace hjn20160520._2_Cashiers
{
    /// <summary>
    /// 退货窗口
    /// </summary>
    public partial class RefundForm : Form
    {
        CashiersFormXP CAForm;
        //已购商品列表
        private BindingList<TuiHuoItemModel> buyedList = new BindingList<TuiHuoItemModel>();
        //选择整单时候使用的缓存
        private BindingList<TuiHuoItemModel> buyedTempList = new BindingList<TuiHuoItemModel>();
        //需要退的商品列表 
        private BindingList<TuiHuoItemModel> tuihuoList = new BindingList<TuiHuoItemModel>();
        //用来查询购物车内是否有抵额退货
        public BindingList<GoodsBuy> goodsBuyList = new BindingList<GoodsBuy>();

        private decimal CurrentTuihuJF = 0.00m; //本次退货需要扣减的积分

        public delegate void RefundFormHandle(BindingList<TuiHuoItemModel> THlist);
        public event RefundFormHandle changed;  //传递退货商品到购物车


        public RefundForm()
        {
            InitializeComponent();
        }

        private void RefundForm_Load(object sender, EventArgs e)
        {
            CAForm = this.Owner as CashiersFormXP;
            this.goodsBuyList = CAForm.goodsBuyList;
            this.ActiveControl = textBox1;
            textBox1.Focus();
            textBox1.SelectAll();
            textBox7.Clear();

            buyedTempList.Clear();
            buyedList.Clear();
            tuihuoList.Clear();

            this.dataGridView1.DataSource = buyedList;
            this.dataGridView2.DataSource = tuihuoList;

            textBox4.Text = "1";
            CurrentTuihuJF = 0.00m; //本次退货需要扣减的积分

        }

        private void RefundForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.Close();
                    break;
                case Keys.Enter:

                    if (!string.IsNullOrEmpty(textBox7.Text.Trim()))
                    {
                        CXFunc();
                    }

                    break;


                case Keys.F2:
                    try
                    {
                        if (string.IsNullOrEmpty(textBox1.Text.Trim()))
                        {
                            textBox1.Focus();
                        }
                        else
                        {
                            FindBuyedItem();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLog("退货查询已购商品出现异常:", ex);

                        MessageBox.Show("查询已购商品出现异常，请联系管理员！");
                    }


                    break;

                case Keys.F4:
                    textBox7.Focus();

                    break;


                case Keys.F5:
                    textBox4.Focus();
                    break;

                case Keys.Delete:
                    DeleTui();
                    break;

                case Keys.Up:
                    UpFun();
                    break;

                case Keys.Down:
                    DownFun();
                    break;

                case Keys.Subtract:
                    UpdataCount();
                    break;

                case Keys.F6:
                    ChooseZD();
                    break;

                case Keys.F7:
                    try
                    {
                        TuiHuoFunc();

                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLog("退货已购商品出现异常:", ex);

                        MessageBox.Show("退货出现异常，请联系管理员！");
                    }
                    break;

                case Keys.F9:
                    ZDDETHFunc();
                    break;


            }
        }


        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {

        }


        //调整表格的列宽、同时隐藏不需要显示的列、禁止编辑、修改列名
        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                //列名
                dataGridView1.Columns[0].HeaderText = "货号";
                dataGridView1.Columns[1].HeaderText = "条码";
                dataGridView1.Columns[2].HeaderText = "品名";
                dataGridView1.Columns[3].HeaderText = "规格";
                dataGridView1.Columns[4].HeaderText = "数量";
                dataGridView1.Columns[6].HeaderText = "单位";
                dataGridView1.Columns[7].HeaderText = "单价";
                dataGridView1.Columns[8].HeaderText = "总价";
                dataGridView1.Columns[12].HeaderText = "活动类型";


                //隐藏
                dataGridView1.Columns[5].Visible = false;  //单位编码
                dataGridView1.Columns[11].Visible = false;  //类型
                dataGridView1.Columns[9].Visible = false;
                dataGridView1.Columns[10].Visible = false;
                dataGridView1.Columns[13].Visible = false;

            }
            catch
            {
            }
        }

        private void dataGridView2_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                //列名
                dataGridView2.Columns[0].HeaderText = "货号";
                dataGridView2.Columns[1].HeaderText = "条码";
                dataGridView2.Columns[2].HeaderText = "品名";
                dataGridView2.Columns[3].HeaderText = "规格";
                dataGridView2.Columns[4].HeaderText = "数量";
                dataGridView2.Columns[6].HeaderText = "单位";
                dataGridView2.Columns[7].HeaderText = "单价";
                dataGridView2.Columns[8].HeaderText = "总价";
                dataGridView2.Columns[12].HeaderText = "活动类型";


                //隐藏
                dataGridView2.Columns[5].Visible = false;  //单位编码
                dataGridView2.Columns[9].Visible = false;
                dataGridView2.Columns[10].Visible = false;
                dataGridView2.Columns[11].Visible = false;  //类型
                dataGridView2.Columns[13].Visible = false;


                //禁止编辑单元格
                //设置单元格是否可以编辑
                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    //允许修改金额
                    if (dataGridView2.Columns[i].Index != 8)
                    {
                        dataGridView2.Columns[i].ReadOnly = true;
                    }
                }
            }
            catch
            {
            }
        }



        //查询已购商品
        private void FindBuyedItem()
        {
            if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                string temp = textBox1.Text.Trim();
                using (var db = new hjnbhEntities())
                {
                    //查询明细单号
                    var jsinfo = db.hd_js.AsNoTracking().Where(e => e.v_code == temp).Select(e => new { e.ls_code, e.je, e.v_code }).FirstOrDefault();
                    if (jsinfo != null)
                    {
                        JSDH = jsinfo.v_code;
                        LSDH = jsinfo.ls_code;

                        var buyinfo = db.hd_ls_detail.AsNoTracking().Where(t => t.v_code == LSDH && t.amount > 0).ToList();
                        if (buyinfo.Count > 0)
                        {

                            //查询该单全部已退的
                            var refundinfo = db.hd_ls_detail.AsNoTracking().Where(t => t.v_code == LSDH && t.amount < 0).ToList();

                            buyedList.Clear(); //清空上次查询

                            foreach (var item in buyinfo)
                            {
                                #region 商品单位查询
                                //需要把单位编号转换为中文以便UI显示
                                int unitID = item.unit.HasValue ? (int)item.unit : 1;
                                string dw = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == unitID).Select(t => t.txt1).FirstOrDefault();
                                #endregion


                                //过滤掉已经退货的，不显示
                                if (refundinfo.Count > 0)
                                {

                                    //有退货历史的情况
                                    // 已退的具体物品
                                    var refunditem = refundinfo.Where(t => t.item_id == item.item_id && t.vtype == item.vtype).ToList();
                                    if (refunditem.Count > 0)
                                    {
                                        //该退货物品的已经退掉数量，目前记录的是个负数形式
                                        decimal refundcount = refunditem.Select(t => t.amount.Value).Sum();
                                        decimal counttemp = item.amount.Value;  //当时购买的数量

                                        counttemp -= Math.Abs(refundcount);
                                        if (counttemp > 0)
                                        {
                                            decimal Pricetemp = item.ls_price.HasValue ? item.ls_price.Value : 0.00m;
                                            int vtypetemp = item.vtype.HasValue ? item.vtype.Value : 0;
                                            string vtpestr = HDtypeFunc(vtypetemp, 1);
                                            buyedList.Add(new TuiHuoItemModel
                                            {
                                                goods = item.cname,
                                                noCode = item.item_id.Value,
                                                barCodeTM = item.tm,
                                                jjPrice = item.jj_price.HasValue ? item.jj_price.Value : 0.00m,
                                                Price = Pricetemp,
                                                ylsPrice = item.yls_price.HasValue ? item.yls_price.Value : 0.00m,
                                                Sum = Math.Round(Pricetemp * counttemp, 2),
                                                countNum = counttemp,
                                                spec = item.spec,
                                                unit = unitID,
                                                unitStr = dw,
                                                MaxTuiCount = counttemp,
                                                vtype = vtypetemp,
                                                vtypeStr = vtpestr
                                            });
                                        }
                                    }
                                    else
                                    {
                                        decimal Pricetemp = item.ls_price.HasValue ? item.ls_price.Value : 0.00m;
                                        int vtypetemp = item.vtype.HasValue ? item.vtype.Value : 0;
                                        string vtpestr = HDtypeFunc(vtypetemp, 1);
                                        //不是退货就原样列出
                                        buyedList.Add(new TuiHuoItemModel
                                        {
                                            goods = item.cname,
                                            noCode = item.item_id.Value,
                                            barCodeTM = item.tm,
                                            jjPrice = item.jj_price.HasValue ? item.jj_price.Value : 0.00m,
                                            Price = Pricetemp,
                                            ylsPrice = item.yls_price.HasValue ? item.yls_price.Value : 0.00m,
                                            Sum = Math.Round(Pricetemp * item.amount.Value, 2),
                                            countNum = item.amount.Value,
                                            MaxTuiCount = item.amount.Value,
                                            spec = item.spec,
                                            unit = unitID,
                                            unitStr = dw,
                                            vtype = vtypetemp,
                                            vtypeStr = vtpestr

                                        });
                                    }
                                }
                                else
                                {
                                    decimal Pricetemp = item.ls_price.HasValue ? item.ls_price.Value : 0.00m;
                                    int vtypetemp = item.vtype.HasValue ? item.vtype.Value : 0;
                                    string vtpestr = HDtypeFunc(vtypetemp, 1);
                                    //没有历史退货的，就一五一十都列出
                                    buyedList.Add(new TuiHuoItemModel
                                    {
                                        goods = item.cname,
                                        noCode = item.item_id.Value,
                                        barCodeTM = item.tm,
                                        jjPrice = item.jj_price.HasValue ? item.jj_price.Value : 0.00m,
                                        Price = Pricetemp,
                                        ylsPrice = item.yls_price.HasValue ? item.yls_price.Value : 0.00m,
                                        Sum = Math.Round(Pricetemp * item.amount.Value, 2),
                                        countNum = item.amount.Value,
                                        MaxTuiCount = item.amount.Value,
                                        spec = item.spec,
                                        unit = unitID,
                                        unitStr = dw,
                                        vtype = vtypetemp,
                                        vtypeStr = vtpestr
                                    });
                                }
                            }


                            dataGridView1.Refresh();
                        }
                        else
                        {
                            MessageBox.Show("无法查询此单号的具体商品信息，请确认输入的单号是否正确？");
                        }

                        if (buyedList.Count > 0)
                        {
                            textBox7.Text = buyedList[0].barCodeTM;
                            buyedTempList.Clear();
                            //存入缓存
                            foreach (var item in buyedList)
                            {
                                buyedTempList.Add(new TuiHuoItemModel
                                {
                                    noCode = item.noCode,
                                    barCodeTM = item.barCodeTM,
                                    goods = item.goods,
                                    countNum = item.countNum,
                                    jjPrice = item.jjPrice,
                                    Price = item.Price,
                                    ylsPrice = item.ylsPrice,
                                    unit = item.unit,
                                    Sum = item.Sum,
                                    unitStr = item.unitStr,
                                    spec = item.spec,
                                    vtype = item.vtype,
                                    vtypeStr = item.vtypeStr,
                                    MaxTuiCount = item.MaxTuiCount
                                });
                            }
                        }
                        else
                        {
                            MessageBox.Show("该单号没有可退的商品！");
                        }

                    }
                    else
                    {
                        MessageBox.Show("没有查询到此单据，请确认输入的单号是否正确？");
                    }

                }
            }
            else
            {
                MessageBox.Show("请输入您的小票单号");
                textBox1.Focus();
                textBox1.SelectAll();
            }

        }


        //返回活动名称
        private string HDtypeFunc(int hdtype, int tj = 0)
        {
            string temp = "";
            switch (hdtype)
            {
                case 0:
                    temp = "非活动商品";
                    break;
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

        //小键盘向上
        private void UpFun()
        {
            try
            {
                //当前行数大于1行才生效
                if (dataGridView2.Rows.Count > 1)
                {
                    int rowindex_temp = dataGridView2.SelectedRows[0].Index;
                    if (rowindex_temp == 0)
                    {
                        dataGridView2.Rows[dataGridView2.Rows.Count - 1].Selected = true;
                        dataGridView2.Rows[rowindex_temp].Selected = false;

                    }
                    else
                    {
                        dataGridView2.Rows[rowindex_temp - 1].Selected = true;
                        dataGridView2.Rows[rowindex_temp].Selected = false;
                    }

                }

            }
            catch (Exception ex)
            {

                LogHelper.WriteLog("会员取货小键盘向上时发生异常：" + ex);

            }
        }

        //小键盘向下
        private void DownFun()
        {
            try
            {
                //当前行数大于1行才生效
                if (dataGridView2.Rows.Count > 1)
                {
                    int rowindexDown_temp = dataGridView2.SelectedRows[0].Index;
                    if (rowindexDown_temp == dataGridView2.Rows.Count - 1)
                    {
                        dataGridView2.Rows[0].Selected = true;
                        dataGridView2.Rows[rowindexDown_temp].Selected = false;

                    }
                    else
                    {
                        dataGridView2.Rows[rowindexDown_temp + 1].Selected = true;
                        dataGridView2.Rows[rowindexDown_temp].Selected = false;
                    }

                }

            }
            catch (Exception ex)
            {

                LogHelper.WriteLog("会员取货小键盘向下时发生异常：" + ex);

            }
        }




        /// <summary>
        /// 添加（将要）需要退货的商品
        /// </summary>
        private void CXFunc()
        {
            try
            {
                string temptxt = textBox7.Text.Trim();
                if (string.IsNullOrEmpty(temptxt))
                {
                    MessageBox.Show("请输入需要取出的商品条码/货号/品名!");
                    return;
                }

                if (string.IsNullOrEmpty(textBox4.Text.Trim()))
                {
                    MessageBox.Show("请输入需要取出的商品数量！");
                    textBox4.Focus();
                    return;
                }



                decimal count_temp = 1;
                decimal.TryParse(textBox4.Text.Trim(), out count_temp);

                int itemid_temp = -1;
                int.TryParse(temptxt, out itemid_temp);



                int indextemp = -1; //使用鼠标选择
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    indextemp = dataGridView1.SelectedRows[0].Index;
                }


                //用列表是因为有时候会有同货号不同活动类型的
                var getitemlist = buyedList.Where(t => t.noCode == itemid_temp || t.barCodeTM.Contains(temptxt) || t.goods.Contains(temptxt)).ToList();
                if (getitemlist.Count > 0)
                {
                    for (int i = 0; i < getitemlist.Count; i++)
                    {

                        if (getitemlist[i].countNum < count_temp)
                        {
                            MessageBox.Show("退货商品[" + getitemlist[i].goods + "]/[" + getitemlist[i].vtypeStr + "]的数量不能大于已购数量，请重新输入！");
                            textBox4.Focus();
                            textBox4.SelectAll();
                            continue;
                        }
                        else
                        {
                            var itemed = tuihuoList.Where(e => e.noCode == getitemlist[i].noCode && e.vtype == getitemlist[i].vtype).FirstOrDefault();
                            if (itemed != null)
                            {
                                itemed.countNum += count_temp;
                                getitemlist[i].countNum -= count_temp;
                                itemed.Sum = Math.Round(itemed.countNum * itemed.Price, 2);
                            }
                            else
                            {

                                getitemlist[i].countNum -= count_temp;

                                var goods = new TuiHuoItemModel
                                {
                                    noCode = getitemlist[i].noCode,
                                    barCodeTM = getitemlist[i].barCodeTM,
                                    goods = getitemlist[i].goods,
                                    countNum = count_temp,
                                    MaxTuiCount = getitemlist[i].MaxTuiCount,
                                    jjPrice = getitemlist[i].jjPrice,
                                    Price = getitemlist[i].Price,
                                    ylsPrice = getitemlist[i].ylsPrice,
                                    unit = getitemlist[i].unit,
                                    Sum = Math.Round(getitemlist[i].Price * count_temp, 2),
                                    unitStr = getitemlist[i].unitStr,
                                    spec = getitemlist[i].spec,
                                    vtype = getitemlist[i].vtype,
                                    vtypeStr = getitemlist[i].vtypeStr
                                };
                                tuihuoList.Add(goods);
                            }
                        }


                        dataGridView1.Refresh();
                        dataGridView2.Refresh();

                    }


                }
                else
                {
                    MessageBox.Show("没有查询到商品，请核对输入的商品信息");
                }

            }
            catch (Exception e)
            {
                LogHelper.WriteLog("退商品查询窗口查询商品时出现异常:", e);
                MessageBox.Show("退商品查询窗口查询商品时出现异常，请联系管理员！");
            }
        }


        //按-号修改金额
        private void UpdataCount()
        {
            if (tuihuoList.Count > 0)
            {
                try
                {
                    dataGridView2.CurrentCell = dataGridView2.SelectedRows[0].Cells[8];
                    dataGridView2.BeginEdit(true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("修改金额异常");
                    LogHelper.WriteLog("收银主界面按*号修改商品数量发生异常:", ex);
                }
            }

        }






        string JSDH = string.Empty;  //结算单号
        string LSDH = string.Empty;  //零售单号
        string vipcard = "";  //会员卡
        string vipname = "";  //会员名字


        /// <summary>
        /// 保存退货,提交数据
        /// </summary>
        private void TuiHuoFunc()
        {
            //1、根据条码查相应的零售明细单
            //2、查到此明细单后在th_flag字段做退货标志
            //3、以此退货商品新建入库主单与入库明细单

            if (tuihuoList.Count == 0)
            {
                MessageBox.Show("请选择需要退货的商品");
                return;
            }

            if (string.IsNullOrEmpty(JSDH) || string.IsNullOrEmpty(LSDH))
            {
                MessageBox.Show("请先查询小票单据");
                return;
            }

            //练习模式下不允许退货
            if (HandoverModel.GetInstance.isLianxi)
            {
                MessageBox.Show("练习模式下该操作无效！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var db = new hjnbhEntities())
            {
                string rkStr = string.IsNullOrEmpty(textBox2.Text.Trim()) ? "前台退货" : textBox2.Text.Trim();
                //存储过程返回状态码
                int re_temp = 0;
                string THNoteID = ""; //退货单号
                string ZJFstr = "";  //总积分
                decimal tuihuoje = 0; //退货总金额

                var vip = db.hd_ls.AsNoTracking().Where(t => t.v_code == LSDH).Select(t => t.vip).FirstOrDefault();

                //获取办理退货的商品零售单信息
                var THinfo = db.hd_ls.Where(t => t.v_code == LSDH).FirstOrDefault();

                var jsid = db.hd_js.Where(t => t.v_code == JSDH).Select(t => t.id).FirstOrDefault();
                //单号计算方式，当前时间+00000+id
                long no_temp = Convert.ToInt64(System.DateTime.Now.ToString("yyyyMMdd") + "000000");
                THNoteID = "LSR" + (no_temp + jsid).ToString();//获取退货入库单号

                //商品信息
                foreach (var item in tuihuoList)
                {
                    var jsInfo = db.hd_js.Where(t => t.v_code == JSDH).FirstOrDefault();
                    if (jsInfo == null)
                    {
                        MessageBox.Show("没有此小票信息！");
                        return;
                    }
                    //找到零售明细 (注意不要加上 t.th_flag != 1 这个判断，因为目前设定，虽然标志为退货，但数量不一定退清了)
                    var mxinfo = db.hd_ls_detail.Where(t => t.item_id == item.noCode && t.v_code == LSDH && t.amount > 0 && t.vtype == item.vtype).FirstOrDefault();
                    if (mxinfo != null)
                    {

                        var pf = db.hd_item_info.AsNoTracking().Where(t => t.item_id == item.noCode).Select(t => t.pf_price).FirstOrDefault();
                        //mxinfo.amount -= item.countNum;  //目前不减去退货数量，是新增一行数量为负的，原来的标志退货
                        mxinfo.th_flag = 1;  //退货标志
                        mxinfo.th_date = System.DateTime.Now;  //退货时间

                        //该退货商品总积分
                        decimal tuihuojftemp = mxinfo.jf.HasValue ? mxinfo.jf.Value : 0.00m;
                        //该退货商品单品积分
                        decimal dpjetemp = tuihuojftemp / mxinfo.amount.Value;
                        //此次退货单品扣减积分
                        decimal currjf = dpjetemp * item.countNum;
                        //此次退货总共扣减积分
                        CurrentTuihuJF += currjf;

                        //新增退货的明细
                        var tuihuoMX = new hd_ls_detail
                        {
                            v_code = LSDH, //标识单号
                            item_id = item.noCode,//商品货号
                            tm = item.barCodeTM,//条码
                            cname = item.goods,//名称
                            spec = item.spec,//规格
                            //hpack_size = (decimal?)item.hpackSize,//不知是什么,包装规格
                            unit = mxinfo.unit,  //单位
                            amount = -item.countNum, //数量
                            jj_price = mxinfo.jj_price, //进价
                            ls_price = item.Price,
                            yls_price = mxinfo.yls_price,//原零售价
                            zk = mxinfo.zk,//折扣
                            jf = -currjf,
                            iszs = mxinfo.iszs,//是否赠送
                            cid = HandoverModel.GetInstance.userID,//零售员ID
                            ctime = System.DateTime.Now, //出单时间
                            vtype = mxinfo.vtype,  //活动类型
                            ywy = mxinfo.ywy,
                            th_date = System.DateTime.Now

                        };

                        db.hd_ls_detail.Add(tuihuoMX);



                        if (!string.IsNullOrEmpty(textBox2.Text.Trim()))
                        {
                            jsInfo.remark += textBox2.Text.Trim();  //备注
                        }

                        tuihuoje += item.Sum; //总金额

                        //扣减结算单的收入金额
                        jsInfo.je -= item.Sum;


                        db.SaveChanges();

                        //要判断是否打包商品
                        var dbinfo = db.hd_item_db.AsNoTracking().Where(t => t.sitem_id == item.noCode).ToList();
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
                                        new SqlParameter("@amount", itemdb.amount.Value*item.countNum),
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
                                    new SqlParameter("@amount", item.countNum),
                                    new SqlParameter("@jj_price", mxinfo.jj_price),
                                    new SqlParameter("@ls_price",  item.Sum),
                                    new SqlParameter("@pf_price", pf.HasValue?pf:0),
                                    new SqlParameter("@remark", "零售退货"),
                                    new SqlParameter("@cid", HandoverModel.GetInstance.userID)
                                };

                                re_temp = db.Database.ExecuteSqlCommand("EXEC [dbo].[create_in_detail] @v_code,@scode,@vtype,0,@item_id,@tm,@amount,@jj_price,@ls_price,@pf_price,@remark,@cid,1,0", sqlThMX);

                                #endregion


                            }

                        }
                    }

                }


                //结算信息
                var jstypeinfo = db.hd_js_type.AsNoTracking().Where(t => t.v_code == JSDH).Select(t => new { t.js_type, t.bankcode }).FirstOrDefault();
                string bankcodetemp = jstypeinfo == null ? THNoteID : jstypeinfo.bankcode;
                int jstypetemp = jstypeinfo == null ? 0 : jstypeinfo.js_type.Value;

                //新增退货结算
                db.hd_js_type.Add(new hd_js_type
                {
                    v_code = JSDH,
                    cid = HandoverModel.GetInstance.userID,
                    ctime = System.DateTime.Now,
                    je = tuihuoje,
                    bankcode = bankcodetemp,
                    status = -1,
                    js_type = jstypetemp
                });



                //管理会员积分
                if (THinfo.vip.Value != 0)
                {
                    //查会员
                    var vipinfo = db.hd_vip_info.Where(e => e.vipcode == THinfo.vip).FirstOrDefault();
                    if (vipinfo != null)
                    {
                        vipinfo.jfnum -= CurrentTuihuJF;
                        ZJFstr = vipinfo.jfnum.ToString();
                        vipname = vipinfo.vipname;
                        vipcard = vipinfo.vipcard;
                        vipinfo.ljxfje -= tuihuoje;  //减去累计消费

                        string vipidStr = vipinfo.vipcode.ToString();
                        //记录会员积分扣减
                        var vipcz = new hd_vip_cz
                        {
                            ckh = vipidStr, //会员编号
                            czr = HandoverModel.GetInstance.userID,
                            rq = System.DateTime.Now, //时间
                            jf = -CurrentTuihuJF,//积分
                            fs = (byte)7, //类型
                            ctype = (byte)0,
                            srvoucher = JSDH, //单号(0923说要用小票单号)
                            je = -tuihuoje,
                            lsh = HandoverModel.GetInstance.scode
                        };

                        db.hd_vip_cz.Add(vipcz);

                        //自动备注积分扣减
                        string memotemp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ： " + " 会员退货扣减积分 " + (-CurrentTuihuJF).ToString() + ";";
                        VipAutoMemoFunc(db, vipinfo.vipcode, vipinfo.vipcard, vipinfo.vipname, memotemp, 3);
                    }
                }

                db.SaveChanges();

                //新增退货主单
                if (THinfo != null)
                {
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
                        new SqlParameter("@hs_code",  vip), 
                        new SqlParameter("@ywy",  THinfo.ywy),
                        new SqlParameter("@srvoucher", THinfo.v_code),
                        new SqlParameter("@remark", rkStr),
                        new SqlParameter("@cid", HandoverModel.GetInstance.userID)

                    };

                        db.Database.ExecuteSqlCommand("EXEC [dbo].[Create_in] @v_code,@scode,@vtype,@hs_code,@ywy,@srvoucher,@remark,@cid,1,0", sqlTh);

                    }

                }

                if (re_temp > 0)
                {
                    //退货金额
                    decimal sum_temp = tuihuoList.Select(t => t.Sum).Sum();
                    HandoverModel.GetInstance.RefundMoney += sum_temp;

                    string jestr = sum_temp.ToString();
                    string jfstr = "-" + CurrentTuihuJF.ToString();

                    //小票单号
                    string id_temp = JSDH + "(退)";
                    //打小票
                    TuiHuoPrinter printer = new TuiHuoPrinter(tuihuoList, vipcard, vipname, "客户退货单据", id_temp, jestr, jfstr, ZJFstr);
                    printer.StartPrint();

                    MessageBox.Show("退货登记成功！");
                    textBox1.SelectAll();
                    tuihuoList.Clear();
                    buyedList.Clear();
                    textBox7.Clear();
                    HandoverModel.GetInstance.TuiHuoJSDH = "";  //结算单号
                    HandoverModel.GetInstance.TuiHuoLSDH = "";  //零售单号
                    JSDH = "";
                    LSDH = "";
                }
                else
                {
                    MessageBox.Show("退货数据登记失败！请核实此单据真实性！");
                }
            }


        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' && e.KeyChar != '.' || e.KeyChar > '9' && e.KeyChar != '.' || ((TextBox)(sender)).Text.IndexOf('.') >= 0 && e.KeyChar == '.') && e.KeyChar != (char)13 && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (DialogResult.Yes == MessageBox.Show("是否确认进行退货处理？", "提醒", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
                {
                    TuiHuoFunc();
                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("退货已购商品出现异常:", ex);

                MessageBox.Show("退货出现异常，请联系管理员！");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox7.Text.Trim()))
            {
                CXFunc();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                bool ishastui = false;
                foreach (var item in goodsBuyList)
                {
                    if (item.isTuiHuo)
                    {
                        ishastui = true;
                    }
                }

                if (ishastui)
                {
                    if (HandoverModel.GetInstance.TuiHuoJSDH != textBox1.Text.Trim())
                    {
                        MessageBox.Show("购物车内已有退货单的商品正在退货中，请完成或者删除该退货商品后再进行其它单号的退货操作！", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                }



                if (string.IsNullOrEmpty(textBox1.Text.Trim()))
                {
                    textBox1.Focus();
                }
                else
                {
                    FindBuyedItem();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("退货查询已购商品出现异常:", ex);

                MessageBox.Show("查询已购商品出现异常，请联系管理员！");
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    textBox7.Text = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
                }
            }
            catch
            {

            }
        }



        //整单抵额
        private void button4_Click(object sender, EventArgs e)
        {
            ZDDETHFunc();
        }

        //整单抵额处理
        private void ZDDETHFunc()
        {
            if (tuihuoList.Count > 0)
            {
                bool isgo = false;
                foreach (var item in tuihuoList)
                {
                    var ishasTui = goodsBuyList.Where(t => t.isTuiHuo && t.noCode == item.noCode && t.vtype == item.vtype).FirstOrDefault();
                    if (ishasTui != null)
                    {
                        if (item.MaxTuiCount <= ishasTui.countNum)
                        {
                            MessageBox.Show("退货商品[" + item.goods + "]数量不允许大于已购数量[" + item.MaxTuiCount + "]，请确认您输入的数据是否正确？", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            isgo = true;
                        }

                    }

                }

                if (isgo == false)
                {
                    changed(tuihuoList);
                    //保存退货小票
                    HandoverModel.GetInstance.TuiHuoJSDH = JSDH;
                    HandoverModel.GetInstance.TuiHuoLSDH = LSDH;
                    this.Close();
                }


            }
            else
            {
                MessageBox.Show("您还没有选择任何需要退货的商品，请先将要退货的商品放入退货列表中！");
            }

        }

        //选择整单
        private void button6_Click(object sender, EventArgs e)
        {
            ChooseZD();
        }



        private void ChooseZD()
        {
            try
            {
                tuihuoList.Clear();
                buyedList.Clear();
                //从缓存取出
                foreach (var item in buyedTempList)
                {
                    buyedList.Add(new TuiHuoItemModel
                    {
                        noCode = item.noCode,
                        barCodeTM = item.barCodeTM,
                        goods = item.goods,
                        countNum = item.countNum,
                        jjPrice = item.jjPrice,
                        Price = item.Price,
                        ylsPrice = item.ylsPrice,
                        unit = item.unit,
                        Sum = item.Sum,
                        unitStr = item.unitStr,
                        spec = item.spec,
                        vtype = item.vtype,
                        vtypeStr = item.vtypeStr,
                        MaxTuiCount = item.MaxTuiCount
                    });
                }


                for (int i = 0; i < buyedList.Count; i++)
                {
                    tuihuoList.Add(new TuiHuoItemModel
                    {
                        noCode = buyedList[i].noCode,
                        barCodeTM = buyedList[i].barCodeTM,
                        goods = buyedList[i].goods,
                        countNum = buyedList[i].countNum,
                        jjPrice = buyedList[i].jjPrice,
                        Price = buyedList[i].Price,
                        ylsPrice = buyedList[i].ylsPrice,
                        unit = buyedList[i].unit,
                        Sum = buyedList[i].Sum,
                        unitStr = buyedList[i].unitStr,
                        spec = buyedList[i].spec,
                        vtype = buyedList[i].vtype,
                        vtypeStr = buyedList[i].vtypeStr,
                        MaxTuiCount = buyedList[i].MaxTuiCount
                    });

                    buyedList[i].countNum = 0.00m;
                }


                dataGridView1.Refresh();
                dataGridView2.Refresh();

            }
            catch (Exception)
            {
                MessageBox.Show("整单选择出现异常！");
            }
        }

        //删除
        private void button7_Click(object sender, EventArgs e)
        {
            DeleTui();
        }


        //删除选择
        private void DeleTui()
        {
            try
            {
                //当前行数大于1行时删除选中行后把往上一行设置为选中状态
                if (dataGridView2.Rows.Count > 0)
                {

                    if (DialogResult.Yes == MessageBox.Show("是否确定从退货列表中删除选中的退货商品？", "提醒", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
                    {
                        int DELindex_temp = dataGridView2.SelectedRows[0].Index;
                        //把数量还原
                        decimal numtemp = tuihuoList[DELindex_temp].countNum;
                        int idtemp = tuihuoList[DELindex_temp].noCode;
                        int vtpytemp = tuihuoList[DELindex_temp].vtype;
                        var buyiteminfo = buyedList.Where(t => t.noCode == idtemp && t.vtype == vtpytemp).FirstOrDefault();
                        if (buyiteminfo != null)
                        {
                            buyiteminfo.countNum += numtemp;
                        }

                        tuihuoList.RemoveAt(DELindex_temp);

                        dataGridView1.Refresh();
                        dataGridView2.Refresh();

                        if (DELindex_temp - 1 >= 0)
                        {
                            dataGridView2.Rows[DELindex_temp - 1].Selected = true;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("收银主界面删除选中行发生异常:", ex);
            }
        }


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


    }
}
