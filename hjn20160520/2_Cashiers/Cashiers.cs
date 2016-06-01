using hjn20160520._2_Cashiers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Configuration;

namespace hjn20160520
{

    #region 与商品选择窗口交互的委托事件
    //public delegate void goodsChooseBarHander(string barCode, string goods, string unit, string spec, string retaols, string pinYin);


    #endregion





    public partial class Cashiers : Form
    {


        //单例
        public static Cashiers GetInstance { get; private set; }

        public ChoiceGoods choice;  // 商品选择窗口
        public ClosingEntries CEform;  //  商品结算窗口
        //测试用数据库
        TestEntities db = new TestEntities();

        //记录购物车内的商品
        public List<string> GoodsList = new List<string>();

        #region 商品属性
        
        //标志一单交易,是否新单
        public bool isNewItem = false;

        //条码
        public string barCode { get;  set; }

        //商名
        public string goods { get;  set; }

        //单位
        public string unit { get;  set; }

        //规格
        public string spec { get;  set; }

        //零售价
        public string retaols { get;  set; }

        //拼音
        public string pinYin { get;  set; }

        //数量
        private float count = 1.00f;

        public float CountNum
        {
            get { return count; }
            set { count = value; }
        }
        

        //金额
        public float sum { get; set; }

        //营业员
        public string salesClerk { get; set; }

        //货号
        public string noCode { get; set; }

        //原价
        public string orig { get; set; }

        //应收总金额
        public float totalMoney { get; private set; }

        #endregion

        public Cashiers()
        {
            InitializeComponent();
        }
        //没用
        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        //右下方当前时间
        private void timer1_Tick(object sender, EventArgs e)
        {
            label_timer.Text = " 当前时间：" + System.DateTime.Now.ToString();
        }

        //窗口全屏设置
        private void Form1_Load(object sender, EventArgs e)
        {
                        //全屏
            //if (this.WindowState == FormWindowState.Maximized)
            //{
            //    this.WindowState = FormWindowState.Normal;
            //}
            //else
            //{
            //    this.FormBorderStyle = FormBorderStyle.None;
            //    this.WindowState = FormWindowState.Maximized;
            //    this.TopMost = true;  //窗口顶置
            //}

            //时间开始
            timer1.Start();
            //窗口赋值
            CEform = new ClosingEntries();
            choice = new ChoiceGoods();
            //单例赋值
            if (GetInstance == null) GetInstance = this;

            //初始化购物车
            if (GoodsList.Count > 0)
            {
                GoodsList.Clear();
            }

        }

        //计时器点击事件（没用到）
        private void label_timer_Click(object sender, EventArgs e)
        {

        }




        //根据条码通过EF进行模糊查询
        private void EFSelectByBarCode()
        {
            string temptxt = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(temptxt))
            {
                MessageBox.Show("请输入需要查找的商品条码");
                return;
            }

            var rules = db.HjnDemoData.Where(t => t.BarCode.Contains(temptxt))
                .Select(t => new { BarCode = t.BarCode, Goods = t.Goods, unit = t.Unit, spec = t.spec, retails = t.UnitPrice, pinyin = t.Pinyin })
                .OrderBy(t => t.pinyin)
                .ToList();

            //如果查出数据不至一条就弹出选择窗口，否则直接显示出来

            if (rules.Count == 0)
            {
                MessageBox.Show("没有查找该商品");
                return;
            }
            //查询到多条则弹出商品选择窗口
            if (rules.Count > 1)
            {
                var form1 = new ChoiceGoods();
                form1.dataGridView1.DataSource = rules;
                form1.ShowDialog();

            }
            //只查到一条就直接上屏
            if (rules.Count == 1)
            {
                foreach (var item in rules)
                {

                    this.barCode = item.BarCode;
                    this.goods = item.Goods;
                    this.unit = item.unit;
                    this.spec = item.spec;
                    this.retaols = Convert.ToString(item.retails);
                    this.pinYin = item.pinyin;

                    GoodsList.Add(item.BarCode.Trim());
                    DataShow();
                }

            }


            //每次查询完毕都得清空输入框
            textBox1.Text = "";
        }



