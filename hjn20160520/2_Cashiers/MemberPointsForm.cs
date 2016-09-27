using Common;
using hjn20160520._2_Cashiers;
using hjn20160520._9_VIPCard;
using hjn20160520.Common;
using hjn20160520.Models;
using Microsoft.VisualBasic;
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
    /// <summary>
    /// 会员积分冲减窗口
    /// </summary>
    public partial class MemberPointsForm : Form
    {
        VipCZYEForm czyeform1 = new VipCZYEForm();  //储值窗口

        //收银窗口
        CashiersFormXP cashForm;
        //信息提示窗口
        TipForm tipForm;
        VIPCardForm VIPForm = new VIPCardForm();  //  会员办理窗口
        //积分储值、扣减
        public string CZJF { get; set; }
        public string KJJF { get; set; }
        //新开会员传递卡号便以自动查询
        private string cardvip = string.Empty;
        //密码输入框
        InputBoxForm InputBoxform = new InputBoxForm();
        bool isVipPWpass = false; //会员密码是否通过验证
        updataPasswordForm passwordForm = new updataPasswordForm();  //会员密码修改窗口

        //列表数据源
        public BindingList<VIPmodel> vipList = new BindingList<VIPmodel>();

        public VipShopForm vipshopform = new VipShopForm(); //会员录入窗口

        public delegate void MemberPointsFormHandle();
        public event MemberPointsFormHandle changed;  //转会员录入 传递事件


        public MemberPointsForm()
        {
            InitializeComponent();
        }

        private void MemberPointsForm_Load(object sender, EventArgs e)
        {
            cashForm = this.Owner as CashiersFormXP;
            tipForm = new TipForm();

            vipList.Clear();
            textBox1.Text = "";
            this.dataGridView1.DataSource = vipList;
            this.textBox1.Focus();
            //默认选中输入框的内容
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                textBox1.SelectAll();
            }

            if (!string.IsNullOrEmpty(HandoverModel.GetInstance.VipCard))
            {
                this.textBox1.Text = HandoverModel.GetInstance.VipCard;

                VIPinfoById(HandoverModel.GetInstance.VipCard);
                ShowVipInfo();
            }

            isVipPWpass = false; //会员密码是否通过验证

            InputBoxform.changed += InputBoxform_changed;
            passwordForm.changed += passwordForm_changed;
            czyeform1.changed += czyeform1_changed;
            VIPForm.changed += VIPForm_changed;

        }

        /// <summary>
        /// 更改密码事件
        /// </summary>
        /// <param name="oldPW">旧密码</param>
        /// <param name="updataPW">新密码</param>
        void passwordForm_changed(string oldPW, string updataPW)
        {

            if (vipPasswordValid(oldPW))
            {
                UpdateVIPPW(updataPW);
            }
        }


        /// <summary>
        /// 密码输入框传递
        /// </summary>
        /// <param name="PW"></param>
        void InputBoxform_changed(string PW)
        {

            this.isVipPWpass = vipPasswordValid(PW);

        }

        void czyeform1_changed(decimal CZYE, decimal KJYE, decimal FQJE, decimal FQSU, decimal YFDJ)
        {

            VipYEFunc(CZYE, KJYE, FQJE, FQSU, YFDJ);
        }



        //快捷键
        private void MemberPointsForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                //回车
                case Keys.Enter:

                    OnEnterClick();
                    break;
                //退出
                case Keys.Escape:
                    //cashForm.Show();
                    this.Close();
                    break;
                //按F8显示会员详细信息   
                case Keys.F8:
                    ShowVipInfo();
                    break;

                //向上键表格换行
                case Keys.Up:

                    UpFun();

                    break;

                //向下键表格换行
                case Keys.Down:

                    DownFun();

                    break;
                //积分冲减
                case Keys.F3:
                    F3Func();
                    break;
                //会员充值
                case Keys.F4:
                    F4Func();

                    break;
                //还款
                case Keys.F5:

                    break;
                //修改密码
                case Keys.F6:
                    updataVipPWFunc();
                    break;
                //发行会员
                case Keys.F7:
                    F7Func();
                    break;
                //登录会员
                case Keys.F12:
                    VipShopFunc();
                    break;

            }
        }

        //新开会员后自动传递卡号并查询一次
        void VIPForm_changed(string s)
        {
            this.cardvip = s;
            this.textBox1.Text = s;
            this.textBox1.SelectAll();
            VIPinfoById(s);
            ShowVipInfo();
        }

        #region 热键相应的功能方法



        //按下回车键
        private void OnEnterClick()
        {
            string temp_txt = this.textBox1.Text.Trim();
            if (string.IsNullOrEmpty(temp_txt))
            {
                tipForm.Tiplabel.Text = "请输入会员卡号、姓名、电话或者大概地址！";
                tipForm.ShowDialog();
            }
            else
            {
                VIPinfoById(temp_txt);
                //this.cardvip = temp_txt;
                //NotShow();
                if (vipList.Count == 1)
                {
                    ShowVipInfo();
                }
            }

        }


        //转会员录入 
        private void VipShopFunc()
        {
            try
            {
                if (vipList.Count <= 0)
                {
                    tipForm.Tiplabel.Text = "没有选中会员，请先在列表中选择！";
                    tipForm.ShowDialog();
                }
                else
                {

                    int id_temp = dataGridView1.SelectedRows[0].Index;
                    string card_temp = vipList[id_temp].vipCard;
                    using (var db = new hjnbhEntities())
                    {

                        var vipInfos = db.hd_vip_info.AsNoTracking().Where(t => t.vipcard == card_temp)
                            .Select(t => new { t.vipname, t.vipcard, t.end_date, t.cstatus, t.vipcode, t.viptype, t.Birthday }).FirstOrDefault();

                        if (vipInfos != null)
                        {
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

                                int viplvInt = vipInfos.viptype.HasValue ? (int)vipInfos.viptype.Value : 0;
                                //bool isvipBirthday = false;
                                if (vipInfos.Birthday.HasValue)
                                {
                                    if (vipInfos.Birthday.Value.Date.Month == System.DateTime.Today.Month && vipInfos.Birthday.Value.Date.Day == System.DateTime.Today.Day)
                                    {
                                        HandoverModel.GetInstance.isVipBirthday = true;
                                    }
                                    else
                                    {
                                        HandoverModel.GetInstance.isVipBirthday = false;

                                    }
                                }
                                else
                                {
                                    HandoverModel.GetInstance.isVipBirthday = false;

                                }
                                HandoverModel.GetInstance.VipID = vipInfos.vipcode;
                                HandoverModel.GetInstance.VipName = vipInfos.vipname;
                                HandoverModel.GetInstance.VipLv = viplvInt;
                                HandoverModel.GetInstance.VipCard = vipInfos.vipcard;

                                changed();  //通知收银界面会员登陆

                            }
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("会员管理转会员消费时发生异常：" + ex);
                MessageBox.Show("会员录入出现异常！请联系管理员");
            }
        }


        //会员ID查信息
        private void VIPinfoById(string temptxt)
        {
            vipList.Clear(); //清空一下重新显示查询值
            using (hjnbhEntities db = new hjnbhEntities())
            {
                //这里只查询卡号、姓名、地址、电话
                var rules = db.hd_vip_info.AsNoTracking().Where(t => (t.vipcard == temptxt) || (t.vipname.Contains(temptxt) || (t.address.Contains(temptxt)) || (t.tel.Contains(temptxt))))
                                          .Select(t => new { t.vipcard, t.vipname, t.tel, t.viptype, t.jfnum, t.cstatus, t.vipcode }).ToList();

                //如果没查询到
                if (rules.Count <= 0)
                {
                    tipForm.Tiplabel.Text = "没有找到此会员，请核对您的信息";
                    tipForm.ShowDialog();
                }
                else
                {
                    //数据库中的数据有些是可以空类型，读取时要核实要显示的列表数据不能为空，最好在添加时判断
                    foreach (var item in rules)
                    {
                        try
                        {
                            if (item.jfnum == null)  //这只是一种情况
                            {
                                vipList.Add(new VIPmodel
                                {
                                    vipCode = item.vipcode,
                                    vipCard = item.vipcard,
                                    vipName = item.vipname,
                                    Tel = item.tel,
                                    vipType = (VipType)item.viptype,
                                    vipStatus = (VipStatus)item.cstatus,
                                    JFnum = 0
                                });
                            }
                            else if (item.vipcard == null)
                            {
                                vipList.Add(new VIPmodel
                                {
                                    vipCode = item.vipcode,
                                    vipCard = "卡号为空",
                                    vipName = item.vipname,
                                    Tel = item.tel,
                                    vipType = (VipType)item.viptype,
                                    vipStatus = (VipStatus)item.cstatus,
                                    JFnum = (float)item.jfnum
                                });
                            }
                            else if (item.vipname == null)
                            {
                                vipList.Add(new VIPmodel
                                {
                                    vipCode = item.vipcode,
                                    vipCard = item.vipcard,
                                    vipName = "没有记录",
                                    Tel = item.tel,
                                    vipType = (VipType)item.viptype,
                                    vipStatus = (VipStatus)item.cstatus,
                                    JFnum = (float)item.jfnum
                                });
                            }
                            else if (item.tel == null)
                            {
                                vipList.Add(new VIPmodel
                                {
                                    vipCode = item.vipcode,
                                    vipCard = item.vipcard,
                                    vipName = item.vipname,
                                    Tel = "没有记录",
                                    vipType = (VipType)item.viptype,
                                    vipStatus = (VipStatus)item.cstatus,
                                    JFnum = (float)item.jfnum
                                });
                            }
                            else
                            {
                                vipList.Add(new VIPmodel
                                {
                                    vipCode = item.vipcode,
                                    vipCard = item.vipcard,
                                    vipName = item.vipname,
                                    Tel = item.tel,
                                    vipType = (VipType)item.viptype,
                                    vipStatus = (VipStatus)item.cstatus,
                                    JFnum = (float)item.jfnum
                                });
                            }

                        }
                        catch (System.InvalidOperationException)
                        {
                            tipForm.Tiplabel.Text = "此会员信息出错，请重新核实数据！";
                            tipForm.ShowDialog();
                        }

                    }
                }


            }

        }


        //处理F6显示会员信息
        private void ShowVipInfo()
        {
            if (vipList.Count <= 0)
            {
                tipForm.Tiplabel.Text = "没有选中会员，请先在列表中选择！";
                tipForm.ShowDialog();
            }
            else
            {
                try
                {
                    int id_temp = dataGridView1.SelectedRows[0].Index;
                    string card_temp = vipList[id_temp].vipCard;
                    int vipid = vipList[id_temp].vipCode;
                    using (hjnbhEntities db = new hjnbhEntities())
                    {
                        decimal Fqje = 0; //分期总金额
                        var fqinfo = db.hd_vip_fq.AsNoTracking().Where(t => t.vipcode == vipid && t.amount > 0).ToList();
                        if (fqinfo.Count > 0)
                        {
                            foreach (var item in fqinfo)
                            {
                                decimal temp = item.mqje.HasValue ? item.mqje.Value : 0;
                                temp *= item.amount.Value;
                                Fqje += temp;
                            }
                        }

                        var reInfo = db.hd_vip_info.AsNoTracking().Where(t => t.vipcode == vipid)
                            .Select(t => new
                            {
                                t.vipcode,
                                t.vipcard,
                                t.vipname,
                                t.tel,
                                t.id_no,
                                t.Email,
                                t.Birthday,
                                t.czk_ye,
                                t.ctime,
                                t.cstatus,
                                t.viptype,
                                t.end_date,
                                t.ljxfje,
                                t.dcMaxQk,
                                t.jfnum,
                                t.bdje,
                                t.address,
                                t.ydje,
                                t.other4
                            }).ToList();

                        foreach (var item in reInfo)
                        {
                            this.label29.Text = item.vipcode.ToString();
                            this.label28.Text = item.vipcard;
                            this.label27.Text = item.vipname;
                            this.label26.Text = item.tel;
                            this.label25.Text = item.id_no;
                            this.label24.Text = item.Email;
                            this.label23.Text = item.Birthday.ToString();
                            this.label22.Text = item.czk_ye.HasValue ? item.czk_ye.ToString() + " 元" : "";
                            this.label45.Text = item.ctime.ToString();
                            this.label1.Text = item.ydje.HasValue ? item.ydje.Value.ToString() + " 元" : "";
                            this.label6.Text = Fqje.ToString("0.00") + " 元";
                            this.label8.Text = item.other4 + " 元";

                            switch (item.cstatus)
                            {
                                case 0:
                                    this.label44.Text = "正常";
                                    break;
                                case 1:
                                    this.label44.Text = "过期";
                                    break;
                                case 2:
                                    this.label44.Text = "作废";
                                    break;

                            }

                            switch (item.viptype)
                            {
                                case 1:
                                    this.label43.Text = "普通会员";
                                    break;
                                case 2:
                                    this.label43.Text = "黄金会员";
                                    break;
                                case 3:
                                    this.label43.Text = "钻石会员";
                                    break;

                            }



                            //到期时间
                            if (item.end_date.HasValue)
                            {
                                this.label42.Text = item.end_date.Value.ToString();
                            }
                            else
                            {
                                this.label42.Text = "永不过期";
                            }

                            this.label41.Text = (item.ljxfje.HasValue ? item.ljxfje.Value : 0).ToString() + " 元";
                            this.label40.Text = item.dcMaxQk.ToString();
                            this.label39.Text = item.jfnum.ToString();
                            this.label38.Text = (item.bdje.HasValue ? item.bdje.Value : 0).ToString() + " 元";
                            this.label48.Text = item.address;

                        }
                        this.panel5.Visible = this.panel6.Visible = this.label48.Visible = true;

                    }

                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("处理F6显示会员信息时发生异常：" + ex);
                    MessageBox.Show("数据库连接出错！");
                    //string tip = ConnectionHelper.ToDo();
                    //if (!string.IsNullOrEmpty(tip))
                    //{
                    //    MessageBox.Show(tip);
                    //}
                }
            }

        }

        //小键盘向上
        private void UpFun()
        {
            try
            {


                //当前行数大于1行才生效
                if (dataGridView1.Rows.Count > 1)
                {
                    int rowindex_temp = dataGridView1.SelectedRows[0].Index;
                    if (rowindex_temp == 0)
                    {
                        dataGridView1.Rows[dataGridView1.Rows.Count - 1].Selected = true;
                        dataGridView1.Rows[rowindex_temp].Selected = false;
                        dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows.Count - 1; //定位滚动条到当前选择行

                    }
                    else
                    {
                        dataGridView1.Rows[rowindex_temp - 1].Selected = true;
                        dataGridView1.Rows[rowindex_temp].Selected = false;
                        dataGridView1.FirstDisplayedScrollingRowIndex = rowindex_temp - 1; //定位滚动条到当前选择行

                    }

                }
            }
            catch (Exception ex)
            {

                LogHelper.WriteLog("会员冲减窗口小键盘向上时发生异常：" + ex);

            }
        }

        //小键盘向下
        private void DownFun()
        {
            try
            {

                //当前行数大于1行才生效
                if (dataGridView1.Rows.Count > 1)
                {
                    int rowindexDown_temp = dataGridView1.SelectedRows[0].Index;
                    if (rowindexDown_temp == dataGridView1.Rows.Count - 1)
                    {
                        dataGridView1.Rows[0].Selected = true;
                        dataGridView1.Rows[rowindexDown_temp].Selected = false;
                        dataGridView1.FirstDisplayedScrollingRowIndex = 0; //定位滚动条到当前选择行

                    }
                    else
                    {
                        dataGridView1.Rows[rowindexDown_temp + 1].Selected = true;
                        dataGridView1.Rows[rowindexDown_temp].Selected = false;
                        dataGridView1.FirstDisplayedScrollingRowIndex = rowindexDown_temp + 1; //定位滚动条到当前选择行

                    }

                }
            }
            catch (Exception ex)
            {

                LogHelper.WriteLog("会员冲减窗口小键盘向下时发生异常：" + ex);

            }
        }


        //F3当前会员冲减积分
        public void VipJFFunc()
        {
            try
            {
                string vipcard_temp = "";
                string vipname_temp = "";
                int vipid = 0;

                if (vipList.Count == 0)
                {
                    return;
                }
                else
                {
                    int index_temp = dataGridView1.SelectedRows[0].Index;
                    vipcard_temp = vipList[index_temp].vipCard;
                    vipname_temp = vipList[index_temp].vipName;
                    vipid = vipList[index_temp].vipCode;
                }
                if (string.IsNullOrEmpty(CZJF) && string.IsNullOrEmpty(KJJF)) return;

                using (var db = new hjnbhEntities())
                {
                    var JFinfo = db.hd_vip_info.Where(t => t.vipcode == vipid).FirstOrDefault();

                    decimal CZJFtoD = 0;
                    decimal KJJFtoD = 0;
                    decimal JFtemp = 0;  //最终积分
                    decimal Czjf = 0;  //本次冲减积分
                    bool isJF = false;

                    if (!string.IsNullOrEmpty(CZJF))
                    {
                        CZJFtoD = Convert.ToDecimal(CZJF);
                        decimal temp = JFinfo.jfnum.HasValue ? JFinfo.jfnum.Value : 0;
                        temp += CZJFtoD;
                        JFinfo.jfnum = temp;
                        Czjf = CZJFtoD;
                        JFtemp = temp;
                        isJF = true;
                    }
                    if (!string.IsNullOrEmpty(KJJF))
                    {
                        KJJFtoD = Convert.ToDecimal(KJJF);
                        decimal temp = JFinfo.jfnum.HasValue ? JFinfo.jfnum.Value : 0;
                        temp -= KJJFtoD;
                        JFinfo.jfnum = temp;
                        Czjf = -KJJFtoD;
                        JFtemp = temp;
                        isJF = true;
                    }

                    if (isJF)
                    {
                        decimal JF = (string.IsNullOrEmpty(CZJF)) ? -KJJFtoD : CZJFtoD;
                        var jf_info = new hd_vip_cz
                        {
                            ckh = vipid.ToString(),
                            rq = System.DateTime.Now,
                            je = JF,
                            fs = (byte)6,
                            ctype = (byte)0,
                            czr = HandoverModel.GetInstance.userID,
                            lsh = HandoverModel.GetInstance.scode
                        };
                        db.hd_vip_cz.Add(jf_info);
                        string temptip = (!string.IsNullOrEmpty(CZJF)) ? "+" + JF.ToString() : JF.ToString();
                        string temp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ： " + " 会员积分充减 " + temptip + ";";
                        VipAutoMemoFunc(db, vipid, vipcard_temp, vipname_temp, temp, 3);
                    }


                    var re = db.SaveChanges();
                    if (re > 0)
                    {
                        VipJFPrinter pr = new VipJFPrinter(JFtemp, 0, Czjf, 0, JFinfo.vipcard, JFinfo.vipname, "会员冲减积分凭证");
                        pr.StartPrint();

                        CZJF = KJJF = string.Empty;
                        label39.Text = JFtemp.ToString();
                        MessageBox.Show("积分冲减成功！");

                    }
                    else
                    {
                        MessageBox.Show("积分冲减失败！");

                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("会员积分冲减窗口冲减积分时出现异常:", e);
                MessageBox.Show("冲减积分时出现异常！");
                //string tip = ConnectionHelper.ToDo();
                //if (!string.IsNullOrEmpty(tip))
                //{
                //    MessageBox.Show(tip);
                //}
            }
        }



        /// <summary>
        /// F4会员余额冲减
        /// </summary>
        /// <param name="CZYE">充值</param>
        /// <param name="KJYE">扣减</param>
        /// <param name="FQJE">分期金额</param>
        /// <param name="FQSU">分期数</param>
        /// <param name="YFDJ">定金</param>
        public void VipYEFunc(decimal CZYE, decimal KJYE, decimal FQJE, decimal FQSU, decimal YFDJ)
        {
            try
            {
                string vipcard_temp = "";
                string vipname_temp = "";
                int vipid = 0;
                if (vipList.Count == 0)
                {
                    return;
                }
                else
                {
                    int index_temp = dataGridView1.SelectedRows[0].Index;
                    vipcard_temp = vipList[index_temp].vipCard;
                    vipname_temp = vipList[index_temp].vipName;
                    vipid = vipList[index_temp].vipCode;
                }
                if (CZYE == 0 && KJYE == 0 && FQJE == 0 && YFDJ == 0) return;
                using (var db = new hjnbhEntities())
                {
                    var VIPinfo = db.hd_vip_info.Where(t => t.vipcode == vipid).FirstOrDefault();

                    decimal CZYEtoD = 0;
                    decimal KJYEtoD = 0;
                    decimal FQJEtoD = 0;
                    decimal FQSUtoD = 0;
                    decimal YFDJtoD = 0;

                    decimal YEtemp = 0;  //总余额
                    decimal cztemp = 0;  //本次充值
                    bool isYE = false;
                    if (CZYE > 0)
                    {
                        CZYEtoD = Math.Round(CZYE, 2); //转换

                        decimal temp = VIPinfo.czk_ye.HasValue ? VIPinfo.czk_ye.Value : 0;
                        temp += CZYEtoD;
                        cztemp = CZYEtoD;
                        VIPinfo.czk_ye = temp;
                        YEtemp = temp;
                        isYE = true;
                    }
                    if (KJYE > 0)
                    {
                        KJYEtoD = Math.Round(KJYE, 2);

                        decimal temp = VIPinfo.czk_ye.HasValue ? VIPinfo.czk_ye.Value : 0;
                        temp -= KJYEtoD;
                        cztemp = -KJYEtoD;
                        VIPinfo.czk_ye = temp;
                        YEtemp = temp;
                        isYE = true;
                    }

                    if (isYE)
                    {
                        decimal Ye = CZYE > 0 ? CZYEtoD : -KJYEtoD;

                        var CJinfo = new hd_vip_cz
                        {
                            ckh = vipid.ToString(),
                            rq = System.DateTime.Now,
                            je = Ye,
                            fs = (byte)2,
                            ctype = (byte)0,
                            czr = HandoverModel.GetInstance.userID,
                            lsh = HandoverModel.GetInstance.scode
                        };
                        db.hd_vip_cz.Add(CJinfo);
                        string temptip = CZYE > 0 ? "+" + Ye.ToString() : Ye.ToString();
                        string temp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ： " + " 会员储卡充减 " + temptip + ";";
                        VipAutoMemoFunc(db, vipid, vipcard_temp, vipname_temp, temp, 4);
                    }


                    //处理分期
                    if (FQJE > 0 && FQSU > 0)
                    {
                        FQJEtoD = Math.Round(FQJE, 2);  //分期金额
                        FQSUtoD = Math.Round(FQSU, 2);  //分期数

                        var newFQinfo = new hd_vip_fq
                        {
                            vipcard = vipcard_temp,
                            vipcode = vipid,
                            vipname = vipname_temp,
                            fqje = FQJEtoD,
                            fqsu = FQSUtoD,
                            mqje = Math.Round(FQJEtoD / FQSUtoD, 2),
                            amount = FQSUtoD,
                            cid = HandoverModel.GetInstance.userID,
                            scode = HandoverModel.GetInstance.scode,
                            ctime = System.DateTime.Now,
                            //valitime =   //有效时间，暂时不用
                        };

                        db.hd_vip_fq.Add(newFQinfo);

                        var CJFQinfo = new hd_vip_cz
                        {
                            ckh = vipid.ToString(),
                            rq = System.DateTime.Now,
                            je = FQJEtoD,
                            fs = (byte)2,
                            ctype = (byte)2,
                            czr = HandoverModel.GetInstance.userID,
                            lsh = HandoverModel.GetInstance.scode
                        };
                        db.hd_vip_cz.Add(CJFQinfo);

                        string temp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ： " + " 会员分期返回余额 " + Math.Round(FQJEtoD / FQSUtoD, 2).ToString() + "*" + FQSUtoD.ToString() + ";";
                        VipAutoMemoFunc(db, vipid, vipcard_temp, vipname_temp, temp, 4);
                    }


                    //处理定金
                    if (YFDJ > 0)
                    {
                        YFDJtoD = Math.Round(YFDJ, 2);

                        decimal ydtemp = VIPinfo.ydje.HasValue ? VIPinfo.ydje.Value : 0;
                        ydtemp += YFDJtoD;

                        VIPinfo.ydje = ydtemp;

                        var CJDJinfo = new hd_vip_cz
                        {
                            ckh = vipid.ToString(),
                            rq = System.DateTime.Now,
                            je = YFDJtoD,
                            fs = (byte)2,
                            ctype = (byte)1,
                            czr = HandoverModel.GetInstance.userID,
                            lsh = HandoverModel.GetInstance.scode
                        };
                        db.hd_vip_cz.Add(CJDJinfo);

                        string temp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ： " + " 会员定金储值 " + YFDJtoD.ToString() + ";";
                        VipAutoMemoFunc(db, vipid, vipcard_temp, vipname_temp, temp, 5);
                    }



                    var re = db.SaveChanges();
                    if (re > 0)
                    {
                        decimal Fqjetemp = 0; //分期总金额
                        if (FQJEtoD != 0)
                        {
                            var fqinfo = db.hd_vip_fq.AsNoTracking().Where(t => t.vipcode == vipid).ToList();
                            if (fqinfo.Count > 0)
                            {
                                foreach (var item in fqinfo)
                                {
                                    decimal temp = item.fqje.HasValue ? item.fqje.Value : 0;
                                    Fqjetemp += temp;
                                }
                            }
                        }

                        decimal YDJE_temp = VIPinfo.ydje.HasValue ? VIPinfo.ydje.Value : 0.00m;

                        VipJFPrinter pr = new VipJFPrinter(0, YEtemp, 0, cztemp, VIPinfo.vipcard, VIPinfo.vipname, "会员冲减余额凭证", FQJEtoD, YFDJtoD, Fqjetemp, YDJE_temp);
                        pr.StartPrint();

                        MessageBox.Show("余额冲减成功！");

                        //刷新UI
                        if (KJYE > 0 || CZYE > 0)
                        {
                            label22.Text = YEtemp.ToString() + " 元";
                        }

                        if (YFDJ > 0)
                        {
                            label1.Text = VIPinfo.ydje.Value.ToString("0.00") + " 元";
                        }

                        if (FQJE > 0 && FQSU > 0)
                        {
                            label6.Text = Fqjetemp.ToString("0.00") + " 元";
                        }

                    }
                    else
                    {
                        MessageBox.Show("余额冲减失败！请核实资料是否正确，必要时请联系管理员");

                    }

                }

            }
            catch (Exception e)
            {
                LogHelper.WriteLog("会员余额冲减窗口冲减金额时出现异常:", e);
                MessageBox.Show("会员余额冲减窗口冲减金额时出现异常！请联系管理员");

            }
        }


        //会员自动备注
        private void VipAutoMemoFunc(hjnbhEntities db, int vipid, string vipCard, string vipName, string Memo, int vtype)
        {
            var VipMemoinfo4 = db.hd_vip_memo.Where(t => t.vipcode == vipid && t.type == vtype).FirstOrDefault();
            if (VipMemoinfo4 != null)
            {
                VipMemoinfo4.memo += Memo;
            }
            else
            {
                //没有就新建
                var newinfo4 = new hd_vip_memo
                {
                    vipcard = vipCard,
                    vipcode = vipid,
                    vipname = vipName,
                    scode = HandoverModel.GetInstance.scode,
                    cid = HandoverModel.GetInstance.userID,
                    memo = Memo,
                    type = vtype,
                    ctime = System.DateTime.Now
                };

                db.hd_vip_memo.Add(newinfo4);
            }
        }



        //会员修改密码
        private void UpdateVIPPW(string newPW)
        {
            try
            {
                int vipid = 0;
                if (vipList.Count == 0)
                {
                    return;
                }
                else
                {
                    int index_temp = dataGridView1.SelectedRows[0].Index;
                    vipid = vipList[index_temp].vipCode;
                }

                using (var db = new hjnbhEntities())
                {
                    var JFinfo = db.hd_vip_info.Where(t => t.vipcode == vipid).FirstOrDefault();
                    int temp = 0;
                    int.TryParse(newPW, out temp);
                    JFinfo.password = temp;
                    var re = db.SaveChanges();
                    if (re > 0)
                        MessageBox.Show("会员密码修改成功！");
                    else
                        MessageBox.Show("会员密码修改失败！是否正确选择需要修改密码的会员？", "错误信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            catch (Exception e)
            {
                LogHelper.WriteLog("会员修改密码窗口出现异常:", e);
                MessageBox.Show("修改密码时出现异常！请确定会员信息是否正常，必要时请联系管理员！", "错误信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //string tip = ConnectionHelper.ToDo();
                //if (!string.IsNullOrEmpty(tip))
                //{
                //    MessageBox.Show(tip);
                //}
            }
        }


        #endregion

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            UpdateNameFunc();
        }
        //调整表格的列宽、同时隐藏不需要显示的列、禁止编辑、修改列名
        private void UpdateNameFunc()
        {
            try
            {
                //列名
                dataGridView1.Columns[1].HeaderText = "会员卡号";
                dataGridView1.Columns[2].HeaderText = "姓名";
                dataGridView1.Columns[4].HeaderText = "电话";
                dataGridView1.Columns[6].HeaderText = "会员卡类";
                dataGridView1.Columns[7].HeaderText = "积分";
                dataGridView1.Columns[9].HeaderText = "状态";


                //隐藏      
                for (int i = 0; i < this.dataGridView1.ColumnCount; i++)
                {
                    if (this.dataGridView1.Columns[i].Index == 1 ||
                        this.dataGridView1.Columns[i].Index == 2 ||
                        this.dataGridView1.Columns[i].Index == 4 ||
                        this.dataGridView1.Columns[i].Index == 6 ||
                        this.dataGridView1.Columns[i].Index == 7 ||
                        this.dataGridView1.Columns[i].Index == 9)
                    {
                        this.dataGridView1.Columns[i].Visible = true;
                    }
                    else
                    {
                        this.dataGridView1.Columns[i].Visible = false;

                    }
                }
            }
            catch
            {
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OnEnterClick();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            ShowVipInfo();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            F3Func();
        }


        //积分冲减
        private void F3Func()
        {
            if (cashForm.isLianXi)
            {
                MessageBox.Show("不允许练习模式进行该操作！");
                return;
            }

            if (vipList.Count == 0)
            {
                MessageBox.Show("请先登录会员或者输入会员卡号查询会员！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //先验证密码
            InputBoxform.ShowDialog();
            if (this.isVipPWpass)
            {
                var czjfform1 = new VipCZJFForm();

                czjfform1.ShowDialog(this);

                this.isVipPWpass = false;
            }
        }


        private void button4_Click(object sender, EventArgs e)
        {

            F4Func();
        }


        //F4储卡管理
        private void F4Func()
        {
            if (cashForm.isLianXi)
            {
                MessageBox.Show("不允许练习模式进行该操作！");
                return;
            }

            if (vipList.Count == 0)
            {
                MessageBox.Show("请先登录会员或者输入会员卡号查询会员！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //先验证密码
            InputBoxform.ShowDialog();
            if (this.isVipPWpass)
            {
                czyeform1.ShowDialog(this);
                this.isVipPWpass = false;
            }
        }


        /// <summary>
        /// 会员密码验证
        /// </summary>
        /// <returns></returns>
        private bool vipPasswordValid(string vippw)
        {

            bool pwok = false;
            int vipid = 0;
            if (vipList.Count == 0)
            {
               
                return pwok;
            }
            else
            {
                int index_temp = dataGridView1.SelectedRows[0].Index;
                vipid = vipList[index_temp].vipCode;
            }



            using (var db = new hjnbhEntities())
            {
                var VIPpasswordinfo = db.hd_vip_info.Where(t => t.vipcode == vipid).Select(t => t.password).FirstOrDefault();
                ////如果原密码为空，则为其设置一个默认密码为0
                string pw = VIPpasswordinfo.HasValue ? VIPpasswordinfo.Value.ToString() : "0";
                if (!string.IsNullOrEmpty(vippw))
                {
                    if (vippw != pw)
                    {
                        MessageBox.Show("会员密码检验失败！请输入正确的会员密码！可尝试使用默认密码 0 。", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        pwok = true;
                    }
                }

            }

            return pwok;
        }


        //修改密码
        private void button5_Click(object sender, EventArgs e)
        {
            updataVipPWFunc();
        }

        //F6修改密码
        private void updataVipPWFunc()
        {
            if (cashForm.isLianXi)
            {
                MessageBox.Show("不允许练习模式进行该操作！");
                return;
            }

            if (vipList.Count == 0)
            {
                MessageBox.Show("请先登录会员或者输入会员卡号查询会员！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            passwordForm.ShowDialog();
        }

        



        private void button6_Click(object sender, EventArgs e)
        {
            F7Func();
        }

        //F7会员卡发行
        private void F7Func()
        {
            if (cashForm.isLianXi)
            {
                MessageBox.Show("不允许练习模式进行该操作！");
                return;
            }

            VIPForm.ShowDialog();
        }



        private void button7_Click(object sender, EventArgs e)
        {
            VipShopFunc();
        }

        private void MemberPointsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            czyeform1.changed -= czyeform1_changed;
            InputBoxform.changed -= InputBoxform_changed;
            passwordForm.changed -= passwordForm_changed;
            VIPForm.changed -= VIPForm_changed;

        }

        //还款
        private void button8_Click(object sender, EventArgs e)
        {

        }

        //还款窗口打开
        private void vipHuanKuanOpenFunc()
        {
            int vipid = 0;
            if (vipList.Count == 0)
            {
                MessageBox.Show("您没有选择任何会员，请先进行会员查询！", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                int index_temp = dataGridView1.SelectedRows[0].Index;
                vipid = vipList[index_temp].vipCode;
            }



        }


        //还款处理
        private void vipHuanKuanFunc(decimal hqje)
        {
            try
            {
                int vipid = 0;
                if (vipList.Count == 0)
                {
                    MessageBox.Show("您没有选择任何会员，请先进行会员查询！", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else
                {
                    int index_temp = dataGridView1.SelectedRows[0].Index;
                    vipid = vipList[index_temp].vipCode;
                }

                using (var db = new hjnbhEntities())
                {
                    var vipinfo = db.hd_vip_info.Where(t => t.vipcode == vipid).FirstOrDefault();
                    if (vipinfo != null)
                    {
                        decimal temp = Convert.ToDecimal(vipinfo.other4) - hqje;
                        vipinfo.other4 = temp.ToString();
                        if (db.SaveChanges() > 0)
                        {
                            MessageBox.Show("会员还款成功！本次还款：" + hqje.ToString() + "元，目前总欠款金额为：" + temp.ToString() + "元");
                        }
                        else
                        {
                            MessageBox.Show("会员还款失败！请确定会员信息是否正常，必要时请联系管理员！");
                        }
                    }
                }

            }
            catch (Exception e)
            {
                LogHelper.WriteLog("会员还款窗口出现异常:", e);
                MessageBox.Show("会员还款时出现异常！请确定会员信息是否正常，必要时请联系管理员！", "错误信息", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }





    }
}
