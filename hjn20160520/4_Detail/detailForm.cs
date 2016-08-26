using Common;
using ExcelData;
using hjn20160520.Common;
using hjn20160520.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hjn20160520._4_Detail
{
    public partial class detailForm : Form
    {

        //主菜单窗口
        MainFormXP MainFormXP;
        //提示信息
        TipForm tipForm;

        //主单列表
        private BindingList<MainNoteModel> RNList = new BindingList<MainNoteModel>();
        //明细列表
        private BindingList<DetailNoteDataModel> RDNList = new BindingList<DetailNoteDataModel>();

        public detailForm()
        {
            InitializeComponent();
        }

        private void detailForm_Load(object sender, EventArgs e)
        {
            MainFormXP = new MainFormXP();

            //设置时间格式
            StartdateTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            StartdateTime.CustomFormat = "yyyy-MM-dd";

            //绑定主单数据源
            dataGridView1.DataSource = RNList;
            //绑定明细数据源
            dataGridView2.DataSource = RDNList;

            //设置时间控件格式
            StartdateTime.Format = DateTimePickerFormat.Custom;
            EnddateTime.Format = DateTimePickerFormat.Custom;
            EnddateTime.CustomFormat = StartdateTime.CustomFormat = "yyyy年MM月dd日";

            textBox1.Focus();
            textBox1.SelectAll();

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

                        //MainFormXP.Show();
                        this.Close();

                        break;


                    //回车
                    case Keys.Enter:
                        //如果用户没输入订单的话就默认使用时间段查询
                        if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
                        {
                            string id_temp = textBox1.Text.Trim();
                            FindRNoteByIDFunc(id_temp);
                            //NotShow(); //隐藏
                        }
                        else
                        {
                            FindOrderByDateTime();
                        }
                        ShowUIFunc(); //刷新UI
                        textBox1.Focus();
                        textBox1.SelectAll();
                        RDNList.Clear(); //每次清空明细
                        break;
                    //查询明细
                    case Keys.F9:
                        FindDetailByGuid();

                        break;
                    case Keys.Up:
                        UpFun();
                        //dataGridView1.Focus();
                        break;
                    case Keys.Down:
                        //dataGridView1.Focus();
                        DownFun();
                        break;
                    case Keys.F12:
                        FindOrderByDateTime();
                        ShowUIFunc(); //刷新UI
                        break;

                }

            }
            return false;
        }

        #endregion

        #region 数据库查询


        //根据主单ID查询单据的方法
        private void FindRNoteByIDFunc(string id)
        {
            if (RNList.Count > 0) RNList.Clear(); //每次查询都清空上次结果
            using (var db = new hjnbhEntities())
            {
                try
                {
                    var note = db.hd_ls.AsNoTracking().Where(t => t.v_code == id);    //查零售单
                    var ceNo = db.hd_js.AsNoTracking().Where(t => t.ls_code == id);   //查结算单
                    var YMID_temp = note.Select(t => t.ywy).FirstOrDefault();  //获取主单号业务员工号
                    //获取业务员工名字
                    var ywyName = db.user_role_view.AsNoTracking().Where(t => t.usr_id == YMID_temp).Select(t => t.usr_name).FirstOrDefault();
                    string jsNO_temp = ceNo.Select(t => t.v_code).FirstOrDefault();  //获取结算单号
                    if (jsNO_temp != null)
                    {
                        //抹零值
                        var moling_temp = ceNo.Select(t => t.moling).FirstOrDefault();
                        //总金额
                        var money_temp = ceNo.Select(t => t.ysje).FirstOrDefault();
                        //收银员工ID
                        var cidID = ceNo.Select(t => t.cid).FirstOrDefault();
                        //获取员工名字
                        var cidName = db.user_role_view.AsNoTracking().Where(t => t.usr_id == cidID).Select(t => t.usr_name).FirstOrDefault();
                        //结算清单
                        var notes = note.ToList();
                        foreach (var item in notes)
                        {
                            RNList.Add(new MainNoteModel
                            {
                                ID = item.v_code, //订单号
                                YMID = YMID_temp,  //业务员ID
                                YwyStr = ywyName + "(" + YMID_temp + ")",  //业务员工名字
                                CID = item.cid.HasValue ? item.cid.Value : 0,
                                //CID=cidID,
                                cidStr = cidName + "(" + cidID + ")",
                                cTiem = item.ctime, //订单时间
                                YSJE = money_temp,  //实收金额
                                MoLing = moling_temp.HasValue ? moling_temp.Value : 0,

                            });
                        }
                    }
                    else
                    {
                        tipForm = new TipForm();
                        tipForm.Tiplabel.Text = "没有此单记录！请确认单号是否正确";
                        tipForm.ShowDialog();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("订单明细主单查询异常:", ex);  //输出异常日志
                    MessageBox.Show("数据库连接出错！");
                    string tip = ConnectionHelper.ToDo();
                    if (!string.IsNullOrEmpty(tip))
                    {
                        MessageBox.Show(tip);
                    }
                }
            }


        }

        //根据主单查询明细
        private void FindDetailByGuid()
        {
            if (RDNList.Count > 0) RDNList.Clear();  //清空先前显示的数据
            if (RNList.Count > 0)
            {
                try
                {
                    int id_temp = dataGridView1.SelectedRows[0].Index;
                    string idNO = RNList[id_temp].ID;
                    using (hjnbhEntities db = new hjnbhEntities())
                    {
                        var infos = db.hd_ls_detail.AsNoTracking().Where(t => t.v_code == idNO).ToList();
                        if (infos.Count > 0)
                        {
                            foreach (var item in infos)
                            {
                                #region 商品单位查询
                                //需要把单位编号转换为中文以便UI显示
                                int unitID = item.unit.HasValue ? (int)item.unit : 1;
                                string dw = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == unitID).Select(t => t.txt1).FirstOrDefault();
                                #endregion

                                RDNList.Add(new DetailNoteDataModel
                                {
                                    null_temp = "", //占位
                                    itemID = (int)item.item_id, //货号
                                    TM = item.tm, //条码
                                    cName = item.cname, //商品名字
                                    spec = item.spec, //规格
                                    StrDw = dw,
                                    count = item.amount, //数量 
                                    ylsPrice = item.yls_price, //单价
                                    TotalMoney = item.amount * item.yls_price //总额
                                });
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("订单明细列表查询发生异常：" + ex);
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
            //MessageBox.Show(RDNList[1].TotalMoney.ToString());

        }

        //根据时间段批量查询订单
        private void FindOrderByDateTime()
        {
            try
            {
                var startTime = this.StartdateTime.Value;
                var endTime = this.EnddateTime.Value.AddDays(1); //不知为什么如果不推前一天的话总是查不到结束那天的数据……
                if (RNList.Count > 0) RNList.Clear();  //每次批量查询时都先清空上次记录
                using (var db = new hjnbhEntities())
                {
                    var orders = db.hd_js.AsNoTracking().Where(t => t.ctime >= startTime && t.ctime <= endTime);
                    //查出时间段内的主单号
                    var time_order = orders.Select(t => t.ls_code).FirstOrDefault();
                    //查出时间段内的所有结算单
                    var time_JSorder = orders.OrderBy(t => t.id).ToList();
                    if (time_JSorder.Count > 0)
                    {
                        //找出结算单的信息
                        foreach (var item in time_JSorder)
                        {
                            //收银员工ID
                            var _cid = item.cid.HasValue ? item.cid.Value : 0;
                            //获取收银员工名字
                            var _cidname = db.user_role_view.AsNoTracking().Where(t => t.usr_id == item.cid).Select(t => t.usr_name).FirstOrDefault();
                            //在零售单查业务员ID
                            var ywyid = db.hd_ls.AsNoTracking().Where(t => t.v_code == item.v_code).Select(t => t.ywy).FirstOrDefault();
                            //再查业务员工名字
                            var ywyname = db.user_role_view.AsNoTracking().Where(t => t.usr_id == ywyid).Select(t => t.usr_name).FirstOrDefault();

                            RNList.Add(new MainNoteModel
                            {
                                ID = item.ls_code, //订单号
                                YMID = ywyid,  //业务员ID
                                YwyStr = ywyname + "(" + ywyid + ")", //业务员工名字
                                CID = _cid, //(收银员)零售员工号
                                cidStr = _cidname + "(" + _cid + ")", //收银员工名字
                                cTiem = item.ctime, //订单时间
                                YSJE = item.ysje.HasValue ? item.ysje.Value : 0,  //实收金额
                                MoLing = item.moling.HasValue ? item.moling.Value : 0,  //抹零金额

                            });
                        }

                        //NotShow(); //隐藏
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
                LogHelper.WriteLog("订单明细时间段批量查询发生异常：" + ex);
                MessageBox.Show("数据库连接出错！");
                string tip = ConnectionHelper.ToDo();
                if (!string.IsNullOrEmpty(tip))
                {
                    MessageBox.Show(tip);
                }
            }

        }

        #endregion

        #region 上下快捷键选择
        //小键盘向上
        private void UpFun()
        {
            try
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
            catch (Exception ex)
            {

                LogHelper.WriteLog("销售明细窗口小键盘向下时发生异常：" + ex);

            }
        }

        //小键盘向下
        private void DownFun()
        {
            try
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
            catch (Exception ex)
            {

                LogHelper.WriteLog("销售明细窗口小键盘向下时发生异常：" + ex);

            }
        }


        #endregion


        #region 自动在数据表格首列绘制序号
        private void dataGridView2_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SetDataGridViewRowXh(e, dataGridView2);
        }
        //在首列绘制序号，如果首列原有内容，会出现重叠，所以首列手动添加一个空列
        private void SetDataGridViewRowXh(DataGridViewRowPostPaintEventArgs e, DataGridView dataGridView)
        {
            SolidBrush solidBrush = new SolidBrush(Color.Black); //更改序号样式
            int xh = e.RowIndex + 1;
            e.Graphics.DrawString(xh.ToString(CultureInfo.CurrentUICulture), e.InheritedRowStyle.Font, solidBrush, e.RowBounds.Location.X + 5, e.RowBounds.Location.Y + 4);
        }
        #endregion

        //不让用户操作明细列表，只是用来显示数据
        private void dataGridView2_Enter(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView2.Focused)
                {
                    textBox1.Focus();
                }
            }
            catch
            {
            }
        }


        //显示订单的合计、单数、总额等的UI
        private void ShowUIFunc()
        {
            if (RNList.Count > 0)
            {
                //我不明白合计是什么意思，先让他等于单数吧……
                label12.Text = label14.Text = RNList.Count.ToString();  //合计、单数
                decimal? temp = 0;
                foreach (var item in RNList)
                {
                    temp += item.YSJE;
                }
                label16.Text = temp.ToString(); //金额
            }
        }

        //导出销售单据
        private void button1_Click(object sender, EventArgs e)
        {
            using (DataTable dt = new DataTable("单据"))
            {
                //创建列
                DataColumn dtc = new DataColumn("单号", typeof(string));
                dt.Columns.Add(dtc);

                dtc = new DataColumn("业务员", typeof(string));
                dt.Columns.Add(dtc);

                dtc = new DataColumn("收银员", typeof(string));
                dt.Columns.Add(dtc);
                dtc = new DataColumn("开单时间", typeof(DateTime));
                dt.Columns.Add(dtc);
                dtc = new DataColumn("金额", typeof(float));
                dt.Columns.Add(dtc);
                dtc = new DataColumn("抹零", typeof(float));
                dt.Columns.Add(dtc);

                foreach (var item in RNList)
                {
                    //添加数据到DataTable
                    DataRow dr = dt.NewRow();
                    dr["单号"] = item.ID;
                    dr["业务员"] = item.YwyStr;
                    dr["收银员"] = item.cidStr;
                    dr["开单时间"] = item.cTiem;
                    dr["金额"] = item.YSJE;
                    dr["抹零"] = item.MoLing;
                    dt.Rows.Add(dr);
                }

                var re = NPOIForExcel.ToExcelWrite(dt);
                if (re != "")
                {
                    MessageBox.Show("导出完成");
                }
                else
                {
                    MessageBox.Show("导出失败");
                }

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var re = NPOIForExcel.ToExcelRead();


            if (re != null)
            {
                MessageBox.Show("导入数据成功");
                dataGridView1.DataSource = re;
            }
            else
            {
                MessageBox.Show("导入数据失败");

            }
        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            UpdateNameFunc();
        }

        //调整表格的列宽、同时隐藏不需要显示的列、禁止编辑、修改列名
        private void UpdateNameFunc()
        {
            try
            {
                //列名
                dataGridView1.Columns[0].HeaderText = "单号";
                dataGridView1.Columns[2].HeaderText = "业务员";
                dataGridView1.Columns[4].HeaderText = "收银员";
                dataGridView1.Columns[5].HeaderText = "开单日期";
                dataGridView1.Columns[6].HeaderText = "金额";
                dataGridView1.Columns[7].HeaderText = "抹零";

                //隐藏
                dataGridView1.Columns[1].Visible = false;  //业务员工ID
                dataGridView1.Columns[3].Visible = false;  //收银员工ID
            }
            catch
            {
            }
        }

        private void dataGridView2_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                //列名
                dataGridView2.Columns[1].HeaderText = "货号";
                dataGridView2.Columns[2].HeaderText = "条码";
                dataGridView2.Columns[3].HeaderText = "品名";
                dataGridView2.Columns[4].HeaderText = "规格";
                dataGridView2.Columns[5].HeaderText = "单位";
                dataGridView2.Columns[6].HeaderText = "数量";
                dataGridView2.Columns[7].HeaderText = "单价";
                dataGridView2.Columns[8].HeaderText = "金额";

            }
            catch
            {
            }
        }


    }
}
