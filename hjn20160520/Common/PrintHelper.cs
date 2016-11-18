using hjn20160520.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace hjn20160520.Common
{
    /// <summary>
    /// 直接数据打印小票
    /// </summary>
    public class PrintHelper
    {

        public string saild_id_; //结算单
        public string date_ = DateTime.Now.ToString("yyyy-MM-dd HH:mm");  //目前时间
        private string date = "";

        public BindingList<GoodsBuy> goodsList = new BindingList<GoodsBuy>();  //数据源
        public decimal? discount_ = 0;   //优惠金额
        public decimal? YS_cash = 0; // 应收金额
        public decimal? recv_cash_ = 0;  // 实收金额
        public string title = "黄金牛儿童百货"; //小票标题
        public string card_no_ = ""; // 会员卡号
        public decimal? mark_in_ = 0; // 本次积分
        private decimal? zhaoling;  //找零钱
        private System.Windows.Forms.PrintPreviewDialog printv_pos = null;  //打印浏览
        private System.Drawing.Printing.PrintDocument printd_pos = null;   //打印文档

        private string cidStr = "";  //重打小票用的操作员
        private string scodeStr = "";  //重打小票用的分店号
        public string SVIDS = "";
        public string WHIDS = "";
        private decimal vipcardXF = 0.00m;  //储卡消费额
        private decimal paycardXF = 0.00m;  //银行卡消费额
        private decimal payYHje = 0.00m; //银行优惠
        private decimal lqXF = 0.00m;  //礼券消费额
        private decimal weixunXF = 0.00m;  //微信消费额
        private decimal zfbXF = 0.00m;  //支付宝消费额
        private decimal vipToJe = 0.00m;  //会员抵额退款转存入储值金额
        private decimal QKJE = 0.00m; //每单欠款金额
        private bool isRePrint = false; //是否重打

        string[] vipinfos = new string[6]; //0会员名字 ，1总积分，2电话，3储卡余额，4定金余额，5分期余额

        public PrintHelper(BindingList<GoodsBuy> goodsList, decimal? jf, decimal? ysje, decimal? ssje, string jsdh, decimal weixunXF, decimal zfbXF, decimal vipcardXF, decimal paycardXF, decimal payYHje, decimal lqXF, decimal? zhaoling, decimal QKJE,string[] vipinfos, string vip = "", string date = "", bool isRePrint = false, string cidStr = "", string scodeStr = "", decimal vipToJe = 0)
        {
            this.QKJE = QKJE;
            this.vipToJe = vipToJe;
            this.zfbXF = zfbXF;
            this.weixunXF = weixunXF;
            this.payYHje = payYHje;
            this.scodeStr = scodeStr;
            this.cidStr = cidStr;
            this.lqXF = lqXF;
            this.paycardXF = paycardXF;
            this.vipcardXF = vipcardXF;
            this.date = date;
            this.isRePrint = isRePrint;
            this.goodsList = goodsList;
            this.mark_in_ = jf;
            this.YS_cash = ysje;  //应收，就是商品总价
            this.recv_cash_ = ssje;  //所收现金,就是客人给我的，还未找零的
            this.saild_id_ = jsdh;  //结算单
            this.zhaoling = zhaoling;
            this.card_no_ = vip;
            this.vipinfos = vipinfos;

            this.printv_pos = new System.Windows.Forms.PrintPreviewDialog();  //打印浏览
            this.printd_pos = new System.Drawing.Printing.PrintDocument();


            this.printv_pos.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.printv_pos.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.printv_pos.ClientSize = new System.Drawing.Size(400, 300); //工作区大小
            this.printv_pos.Document = this.printd_pos;
            this.printv_pos.Enabled = true;
            this.printv_pos.Name = "打印浏览测试";
            this.printv_pos.Visible = false;  //打印浏览不可见

            this.printd_pos.DocumentName = "黄金牛POS小票";
            this.printd_pos.OriginAtMargins = true;
            //打印事件
            this.printd_pos.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.PrintPage);

        }

        //转换像素
        private int getYc(double cm)
        {
            return (int)(cm / 25.4) * 100;
        }


        //查询会员信息
        //private string[] queryVipinfo()
        //{
        //    string[] vipinfos = new string[6];

        //    try
        //    {
        //        using (var db = new hjnbhEntities())
        //        {
        //            var vipinfo = db.hd_vip_info.AsNoTracking().Where(t => t.vipcard == card_no_)
        //                .Select(t => new { t.tel, t.vipname, t.jfnum, t.czk_ye,t.ydje }).FirstOrDefault();
        //            if (vipinfo != null)
        //            {
        //                vipinfos[0] = vipinfo.vipname;
        //                vipinfos[1] = vipinfo.jfnum.HasValue ? vipinfo.jfnum.Value.ToString("0.00") : "";
        //                vipinfos[2] = vipinfo.tel;
        //                vipinfos[3] = vipinfo.czk_ye.HasValue ? vipinfo.czk_ye.Value.ToString("0.00") : "";
        //                vipinfos[4] = vipinfo.ydje.HasValue ? vipinfo.ydje.Value.ToString("0.00") : "";
        //            }

        //            decimal Fqje = 0; //分期总金额
        //            var fqinfo = db.hd_vip_fq.AsNoTracking().Where(t => t.vipcard == card_no_ && t.amount > 0).ToList();
        //            if (fqinfo.Count > 0)
        //            {
        //                foreach (var item in fqinfo)
        //                {
        //                    decimal temp = item.mqje.HasValue ? item.mqje.Value : 0;
        //                    temp *= item.amount.Value;
        //                    Fqje += temp;
        //                }
        //                vipinfos[5] = Fqje.ToString("0.00");
        //            }

        //        }

        //    }
        //    catch
        //    {

        //    }
        //    return vipinfos;

        //}

        //取得打印文档,打印模板 
        private string GetPrintStr()
        {

            StringBuilder sb = new StringBuilder();

            string tit = "";

            if (!string.IsNullOrEmpty(HandoverModel.GetInstance.PrintTitle))
            {
                tit = HandoverModel.GetInstance.PrintTitle;
            }
            else
            {
                tit = title + HandoverModel.GetInstance.scodeName;
            }

            sb.Append("\n");

            sb.Append(PadEx(tit) + "\n");

            sb.Append("= = = = = = = = = = = = = = = = = = = =\n");

            if (isRePrint)
            {
                sb.Append("  单号:" + this.saild_id_ + "\t" + "分店:" + scodeStr + "\n");
                sb.Append("  日期:" + date + "\t" + "工号:" + cidStr + "\n");
            }
            else
            {
                sb.Append("  单号:" + this.saild_id_ + "\t" + "分店:" + HandoverModel.GetInstance.scode.ToString() + "\n");
                sb.Append("  日期:" + date + "\t" + "工号:" + HandoverModel.GetInstance.userID.ToString() + "\n");
            }


            //sb.Append("  商品编号" + "\t" + "品名" + "\t" + "数量" + "\t" + "金额" + "\n");
            sb.Append("  " + "品名" + "\t" + "                " + "数量" + "\t" + "金额" + "\n");
            sb.Append("---------------------------------------\n");

            decimal count_temp = 0.00m; //合计数量
            decimal sum = 0.00m; //合计总金额
            decimal HJSum = 0.00m;  //用于输出大写金额（要取正数，负数不能转换，所以就这样了）
            decimal TuiHuoJe = 0.00m; //抵额退货金额

            for (int i = 0; i < goodsList.Count; i++)
            {
                int k = i + 1;
                string temp = GetFirstString(goodsList[i].goods, 12);
                string zs = GetFirstString(goodsList[i].goods, 10);

                string name = PadRightEx(temp, 18, ' ');   //一个参数时默认填充空格
                string zsname = PadRightEx(zs, 12, ' ');   //只是显示长度不同

                if (goodsList[i].isZS)
                {
                    sb.Append("  条码：  " + goodsList[i].barCodeTM + "\n");

                    //现在的pfPrice已经全部被我标记为原价
                    sb.Append("  原价：  " + goodsList[i].pfPrice.ToString() + "\n");

                    if (goodsList[i].vtype == 10)
                    {
                        sb.Append(k.ToString() + " " + "限促：" + zsname + "\t" + goodsList[i].countNum.ToString("0.00") + "  " + goodsList[i].Sum.ToString("0.00") + "\n");

                    }
                    else if (goodsList[i].vtype != 0)
                    {
                        sb.Append(k.ToString() + " " + "赠送：" + zsname + "\t" + goodsList[i].countNum.ToString("0.00") + "  " + goodsList[i].Sum.ToString("0.00") + "\n");
                    }

                }
                //抵额退货
                else if (goodsList[i].isTuiHuo)
                {

                    TuiHuoJe += goodsList[i].Sum;

                    sb.Append("  条码：  " + goodsList[i].barCodeTM + "\n");
                    sb.Append(k.ToString() + " " + "退货：" + zsname + "\t" + goodsList[i].countNum.ToString("0.00") + "  " + "-" + goodsList[i].Sum.ToString("0.00") + "\n");

                }

                else
                {
                    sb.Append("  条码：  " + goodsList[i].barCodeTM + "\n");
                    sb.Append(k.ToString() + " " + name + "\t" + goodsList[i].countNum.ToString("0.00") + "  " + goodsList[i].Sum.ToString("0.00") + "\n");

                }

                count_temp += goodsList[i].countNum;

                if (goodsList[i].Sum > 0)
                {
                    sum += goodsList[i].Sum;
                }

            }

            sum -= (TuiHuoJe + lqXF);
            HJSum = Math.Abs(sum);
            //统计sum时候已经把负数的礼券- lqXF 减去了，这里没有再减
            decimal xianjin = sum - vipcardXF - paycardXF  - payYHje - zfbXF - weixunXF;  //现金消费

            sb.Append("\n");

            sb.Append("  总 数 量：" + count_temp.ToString("0.00") + "\n");
            sb.Append("  总 金 额：" + sum.ToString("0.00") + "\n");
            if (vipcardXF > 0)
            {
                sb.Append("  储　　卡：" + vipcardXF.ToString("0.00") + "\n");
            }

            if (lqXF > 0)
            {
                sb.Append("  礼　　券：" + lqXF.ToString("0.00") + "\n");
            }

            if (paycardXF > 0)
            {
                if (payYHje > 0)
                {
                    sb.Append("  银 联 卡：" + paycardXF.ToString("0.00") + " &" + "优惠返现：" + payYHje.ToString("0.00") + "\n");
                }
                else
                {
                    sb.Append("  银 联 卡：" + paycardXF.ToString("0.00") + "\n");
                }
            }


            if (zfbXF > 0)
            {
                sb.Append("  " + "支付宝付款：" + zfbXF.ToString("0.00") + "\n");
            }

            if (weixunXF > 0)
            {
                sb.Append("  " + "微信支付：" + weixunXF.ToString("0.00") + "\n");
            }

            if (QKJE > 0)
            {
                sb.Append("  " + "欠款金额：" + QKJE.ToString("0.00") + "\n");
            }

            sb.Append("  " + "现　　金：" + xianjin.ToString() + "\n");
            //sb.Append("  " + "付款总额：" + recv_cash_.Value.ToString("0.00") + "\n");  //1026厚爱 说不用显示
            sb.Append("  " + "找　　零：" + zhaoling.Value.ToString("0.00") + "\n");

            if (TuiHuoJe != 0)
            {
                sb.Append("  " + "退货金额：" + TuiHuoJe.ToString() + "\n");
            }

            //大写金额
            sb.Append("  合计金额：" + NumGetString.NumGetStr(HJSum) + "\n");

            if (!string.IsNullOrEmpty(card_no_))
            {

                sb.Append("  会员名字：" + vipinfos[0] + "\n");

                sb.Append("  会员卡号：" + card_no_ + "\n");
                //转存储值
                if (vipToJe > 0)
                {
                    sb.Append("  本次储值：" + vipToJe.ToString("0.00") + "\n");

                }
                sb.Append("  储卡余额：" + vipinfos[3] + "\n");
                sb.Append("  分期余额：" + vipinfos[5] + "\n");
                sb.Append("  定金余额：" + vipinfos[4] + "\n");

                sb.Append("  累计积分：" + vipinfos[1] + "\n");

                sb.Append("  本次积分：" + mark_in_ + "\n");
                sb.Append("  会员电话：" + vipinfos[2] + "\n");
            }


            if (isRePrint)
            {
                sb.Append("*************** 重打小票 ***************\n");
                sb.Append("  重打时间：" + date_ + "\n");
            }
            else
            {
                sb.Append("---------------------------------------\n");
            }

            if (!string.IsNullOrEmpty(HandoverModel.GetInstance.Call))
            {
                string tempStr = Regex.Replace(HandoverModel.GetInstance.Call, "(.{16})", "$1\r\n" + "  ");
                sb.Append("  电话：" + tempStr + "\n");
            }

            if (!string.IsNullOrEmpty(HandoverModel.GetInstance.Address))
            {
                string tempStr = Regex.Replace(HandoverModel.GetInstance.Address, "(.{16})", "$1\r\n" + "  ");
                sb.Append("  地址：" + tempStr + "\n");
            }

            if (!string.IsNullOrEmpty(HandoverModel.GetInstance.Remark1))
            {
                string tempStr = Regex.Replace(HandoverModel.GetInstance.Remark1, "(.{16})", "$1\r\n" + "  ");
                sb.Append("  " + tempStr + "\n");
            }

            if (!string.IsNullOrEmpty(HandoverModel.GetInstance.Remark2))
            {
                string tempStr = Regex.Replace(HandoverModel.GetInstance.Remark2, "(.{16})", "$1\r\n" + "  ");
                sb.Append("  " + tempStr + "\n");
            }

            if (!string.IsNullOrEmpty(HandoverModel.GetInstance.Remark3))
            {
                string tempStr = Regex.Replace(HandoverModel.GetInstance.Remark3, "(.{16})", "$1\r\n" + "  ");
                sb.Append("  " + tempStr + "\n");
            }

            if (!string.IsNullOrEmpty(HandoverModel.GetInstance.Remark4))
            {
                string tempStr = Regex.Replace(HandoverModel.GetInstance.Remark4, "(.{16})", "$1\r\n" + "  ");
                sb.Append("  " + tempStr + "\n");
            }


            return sb.ToString();
        }


        /// <summary>
        /// POS打印
        /// </summary>
        private void print()
        {
            this.printd_pos.PrintController = new System.Drawing.Printing.StandardPrintController();
            this.printd_pos.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(PrintPage);

            //设置边距
            System.Drawing.Printing.Margins margins = new System.Drawing.Printing.Margins(5, 5, 5, 5);
            this.printd_pos.DefaultPageSettings.Margins = margins;
            //页面大小(name,宽，高)
            this.printd_pos.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("First custom size", HandoverModel.GetInstance.PageWidth, HandoverModel.GetInstance.PageHeight);
            //this.printd_pos.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("First custom size", getYc(76), 600);

            //this.printDocument1.PrinterSettings.PrinterName = "";
            //Margins margins = new Margins(
            //打开打印浏览
            //this.printv_pos.Document = this.printd_pos;
            //printv_pos.PrintPreviewControl.AutoZoom = false;
            //printv_pos.PrintPreviewControl.Zoom = 1;
            // this.printv_pos.ShowDialog(win);

            short rr = Convert.ToInt16(this.printd_pos.PrinterSettings.MaximumCopies);  //最大支持数量
            if (HandoverModel.GetInstance.PrintCopies > rr)
            {
                this.printd_pos.PrinterSettings.Copies = rr;   //设置打印数量，超过最大则以最大的
                System.Windows.Forms.MessageBox.Show("该品牌打印机支持打印份数最多为 " + rr.ToString());
            }
            else
            {
                var tt = HandoverModel.GetInstance.PrintCopies;
                if (tt <= 0) tt = 1;
                this.printd_pos.PrinterSettings.Copies = tt;   //设置打印数量，超过最大则以最大的

            }
            
              
            printd_pos.Print();


        }

        //编写页面内容
        private void PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            //内容
            string strFile = GetPrintStr();

            //打印字体样式
            Font ft = new Font(HandoverModel.GetInstance.PrintFont, HandoverModel.GetInstance.FontSize, FontStyle.Regular);
            Point pt = new Point(0, 0);
            g.DrawString(strFile, ft, new SolidBrush(Color.Black), pt);
        }

        //(如果默认打印机是输出图片，那在打印时会弹出 另存为 对话框)
        public void StartPrint()
        {
            if (HandoverModel.GetInstance.isPrint)
            {
                print();
            }
            //this.goodsList = goodsList;
            //print();
        }


        //打印浏览
        public void PrintView()
        {
            this.printd_pos.PrintController = new System.Drawing.Printing.StandardPrintController();
            this.printd_pos.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(PrintPage);

            //设置边距
            System.Drawing.Printing.Margins margins = new System.Drawing.Printing.Margins(5, 5, 5, 5);
            this.printd_pos.DefaultPageSettings.Margins = margins;
            //页面大小(name,宽，高)
            this.printd_pos.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("First custom size", HandoverModel.GetInstance.PageWidth, HandoverModel.GetInstance.PageHeight);
            //this.printd_pos.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("First custom size", getYc(76), 600);

            //this.printDocument1.PrinterSettings.PrinterName = "";
            //Margins margins = new Margins(
            //打开打印浏览
            this.printv_pos.Document = this.printd_pos;
            printv_pos.PrintPreviewControl.AutoZoom = false;
            printv_pos.PrintPreviewControl.Zoom = 1;
            this.printv_pos.ShowDialog();
        }



        /// <summary>
        /// 截取中英文的字符串
        /// </summary>
        /// <param name="stringToSub"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public string GetFirstString(string stringToSub, int length)
        {
            Regex regex = new Regex("[\u4e00-\u9fa5]+", RegexOptions.Compiled);
            char[] stringChar = stringToSub.ToCharArray();
            StringBuilder sb = new StringBuilder();
            int nLength = 0;
            bool isCut = false;
            for (int i = 0; i < stringChar.Length; i++)
            {
                if (regex.IsMatch((stringChar[i]).ToString()))
                {
                    sb.Append(stringChar[i]);
                    nLength += 2;
                }
                else
                {
                    sb.Append(stringChar[i]);
                    nLength = nLength + 1;
                }

                if (nLength > length)
                {
                    isCut = true;
                    break;
                }
            }
            if (isCut)
                return sb.ToString() + "..";   //填充后部
            else
                return sb.ToString() + "    ";   //填充
        }

        /// <summary>
        /// 填充字符串为固定长度
        /// </summary>
        /// <param name="str"></param>
        /// <param name="totalByteCount"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private string PadRightEx(string str, int totalByteCount, char c)
        {
            Encoding coding = Encoding.GetEncoding("gb2312");
            int dcount = 0;
            foreach (char ch in str.ToCharArray())
            {
                if (coding.GetByteCount(ch.ToString()) == 2)
                    dcount++;
            }
            string w = str.PadRight(totalByteCount - dcount, c);
            return w;
        }


        /// <summary>
        /// 左右填充到固定长度，目的是居中
        /// </summary>
        private string PadEx(string str, int totalByteCount = 30)
        {
            totalByteCount += 10 - str.Length;

            int temp = (totalByteCount - str.Length) / 2;
            int temp2 = temp + str.Length;
            string w = str.PadLeft(temp2);
            return w;
        }


    }
}
