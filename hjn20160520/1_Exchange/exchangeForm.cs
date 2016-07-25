using Common;
using hjn20160520.Common;
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

namespace hjn20160520._1_Exchange
{
    /// <summary>
    /// 交班
    /// </summary>
    public partial class exchangeForm : Form
    {

        //主菜单
        MainForm mainform;


        public exchangeForm()
        {
            InitializeComponent();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void exchangeForm_Load(object sender, EventArgs e)
        {

            mainform = new MainForm();
            ShowUI();

        }

        //热键
        private void exchangeForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    EXFunc();
                    break;

                case Keys.Escape:
                    this.Close();
                    break;
            }
        }


        //处理交班逻辑
        private void EXFunc()
        {
            try
            {
                if (HandoverModel.GetInstance.isWorking == false)
                {
                    MessageBox.Show("您还未当班，不能进行交班操作！");
                    return;
                }

                DateTime time_temp = System.DateTime.Now;
                using (var db = new hjnbhEntities())
                {
                    var JBInfo = new hd_dborjb
                    {
                        scode = HandoverModel.GetInstance.scode,
                        bcode = HandoverModel.GetInstance.bcode,
                        usr_id = HandoverModel.GetInstance.userID,
                        cname = HandoverModel.GetInstance.userName,
                        dbje = HandoverModel.GetInstance.SaveMoney,
                        dtime = HandoverModel.GetInstance.workTime,
                        jbje = HandoverModel.GetInstance.Money,
                        jcount = HandoverModel.GetInstance.OrderCount,
                        tkje = HandoverModel.GetInstance.RefundMoney,
                        qkje = HandoverModel.GetInstance.DrawMoney,
                        jtime = time_temp
                    };
                    db.hd_dborjb.Add(JBInfo);
                    var re = db.SaveChanges();
                    if (re > 0)
                    {
                        MessageBox.Show("交班成功！");
                        //交班时间
                        label16.Text = time_temp.ToString();
                        HandoverModel.GetInstance.isWorking = false;
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("交班界面进行交班时发生异常:", ex);
                MessageBox.Show("数据库连接出错！");
                string tip = ConnectionHelper.ToDo();
                if (!string.IsNullOrEmpty(tip))
                {
                    MessageBox.Show(tip);
                }
            }
        }
        //刷新UI
        private void ShowUI()
        {
            //收银员工号
            label20.Text = HandoverModel.GetInstance.userID.ToString();
            //收银机号
            label19.Text = HandoverModel.GetInstance.bcode.ToString();
            //当班时间
            label18.Text = HandoverModel.GetInstance.workTime.ToString();
            //当班金额
            label17.Text = HandoverModel.GetInstance.SaveMoney.ToString();

            //交易单数
            label15.Text = HandoverModel.GetInstance.OrderCount.ToString();
            //退款金额
            label14.Text = HandoverModel.GetInstance.RefundMoney.ToString();
            //中途提款
            label13.Text = HandoverModel.GetInstance.DrawMoney.ToString();
            //应交金额
            label4.Text = HandoverModel.GetInstance.Money.ToString();
        }





    }
}
