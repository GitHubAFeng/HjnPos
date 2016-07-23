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

        public RefundForm()
        {
            InitializeComponent();
        }

        private void RefundForm_Load(object sender, EventArgs e)
        {

        }

        private void RefundForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.Close();
                    break;
                case Keys.Enter:
                    if (Cashiers.GetInstance.isLianXi)
                    {
                        MessageBox.Show("不允许练习模式进行该操作！");
                        return;
                    }

                    try
                    {
                        THFunc();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLog("收银主界面进行退货提单时发生异常:", ex);
                        MessageBox.Show("数据库连接出错！");
                        string tip = ConnectionHelper.ToDo();
                        if (!string.IsNullOrEmpty(tip))
                        {
                            MessageBox.Show(tip);
                        }
                    }
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
                string tmtemp = textBox3.Text.Trim();  //商品条码
                //凭小票上的结算单号找到零售单 退货
                string lsdh = db.hd_js.AsNoTracking().Where(t => t.v_code == temp).Select(t => t.ls_code).FirstOrDefault();
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
                            db.SaveChanges();
                            //入库主单
                            //var THItem = new hd_in
                            //{
                            //    v_code = THNoteID,
                            //    vtype = 106, //退货
                            //    scode = HandoverModel.GetInstance.scode,
                            //    hs_code = THinfo.vip.HasValue ? THinfo.vip.Value : 0,
                            //    ywy = THinfo.ywy,
                            //    remark = textBox2.Text.Trim(),
                            //    srvoucher = THinfo.v_code,
                            //    cid = HandoverModel.GetInstance.userID,
                            //    ctime = timer
                            //};
                            //db.hd_in.Add(THItem);

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


                            ////入库明细单
                            //var THMXItem = new hd_in_detail
                            //{
                            //    v_code = THNoteID,
                            //    scode = HandoverModel.GetInstance.scode,
                            //    item_id = mxinfo.item_id,
                            //    tm = mxinfo.tm,
                            //    cname = mxinfo.cname,
                            //    spec = mxinfo.spec,
                            //    hpack_size = mxinfo.hpack_size,
                            //    unit = mxinfo.unit,
                            //    amount = mxinfo.amount,
                            //    jj_price = mxinfo.jj_price,
                            //    yjj_price = mxinfo.jj_price,
                            //    ls_price = mxinfo.ls_price,
                            //    yls_price = mxinfo.yls_price,
                            //    cid = mxinfo.cid,
                            //    ctime = mxinfo.ctime,
                            //};
                            //db.hd_in_detail.Add(THMXItem);

                            ////分店库存退货商品回仓+1
                            //int scode = HandoverModel.GetInstance.scode;  //分店号
                            //var info = db.hd_istore.Where(t => t.scode == scode && t.item_id == mxinfo.item_id).FirstOrDefault();
                            //if (info != null)
                            //{
                            //    info.amount += mxinfo.amount;
                            //}

                            //re_temp = db.SaveChanges();
                            //sp.Complete();








                            //}
                            if (re_temp > 0)
                            {
                                //if (THinfo.vip != 0)
                                //{
                                //    //退货金额
                                //    //var sum_temp = mxinfo.amount * mxinfo.ls_price;
                                //    //HandoverModel.GetInstance.RefundMoney+=mxinfo.
                                //}

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





    }
}
