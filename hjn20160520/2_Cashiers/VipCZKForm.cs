﻿using hjn20160520.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace hjn20160520._2_Cashiers
{
    /// <summary>
    /// 会员储值卡消费
    /// </summary>
    public partial class VipCZKForm : Form
    {
        ClosingEntries CE;
        //这是委托与事件的第一步  
        public delegate void VipCZKFormHandle(decimal CzkJe, decimal DJJe, decimal FQJe, BindingList<VipFQModel> FqList);
        public event VipCZKFormHandle changed;

        private BindingList<VipFQModel> VipFqList = new BindingList<VipFQModel>(); //分期数据源

        decimal setemp = 0;  //目前可用储卡余额
        decimal djtemp = 0;  //目前可用定金余额

        private decimal usedCzkJe = 0;  //使用储卡金额
        private decimal usedDJJe = 0;  //使用定金金额
        private decimal usedFQJe = 0;  //使用分期金额

        public VipCZKForm()
        {
            InitializeComponent();
        }

        private void VipCZKForm_Load(object sender, EventArgs e)
        {
            CE = this.Owner as ClosingEntries;
            ShowSE(); //读取储值余额
            LoadFQdataFunc();  //读取定金与分期

            if (setemp < CE.JE)
            {
                textBox1.Text = setemp.ToString();
            }
            else
            {
                textBox1.Text = CE.JE.ToString();
            }




            this.dataGridView1.DataSource = VipFqList;
            dataGridView1.ClearSelection();

            textBox1.Focus();
            textBox1.SelectAll();
        }

        private void VipCZKForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    KaFunc();

                    break;

                case Keys.Escape:
                    this.Close();

                    break;

                case Keys.F1:
                    SelectUsedFunc();

                    break;

            }
        }

        //回车确定
        private void KaFunc()
        {
            //使用储值
            if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                if (decimal.TryParse(textBox1.Text.Trim(), out usedCzkJe))
                {
                    if (usedCzkJe > setemp)
                    {
                        usedCzkJe = setemp;
                        //MessageBox.Show("输入的金额不能大于应付金额！");
                    }
                }

            }

            //使用定金
            if (!string.IsNullOrEmpty(textBox2.Text.Trim()))
            {
                if (decimal.TryParse(textBox2.Text.Trim(), out usedDJJe))
                {
                    if (usedDJJe > djtemp)
                    {
                        usedDJJe = djtemp;

                    }
                }
            
            }

            //使用分期
            usedFQJe = VipFqList.Where(t => t.Used).Select(t => t.MQJE).Sum();

            changed(usedCzkJe, usedDJJe, usedFQJe, VipFqList);

        }

        //显示余额
        private void ShowSE()
        {
            using (var db = new hjnbhEntities())
            {
                var SE = db.hd_vip_info.AsNoTracking().Where(t => t.vipcode == HandoverModel.GetInstance.VipID).Select(t => t.czk_ye).FirstOrDefault();
                if (SE != null)
                {
                    setemp = SE.Value;
                    label3.Text = SE.ToString();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            KaFunc();

        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                //列名
                dataGridView1.Columns[0].HeaderText = "序号";
                dataGridView1.Columns[1].HeaderText = "是否使用";
                dataGridView1.Columns[2].HeaderText = "会员姓名";
                dataGridView1.Columns[3].HeaderText = "分期金额";
                dataGridView1.Columns[4].HeaderText = "过期时间";

                //隐藏
                dataGridView1.Columns[5].Visible = false;
                dataGridView1.Columns[6].Visible = false;
                dataGridView1.Columns[7].Visible = false;  

                //禁止编辑单元格
                //设置单元格是否可以编辑
                //for (int i = 0; i < dataGridView1.Columns.Count; i++)
                //{
                //    if (dataGridView1.Columns[i].Index != 5)
                //    {
                //        dataGridView1.Columns[i].ReadOnly = true;
                //    }
                //}

            }
            catch 
            {

            }
        }



        //读取分期数据 与 定金数据
        private void LoadFQdataFunc()
        {
            int vipid = HandoverModel.GetInstance.VipID;
            int index_ = 1;
            using (var db = new hjnbhEntities())
            {
                var FQinfo = db.hd_vip_fq.AsNoTracking().Where(t => t.vipcode == vipid && t.amount > 0).ToList();
                if (FQinfo.Count > 0)
                {
                    foreach (var item in FQinfo)
                    {
                        for (int i = 0; i < item.amount; i++)
                        {
                            var newfqinfo = new VipFQModel
                            {
                                indexid = index_,
                                id = item.id,
                                vipCode = item.vipcode.Value,
                                vipName = item.vipname,
                                MQJE = item.mqje.Value,
                                ValiTime = item.valitime.HasValue ? item.valitime.Value.ToString("yyyy/MM/dd") : "无",
                                amount = item.amount.Value,
                                Used = false
                            };

                            VipFqList.Add(newfqinfo);

                            index_++;
                        }

                    }

                    //if (VipFqList.Count > 0)
                    //{
                    //    dataGridView1.Refresh();
                    //}
 
                }

                //定金
                var djinfo = db.hd_vip_info.AsNoTracking().Where(t => t.vipcode == vipid).Select(t => t.ydje).FirstOrDefault();
                if (djinfo != null)
                {
                    djtemp = djinfo.Value;
                    label9.Text = djinfo.ToString();
                    textBox2.Text = djinfo.ToString();
                }


            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SelectUsedFunc();
        }


        //选择使用
        private void SelectUsedFunc()
        {
            if (dataGridView1.RowCount > 0 )
            {
                if(dataGridView1.SelectedRows.Count > 0)
                {
                    int rowid = dataGridView1.SelectedRows[0].Index;
                    VipFqList[rowid].Used = !VipFqList[rowid].Used;
                }
                else
                {
                    dataGridView1.Rows[0].Selected = true;
                    VipFqList[0].Used = !VipFqList[0].Used;
                }


                dataGridView1.Refresh();

                int usedcount = 0; //期数
                decimal usedjetemp = 0.00m; //使用金额
                foreach (var item in VipFqList)
                {
                    if (item.Used)
                    {
                        usedcount++;
                        usedjetemp += item.MQJE;
                    }
                }

                label6.Text = usedcount.ToString();
                label11.Text = usedjetemp.ToString();
            }

        }





    }
}
