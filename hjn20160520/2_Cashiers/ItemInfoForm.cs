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

namespace hjn20160520._2_Cashiers
{
    public partial class ItemInfoForm : Form
    {
        TipForm tipForm;
        //string flagStr = string.Empty;  //记录输入框的查询文本，防止用户重复查询
        private BindingList<GoodsBuy> goodsInfoList = new BindingList<GoodsBuy>();
        ChoiceGoods CGForm = new ChoiceGoods(); //商品选择窗口


        public ItemInfoForm()
        {
            InitializeComponent();
        }


        private void ItemInfoForm_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void ItemInfoForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.Close();
                    break;

                case Keys.Enter:

                    EnterFunc();

                    break;

                //case Keys.F3:

                //    ShowUIInfo(HandoverModel.GetInstance.scode);

                //    break;
                case Keys.F2:

                    int code_temp = 0;
                    if (int.TryParse(comboBox1.SelectedValue.ToString(), out code_temp))
                    {
                        ShowUIInfo(code_temp);
                    }
                    else
                    {
                        MessageBox.Show("没有此分店数据！");
                    }

                    break;
                case Keys.Up:
                    UpFun();
                    break;

                case Keys.Down:
                    DownFun();
                    break;

            }
        }


        /// <summary>
        /// 回车键
        /// </summary>
        private void EnterFunc()
        {
            //if (textBox1.Text == flagStr)
            //{
            //    //MessageBox.Show("Test");
            //    return;
            //}
            Tipslabel.Visible = true;

            //CXFunc();
            EFSelectByBarCode();

            //如果结果只有一个就自动显示明细
            if (goodsInfoList.Count == 1)
            {
                int code_temp_1 = 0;
                if (int.TryParse(comboBox1.SelectedValue.ToString(), out code_temp_1))
                {
                    ShowUIInfo(code_temp_1);
                }
                else
                {
                    MessageBox.Show("没有此分店数据！");
                }
            }

            Tipslabel.Text = "查询完成";
            //flagStr = textBox1.Text;
            textBox1.SelectAll();
        }



        #region 旧的查询方法，1121更新


        //根据条码/货号、品名查询商品
        //private void CXFunc()
        //{
        //    try
        //    {
        //        string temptxt = textBox1.Text.Trim();
        //        if (string.IsNullOrEmpty(temptxt))
        //        {
        //            tipForm.Tiplabel.Text = "请输入需要查找的商品条码!";
        //            tipForm.ShowDialog();
        //            return;
        //        }
        //        int itemid_temp = -1;
        //        int.TryParse(temptxt, out itemid_temp);

        //        if (goodsInfoList.Count > 0) goodsInfoList.Clear();
        //        using (hjnbhEntities db = new hjnbhEntities())
        //        {
        //            var rules = db.hd_item_info.AsNoTracking().Where(t => t.tm.Contains(temptxt) || t.cname.Contains(temptxt) || t.item_id == itemid_temp).ToList();

        //            //如果查出数据不至一条就弹出选择窗口，否则直接显示出来
        //            if (rules.Count == 0)
        //            {
        //                this.textBox1.SelectAll();
        //                tipForm.Tiplabel.Text = "没有查找到该商品!";
        //                tipForm.ShowDialog();
        //                return;
        //            }
        //            #region 查到多条记录时

        //            if (rules.Count > 30)
        //            {

        //                if (DialogResult.No == MessageBox.Show("查询到多个类似的商品，数据量较大时可能造成几秒的卡顿，是否继续查询？", "提醒", MessageBoxButtons.YesNo))
        //                {
        //                    return;
        //                }
        //            }

        //            foreach (var item in rules)
        //            {
        //                #region 商品单位查询
        //                //需要把单位编号转换为中文以便UI显示
        //                int unitID = item.unit.HasValue ? (int)item.unit : 1;
        //                string dw = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == unitID).Select(t => t.txt1).FirstOrDefault();
        //                #endregion

        //                goodsInfoList.Add(new GoodsBuy
        //                {
        //                    noCode = item.item_id,
        //                    barCodeTM = item.tm,
        //                    goods = item.cname,
        //                    spec = item.spec,
        //                    unitStr = dw,
        //                    lsPrice = item.ls_price,
        //                    hyPrice = item.hy_price,

        //                });
        //            }

        //            dataGridView1.DataSource = goodsInfoList;
        //            //NotShowFunc();

        //            #endregion
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        LogHelper.WriteLog("商品详情查询窗口查询商品时出现异常:", e);
        //        MessageBox.Show("数据库连接出错！");
        //        string tip = ConnectionHelper.ToDo();
        //        if (!string.IsNullOrEmpty(tip))
        //        {
        //            MessageBox.Show(tip);
        //        }
        //    }
        //}

        #endregion



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

                int the_scode = int.Parse(comboBox1.SelectedValue.ToString());


                using (hjnbhEntities db = new hjnbhEntities())
                {

                    var itemsInfo = db.v_xs_item_info.AsNoTracking().Where(t => t.tm.Contains(temptxt) || t.ftm.Contains(temptxt) || t.cname.Contains(temptxt) || t.item_id == itemid_temp)
                        .Where(t => t.scode == the_scode || t.scode == 0)
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
                                    isCyjf = item.cyjf == 1 ? true : false,
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
                                    //Sum = item.Sum,
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
                            //如果会员价为0就转换为零售价
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
                                //Sum = BuyListTemp[0].Sum
                            };

                            if (goodsInfoList.Count == 0)
                            {
                                goodsInfoList.Add(newGoods_temp);
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


                    //更新商品售价
                    for (int i = 0; i < goodsInfoList.Count; i++)
                    {
                        goodsInfoList[i].Sum = VipID == 0 ? Math.Round(goodsInfoList[i].countNum * goodsInfoList[i].lsPrice.Value, 2) : Math.Round(goodsInfoList[i].countNum * goodsInfoList[i].hyPrice.Value, 2);
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



        //显示下方详情
        private void ShowUIInfo(int scode)
        {
            try
            {
                int itemid_temp = 0;
                if (goodsInfoList.Count == 0)
                {
                    MessageBox.Show("请先查询商品！");
                    return;
                }
                else
                {
                    int index_temp = dataGridView1.SelectedRows[0].Index;
                    itemid_temp = goodsInfoList[index_temp].noCode;
                }

                using (hjnbhEntities db = new hjnbhEntities())
                {
                    var info = db.hd_istore.AsNoTracking().Where(t => t.scode == scode && t.item_id == itemid_temp).FirstOrDefault();
                    if (info != null)
                    {
                        label8.Text = info.amount.HasValue ? info.amount.Value.ToString() : "空";      //库存

                    }
                    else
                    {
                        label8.Text = "空";
                    }


                    var rules = db.hd_item_info.AsNoTracking().Where(t => t.item_id == itemid_temp).FirstOrDefault();
                    if (rules != null)
                    {
                        int temp = rules.status.HasValue ? (int)rules.status.Value : 0;
                        string str_temp = string.Empty;
                        switch (temp)
                        {
                            case 0:
                                str_temp = "正常";
                                break;
                            case 1:
                                str_temp = "只销不进";
                                break;
                            case 2:
                                str_temp = "停止销售";
                                break;
                            default:
                                str_temp = "正常";
                                break;
                        }

                        int zk = rules.isale.HasValue ? rules.isale.Value : 1;
                        string str_zk = string.Empty;
                        switch (zk)
                        {
                            case 0:
                                str_zk = "不能";
                                break;
                            case 1:
                                str_zk = "可以";
                                break;
                            default:
                                str_zk = "空";
                                break;
                        }

                        int lb = rules.lb_code.HasValue ? rules.lb_code.Value : 0;
                        //类别
                        var lbinfo = db.hd_item_lb.AsNoTracking().Where(e => e.lb_code == lb).Select(e => e.cname).FirstOrDefault();
                        if (lbinfo != null)
                        {
                            label17.Text = lbinfo;
                        }
                        else
                        {
                            label17.Text = "空";
                        }

                        //提成方式
                        switch (rules.tc_type)
                        {
                            case 0:
                                label22.Text = "比例";
                                break;
                            case 1:
                                label22.Text = "金额";
                                break;
                            case 2:
                                label22.Text = "毛利";
                                break;
                            default:
                                label22.Text = "空";
                                break;

                        }

                        //是否打包
                        switch (rules.db_flag)
                        {
                            case 0:
                                label22.Text = "否";
                                break;
                            case 1:
                                label22.Text = "是";
                                break;
                            default:
                                label22.Text = "否";
                                break;

                        }

                        //是否积分
                        switch (rules.cy_jf)
                        {
                            case 0:
                                label42.Text = "否";
                                break;
                            case 1:
                                label42.Text = "是";
                                break;
                            default:
                                label42.Text = "是";
                                break;

                        }




                        label10.Text = itemid_temp.ToString();  //货号
                        label9.Text = str_temp; //状态  
                        label7.Text = str_zk;   //能否打折

                        label23.Text = rules.cname;  //品名
                        label10.Text = rules.item_id.ToString();  //货号
                        //label11.Text = rules.tm;  //条码
                        label36.Text = rules.py; //拼音
                        label21.Text = rules.manufactory;  //产地
                        label40.Text = rules.manufacturer; //厂家
                        label15.Text = rules.brand; //品牌
                        label34.Text = rules.tc_je.ToString(); //提成金额
                        label27.Text = rules.jj_price.ToString();  //进价
                        label28.Text = rules.hy_price.ToString(); //会员价
                        label31.Text = rules.ls_price.ToString(); // 零售价
                        label32.Text = rules.pf_price.ToString();  //批发价
                        label38.Text = rules.cx_price.ToString();  //促销价
                        

                    }

                }
                //MessageBox.Show("查询完成");
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("商品详情查询窗口查询明细时出现异常:", e);
                MessageBox.Show("数据库连接出错！");
                //string tip = ConnectionHelper.ToDo();
                //if (!string.IsNullOrEmpty(tip))
                //{
                //    MessageBox.Show(tip);
                //}
            }
        }


        private void ItemInfoForm_Load(object sender, EventArgs e)
        {
            this.ActiveControl = textBox1;
            textBox1.Focus();
            textBox1.SelectAll();
            tipForm = new TipForm();

            //label8.Text = string.Empty;  //库存
            //label10.Text = string.Empty;
            //label9.Text = string.Empty;
            //label7.Text = string.Empty;
            Tipslabel.Visible = false;

            ShowScodeFunc();

            dataGridView1.DataSource = goodsInfoList;

            CGForm.changed += CGForm_changed;

        }


        // 从商品选择窗口传递回的商品
        void CGForm_changed(GoodsBuy goods)
        {

            var re = goodsInfoList.Where(t => t.noCode == goods.noCode).FirstOrDefault();
            //如果存在还要判定是否数量限购封顶，如果封顶了再另起一组
            if (re != null)
            {
                if (re.isXG == false)
                {
                    re.countNum++;
                    re.Sum = HandoverModel.GetInstance.VipID > 0 ? Math.Round(re.hyPrice.Value * re.countNum, 2) : Math.Round(re.lsPrice.Value * re.countNum, 2);
                    dataGridView1.Refresh();
                }
                else
                {
                    if (DialogResult.OK == MessageBox.Show("此单 " + goods.goods + " 超出限购的部分将不再享受活动优惠，是否确认购买？", "活动提醒", MessageBoxButtons.OKCancel))
                    {
                        //另起的这一组也要能数量叠加
                        var reXG = goodsInfoList.Where(t => t.noCode == goods.noCode && t.isXG == false).FirstOrDefault();
                        if (reXG != null)
                        {
                            reXG.countNum++;
                            reXG.Sum = HandoverModel.GetInstance.VipID > 0 ? Math.Round(reXG.hyPrice.Value * reXG.countNum, 2) : Math.Round(reXG.lsPrice.Value * reXG.countNum, 2);
                            dataGridView1.Refresh();
                        }
                        else
                        {
                            goodsInfoList.Add(goods);

                        }

                    }
                }

            }
            else
            {

                goodsInfoList.Add(goods);

            }

            //textBox1.Clear();
        }


        //调整表格的列宽、同时隐藏不需要显示的列、禁止编辑、修改列名
        private void UpdateNameFunc()
        {
            try
            {
                //列名
                dataGridView1.Columns[1].HeaderText = "条码";
                dataGridView1.Columns[2].HeaderText = "品名";
                dataGridView1.Columns[3].HeaderText = "规格";
                dataGridView1.Columns[6].HeaderText = "单位";
                dataGridView1.Columns[8].HeaderText = "零售价";
                dataGridView1.Columns[9].HeaderText = "会员价";
                dataGridView1.Columns[11].HeaderText = "拼音";


                //隐藏      
                dataGridView1.Columns[0].Visible = false;
                dataGridView1.Columns[4].Visible = false;
                dataGridView1.Columns[5].Visible = false;
                dataGridView1.Columns[7].Visible = false;
                dataGridView1.Columns[10].Visible = false;
                dataGridView1.Columns[11].Visible = false;
                dataGridView1.Columns[12].Visible = false;
                dataGridView1.Columns[13].Visible = false;
                dataGridView1.Columns[14].Visible = false;
                dataGridView1.Columns[15].Visible = false;
                dataGridView1.Columns[16].Visible = false;
                dataGridView1.Columns[17].Visible = false;
                dataGridView1.Columns[18].Visible = false; //批发价
                dataGridView1.Columns[19].Visible = false; //活动商品标志
                dataGridView1.Columns[20].Visible = false; //VIP标志
                dataGridView1.Columns[21].Visible = false; //限购标志
                dataGridView1.Columns[22].Visible = false; //活动类型
                dataGridView1.Columns[23].Visible = false;  //业务
                dataGridView1.Columns[24].Visible = false; //品牌
                dataGridView1.Columns[25].Visible = false; //类别
                dataGridView1.Columns[26].Visible = false; //关联
                dataGridView1.Columns[27].Visible = false; //是否打包
                dataGridView1.Columns[28].Visible = false; //是否抵额退货
                dataGridView1.Columns[29].Visible = false; //
                dataGridView1.Columns[30].Visible = false; //


                //列宽   
                dataGridView1.Columns[2].Width = 180;

            }
            catch
            {
            }
        }


        //小键盘向上
        private void UpFun()
        {
            try
            {
                //当前行数大于1行才生效
                if (dataGridView1.Rows.Count > 1)
                {
                    int rowindex_temp = dataGridView1.SelectedRows[0].Index;
                    if (rowindex_temp == 0)
                    {
                        dataGridView1.Rows[dataGridView1.Rows.Count - 1].Selected = true;
                        dataGridView1.Rows[rowindex_temp].Selected = false;

                    }
                    else
                    {
                        dataGridView1.Rows[rowindex_temp - 1].Selected = true;
                        dataGridView1.Rows[rowindex_temp].Selected = false;
                    }

                }

            }
            catch (Exception ex)
            {

                LogHelper.WriteLog("商品信息窗口小键盘向上时发生异常：" + ex);

            }
        }

        //小键盘向下
        private void DownFun()
        {
            try
            {
                //当前行数大于1行才生效
                if (dataGridView1.Rows.Count > 1)
                {
                    int rowindexDown_temp = dataGridView1.SelectedRows[0].Index;
                    if (rowindexDown_temp == dataGridView1.Rows.Count - 1)
                    {
                        dataGridView1.Rows[0].Selected = true;
                        dataGridView1.Rows[rowindexDown_temp].Selected = false;

                    }
                    else
                    {
                        dataGridView1.Rows[rowindexDown_temp + 1].Selected = true;
                        dataGridView1.Rows[rowindexDown_temp].Selected = false;
                    }

                }

            }
            catch (Exception ex)
            {

                LogHelper.WriteLog("商品信息窗口小键盘向下时发生异常：" + ex);

            }
        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            UpdateNameFunc();
        }



        //从数据库读取所有分店信息给下拉框
        private void ShowScodeFunc()
        {
            try
            {

                using (var db = new hjnbhEntities())
                {
                    var infos = db.hd_dept_info.AsNoTracking().Select(t => new { t.scode, t.cname }).ToList();
                    if (infos.Count > 0)
                    {
                        DataTable dt = new DataTable();
                        DataColumn dc1 = new DataColumn("id");
                        DataColumn dc2 = new DataColumn("name");
                        DataColumn dc3 = new DataColumn("display");
                        dt.Columns.Add(dc1);
                        dt.Columns.Add(dc2);
                        dt.Columns.Add(dc3);

                        foreach (var item in infos)
                        {
                            DataRow dr1 = dt.NewRow();
                            dr1["id"] = item.scode;
                            dr1["name"] = item.cname;
                            dr1["display"] = item.cname + "(" + item.scode + ")";

                            dt.Rows.Add(dr1);
                        }

                        comboBox1.DataSource = dt;
                        comboBox1.ValueMember = "id";  //值字段
                        //comboBox1.DisplayMember = "name"; //显示的字段
                        comboBox1.DisplayMember = "display"; //显示的字段
                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("系统设置在线读取分店信息时发生异常:", ex);
                MessageBox.Show("数据库连接出错！");
                //string tip = ConnectionHelper.ToDo();
                //if (!string.IsNullOrEmpty(tip))
                //{
                //    MessageBox.Show(tip);
                //}
            }

        }


        //查询库存
        private void button1_Click(object sender, EventArgs e)
        {
            int code_temp = 0;
            if (int.TryParse(comboBox1.SelectedValue.ToString(), out code_temp))
            {
                ShowUIInfo(code_temp);
            }
            else
            {
                MessageBox.Show("没有此分店数据！");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            EnterFunc();
        }

        private void ItemInfoForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CGForm.changed -= CGForm_changed;

        }













    }
}