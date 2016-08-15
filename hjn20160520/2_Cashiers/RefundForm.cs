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
        //信息提示窗口
        TipForm tipForm;


        //已购商品列表
        private BindingList<TuiHuoItemModel> buyedList = new BindingList<TuiHuoItemModel>();
        //需要退的商品列表 
        private BindingList<TuiHuoItemModel> tuihuoList = new BindingList<TuiHuoItemModel>();




        public RefundForm()
        {
            InitializeComponent();
        }

        private void RefundForm_Load(object sender, EventArgs e)
        {
            this.ActiveControl = textBox1;
            textBox1.Focus();

            this.dataGridView1.DataSource = buyedList;
            this.dataGridView2.DataSource = tuihuoList;


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
                    if (string.IsNullOrEmpty(textBox1.Text.Trim()))
                    {
                        textBox1.Focus();
                    }
                    else
                    {
                        FindBuyedItem();
                    }

                    break;

                case Keys.F4:
                    textBox7.Focus();

                    break;


                case Keys.F5:
                    textBox4.Focus();
                    break;

                case Keys.Delete:

                    Dele();
                    break;

                case Keys.Up:
                    UpFun();
                    break;

                case Keys.Down:
                    DownFun();
                    break;

                case Keys.Add:
                    UpdataCount();
                    break;

                case Keys.F7:
                    TuiHuoFunc();
                    break;
            }
        }


        //处理单品退货
        private void THFunc()
        {
            //1、根据条码查相应的零售明细单
            //2、查到此明细单后在th_flag字段做退货标志
            //3、以此退货商品新建入库主单与入库明细单
            if (string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                tipForm = new TipForm();
                tipForm.Tiplabel.Text = "请输入商品条码！";
                tipForm.ShowDialog();
                textBox1.SelectAll();
                return;
            }
            using (var db = new hjnbhEntities())
            {
                string temp = textBox1.Text.Trim();  //结算单
                string tmtemp = textBox7.Text.Trim();  //商品条码
                var jsInfo = db.hd_js.Where(t => t.v_code == temp).FirstOrDefault();
                //凭小票上的结算单号找到零售单 退货
                string lsdh = jsInfo.ls_code;
                if (lsdh != null)
                {

                    //商品信息

                    var mxinfo = db.hd_ls_detail.Where(t => t.tm == tmtemp && t.v_code == lsdh).FirstOrDefault();
                    if (mxinfo != null)
                    {
                        //单号计算方式，当前时间+00000+id
                        long no_temp = Convert.ToInt64(System.DateTime.Now.ToString("yyyyMMdd") + "000000");
                        string THNoteID = "LSR" + (no_temp + mxinfo.id).ToString();//获取退货入库单号
                        //查此是否已经有退货记录
                        if (mxinfo.th_flag != 1)
                        {

                            //DateTime timer = System.DateTime.Now; //统一成单时间
                            //using (var sp = new TransactionScope())
                            //{
                            mxinfo.th_flag = 1;  //退货标志
                            jsInfo.remark = textBox2.Text.Trim();  //备注
                            db.SaveChanges();


                            //获取办理退货的商品零售单信息
                            var THinfo = db.hd_ls.Where(t => t.v_code == lsdh).FirstOrDefault();
                            var pf = db.hd_item_info.AsNoTracking().Where(t => t.tm == tmtemp).Select(t => t.pf_price).FirstOrDefault();

                            int re_temp = 0;

                            #region SQL操作退货

                            var sqlTh = new SqlParameter[]
                        {
                            new SqlParameter("@v_code", THNoteID), 
                            new SqlParameter("@scode", HandoverModel.GetInstance.scode),
                            new SqlParameter("@vtype", 103),
                            new SqlParameter("@hs_code",  THinfo.vip.HasValue ? THinfo.vip.Value : 0), 
                            new SqlParameter("@ywy",  THinfo.ywy),
                            new SqlParameter("@srvoucher", THinfo.v_code),
                            new SqlParameter("@remark", textBox2.Text.Trim()),
                            new SqlParameter("@cid", HandoverModel.GetInstance.userID)

                        };

                            db.Database.ExecuteSqlCommand("EXEC [dbo].[Create_in] @v_code,@scode,@vtype,@hs_code,@ywy,@srvoucher,@remark,@cid,1,0", sqlTh);


                            var sqlThMX = new SqlParameter[]
                        {
                            new SqlParameter("@v_code", THNoteID), 
                            new SqlParameter("@scode", HandoverModel.GetInstance.scode),
                            new SqlParameter("@vtype", 103),
                            //new SqlParameter("@lid", 0),  这个不知是什么
                            new SqlParameter("@item_id",  mxinfo.item_id),
                            new SqlParameter("@tm", mxinfo.tm),
                            new SqlParameter("@amount", mxinfo.amount),
                            new SqlParameter("@jj_price", mxinfo.jj_price),
                            new SqlParameter("@ls_price",  mxinfo.ls_price),
                            new SqlParameter("@pf_price", pf.HasValue?pf:0),
                            new SqlParameter("@remark", "零售退货"),
                            new SqlParameter("@cid", HandoverModel.GetInstance.userID)
                        };

                            re_temp = db.Database.ExecuteSqlCommand("EXEC [dbo].[create_in_detail] @v_code,@scode,@vtype,0,@item_id,@tm,@amount,@jj_price,@ls_price,@pf_price,@remark,@cid,1,0", sqlThMX);



                            #endregion

                            if (re_temp > 0)
                            {
                                //退货金额
                                decimal sum_temp = Convert.ToDecimal(mxinfo.amount * mxinfo.ls_price);
                                HandoverModel.GetInstance.RefundMoney += sum_temp;

                                tipForm = new TipForm();
                                tipForm.Tiplabel.Text = "退货登记成功！";
                                tipForm.ShowDialog();
                                textBox1.SelectAll();
                            }
                            else
                            {
                                MessageBox.Show("退货数据登记失败！");
                            }
                        }
                        else
                        {
                            MessageBox.Show("操作失败，因为该单据商品存在退货记录！");
                        }

                    }
                    else
                    {
                        tipForm = new TipForm();
                        tipForm.Tiplabel.Text = "查询的商品不存在！";
                        tipForm.ShowDialog();
                        textBox1.SelectAll();
                    }
                }
                else
                {
                    MessageBox.Show("查无此单！");
                }
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
                //dataGridView1.Columns[9].HeaderText = "零售价";
                //dataGridView1.Columns[10].HeaderText = "会员价";
                dataGridView1.Columns[7].HeaderText = "金额";
                //dataGridView1.Columns[12].HeaderText = "拼音";
                //dataGridView1.Columns[13].HeaderText = "备注";
                //dataGridView1.Columns[14].HeaderText = "营业员";

                //隐藏
                dataGridView1.Columns[5].Visible = false;  //单位编码
                //dataGridView1.Columns[8].Visible = false;  //进价
                //dataGridView1.Columns[12].Visible = false;  //拼音
                //dataGridView1.Columns[15].Visible = false;
                //dataGridView1.Columns[16].Visible = false;
                //dataGridView1.Columns[17].Visible = false; //折扣
                //dataGridView1.Columns[18].Visible = false; //批发价
                //dataGridView1.Columns[19].Visible = false; //活动商品标志
                //dataGridView1.Columns[20].Visible = false; //VIP标志
                //dataGridView1.Columns[21].Visible = false; //限购标志
                //dataGridView1.Columns[22].Visible = false; //活动类型
                //dataGridView1.Columns[23].Visible = false;  //业务
                //dataGridView1.Columns[24].Visible = false; //品牌
                //dataGridView1.Columns[25].Visible = false; //类别

                //列宽
                //dataGridView1.Columns[0].Width = 30;
                //dataGridView1.Columns[1].Width = 80;
                //dataGridView1.Columns[2].Width = 130;  //条码
                //dataGridView1.Columns[3].Width = 260;  //品名
                //单元格文字色
                //dataGridView_Cashiers.Columns[13].DefaultCellStyle.ForeColor = Color.Red;  //备注列

                ////禁止编辑单元格
                ////设置单元格是否可以编辑
                //for (int i = 0; i < dataGridView1.Columns.Count; i++)
                //{
                //    if (dataGridView1.Columns[i].Index != 5)
                //    {
                //        dataGridView1.Columns[i].ReadOnly = true;
                //    }
                //}
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
                //dataGridView2.Columns[9].HeaderText = "零售价";
                //dataGridView2.Columns[10].HeaderText = "会员价";
                dataGridView2.Columns[7].HeaderText = "金额";
                //dataGridView2.Columns[12].HeaderText = "拼音";
                //dataGridView2.Columns[13].HeaderText = "备注";
                //dataGridView2.Columns[14].HeaderText = "营业员";

                //隐藏
                dataGridView2.Columns[5].Visible = false;  //单位编码
                //dataGridView2.Columns[8].Visible = false;  //进价
                //dataGridView2.Columns[12].Visible = false;  //拼音
                //dataGridView2.Columns[15].Visible = false;
                //dataGridView2.Columns[16].Visible = false;
                //dataGridView2.Columns[17].Visible = false; //折扣
                //dataGridView2.Columns[18].Visible = false; //批发价
                //dataGridView2.Columns[19].Visible = false; //活动商品标志
                //dataGridView2.Columns[20].Visible = false; //VIP标志
                //dataGridView2.Columns[21].Visible = false; //限购标志
                //dataGridView2.Columns[22].Visible = false; //活动类型
                //dataGridView2.Columns[23].Visible = false;  //业务
                //dataGridView2.Columns[24].Visible = false; //品牌
                //dataGridView2.Columns[25].Visible = false; //类别

                //列宽
                //dataGridView2.Columns[0].Width = 30;
                //dataGridView2.Columns[1].Width = 80;
                //dataGridView2.Columns[2].Width = 130;  //条码
                //dataGridView2.Columns[3].Width = 260;  //品名
                //单元格文字色
                //dataGridView_Cashiers.Columns[13].DefaultCellStyle.ForeColor = Color.Red;  //备注列

                //禁止编辑单元格
                //设置单元格是否可以编辑
                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    //允许修改金额
                    if (dataGridView2.Columns[i].Index != 7)
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

                        var buyinfo = db.hd_ls_detail.AsNoTracking().Where(t => t.v_code == jsinfo.ls_code && t.amount > 0).ToList();
                        if (buyinfo.Count > 0)
                        {
                            buyedList.Clear(); //清空上次查询

                            foreach (var item in buyinfo)
                            {
                                #region 商品单位查询
                                //需要把单位编号转换为中文以便UI显示
                                int unitID = item.unit.HasValue ? (int)item.unit : 1;
                                string dw = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == unitID).Select(t => t.txt1).FirstOrDefault();
                                #endregion

                                buyedList.Add(new TuiHuoItemModel
                                {
                                    goods = item.cname,
                                    noCode = item.item_id.Value,
                                    barCodeTM = item.tm,
                                    Sum = jsinfo.je.Value,
                                    countNum = item.amount.Value,
                                    spec = item.spec,
                                    unit = unitID,
                                    unitStr = dw

                                });
                            }

                            dataGridView1.Refresh();

                        }

                    }
                    else
                    {
                        MessageBox.Show("没有查询到此单据，请确认输入的单号是否正确");
                    }

                }




            }
            else
            {
                MessageBox.Show("请输入小票单号");
                textBox1.Focus();
                textBox1.SelectAll();
            }

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
                            //dataGridView_Cashiers.Rows.RemoveAt(DELindex_temp);

                            //string de_temp = dataGridView_Cashiers.CurrentRow.Cells[2].Value.ToString();

                            tuihuoList.RemoveAt(DELindex_temp);
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

                var getitem = buyedList.Where(t => t.barCodeTM.Contains(temptxt) || t.goods.Contains(temptxt) || t.noCode == itemid_temp).FirstOrDefault();
                if (getitem != null)
                {
                    if (getitem.countNum < count_temp)
                    {
                        MessageBox.Show("退货数量不能大于已购数量，请重新输入！");
                        textBox4.Focus();
                        textBox4.SelectAll();
                    }
                    else
                    {

                        getitem.countNum -= count_temp;

                        var goods = new TuiHuoItemModel
                        {
                            noCode = getitem.noCode,
                            barCodeTM = getitem.barCodeTM,
                            goods = getitem.goods,
                            countNum = count_temp,
                            Sum = getitem.Sum,
                            unitStr = getitem.unitStr,
                            spec = getitem.spec
                        };

                        var itemed = tuihuoList.Where(e => e.noCode == goods.noCode).FirstOrDefault();
                        if (itemed != null)
                        {
                            itemed.countNum += goods.countNum;
                        }
                        else
                        {
                            tuihuoList.Add(goods);
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
                MessageBox.Show("数据库连接出错！");
            }
        }


        //按+号修改金额
        private void UpdataCount()
        {
            if (tuihuoList.Count > 0)
            {
                try
                {
                    dataGridView2.CurrentCell = dataGridView2.SelectedRows[0].Cells[7];
                    dataGridView2.BeginEdit(true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("修改金额异常");
                    LogHelper.WriteLog("收银主界面按+号修改商品数量发生异常:", ex);
                }
            }

        }






        string JSDH = string.Empty;  //结算单号
        string LSDH = string.Empty;  //零售单号
        string vipcard = "";  //会员卡
        string vipname = "";  //会员名字


        //保存退货
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


            using (var db = new hjnbhEntities())
            {
                //商品信息
                foreach (var item in tuihuoList)
                {

                    var jsInfo = db.hd_js.Where(t => t.v_code == JSDH).FirstOrDefault();
                    if (jsInfo == null) continue;

                    var mxinfo = db.hd_ls_detail.Where(t => t.item_id == item.noCode && t.v_code == LSDH && t.amount > 0).FirstOrDefault();
                    if (mxinfo != null)
                    {
                        //单号计算方式，当前时间+00000+id
                        long no_temp = Convert.ToInt64(System.DateTime.Now.ToString("yyyyMMdd") + "000000");
                        string THNoteID = "LSR" + (no_temp + mxinfo.id).ToString();//获取退货入库单号
                        //查此是否已经有退货记录
                        if (mxinfo.th_flag != 1)
                        {
                            //获取办理退货的商品零售单信息
                            var THinfo = db.hd_ls.Where(t => t.v_code == LSDH).FirstOrDefault();
                            if (THinfo == null) continue;
                            var pf = db.hd_item_info.AsNoTracking().Where(t => t.item_id == item.noCode).Select(t => t.pf_price).FirstOrDefault();
                            mxinfo.amount -= item.countNum;
                            mxinfo.th_flag = 1;  //退货标志
                            if (!string.IsNullOrEmpty(textBox2.Text.Trim()))
                            {
                                jsInfo.remark += textBox2.Text.Trim();  //备注
                            }

                            //管理会员积分
                            if (THinfo.vip.HasValue)
                            {
                                //查会员
                                var vipinfo = db.hd_vip_info.Where(e => e.vipcode == THinfo.vip).FirstOrDefault();
                                decimal tempjf = item.Sum / 10;
                                vipinfo.jfnum -= tempjf;

                                vipname = vipinfo.vipname;
                                vipcard = vipinfo.vipcard;


                            }

                            db.SaveChanges();


                            //存储过程返回状态码
                            int re_temp = 0;

                            #region SQL操作退货

                            var sqlTh = new SqlParameter[]
                        {
                            new SqlParameter("@v_code", THNoteID), 
                            new SqlParameter("@scode", HandoverModel.GetInstance.scode),
                            new SqlParameter("@vtype", 103),
                            new SqlParameter("@hs_code",  THinfo.vip.HasValue ? THinfo.vip.Value : 0), 
                            new SqlParameter("@ywy",  THinfo.ywy),
                            new SqlParameter("@srvoucher", THinfo.v_code),
                            new SqlParameter("@remark", textBox2.Text.Trim()),
                            new SqlParameter("@cid", HandoverModel.GetInstance.userID)

                        };

                            db.Database.ExecuteSqlCommand("EXEC [dbo].[Create_in] @v_code,@scode,@vtype,@hs_code,@ywy,@srvoucher,@remark,@cid,1,0", sqlTh);


                            var sqlThMX = new SqlParameter[]
                        {
                            new SqlParameter("@v_code", THNoteID), 
                            new SqlParameter("@scode", HandoverModel.GetInstance.scode),
                            new SqlParameter("@vtype", 103),
                            //new SqlParameter("@lid", 0),  这个不知是什么
                            new SqlParameter("@item_id",  mxinfo.item_id),
                            new SqlParameter("@tm", mxinfo.tm),
                            new SqlParameter("@amount", mxinfo.amount),
                            new SqlParameter("@jj_price", mxinfo.jj_price),
                            new SqlParameter("@ls_price",  mxinfo.ls_price),
                            new SqlParameter("@pf_price", pf.HasValue?pf:0),
                            new SqlParameter("@remark", "零售退货"),
                            new SqlParameter("@cid", HandoverModel.GetInstance.userID)
                        };

                            re_temp = db.Database.ExecuteSqlCommand("EXEC [dbo].[create_in_detail] @v_code,@scode,@vtype,0,@item_id,@tm,@amount,@jj_price,@ls_price,@pf_price,@remark,@cid,1,0", sqlThMX);





                            #endregion

                            if (re_temp > 0)
                            {
                                //退货金额
                                decimal sum_temp = Convert.ToDecimal(mxinfo.amount * mxinfo.ls_price);
                                HandoverModel.GetInstance.RefundMoney += sum_temp;

                                //tipForm = new TipForm();
                                //tipForm.Tiplabel.Text = "退货登记成功！";
                                //tipForm.ShowDialog();
                                MessageBox.Show("退货登记成功！");
                                textBox1.SelectAll();

                                //打小票
                                TuiHuoPrinter printer = new TuiHuoPrinter(tuihuoList, vipcard, vipname, "客户退货单据", THNoteID);
                                printer.StartPrint();

                            }
                            else
                            {
                                MessageBox.Show("退货数据登记失败！");
                            }
                        }
                        else
                        {
                            MessageBox.Show("操作失败，因为该单据商品存在退货记录！");
                        }

                    }

                }
            }


        }




    }
}