        //条码文本输入框按键事件
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        //窗体在屏幕居中
        private void Cashiers_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.DarkOliveGreen, 0, 0, this.Width - 1, this.Height - 1);
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        //清除数据显示的空格
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value is string)
                e.Value = e.Value.ToString().Trim();
        }


        //窗体激活事件
        private void Cashiers_Activated(object sender, EventArgs e)
        {
            //if (textBox1.Focused && dataGridView1.RowCount > 0)
            //{
            //    this.dataGridView1.Focus();
            //    this.dataGridView1.Rows[0].Selected = true;
            //}


            //if (ActiveControl is DataGridView)
            //{

            //        MessageBox.Show(dataGridView1.RowCount.ToString());
                
            //}


        }

        //刷新datagridview数据显示,如果原来有相同的商品叠加时只需要更改数量就行了
        public void DataShow()
        {
            if (GoodsList.Count > 0 && GoodsList.Contains(this.barCode))
            {
                int rowcount = dataGridView_Cashiers.Rows.Count;
                int cellcount = dataGridView_Cashiers.Rows[0].Cells.Count;
                for (int i = 0; i < rowcount; i++)
                {
                    for (int j = 0; j < cellcount; j++)
                    {
                        if (this.barCode == dataGridView_Cashiers.Rows[i].Cells[j].Value.ToString())
                        {

                            int val = Convert.ToInt32(dataGridView_Cashiers.Rows[i].Cells[5].Value);
                            int val_count = val + 1;
                            dataGridView_Cashiers.Rows[i].Cells[5].Value = val_count;

                            float val_8 = float.Parse(dataGridView_Cashiers.Rows[i].Cells[5].Value.ToString());
                            float temp = float.Parse(this.retaols);
                            dataGridView_Cashiers.Rows[i].Cells[8].Value = temp * val_8;

                        }
                    }
                }
            }
            else
            {

                GoodsList.Add(this.barCode);
                this.orig = this.retaols;
                this.salesClerk = "测试人员";
                this.noCode = "NO";
                float temp = float.Parse(this.retaols);
                this.sum = this.CountNum * temp;
                //数据表每次有列的增减，需要在下面代码里变动，按列依次排列，空内容可以留“”
                string[] row = {"", this.noCode, this.barCode, this.goods, this.spec, this.CountNum.ToString(), this.orig, this.retaols, this.sum.ToString(), this.salesClerk };

                this.dataGridView_Cashiers.Rows.Add(row);
            
            }

            ShowDown();
        }

        //判断用户是否修改单元格
        private void dataGridView_Cashiers_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {


        }


        //当用户开始编辑数据网格时，保存修改前的值，方便返回操作
        private void dataGridView_Cashiers_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            //MessageBox.Show(dataGridView_Cashiers.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
        }


        //当用户直接在UI上修改数据完毕时
        private void dataGridView_Cashiers_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

            var v = dataGridView_Cashiers.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            float temp = float.Parse(this.retaols);
            float tempV = float.Parse(v);
            dataGridView_Cashiers.Rows[e.RowIndex].Cells[e.ColumnIndex + 3].Value = tempV * temp;

            ShowDown();

            textBox1.Focus();  //焦点回到条码输入框
        }

        //datagridview单元格修改值提交时验证数据是否符合要求
        private void dataGridView_Cashiers_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            //验证第5列，数量
            if (e.ColumnIndex == 5)
            {
                int temp_int = 0;
                if (int.TryParse(e.FormattedValue.ToString(), out temp_int))
                {
                    e.Cancel = false;
                }
                else
                {
                    e.Cancel = true;
                    dataGridView_Cashiers.CancelEdit();
                    MessageBox.Show("数量请输入整数");
                }

                

                
            }
        }

        //下方总汇的UI显示
        public void ShowDown()
        {
            //if (isNewItem) return;

            if (dataGridView_Cashiers.Rows.Count > 0) {
                //label84.Text = dataGridView_Cashiers.SelectedRows[0].Cells[3].Value.ToString();
                label84.Text = dataGridView_Cashiers.CurrentRow.Cells[3].Value.ToString();
                //用选中行容易报错
                //label83.Text = dataGridView_Cashiers.SelectedRows[0].Cells[7].Value.ToString() + "  元";
                label83.Text = dataGridView_Cashiers.CurrentRow.Cells[7].Value.ToString() + "  元";


                float temp_r = 0;
                int temp_c = 0;
                foreach (DataGridViewRow row in dataGridView_Cashiers.Rows)
                {
                    temp_r += float.Parse(row.Cells[8].Value.ToString());
                    temp_c += int.Parse(row.Cells[5].Value.ToString());
                }

                label81.Text = temp_r.ToString() + "  元";
                label82.Text = temp_c.ToString();
                totalMoney = temp_r;
            }
            else
            {
                label84.Text = "";
                label83.Text = "";
                label81.Text = "";
                label82.Text = "";
            }
        }


        //当datagridview行选中时触发事件
        private void dataGridView_Cashiers_SelectionChanged(object sender, EventArgs e)
        {
            ShowDown();
        }



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
                    case Keys.Delete:

                        //如果当前只有一行就直接清空
                        if (dataGridView_Cashiers.Rows.Count == 1)
                        {
                            int DELindex1_temp = dataGridView_Cashiers.SelectedRows[0].Index;
                            dataGridView_Cashiers.Rows.RemoveAt(DELindex1_temp);
                            GoodsList.Clear();
                        }
                        //当前行数大于1行时删除选中行后把往上一行设置为选中状态
                        if (dataGridView_Cashiers.Rows.Count > 1)
                        {
                             int DELindex_temp = dataGridView_Cashiers.SelectedRows[0].Index;
                             dataGridView_Cashiers.Rows.RemoveAt(DELindex_temp);
                             try
                             {
                                 string de_temp = dataGridView_Cashiers.CurrentRow.Cells[2].Value.ToString();

                                 if (DELindex_temp - 1 >= 0)
                                 {
                                     dataGridView_Cashiers.Rows[DELindex_temp - 1].Selected = true;
                                 }

                                 GoodsList.Remove(de_temp.Trim());
                             }
                             catch { }


                            //MessageBox.Show(GoodsList.Count.ToString());
                        }


                        break;
                        //回车
                    case Keys.Enter:

                        if (string.IsNullOrEmpty(textBox1.Text) && dataGridView_Cashiers.Rows.Count > 0)
                        {
                            CEform.ShowDialog();
                        }

                        if (!string.IsNullOrEmpty(textBox1.Text) || dataGridView_Cashiers.Rows.Count == 0)
                        {
                            EFSelectByBarCode();
                        }

                        if (isNewItem)
                        {
                            this.label85.Visible = false;
                            this.label86.Visible = false;
                            this.label87.Visible = false;
                            this.label88.Visible = false;
                            isNewItem = false;
                        }

                        break;
                        //小键盘+号
                    case Keys.Add:
                        dataGridView_Cashiers.CurrentCell = dataGridView_Cashiers.SelectedRows[0].Cells[5];
                        dataGridView_Cashiers.BeginEdit(true);
                        break;

                    //向上键表格换行
                    case Keys.Up:
                        //当前行数大于1行才生效
                        if (dataGridView_Cashiers.Rows.Count > 1)
                        {
                            int rowindex_temp = dataGridView_Cashiers.SelectedRows[0].Index;
                            if (rowindex_temp == 0)
                            {
                                dataGridView_Cashiers.Rows[dataGridView_Cashiers.Rows.Count - 1].Selected = true;
                                dataGridView_Cashiers.Rows[rowindex_temp].Selected = false;

                            }
                            else
                            {
                                dataGridView_Cashiers.Rows[rowindex_temp - 1].Selected = true;
                                dataGridView_Cashiers.Rows[rowindex_temp].Selected = false;
                            }
                        } 


                        break;

                    //向下键表格换行
                    case Keys.Down:
                        //当前行数大于1行才生效
                        if (dataGridView_Cashiers.Rows.Count > 1)
                        {
                            int rowindexDown_temp = dataGridView_Cashiers.SelectedRows[0].Index;
                            if (rowindexDown_temp == dataGridView_Cashiers.Rows.Count - 1)
                            {
                                dataGridView_Cashiers.Rows[0].Selected = true;
                                dataGridView_Cashiers.Rows[rowindexDown_temp].Selected = false;

                            }
                            else
                            {
                                dataGridView_Cashiers.Rows[rowindexDown_temp + 1].Selected = true;
                                dataGridView_Cashiers.Rows[rowindexDown_temp].Selected = false;
                            }
                        }


                        break;
              }

            }
            return false;
        }

        #region 自动在数据表格首列绘制序号
        
        
        //表格绘制事件
        private void dataGridView_Cashiers_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SetDataGridViewRowXh(e, dataGridView_Cashiers);
        }
        //在首列绘制序号，如果首列原有内容，会出现重叠，所以首列留空
        private void SetDataGridViewRowXh(DataGridViewRowPostPaintEventArgs e, DataGridView dataGridView)
        {
            SolidBrush solidBrush = new SolidBrush(Color.White); //更改序号样式
            int xh = e.RowIndex + 1;
            e.Graphics.DrawString(xh.ToString(CultureInfo.CurrentUICulture), e.InheritedRowStyle.Font, solidBrush, e.RowBounds.Location.X + 5, e.RowBounds.Location.Y + 4);
        }
        #endregion



        //重新开始一单交易，窗口控件初始化
        public void InitItemFrom()
        {
            GoodsList.Clear();
            for (int i = 0; i < dataGridView_Cashiers.Rows.Count; i++)
            {
                dataGridView_Cashiers.Rows.Remove(dataGridView_Cashiers.Rows[i]);
            }
            isNewItem = true;
        }

        //锁定窗口焦点始终在条码输入框上(目前与数据直接修改功能冲突)
        private void textBox1_Leave(object sender, EventArgs e)
        {
            //textBox1.Focus();
        }

    }
}
