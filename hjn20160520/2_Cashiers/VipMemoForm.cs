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
        StringBuilder StrVipMemo = new StringBuilder();   //输入会员消息
        bool isdate = false;     //判断是否已经添加了日期

        public VipMemoForm()
        {
            InitializeComponent();
        }

        private void VipMemoForm_Load(object sender, EventArgs e)
        {
            ReaderVipInfoFunc();
            textBox1.Focus();
            textBox1.SelectAll();
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
            string vipid = Cashiers.GetInstance.VipID.ToString();
            if (string.IsNullOrEmpty(vipid) || vipid == "0")
            {
                MessageBox.Show("请先在收银窗口登记会员卡号");
            }
            using (var db = new hjnbhEntities())
            {
                var vipInfo = db.hd_vip_info.AsNoTracking().Where(t => t.vipcard == vipid).Select(t => t.sVipMemo).FirstOrDefault();
                if (!string.IsNullOrEmpty(vipInfo))
                {
                    StringBuilder StrB = new StringBuilder();
                    StrB.Append(TextByDateFunc(vipInfo));
                    richTextBox1.AppendText(StrB.ToString());
                }

            }
        }


        //正则匹配日期
        string strEx = @"^((((1[6-9]|[2-9]\d)\d{2})-(0?[13578]|1[02])-(0?[1-9]|[12]\d|3[01]))|(((1[6-9]|[2-9]\d)\d{2})-(0?[13456789]|1[012])-(0?[1-9]|[12]\d|30))|(((1[6-9]|[2-9]\d)\d{2})-0?2-(0?[1-9]|1\d|2[0-9]))|(((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))-0?2-29-))$";
        //按时间分割文本
        private string TextByDateFunc(string text)
        {
            return (Regex.Replace(text, strEx, "$1\r\n\t"));

        }

        //写入会员消息
        private void VipInfoWriteFunc()
        {
            string infos = string.Empty;
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                infos = textBox1.Text;
                if (isdate)
                {
                    richTextBox1.AppendText("  " + infos);
                    StrVipMemo.Append("  " + infos);
                }
                else
                {
                    richTextBox1.AppendText("\r\n" + System.DateTime.Now.Date.ToString("yyyy-MM-dd") + "  " + infos);
                    StrVipMemo.Append(System.DateTime.Now.Date.ToString("yyyy-MM-dd") + "  " + infos);
                    isdate = true;
                }
                Cashiers.GetInstance.VipMdemo = StrVipMemo.ToString();  //传递到结算
            }

        }


    }
}
