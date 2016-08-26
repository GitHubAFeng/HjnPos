using Common;
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

namespace hjn20160520._2_Cashiers
{
    public partial class VipShopForm : Form
    {
        TipForm tipForm; 
        int vipid = 0;   //所查VIP号
        //用于其它窗口传值给本窗口控件
        //这是委托与事件的第一步  
        public delegate void VipShopFormHandle(string s);
        public event VipShopFormHandle changed;
        //传递给收银的vipID
        public delegate void VIPHandle(int vipid, string vipcrad ,int viplv);
        public event VIPHandle VIPchanged; 

        public VipShopForm()
        {
            InitializeComponent();
        }

        private void VipShopForm_Load(object sender, EventArgs e)
        {
            this.textBox1.SelectAll();
        }

        //快捷键
        private void VipShopForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    VipCardFunc();

                    break;

                case Keys.Escape:
                    this.Close();
                    break;

                case Keys.F11:
                    //if (CashiersFormXP.GetInstance.VipID == 0)
                    //{
                    //    MessageBox.Show("目前没有录入会员");
                    //}
                    //else
                    //{
                    //    DialogResult RSS = MessageBox.Show(this, "是否取消已录入的会员？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    //    switch (RSS)
                    //    {
                    //        case DialogResult.Yes:
                    //            CashiersFormXP.GetInstance.VipID = 0;
                    //            break;
                    //    }
                    //    //CashiersFormXP.GetInstance.VipID = 0;
                    //    //MessageBox.Show("取消已录入的会员");
                    //}
                    break;
            }
        }


        //会员卡查询，预计可能要传递一些优惠信息，但目前先查会员姓名
        public void VipCardFunc(string card = "")
        {
            //try
            //{
                string text_temp = string.Empty;
                if (card != "")
                {
                    text_temp = card;
                }
                else
                {
                    if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
                    {
                        text_temp = textBox1.Text.Trim();
                    }

                }

                if (string.IsNullOrEmpty(text_temp)) return;

                using (var db = new hjnbhEntities())
                {
                    //手机号也能查
                    var vipInfos = db.hd_vip_info.AsNoTracking().Where(t => t.vipcard == text_temp || t.tel == text_temp)
                        .Select(t => new { t.vipname, t.vipcard, t.end_date, t.cstatus, t.vipcode, t.viptype, t.Birthday }).FirstOrDefault();


                    if (text_temp == "-1")
                    {
                        CashiersFormXP.GetInstance.isVipBirthday = false;  //会员生日
                        VIPchanged(0, "", 0);  //传递会员ID
                        CashiersFormXP.GetInstance.XSHDFunc(db);
                        CashiersFormXP.GetInstance.YHHDFunc(db);
                     
                        changed("未录入");  //传递会员名字
                        this.Close();
                        return;
                    }

                    if (vipInfos != null)
                    {
                        ////查会员等级
                        //var vipLV = db.hd_vip_info.AsNoTracking().Where(t => t.vipcode == vipInfos.vipcode).Select(t => t.viptype).FirstOrDefault();
                        ////查会员生日
                        //var vipBirthday = db.hd_vip_info.AsNoTracking().Where(t => t.vipcode == vipInfos.vipcode).Select(t => t.Birthday).FirstOrDefault();

                        if (System.DateTime.Now > vipInfos.end_date)
                        {
                            MessageBox.Show("此会员卡已经过期！不能使用！");
                        }
                        else if (vipInfos.cstatus != 0)
                        {
                            MessageBox.Show("此会员卡处于非正常状态！不能使用！");
                        }
                        else
                        {
                            if (CashiersFormXP.GetInstance != null)
                            {
                                int viplvInt = vipInfos.viptype.HasValue ? (int)vipInfos.viptype.Value : 0;
                                //bool isvipBirthday = false;
                                if (vipInfos.Birthday.HasValue)
                                {
                                    if (vipInfos.Birthday.Value.Date.Month == System.DateTime.Today.Month && vipInfos.Birthday.Value.Date.Day == System.DateTime.Today.Day)
                                    {
                                        //isvipBirthday = true;
                                        CashiersFormXP.GetInstance.isVipBirthday = true;  //会员生日
                                    }
                                    else
                                    {
                                        CashiersFormXP.GetInstance.isVipBirthday = false;  //会员生日
                                    }
                                }
                                else
                                {
                                    CashiersFormXP.GetInstance.isVipBirthday = false;  //会员生日
                                }

                                vipid = vipInfos.vipcode;
                                VIPchanged(vipid, vipInfos.vipcard, viplvInt);
                                string temp_name = vipInfos.vipname;
                                CashiersFormXP.GetInstance.XSHDFunc(db);
                                CashiersFormXP.GetInstance.YHHDFunc(db);

                                changed(temp_name);  //事件传值
                            }

                            //if (ClosingEntries.GetInstance != null) ClosingEntries.GetInstance.VIPShowUI();
                        }

                        this.Close();

                    }
                    else
                    {
                        tipForm = new TipForm();
                        tipForm.Tiplabel.Text = "查询失败，请核实会员卡号是否正确！";
                        tipForm.ShowDialog();
                        textBox1.SelectAll();
                    }



                }

            //}
            //catch (Exception e)
            //{
            //    LogHelper.WriteLog("会员录入窗口登记时出现异常:", e);
            //    MessageBox.Show("数据库连接出错！");
            //    string tip = ConnectionHelper.ToDo();
            //    if (!string.IsNullOrEmpty(tip))
            //    {
            //        MessageBox.Show(tip);
            //    }
            //}

        }

        /// <summary>
        /// 取消会员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {

            if (CashiersFormXP.GetInstance.VipID == 0)
            {
                MessageBox.Show("目前没有录入会员");
            }
            else
            {
                DialogResult RSS = MessageBox.Show(this, "是否取消已录入的会员？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                switch (RSS)
                {
                    case DialogResult.Yes:
                        CashiersFormXP.GetInstance.VipID = 0;

                        CashiersFormXP.GetInstance.HDUIFunc();
                        break;
                }
                //CashiersFormXP.GetInstance.VipID = 0;
                //MessageBox.Show("取消已录入的会员");
            }
        }


    }
}
