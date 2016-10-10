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
    /// <summary>
    /// 重打小票
    /// </summary>
    public partial class RePrintForm : Form
    {
        CashiersFormXP xp;

        //bool isTH = false;  //是否退货

        public RePrintForm()
        {
            InitializeComponent();
        }

        private void RePrintForm_Load(object sender, EventArgs e)
        {
            xp = this.Owner as CashiersFormXP;

            this.textBox1.Focus();
        }

        private void RePrintForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F4:
                    LastJSdhFunc();
                    break;

                case Keys.F5:
                    RePrintFunc();
                    break;

                case Keys.Escape:
                    this.Close();
                    break;
            }
        }

        /// <summary>
        /// 上单单号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            //PrintHelper ph = new PrintHelper(xp.lastGoodsList, xp.jf, xp.ysje, xp.ssje, xp.jsdh, xp.vipCradXF, xp.payXF, xp.LQXF, xp.zhaoling, xp.lastVipcard, xp.dateStr, true);
            //ph.PrintView();
            LastJSdhFunc();
        }

        /// <summary>
        /// 上单单号
        /// </summary>
        private void LastJSdhFunc()
        {
            this.textBox1.Clear();
            this.textBox1.Text = xp.jsdh;
            this.textBox1.SelectAll();
            this.textBox1.Focus();
        }

        /// <summary>
        /// 重打指定单据号小票
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {

            RePrintFunc();
        }


        /// <summary>
        /// 重打小票方法
        /// </summary>
        private void RePrintFunc()
        {

            try
            {

                var itemList = new BindingList<GoodsBuy>();   //商品列表 
                var jsDH = textBox1.Text.Trim();  //单号 
                if (!string.IsNullOrEmpty(jsDH))
                {
                    using (var db = new hjnbhEntities())
                    {
                        //结算信息
                        var JSinfo = db.hd_js.AsNoTracking().Where(t => t.v_code == jsDH).FirstOrDefault();
                        if (JSinfo != null)
                        {
                            //结算类型
                            var jsType = db.hd_js_type.AsNoTracking().Where(t => t.v_code == jsDH).ToList();
                            decimal vipCradjs = 0;  //储值卡消费
                            decimal payjs = 0;  //银行卡消费
                            decimal payAllje = 0; //银行优惠
                            decimal LQjs = 0;  //礼券消费
                            decimal weixunXF = 0.00m;  //微信消费
                            decimal zfbXF = 0.00m;  //支付宝消费额

                            if (jsType.Count > 0)
                            {
                                payjs = jsType.Where(t => t.js_type == 1).Select(t => t.je.Value).Sum();
                                LQjs = jsType.Where(t => t.js_type == 2).Select(t => t.je.Value).Sum();
                                vipCradjs = jsType.Where(t => t.js_type == 3).Select(t => t.je.Value).Sum();
                                payAllje = jsType.Where(t => t.js_type == 9).Select(t => t.je.Value).Sum();
                                weixunXF = jsType.Where(t => t.js_type == 8).Select(t => t.je.Value).Sum();
                                zfbXF = jsType.Where(t => t.js_type == 7).Select(t => t.je.Value).Sum();
                            }


                            decimal ssje = JSinfo.ssje.Value;  //实收
                            string lsDH = JSinfo.ls_code;   //零售单号

                            bool isvip = false;  //是不是会员消费
                            string vipcard = "";  //会员卡号
                            decimal vipjf = 0;  //会员积分
                            decimal viptoje = 0;  //会员抵资充值的金额(退款转储值)


                            //零售信息
                            var lsinfo = db.hd_ls.AsNoTracking().Where(t => t.v_code == lsDH).FirstOrDefault();
                            if (lsinfo != null)
                            {
                                if (lsinfo.vip.HasValue)
                                {
                                    if (lsinfo.vip.Value > 0)
                                    {
                                        vipcard = db.hd_vip_info.AsNoTracking().Where(t => t.vipcode == lsinfo.vip.Value).Select(t => t.vipcard).FirstOrDefault();
                                        isvip = true;
                                    }
                                }

                            }


                            //发生抵额退货的商品
                            var dzthitem = db.hd_ls_detail.AsNoTracking().Where(t => t.amount < 0 && t.dzth_code == jsDH).ToList();
                            if (dzthitem.Count > 0)
                            {
                                foreach (var item in dzthitem)
                                {
                                    itemList.Add(new GoodsBuy
                                    {
                                        goods = item.cname,
                                        barCodeTM = item.tm,
                                        isTuiHuo = true,
                                        vtype = item.vtype.Value,
                                        pfPrice = item.yls_price,
                                        lsPrice = item.ls_price,
                                        countNum = Math.Abs(item.amount.Value),
                                        Sum = Math.Round(item.ls_price.Value * Math.Abs(item.amount.Value), 2)
                                    });

                                }
                            }


                            //正常零售的商品详情
                            var lsmxitem = db.hd_ls_detail.AsNoTracking().Where(t => t.v_code == lsDH && t.amount > 0).ToList();
                            if (lsmxitem.Count > 0)
                            {
                                foreach (var item in lsmxitem)
                                {
                                    itemList.Add(new GoodsBuy
                                    {
                                        goods = item.cname,
                                        barCodeTM = item.tm,
                                        isZS = item.iszs > 0 ? true : false,
                                        vtype = item.vtype.Value,
                                        pfPrice = item.yls_price,
                                        lsPrice = item.ls_price,
                                        countNum = item.amount.Value,
                                        Sum = Math.Round(item.ls_price.Value * item.amount.Value, 2)
                                    });

                                }

                            }

                            //会员积分与金额情况
                            if (isvip)
                            {
                                var vipinfo = db.hd_vip_cz.AsNoTracking().Where(t => t.ckh == lsinfo.vip.Value.ToString() && t.srvoucher == jsDH).ToList();
                                if (vipinfo.Count > 0)
                                {
                                    foreach (var item in vipinfo)
                                    {

                                        if (item.fs == 7)
                                        {
                                            vipjf = item.jf.HasValue ? item.jf.Value : 0;
                                        }


                                        if (item.ctype == 3)
                                        {
                                            vipjf = item.jf.HasValue ? item.jf.Value : 0;
                                        }

                                        //退款转储值的情况
                                        if (item.ctype == 4)
                                        {
                                            viptoje = item.je.HasValue ? item.je.Value : 0;
                                        }
                                    }

                                }
                            }


                            string cid = JSinfo.cid.ToString();  //操作员
                            string scode = lsinfo.scode.ToString();  //分店号
                            decimal zhaoling = JSinfo.ssje.Value - JSinfo.ysje.Value;   //找零
                            string datetime = JSinfo.ctime.Value.ToString("yyyy-MM-dd HH:mm");  //日期

                            PrintHelper ph = new PrintHelper(itemList, vipjf, JSinfo.ysje, JSinfo.ssje, jsDH, weixunXF, zfbXF, vipCradjs, payjs, payAllje, LQjs, zhaoling, vipcard, datetime, true, cid, scode, viptoje);
                            ph.PrintView();


                        }
                        else
                        {
                            MessageBox.Show("找不到此单据号小票信息，请核实单号无误！");
                        }


                    }
                }


            }
            catch (Exception ex)
            {

                LogHelper.WriteLog("重打小票窗口查找小票时出现异常:", ex);
                MessageBox.Show("重打小票异常，请核实网络是否可用，必要时请联系管理员！");
            }

        }














    }
}
