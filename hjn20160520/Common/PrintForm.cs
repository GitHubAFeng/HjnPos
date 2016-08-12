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
using System.Windows.Forms;

namespace hjn20160520.Common
{
    /// <summary>
    /// 小票打印排版
    /// </summary>
    public partial class PrintForm : Form
    {

        private System.Windows.Forms.PrintPreviewDialog printv_pos = new System.Windows.Forms.PrintPreviewDialog();  //打印浏览
        private System.Drawing.Printing.PrintDocument printd_pos = new System.Drawing.Printing.PrintDocument();   //打印文档


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

        public string SVIDS = "";
        public string WHIDS = "";

        public PrintForm(BindingList<GoodsBuy> goodsList,decimal? jf = 1, decimal? ysje = 1, decimal? ssje= 1, string jsdh= "空", JSType jstype = JSType.Cash, decimal? zhaoling = 1,string vipcard="")
        {
            InitializeComponent();
            //this.printv_pos = new System.Windows.Forms.PrintPreviewDialog();  //打印浏览
            //this.printd_pos = new System.Drawing.Printing.PrintDocument();

            this.goodsList = goodsList;
            this.mark_in_ = jf;
            this.YS_cash = ysje;
            this.recv_cash_ = ssje;
            this.saild_id_ = jsdh;  //结算单
            this.jstype = jstype;
            this.zhaoling = zhaoling;

            this.card_no_ = vipcard;


            switch ((int)jstype)
            {
                case 0:
                    Strjstype = "现金";
                    break;
                case 1:
                    Strjstype = "银联卡";
                    break;
            }

        }

        private void PrintForm_Load(object sender, EventArgs e)
        {
            //this.tableLayoutPanel1.AutoSize = true;
            //this.tableLayoutPanel1.MaximumSize = new Size(250, 1000);

            //this.printv_pos = new System.Windows.Forms.PrintPreviewDialog();  //打印浏览
            //this.printd_pos = new System.Drawing.Printing.PrintDocument();
            //打印事件
            this.printd_pos.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.PrintPage);

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

            PrintSetSrt();
            StartPrint();
        }

        //编写页面内容
        private void PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            //Graphics g = e.Graphics;
            ////内容
            //string strFile = GetPrintStr();

            ////打印字体样式
            //Font ft = new Font("宋体", 8.5F, FontStyle.Regular);
            //Point pt = new Point(0, 0);
            //g.DrawString(strFile, ft, new SolidBrush(Color.Black), pt);


            Bitmap _NewBitmap = new Bitmap(tableLayoutPanel1.Width, tableLayoutPanel1.Height);
            tableLayoutPanel1.DrawToBitmap(_NewBitmap, new Rectangle(0, 0, _NewBitmap.Width, _NewBitmap.Height));
            e.Graphics.DrawImage(_NewBitmap, 0, 0, _NewBitmap.Width, _NewBitmap.Height);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //设置边距
            System.Drawing.Printing.Margins margins = new System.Drawing.Printing.Margins(5, 5, 5, 5);
            this.printd_pos.DefaultPageSettings.Margins = margins;
            //页面大小(name,宽，高)
            this.printd_pos.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("First custom size", 240, 600);

            //PrintSetSrt();

