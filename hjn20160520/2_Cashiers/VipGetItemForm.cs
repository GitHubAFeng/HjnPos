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

                    //if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
                    //{

                    //    if (string.IsNullOrEmpty(textBox4.Text.Trim()))
                    //    {
                    //        MessageBox.Show("请输入会员卡号或者手机号");
                    //    }
                    //    else
                    //    {
                    //        CXFunc();

                    //    }
                    //}

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
                    GetVipItemFunc();
                    
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
                string goodsinfotemp = ""; //一共取出的商品
                int vipNo = 0;  //会员
                if (savedlist.Count > 0)
                {
                    using (var db = new hjnbhEntities())
                    {
                        foreach (var item in WantGetlist)
                        {
                            var getiteminfo = db.hd_vip_item.Where(e => e.item_id == item.itemid && e.vipcode == item.vipid && e.amount > 0).FirstOrDefault();
                            if (getiteminfo != null)
                            {
                                if (getiteminfo.amount - item.count >= 0)
                                {
                                    getiteminfo.scode = HandoverModel.GetInstance.scode;
                                    getiteminfo.amount -= item.count;
                                    getiteminfo.cid = HandoverModel.GetInstance.userID;
                                    getiteminfo.ctime = System.DateTime.Now;

                                    goodsinfotemp += "[" + item.itemid + "/" + item.cname + "*" + item.count.ToString() + "] ";
                                }

                                vipNo = getiteminfo.vipcode;
                            }
                            else
                            {
                                MessageBox.Show("没有找到该会员存放商品！");
                            }

                        }

                        string temp = System.DateTime.Now.Date.ToString("yyyy-MM-dd") + "： " + " 会员取出商品 " + goodsinfotemp + ";";

                        //会员取货自动备注
                        var VipMemoinfo2 = db.hd_vip_memo.Where(t => t.vipcode == vipNo && t.type == 2).FirstOrDefault();
                        if (VipMemoinfo2 != null)
                        {
                          
                            VipMemoinfo2.memo += temp;
                        }
                        else
                        {
                            //没有就新建
                            var newinfo2 = new hd_vip_memo
                            {
                                vipcard = HandoverModel.GetInstance.VipCard,
                                vipcode = vipNo,
                                vipname = HandoverModel.GetInstance.VipName,
                                scode = HandoverModel.GetInstance.scode,
                                cid = HandoverModel.GetInstance.userID,
                                memo = temp,
                                type = 2,
                                ctime = System.DateTime.Now
                            };

                            db.hd_vip_memo.Add(newinfo2);
                        }

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


        private void F3Func()
        {

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


        private void getVipItem(int id)
        {
            savedlist.Clear();
            using (var db = new hjnbhEntities())
            {
                var infolist = db.hd_vip_item.AsNoTracking().Where(e => e.vipcode == id && e.amount > 0).ToList();
                if (infolist.Count > 0)
                {
                    foreach (var item in infolist)
                    {
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
                            scode =item.scode,
                            

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


        private void CXFunc()
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

                int itemid_temp = -1;
                int.TryParse(temptxt, out itemid_temp);

                var getitem = savedlist.Where(t => t.tm.Contains(temptxt) || t.cname.Contains(temptxt) || t.itemid == itemid_temp).FirstOrDefault();
                if (getitem != null)
                {
                    if (getitem.count < count_temp)
                    {
                        MessageBox.Show("取出数量不能大于已存数量，请重新输入！");
                        textBox2.Focus();
                        textBox2.SelectAll();
                    }
                    else
                    {

                        getitem.count -= count_temp;

                        var goods =  new VipItemModel
                        {
                            itemid = getitem.itemid,
                            tm = getitem.tm,
                            cid = HandoverModel.GetInstance.userID,
                            cname = getitem.cname,
                            scode =HandoverModel.GetInstance.scode,
                            vipName = getitem.vipName,
                            vipid = getitem.vipid,
                            vipcard = getitem.vipcard,
                            ctime = System.DateTime.Now,
                            count = count_temp
                        };

                        var itemed = WantGetlist.Where(e => e.itemid == goods.itemid).FirstOrDefault();
                        if (itemed != null)
                        {
                            itemed.count += goods.count;
                        }
                        else
                        {
                            WantGetlist.Add(goods);
                        }


                        dataGridView1.Refresh();
                        dataGridView2.Refresh();

                    }


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
                    DialogResult RSS = MessageBox.Show(this, "确定要删除选中的商品？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    switch (RSS)
                    {
                        case DialogResult.Yes:

                            int DELindex_temp = dataGridView2.SelectedRows[0].Index;
                            //dataGridView_Cashiers.Rows.RemoveAt(DELindex_temp);

                            //string de_temp = dataGridView_Cashiers.CurrentRow.Cells[2].Value.ToString();

                            WantGetlist.RemoveAt(DELindex_temp);
                            dataGridView2.Refresh();

                            if (DELindex_temp - 1 >= 0)
                            {
                                dataGridView2.Rows[DELindex_temp - 1].Selected = true;
                            }
                            break;
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
                dataGridView2.Columns[9].HeaderText = "时间";

                //隐藏      
                dataGridView2.Columns[4].Visible = false;
                dataGridView2.Columns[5].Visible = false;
                dataGridView2.Columns[6].Visible = false;
                dataGridView2.Columns[7].Visible = false;
                dataGridView2.Columns[8].Visible = false;
                //dataGridView2.Columns[9].Visible = false;
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
                dataGridView1.Columns[9].HeaderText = "时间";

                //隐藏      
                dataGridView1.Columns[4].Visible = false;
                dataGridView1.Columns[5].Visible = false;
                dataGridView1.Columns[6].Visible = false;
                dataGridView1.Columns[7].Visible = false;
                dataGridView1.Columns[8].Visible = false;
                //dataGridView1.Columns[9].Visible = false;

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
            }
           

            textBox1.Clear();
            textBox2.Text = "1";
            this.dataGridView1.DataSource = savedlist;
            this.dataGridView2.DataSource = WantGetlist;
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
            GetVipItemFunc();

        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    textBox1.Text = dataGridView1.SelectedRows[0].Cells[1].Value as string;
                }
            }
            catch
            {
                
            }

        }










    }
}
