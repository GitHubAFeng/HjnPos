using Common;
using hjn20160520.Common;
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
        TipForm tipForm; //信息提示
        string Sta_Temp = string.Empty;   //修改单据时传递单据状态

        //商品明细列表 
        public BindingList<RRGoodsModel> GoodsList = new BindingList<RRGoodsModel>();
        //记录从数据库查到的商品,选择商品
        public BindingList<RRGoodsModel> GoodsChooseList = new BindingList<RRGoodsModel>();
        //主单列表数据源
        public BindingList<BHInfoNoteModel> BHmainNoteList = new BindingList<BHInfoNoteModel>();

        //查询商品明细的列表 
        private BindingList<RRGoodsModel> MXList = new BindingList<RRGoodsModel>();

        //是否审核
        public bool isMK = false;
        //审核时间
        //public DateTime MKtime;
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
            dataGridView2.DataSource = MXList;
            //UpdateNameFunc();

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
                    //ESC键
                    case Keys.Escape:

                        mainForm.Show();
                        this.Close();

                        break;
                    //删除
                    case Keys.Delete:
                        if (Dele())
                        {
                            MXList.Clear();
                            MessageBox.Show("此时客户端删除单据并不能影响服务器的数据");
                        }
                        break;

                    //回车
                    case Keys.Enter:

                        FindDetailByNo();
                        break;
                    case Keys.Up:
                        UpFun();
                        button6.Focus(); //回收焦点，防止快捷键冲突
                        break;
                    case Keys.Down:
                        DownFun();
                        button6.Focus();
                        break;
                    //按时间查询主单
                    case Keys.F2:
                        FindOrderByDateTime();
                        button6.Focus();  //回收焦点，把焦点放在这个隐藏控件上
                        break;
                    //新单
                    case Keys.F3:
                        OnRNFormClickFunc();
                        break;
                    //修改
                    case Keys.F4:
                        ModifiedFunc();
                        break;



                }

            }
            return false;
        }

        #region 上下快捷键选择
        //小键盘向上
        private void UpFun()
        {
            //当前行数大于1行才生效
            if (dataGridView1.Rows.Count > 1)
            {
                int rowindex_temp = dataGridView1.SelectedRows[0].Index;
                if (rowindex_temp == 0)
                {
                    dataGridView1.Rows[dataGridView1.Rows.Count - 1].Selected = true;
                    dataGridView1.Rows[rowindex_temp].Selected = false;
                    dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows.Count - 1; //定位滚动条到当前选择行
                }
                else
                {
                    dataGridView1.Rows[rowindex_temp - 1].Selected = true;
                    dataGridView1.Rows[rowindex_temp].Selected = false;
                    dataGridView1.FirstDisplayedScrollingRowIndex = rowindex_temp - 1; //定位滚动条到当前选择行
                }
            }
        }

        //小键盘向下
        private void DownFun()
        {
            //当前行数大于1行才生效
            if (dataGridView1.Rows.Count > 1)
            {
                int rowindexDown_temp = dataGridView1.SelectedRows[0].Index;
                if (rowindexDown_temp == dataGridView1.Rows.Count - 1)
                {
                    dataGridView1.Rows[0].Selected = true;
                    dataGridView1.Rows[rowindexDown_temp].Selected = false;
                    dataGridView1.FirstDisplayedScrollingRowIndex = 0; //定位滚动条到当前选择行
                }
                else
                {
                    dataGridView1.Rows[rowindexDown_temp + 1].Selected = true;
                    dataGridView1.Rows[rowindexDown_temp].Selected = false;
                    dataGridView1.FirstDisplayedScrollingRowIndex = rowindexDown_temp + 1; //定位滚动条到当前选择行
                }

            }
        }


        #endregion

        //删除单行
        private bool Dele()
        {
            //如果当前只有一行就直接清空
            if (dataGridView1.Rows.Count == 1)
            {
                int DELindex1_temp = dataGridView1.SelectedRows[0].Index;
                dataGridView1.Rows.RemoveAt(DELindex1_temp);
                return true;
            }
            //当前行数大于1行时删除选中行后把往上一行设置为选中状态
            if (dataGridView1.Rows.Count > 1)
            {
                int DELindex_temp = dataGridView1.SelectedRows[0].Index;
                dataGridView1.Rows.RemoveAt(DELindex_temp);
                try
                {
                    string de_temp = dataGridView1.CurrentRow.Cells[2].Value.ToString();

                    if (DELindex_temp - 1 >= 0)
                    {
                        dataGridView1.Rows[DELindex_temp - 1].Selected = true;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("补货界面删除选中行发生异常:", ex);
                }

            }
            return false;
        }


        //获取当前选择行,获取单据状态
        private void GetNoteStaFunc()
        {
            //当前选择的单据状态
            if (dataGridView1.SelectedRows.Count > 0)
            {

            }
        }

        //修改功能
        private void ModifiedFunc()
        {
            if (dataGridView1.Rows.Count == 0)
            {
                tipForm = new TipForm();
                tipForm.Tiplabel.Text = "请选中您要修改的主单";
                tipForm.ShowDialog();
                return;
            }

            GoodsList.Clear();
            if (dataGridView2.Rows.Count > 1)
            {
                foreach (var item in MXList)
                {
                    GoodsList.Add(item);                 
                }
            }
            else
            {
                try
                {
                    int id_temp = dataGridView1.SelectedRows[0].Index;
                    string no_temp = BHmainNoteList[id_temp].Bno;
                    using (hjnbhEntities db = new hjnbhEntities())
                    {
                        var infos = db.hd_bh_detail.AsNoTracking().Where(t => t.b_no == no_temp).ToList();
                        if (infos.Count > 0)
                        {
                            foreach (var item in infos)
                            {
                                #region 商品单位查询
                                //需要把单位编号转换为中文以便UI显示
                                int unitID = 1;
                                int.TryParse(item.unit, out unitID);
                                string dw = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == unitID).Select(t => t.txt1).FirstOrDefault();
                                #endregion
                                GoodsList.Add(new RRGoodsModel
                                {
                                    barCodeTM = item.tm,
                                    goods = item.cname,
                                    spec = item.spec,
                                    unit = dw,
                                    countNum = item.amount,
                                    JianShu = item.hpack_size,
                                    lsPrice = item.ls_price,
                                    Extant = 0  //这个现存不知取什么数据 
                                });
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("补货订单明细列表修改功能发生异常：" + ex);
                    MessageBox.Show("数据库连接出错！");
                    string tip = ConnectionHelper.ToDo();
                    if (!string.IsNullOrEmpty(tip))
                    {
                        MessageBox.Show(tip);
                    }
                }
            }
            RNForm.ShowDialog();
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
            if (Dele())
            {
                MXList.Clear();
                MessageBox.Show("此时客户端删除单据并不能影响服务器的数据");
            }
        }
        //F4修改按钮
        private void button3_Click(object sender, EventArgs e)
        {
            ModifiedFunc();
        }
        //ESC关闭按钮
        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
            mainForm.Show();
        }
        //F2日期查询按钮
        private void button1_Click(object sender, EventArgs e)
        {
            FindOrderByDateTime();
            button6.Focus();  //回收焦点，把焦点放在这个隐藏控件上
        }

        //处理新单窗口事件
        private void OnRNFormClickFunc()
        {
            GoodsList.Clear();
            RNForm.ShowDialog();
        }

        //根据主单查询明细
        private void FindDetailByNo()
        {
            if (MXList.Count > 0) MXList.Clear();  //清空先前显示的数据
            if (BHmainNoteList.Count > 0)
            {
                try
                {
                    int id_temp = dataGridView1.SelectedRows[0].Index;
                    string no_temp = BHmainNoteList[id_temp].Bno;
                    using (hjnbhEntities db = new hjnbhEntities())
                    {
                        var infos = db.hd_bh_detail.AsNoTracking().Where(t => t.b_no == no_temp).ToList();
                        if (infos.Count > 0)
                        {
                            foreach (var item in infos)
                            {
                                #region 商品单位查询
                                //需要把单位编号转换为中文以便UI显示
                                int unitID = 1;
                                int.TryParse(item.unit, out unitID);
                                string dw = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == unitID).Select(t => t.txt1).FirstOrDefault();
                                #endregion
                                MXList.Add(new RRGoodsModel
                                {
                                    barCodeTM = item.tm,
                                    goods = item.cname,
                                    spec = item.spec,
                                    unit = dw,
                                    countNum = item.amount,
                                    JianShu = item.hpack_size,
                                    lsPrice = item.ls_price,
                                    Extant = 0  //这个现存不知取什么数据 
                                });
                            }
                        }

                    }
                    //DeShowGoods();  //隐藏
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("补货订单明细列表查询发生异常：" + ex);
                    MessageBox.Show("数据库连接出错！");
                    string tip = ConnectionHelper.ToDo();
                    if (!string.IsNullOrEmpty(tip))
                    {
                        MessageBox.Show(tip);
                    }
                }
            }
            else
            {
                tipForm = new TipForm();
                tipForm.Tiplabel.Text = "请选中您要查询的主单";
                tipForm.ShowDialog();
            }
        }

        //根据时间段批量查询订单
        private void FindOrderByDateTime()
        {
            try
            {
                var startTime = this.dateTimePicker1.Value;
                var endTime = this.dateTimePicker2.Value.AddDays(1); //不知为什么如果不推前一天的话总是查不到结束那天的数据……
                if (BHmainNoteList.Count > 0) BHmainNoteList.Clear();  //每次批量查询时都先清空上次记录
                using (var db = new hjnbhEntities())
                {
                    var time_order = db.hd_bh_info.AsNoTracking().Where(t => t.ctime >= startTime && t.ctime <= endTime).OrderBy(t => t.id).ToList();
                    if (time_order.Count > 0)
                    {

                        foreach (var item in time_order)
                        {
                            int sta = item.b_status.HasValue ? item.b_status.Value : -1;
                            string sta_temp = string.Empty;
                            switch (sta)
                            {
                                case 0:
                                    sta_temp = "未审"; //状态
                                    break;
                                case 1:
                                    sta_temp = "已审"; //状态
                                    break;
                                case 2:
                                    sta_temp = "反审"; //状态
                                    break;
                                case -1:
                                    sta_temp = "空"; //状态
                                    break;
                            }

                            BHmainNoteList.Add(new BHInfoNoteModel
                            {
                                Bno = item.b_no,
                                CID = item.cid.HasValue ? item.cid.Value : 0,
                                CTime = item.ctime.HasValue ? item.ctime.Value : Convert.ToDateTime("0001-01-01 01:01:01"),
                                ATime = item.a_time.HasValue ? item.a_time.Value : Convert.ToDateTime("0001-01-01 01:01:01"),
                                OID = item.o_id.HasValue ? item.o_id.Value : 0,
                                AID = item.a_id.HasValue ? item.o_id.Value : 0,
                                Bstatus = sta_temp
                            });
                        }
                        //NotShowDataColumn(); //隐藏 
                    }
                    else
                    {
                        tipForm = new TipForm();
                        tipForm.Tiplabel.Text = "没有查询到此时间段内的订单，请核实您的查询时间！";
                        tipForm.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("补货订单明细时间段批量查询发生异常：" + ex);
                MessageBox.Show("数据库连接出错！");
                string tip = ConnectionHelper.ToDo();
                if (!string.IsNullOrEmpty(tip))
                {
                    MessageBox.Show(tip);
                }
            }

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

        //自动隐藏列表1不需要显示的列
        private void ReplenishRequestForm_Activated(object sender, EventArgs e)
        {
            //NotShowDataColumn();
        }


        //禁止列表1获取焦点,防止快捷键冲突
        private void dataGridView1_Enter(object sender, EventArgs e)
        {
            button6.Focus();
        }

        private void dateTimePicker1_Enter(object sender, EventArgs e)
        {
        }

        private void dateTimePicker2_Enter(object sender, EventArgs e)
        {
            Tipslabel.Text = "请选择截止日期并按F2键进行查询";

        }

        private void button6_Enter(object sender, EventArgs e)
        {
            Tipslabel.Text = "按上下键浏览单据，按Enter键查询单据明细";
        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            UpdateNameFunc();
        }

        //调整表格1的列宽、同时隐藏不需要显示的列、禁止编辑、修改列名
        private void UpdateNameFunc()
        {
            try
            {
                //列名
                dataGridView1.Columns[1].HeaderText = "单号";
                dataGridView1.Columns[3].HeaderText = "经手人";
                dataGridView1.Columns[4].HeaderText = "操作日";
                dataGridView1.Columns[5].HeaderText = "审核日";
                dataGridView1.Columns[7].HeaderText = "制单";
                dataGridView1.Columns[9].HeaderText = "审核";
                dataGridView1.Columns[10].HeaderText = "状态";


                //隐藏      
                dataGridView1.Columns[0].Visible = false;  //隐藏货号
                dataGridView1.Columns[2].Visible = false;  //隐藏经手人ID
                dataGridView1.Columns[6].Visible = false;  //隐藏制作人ID
                dataGridView1.Columns[8].Visible = false;   //隐藏审核人ID
                dataGridView1.Columns[11].Visible = false;
                dataGridView1.Columns[16].Visible = false;
                dataGridView1.Columns[15].Visible = false;
                dataGridView1.Columns[14].Visible = false;
                dataGridView1.Columns[13].Visible = false;
                dataGridView1.Columns[12].Visible = false;

            }
            catch
            {
            }
        }

        //调整表格2的列宽、同时隐藏不需要显示的列、禁止编辑、修改列名
        private void UpdateNameFunc2()
        {
            try
            {
                //列名
                dataGridView2.Columns[1].HeaderText = "序";
                dataGridView2.Columns[2].HeaderText = "条码";
                dataGridView2.Columns[3].HeaderText = "品名";
                dataGridView2.Columns[4].HeaderText = "规格";
                dataGridView2.Columns[5].HeaderText = "单位";
                dataGridView2.Columns[6].HeaderText = "数量";
                dataGridView2.Columns[7].HeaderText = "件数";
                dataGridView2.Columns[8].HeaderText = "单价";
                dataGridView2.Columns[10].HeaderText = "现存";


                //隐藏      
                dataGridView2.Columns[0].Visible = false;  //隐藏货号
                dataGridView2.Columns[11].Visible = false;
                dataGridView2.Columns[9].Visible = false;   //隐藏拼音
                dataGridView2.Columns[10].Visible = false;  //现存

            }
            catch
            {
            }
        }
        private void dataGridView2_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            UpdateNameFunc2();
        }

    }
}
