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

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
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
                    var rules = db.HjnDemoData.Where(t => t.BarCode.Contains(temptxt)).Select(t => new { BarCode = t.BarCode, Goods = t.Goods, spec = t.spec, retails =t.UnitPrice}).ToList();


                    //foreach (var c in rules)
                    //{
                    //    MessageBox.Show(c.Goods);
                    //}

                    if (rules.Count > 1)
                    {
                        var form1 = new ChoiceGoods();
                        form1.dataGridView1.DataSource = rules;
                        form1.ShowDialog();
                        this.dataGridView1.DataSource = rules;

                        //MessageBox.Show(rules.Count.ToString());
                    }
                    else
                    {
                        this.dataGridView1.DataSource = rules;
                    }
                    
                    
         
                }


            }
        }

        private void Cashiers_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.DarkOliveGreen, 0, 0, this.Width - 1, this.Height - 1);
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value is string)
                e.Value = e.Value.ToString().Trim();
        }

















    }
}
