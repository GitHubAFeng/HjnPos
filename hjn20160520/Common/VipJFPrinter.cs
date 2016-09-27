using hjn20160520.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace hjn20160520.Common
{
    /// <summary>
    /// 会员积分冲减、余额冲减打印、还款打印
    /// </summary>
    public class VipJFPrinter
    {


        private decimal vipjf = 0;  //VIP积分
        private decimal vipje = 0;  //VIP余额
        private decimal czjf = 0;   //充值积分
        private decimal czje = 0;   //充值金额
        private decimal czFqJe = 0;   //本次充值分期金额
        private decimal czDjJe = 0;   //本次充值定金金额
        private decimal FqJe = 0;   //分期金额
        private decimal DjJe = 0;   //定金金额

        public string date_ = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        public BindingList<VipItemModel> goodsList = new BindingList<VipItemModel>();  //数据源
        public decimal? discount_ = 0;   //优惠金额
        public decimal? YS_cash = 0; // 应收金额
        public decimal? recv_cash_ = 0;  // 实收金额
        public string title = "黄金牛百货连锁店"; //小票标题
        public string card_no_ = ""; // 会员卡号
        public string vipname = ""; //会员姓名
        public decimal? mark_in_ = 0; // 本次积分
        public JSType jstype; //付款方式
        private System.Windows.Forms.PrintPreviewDialog printv_pos = null;  //打印浏览
        private System.Drawing.Printing.PrintDocument printd_pos = null;   //打印文档

        public string SVIDS = "";
        public string WHIDS = "";

        private string title2;
        decimal HKJE = 0.00m;   //本次还款
        decimal QKJE = 0.00m;  //欠款


        public VipJFPrinter(decimal vipjf = 0, decimal vipje = 0, decimal czjf = 0, decimal czje = 0, string vipcard = "", string vipname = "", string title2 = "", decimal czFqJe = 0, decimal czDjJe = 0, decimal FqJe = 0, decimal DjJe = 0, decimal HKJE = 0, decimal QKJE = 0)
        {
            this.QKJE = QKJE;
            this.HKJE = HKJE;
            this.czDjJe = czDjJe;
            this.czFqJe = czFqJe;
            this.czjf = czjf;
            this.czje = czje;
            this.vipjf = vipjf;
            this.vipje = vipje;

            this.title2 = title2;
            this.vipname = vipname;
            this.card_no_ = vipcard;
            this.FqJe = FqJe;
            this.DjJe = DjJe;

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
            sb.Append(PadEx(title2) + "\n");
            sb.Append("= = = = = = = = = = = = = = = = = = = =\n");
            sb.Append("  分  店: " + HandoverModel.GetInstance.scode.ToString() + "\t" + "工  号: " + HandoverModel.GetInstance.userID.ToString() + "\n");
            sb.Append("  日  期: " + date_ + "\n");

            sb.Append("----------------------------------------\n");

            sb.Append("  会员卡号：" + card_no_ + "\n");
            sb.Append("  会员姓名：" + vipname + "\n");


            string czjftemp = "";
            if (czjf != 0)
            {
                czjftemp = czjf.ToString("0.00");
                sb.Append("  充减积分：" + czjftemp + "\n");

            }

            string vipjftemp = "";
            if (vipjf != 0)
            {
                vipjftemp = vipjf.ToString("0.00");
                sb.Append("  总 积 分：" + vipjftemp + "\n");

            }

            string qkjetemp = "";
            string hkjetemp = "";
            if (HKJE != 0)
            {
                hkjetemp = HKJE.ToString("0.00");
                qkjetemp = QKJE.ToString("0.00");

                sb.Append("  本次还款：" + hkjetemp + "\n");
                sb.Append("  尚欠金额：" + qkjetemp + "\n");

            }

            string czjetemp = "";
            if (czje != 0)
            {
                czjetemp = czje.ToString("0.00");
                sb.Append("  本次储值：" + czjetemp + "\n");
            }

            string vipjetemp = "";
            if (vipje != 0)
            {
                vipjetemp = vipje.ToString("0.00");
                sb.Append("  储值余额：" + vipjetemp + "\n");

            }



            string DjJetemp = "";
            if (DjJe != 0)
            {
                DjJetemp = DjJe.ToString("0.00");
                sb.Append("  定金余额：" + DjJetemp + "\n");
            }


            string czDjJetemp = "";
            if (czDjJe != 0)
            {
                czDjJetemp = czDjJe.ToString("0.00");
                sb.Append("  定金储值：" + czDjJetemp + "\n");

            }

            string czFqJetemp = "";
            if (czFqJe != 0)
            {
                czFqJetemp = czFqJe.ToString("0.00");
                sb.Append("  分期储值：" + czFqJetemp + "\n");

            }
            
            string FqJetemp = "";
            if (FqJe != 0)
            {
                FqJetemp = FqJe.ToString("0.00");
                sb.Append("  分期余额：" + FqJetemp + "\n");

            }


            sb.Append("----------------------------------------\n");
            if (!string.IsNullOrEmpty(HandoverModel.GetInstance.Call))
            {
                string tempStr = Regex.Replace(HandoverModel.GetInstance.Call, "(.{14})", "$1\r\n");
                sb.Append("  电话：" + tempStr + "\n");
            }

            if (!string.IsNullOrEmpty(HandoverModel.GetInstance.Address))
            {
                string tempStr = Regex.Replace(HandoverModel.GetInstance.Address, "(.{14})", "$1\r\n");
                sb.Append("  地址：" + tempStr + "\n");
            }

            if (!string.IsNullOrEmpty(HandoverModel.GetInstance.Remark1))
            {
                string tempStr = Regex.Replace(HandoverModel.GetInstance.Remark1, "(.{14})", "$1\r\n");
                sb.Append("  " + tempStr + "\n");
            }

            if (!string.IsNullOrEmpty(HandoverModel.GetInstance.Remark2))
            {
                string tempStr = Regex.Replace(HandoverModel.GetInstance.Remark2, "(.{14})", "$1\r\n");
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
            print();
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