            printd_pos.Print();
        }


        public void StartPrint()
        {
            //设置边距
            System.Drawing.Printing.Margins margins = new System.Drawing.Printing.Margins(5, 5, 5, 5);
            this.printd_pos.DefaultPageSettings.Margins = margins;
            //页面大小(name,宽，高)
            this.printd_pos.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("First custom size", 240, 600);


            printd_pos.Print();
        }


        private void PrintSetSrt()
        {
            if (!string.IsNullOrEmpty(HandoverModel.GetInstance.scodeName))
            {
                label1.Text = HandoverModel.GetInstance.scodeName;   //标题
            }
            else
            {
                label1.Text = "黄金牛百货连锁店";
            }

            label4.Text = this.saild_id_; //单号
            label38.Text = HandoverModel.GetInstance.userID.ToString();  //工号
            label7.Text = date_;  //日期
            label37.Text = HandoverModel.GetInstance.scode.ToString();  //分店

            //StringBuilder itemidStr = new StringBuilder();
            //StringBuilder nameStr = new StringBuilder();
            //StringBuilder countStr = new StringBuilder();
            //StringBuilder sumStr = new StringBuilder();
            int count_temp = 0; //合计数量

            for (int i = 0; i < goodsList.Count; i++)
            {
                var itemidStr = new Label(); //商品号
                var nameStr = new Label();     //品名
                var countSt = new Label();     //数量
                var sumStr = new Label();     //金额

                itemidStr.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                nameStr.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                countSt.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                sumStr.Anchor = AnchorStyles.Left | AnchorStyles.Top;

                itemidStr.Font = new System.Drawing.Font("宋体", 8);
                nameStr.Font = new System.Drawing.Font("宋体", 8);
                countSt.Font = new System.Drawing.Font("宋体", 8);
                sumStr.Font = new System.Drawing.Font("宋体", 8);

                tableLayoutPanel2.RowCount = goodsList.Count;    //设置分成几行  
                tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  

                itemidStr.Text = goodsList[i].noCode.ToString();
                nameStr.Text = goodsList[i].goods;
                countSt.Text = goodsList[i].countNum.ToString();
                sumStr.Text = goodsList[i].Sum.ToString();

                tableLayoutPanel2.Controls.Add(itemidStr, 0, i);
                tableLayoutPanel2.Controls.Add(nameStr, 1, i);
                tableLayoutPanel2.Controls.Add(countSt, 2, i);
                tableLayoutPanel2.Controls.Add(sumStr, 3, i);

                count_temp += goodsList[i].countNum;
            }

            //foreach (var item in goodsList)
            //{

            //    itemidStr.Append(item.noCode.ToString() + "\r\n");
            //    nameStr.Append(item.goods + "\r\n");
            //    countStr.Append(item.countNum.ToString() + "\r\n");
            //    sumStr.Append(item.Sum.ToString() + "\r\n");

            //    count_temp += item.countNum;
            //}


            //label5.Text = itemidStr.ToString();  //商品号
            //label11.Text = nameStr.ToString();  //品名
            //label9.Text = countStr.ToString();  //数量
            //label13.Text = sumStr.ToString();  //金额

            label21.Text = recv_cash_.ToString();   //付款金额
            label19.Text = count_temp.ToString(); //购买件数
            label23.Text = YS_cash.ToString();  //应付金额
            label15.Text = Strjstype;  //付款方式
            label27.Text = recv_cash_.ToString();   //付款金额
            label29.Text = zhaoling.ToString();  //找零
            label33.Text = card_no_;  //会员卡
            label35.Text = mark_in_.ToString();  //积分
            label26.Text = NumGetString.NumGetStr(YS_cash.Value);   //大写金额
        }


        //取得打印文档,打印模板  
        private string GetPrintStr()
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(HandoverModel.GetInstance.scodeName))
            {
                title = HandoverModel.GetInstance.scodeName;
            }

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

            //card_no_ = Cashiers.GetInstance.VipID.ToString();
            card_no_ = "pw";

            sb.Append("\n");

            //sb.Append("  优惠金额：" + discount_ + "\n");
            sb.Append("  购买件数：" + count_temp.ToString() + "\t" + "应收金额：" + YS_cash + "\n");
            sb.Append("  付款方式：" + Strjstype + "\n");
            sb.Append("  " + "付款金额：" + recv_cash_ + "\t" + "    找零：" + zhaoling.ToString() + "\n");
            //大写金额
            sb.Append("  合计金额：" + NumGetString.NumGetStr(recv_cash_.Value) + "\n");
            sb.Append("  会员卡号：" + card_no_ + "\t" + "本次积分：" + mark_in_ + "\n");
            sb.Append("***************************************\n");
            string myfoot = string.Format("  {0}\n", "欢迎下次光临！");
            sb.Append(myfoot);
            return sb.ToString();
        }


        //自动关闭窗口
        int waitSecond = 5;
        private void timer1_Tick(object sender, EventArgs e)
        {


           if (waitSecond < 1)
            {
                this.Close();
            }
            else
            {
                waitSecond--;
                //label1.Text = "窗口将在" + waitSecond + "秒后关闭";
                
            }
        }


    }
}
