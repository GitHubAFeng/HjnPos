using hjn20160520.Common;
using hjn20160520.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hjn20160520._9_VIPCard
{
    public partial class VIPCardForm : Form
    {
        //单例
        public static VIPCardForm GetInstance { get;private set; }
        //public VIPmodel vip;
        TipForm tipForm;

        public VIPCardForm()
        {
            InitializeComponent();
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.label17.Text = DateTime.Now.ToString();
        }

        private void VIPCardForm_Load(object sender, EventArgs e)
        {
            if (GetInstance == null) GetInstance = this;
            this.timer1.Start();
            textBox10.Focus();
            tipForm = new TipForm();
            //设置下拉的默认值
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 1;
            comboBox3.SelectedIndex = 0;

        }

        private void VIPCardForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    if (ValiFun())
                    {
                        tipForm.code = 2;  //2为会员办理功能扩展信息提示框的识别码
                        tipForm.Tiplabel.Text = "您已经填写部分内容，是否确认退出会员办理？";
                        tipForm.label3.Text = "按ESC键清空内容并退出，按回车键继续办理…";
                        tipForm.ShowDialog();
                    }
                    else
                    {
                        this.Close();
                    }

                    break;

                case Keys.Enter:
                    if (SaveVIP())
                    {
                        
                        tipForm.Tiplabel.Text = "会员办理成功！";
                        tipForm.ShowDialog();
                    }
                    else
                    {
                        tipForm.Tiplabel.Text = "会员办理失败！请核实网络连接是否正常，会员信息的卡号、姓名、电话不可为空！";
                        tipForm.ShowDialog();
                    }

                    break;
            }
        }
        //创建新会员实例，个别会员信息可能以后按需要修改
        VIPmodel CreateVIP()
        {
            VIPmodel vip = new VIPmodel();

            if (!string.IsNullOrEmpty(textBox10.Text))
                vip.vipCard = textBox10.Text.Trim();

            if (!string.IsNullOrEmpty(textBox1.Text))
                vip.password = int.Parse(textBox1.Text.Trim());

            if (!string.IsNullOrEmpty(textBox2.Text))
                vip.vipName = textBox2.Text.Trim();

            if (!string.IsNullOrEmpty(textBox3.Text))
            vip.Tel = textBox3.Text.Trim();

            if (!string.IsNullOrEmpty(comboBox1.Text))
            {
                vip.other1 = comboBox1.Text;
            }
            else
            {
                vip.other1 = "未知";
            }
                

            if (!string.IsNullOrEmpty(textBox5.Text))
                vip.email = textBox5.Text.Trim();

            vip.birthday = dateTimePicker1.Value.ToString();

            if (!string.IsNullOrEmpty(textBox6.Text))
                vip.address = textBox6.Text.Trim();

            if (!string.IsNullOrEmpty(textBox11.Text))
                vip.id_No = textBox11.Text.Trim();

            if (!string.IsNullOrEmpty(textBox7.Text))
                vip.BDJE = float.Parse(textBox7.Text.Trim());

            if (!string.IsNullOrEmpty(comboBox2.Text))
            {
                vip.vipTypeStr = comboBox2.Text;

            }
            else
            {
                vip.vipTypeStr = "未知";
            }

            if (!string.IsNullOrEmpty(comboBox3.Text))
                vip.vipStatusStr = comboBox3.Text;

            if (checkBox1.Checked)
            {
                vip.valiDate = "永不过期";
            }
            else
            {
                vip.valiDate = dateTimePicker2.Value.ToString();
            }

            return vip;
        }


        //关闭窗口时判断表上有没有内容，防止误操作,如果为true则说明有内容
        bool ValiFun()
        {
            bool isHave = false;
            if (textBox10.Text.Trim() != string.Empty||
                textBox1.Text.Trim() != string.Empty ||
                textBox2.Text.Trim() != string.Empty ||
                textBox3.Text.Trim() != string.Empty ||
                textBox5.Text.Trim() != string.Empty ||
                textBox6.Text.Trim() != string.Empty ||
                textBox11.Text.Trim() != string.Empty ||
                textBox7.Text.Trim() != string.Empty
                )
            {
                isHave = true;
            }
            return isHave;
        }


        //按回车键时往数据库中插入会员
        private bool SaveVIP()
        {
            var tempVIP = CreateVIP();
            int tempType = 0;
            int tempStatus = 0;

            if (tempVIP.vipTypeStr == "普通会员")
                tempType = 1;
            if (tempVIP.vipTypeStr == "黄金会员")
                tempType = 2;
            if (tempVIP.vipTypeStr == "钻石会员")
                tempType = 3;
            if (tempVIP.vipTypeStr == "通用会员")
                tempType = 0;
            if (tempVIP.vipTypeStr == "锁定")
                tempType = 101;

            switch (tempVIP.vipStatusStr)
            {
                case "正常":
                    tempStatus = 0;
                    break;
                case "过期":
                    tempStatus = -1;
                    break;

                case "作废" :
                    tempStatus = -2;
                    break;
            }

            if (string.IsNullOrEmpty(textBox10.Text) ||
                string.IsNullOrEmpty(textBox2.Text)||
                string.IsNullOrEmpty(textBox3.Text)||
                string.IsNullOrEmpty(comboBox2.Text)||
                string.IsNullOrEmpty(comboBox3.Text))
            {
                return false;             
            }

            using (hjnbhEntities db = new hjnbhEntities())
            {
                var vipInfo = new hd_vip_info
                {
                    vipcard = tempVIP.vipCard,
                    password = tempVIP.password,
                    vipname = tempVIP.vipName,
                    tel = tempVIP.Tel,
                    other2 = tempVIP.other1,
                    Email = tempVIP.email,
                    id_no = tempVIP.id_No,
                    bdje = (decimal)tempVIP.BDJE,
                    viptype = (byte)tempType,
                    cstatus = tempStatus,
                    validate = Convert.ToDateTime(tempVIP.valiDate),
                    address = tempVIP.address,
                    ljxfje = 0,
                    zkh = 0,
                    cid = 0,
                    utime = Convert.ToDateTime(tempVIP.valiDate),
                    uid = 0,
                    yje = 0,
                    jfnum = 0   //这个数据在会员查询列表中将使用，如果不赋值可能会报空指针异常
                };

                db.hd_vip_info.Add(vipInfo);
                //MessageBox.Show(db.SaveChanges().ToString());
                return db.SaveChanges() > 0;


            }

        }


        //退出办理时清空内容
        public void ClearTextBoxOnESC()
        {
            textBox10.Text = "";
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
            textBox11.Text = "";
            textBox7.Text = "";
        }





    }
}
