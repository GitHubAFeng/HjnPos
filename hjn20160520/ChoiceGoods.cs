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



        Cashiers cashiersForm;



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


        private void panel4_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics,
                this.panel4.ClientRectangle,
                Color.White,//7f9db9
                1,
                ButtonBorderStyle.Solid,
                Color.White,
                0,
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
            cashiersForm = new Cashiers();
  
        }

        private void ChoiceGoods_KeyDown(object sender, KeyEventArgs e)
        {

        }


        //重写热键方法，实现ESC退出，Enter进入
        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
        {
            int WM_KEYDOWN = 256;
            int WM_SYSKEYDOWN = 260;
            if (msg.Msg == WM_KEYDOWN | msg.Msg == WM_SYSKEYDOWN)
            {
                switch (keyData)
                {
                    case Keys.Escape:
                        this.Close();//esc关闭窗体
                        break;
                    case  Keys.Enter:
                        MessageBox.Show("Test");
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
