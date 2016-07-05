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
                    THFunc();
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
                var mxinfo = db.hd_ls_detail.Where(t => t.tm == textBox1.Text.Trim()).FirstOrDefault();
                if (mxinfo != null)
                {
                    //单号计算方式，当前时间+00000+id
                    long no_temp = Convert.ToInt64(System.DateTime.Now.ToString("yyyyMMdd") + "000000");
                    string THNoteID = "THD" + (no_temp + mxinfo.id).ToString();//获取退货入库单号
                    //获取办理退货的商品信息
                    var THinfo = db.hd_ls.Where(t => t.v_code == mxinfo.v_code).FirstOrDefault();
                    DateTime timer = System.DateTime.Now; //统一成单时间

                    mxinfo.th_flag = 1;  //退货标志

                    //入库主单
                    var THItem = new hd_in
                    {
                        v_code=THNoteID,
                        vtype=106, //退货
                        scode = HandoverModel.GetInstance.scode,
                        hs_code = THinfo.vip.HasValue ? THinfo.vip.Value : 0,
                        ywy = THinfo.ywy,
                        remark=textBox2.Text.Trim(),
                        srvoucher = THinfo.v_code,
                        cid=HandoverModel.GetInstance.userID,
                        ctime = timer
                    };
                    db.hd_in.Add(THItem);

                    //入库明细单
                    var THMXItem = new hd_in_detail
                    {
                        v_code = THNoteID,
                        scode = HandoverModel.GetInstance.scode,
                        item_id = mxinfo.item_id,
                        tm = mxinfo.tm,
                        cname = mxinfo.cname,
                        spec = mxinfo.spec,
                        hpack_size = mxinfo.hpack_size,
                        unit = mxinfo.unit,
                        amount = mxinfo.amount,
                        jj_price = mxinfo.jj_price,
                        yjj_price = mxinfo.jj_price,
                        ls_price = mxinfo.ls_price,
                        yls_price = mxinfo.yls_price,
                        cid = mxinfo.cid,
                        ctime = mxinfo.ctime,
                    };
                    db.hd_in_detail.Add(THMXItem);

                    //分店库存退货商品回仓+1
                    int scode = HandoverModel.GetInstance.scode;  //分店号
                    var info = db.hd_istore.Where(t => t.scode == scode && t.item_id == mxinfo.item_id).FirstOrDefault();
                    if (info != null)
                    {
                        info.amount += mxinfo.amount;
                    }

                   var re = db.SaveChanges();
                   if (re > 0)
                   {
                       tipForm = new TipForm();
                       tipForm.Tiplabel.Text = "退货登记成功！";
                       tipForm.ShowDialog();
                       textBox1.SelectAll();
                   }
                   else
                   {
                       MessageBox.Show("退货失败！");
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



        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {

        }





    }
}
