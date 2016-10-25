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
using System.Windows.Forms;

namespace hjn20160520._2_Cashiers
{
    public partial class VipGetItemForm : Form
    {


        //已存的商品
        private BindingList<VipItemModel> savedlist = new BindingList<VipItemModel>();

        //需要存的商品
        private BindingList<VipItemModel> WantGetlist = new BindingList<VipItemModel>();

        InputBoxForm passwordForm = new InputBoxForm();  //会员密码检证窗口
        string VipPW = ""; //会员密码验证

        string vipname = "";  //会员名字
        int vipid = -1;
        string vipcard = "";


        public VipGetItemForm()
        {
            InitializeComponent();
        }

        private void VipGetItemForm_KeyDown(object sender, KeyEventArgs e)
        {

            switch (e.KeyCode)
            {

                //向上键表格换行
                case Keys.Up:

                    UpFun();

                    break;

                //向下键表格换行
                case Keys.Down:

                    DownFun();

                    break;

                case Keys.F5:
                    textBox2.Focus();
                    textBox2.SelectAll();
                    break;

                case Keys.F3:
                    F3Func();
                    break;

                case Keys.Escape:
                    this.Close();
                    break;

                case Keys.F4:
                    textBox1.Focus();
                    textBox1.SelectAll();

                    break;

                case Keys.Delete:
                    Dele();
                    break;

                case Keys.Enter:

                    if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
                    {

                        if (string.IsNullOrEmpty(textBox4.Text.Trim()))
                        {
                            MessageBox.Show("请输入会员卡号或者手机号");
                        }
                        else
                        {
                            CXFunc();

                        }
                    }

                    break;


                case Keys.F7:
                    F7Func();

                    break;

            }



        }


