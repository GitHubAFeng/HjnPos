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

namespace hjn20160520._2_Cashiers
{
    public partial class ItemInfoForm : Form
    {
        TipForm tipForm;

        private BindingList<GoodsBuy> goodsInfoList = new BindingList<GoodsBuy>();

        public ItemInfoForm()
        {
            InitializeComponent();
        }


        private void ItemInfoForm_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void ItemInfoForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.Close();
                    break;

                case Keys.Enter:
                    CXFunc();
                    textBox1.SelectAll();
                    break;

                case Keys.F3:
                    ShowUIInfo(HandoverModel.GetInstance.scode);
                    break;

                case Keys.Up:
                    UpFun();
                    break;

                case Keys.Down:
                    DownFun();
                    break;

            }
        }

        //查询方法
        private void CXFunc()
        {

            string temptxt = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(temptxt))
            {
                tipForm.Tiplabel.Text = "请输入需要查找的商品条码!";
                tipForm.ShowDialog();
                return;
            }
            if (goodsInfoList.Count > 0) goodsInfoList.Clear();
            using (hjnbhEntities db = new hjnbhEntities())
            {
                var rules = db.hd_item_info.AsNoTracking().Where(t => t.tm.Contains(temptxt)).ToList();
                        
                //如果查出数据不至一条就弹出选择窗口，否则直接显示出来
                if (rules.Count == 0)
                {
                    this.textBox1.SelectAll();
                    tipForm.Tiplabel.Text = "没有查找到该商品!";
                    tipForm.ShowDialog();
                    return; 
                }
                #region 查到多条记录时
                string tip_temp = Tipslabel.Text;
                Tipslabel.Text = "商品正在查询中，请稍等！";

                foreach (var item in rules)
                {
                    #region 商品单位查询
                    //需要把单位编号转换为中文以便UI显示
                    int unitID = item.unit.HasValue ? (int)item.unit : 1;
                    string dw = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == unitID).Select(t => t.txt1).FirstOrDefault();
                    #endregion

                    goodsInfoList.Add(new GoodsBuy
                    {
                        noCode=item.item_id,
                        barCodeTM=item.tm,
                        goods=item.cname,
                        spec=item.spec,
                        unitStr=dw,
                        lsPrice=item.ls_price,
                        hyPrice=item.hy_price,

                    });
                }

                Tipslabel.Text = tip_temp;

                dataGridView1.DataSource = goodsInfoList;
                //NotShowFunc();

                #endregion
            }
        }


        //显示下方详情
        private void ShowUIInfo(int scode)
        {
            int itemid_temp = 0;
            if (goodsInfoList.Count == 0)
            {
                MessageBox.Show("请先查询会员！");
                return;
            }
            else
            {
                int index_temp = dataGridView1.SelectedRows[0].Index;
                itemid_temp = goodsInfoList[index_temp].noCode;
            }

            using (hjnbhEntities db = new hjnbhEntities())
            {
                var info = db.hd_istore.AsNoTracking().Where(t => t.scode == scode && t.item_id == itemid_temp).FirstOrDefault();
                if (info != null)
                {
                    label8.Text = info.amount.ToString();  //库存

                }


                var rules = db.hd_item_info.AsNoTracking().Where(t => t.item_id == itemid_temp).FirstOrDefault();
                if (rules != null)
                {
                    int temp = rules.status.HasValue ? (int)rules.status.Value : 0;
                    string str_temp = string.Empty;
                    switch (temp)
                    {
                        case 0:
                            str_temp = "正常";
                            break;
                        case 1:
                            str_temp = "只销不进";
                            break;
                        case 2:
                            str_temp = "停止销售";
                            break;
                    }

                    int zk = rules.isale.HasValue ? rules.isale.Value : 1;
                    string str_zk = string.Empty;
                    switch (zk)
                    {
                        case 0:
                            str_zk = "可以";
                            break;
                        case 1:
                            str_zk = "不能";
                            break;
                    }

                    label10.Text = itemid_temp.ToString();  //货号
                    label9.Text = str_temp; //状态  
                    label7.Text = str_zk;   //能否打折
                }

            }
        }


        private void ItemInfoForm_Load(object sender, EventArgs e)
        {
            tipForm = new TipForm();
            textBox1.Focus();
            textBox1.SelectAll();
            label8.Text = string.Empty;  //库存
            label10.Text = string.Empty; 
            label9.Text = string.Empty;
            label7.Text = string.Empty; 
        }




        //调整表格的列宽、同时隐藏不需要显示的列、禁止编辑、修改列名
        private void UpdateNameFunc()
        {
            try
            {
                //列名
                dataGridView1.Columns[1].HeaderText = "条码";
                dataGridView1.Columns[2].HeaderText = "品名";
                dataGridView1.Columns[3].HeaderText = "规格";
                dataGridView1.Columns[6].HeaderText = "单位";
                dataGridView1.Columns[8].HeaderText = "零售价";
                dataGridView1.Columns[9].HeaderText = "会员价";
                dataGridView1.Columns[11].HeaderText = "拼音";


                //隐藏      
                dataGridView1.Columns[0].Visible = false;
                dataGridView1.Columns[4].Visible = false;
                dataGridView1.Columns[5].Visible = false;
                dataGridView1.Columns[7].Visible = false;
                dataGridView1.Columns[10].Visible = false;
                dataGridView1.Columns[11].Visible = false;
                dataGridView1.Columns[12].Visible = false;
                dataGridView1.Columns[13].Visible = false;
                dataGridView1.Columns[14].Visible = false;
                dataGridView1.Columns[15].Visible = false;
                dataGridView1.Columns[16].Visible = false;
                dataGridView1.Columns[17].Visible = false;
                //列宽   
                dataGridView1.Columns[2].Width = 200;

            }
            catch
            {
            }
        }
        //隐藏不需要显示的列 0   4  5   7   10- 17
        private void NotShowFunc()
        {
            if (dataGridView1.Rows.Count > 0)
            {
                try
                {
                    dataGridView1.Columns[0].Visible = false;
                    dataGridView1.Columns[4].Visible = false;
                    dataGridView1.Columns[5].Visible = false;
                    dataGridView1.Columns[7].Visible = false;
                    dataGridView1.Columns[10].Visible = false;
                    dataGridView1.Columns[11].Visible = false;
                    dataGridView1.Columns[12].Visible = false;
                    dataGridView1.Columns[13].Visible = false;
                    dataGridView1.Columns[14].Visible = false;
                    dataGridView1.Columns[15].Visible = false;
                    dataGridView1.Columns[16].Visible = false;
                    dataGridView1.Columns[17].Visible = false;
                }
                catch
                {
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

                    }
                    else
                    {
                        dataGridView1.Rows[rowindex_temp - 1].Selected = true;
                        dataGridView1.Rows[rowindex_temp].Selected = false;
                    }

                }

            }
            catch (Exception ex)
            {

                LogHelper.WriteLog("商品信息窗口小键盘向上时发生异常：" + ex);

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

                    }
                    else
                    {
                        dataGridView1.Rows[rowindexDown_temp + 1].Selected = true;
                        dataGridView1.Rows[rowindexDown_temp].Selected = false;
                    }

                }

            }
            catch (Exception ex)
            {

                LogHelper.WriteLog("商品信息窗口小键盘向下时发生异常：" + ex);

            }
        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            UpdateNameFunc();
        }

    }
}