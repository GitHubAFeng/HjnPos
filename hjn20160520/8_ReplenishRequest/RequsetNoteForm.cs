﻿using Common;
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
using System.Transactions;
using System.Windows.Forms;

namespace hjn20160520._8_ReplenishRequest
{
    //处理补货商品申请逻辑
    public partial class RequsetNoteForm : Form
    {

        //提示信息窗口
        TipForm tipForm;


        public RequsetNoteForm()
        {
            InitializeComponent();
        }

        private void RequsetNoteForm_Load(object sender, EventArgs e)
        {
            tipForm = new TipForm();
            textBox1.Focus();
            dataGridView1.DataSource = ReplenishRequestForm.GetInstance.GoodsList;
            this.comboBox5.SelectedIndex = 0;
            DeShowGoods(); //隐藏列
            InitRNForm(); //初始化
        }

        //初始化新单窗口
        private void InitRNForm()
        {
            button1.Enabled = false;
            textBox1.Enabled = textBox2.Enabled = true;
            label7.Text = System.DateTime.Now.ToString("yyyy-MM-dd");  //制单日
            label11.Text = "";  //审核日
            label5.Text = "未传送";  //单号
            label27.Text = "";
            label28.Text = "";
            textBox1.Text = "";
            textBox2.Text = "00";
            switch (HandoverModel.GetInstance.RoleID)
            {
                case 1:
                    label10.Text = label6.Text = "系统管理员";
                    break;
                case 2:
                    label10.Text = label6.Text = "后台操作员";
                    break;
                case 3:
                    label10.Text = label6.Text = "前台操作员";
                    break;
                case 4:
                    label10.Text = label6.Text = "业务员";
                    break;
                case 5:
                    label10.Text = label6.Text = "测试人员";
                    break;
                default:
                    label10.Text = label6.Text = "收银员";
                    break;
            }
            ReplenishRequestForm.GetInstance.isMK = false;
            ReplenishRequestForm.GetInstance.GoodsList.Clear();
        }
        //统计单数与数量合计
        private void ShowUIFunc()
        {
            label27.Text = ReplenishRequestForm.GetInstance.GoodsList.Count.ToString();
            int temp = 0;
            foreach (var item in ReplenishRequestForm.GetInstance.GoodsList)
            {
                temp += item.countNum;
            }

            label28.Text = temp.ToString();
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
                    case Keys.Up:
                        UpFun();
                        break;
                    case Keys.Down:
                        DownFun();
                        break;
                    case Keys.F1:
                        textBox1.Focus();
                        textBox1.SelectAll();
                        break;
                    case Keys.F2:
                        textBox2.Focus();
                        textBox2.SelectAll();
                        break;
                    case Keys.F4:
                        OnMakeTureFunc();
                        break;
                    //好像没什么用，我隐藏先
                    //case Keys.F5:
                    //    this.Close();
                    //    break;
                    case Keys.F6:
                        OnUploadFunc();
                        break;
                }

            }
            return false;
        }

        //处理上传的逻辑
        private void OnUploadFunc()
        {
            if (ReplenishRequestForm.GetInstance.isMK)
            {
                UpdataDBFunc();

            }
            else
            {
                tipForm.Tiplabel.Text = "您的单据还未经过审核不能发送！";
                tipForm.ShowDialog();
            }
        }


        //根据条码查询商品的方法
        private void FindGoodsByTM()
        {
            int tempcount =0;
            if (!string.IsNullOrEmpty(textBox2.Text.Trim()))
            {
                tempcount = int.Parse(textBox2.Text.Trim());

            }

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
                    ReplenishRequestForm.GetInstance.GoodsChooseList.Clear();

                    foreach (var item in rules)
                    {

                        ReplenishRequestForm.GetInstance.GoodsChooseList.Add(new RRGoodsModel { noCode = item.noCode, barCodeTM = item.BarCode, goods = item.Goods, unit = item.unit.ToString(), spec = item.spec, countNum = tempcount, lsPrice = (float)item.retails, PinYin = item.pinyin });

                    }


                    form1.dataGridView1.DataSource = ReplenishRequestForm.GetInstance.GoodsChooseList;
                    //隐藏不需要显示的列
                    form1.dataGridView1.Columns[0].Visible = false;
                    form1.dataGridView1.Columns[1].Visible = false;
                    form1.dataGridView1.Columns[5].Visible = false;
                    form1.dataGridView1.Columns[7].Visible = false;
                    form1.dataGridView1.Columns[10].Visible = false;
                    form1.dataGridView1.Columns[11].Visible = false;

                    form1.ShowDialog();

                }
                //只查到一条如果没有重复的就直接上屏，除非表格正在修改数量
                if (rules.Count == 1 && !dataGridView1.IsCurrentCellInEditMode)
                {
                    RRGoodsModel newGoods_temp = new RRGoodsModel();
                    foreach (var item in rules)
                    {
                        newGoods_temp = new RRGoodsModel { noCode = item.noCode, barCodeTM = item.BarCode, goods = item.Goods,countNum = tempcount, unit = item.unit.ToString(), spec = item.spec, lsPrice = (float)item.retails, PinYin = item.pinyin };

                    }

                    if (ReplenishRequestForm.GetInstance.GoodsList.Count == 0)
                    {
                        ReplenishRequestForm.GetInstance.GoodsList.Add(newGoods_temp);
                    }
                    else
                    {

                        if (ReplenishRequestForm.GetInstance.GoodsList.Any(n => n.noCode == newGoods_temp.noCode))
                        {
                            var o = ReplenishRequestForm.GetInstance.GoodsList.Where(p => p.noCode == newGoods_temp.noCode);
                            foreach (var _item in o)
                            {
                                _item.countNum += tempcount;
                            }
                            dataGridView1.Refresh();
                        }
                        else
                        {
                            ReplenishRequestForm.GetInstance.GoodsList.Add(newGoods_temp);
                        }

                    }
                    textBox1.Text = newGoods_temp.barCodeTM;
                }
            }

            //每次查询后全选
            textBox1.SelectAll();
            DeShowGoods();  //隐藏一些列
        }


        #region 审核单据
        

        //处理审核逻辑(交给信息提示窗口处理)
        private void OnMakeTureFunc()
        {
            if (ReplenishRequestForm.GetInstance.isMK) return;  //如果已经审核过就不要才审核了
            if (ReplenishRequestForm.GetInstance.GoodsList.Count == 0)
            {
                tipForm.Tiplabel.Text = "请先填写需要审核的商品";
                tipForm.ShowDialog();
            }
            else
            {
                tipForm.code = 3;
                tipForm.Tiplabel.Text = "您是否确定审核此单？";
                tipForm.ESClabel.Text = "按ESC键返回，按Enter键确定……";
                tipForm.ShowDialog();
            }

        }


        #endregion

        #region 创建主单据
        //接收时间
        DateTime? time;
        //单据状态
        int status = 0;  //0为未发送，1为已发送
        //创建结算单据
        //补货申请单成单
        private BHInfoNoteModel BHNoteFunc()
        {
            var BhInfo = new BHInfoNoteModel();
            BhInfo.CID = HandoverModel.GetInstance.RoleID;  //制作人ID
            time = BhInfo.CTime = System.DateTime.Now;  //制单时间
            BhInfo.ATime = ReplenishRequestForm.GetInstance.MKtime;  //审核时间
            BhInfo.OID = this.comboBox5.SelectedIndex;  //经办人ID
            BhInfo.AID = HandoverModel.GetInstance.RoleID;  //审核人ID
            switch (status)
            {
                case 0:
                    BhInfo.Bstatus = "未发送"; //状态
                    break;
                case 1:
                    BhInfo.Bstatus = "已发送"; //状态
                    break;
            }

            return BhInfo;
        }


        #endregion

        #region 向数据库上传补货单
        //单号计算方式，当前时间+00000+id
        long no_temp = Convert.ToInt64(System.DateTime.Now.ToString("yyyyMMdd") + "000000");

        private void UpdataDBFunc()
        {
            using (var db = new hjnbhEntities())
            {
                using (var scope = new TransactionScope())
                {                   
                    var BHnote = BHNoteFunc();
                    var HDBH = new hd_bh_info
                    {

                        cid = BHnote.CID,
                        ctime = BHnote.CTime,
                        bh_time = BHnote.BHtime,   //补货时间
                        b_status = 1,
                        b_type = BHnote.BHtype,
                        zd_time = BHnote.ZDtime,
                        bt_change_time = BHnote.changeTime,
                        o_id = BHnote.OID,
                        scode = BHnote.scode,
                        a_id = BHnote.AID,
                        a_time = BHnote.ATime,
                        del_flag = (byte?)BHnote.delFlag
                    };

                    db.hd_bh_info.Add(HDBH);
                    db.SaveChanges(); //保存一次才能生效
                    string noteNO = label5.Text = "BHS" + (no_temp + HDBH.id).ToString();  //获取ID并生成补货单号
                    HDBH.b_no = noteNO;

                    foreach (var item in ReplenishRequestForm.GetInstance.GoodsList)
                    {
                        //部分字段没有赋值
                        var BHMX = new hd_bh_detail
                        {
                            b_no = noteNO,
                            item_id = item.noCode,
                            tm = item.barCodeTM,
                            cname = item.goods,
                            spec = item.spec,
                            unit = item.unit,
                            amount = (decimal)item.countNum,    //数量为输入值(如果不转换类型的话，这值总是0)
                            ls_price = (decimal)item.lsPrice,

                        };

                        db.hd_bh_detail.Add(BHMX);
                    }

                    BHnote.Bno = noteNO;
                    BHnote.Bstatus = "已发送";
                    db.SaveChanges();
                    scope.Complete();  //提交事务

                    ReplenishRequestForm.GetInstance.BHmainNoteList.Add(BHnote); //发送后的单据放入表单中


                }
            }
        }

        #endregion


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


        //默认隐藏商品列表不需要显示的列
        private void DeShowGoods()
        {
            
            if (ReplenishRequestForm.GetInstance.GoodsList.Count > 0)
            {
                dataGridView1.Columns[0].Visible = false;  //隐藏货号
                dataGridView1.Columns[11].Visible = false;
                dataGridView1.Columns[9].Visible = false;   //隐藏拼音
                dataGridView1.Columns[7].Visible = false;  //暂时隐藏了单位

            }
        }

        //数量输入框回车事件
        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (!string.IsNullOrEmpty(this.textBox2.Text.Trim()))
                    {
                        FindGoodsByTM();
                    }

                    this.textBox1.Focus();
                    this.textBox1.SelectAll();
                    break;
            }
        }

        //条码输入框回车事件
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (!string.IsNullOrEmpty(this.textBox1.Text.Trim()))
                    {
                        this.textBox2.Focus();
                        this.textBox2.SelectAll();
                    }
                    break;
            }
        }

        //限制数量输入框只能输入数字，只能整数
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (e.KeyChar == 0x20) e.KeyChar = (char)0;  //禁止空格键  
            if ((e.KeyChar == 0x2D) && (((TextBox)sender).Text.Length == 0)) return;   //处理负数  
            if (e.KeyChar > 0x20)
            {
                try
                {
                    double.Parse(((TextBox)sender).Text + e.KeyChar.ToString());
                }
                catch
                {
                    e.KeyChar = (char)0;   //处理非法字符  
                }
            } 
        }

        //F4按钮点击
        private void button1_Click(object sender, EventArgs e)
        {
            OnMakeTureFunc();
        }

        //判断有列表有内容时才允许审核
        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            if (dataGridView1.RowCount > 0 )
            {
                if (button1.Enabled == false)
                {
                    button1.Enabled = true;
                    
                }
            }
            else
            {
                if (button1.Enabled == true)
                {
                    button1.Enabled = false;

                }
            }

            ShowUIFunc(); //统计单数
        }
        //数据删除事件
        private void dataGridView1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            ShowUIFunc(); //统计单数
        }
        //F5关闭窗口，好像没什么用，我隐藏先
        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        //F6上传按钮
        private void button2_Click(object sender, EventArgs e)
        {
            OnUploadFunc();
        }


    }
}
