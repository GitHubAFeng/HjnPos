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
using System.Windows.Forms;

namespace hjn20160520._2_Cashiers
{
    public partial class VipSaveItemForm : Form
    {
        CashiersFormXP CFxp;
        TipForm tipForm = new TipForm();

        //已存的商品
        private BindingList<VipItemModel> savedlist = new BindingList<VipItemModel>();

        //需要存的商品
        private BindingList<VipItemModel> WantSavelist = new BindingList<VipItemModel>();

        string vipname = "";  //会员名字
        int vipid = -1;
        string vipcard = "";


        public VipSaveItemForm()
        {
            InitializeComponent();
        }

        private void VipSaveItemForm_Load(object sender, EventArgs e)
        {
            CFxp = this.Owner as CashiersFormXP;

            this.ActiveControl = textBox3;
            this.textBox3.Focus();
            this.dataGridView1.DataSource = savedlist;
            this.dataGridView2.DataSource = WantSavelist;

            vipname = CFxp.lastVipName;
            vipid = CFxp.lastvipid;
            vipcard = CFxp.lastVipcard;

            if (!string.IsNullOrEmpty(CFxp.jsdh))
            {
                this.textBox3.Text = CFxp.jsdh;
                textBox3.SelectAll();

            }


            if (!string.IsNullOrEmpty(HandoverModel.GetInstance.VipCard))
            {
                textBox4.Text = HandoverModel.GetInstance.VipCard;
            }

            textBox2.Text = "1";

            cho.changed += cho_changed;

        }



        private void VipSaveItemForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    Dele();
                    break;

                //回车
                case Keys.Enter:

                    if (string.IsNullOrEmpty(textBox4.Text.Trim()))
                    {
                        MessageBox.Show("请输入会员卡号或者手机号");
                    }
                    else
                    {
                        CXFunc();

                    }
                    break;

                case Keys.F3:
                    F3Func();
                    break;
                case Keys.F2:
                    F2Func();
                    break;

                case Keys.F5:
                    textBox2.Focus();
                    textBox2.SelectAll();

                    break;

                case Keys.F4:

                    textBox1.Focus();
                    textBox1.SelectAll();

                    //if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
                    //{

                    //    if (string.IsNullOrEmpty(textBox4.Text.Trim()))
                    //    {
                    //        MessageBox.Show("请输入会员卡号或者手机号");
                    //    }
                    //    else
                    //    {
                    //        CXFunc();

                    //    }
                    //}

                    break;
                //退出
                case Keys.Escape:
                    this.Close();
                    break;


                //取上单的单号与会员
                case Keys.F1:
                    GetNoteAndVipFunc();
                    break;

                //保存
                case Keys.F7:
                    saveVipItem();
                    getVipItem(vipid);
                    break;

                case Keys.F8:
                    if (vipid == -1)
                    {
                        MessageBox.Show("请先查询会员");
                        textBox4.Focus();
                        textBox4.SelectAll();
                    }
                    else
                    {
                        getVipItem(vipid);

                    }

                    break;

                //向上键表格换行
                case Keys.Up:

                    UpFun();

                    break;

                //向下键表格换行
                case Keys.Down:

                    DownFun();

