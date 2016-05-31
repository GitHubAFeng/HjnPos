﻿using System;
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
    //public delegate void goodsChooseBarHander(string barCode, string goods, string unit, string spec, string retaols, string pinYin);


    #endregion





    public partial class Cashiers : Form
    {


        //string TestDB = ConfigurationManager.ConnectionStrings["TestEntities"].ConnectionString;
        public static Cashiers GetInstance { get; private set; }

        public ChoiceGoods choice;

        TestEntities db = new TestEntities();

        //数据显示序号
        int id = 1;


        public List<string> GoodsList = new List<string>();

        #region 商品属性
        

        //条码
        public string barCode { get;  set; }

        //商名
        public string goods { get;  set; }

        //单位
        public string unit { get;  set; }

        //规格
        public string spec { get;  set; }

        //零售价
        public string retaols { get;  set; }

        //拼音
        public string pinYin { get;  set; }

        //数量
        private float count = 1.00f;

        public float CountNum
        {
            get { return count; }
            set { count = value; }
        }
        

        //金额
        public float sum { get; set; }

        //营业员
        public string salesClerk { get; set; }

        //货号
        public string noCode { get; set; }

        //原价
        public string orig { get; set; }

        #endregion

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

            if (GetInstance == null) GetInstance = this;

            //this.dataGridView1.Rows.Add("1", "测试");


        }

        private void label_timer_Click(object sender, EventArgs e)
        {

        }




        //根据条码通过EF进行模糊查询
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {

                string temptxt = textBox1.Text.Trim();

                var rules = db.HjnDemoData.Where(t => t.BarCode.Contains(temptxt))
                    .Select(t => new {BarCode = t.BarCode, Goods = t.Goods, unit = t.Unit, spec = t.spec, retails = t.UnitPrice, pinyin = t.Pinyin })
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
                    foreach (var item in rules)
                    {

                        this.barCode = item.BarCode;
                        this.goods = item.Goods;
                        this.unit = item.unit;
                        this.spec = item.spec;
                        this.retaols = Convert.ToString(item.retails);
                        this.pinYin = item.pinyin;

                        GoodsList.Add(item.BarCode);
                        DataShow();
                    }
                    
                    

                }


                BindingSource bb = new BindingSource();
                bb.DataSource = rules;
                    
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

        //刷新数据显示
        public void DataShow()
        {
            if (GoodsList.Count > 0 && GoodsList.Contains(this.barCode))
            {
                int rowcount = dataGridView_Cashiers.Rows.Count;
                int cellcount = dataGridView_Cashiers.Rows[0].Cells.Count;
                for (int i = 0; i < rowcount; i++)
                {
                    for (int j = 0; j < cellcount; j++)
                    {
                        if (this.barCode == dataGridView_Cashiers.Rows[i].Cells[j].Value.ToString())
                        {

                            int val = Convert.ToInt32(dataGridView_Cashiers.Rows[i].Cells[5].Value);
                            int val_count = val + 1;
                            dataGridView_Cashiers.Rows[i].Cells[5].Value = val_count;

                            float val_8 = float.Parse(dataGridView_Cashiers.Rows[i].Cells[5].Value.ToString());
                            float temp = float.Parse(this.retaols);
                            dataGridView_Cashiers.Rows[i].Cells[8].Value = temp * val_8;

                        }
                    }
                }
            }
            else
            {
                this.orig = this.retaols;
                this.salesClerk = "测试人员";
                this.noCode = "NO";
                float temp = float.Parse(this.retaols);
                this.sum = this.CountNum * temp;

                string[] row = { id.ToString(), this.noCode, this.barCode, this.goods, this.spec, this.CountNum.ToString(), this.orig, this.retaols, this.sum.ToString(), this.salesClerk };

                this.dataGridView_Cashiers.Rows.Add(row);
                id++;
            }


        }

        //判断用户是否修改单元格
        private void dataGridView_Cashiers_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {


        }

        //用户结束修改
        private void dataGridView_Cashiers_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

            var v = dataGridView_Cashiers.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            float temp = float.Parse(this.retaols);
            float tempV = float.Parse(v);
            dataGridView_Cashiers.Rows[e.RowIndex].Cells[e.ColumnIndex + 3].Value = tempV * temp;


        }




    }
}
