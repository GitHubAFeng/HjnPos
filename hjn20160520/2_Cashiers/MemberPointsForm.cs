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

        //收银窗口
        CashiersFormXP cashForm;
        //信息提示窗口
        TipForm tipForm;
        VIPCardForm VIPForm;  //  会员办理窗口
        //余额储值/扣减，积分储值、扣减
        public string CZYE { get; set; }
        public string KJYE { get; set; }
        public string CZJF { get; set; }
        public string KJJF { get; set; }

        private string cardvip = string.Empty; 

        //列表数据源
        public BindingList<VIPmodel> vipList = new BindingList<VIPmodel>();

        public VipShopForm vipshopform = new VipShopForm(); //会员录入窗口

        //public delegate void FormHandle(string s);
        //public event FormHandle changed;
        ////传递给收银的vipID
        //public delegate void VIPHandle(int vipid, string vipcrad , int viplv);
        //public event VIPHandle VIPchanged; 

        public MemberPointsForm()
        {
            InitializeComponent();
        }

        private void MemberPointsForm_Load(object sender, EventArgs e)
        {
            cashForm = this.Owner as CashiersFormXP;
            tipForm = new TipForm();
            VIPForm = new VIPCardForm();

            this.dataGridView1.DataSource = vipList;
            this.textBox1.Focus();
            //默认选中输入框的内容
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                textBox1.SelectAll();
            }

            if (!string.IsNullOrEmpty(HandoverModel.GetInstance.VipCard))
            {
                VIPinfoById(HandoverModel.GetInstance.VipCard);
                ShowVipInfo();
            }


        }

        private void label8_Click(object sender, EventArgs e)
        {

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
                //按F6显示会员详细信息   
                case Keys.F6:
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
                    if (cashForm.isLianXi)
                    {
                        MessageBox.Show("不允许练习模式进行该操作！");
                        return;
                    }
                    if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
                    {
                        var czjfform1 = new VipCZJFForm();

                        czjfform1.ShowDialog(this);
                        //VipJFFunc();

                    }
                    else
                    {
                        MessageBox.Show("请先查询会员");
                    }

                    break;
                //会员充值
                case Keys.F4:
                    if (cashForm.isLianXi)
                    {
                        MessageBox.Show("不允许练习模式进行该操作！");
                        return;
                    }

                    if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
                    {
                        var czyeform1 = new VipCZYEForm();

                        czyeform1.ShowDialog(this);
                        //VipYEFunc();

                    }
                    else
                    {
                        MessageBox.Show("请先查询会员");
                    }
                    break;
                //修改密码
                case Keys.F5:
                    if (cashForm.isLianXi)
                    {
                        MessageBox.Show("不允许练习模式进行该操作！");
                        return;
                    }

                    UpdateVIPPW();
                    break;
                //发行会员
                case Keys.F7:
                    if (cashForm.isLianXi)
                    {
                        MessageBox.Show("不允许练习模式进行该操作！");
                        return;
                    }
                    VIPForm.changed += VIPForm_changed;
                    VIPForm.ShowDialog();
                    break;
                //登录会员
                //case Keys.F12:
                //    if (vipList.Count > 0)
                //    {
                //        int tempindex = dataGridView1.SelectedRows[0].Index;
                //        string tempvip = vipList[tempindex].vipCard;
                //        //VipCardFunc(tempvip);
                //        vipshopform.vipcrad = tempvip;
                //        vipshopform.ShowDialog();
                //    }
                //    else
                //    {
                //        vipshopform.ShowDialog();
                //    }

                //    //if (string.IsNullOrEmpty(cardvip))
                //    //{

                //    //    MessageBox.Show("请先查询会员");
                //    //}
                //    //else
                //    //{
                //    //    VipCardFunc(cardvip);
                //    //}

                //    //vipshopform.ShowDialog();
                //    break;
                //    //会员存货
                //case Keys.F9:
                //    VipSaveItemForm vipsave = new VipSaveItemForm();
                //    vipsave.ShowDialog();
                //    break;
                //    //会员取货
                //case Keys.F10:
                //    VipGetItemForm vipget = new VipGetItemForm();
                //    vipget.ShowDialog();
                //    break;



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

        //public void VipCardFunc(string card = "")
        //{
        //    try
        //    {
        //        string text_temp = string.Empty;
        //        if (card != "")
        //        {
        //            text_temp = card;
        //        }
        //        else
        //        {
        //            if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
        //            {
        //                text_temp = textBox1.Text.Trim();
        //            }

        //        }

        //        if (string.IsNullOrEmpty(text_temp)) return;

        //        using (var db = new hjnbhEntities())
        //        {
        //            //手机号也能查
        //            var vipInfos = db.hd_vip_info.AsNoTracking().Where(t => t.vipcard == text_temp || t.tel == text_temp)
        //                .Select(t => new { t.vipname, t.vipcard, t.end_date, t.cstatus, t.vipcode }).FirstOrDefault();



        //            if (text_temp == "-1")
        //            {
        //                //CashiersFormXP.GetInstance.VipID = 0;
        //                VIPchanged(0,"",0);  //传递会员ID
        //                CashiersFormXP.GetInstance.XSHDFunc(db);
        //                CashiersFormXP.GetInstance.YHHDFunc(db);
        //                changed("未录入");  //传递会员名字
        //                this.Close();
        //                return;
        //            }

        //            if (vipInfos != null)
        //            {
        //                var vipLV = db.hd_vip_info.AsNoTracking().Where(t => t.vipcode == vipInfos.vipcode).Select(t => t.viptype).FirstOrDefault();
        //                if (System.DateTime.Now > vipInfos.end_date)
        //                {
        //                    MessageBox.Show("此会员卡已经过期！不能使用！");
        //                }
        //                else if (vipInfos.cstatus != 0)
        //                {
        //                    MessageBox.Show("此会员卡处于非正常状态！不能使用！");
        //                }
        //                else
        //                {
        //                    if (CashiersFormXP.GetInstance != null)
        //                    {
        //                        int viplvInt = vipLV.HasValue ? (int)vipLV.Value : 0;

        //                        VIPchanged(vipInfos.vipcode, vipInfos.vipcard, viplvInt);
        //                        string temp_name = vipInfos.vipname;
        //                        CashiersFormXP.GetInstance.XSHDFunc(db);
        //                        CashiersFormXP.GetInstance.YHHDFunc(db);

        //                        changed(temp_name);  //事件传值
        //                    }

        //                    //if (ClosingEntries.GetInstance != null) ClosingEntries.GetInstance.VIPShowUI();
        //                }

        //                this.Close();

        //            }
        //            else
        //            {
        //                tipForm = new TipForm();
        //                tipForm.Tiplabel.Text = "查询失败，请核实会员卡号是否正确！";
        //                tipForm.ShowDialog();
        //                textBox1.SelectAll();
        //            }



        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        LogHelper.WriteLog("会员录入窗口登记时出现异常:", e);
        //        MessageBox.Show("数据库连接出错！");
        //        //string tip = ConnectionHelper.ToDo();
        //        //if (!string.IsNullOrEmpty(tip))
        //        //{
        //        //    MessageBox.Show(tip);
        //        //}
        //    }

        //}


        //会员ID查信息
        private void VIPinfoById(string temptxt)
        {
            vipList.Clear(); //清空一下重新显示查询值
            using (hjnbhEntities db = new hjnbhEntities())
            {
                //这里只查询卡号、姓名、地址、电话
                var rules = db.hd_vip_info.AsNoTracking().Where(t => (t.vipcard == temptxt) || (t.vipname.Contains(temptxt) || (t.address.Contains(temptxt)) || (t.tel.Contains(temptxt))))
                                          .Select(t => new { t.vipcard, t.vipname, t.tel, t.viptype, t.jfnum, t.cstatus ,t.vipcode}).ToArray();

                //如果没查询到
                if (rules.Count() <= 0)
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
                    using (hjnbhEntities db = new hjnbhEntities())
                    {
                        var reInfo = db.hd_vip_info.AsNoTracking().Where(t => t.vipcard == card_temp)
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
                                t.address
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
                            this.label22.Text = item.czk_ye.ToString() + " 元";
                            this.label45.Text = item.ctime.ToString();

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
                            //this.label44.Text = item.cstatus.ToString();
                            //this.label43.Text = item.viptype.ToString();
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
                string vipcard_temp = string.Empty;
                string vipid = "";

                if (vipList.Count == 0)
                {
                    MessageBox.Show("请先查询会员！");
                    return;
                }
                else
                {
                    int index_temp = dataGridView1.SelectedRows[0].Index;
                    vipcard_temp = vipList[index_temp].vipCard;
                    vipid = vipList[index_temp].vipCode.ToString();
                }
                if (string.IsNullOrEmpty(CZJF) && string.IsNullOrEmpty(KJJF)) return;

                using (var db = new hjnbhEntities())
                {
                    var JFinfo = db.hd_vip_info.Where(t => t.vipcard == vipcard_temp).FirstOrDefault();
                    decimal JFtemp = 0;  //最终积分
                    decimal Czjf = 0;  //本次冲减积分
                    bool isJF = false;

                    if (!string.IsNullOrEmpty(CZJF))
                    {
                        decimal temp = JFinfo.jfnum.HasValue ? JFinfo.jfnum.Value : 0;
                        temp += Convert.ToDecimal(CZJF);
                        JFinfo.jfnum = temp;
                        Czjf = Convert.ToDecimal(CZJF);
                        JFtemp = temp;
                        isJF = true;
                    }
                    if (!string.IsNullOrEmpty(KJJF))
                    {
                        decimal temp = JFinfo.jfnum.HasValue ? JFinfo.jfnum.Value : 0;
                        temp -= Convert.ToDecimal(KJJF);
                        JFinfo.jfnum = temp;
                        Czjf = -Convert.ToDecimal(KJJF);
                        JFtemp = temp;
                        isJF = true;
                    }

                    if (isJF)
                    {
                     
                        var jf_info = new hd_vip_cz
                        {
                            //ckh = vipcard_temp,
                            ckh = vipid,
                            rq = System.DateTime.Now,
                            //jf = (string.IsNullOrEmpty(CZJF)) ? -Convert.ToDecimal(KJJF) : Convert.ToDecimal(CZJF),
                            //fs = (string.IsNullOrEmpty(CZJF)) ? (byte)6 : (byte)7,
                            je = (string.IsNullOrEmpty(CZJF)) ? -Convert.ToDecimal(KJJF) : Convert.ToDecimal(CZJF),                            
                            fs = (byte)6,
                            czr = HandoverModel.GetInstance.userID,
                            lsh = HandoverModel.GetInstance.scode
                        };
                        db.hd_vip_cz.Add(jf_info);
                    }


                    var re = db.SaveChanges();
                    if (re > 0)
                    {
                        VipJFPrinter pr = new VipJFPrinter(JFtemp, 0, Czjf, 0, JFinfo.vipcard, JFinfo.vipname,"会员冲减积分凭证");
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
                MessageBox.Show("数据库连接出错！");
                string tip = ConnectionHelper.ToDo();
                if (!string.IsNullOrEmpty(tip))
                {
                    MessageBox.Show(tip);
                }
            }
        }



        //F4会员余额冲减
        public void VipYEFunc()
        {
            //try
            //{
                string vipcard_temp = string.Empty;
                string vipid = "";
                if (vipList.Count == 0)
                {
                    MessageBox.Show("请先查询会员！");
                    return;
                }
                else
                {
                    int index_temp = dataGridView1.SelectedRows[0].Index;
                    vipcard_temp = vipList[index_temp].vipCard;
                    vipid = vipList[index_temp].vipCode.ToString();
                }
                if (string.IsNullOrEmpty(CZYE) && string.IsNullOrEmpty(KJYE)) return;
                using (var db = new hjnbhEntities())
                {
                    var JFinfo = db.hd_vip_info.Where(t => t.vipcard == vipcard_temp).FirstOrDefault();
                    decimal YEtemp = 0;  //总余额
                    decimal cztemp = 0;  //本次充值
                    bool isYE = false;
                    if (!string.IsNullOrEmpty(CZYE))
                    {
                        decimal temp = JFinfo.czk_ye.HasValue ? JFinfo.czk_ye.Value : 0;
                        temp += Convert.ToDecimal(CZYE);
                        cztemp = Convert.ToDecimal(CZYE);
                        JFinfo.czk_ye = temp;
                        YEtemp = temp;
                        isYE = true;
                    }
                    if (!string.IsNullOrEmpty(KJYE))
                    {
                        decimal temp = JFinfo.czk_ye.HasValue ? JFinfo.czk_ye.Value : 0;
                        temp -= Convert.ToDecimal(KJYE);
                        cztemp = -Convert.ToDecimal(KJYE);
                        JFinfo.czk_ye = temp;
                        YEtemp = temp;
                        isYE = true;
                    }

                    if (isYE)
                    {
                        decimal Ye = (string.IsNullOrEmpty(CZYE)) ? -Convert.ToDecimal(KJYE) : Convert.ToDecimal(CZYE);
                        var CJinfo = new hd_vip_cz
                        {
                            //ckh = vipcard_temp,
                            ckh = vipid,
                            rq = System.DateTime.Now,
                            //je = (string.IsNullOrEmpty(CZYE)) ? -Convert.ToDecimal(KJYE) : Convert.ToDecimal(CZYE),
                            //fs = (string.IsNullOrEmpty(CZYE)) ? (byte)3 : (byte)2,
                            je = Ye,
                            fs = (byte)2,
                            czr = HandoverModel.GetInstance.userID,
                            lsh = HandoverModel.GetInstance.scode
                        };
                        db.hd_vip_cz.Add(CJinfo);

                    }

                    var re = db.SaveChanges();
                    if (re > 0)
                    {
                        VipJFPrinter pr = new VipJFPrinter(0, YEtemp, 0, cztemp, JFinfo.vipcard, JFinfo.vipname, "会员冲减余额凭证");
                        pr.StartPrint();

                        CZYE = KJYE = string.Empty;
                        label22.Text = YEtemp.ToString() + " 元";
                        MessageBox.Show("余额冲减成功！");
                    }
                    else
                    {
                        MessageBox.Show("余额冲减失败！");

                    }


                }
            //}
            //catch (Exception e)
            //{
            //    LogHelper.WriteLog("会员余额冲减窗口冲减积分时出现异常:", e);
            //    MessageBox.Show("数据库连接出错！");
            //    string tip = ConnectionHelper.ToDo();
            //    if (!string.IsNullOrEmpty(tip))
            //    {
            //        MessageBox.Show(tip);
            //    }
            //}
        }


        //会员修改密码
        private void UpdateVIPPW()
        {
            try
            {
                string vipcard_temp = string.Empty;
                if (vipList.Count == 0)
                {
                    MessageBox.Show("请先查询会员！");
                    return;
                }
                else
                {
                    int index_temp = dataGridView1.SelectedRows[0].Index;
                    vipcard_temp = vipList[index_temp].vipCard;
                }

                using (var db = new hjnbhEntities())
                {
                    var JFinfo = db.hd_vip_info.Where(t => t.vipcard == vipcard_temp).FirstOrDefault();
                    string newPW = Interaction.InputBox("请输入密码", "输入密码", "", -1, -1);
                    int temp = 0;
                    if (!string.IsNullOrEmpty(newPW.Trim()))
                    {
                        int.TryParse(newPW, out temp);
                        JFinfo.password = temp;
                        var re = db.SaveChanges();
                        if (re > 0)
                            MessageBox.Show("密码修改成功！");
                        else
                            MessageBox.Show("密码修改失败！");
                    }


                }
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("会员积分冲减窗口修改密码时出现异常:", e);
                MessageBox.Show("数据库连接出错！");
                string tip = ConnectionHelper.ToDo();
                if (!string.IsNullOrEmpty(tip))
                {
                    MessageBox.Show(tip);
                }
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








    }
}
