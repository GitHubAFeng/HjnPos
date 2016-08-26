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

namespace hjn20160520._2_Cashiers
{
    /// <summary>
    /// 挂单窗口
    /// </summary>
    public partial class GoodsNote : Form
    {
        CashiersFormXP CFFormXp;

        public delegate void GoodsNoteFormHandle(int va ,int inde);
        public event GoodsNoteFormHandle changed;  //传递事件

        public GoodsNote()
        {
            InitializeComponent();
        }

        private void GoodsNote_Load(object sender, EventArgs e)
        {
            CFFormXp = this.Owner as CashiersFormXP;
            //this.FormBorderStyle = FormBorderStyle.None;//无边框
            this.ActiveControl = this.dataGridViewGN1;
            ShowItemFunc();
            //this.dataGridViewGN1.Focus();

        }



        //重写热键方法，实现ESC退出，Enter选择
        //protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
        //{
        //    int WM_KEYDOWN = 256;
        //    int WM_SYSKEYDOWN = 260;
        //    if (msg.Msg == WM_KEYDOWN | msg.Msg == WM_SYSKEYDOWN)
        //    {
        //        switch (keyData)
        //        {
        //            //ESC退出
        //            case Keys.Escape:

        //                this.Close();//esc关闭窗体
        //                break;
        //            //按回车
        //            case Keys.Enter:

        //                if (dataGridViewGN1.RowCount > 0)
        //                {
        //                    try
        //                    {

        //                        int temp = Convert.ToInt32(dataGridViewGN1.SelectedRows[0].Cells[0].Value);
        //                        int ind = dataGridViewGN1.SelectedRows[0].Index;
        //                        //CashiersFormXP.GetInstance.GetNoteByorder(temp);

        //                        //CashiersFormXP.GetInstance.noteList.RemoveAt(dataGridViewGN1.SelectedRows[0].Index);
        //                        changed(temp, ind);

        //                        if (dataGridViewGN1.RowCount == 0)
        //                        {
        //                            dataGridViewDN2.DataSource = null;
        //                        }

        //                        this.Close();
        //                    }
        //                    catch
        //                    {

        //                    }
        //                }


        //                break;

        //        }

        //    }
        //    return false;
        //}

        //实时显示订单里的商品清单
        private void dataGridViewGN1_SelectionChanged(object sender, EventArgs e)
        {
            ShowItemFunc();
        }

        //实时显示订单里的商品清单
        private void ShowItemFunc()
        {
            if (dataGridViewGN1.RowCount > 0)
            {
                try
                {


                    dataGridViewDN2.DataSource = null;

                    int temp = Convert.ToInt32(dataGridViewGN1.SelectedRows[0].Cells[0].Value);


                    var templist = CFFormXp.NoteSeleOrder(temp).Select(t => new { t.noCode, t.barCodeTM, t.goods, t.countNum, t.lsPrice, t.Sum }).ToArray();
                    dataGridViewDN2.DataSource = templist;

                    this.dataGridViewDN2.Refresh();

                }
                catch
                {

                }
            }
        }

        #region 自动在数据表格首列绘制序号
        private void dataGridViewDN2_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SetDataGridViewRowXh(e, dataGridViewDN2);
        }

        //在首列绘制序号，如果首列原有内容，会出现重叠，所以首列留空
        private void SetDataGridViewRowXh(DataGridViewRowPostPaintEventArgs e, DataGridView dataGridView)
        {
            SolidBrush solidBrush = new SolidBrush(Color.Black); //更改序号样式
            int xh = e.RowIndex + 1;
            e.Graphics.DrawString(xh.ToString(CultureInfo.CurrentUICulture), e.InheritedRowStyle.Font, solidBrush, e.RowBounds.Location.X + 5, e.RowBounds.Location.Y + 4);
        }
        #endregion


        //否决
        private void GoodsNote_Activated(object sender, EventArgs e)
        {
        }

        private void dataGridViewGN1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            UpdateNameFunc();
        }

        //调整表格1的列宽、同时隐藏不需要显示的列、禁止编辑、修改列名
        private void UpdateNameFunc()
        {
            try
            {
                //列名
                dataGridViewGN1.Columns[0].HeaderText = "单号";
                dataGridViewGN1.Columns[1].HeaderText = "业务员";
                dataGridViewGN1.Columns[2].HeaderText = "收银员";
                dataGridViewGN1.Columns[3].HeaderText = "时间";
                dataGridViewGN1.Columns[4].HeaderText = "金额";
                //dataGridViewGN1.Columns[11].HeaderText = "拼音";

                //列宽   
                //dataGridViewGN1.Columns[2].Width = 200;

            }
            catch
            {
            }
        }

        private void dataGridViewDN2_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            UpdateNameFunc2();
        }

        //调整表格2的列宽、同时隐藏不需要显示的列、禁止编辑、修改列名
        private void UpdateNameFunc2()
        {
            try
            {
                //列名
                dataGridViewDN2.Columns[1].HeaderText = "货号";
                dataGridViewDN2.Columns[2].HeaderText = "条码";
                dataGridViewDN2.Columns[3].HeaderText = "品名";
                dataGridViewDN2.Columns[4].HeaderText = "数量";
                dataGridViewDN2.Columns[5].HeaderText = "单价";
                dataGridViewDN2.Columns[6].HeaderText = "金额";

                //列宽   
                //dataGridViewGN1.Columns[2].Width = 200;

            }
            catch
            {
            }
        }

        private void GoodsNote_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                //ESC退出
                case Keys.Escape:

                    this.Close();//esc关闭窗体
                    break;
                //按回车
                case Keys.Enter:

                    if (dataGridViewGN1.RowCount > 0)
                    {
                        try
                        {

                            int temp = Convert.ToInt32(dataGridViewGN1.SelectedRows[0].Cells[0].Value);
                            int ind = dataGridViewGN1.SelectedRows[0].Index;
                            //CashiersFormXP.GetInstance.GetNoteByorder(temp);

                            //CashiersFormXP.GetInstance.noteList.RemoveAt(dataGridViewGN1.SelectedRows[0].Index);
                            changed(temp, ind);

                            if (dataGridViewGN1.RowCount == 0)
                            {
                                dataGridViewDN2.DataSource = null;
                            }

                            this.Close();
                        }
                        catch
                        {

                        }
                    }


                    break;

            }
        }



















    }
}
