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
    //处理补货商品申请逻辑
    public partial class RequsetNoteForm : Form
    {
        //单例
        public static RequsetNoteForm GetInstance { get;private set; }
        //提示信息窗口
        TipForm tipForm;


        //商品列表 
        public BindingList<RRGoodsModel> GoodsList = new BindingList<RRGoodsModel>();
        //记录从数据库查到的商品
        public BindingList<RRGoodsModel> GoodsChooseList = new BindingList<RRGoodsModel>();


        public RequsetNoteForm()
        {
            InitializeComponent();
        }

        private void RequsetNoteForm_Load(object sender, EventArgs e)
        {
            if (GetInstance == null) GetInstance = this;
            tipForm = new TipForm();
            textBox1.Focus();
            dataGridView1.DataSource = GoodsList;
            label16.Text = HandoverModel.GetInstance.RoleID.ToString();  //员工ID
        }


        //重写热键方法，实现ESC退出，Enter选择
        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
        {
            int WM_KEYDOWN = 256;
            int WM_SYSKEYDOWN = 260;
            if (msg.Msg == WM_KEYDOWN | msg.Msg == WM_SYSKEYDOWN)
            {
                switch (keyData)
                {
                    //ESC退出
                    case Keys.Escape:
                        this.Close();//esc关闭窗体
                        break;
                    //按回车
                    case Keys.Enter:
                        FindGoodsByTM();
                        break;
                    case Keys.Up:
                        UpFun();
                        break;
                    case Keys.Down:
                        DownFun();
                        break;

                }

            }
            return false;
        }


        //根据条码查询商品的方法
        private void FindGoodsByTM()
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
                        .Select(t => new { noCode = t.item_id, BarCode = t.tm, Goods = t.cname, unit = t.unit, spec = t.spec, retails = t.ls_price, pinyin = t.py })
                        .OrderBy(t => t.pinyin)
                        .ToList();

                //如果查出数据不至一条就弹出选择窗口，否则直接显示出来

                if (rules.Count == 0)
                {

                    this.textBox1.SelectAll();
                    tipForm.Tiplabel.Text = "没有查找到该商品!";
                    tipForm.ShowDialog();
                    return;
                }
                //查询到多条则弹出商品选择窗口，排除表格在正修改时发生判断
                if (rules.Count > 1 && !dataGridView1.IsCurrentCellInEditMode)
                {

                    var form1 = new RRChooseGoodsForm();


                    foreach (var item in rules)
                    {

                        GoodsChooseList.Add(new RRGoodsModel { noCode = item.noCode, barCodeTM = item.BarCode, goods = item.Goods, unit = item.unit.ToString(), spec = item.spec, lsPrice = item.retails.ToString(),PinYin=item.pinyin });

                    }


                    form1.dataGridView1.DataSource = GoodsChooseList;
                    //隐藏不需要显示的列
                    form1.dataGridView1.Columns[0].Visible = false;
                    form1.dataGridView1.Columns[1].Visible = false;
                    form1.dataGridView1.Columns[6].Visible = false;
                    form1.dataGridView1.Columns[7].Visible = false;
                    form1.dataGridView1.Columns[10].Visible = false;

                    form1.ShowDialog();

                }
                //只查到一条如果没有重复的就直接上屏，除非表格正在修改数量
                if (rules.Count == 1 && !dataGridView1.IsCurrentCellInEditMode)
                {
                    RRGoodsModel newGoods_temp = new RRGoodsModel();
                    foreach (var item in rules)
                    {
                        newGoods_temp = new RRGoodsModel { noCode = item.noCode, barCodeTM = item.BarCode, goods = item.Goods, unit = item.unit.ToString(), spec = item.spec, lsPrice = item.retails.ToString(), PinYin = item.pinyin };

                    }

                    if (GoodsList.Count == 0)
                    {
                        GoodsList.Add(newGoods_temp);
                    }
                    else
                    {

                        if (GoodsList.Any(n => n.noCode == newGoods_temp.noCode))
                        {
                            var o = GoodsList.Where(p => p.noCode == newGoods_temp.noCode);
                            foreach (var _item in o)
                            {
                                _item.countNum++;
                            }
                            dataGridView1.Refresh();
                        }
                        else
                        {
                            GoodsList.Add(newGoods_temp);
                        }

                    }

                }
            }

            //每次查询后全选
            textBox1.SelectAll();
            if (GoodsList.Count > 0)
            {
                dataGridView1.Columns[0].Visible = false;  //隐藏货号
                dataGridView1.Columns[9].Visible = false;  //隐藏拼音
            }
        }


        //用户从商品选择窗口选中的商品,如果补货清单中已存在该商品则数量加1，否则新增
        public void UserChooseGoods(int index)
        {
            if (GoodsList.Any(k => k.noCode == GoodsChooseList[index].noCode))
            {
                var se = GoodsList.Where(h => h.noCode == GoodsChooseList[index].noCode);
                foreach (var item in se)
                {
                    item.countNum++;
                }

                dataGridView1.Refresh();

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


        }


        //补货申请单成单
        private void BHNoteFunc()
        {
            var BhInfo = new BHInfoNoteModel();



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

        #region 自动在数据表格首列绘制序号
        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
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





    }
}
