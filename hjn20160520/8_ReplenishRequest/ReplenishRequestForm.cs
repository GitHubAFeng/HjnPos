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

        //主菜单
        MainForm mainForm;
        //新单窗口
        RequsetNoteForm RNForm;

        //主单列表数据源
        public BindingList<BHInfoNoteModel> BHmainNoteList = new BindingList<BHInfoNoteModel>();

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
            mainForm = new MainForm();
            dataGridView1.DataSource = BHmainNoteList;
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
                    //删除DEL键
                    case Keys.Escape:

                        mainForm.Show();
                        this.Close();

                        break;

                    //回车
                    case Keys.Enter:


                        break;
                        //新单
                    case Keys.F3:
                        RNForm = new RequsetNoteForm();
                        RNForm.ShowDialog();

                        break;


                }

            }
            return false;
        }

        #endregion

        //F3按钮
        private void button2_Click(object sender, EventArgs e)
        {
            RNForm = new RequsetNoteForm();
            RNForm.ShowDialog();
        }

        //Del删除按钮
        private void button4_Click(object sender, EventArgs e)
        {

        }
        //F4修改按钮
        private void button3_Click(object sender, EventArgs e)
        {

        }
        //ESC关闭按钮
        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        //F2日期查询按钮
        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(DateTime.Now.ToString("yyyyMMdd"));
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







    }
}
