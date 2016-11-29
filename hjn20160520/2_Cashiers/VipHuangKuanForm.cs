using hjn20160520.Common;
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
    /// 会员还款
    /// </summary>
    public partial class VipHuangKuanForm : Form
    {
        MemberPointsForm MPFrom;
        decimal VipQKJE = 0; //变量的
        decimal qkje = 0; //不变的

        int hkvipcode = 0;  //来还款的会员

        public delegate void VipHuangKuanFormHandle(decimal je);
        public event VipHuangKuanFormHandle changed;  //传递还款金额

        private BindingList<VipHqModel> viphqList = new BindingList<VipHqModel>();  //欠款数据列表

        public VipHuangKuanForm()
        {
            InitializeComponent();
        }

        private void VipHuangKuanForm_Load(object sender, EventArgs e)
        {
            MPFrom = this.Owner as MemberPointsForm;
            this.VipQKJE = this.qkje = MPFrom.VipQKJE;
            this.hkvipcode = MPFrom.hkvipcode;
            this.label5.Text = this.VipQKJE.ToString("0.00");
            this.textBox1.Text = "";
            this.textBox1.Focus();

            viphqList.Clear();
            dataGridView1.DataSource = viphqList;

            QKDataFunc();
        }

        private void VipHuangKuanForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.Close();
                    break;

                case Keys.Enter:
                    OKFunc();
                    break;
                case Keys.F1:
                    alltorepay();
                    break;
            }
        }

        private void alltorepay()
        {
            this.textBox1.Text = qkje.ToString("0.00");
            this.textBox1.Focus();
            this.textBox1.SelectAll();
        }


        private void button2_Click(object sender, EventArgs e)
        {
            alltorepay();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OKFunc();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' && e.KeyChar != '.' || e.KeyChar > '9' && e.KeyChar != '.' || ((TextBox)(sender)).Text.IndexOf('.') >= 0 && e.KeyChar == '.') && e.KeyChar != (char)13 && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            this.VipQKJE = MPFrom.VipQKJE;
            if (string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                this.label5.Text = this.VipQKJE.ToString("0.00");
            }
            else
            {
                decimal temp = 0;
                if (decimal.TryParse(textBox1.Text.Trim(), out temp))
                {
                    this.VipQKJE -= temp;
                    this.label5.Text = this.VipQKJE.ToString("0.00");
                }
            }

        }


        private void OKFunc()
        {
            if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                decimal temp = 0;
                if (decimal.TryParse(textBox1.Text.Trim(), out temp))
                {
                    if (this.VipQKJE < 0)
                    {
                        MessageBox.Show("会员欠款金额不可以小于零！请检查是否输入正确？");
                    }
                    else
                    {
                        changed(temp);
                        this.Close();
                    }

                }
                else
                {
                    MessageBox.Show("您输入的金额不正确！请重新输入");
                }
            }
        }


        //查询欠款
        private void QKDataFunc()
        {
            using (var db = new hjnbhEntities())
            {
                var qkinfo = db.hd_vip_qk.AsNoTracking().Where(t => t.vipcode == hkvipcode && t.type == 0).ToList();
                if (qkinfo.Count > 0)
                {
                    foreach (var item in qkinfo)
                    {
                        decimal qktemp = item.qkje.HasValue ? item.qkje.Value : 0.00m;
                        decimal hktemp = item.hkje.HasValue ? item.hkje.Value : 0.00m;
                        if (qktemp > hktemp)
                        {
                            viphqList.Add(new VipHqModel
                            {
                                cidStr = item.cid.HasValue ? item.cid.Value.ToString() : "",
                                ctime = item.ctime.HasValue ? item.ctime.Value.ToString("yyyy-MM-dd") : "",
                                jscode = item.js_code,
                                qkje = qktemp - hktemp,
                                scodeStr = item.scode.HasValue ? item.scode.Value.ToString() : ""
                            });
                        }

                    }
                }
            }

        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            UpdateNameFunc();
        }


        private void UpdateNameFunc()
        {
            try
            {
                //列名
                dataGridView1.Columns[0].HeaderText = "单号";
                dataGridView1.Columns[1].HeaderText = "欠款";
                dataGridView1.Columns[2].HeaderText = "分店";
                dataGridView1.Columns[3].HeaderText = "收银员";
                dataGridView1.Columns[4].HeaderText = "时间";

                dataGridView1.Columns[0].Width = 130; 

            }
            catch
            {
            }
        }



    }
}
