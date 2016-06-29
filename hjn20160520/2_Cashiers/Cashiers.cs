using Common;
using hjn20160520._2_Cashiers;
using hjn20160520._9_VIPCard;
using hjn20160520.Common;
using hjn20160520.Models;
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
    /// <summary>
    /// 结算方式
    /// </summary>
    public enum JSType
    {
        Cash = 0, //现金
        UnionPay = 1, //银联卡
        Coupon = 2,  //购物劵
        Others = 3  //其它
    }


    /// <summary>
    /// 收银模块主逻辑
    /// </summary>
    public partial class Cashiers : Form
    {
        //单例
        public static Cashiers GetInstance { get; private set; }

         ChoiceGoods choice;  // 商品选择窗口
         ClosingEntries CEform;  //  商品结算窗口
         GoodsNote GNform; //挂单窗口
         MainForm mainForm;  //主菜单
         MemberPointsForm MPForm;  //会员积分冲减窗口
         SalesmanForm SMForm; //业务员录入窗口
         LockScreenForm LSForm;  //锁屏窗口
         public RefundForm RDForm;  //整单退货窗口
         VipShopForm vipForm; //会员消费窗口


        //公共提示信息窗口
        TipForm tipForm;
        
        //记录购物车内的商品
        public BindingList<GoodsBuy> goodsBuyList = new BindingList<GoodsBuy>();
        //记录从数据库查到的商品
        public BindingList<GoodsBuy> goodsChooseList = new BindingList<GoodsBuy>();
        //挂单取单窗口的挂单列表
        public BindingList<GoodsNoteModel> noteList = new BindingList<GoodsNoteModel>();
        //挂单窗口中订单号与订单商品清单对应的字典列表
        public Dictionary<int, BindingList<GoodsBuy>> noteDict = new Dictionary<int, BindingList<GoodsBuy>>();

        #region 收银属性
        

        //单号，临时，以后要放上数据库读取
        public int OrderNo = 0;

        //标志一单交易,是否新单
        public bool isNewItem = false;

        //应收总金额
        public float totalMoney { get; set; }

        //营业员
        public string SalesMan { get; set; }

        //进行消费的会员ID
        public int VipID { get; set; }

        #endregion

        public Cashiers()
        {
            InitializeComponent();
        }


        //右下方当前时间
        private void timer1_Tick(object sender, EventArgs e)
        {
            label_timer.Text = " 当前时间：" + System.DateTime.Now.ToString();
        }

        //窗口初始化
        private void Form1_Load(object sender, EventArgs e)
        {
            //单例赋值
            if (GetInstance == null) GetInstance = this;

            Init();
        }

        //初始化窗口
        private void Init()
        {
            //窗口全屏设置全屏
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
            GNform = new GoodsNote();
            mainForm = new MainForm();
            MPForm = new MemberPointsForm();
            tipForm = new TipForm();
            SMForm = new SalesmanForm();
            LSForm = new LockScreenForm();
            RDForm = new RefundForm();
            vipForm = new VipShopForm();

            dataGridView_Cashiers.DataSource = goodsBuyList;

            //初始化购物车
            if (goodsBuyList.Count > 0)
            {
                goodsBuyList.Clear();
            }

            //合计等文本初始化，完工后直接设置为空
            label84.Text = "";
            label83.Text = "";
            label81.Text = "";
            label82.Text = "";

            label100.Text = HandoverModel.GetInstance.userName;  //员工名字

        }


        //计时器点击事件（没用到）
        private void label_timer_Click(object sender, EventArgs e)
        {

        }



        #region 商品查询
        
        
        //bool isFirst = true;
        //根据条码通过EF进行模糊查询
        private void EFSelectByBarCode()
        {
            string temptxt = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(temptxt))
            {
                //MessageBox.Show("请输入需要查找的商品条码");
                tipForm.Tiplabel.Text = "请输入需要查找的商品条码!";
                tipForm.ShowDialog();
                return;
            }
            using (hjnbhEntities db = new hjnbhEntities())
            {
                var rules = db.hd_item_info.Where(t => t.tm.Contains(temptxt))
                        .Select(t => new { noCode = t.item_id, BarCode = t.tm, Goods = t.cname, unit = t.unit, spec = t.spec, retails = t.ls_price, pinyin = t.py, goodsDes = t.manufactory })
                        .OrderBy(t => t.pinyin)
                        .ToList();

                //如果查出数据不至一条就弹出选择窗口，否则直接显示出来

                if (rules.Count == 0)
                {
                    //MessageBox.Show("没有查找到该商品");
                    this.textBox1.SelectAll();
                    tipForm.Tiplabel.Text = "没有查找到该商品!";
                    tipForm.ShowDialog();
                    return;
                }
                //查询到多条则弹出商品选择窗口，排除表格在正修改时发生判断
                if (rules.Count > 1 && !dataGridView_Cashiers.IsCurrentCellInEditMode)
                {

                    var form1 = new ChoiceGoods();
                    

                    foreach (var item in rules)
                    {

                        goodsChooseList.Add(new GoodsBuy { noCode = item.noCode, barCodeTM = item.BarCode, goods = item.Goods, unit = item.unit.ToString(), spec = item.spec, lsPrice = item.retails.ToString(), pinYin = item.pinyin, salesClerk = HandoverModel.GetInstance.YWYStr, goodsDes = item.goodsDes });

                    }

                    
                    form1.dataGridView1.DataSource = goodsChooseList;
                    //隐藏不需要显示的列
                    form1.dataGridView1.Columns[0].Visible = false;
                    form1.dataGridView1.Columns[4].Visible = false;
                    form1.dataGridView1.Columns[6].Visible = false;
                    form1.dataGridView1.Columns[8].Visible = false;
                    form1.dataGridView1.Columns[10].Visible = false;
                    form1.dataGridView1.Columns[11].Visible = false;

                    //设置单元格不可以编辑
                    for (int i = 0; i < form1.dataGridView1.Columns.Count; i++)
                    {
                        form1.dataGridView1.Columns[i].ReadOnly = true;
                    }

                    form1.ShowDialog();

                }
                //只查到一条如果没有重复的就直接上屏，除非表格正在修改数量
                if (rules.Count == 1 && !dataGridView_Cashiers.IsCurrentCellInEditMode)
                {
                    GoodsBuy newGoods_temp = new GoodsBuy();
                    foreach (var item in rules)
                    {
                        newGoods_temp = new GoodsBuy { noCode = item.noCode, barCodeTM = item.BarCode, goods = item.Goods, unit = item.unit.ToString(), spec = item.spec, lsPrice = item.retails.ToString(), pinYin = item.pinyin, salesClerk = HandoverModel.GetInstance.YWYStr, goodsDes = item.goodsDes };

                    }

                    if (goodsBuyList.Count == 0)
                    {
                        goodsBuyList.Add(newGoods_temp);

                        //设置单元格是否可以编辑
                        for (int i = 0; i < dataGridView_Cashiers.Columns.Count; i++)
                        {
                            if (dataGridView_Cashiers.Columns[i].Index != 5)
                            {
                                dataGridView_Cashiers.Columns[i].ReadOnly = true;
                            }
                        }

                    }
                    else
                    {
                       
                        if (goodsBuyList.Any(n => n.noCode == newGoods_temp.noCode))
                        {
                           var o= goodsBuyList.Where(p => p.noCode == newGoods_temp.noCode);
                           foreach (var _item in o)
                           {
                               _item.countNum++;
                           }
                           dataGridView_Cashiers.Refresh();
                        }
                        else
                        {
                            goodsBuyList.Add(newGoods_temp);
                        }

                                       
                    }

                    //调整DatagridView列宽百分比例，只需第一次加载时运行一次便可，自动列宽模式必须是Fill
                    //if (isFirst)
                    //{
                    //    this.dataGridView_Cashiers.Columns[0].FillWeight = 5;
                    //    this.dataGridView_Cashiers.Columns[1].FillWeight = 10;
                    //    this.dataGridView_Cashiers.Columns[2].FillWeight = 15;
                    //    this.dataGridView_Cashiers.Columns[3].FillWeight = 15;
                    //    this.dataGridView_Cashiers.Columns[4].FillWeight = 9;
                    //    this.dataGridView_Cashiers.Columns[5].FillWeight = 9;
                    //    this.dataGridView_Cashiers.Columns[6].FillWeight = 5;
                    //    this.dataGridView_Cashiers.Columns[7].FillWeight = 9;
                    //    this.dataGridView_Cashiers.Columns[8].FillWeight = 9;
                    //    this.dataGridView_Cashiers.Columns[9].FillWeight = 9;
                    //    this.dataGridView_Cashiers.Columns[10].FillWeight = 9;      
                    //    isFirst = false;
                    //}

                    //dataGridView_Cashiers.Refresh();
                }
            }

            //每次查询完毕都得清空输入框
            textBox1.Text = "";
        }

        #endregion

        //条码文本输入框按键事件
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        //窗体在屏幕居中
        private void Cashiers_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.DarkOliveGreen, 0, 0, this.Width - 1, this.Height - 1);
        }
        //网格内容点击事件（没用到）
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

            if (label103.Text == "未登记")
            {
                Tipslabel.Text = "营业员还未登记，请按F4键录入";
            }

            if (label23.Text == "未登记")
            {
                Tipslabel.Text = "如果是会员消费，请按F12键录入";
            }

            if (label103.Text == "未登记" && label103.Text == "未登记")
            {
                Tipslabel.Text = "营业员还未登记，请按F4键录入。如果是会员消费，请按F12键录入";
            }
            
        }


        //判断用户是否修改单元格
        private void dataGridView_Cashiers_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {


        }


        //当用户开始编辑数据网格时，保存修改前的值，方便返回操作
        private void dataGridView_Cashiers_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {

        }


        //当用户直接在UI上修改数据完毕时
        private void dataGridView_Cashiers_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

            dataGridView_Cashiers.Refresh();

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
                    //MessageBox.Show("数量请输入整数");
                    tipForm.Tiplabel.Text = "数量请输入整数!";
                    tipForm.ShowDialog();
                }

                

                
            }
        }

        //显示UI，购物车商品总额，总数量
        public void ShowDown()
        {
            //if (isNewItem) return;

            if (dataGridView_Cashiers.Rows.Count > 0)
            {
                try
                {
                    float temp_r = 0;
                    int temp_c = 0;
                    foreach (DataGridViewRow row in dataGridView_Cashiers.Rows)
                    {
                        temp_r += float.Parse(row.Cells[9].Value.ToString());
                        temp_c += int.Parse(row.Cells[5].Value.ToString());
                    }

                    label81.Text = temp_r.ToString() + "  元";
                    label82.Text = temp_c.ToString();
                    totalMoney = temp_r;  //获取总金额
                }
                catch (Exception e)
                {
                    LogHelper.WriteLog("收银主界面下方UI显示异常:", e);
                }
            }
            else
            {
                label81.Text = "";
                label82.Text = "";
            }
        }


        //当datagridview行选中时触发事件,显示当前选中的商品名字，商品零售价
        private void dataGridView_Cashiers_SelectionChanged(object sender, EventArgs e)
        {

            if (dataGridView_Cashiers.Rows.Count > 0)
            {
                try
                {
                     label84.Text = dataGridView_Cashiers.SelectedRows[0].Cells[3].Value.ToString();
                     label83.Text = dataGridView_Cashiers.SelectedRows[0].Cells[8].Value.ToString() + "  元";
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("收银主界面根据选择自动显示商品信息发生异常:", ex);
                }
            }
            else
            {
                label84.Text = "";
                label83.Text = "";
            }
           

        }

        #region 热键注册

        //普通快捷键设置
        private void Cashiers_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                    //F4键登记业务员
                case Keys.F4:
                    SMForm.ShowDialog();
              
                    break;
                    //最小化
                case Keys.Pause:
                    this.WindowState = FormWindowState.Minimized;
                    break;
                    //锁屏
                case Keys.Home:
                    //LSForm = new LockScreenForm();
                    LSForm.ShowDialog();
                    //this.Hide();
                    break;
                    //退货
                case Keys.F9:
                    Refund();
                    break;
                    //打开会员卡窗口
                case Keys.F12:
                    vipForm.ShowDialog();
                    break;

            }
        }


        int timer_temp = 0;  // 临时变量，解决结算后无法显示结算信息的BUG，因为按回车关闭结算窗口后会同时触发该窗口的回车事件……
        //重写热键方法，这个优先级最高
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

                        Dele();

                        break;

                        //清空购物车
                    case Keys.Insert:

                        InsertFun();

                        break;

                        //回车
                    case Keys.Enter:

                        EnterFun();
                        //MessageBox.Show(goodsBuyList.Count.ToString());
                        break;

                        //小键盘+号
                    case Keys.Add:

                        UpdataCount();
                        break;

                    //向上键表格换行
                    case Keys.Up:

                        UpFun();

                        break;

                    //向下键表格换行
                    case Keys.Down:

                        DownFun();
                     
                        break;


                    case Keys.PageUp:

                        UpNote();

                        break;

                    case Keys.PageDown:

                        GetNote();

                        break;

                    case Keys.Escape:
                        OnCashFormESC();
                    
                        break;

                        //打开会员积分冲减窗口
                    case Keys.F2:
                        VIPForm();
                        break;

              }

            }
            return false;
        }


        //删除单行
        private void Dele()
        {
            //如果当前只有一行就直接清空
            if (dataGridView_Cashiers.Rows.Count == 1)
            {
                int DELindex1_temp = dataGridView_Cashiers.SelectedRows[0].Index;
                dataGridView_Cashiers.Rows.RemoveAt(DELindex1_temp);

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


                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("收银主界面删除选中行发生异常:", ex);
                }

            }
        }

        //删除全部，清空购物车
        private void InsertFun()
        {
            if (dataGridView_Cashiers.Rows.Count > 0)
            {
                goodsBuyList.Clear();
            }

        }


        //回车功能
        private void EnterFun()
        {
            //如果输入框为空且购物车有商品时，则弹出结算窗口
            if (string.IsNullOrEmpty(textBox1.Text) && goodsBuyList.Count > 0 && !dataGridView_Cashiers.IsCurrentCellInEditMode)
            {
                CEform.ShowDialog();
            }
            //如果输入框有内容或者购物车没有商品，则进行商品查询
            if (!string.IsNullOrEmpty(textBox1.Text) || goodsBuyList.Count == 0)
            {
                if (isNewItem == false)
                {
                    EFSelectByBarCode();  // 查数据库
                }

            }

            //是否新单
            if (isNewItem)
            {
                //人为地造成需要按两次回车才触发
                timer_temp++;
                if (timer_temp == 2)
                {
                    //this.label85.Visible = false;
                    //this.label86.Visible = false;
                    //this.label87.Visible = false;
                    //this.label88.Visible = false;
                    this.tableLayoutPanel2.Visible = false;  //隐藏结算结果
                    isNewItem = false;
                    timer_temp = 0;
                }


            }
        }


        //小键盘向上
        private void UpFun()
        {
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
        }

        //小键盘向下
        private void DownFun()
        {
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
        }

        //负责挂单的逻辑
        private void UpNote()
        {
            if (goodsBuyList.Count > 0)
            {
                //goodsNoteList.Add(goodsBuyList);

                string date_temp = System.DateTime.Now.ToString();
                string cashier_temp = goodsBuyList[0].salesClerk;
                OrderNo++;

                noteList.Add(new GoodsNoteModel { noNote = OrderNo, upDate = date_temp, cashier = cashier_temp, totalM = totalMoney });

                GNform.dataGridViewGN1.DataSource = noteList;

                var list_temp = goodsBuyList.Select(t => t).ToList<GoodsBuy>();
                var list_2 = new BindingList<GoodsBuy>();

                foreach (var item in list_temp)
                {
                    list_2.Add(item);
                }

                if (!noteDict.ContainsKey(OrderNo))
                {
                    noteDict.Add(OrderNo, list_2);
                }
                //挂单后清屏
                goodsBuyList.Clear();

            }
            else
            {
                tipForm.Tiplabel.Text = "当前无销售商品，空单不能挂起！";
                tipForm.ShowDialog();
                
            }
            //更新挂单数量
            label98.Text = noteList.Count.ToString();

        }

        //负责取出挂单
        private void GetNote()
        {

            if (goodsBuyList.Count > 0)
            {
                tipForm.Tiplabel.Text = "此单已有商品明细，请先结算或者清除此单后再进行取单操作！";
                tipForm.ShowDialog();
            }
            else
            {
                GNform.ShowDialog();
            }
            //更新挂单数量
            label98.Text = noteList.Count.ToString();
            
        }

        //收银窗口退出
        private void OnCashFormESC()
        {
            if (noteList.Count > 0)
            {
                tipForm.Tiplabel.Text = "您有挂单或未结单，请先取消后再退出前台收银！";
                tipForm.ShowDialog();
            }
            else if (goodsBuyList.Count > 0)           
            {
                tipForm.Tiplabel.Text = "请先清空当前商品清单后再退出前台收银！";
                tipForm.ShowDialog();
            }
            else
            {
                mainForm.Show();
                this.Hide();
            }



        }

        //按+号修改数量
        private void UpdataCount()
        {
            if (goodsBuyList.Count > 0)
            {
                try
                {
                    dataGridView_Cashiers.CurrentCell = dataGridView_Cashiers.SelectedRows[0].Cells[5];
                    dataGridView_Cashiers.BeginEdit(true);
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("收银主界面按+号修改商品数量发生异常:", ex);
                }
            }
          
        }

        //会员管理窗口
        private void VIPForm()
        {

            MPForm.ShowDialog();

        }

        //负责退货逻辑
        private void Refund()
        {
            this.label96.Text = "退货";
            tipForm.code = 1;
            tipForm.Tiplabel.Text = "按Shift键进行单品退货，按回车键进行整单退货";
            tipForm.ShowDialog();
        }



        #endregion 

        #region 自动在数据表格首列绘制序号


        //表格绘制事件
        private void dataGridView_Cashiers_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SetDataGridViewRowXh(e, dataGridView_Cashiers);
        }
        //在首列绘制序号，如果首列原有内容，会出现重叠，所以首列手动添加一个空列
        private void SetDataGridViewRowXh(DataGridViewRowPostPaintEventArgs e, DataGridView dataGridView)
        {
            SolidBrush solidBrush = new SolidBrush(Color.White); //更改序号样式
            int xh = e.RowIndex + 1;
            e.Graphics.DrawString(xh.ToString(CultureInfo.CurrentUICulture), e.InheritedRowStyle.Font, solidBrush, e.RowBounds.Location.X + 5, e.RowBounds.Location.Y + 4);
        }
        #endregion



        //锁定窗口焦点始终在条码输入框上(目前与数据直接修改功能冲突)
        private void textBox1_Leave(object sender, EventArgs e)
        {
            //textBox1.Focus();
        }


        //用户从商品选择窗口选中的商品,如果购物车已存在该商品则数量加1，否则新增
        public void UserChooseGoods(int index)
        {
            if (goodsBuyList.Any(k => k.noCode == goodsChooseList[index].noCode))
            {
                var se = goodsBuyList.Where(h => h.noCode == goodsChooseList[index].noCode);
                foreach (var item in se)
                {
                    item.countNum++;
                }

                dataGridView_Cashiers.Refresh();
                ShowDown();
            }
            else
            {
                try
                {
                    goodsBuyList.Add(goodsChooseList[index]);
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("收银主界面重复商品数量自增时发生异常:", ex);
                }
            }

            
        }

        //每当购物车内有商品列表增减时刷新UI
        private void dataGridView_Cashiers_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            ShowDown();
        }

        //结单，完成了一单，全部还原重新开始
        //显示上单、结单实收款与总金额
        public void isNewItems(bool NewOrOld = false)
        {
            if (NewOrOld)
            {
                goodsBuyList.Clear();
                label81.Text = "";
                label82.Text = "";
                label83.Text = "";
                label84.Text = "";
                //this.label85.Visible = true;
                //this.label86.Visible = true;
                //this.label87.Visible = true;
                //this.label88.Visible = true;
                this.tableLayoutPanel2.Visible = true; //显示结算UI
                isNewItem = true;

                //上单合计
                this.label91.Visible = true;
                this.label91.Text = ClosingEntries.GetInstance.CETotalMoney + " 元";



            }

        }



        //根据挂单窗口中选择的挂单来显示商品清单
        public BindingList<GoodsBuy> NoteSeleOrder(int order)
        {


            BindingList<GoodsBuy> list_temp = new BindingList<GoodsBuy>();
            noteDict.TryGetValue(order, out list_temp);


            return list_temp;

        }


        //从挂单窗口中取单
        public void GetNoteByorder(int order)
        {

            var list_temp = new BindingList<GoodsBuy>();
            var list_2 = new BindingList<GoodsBuy>();

            noteDict.TryGetValue(order, out list_temp);

            var list = list_temp.Select(t => t).ToList<GoodsBuy>();

            foreach (var item in list)
            {
                goodsBuyList.Add(item);
            }

        }

        // 从外部调用退货窗口的方法
        public void ShowRDForm()
        {
            this.RDForm.ShowDialog();
        }




    }
}