        /// <summary>
        /// 取出商品，保存数据库
        /// </summary>
        private void GetVipItemFunc()
        {
            try
            {
                if (HandoverModel.GetInstance.isLianxi)
                {
                    MessageBox.Show("练习模式下该操作无效！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string goodsinfotemp = ""; //一共取出的商品
                int vipNo = 0;  //会员
                if (savedlist.Count > 0)
                {
                    using (var db = new hjnbhEntities())
                    {
                        foreach (var item in WantGetlist)
                        {
                            var getiteminfo = db.hd_vip_item.Where(e => e.item_id == item.itemid && e.vipcode == item.vipid && e.amount > 0).ToList();
                            if (getiteminfo.Count > 0)
                            {
                                decimal sumcount = getiteminfo.Select(t => t.amount).Sum();
                                if (sumcount - item.count >= 0)
                                {
                                    foreach (var getitem in getiteminfo)
                                    {
                                        if (getitem.scode == item.scode)
                                        {
                                            if (getitem.amount > item.count)
                                            {
                                                getitem.amount -= item.count;
                                            }
                                            else
                                            {
                                                getitem.amount = 0.00m;
                                            }

                                            getitem.cid = HandoverModel.GetInstance.userID;
                                            //getitem.scode = HandoverModel.GetInstance.scode;
                                            getitem.ctime = System.DateTime.Now;
                                            decimal gettemp = getitem.get_count.HasValue ? getitem.get_count.Value : 0.00m;
                                            gettemp += item.count;
                                            getitem.get_count = gettemp;
                                        }
                                    }


                                    goodsinfotemp += "[" + item.itemid + "/" + item.cname + "*" + item.count.ToString() + "] ";
                                }

                                vipNo = getiteminfo[0].vipcode;
                            }
                            else
                            {
                                MessageBox.Show("没有找到该会员存放商品！");
                            }

                        }

                        //会员取货自动备注
                        string temp2 = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ： " + " 会员取出商品 " + goodsinfotemp + ";";
                        VipAutoMemoFunc(db, vipNo, HandoverModel.GetInstance.VipCard, HandoverModel.GetInstance.VipName, temp2, 2);

                        var re = db.SaveChanges();
                        if (re > 0)
                        {
                            VipItemPrinter printer = new VipItemPrinter(WantGetlist, vipcard, vipname, "会员取货凭证");
                            printer.StartPrint();

                            MessageBox.Show("会员商品取出成功！");
                            WantGetlist.Clear();
                            textBox1.Clear();
                        }
                        else
                        {
                            MessageBox.Show("取出失败，请核实该商品信息的真实性！");

                        }
                    }

                }
                else
                {
                    MessageBox.Show("当前没有选择商品");
                }
            }
            catch (Exception ex)
            {

                LogHelper.WriteLog("会员取货窗口确认取出时出现异常:", ex);
                MessageBox.Show("会员取货时出现异常！请联系管理员！");
            }


        }


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



        private void F3Func()
        {

            if (WantGetlist.Count > 0)
            {
                if (DialogResult.Yes == MessageBox.Show("目前待取商品已被锁定，是否要清空待取列表并刷新已存商品列表？", "提醒", MessageBoxButtons.YesNo))
                {
                    WantGetlist.Clear();
                }
            }


            //查询会员 
            if (string.IsNullOrEmpty(textBox4.Text.Trim()))
            {
                textBox4.Focus();
                textBox4.SelectAll();
            }
            else
            {
                VipCardFunc(textBox4.Text.Trim());
                label7.Text = vipname;
                if (vipid != -1)
                {
                    getVipItem(vipid);
                }
            }

        }



        public bool VipCardFunc(string card = "")
        {
            try
            {
                string text_temp = string.Empty;
                if (card != "")
                {
                    text_temp = card;
                }
                else
                {
                    if (!string.IsNullOrEmpty(textBox4.Text.Trim()))
                    {
                        text_temp = textBox4.Text.Trim();
                    }

                }

                if (string.IsNullOrEmpty(text_temp))
                {
                    return false;
                }

                using (var db = new hjnbhEntities())
                {
                    //手机号也能查
                    var vipInfos = db.hd_vip_info.AsNoTracking().Where(t => t.vipcard == text_temp || t.tel == text_temp)
                        .Select(t => new { t.vipname, t.vipcard, t.vipcode }).FirstOrDefault();


                    if (vipInfos != null)
                    {
                        this.vipcard = vipInfos.vipcard;
                        this.vipid = vipInfos.vipcode;
                        this.vipname = vipInfos.vipname;

                        return true;
                    }
                    else
                    {
                        MessageBox.Show("查询失败，请核实会员卡号是否正确！");
                    }


                }
                return false;
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("会员取货窗口登记时出现异常:", e);
                MessageBox.Show("数据库连接出错！");
                return false;
            }

        }

        /// <summary>
        /// 查询已经存的商品
        /// </summary>
        /// <param name="id"></param>
        private void getVipItem(int id)
        {
            savedlist.Clear();
            using (var db = new hjnbhEntities())
            {
                var infolist = db.hd_vip_item.AsNoTracking().Where(e => e.vipcode == id && e.amount > 0).ToList();
                if (infolist.Count > 0)
                {
                    //查分店名字
                    var scodeinfoDict = db.hd_dept_info.AsNoTracking().Select(t => new { t.scode, t.cname }).ToDictionary(k => k.scode, v => v.cname);

                    foreach (var item in infolist)
                    {
                        string temp = "";
                        scodeinfoDict.TryGetValue(item.scode, out temp);

                        savedlist.Add(new VipItemModel
                        {
                            itemid = item.item_id,
                            tm = item.tm,
                            count = item.amount,
                            cname = item.cname,
                            ctime = item.ctime.Value,
                            vipcard = item.vipcard,
                            vipid = item.vipcode,
                            vipName = item.vipname,
                            scode = item.scode,
                            scodeStr = temp,
                            cid = item.cid.HasValue ? item.cid.Value : 0
                        });

                    }

                    dataGridView1.Refresh();

                    if (savedlist.Count > 0)
                    {
                        textBox1.Text = savedlist[0].tm;
                        textBox1.Focus();
                        textBox1.SelectAll();
                    }

                }
            }
        }


        private void CXFunc(int goodsid = 0)
        {
            try
            {
                string temptxt = textBox1.Text.Trim();
                if (string.IsNullOrEmpty(temptxt))
                {
                    MessageBox.Show("请输入需要取出的商品条码/货号/品名!");
                    return;
                }

                if (string.IsNullOrEmpty(textBox2.Text.Trim()))
                {
                    MessageBox.Show("请输入需要取出的商品数量！");
                    textBox2.Focus();
                    return;
                }

                decimal count_temp = 1;
                decimal.TryParse(textBox2.Text.Trim(), out count_temp);


                if (goodsid == 0)
                {
                    if (dataGridView1.Rows.Count > 0)
                    {
                        int indexid = dataGridView1.SelectedRows[0].Index;
                        goodsid = savedlist[indexid].itemid;
                    }
                }


                var getitem = savedlist.Where(t => t.itemid == goodsid || t.tm.Contains(temptxt) || t.cname.Contains(temptxt)).ToList();
                if (getitem.Count > 0)
                {

                    for (int i = 0; i < getitem.Count; i++)
                    {

                        if (getitem[i].count < count_temp)
                        {
                            MessageBox.Show("商品[" + getitem[i].cname + "]取出数量不能大于已存数量，请重新输入！");
                            textBox2.Focus();
                            textBox2.SelectAll();
                            continue;
                        }

                        var wantgetitem = WantGetlist.Where(t => t.itemid == getitem[i].itemid && t.scode == getitem[i].scode).FirstOrDefault();
                        if (wantgetitem != null)
                        {
                            wantgetitem.count += count_temp;
                            getitem[i].count -= count_temp;
                        }
                        else
                        {
                            getitem[i].count -= count_temp;

                            var goods = new VipItemModel
                            {
                                itemid = getitem[i].itemid,
                                tm = getitem[i].tm,
                                cid = getitem[i].cid,
                                cname = getitem[i].cname,
                                scode = getitem[i].scode,
                                vipName = getitem[i].vipName,
                                vipid = getitem[i].vipid,
                                vipcard = getitem[i].vipcard,
                                ctime = System.DateTime.Now,
                                count = count_temp,
                                scodeStr = getitem[i].scodeStr
                            };

                            WantGetlist.Add(goods);

                        }
                    }



                    dataGridView1.Refresh();
                    dataGridView2.Refresh();

                }
                else
                {
                    MessageBox.Show("没有查询到商品，请核对输入的商品信息");
                }





            }
            catch (Exception e)
            {
                LogHelper.WriteLog("会员存货商品查询窗口查询商品时出现异常:", e);
                MessageBox.Show("员存货商品查询出现异常！请联系管理员！");
            }
        }





        private void Dele()
        {
            try
            {
                //当前行数大于1行时删除选中行后把往上一行设置为选中状态
                if (dataGridView2.Rows.Count > 0)
                {

                    if (DialogResult.Yes == MessageBox.Show("确定要删除选中的商品？", "提醒", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
                    {
                        int DELindex_temp = dataGridView2.SelectedRows[0].Index;
                        //把数量还原
                        decimal numtemp = WantGetlist[DELindex_temp].count;
                        int idtemp = WantGetlist[DELindex_temp].itemid;
                        int scodetemp = WantGetlist[DELindex_temp].scode;
                        var savediteminfo = savedlist.Where(t => t.itemid == idtemp && t.scode == scodetemp).FirstOrDefault();
                        if (savediteminfo != null)
                        {
                            savediteminfo.count += numtemp;
                        }

                        WantGetlist.RemoveAt(DELindex_temp);

                        dataGridView1.Refresh();
                        dataGridView2.Refresh();

                        if (DELindex_temp - 1 >= 0)
                        {
                            dataGridView2.Rows[DELindex_temp - 1].Selected = true;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("收银主界面删除选中行发生异常:", ex);
            }
        }



        //小键盘向上
        private void UpFun()
        {
            try
            {
                //当前行数大于1行才生效
                if (dataGridView2.Rows.Count > 1)
                {
                    int rowindex_temp = dataGridView2.SelectedRows[0].Index;
                    if (rowindex_temp == 0)
                    {
                        dataGridView2.Rows[dataGridView2.Rows.Count - 1].Selected = true;
                        dataGridView2.Rows[rowindex_temp].Selected = false;

                    }
                    else
                    {
                        dataGridView2.Rows[rowindex_temp - 1].Selected = true;
                        dataGridView2.Rows[rowindex_temp].Selected = false;
                    }

                }

            }
            catch (Exception ex)
            {

                LogHelper.WriteLog("会员取货小键盘向上时发生异常：" + ex);

            }
        }

        //小键盘向下
        private void DownFun()
        {
            try
            {
                //当前行数大于1行才生效
                if (dataGridView2.Rows.Count > 1)
                {
                    int rowindexDown_temp = dataGridView2.SelectedRows[0].Index;
                    if (rowindexDown_temp == dataGridView2.Rows.Count - 1)
                    {
                        dataGridView2.Rows[0].Selected = true;
                        dataGridView2.Rows[rowindexDown_temp].Selected = false;

                    }
                    else
                    {
                        dataGridView2.Rows[rowindexDown_temp + 1].Selected = true;
                        dataGridView2.Rows[rowindexDown_temp].Selected = false;
                    }

                }

            }
            catch (Exception ex)
            {

                LogHelper.WriteLog("会员取货小键盘向下时发生异常：" + ex);

            }
        }

        private void dataGridView2_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                //列名
                dataGridView2.Columns[0].HeaderText = "货号";
                dataGridView2.Columns[1].HeaderText = "条码";
                dataGridView2.Columns[2].HeaderText = "品名";
                dataGridView2.Columns[3].HeaderText = "数量";
                dataGridView2.Columns[8].HeaderText = "分店";
                dataGridView2.Columns[10].HeaderText = "时间";

                //隐藏      
                dataGridView2.Columns[4].Visible = false;
                dataGridView2.Columns[5].Visible = false;
                dataGridView2.Columns[6].Visible = false;
                dataGridView2.Columns[7].Visible = false;
                dataGridView2.Columns[9].Visible = false;
            }
            catch
            {
            }
        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                //列名
                dataGridView1.Columns[0].HeaderText = "货号";
                dataGridView1.Columns[1].HeaderText = "条码";
                dataGridView1.Columns[2].HeaderText = "品名";
                dataGridView1.Columns[3].HeaderText = "数量";
                dataGridView1.Columns[8].HeaderText = "分店";
                dataGridView1.Columns[10].HeaderText = "时间";

                //隐藏      
                dataGridView1.Columns[4].Visible = false;
                dataGridView1.Columns[5].Visible = false;
                dataGridView1.Columns[6].Visible = false;
                dataGridView1.Columns[7].Visible = false;
                dataGridView1.Columns[9].Visible = false;

            }
            catch
            {
            }
        }

        private void VipGetItemForm_Load(object sender, EventArgs e)
        {
            this.ActiveControl = textBox4;
            this.textBox4.Focus();
            if (!string.IsNullOrEmpty(HandoverModel.GetInstance.VipCard))
            {
                textBox4.Text = HandoverModel.GetInstance.VipCard;
                this.textBox4.SelectAll();
                F3Func();
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    textBox1.Text = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
                }
            }


            textBox1.Clear();
            textBox2.Text = "1";
            this.dataGridView1.DataSource = savedlist;
            this.dataGridView2.DataSource = WantGetlist;

            VipPW = "";
            passwordForm.changed += passwordForm_changed;


        }

        void passwordForm_changed(string PW)
        {
            this.VipPW = PW;
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' && e.KeyChar != '.' || e.KeyChar > '9' && e.KeyChar != '.' || ((TextBox)(sender)).Text.IndexOf('.') >= 0 && e.KeyChar == '.') && e.KeyChar != (char)13 && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            F3Func();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Dele();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
            {

                if (string.IsNullOrEmpty(textBox4.Text.Trim()))
                {
                    MessageBox.Show("请输入会员卡号或者手机号");
                }
                else
                {
                    CXFunc();

                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            F7Func();

        }

        //取出
        private void F7Func()
        {
            if (HandoverModel.GetInstance.isLianxi)
            {
                MessageBox.Show("练习模式下该操作无效！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (this.vipid != 0)
            {
                using (var db = new hjnbhEntities())
                {
                    var Vippasswordinfo = db.hd_vip_info.Where(t => t.vipcode == vipid).Select(t => t.password).FirstOrDefault();

                    string vippw = Vippasswordinfo.HasValue ? Vippasswordinfo.Value.ToString() : "0";
                    //先验证密码
                    passwordForm.ShowDialog();
                    if (VipPW != vippw)
                    {
                        MessageBox.Show("会员密码检验失败！请输入正确的会员密码！可尝试使用默认密码 0 。", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else
                    {
                        GetVipItemFunc();

                        VipPW = ""; //会员密码验证，用完要清空，防止被盗用
                    }
                }

            }
            else
            {
                MessageBox.Show("您还没有登记会员，请先在按F3查询会员！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }




        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    textBox1.Text = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
                }
            }
            catch
            {

            }

        }

        private void VipGetItemForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            passwordForm.changed -= passwordForm_changed;

        }










    }
}
