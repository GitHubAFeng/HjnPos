﻿using Common;
using hjn20160520.Models;
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
    public partial class TiKuanForm : Form
    {
        public TiKuanForm()
        {
            InitializeComponent();
        }

        private void TiKuanForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.Close();
                    break;
                case Keys.Enter:
                    TKFunc();
                    break;
            }
        }

        /// <summary>
        /// 提款处理
        /// </summary>
        /// <param name="TK">金额</param>
        private void TiKuanFunc(decimal TK)
        {
            //钱箱金额
            decimal AllJE = HandoverModel.GetInstance.Money + HandoverModel.GetInstance.SaveMoney;
            if (TK > AllJE)
            {
                MessageBox.Show("钱箱金额不足！请确认输入数值是否正确？");
                textBox1.Focus();
                textBox1.SelectAll();
            }
            else
            {
                //提款还未做上传
                HandoverModel.GetInstance.DrawMoney += TK;
                MessageBox.Show("提款成功！已提出金额：" + TK.ToString() + "元");
            }

        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' && e.KeyChar != '.' || e.KeyChar > '9' && e.KeyChar != '.' || ((TextBox)(sender)).Text.IndexOf('.') >= 0 && e.KeyChar == '.') && e.KeyChar != (char)13 && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }

        private void TiKuanForm_Load(object sender, EventArgs e)
        {
            //钱箱金额
            decimal alltemp = HandoverModel.GetInstance.Money + HandoverModel.GetInstance.SaveMoney;
            label7.Text = alltemp.ToString();
            textBox1.Text = "";
            textBox1.Focus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TKFunc();

        }


        //提款
        private void TKFunc()
        {
            try
            {

                decimal TK_temp = 0;  //提款金额
                if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
                {
                    decimal.TryParse(textBox1.Text.Trim(), out TK_temp);
                }

                int qxid_temp = 0; //权限工号
                int qxzm_temp = 0;  //权限密码

                if (!string.IsNullOrEmpty(textBox2.Text.Trim()))
                {
                    int.TryParse(textBox2.Text.Trim(), out qxid_temp);
                }

                if (!string.IsNullOrEmpty(textBox3.Text.Trim()))
                {
                    int.TryParse(textBox3.Text.Trim(), out qxzm_temp);
                }

                //检证用户权限    
                using (var db = new hjnbhEntities())
                {
                    //目前数据库中没有挂账上限字段，先用挂零的字段实现功能先……
                    var res = db.hd_sys_qx.AsNoTracking().Where(t => (t.usr_id == qxid_temp) && (t.zm == qxzm_temp)).FirstOrDefault();

                    if (res != null)
                    {

                        decimal temp = res.mje.HasValue ? res.mje.Value : 0;
                        if (TK_temp <= temp)
                        {
                            //权限通过，可以处理
                            TiKuanFunc(TK_temp);
                        }
                        else
                        {
                            MessageBox.Show("提款金额数目过大，您的权限不足！");
                            textBox1.Focus();
                            textBox1.SelectAll();
                        }

                    }
                    else
                    {

                        MessageBox.Show("权限工号或者密码错误！");
                        textBox2.Focus();
                        textBox2.SelectAll();
                    }

                }
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("会员提款窗口进行提款时出现异常:", e);
                MessageBox.Show("会员提款出现异常！请检查数值输入是否正常，必要时候请联系管理员！");

            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //钱箱金额
            decimal AllJE = HandoverModel.GetInstance.Money + HandoverModel.GetInstance.SaveMoney;
            if (string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                this.label7.Text = AllJE.ToString("0.00");
            }
            else
            {
                decimal temp = 0;
                if (decimal.TryParse(textBox1.Text.Trim(), out temp))
                {
                    AllJE -= temp;
                    this.label7.Text = AllJE.ToString("0.00");
                }
            }
        }



    }
}
