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
    public partial class GoodsNote : Form
    {
        public GoodsNote()
        {
            InitializeComponent();
        }

        private void GoodsNote_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.None;//无边框
            this.dataGridViewGN1.Focus();

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

                        if (dataGridViewGN1.RowCount > 0)
                        {
                            try
                            {

                                int temp = Convert.ToInt32(dataGridViewGN1.SelectedRows[0].Cells[0].Value);

                                Cashiers.GetInstance.GetNoteByorder(temp);

                                Cashiers.GetInstance.noteList.RemoveAt(dataGridViewGN1.SelectedRows[0].Index);

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
            return false;
        }

        //实时显示订单里的商品清单
        private void dataGridViewGN1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewGN1.RowCount > 0)
            {
                try
                {
                

                    dataGridViewDN2.DataSource = null;

                    int temp = Convert.ToInt32(dataGridViewGN1.SelectedRows[0].Cells[0].Value);


                    var templist = Cashiers.GetInstance.NoteSeleOrder(temp).Select(t => new { t.noCode, t.barCodeTM, t.goods, t.countNum, t.lsPrice, t.Sum }).ToArray();
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
            SolidBrush solidBrush = new SolidBrush(Color.White); //更改序号样式
            int xh = e.RowIndex + 1;
            e.Graphics.DrawString(xh.ToString(CultureInfo.CurrentUICulture), e.InheritedRowStyle.Font, solidBrush, e.RowBounds.Location.X + 5, e.RowBounds.Location.Y + 4);
        }
        #endregion


        //否决
        private void GoodsNote_Activated(object sender, EventArgs e)
        {
            //隐藏不需要的列
              //dataGridViewDN2.Columns[4].Visible = false;
              //dataGridViewDN2.Columns[7].Visible = false;
              //dataGridViewDN2.Columns[10].Visible = false;
              //dataGridViewDN2.Columns[12].Visible = false;
              //dataGridViewDN2.Columns[11].Visible = false;
              //dataGridViewDN2.Columns[6].Visible = false;

            //dataGridViewDN2.ClearSelection();

        }























    }
}