                    break;

            }
        }

        private void saveVipItem()
        {
            try
            {
                this.vipcard = this.textBox4.Text.Trim();

                if (this.vipid == -1 || string.IsNullOrEmpty(this.vipcard))
                {
                    MessageBox.Show("会员卡号不能为空");
                    return;
                }


                using (var db = new hjnbhEntities())
                {
                    foreach (var item in WantSavelist)
                    {
                        var saveinfo = new hd_vip_item
                        {
                            item_id = item.itemid,
                            tm = item.tm,
                            cname = item.cname,
                            vipcard = item.vipcard,
                            vipcode = item.vipid,
                            vipname = item.vipName,
                            scode = item.scode,
                            ctime = item.ctime,
                            cid = item.cid,
                            amount = item.count,

                        };

                        db.hd_vip_item.Add(saveinfo);

                    }

                    var re = db.SaveChanges();
                    if (re > 0)
                    {
                        VipItemPrinter printer = new VipItemPrinter(WantSavelist, vipcard, vipname, "会员存货凭证");
                        printer.StartPrint();

                        MessageBox.Show("会员商品保存成功！");
                        WantSavelist.Clear();
                    }
                    else
                    {
                        MessageBox.Show("会员商品保存失败！请核实相关信息真实性！");

                    }

                }



            }
            catch (Exception ex)
            {

                LogHelper.WriteLog("会员存货窗口保存商品时出现异常:", ex);
                MessageBox.Show("保存商品时出现异常！请联系管理员");
            }

        }

        private void F2Func()
        {
            //查询会员 
            if (string.IsNullOrEmpty(textBox3.Text.Trim()))
            {
                textBox3.Focus();
                textBox3.SelectAll();
            }
            else
            {
                getgoodslist();
                textBox3.SelectAll();
            }
        }

        private void F3Func()
        {
            //查询会员 
            if (string.IsNullOrEmpty(textBox4.Text.Trim()))
            {
                textBox4.Focus();
                textBox4.SelectAll();
            }
            else
            {
                VipCardFunc(textBox4.Text.Trim());
                label7.Text = vipname;
                if (vipid != -1)
                {
                    getVipItem(vipid);
                }
            }



        }



        //取上单的单据与会员并填入输入框中
        private void GetNoteAndVipFunc()
        {
            if (string.IsNullOrEmpty(CFxp.jsdh) || string.IsNullOrEmpty(CFxp.lastVipcard))
            {
                MessageBox.Show("查询不到记录");
            }
            else
            {
                this.textBox3.Text = CFxp.jsdh;
                this.textBox4.Text = CFxp.lastVipcard;
                this.label7.Text = CFxp.lastVipName;
            }

        }



        ChoiceGoods cho = new ChoiceGoods();

        //查询商品
        //根据条码/货号、品名查询商品
        private void CXFunc()
        {
            try
            {
                #region
                string temptxt = textBox1.Text.Trim();
                if (string.IsNullOrEmpty(temptxt))
                {
                    tipForm.Tiplabel.Text = "请输入需要查找的商品条码!";
                    tipForm.ShowDialog();
                    return;
                }

                if (string.IsNullOrEmpty(textBox2.Text.Trim()))
                {
                    MessageBox.Show("请输入需要存入的商品数量！");
                    textBox2.Focus();
                    return;
                }

                decimal count_temp = 1;
                decimal.TryParse(textBox2.Text.Trim(), out count_temp);

                int itemid_temp = -1;
                int.TryParse(temptxt, out itemid_temp);

                var BuyListTemp = new BindingList<GoodsBuy>();   //缓存

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
                            t.pp
                        })
                        .ToList();

                    //全部放入缓存先
                    if (itemsInfo.Count > 0)
                    {
                        if (itemsInfo.Count > 10)
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
                            //再查打包商品
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
                                    //isVip = VipID == 0 ? false : true,
                                    PP = item != null ? item.pp : "",
                                    LB = item != null ? item.lb_code : 0,
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
                                    //isVip = VipID == 0 ? false : true,
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

                        //放上选择列表
                        foreach (var item in BuyListTemp)
                        {
                            cho.ChooseList.Add(new GoodsBuy
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
                                hyPrice = item.hyPrice,
                                status = item.status,
                                pfPrice = item.pfPrice,
                                isVip = item.isVip,
                                PP = item.PP,
                                LB = item.LB,
                                isDbItem = item.isDbItem,
                                countNum = count_temp

                            });
                        }

                        cho.ShowDialog();


                    }
                    else
                    {
                        //一件也没有找到
                        MessageBox.Show("没有查找到该商品!");

                    }

                #endregion


                    #region 旧代码
                    //    var itemsInfo = db.v_xs_item_info.AsNoTracking().Where(t => t.tm.Contains(temptxt) || t.cname.Contains(temptxt) || t.item_id == itemid_temp)
                    //        //.Where(t => t.scode == HandoverModel.GetInstance.scode)
                    //        .Select(t => new
                    //            {
                    //                t.item_id,
                    //                t.tm,
                    //                t.cname,
                    //                t.dw,
                    //                t.spec,
                    //                t.scode,
                    //                t.jj_price,
                    //                t.ls_price,
                    //                t.hy_price,
                    //                t.lb_code,
                    //                t.pp
                    //            })
                    //        .ToList();


                    //    //如果查出数据不至一条就弹出选择窗口，否则直接显示出来
                    //    if (itemsInfo.Count == 0)
                    //    {
                    //        this.textBox1.Focus();
                    //        this.textBox1.SelectAll();
                    //        MessageBox.Show("没有查找到该商品!");

                    //        return;
                    //    }

                    //    #region 查到多条记录时
                    //        //查询到多条则弹出商品选择窗口，排除表格在正修改时发生判断

                    //        if (itemsInfo.Count > 10)
                    //        {

                    //            if (DialogResult.No == MessageBox.Show("查询到多个类似的商品，数据量较大时可能造成几秒的卡顿，是否继续查询？", "提醒", MessageBoxButtons.YesNo))
                    //            {
                    //                return;
                    //            }
                    //        }

                    //        foreach (var item in itemsInfo)
                    //        {

                    //            #region 商品状态、批发价、厂家、拼音、单位编号
                    //            var itemsOtherInfo = db.hd_item_info.AsNoTracking().Where(t => t.item_id == item.item_id)
                    //                    .Select(t => new
                    //                    {
                    //                        t.unit,
                    //                        t.py,
                    //                        t.manufactory,
                    //                        t.status,
                    //                        t.pf_price,
                    //                    })
                    //                    .FirstOrDefault();

                    //            #endregion

                    //            cho.ChooseList.Add(new GoodsBuy
                    //            {
                    //                noCode = item.item_id,
                    //                barCodeTM = item.tm,
                    //                goods = item.cname,
                    //                spec = item.spec,
                    //                unitStr = item.dw,
                    //                lsPrice = Convert.ToDecimal(item.ls_price),
                    //                hyPrice = Convert.ToDecimal(item.hy_price),
                    //                pinYin = itemsOtherInfo.py,
                    //                countNum = count_temp
                    //            });

                    //        }


                    //    if (cho.ChooseList.Count > 0)
                    //    {

                    //        cho.ShowDialog();
                    //    }

                    #endregion

                }
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("会员存货商品查询窗口查询商品时出现异常:", e);
                MessageBox.Show("数据库连接出错！");
            }
        }

        //放入将存放列表
        void cho_changed(GoodsBuy goods)
        {

            decimal count_temp = 1;
            if (decimal.TryParse(textBox2.Text.Trim(), out count_temp))
            {

                if (VipCardFunc())
                {
                    label7.Text = vipname;

                    var vipitem = new VipItemModel
                    {
                        cid = HandoverModel.GetInstance.userID,
                        tm = goods.barCodeTM,
                        itemid = goods.noCode,
                        cname = goods.goods,
                        ctime = System.DateTime.Now,
                        count = count_temp,
                        vipcard = vipcard,
                        vipid = vipid,
                        vipName = vipname,
                        scode = HandoverModel.GetInstance.scode

                    };

                    this.WantSavelist.Add(vipitem);
                }

                textBox1.SelectAll();

            }
            else
            {
                MessageBox.Show("请输入正确的数量");
                textBox2.Focus();
                textBox2.SelectAll();
            }






        }


        public bool VipCardFunc(string card = "")
        {
            try
            {
                string text_temp = string.Empty;
                if (card != "")
                {
                    text_temp = card;
                }
                else
                {
                    if (!string.IsNullOrEmpty(textBox4.Text.Trim()))
                    {
                        text_temp = textBox4.Text.Trim();
                    }

                }

                if (string.IsNullOrEmpty(text_temp))
                {
                    return false;
                }

                using (var db = new hjnbhEntities())
                {
                    //手机号也能查
                    var vipInfos = db.hd_vip_info.AsNoTracking().Where(t => t.vipcard == text_temp || t.tel == text_temp)
                        .Select(t => new { t.vipname, t.vipcard, t.vipcode }).FirstOrDefault();


                    if (vipInfos != null)
                    {
                        this.vipcard = vipInfos.vipcard;
                        this.vipid = vipInfos.vipcode;
                        this.vipname = vipInfos.vipname;

                        return true;
                    }
                    else
                    {
                        tipForm = new TipForm();
                        tipForm.Tiplabel.Text = "查询失败，请核实会员卡号是否正确！";
                        tipForm.ShowDialog();
                    }


                }
                return false;
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("会员录入窗口登记时出现异常:", e);
                MessageBox.Show("数据库连接出错！");
                return false;
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
                dataGridView2.Columns[3].HeaderText = "数量";
                dataGridView2.Columns[9].HeaderText = "时间";

                //隐藏      
                dataGridView2.Columns[4].Visible = false;
                dataGridView2.Columns[5].Visible = false;
                dataGridView2.Columns[6].Visible = false;
                dataGridView2.Columns[7].Visible = false;
                dataGridView2.Columns[8].Visible = false;
                //dataGridView2.Columns[9].Visible = false;
            }
            catch
            {
            }
        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                //列名
                dataGridView1.Columns[0].HeaderText = "货号";
                dataGridView1.Columns[1].HeaderText = "条码";
                dataGridView1.Columns[2].HeaderText = "品名";
                dataGridView1.Columns[3].HeaderText = "数量";
                dataGridView1.Columns[9].HeaderText = "时间";

                //隐藏      
                dataGridView1.Columns[4].Visible = false;
                dataGridView1.Columns[5].Visible = false;
                dataGridView1.Columns[6].Visible = false;
                dataGridView1.Columns[7].Visible = false;
                dataGridView1.Columns[8].Visible = false;
                //dataGridView1.Columns[9].Visible = false;

            }
            catch
            {
            }
        }




        private void VipSaveItemForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            cho.changed -= cho_changed;
        }


        private void Dele()
        {
            try
            {
                //当前行数大于1行时删除选中行后把往上一行设置为选中状态
                if (dataGridView2.Rows.Count > 0)
                {
                    DialogResult RSS = MessageBox.Show(this, "确定要删除选中的商品？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    switch (RSS)
                    {
                        case DialogResult.Yes:

                            int DELindex_temp = dataGridView2.SelectedRows[0].Index;
                            //dataGridView_CashiersFormXP.Rows.RemoveAt(DELindex_temp);

                            //string de_temp = dataGridView_CashiersFormXP.CurrentRow.Cells[2].Value.ToString();

                            WantSavelist.RemoveAt(DELindex_temp);
                            dataGridView2.Refresh();

                            if (DELindex_temp - 1 >= 0)
                            {
                                dataGridView2.Rows[DELindex_temp - 1].Selected = true;
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




        private void getgoodslist(string node = "")
        {

            if (node == "")
            {
                if (string.IsNullOrEmpty(textBox3.Text.Trim()))
                {
                    MessageBox.Show("请输入小票单据号");
                    textBox3.Focus();
                    textBox3.SelectAll();
                }
                else
                {
                    node = textBox3.Text.Trim();
                }
            }

            if (string.IsNullOrEmpty(node)) return;

            using (var db = new hjnbhEntities())
            {
                var ls = db.hd_js.AsNoTracking().Where(t => t.v_code == node).Select(t => t.ls_code).FirstOrDefault();
                if (ls != null)
                {
                    var lslist = db.hd_ls_detail.AsNoTracking().Where(e => e.v_code == ls && e.amount > 0).ToList();
                    if (lslist.Count > 0)
                    {
                        foreach (var item in lslist)
                        {
                            WantSavelist.Add(new VipItemModel
                            {
                                itemid = item.item_id.HasValue ? item.item_id.Value : 0,
                                tm = item.tm,
                                cname = item.cname,
                                count = item.amount.HasValue ? item.amount.Value : 0,
                                scode = HandoverModel.GetInstance.scode,
                                vipcard = vipcard,
                                vipid = vipid,
                                cid = HandoverModel.GetInstance.userID,
                                ctime = System.DateTime.Now,
                                vipName = vipname

                            });
                        }

                        dataGridView2.Refresh();
                    }


                }



            }




        }





        //查看已存
        private void getVipItem(int id)
        {
            savedlist.Clear();
            using (var db = new hjnbhEntities())
            {
                var infolist = db.hd_vip_item.AsNoTracking().Where(e => e.vipcode == id && e.amount > 0).ToList();
                if (infolist.Count > 0)
                {
                    foreach (var item in infolist)
                    {
                        savedlist.Add(new VipItemModel
                        {
                            itemid = item.item_id,
                            tm = item.tm,
                            count = item.amount,
                            cname = item.cname,
                            ctime = item.ctime.Value

                        });

                    }

                    dataGridView1.Refresh();
                }
            }
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

                LogHelper.WriteLog("会员存货小键盘向上时发生异常：" + ex);

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

                LogHelper.WriteLog("会员存货小键盘向下时发生异常：" + ex);

            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' && e.KeyChar != '.' || e.KeyChar > '9' && e.KeyChar != '.' || ((TextBox)(sender)).Text.IndexOf('.') >= 0 && e.KeyChar == '.') && e.KeyChar != (char)13 && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            GetNoteAndVipFunc();

        }

        private void button5_Click(object sender, EventArgs e)
        {
            Dele();

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox4.Text.Trim()))
            {
                MessageBox.Show("请输入会员卡号或者手机号");
            }
            else
            {
                CXFunc();

            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            saveVipItem();
            getVipItem(vipid);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            F3Func();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            F2Func();

        }







    }
}
