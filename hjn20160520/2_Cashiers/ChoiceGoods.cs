using hjn20160520.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hjn20160520
{



    public partial class ChoiceGoods : Form
    {
        TipForm tipForm; //提示信息

        public ChoiceGoods()
        {
            InitializeComponent();
        }


        #region 更改Panel的默认边框色
        //更改Panel的默认边框色
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics,
                            this.panel1.ClientRectangle,
                            Color.White,//7f9db9
                            1,
                            ButtonBorderStyle.Solid,
                            Color.White,
                            1,
                            ButtonBorderStyle.Solid,
                            Color.White,
                            1,
                            ButtonBorderStyle.Solid,
                            Color.White,
                            1,
                            ButtonBorderStyle.Solid);

        }



        private void panel3_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics,
                            this.panel3.ClientRectangle,
                            Color.White,//7f9db9
                            1,
                            ButtonBorderStyle.Solid,
                            Color.White,
                            1,
                            ButtonBorderStyle.Solid,
                            Color.White,
                            1,
                            ButtonBorderStyle.Solid,
                            Color.White,
                            1,
                            ButtonBorderStyle.Solid);

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics,
                            this.panel2.ClientRectangle,
                            Color.White,//7f9db9
                            1,
                            ButtonBorderStyle.Solid,
                            Color.White,
                            1,
                            ButtonBorderStyle.Solid,
                            Color.White,
                            1,
                            ButtonBorderStyle.Solid,
                            Color.White,
                            1,
                            ButtonBorderStyle.Solid);
        }




        #endregion


        private void ChoiceGoods_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.None;//无边框

            this.KeyPreview = true;


        }

        private void ChoiceGoods_KeyDown(object sender, KeyEventArgs e)
        {

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
                        Cashiers.GetInstance.goodsChooseList.Clear();  //既然不需要，那么把查到的数据清空。
                        this.Close();//esc关闭窗体
                        break;
                        //按回车
                    case  Keys.Enter:
                        if (dataGridView1.SelectedRows[0] != null)
                        {
                            int temp_index = dataGridView1.SelectedRows[0].Index;
                            //先判断该商品状态是否允许销售
                            if (Cashiers.GetInstance.goodsChooseList[temp_index].status.HasValue)
                            {
                                if (Cashiers.GetInstance.goodsChooseList[temp_index].status.Value == 2)
                                {
                                    tipForm = new TipForm();
                                    tipForm.Tiplabel.Text = "此商品目前处于停止销售状态！";
                                    tipForm.ShowDialog();
                                }
                                else
                                {
                                    Cashiers.GetInstance.UserChooseGoods(temp_index);
                                    //每次选择完都要清空该列表，防止商品重复出现
                                    Cashiers.GetInstance.goodsChooseList.Clear();
                                    Cashiers.GetInstance.textBox1.Text = "";
                                    this.Close();//关闭窗体
                                }

                            }

                        }
                        else
                        {
                            MessageBox.Show("没有选中任何商品");

                        }

                        break;

                }

            }
            return false;
        }

        //清除数据字符串空格
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value is string)
                e.Value = e.Value.ToString().Trim();
        }


        //窗体关闭事件
        private void ChoiceGoods_FormClosing(object sender, FormClosingEventArgs e)
        {
            //cashiersForm.dataGridView1.Rows[0].Selected = true;
        }





    }
}
