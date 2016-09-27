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
        bool isappend = false;  //控制旧消息添加次数，不要重复添加

        //public delegate void VipMemoFormHandle();
        //public event VipMemoFormHandle changed;  //传递会员备注事件

        //会员备注信息
        StringBuilder Str0 = new StringBuilder();
        StringBuilder Str1 = new StringBuilder();
        StringBuilder Str2 = new StringBuilder();
        StringBuilder Str3 = new StringBuilder();
        StringBuilder Str4 = new StringBuilder();
        StringBuilder Str5 = new StringBuilder();

        //当前选中的选项卡
        int tid = 0;


        public VipMemoForm()
        {
            InitializeComponent();
        }

        private void VipMemoForm_Load(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 0;
            ReaderVipInfoFunc();
            loadmemobuff();
            this.ActiveControl = textBox1;
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
                    VipInfoWriteFunc(tid);

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
                    var vipMemoInfo = db.hd_vip_memo.AsNoTracking().Where(t => t.vipcode == vipid).Select(t => new { t.memo, t.type }).ToList();

                    if (vipMemoInfo.Count > 0)
                    {

                        foreach (var item in vipMemoInfo)
                        {
                            savememobuff(item.type.Value, item.memo);

                        }

                    }

                }
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("会员消息备注窗口读取会员消息时出现异常:", e);
                MessageBox.Show("会员消息备注窗口读取会员消息时出现异常！请联系管理员");
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

        /// <summary>
        /// 写入会员消息
        /// </summary>
        /// <param name="memotype">写入的类型</param>
        private void VipInfoWriteFunc(int memotype = 0)
        {
            try
            {

                if (HandoverModel.GetInstance.VipID <= 0) return;

                string infos = string.Empty;
                if (!string.IsNullOrEmpty(textBox1.Text))
                {

                    using (var db = new hjnbhEntities())
                    {
                        StringBuilder StrVipMemo = new StringBuilder();   //输入会员消息
                        infos = textBox1.Text.Trim();

                        var Vipinfo = db.hd_vip_memo.Where(t => t.vipcode == HandoverModel.GetInstance.VipID && t.type == memotype).FirstOrDefault();
                        //var Vipinfo = db.hd_vip_info.Where(t => t.vipcode == HandoverModel.GetInstance.VipID).FirstOrDefault();
                        if (Vipinfo != null)
                        {

                            if (isdate)
                            {
                                //richTextBox1.AppendText("  " + infos);
                                StrVipMemo.Append("  " + infos);

                                string temp = StrVipMemo.ToString();
                                Vipinfo.memo += temp;

                                if (db.SaveChanges() == 0)
                                {
                                    MessageBox.Show("会员消息提交失败，请先核实该会员资料，必要时请联系管理员！");
                                }
                                else
                                {
                                    savememobuff(memotype, temp);
                                    loadmemobuff(memotype);

                                }
                            }
                            else
                            {
                                //richTextBox1.AppendText("\r\n" + System.DateTime.Now.Date.ToString("yyyy-MM-dd") + "  " + infos);
                                StrVipMemo.Append(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ： " + infos + ";");
                                isdate = true;

                                string temp = StrVipMemo.ToString();
                                Vipinfo.memo += temp;
                                Vipinfo.cid = HandoverModel.GetInstance.userID;
                                Vipinfo.scode = HandoverModel.GetInstance.scode;
                                Vipinfo.ctime = System.DateTime.Now;
                                if (db.SaveChanges() == 0)
                                {
                                    MessageBox.Show("会员消息提交失败，请先核实该会员资料，必要时请联系管理员！");
                                }
                                else
                                {
                                    savememobuff(memotype, temp);
                                    loadmemobuff(memotype);

                                }
                            }

                        }
                        else
                        {

                            //richTextBox1.AppendText("\r\n" + System.DateTime.Now.Date.ToString("yyyy-MM-dd") + "  " + infos);
                            StrVipMemo.Append(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ： " + infos + ";");
                            isdate = true;

                            string temp = StrVipMemo.ToString();

                            //没有就新建
                            var newinfo = new hd_vip_memo
                            {
                                vipcard = HandoverModel.GetInstance.VipCard,
                                vipcode = HandoverModel.GetInstance.VipID,
                                vipname = HandoverModel.GetInstance.VipName,
                                scode = HandoverModel.GetInstance.scode,
                                cid = HandoverModel.GetInstance.userID,
                                memo = temp,
                                type = memotype,
                                ctime = System.DateTime.Now
                            };

                            db.hd_vip_memo.Add(newinfo);
                            if (db.SaveChanges() == 0)
                            {
                                MessageBox.Show("会员消息提交失败，请先核实该会员资料，必要时请联系管理员！");
                            }
                            else
                            {
                                savememobuff(memotype, temp);
                                loadmemobuff(memotype);
                            }
                        }

                        textBox1.Clear();
                        textBox1.Focus();
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("会员消息备注窗口写入会员消息时出现异常:", ex);
                MessageBox.Show("会员消息备注窗口写入会员消息时出现异常！请联系管理员");
            }
        }



        /// <summary>
        /// 在缓存上保存会员备注
        /// </summary>
        /// <param name="memotype">类型</param>
        /// <param name="temp">消息</param>
        private void savememobuff(int memotype, string temp)
        {
            //把消息存入缓存
            switch (memotype)
            {
                //自定义
                case 0:
                    string allmemo = "";
                    if (isappend == false)
                    {
                        //现在又要为了对接旧系统的备注数据，所以把旧备注放在这里
                        allmemo = temp + "\r\n" + "会员历史备注信息：" + sVipMemoFunc();
                         isappend = true;
                    }
                    else
                    {
                        allmemo = temp;
                    }

                    Str0.Append(TextByDateFunc(allmemo));
                    break;
                //活动
                case 1:
                    Str1.Append(TextByDateFunc(temp));
                    break;
                //存取货
                case 2:
                    Str2.Append(TextByDateFunc(temp));
                    break;
                //积分
                case 3:
                    Str3.Append(TextByDateFunc(temp));
                    break;
                //储卡
                case 4:
                    Str4.Append(TextByDateFunc(temp));
                    break;
                //定金
                case 5:
                    Str5.Append(TextByDateFunc(temp));
                    break;
            }


        }


        /// <summary>
        /// 读取旧系统会员备注
        /// </summary>
        private string sVipMemoFunc()
        {
            string svipmemo = "";
            using (var db = new hjnbhEntities())
            {
                int VipID = HandoverModel.GetInstance.VipID;
                var vipsvipmemoinfo = db.hd_vip_info.AsNoTracking().Where(t => t.vipcode == VipID).Select(t => t.sVipMemo).FirstOrDefault();
                if (!string.IsNullOrEmpty(vipsvipmemoinfo))
                {
                    svipmemo = vipsvipmemoinfo;
                }
            }

            return svipmemo;
        }


        /// <summary>
        /// 根据类型读取会员备注
        /// </summary>
        /// <param name="memotype"></param>
        private void loadmemobuff(int memotype = 0)
        {
            switch (memotype)
            {
                //自定义
                case 0:
                    richTextBox1.Text = Str0.ToString();
                    break;
                //活动
                case 1:
                    richTextBox1.Text = Str1.ToString();
                    break;
                //存取货
                case 2:
                    richTextBox1.Text = Str2.ToString();
                    break;
                //积分
                case 3:
                    richTextBox1.Text = Str3.ToString();
                    break;
                //储卡
                case 4:
                    richTextBox1.Text = Str4.ToString();
                    break;
                //定金
                case 5:
                    richTextBox1.Text = Str5.ToString();
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            VipInfoWriteFunc(tid);
        }

        private void VipMemoForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            richTextBox1.Text = "";
            Str0.Clear();
            Str1.Clear();
            Str2.Clear();
            Str3.Clear();
            Str4.Clear();
            Str5.Clear();
            //changed();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            tid = tabControl1.SelectedIndex;
            loadmemobuff(tid);
            //MessageBox.Show(tid.ToString()+"|"+tabControl1.SelectedTab.ToString());
        }


    }
}
