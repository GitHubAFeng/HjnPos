using Common;
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
    /// <summary>
    /// 会员卡发行
    /// </summary>
    public partial class VIPCardForm : Form
    {
        //单例
        public static VIPCardForm GetInstance { get; private set; }
        //public VIPmodel vip;
        TipForm tipForm;
        private string vip;

        //用于其它窗口传值给本窗口控件
        //这是委托与事件的第一步  ,把新开的会员卡号传给会员查询
        public delegate void VIPHandle(string s);
        public event VIPHandle changed;

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
            FindCardFunc();
            checkBox1.Checked = true;
        }

        //自动查询可用的会员卡(选填)
        private void FindCardFunc()
        {
            try
            {
                using (var db = new hjnbhEntities())
                {
                    var maxID_temp = db.hd_vip_info.AsNoTracking().Max(t => t.vipcode);
                    //var maxcards_temp = db.hd_vip_info.AsNoTracking().Where(e => e.vipcode == maxID_temp).Select(e => e.vipcard).FirstOrDefault();
                    textBox10.Text = (maxID_temp + 100001).ToString();
                    textBox10.SelectAll();
                }
            }
            catch (Exception)
            {

                MessageBox.Show("无法自动查询可用会员卡！");
            }

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
                        tipForm.ESClabel.Text = "按ESC键清空内容并退出，按回车键继续办理…";
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
                        tipForm.code = 2;
                        tipForm.Tiplabel.Text = "会员办理成功！会员卡号为：" + vip;
                        tipForm.ESClabel.Text = "按ESC键清空会员信息并退出，按回车键继续编辑……";
                        tipForm.ShowDialog();

                    }
                    else
                    {
                        //tipForm.Tiplabel.Text = "会员办理失败！请核实网络连接是否正常，会员信息的卡号、姓名、电话不可为空！";
                        //tipForm.ShowDialog();
                        this.Close();
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
            {
                int passtemp = 0;
                if (int.TryParse(textBox1.Text.Trim(), out passtemp))
                {
                    vip.password = passtemp;
                }
                else
                {
                    MessageBox.Show("请输入0~9数字组合会员密码");
                }
            }

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

            vip.birthday = dateTimePicker1.Value;

            if (!string.IsNullOrEmpty(textBox6.Text))
                vip.address = textBox6.Text.Trim();

            if (!string.IsNullOrEmpty(textBox11.Text))
                vip.id_No = textBox11.Text.Trim();

            if (!string.IsNullOrEmpty(textBox7.Text))
                vip.BDJE = Convert.ToDecimal(textBox7.Text.Trim());

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
                //vip.end_Date = dateTimePicker2.MaxDate;  //永远不过期
                //vip.end_Date = Convert.ToDateTime("2100-01-01 01:01:01");  //永远不过期
                //vip.end_Date = DBNull.Value;
            }
            else
            {
                vip.end_Date = dateTimePicker2.Value;
            }

            vip.cTime = System.DateTime.Now;

            return vip;
        }


        //关闭窗口时判断表上有没有内容，防止误操作,如果为true则说明有内容
        bool ValiFun()
        {
            bool isHave = false;
            if (textBox10.Text.Trim() != string.Empty ||
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
            try
            {
                if (HandoverModel.GetInstance.isLianxi)
                {
                    MessageBox.Show("练习模式下该操作无效！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

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

                    case "作废":
                        tempStatus = -2;
                        break;
                }
                //MessageBox.Show(tempVIP.birthday.ToString());
                if (string.IsNullOrEmpty(textBox2.Text) || string.IsNullOrEmpty(textBox3.Text) || string.IsNullOrEmpty(textBox10.Text.Trim()))
                {
                    MessageBox.Show("请检查会员卡号、会员姓名与会员电话信息，此三项为必填项！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                using (hjnbhEntities db = new hjnbhEntities())
                {
                    var card_temp = db.hd_vip_info.AsNoTracking().Where(t => t.vipcard == tempVIP.vipCard).FirstOrDefault();
                    if (card_temp != null)
                    {
                        MessageBox.Show("此会员卡号已经被使用，将重新查找可用卡号！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning); 
                        FindCardFunc();
                        return false;
                    }

                    var vipInfo = new hd_vip_info
                    {
                        vipcard = tempVIP.vipCard,
                        password = tempVIP.password,
                        vipname = tempVIP.vipName,
                        tel = tempVIP.Tel,
                        other2 = tempVIP.other1,
                        Email = tempVIP.email,
                        id_no = tempVIP.id_No,
                        czk_ye = tempVIP.BDJE,
                        //bdje = tempVIP.BDJE,
                        viptype = (byte)tempType,
                        cstatus = tempStatus,
                        validate = tempVIP.valiDate,  //有效期
                        end_date = tempVIP.end_Date,
                        address = tempVIP.address,
                        ctime = tempVIP.cTime,   //创建日期
                        Birthday = tempVIP.birthday,
                        ljxfje = 0, //累计消费
                        zkh = 0,  //折扣
                        cid = HandoverModel.GetInstance.userID,
                        utime = System.DateTime.Now,  //修改日期
                        uid = HandoverModel.GetInstance.userID,
                        yje = 0,  //不知是什么
                        jfnum = 0   //累计积分，这个数据在会员查询列表中将使用，如果不赋值可能会报空指针异常
                    };

                    db.hd_vip_info.Add(vipInfo);
                    var re = db.SaveChanges();

                    decimal jetemp = tempVIP.BDJE.HasValue ? tempVIP.BDJE.Value : 0;
                    if (jetemp > 0)
                    {
                        var vipczk = new hd_vip_cz
                        {
                            ckh = vipInfo.vipcode.ToString(), //会员编号
                            rq = System.DateTime.Now, //时间
                            fs = (byte)1, //类型
                            srvoucher = "开卡", //单号
                            je = jetemp,
                            czr = HandoverModel.GetInstance.userID,
                            //jf = vipczkxfje / 10,
                            lsh = HandoverModel.GetInstance.scode
                        };
                        db.hd_vip_cz.Add(vipczk);
                        var temp = db.SaveChanges();
                        if (temp > 0)
                        {
                            VipJFPrinter pr = new VipJFPrinter(0, jetemp, 0, jetemp , vipInfo.vipcard, vipInfo.vipname, "会员冲减余额凭证");
                            pr.StartPrint();
                        }

                    }


                    if (changed != null)
                    {
                        changed(vipInfo.vipcard);
                    }
                    this.vip = vipInfo.vipcard;

                    return re > 0;

                }
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("会员发行窗口上传会员信息时出现异常:", e);
                MessageBox.Show("开通会员失败！是否正确填写会员资料？必要时候请联系管理员！", "错误信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //string tip = ConnectionHelper.ToDo();
                //if (!string.IsNullOrEmpty(tip))
                //{
                //    MessageBox.Show(tip);
                //}
                return false;
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

        private void button1_Click(object sender, EventArgs e)
        {
            if (SaveVIP())
            {
                tipForm.code = 2;
                tipForm.Tiplabel.Text = "会员办理成功！会员卡号为：" + vip;
                tipForm.ESClabel.Text = "按ESC键清空会员信息并退出，按回车键继续编辑……";
                tipForm.ShowDialog();

            }
            else
            {
                //tipForm.Tiplabel.Text = "会员办理失败！请核实网络连接是否正常，会员信息的卡号、姓名、电话不可为空！";
                //tipForm.ShowDialog();
                this.Close();

            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsNumber(e.KeyChar)) && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }





    }
}
