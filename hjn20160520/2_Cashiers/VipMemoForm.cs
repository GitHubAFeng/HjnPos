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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hjn20160520._2_Cashiers
{
    public partial class VipMemoForm : Form
    {
        bool isdate = false;     //判断是否已经添加了日期

        public delegate void VipMemoFormHandle();
        public event VipMemoFormHandle changed;  //传递会员备注事件

        public VipMemoForm()
        {
            InitializeComponent();
        }

        private void VipMemoForm_Load(object sender, EventArgs e)
        {
            //richTextBox1.Clear();
            richTextBox1.Text = "";
            ReaderVipInfoFunc();
            textBox1.Focus();
            textBox1.Clear();
        }

        private void VipMemoForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.Close();
                    break;

                case Keys.Enter:
                    VipInfoWriteFunc();
                    break;
            }
        }




        //读取会员消息
        private void ReaderVipInfoFunc()
        {
            try
            {
                int vipid = HandoverModel.GetInstance.VipID;
                if (vipid == 0)
                {
                    MessageBox.Show("请先登记会员!");
                    this.Close();
                }
                using (var db = new hjnbhEntities())
                {
                    var vipInfo = db.hd_vip_info.AsNoTracking().Where(t => t.vipcode == vipid).Select(t => t.sVipMemo).FirstOrDefault();
                    if (!string.IsNullOrEmpty(vipInfo))
                    {
                        StringBuilder StrB = new StringBuilder();
                        StrB.Append(TextByDateFunc(vipInfo));
                        richTextBox1.AppendText(StrB.ToString());

                    }

                }
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("会员消息备注窗口读取会员消息时出现异常:", e);
                //MessageBox.Show("数据库连接出错！");
                //string tip = ConnectionHelper.ToDo();
                //if (!string.IsNullOrEmpty(tip))
                //{
                //    MessageBox.Show(tip);
                //}
            }
        }


        //正则匹配日期
        string strEx = @"((?<!\d)((\d{2,4}(\.|年|\/|\-))((((0?[13578]|1[02])(\.|月|\/|\-))((3[01])|([12][0-9])|(0?[1-9])))|(0?2(\.|月|\/|\-)((2[0-8])|(1[0-9])|(0?[1-9])))|(((0?[469]|11)(\.|月|\/|\-))((30)|([12][0-9])|(0?[1-9]))))|((([0-9]{2})((0[48]|[2468][048]|[13579][26])|((0[48]|[2468][048]|[3579][26])00))(\.|年|\/|\-))0?2(\.|月|\/|\-)29))日?(?!\d))";
        //string strEx = @"^((((1[6-9]|[2-9]\d)\d{2})-(0?[13578]|1[02])-(0?[1-9]|[12]\d|3[01]))|(((1[6-9]|[2-9]\d)\d{2})-(0?[13456789]|1[012])-(0?[1-9]|[12]\d|30))|(((1[6-9]|[2-9]\d)\d{2})-0?2-(0?[1-9]|1\d|2[0-9]))|(((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))-0?2-29-))$";
        //按时间分割文本
        private string TextByDateFunc(string text)
        {
            return (Regex.Replace(text, strEx, "\r\n$1"));

        }

        //写入会员消息
        private void VipInfoWriteFunc()
        {
            if (HandoverModel.GetInstance.VipID <= 0) return;

            string infos = string.Empty;
            if (!string.IsNullOrEmpty(textBox1.Text))
            {

                using (var db = new hjnbhEntities())
                {
                    var Vipinfo = db.hd_vip_info.Where(t => t.vipcode == HandoverModel.GetInstance.VipID).FirstOrDefault();
                    if (Vipinfo != null)
                    {

                        StringBuilder StrVipMemo = new StringBuilder();   //输入会员消息
                        infos = textBox1.Text.Trim();

                        if (isdate)
                        {
                            richTextBox1.AppendText("  " + infos);
                            StrVipMemo.Append("  " + infos);

                            string temp = StrVipMemo.ToString();
                            Vipinfo.sVipMemo += temp;
                            if (db.SaveChanges() == 0)
                            {
                                MessageBox.Show("会员消息提交失败，请先核实该会员资料，必要时请联系管理员！");
                            }
                        }
                        else
                        {
                            richTextBox1.AppendText("\r\n" + System.DateTime.Now.Date.ToString("yyyy-MM-dd") + "  " + infos);
                            StrVipMemo.Append(System.DateTime.Now.Date.ToString("yyyy-MM-dd") + "  " + infos + "  ");
                            isdate = true;

                            string temp = StrVipMemo.ToString();
                            Vipinfo.sVipMemo += temp;
                            if (db.SaveChanges() == 0)
                            {
                                MessageBox.Show("会员消息提交失败，请先核实该会员资料，必要时请联系管理员！");
                            }
                        }

                        textBox1.Clear();
                        textBox1.Focus();
                    }

                }

                //changed(temp);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            VipInfoWriteFunc();
        }

        private void VipMemoForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            changed();
        }


    }
}
