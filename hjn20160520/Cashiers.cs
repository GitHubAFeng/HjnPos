using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Configuration;

namespace hjn20160520
{

    #region 与商品选择窗口交互的委托事件
    //public delegate void goodsChooseBarHander(string message);


    #endregion

    public partial class Cashiers : Form
    {

        //public event goodsChooseBarHander goodsBarEvent;

        //string TestDB = ConfigurationManager.ConnectionStrings["TestEntities"].ConnectionString;


        public ChoiceGoods choice;



        public Cashiers()
        {
            InitializeComponent();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label_timer.Text = " 当前时间：" + System.DateTime.Now.ToString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
                        //全屏
            //if (this.WindowState == FormWindowState.Maximized)
            //{
            //    this.WindowState = FormWindowState.Normal;
            //}
            //else
            //{
            //    this.FormBorderStyle = FormBorderStyle.None;
            //    this.WindowState = FormWindowState.Maximized;
            //    this.TopMost = true;  //窗口顶置
            //}

            timer1.Start();
            

            choice = new ChoiceGoods();

        }

        private void label_timer_Click(object sender, EventArgs e)
        {

        }




        //根据条码通过EF进行模糊查询
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //MessageBox.Show("aaaa");

                string temptxt = textBox1.Text.Trim();

                using (var db = new TestEntities())
                {
                    var rules = db.HjnDemoData.Where(t => t.BarCode.Contains(temptxt))
                        .Select(t => new { BarCode = t.BarCode, Goods = t.Goods, unit = t.Unit, spec = t.spec, retails = t.UnitPrice, pinyin = t.Pinyin })
                        .OrderBy(t => t.pinyin)
                        .ToList();


                    if (rules.Count > 1)
                    {
                        var form1 = new ChoiceGoods();
                        form1.dataGridView1.DataSource = rules;
                        form1.ShowDialog();
                    }
                    else
                    {


                        this.dataGridView1.DataSource = rules;

                    }



                    BindingSource bb = new BindingSource();
                    bb.DataSource = rules;
                    




                }


            }
        }

        //窗体在屏幕居中
        private void Cashiers_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.DarkOliveGreen, 0, 0, this.Width - 1, this.Height - 1);
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        //清除数据显示的空格
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value is string)
                e.Value = e.Value.ToString().Trim();
        }


        //窗体激活事件
        private void Cashiers_Activated(object sender, EventArgs e)
        {
            //if (textBox1.Focused && dataGridView1.RowCount > 0)
            //{
            //    this.dataGridView1.Focus();
            //    this.dataGridView1.Rows[0].Selected = true;
            //}


            //if (ActiveControl is DataGridView)
            //{

            //        MessageBox.Show(dataGridView1.RowCount.ToString());
                
            //}


        }










    }
}
