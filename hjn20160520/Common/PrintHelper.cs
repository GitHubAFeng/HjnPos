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
    public class PrintHelper
    {
        public string saild_id_; //结算单
        public string date_ = DateTime.Now.ToString("yyyy-MM-dd hh:mm");
        //public DataTable datas_ = new DataTable(); //数据源
        public BindingList<GoodsBuy> goodsList = new BindingList<GoodsBuy>();  //数据源
        public decimal? discount_ = 0;   //优惠金额
        public decimal? YS_cash = 0; // 应收金额
        public decimal? recv_cash_ = 0;  // 实收金额
        public string title = "黄金牛百货连锁店"; //小票标题
        public string card_no_ = ""; // 会员卡号
        public decimal? mark_in_ = 0; // 本次积分
        public JSType jstype; //付款方式
        private string Strjstype; //付款方式转换中文
        private decimal? zhaoling;  //找零钱
        private System.Windows.Forms.PrintPreviewDialog printv_pos = null;  //打印浏览
        private System.Drawing.Printing.PrintDocument printd_pos = null;   //打印文档

        public string SVIDS = "";
        public string WHIDS = "";

        public PrintHelper(decimal? jf, decimal? ysje, decimal? ssje, string jsdh, JSType jstype, decimal? zhaoling)
        {

            //this.goodsList = goodsList;
            this.mark_in_ = jf;
            this.YS_cash = ysje;
            this.recv_cash_ = ssje;
            this.saild_id_ = jsdh;  //结算单
            this.jstype = jstype;
            this.zhaoling = zhaoling;

            switch ((int)jstype)
            {
                case 0:
                    Strjstype = "现金";
                    break;
                case 1:
                    Strjstype = "银联卡";
                    break;
            }

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

            sb.Append("*********" + title + "***********\n");

            sb.Append("  单  号:" + this.saild_id_ + "  " + "分店:" + HandoverModel.GetInstance.scode.ToString() + "\n");
            sb.Append("  日  期:" + date_ + "   " + "工号:" + HandoverModel.GetInstance.userID.ToString() + "\n");

            sb.Append("  商品编号" + "\t" + "品名" + "\t" + "数量" + "\t" + "金额" + "\n");

            int count_temp = 0; //合计数量
            //品名设置每7字换一行
            foreach (var item in goodsList)
            {
                sb.Append("  " + item.noCode.ToString() + "\t" + Regex.Replace(item.goods + " ", "(.{7})", "$1\r\n\t")
                   + "\t" + "\t" + item.countNum.ToString() + "\t" + item.Sum.ToString() + "\n");

                count_temp += item.countNum;
            }

            card_no_ = Cashiers.GetInstance.VipID.ToString();

            sb.Append("\n");

            //sb.Append("  优惠金额：" + discount_ + "\n");
            sb.Append("  购买件数：" + count_temp.ToString() + "\t" + "应收金额：" + YS_cash + "\n");
            sb.Append("  付款方式：" + Strjstype  + "\n");
            sb.Append("  " + "付款金额：" + recv_cash_ + "\t" + "    找零：" + zhaoling.ToString() + "\n");
            //大写金额
            sb.Append("  合计金额：" + NumGetString.NumGetStr(recv_cash_.Value) + "\n");
            sb.Append("  会员卡号：" + card_no_ + "\t" + "本次积分：" + mark_in_ + "\n");
            sb.Append("***************************************\n");
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
        public void StartPrint(BindingList<GoodsBuy> goodsList)
        {
            this.goodsList = goodsList;
            print();
        }


    }
}
