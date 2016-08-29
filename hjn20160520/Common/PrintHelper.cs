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
    /// 直接数据打印小票（排版不灵活）
    /// </summary>
    public class PrintHelper
    {
        public string saild_id_; //结算单
        public string date_ = DateTime.Now.ToString("yyyy-MM-dd hh:mm");  //目前时间
        private string date = "";
        //public DataTable datas_ = new DataTable(); //数据源
        public BindingList<GoodsBuy> goodsList = new BindingList<GoodsBuy>();  //数据源
        public decimal? discount_ = 0;   //优惠金额
        public decimal? YS_cash = 0; // 应收金额
        public decimal? recv_cash_ = 0;  // 实收金额
        public string title = "黄金牛儿童百货"; //小票标题
        public string card_no_ = ""; // 会员卡号
        public decimal? mark_in_ = 0; // 本次积分
        //public JSType jstype; //付款方式
        //private string Strjstype; //付款方式转换中文
        private decimal? zhaoling;  //找零钱
        private System.Windows.Forms.PrintPreviewDialog printv_pos = null;  //打印浏览
        private System.Drawing.Printing.PrintDocument printd_pos = null;   //打印文档

        public string SVIDS = "";
        public string WHIDS = "";
        private decimal vipcardXF = 0.00m;  //储卡消费额
        private decimal paycardXF = 0.00m;  //银行卡消费额
        private decimal lqXF = 0.00m;  //礼券消费额

        //private string cejsStr = ""; //结算方式
        private bool isRePrint = false; //是否重打

        public PrintHelper(BindingList<GoodsBuy> goodsList, decimal? jf, decimal? ysje, decimal? ssje, string jsdh, decimal vipcardXF,decimal paycardXF,decimal lqXF, decimal? zhaoling, string vip = "", string date = "", bool isRePrint = false)
        {
            this.lqXF = lqXF;
            this.paycardXF = paycardXF;
            this.vipcardXF = vipcardXF;
            this.date = date;
            this.isRePrint = isRePrint;
            this.goodsList = goodsList;
            this.mark_in_ = jf;
            this.YS_cash = ysje;
            this.recv_cash_ = ssje;
            this.saild_id_ = jsdh;  //结算单
            //this.jstype = jstype;
            this.zhaoling = zhaoling;
            this.card_no_ = vip;

            //this.cejsStr = cejsStr;
            //switch ((int)jstype)
            //{
            //    case 0:
            //        Strjstype = "现金";
            //        break;
            //    case 1:
            //        Strjstype = "银联卡";
            //        break;
            //    case 2:
            //        Strjstype = "礼券";
            //        break;
            //    case 3:
            //        Strjstype = "储值卡";
            //        break;
            //}

            this.printv_pos = new System.Windows.Forms.PrintPreviewDialog();  //打印浏览
            this.printd_pos = new System.Drawing.Printing.PrintDocument();


            this.printv_pos.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.printv_pos.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.printv_pos.ClientSize = new System.Drawing.Size(400, 300); //工作区大小
            this.printv_pos.Document = this.printd_pos;
            this.printv_pos.Enabled = true;
            this.printv_pos.Name = "打印浏览测试";
            this.printv_pos.Visible = false;  //打印浏览不可见
            // 
            // printd_pos
            // 
            this.printd_pos.DocumentName = "黄金牛POS小票";
            this.printd_pos.OriginAtMargins = true;
            //打印事件
            this.printd_pos.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.PrintPage);


            //datas_.Clear();
        }

        //转换像素
        private int getYc(double cm)
        {
            return (int)(cm / 25.4) * 100;
        }

        //取得打印文档,打印模板  
        private string GetPrintStr()
        {
            StringBuilder sb = new StringBuilder();
            //if (!string.IsNullOrEmpty(HandoverModel.GetInstance.scodeName))
            //{
            //    title = HandoverModel.GetInstance.scodeName;
            //}

            string tit = title + HandoverModel.GetInstance.scodeName;

            sb.Append(PadEx(tit) + "\n");

            //sb.Append("\t" +"\t"+ title + "\t"+"\n");
            //sb.Append(PadEx(title) + "\n");
            //sb.Append(PadEx(title2) + "\n");
            sb.Append("= = = = = = = = = = = = = = = = = = = =\n");

            sb.Append("  单  号:" + this.saild_id_ + "  " + "分店:" + HandoverModel.GetInstance.scode.ToString() + "\n");
            sb.Append("  日  期:" + date + "   " + "工号:" + HandoverModel.GetInstance.userID.ToString() + "\n");

            //sb.Append("  商品编号" + "\t" + "品名" + "\t" + "数量" + "\t" + "金额" + "\n");
            sb.Append("  " + "品名" + "\t" + "                " + "数量" + "\t" + "金额" + "\n");
            sb.Append("---------------------------------------\n");

            decimal count_temp = 0.00m; //合计数量
            decimal sum = 0.00m; //合计总金额

            for (int i = 0; i < goodsList.Count; i++)
            {
                int k = i + 1;
                string temp = GetFirstString(goodsList[i].goods, 16);
                string zs = GetFirstString(goodsList[i].goods, 10);

                string name = PadRightEx(temp, 18, ' ');   //一个参数时默认填充空格
                string zsname = PadRightEx(zs, 12, ' ');

                if (goodsList[i].isZS)
                {
                    sb.Append("  条码：  " + goodsList[i].barCodeTM + "\n");

                    //因为为活动5的赠品原价对应字段与其它的不一样
                    if (goodsList[i].vtype !=0)
                    {
                        sb.Append("  原价：  " + goodsList[i].pfPrice.ToString() + "\n");
                    }
                    else
                    {
                        sb.Append("  原价：  " + goodsList[i].lsPrice.ToString() + "\n");
                    }

                    sb.Append(k.ToString() + " " + "赠送：" + zsname + "\t" + goodsList[i].countNum.ToString() + "\t" + goodsList[i].Sum.ToString() + "\n");
                }
                else
                {
                    sb.Append("  条码：  " + goodsList[i].barCodeTM + "\n");
                    sb.Append(k.ToString() + " " + name + "\t" + goodsList[i].countNum.ToString() + "\t" + goodsList[i].Sum.ToString() + "\n");

                }

                count_temp += goodsList[i].countNum;
                sum += goodsList[i].Sum.Value;

            }


            sb.Append("\n");

            sb.Append("  总 数 量：" + count_temp.ToString() +"\n");
            sb.Append("  总 金 额：" + sum.ToString() + "\n");
            sb.Append("  储　　卡：" + vipcardXF.ToString() + "\n");
            sb.Append("  礼　　券：" + lqXF.ToString() + "\n");
            sb.Append("  银 联 卡：" + paycardXF.ToString() + "\n");

            //sb.Append("  " + "付款金额：" + recv_cash_.ToString() + "\t" + "找零：" + zhaoling.ToString() + "\n");
            sb.Append("  " + "付款金额：" + recv_cash_.ToString() + "\n");
            sb.Append("  " + "找　　零：" + zhaoling.ToString() + "\n");
            //大写金额
            sb.Append("  合计金额：" + NumGetString.NumGetStr(recv_cash_.Value) + "\n");
            sb.Append("  会员卡号：" + card_no_ + "\n");
            sb.Append("  本次积分：" + mark_in_ + "\n");
            if (isRePrint)
            {
                sb.Append("*************** 重打小票 ***************\n");
                sb.Append("  重打时间：" + date_ + "\n");
            }
            else
            {
                sb.Append("---------------------------------------\n");
                string myfoot = string.Format("  {0}\n", "欢迎下次光临！");
                sb.Append(myfoot);

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
            this.printd_pos.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("First custom size", 240, 600);
            //this.printd_pos.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("First custom size", getYc(76), 600);

            //this.printDocument1.PrinterSettings.PrinterName = "";
            //Margins margins = new Margins(
            //打开打印浏览
            //this.printv_pos.Document = this.printd_pos;
            //printv_pos.PrintPreviewControl.AutoZoom = false;
            //printv_pos.PrintPreviewControl.Zoom = 1;
            // this.printv_pos.ShowDialog(win);


            printd_pos.Print();


        }

        //编写页面内容
        private void PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            //内容
            string strFile = GetPrintStr();

            //打印字体样式
            Font ft = new Font("宋体", 8.5F, FontStyle.Regular);
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
                return sb.ToString()+"    ";   //填充
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
