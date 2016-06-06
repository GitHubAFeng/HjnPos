using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
            //this.dataGridViewDN2.DataSource = Cashiers.GetInstance.noteGoodsList;
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

                        break;

                }

            }
            return false;
        }

        private void dataGridViewGN1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewGN1.RowCount > 0)
            {
                try
                {
                
                    //Cashiers.GetInstance.noteGoodsList.Clear();
                    dataGridViewDN2.DataSource = null;

                    int temp = Convert.ToInt32(dataGridViewGN1.SelectedRows[0].Cells[0].Value);

                    //Cashiers.GetInstance.NoteSeleOrder(temp);

                    //dataGridViewDN2.DataSource = Cashiers.GetInstance.noteGoodsList;
                    dataGridViewDN2.DataSource = Cashiers.GetInstance.NoteSeleOrder(temp);

                    this.dataGridViewDN2.Refresh();

                }
                catch
                {
                    
                }
            }
        }
























    }
}
