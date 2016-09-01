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
    public class JiaoBanPrinter
    {

        
        private string dh = ""; //交班流水号

        public string date_ = DateTime.Now.ToString("yyyy-MM-dd hh:mm");

        public string title = "黄金牛百货连锁店"; //小票标题

        private System.Windows.Forms.PrintPreviewDialog printv_pos = null;  //打印浏览
        private System.Drawing.Printing.PrintDocument printd_pos = null;   //打印文档

        private string title2 = "";



        public JiaoBanPrinter(string dh , string title2 = "前台收银员交班凭证")
        {
            this.dh = dh;

            this.title2 = title2;


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

            //sb.Append(PadEx("黄金牛儿童百货" + HandoverModel.GetInstance.scodeName) + "\n");
            sb.Append(PadEx(title2) + "\n");

            sb.Append("= = = = = = = = = = = = = = = = = = = =\n");
            sb.Append("  交班流水: " + dh + "\n");
            sb.Append("  分 店 号: " + HandoverModel.GetInstance.scode.ToString() + "\t" + "    " + "终端号: " + HandoverModel.GetInstance.bcode.ToString() + "\n");
            sb.Append("  员工编号: " + HandoverModel.GetInstance.userID.ToString() + "\n");
            sb.Append("  交易单数: " + HandoverModel.GetInstance.OrderCount.ToString() + "\n");
            sb.Append("  当班金额: " + HandoverModel.GetInstance.SaveMoney.ToString() + "\n");
            sb.Append("  退货金额: " + HandoverModel.GetInstance.RefundMoney.ToString() + "\n");
            sb.Append("  当班时间: " + HandoverModel.GetInstance.workTime.ToString() + "\n");
            sb.Append("  交班时间: " + HandoverModel.GetInstance.ClosedTime.ToString() + "\n");

            sb.Append("----------------------------------------\n");
            sb.Append("  币  种" + "\t" + "    " + "实  收" + "\n");
            //sb.Append("\n");
            sb.Append("  现  金：" + "\t" + "    " + HandoverModel.GetInstance.CashMoney.ToString() + "\n");
            sb.Append("  银联卡：" + "\t" + "    " + HandoverModel.GetInstance.paycardMoney.ToString() + "\n");
            sb.Append("  储值卡：" + "\t" + "    " + HandoverModel.GetInstance.VipCardMoney.ToString() + "\n");
            sb.Append("  礼  券：" + "\t" + "    " + HandoverModel.GetInstance.LiQuanMoney.ToString() + "\n");

            sb.Append("----------------------------------------\n");

            sb.Append("  应交金额：" + HandoverModel.GetInstance.Money.ToString() +" 元"+ "\n");
            sb.Append("= = = = = = = = = = = = = = = = = = = =\n");

            sb.Append("  收银员签名："+ "\n");
            sb.Append("  财务签名："+ "\n");


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
