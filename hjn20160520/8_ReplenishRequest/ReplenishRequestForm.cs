using Common;
using hjn20160520.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hjn20160520._8_ReplenishRequest
{
    public partial class ReplenishRequestForm : Form
    {
        //单例
        public static ReplenishRequestForm GetInstance { get; private set; }

        //主菜单
        MainForm mainForm;
        //制单窗口
        RequsetNoteForm RNForm;


        //商品列表 
        public BindingList<RRGoodsModel> GoodsList = new BindingList<RRGoodsModel>();
        //记录从数据库查到的商品,选择商品
        public BindingList<RRGoodsModel> GoodsChooseList = new BindingList<RRGoodsModel>();
        //主单列表数据源
        public BindingList<BHInfoNoteModel> BHmainNoteList = new BindingList<BHInfoNoteModel>();
        //临时商品暂存
        //public RRGoodsModel tempGoods = new RRGoodsModel();

        //是否审核
        public bool isMK = false;
        //审核时间
        public DateTime MKtime;
        //审核人ID
        public int aid = 0;

        public ReplenishRequestForm()
        {
            InitializeComponent();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void ReplenishRequestForm_Load(object sender, EventArgs e)
        {
            if (GetInstance == null) GetInstance = this;
            RNForm = new RequsetNoteForm();
            mainForm = new MainForm();
            dataGridView1.DataSource = BHmainNoteList;
            //this.FormBorderStyle = FormBorderStyle.None;

        }


        #region 热键注册

        //重写热键方法
        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
        {
            int WM_KEYDOWN = 256;
            int WM_SYSKEYDOWN = 260;
            if (msg.Msg == WM_KEYDOWN | msg.Msg == WM_SYSKEYDOWN)
            {
                switch (keyData)
                {
                    //删除DEL键
                    case Keys.Escape:

                        mainForm.Show();
                        this.Close();

                        break;

                    //回车
                    case Keys.Enter:


                        break;
                        //新单
                    case Keys.F3:
                        OnRNFormClickFunc();
                        break;


                }

            }
            return false;
        }

        #endregion

        //F3按钮-新单窗口
        private void button2_Click(object sender, EventArgs e)
        {
            OnRNFormClickFunc();
        }

        //Del删除按钮
        private void button4_Click(object sender, EventArgs e)
        {

        }
        //F4修改按钮
        private void button3_Click(object sender, EventArgs e)
        {

        }
        //ESC关闭按钮
        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        //F2日期查询按钮
        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(DateTime.Now.ToString("yyyyMMdd"));
        }

        //处理新单窗口事件
        private void OnRNFormClickFunc()
        {
            RNForm.ShowDialog();
            //InitRNForm();
        }




        #region 自动在数据表格首列绘制序号
        private void dataGridView2_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SetDataGridViewRowXh(e, dataGridView1);
        }
        //在首列绘制序号，如果首列原有内容，会出现重叠，所以首列手动添加一个空列
        private void SetDataGridViewRowXh(DataGridViewRowPostPaintEventArgs e, DataGridView dataGridView)
        {
            SolidBrush solidBrush = new SolidBrush(Color.Black); //更改序号样式
            int xh = e.RowIndex + 1;
            e.Graphics.DrawString(xh.ToString(CultureInfo.CurrentUICulture), e.InheritedRowStyle.Font, solidBrush, e.RowBounds.Location.X + 5, e.RowBounds.Location.Y + 4);
        }
        #endregion



        //用户从商品选择窗口选中的商品,如果补货清单中已存在该商品则数量加1，否则新增
        public void UserChooseGoods(int index)
        {
            if (GoodsList.Any(k => k.noCode == GoodsChooseList[index].noCode))
            {
                var se = GoodsList.Where(h => h.noCode == GoodsChooseList[index].noCode);
                foreach (var item in se)
                {
                    if (item.countNum > 0)
                    {
                        item.countNum += item.countNum;
                    }
                    else
                    {
                        item.countNum++;

                    }
                }

                RNForm.dataGridView1.Refresh();

            }
            else
            {
                try
                {
                    GoodsList.Add(GoodsChooseList[index]);

                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("补货申请重复商品数量自增时发生异常:", ex);
                }
            }

            RNForm.textBox1.Text = GoodsChooseList[index].barCodeTM;
        }

        //通过审核后冻结审核按钮与输入框
        public void FreezeRNForm()
        {
            RNForm.button1.Enabled = RNForm.textBox1.Enabled = RNForm.textBox2.Enabled = false;
        }
        //审核日
        public void MKDate()
        {
            RNForm.label11.Text = System.DateTime.Now.ToString("yyyy-MM-dd");
        }

    }
}
