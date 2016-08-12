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
    public class VipItemPrinter
    {

        public string saild_id_; //结算单
        public string date_ = DateTime.Now.ToString("yyyy-MM-dd hh:mm");
        //public DataTable datas_ = new DataTable(); //数据源
        public BindingList<VipItemModel> goodsList = new BindingList<VipItemModel>();  //数据源
        public decimal? discount_ = 0;   //优惠金额
        public decimal? YS_cash = 0; // 应收金额
        public decimal? recv_cash_ = 0;  // 实收金额
        public string title = "黄金牛百货连锁店"; //小票标题
        public string card_no_ = ""; // 会员卡号
        public string vipname = ""; //会员姓名
        public decimal? mark_in_ = 0; // 本次积分
        public JSType jstype; //付款方式
        //private string Strjstype; //付款方式转换中文
        //private decimal? zhaoling;  //找零钱
        private System.Windows.Forms.PrintPreviewDialog printv_pos = null;  //打印浏览
        private System.Drawing.Printing.PrintDocument printd_pos = null;   //打印文档

        public string SVIDS = "";
        public string WHIDS = "";

        private string title2;

        public VipItemPrinter(BindingList<VipItemModel> goodsList, string vipcard = "", string vipname = "" , string title2 = "")
        {

            this.goodsList = goodsList;
            this.title2 = title2;
            this.vipname = vipname;
            this.card_no_ = vipcard;

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
            if (!string.IsNullOrEmpty(HandoverModel.GetInstance.scodeName))
            {
                title = HandoverModel.GetInstance.scodeName;
            }

            sb.Append("\t" +"\t"+ title + "\t"+"\n");
            sb.Append("\t" + "\t" + title2 + "\t" + "\n");
            sb.Append("= = = = = = = = = = = = = = = = = = = =\n");
            sb.Append("  分  店: " + HandoverModel.GetInstance.scode.ToString() + "\t" + "工  号: " + HandoverModel.GetInstance.userID.ToString() + "\n");
            sb.Append("  日  期: " + date_ + "\n");

            sb.Append("  " + "品  名" + "\t" + "               " + "数  量" + "\t" + "\n");
            sb.Append("----------------------------------------\n");

            decimal count_temp = 0; //合计数量


            for (int i = 0; i < goodsList.Count; i++)
            {
                int k = i + 1;
                string temp = GetFirstString(goodsList[i].cname, 28);

                string name = PadRightEx(temp, 28, ' ');   //一个参数时默认填充空格


                sb.Append("  条码：  " + goodsList[i].tm + "\n");
                sb.Append(k.ToString() + " " + name + "\t" + goodsList[i].count.ToString() + "\n");

                count_temp += goodsList[i].count;


            }


            sb.Append("\n");

            sb.Append("  总 数 量：" + count_temp.ToString()  + "\n");

            sb.Append("  会员卡号：" + card_no_ + "\n");
            sb.Append("  会员姓名：" + vipname + "\n");

            sb.Append("----------------------------------------\n");

            string myfoot = string.Format("  {0}\n", "欢迎下次光临！");
            sb.Append(myfoot);
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
            //this.goodsList = goodsList;
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






    }
}
