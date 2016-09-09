using Common;
using hjn20160520.Common;
using hjn20160520.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Forms;

namespace hjn20160520._8_ReplenishRequest
{
    //处理补货商品申请逻辑
    public partial class RequsetNoteForm : Form
    {

        //提示信息窗口
        TipForm tipForm;
        int tempOID = -1;  //经办人ID
        //经办人名字
        string relusName = string.Empty;

        public RequsetNoteForm()
        {
            InitializeComponent();
        }

        private void RequsetNoteForm_Load(object sender, EventArgs e)
        {
            tipForm = new TipForm();
            textBox1.Focus();
            dataGridView1.DataSource = ReplenishRequestForm.GetInstance.GoodsList;
            this.comboBox5.SelectedIndex = 0;
            //DeShowGoods(); //隐藏列
            InitRNForm(); //初始化

            textBox2.Text = "1";
        }

        //初始化新单窗口
        private void InitRNForm()
        {
            button1.Enabled = false;
            textBox1.Enabled = textBox2.Enabled = true;
            label7.Text = System.DateTime.Now.ToString("yyyy-MM-dd");  //制单日
            label11.Text = "未审核";  //审核日
            label5.Text = "未传送";  //单号
            label27.Text = "";
            label28.Text = "";
            textBox1.Text = "";
            textBox2.Text = "00";
            label10.Text = label6.Text = HandoverModel.GetInstance.userName + "(" + HandoverModel.GetInstance.RoleName + ")";

            ReplenishRequestForm.GetInstance.isMK = false;
            //ReplenishRequestForm.GetInstance.GoodsList.Clear();

            //判断修改状态
            if (!string.IsNullOrEmpty(ReplenishRequestForm.GetInstance.Sta_Temp) && ReplenishRequestForm.GetInstance.isUpdate)
            {
                label4.Text = ReplenishRequestForm.GetInstance.Sta_Temp;
            }

            if (!string.IsNullOrEmpty(ReplenishRequestForm.GetInstance.code_Temp) && ReplenishRequestForm.GetInstance.isUpdate)
            {
                label5.Text = ReplenishRequestForm.GetInstance.code_Temp;
            }
        }
        //统计单数与数量合计
        private void ShowUIFunc()
        {
            label27.Text = ReplenishRequestForm.GetInstance.GoodsList.Count.ToString();
            decimal? temp = 0;
            foreach (var item in ReplenishRequestForm.GetInstance.GoodsList)
            {
                temp += item.countNum;
            }

            label28.Text = temp.ToString();
        }


        //重写热键方法
        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
        {
            int WM_KEYDOWN = 256;
            int WM_SYSKEYDOWN = 260;
            if (msg.Msg == WM_KEYDOWN | msg.Msg == WM_SYSKEYDOWN)
            {
                switch (keyData)
                {
                    //ESC退出
                    case Keys.Escape:

                        this.Close();//esc关闭窗体
                        break;
                    case Keys.Up:
                        UpFun();
                        break;
                    case Keys.Down:
                        DownFun();
                        break;
                    //删除
                    case Keys.Delete:
                        Dele();
                        break;
                    //条码
                    case Keys.F1:
                        textBox1.Focus();
                        textBox1.SelectAll();
                        break;
                    //数量
                    case Keys.F2:
                        textBox2.Focus();
                        textBox2.SelectAll();
                        break;

                    case Keys.F3:

                        GetOidFunc();

                        break;
                    //审核
                    case Keys.F4:
                        //OnMakeTureFunc();   //前台不需要审核
                        break;
                    //好像没什么用，我隐藏先
                    //case Keys.F5:
                    //    this.Close();
                    //    break;

                    //上传
                    case Keys.F6:
                        OnUploadFunc();
                        break;

                    case Keys.Enter:
                        EnterFunc();
                        break;
                }

            }
            return false;
        }

        //处理上传的逻辑
        private void OnUploadFunc()
        {
            //目前的需求是无论是否通过审核也要上传保存
            //if (ReplenishRequestForm.GetInstance.isMK)
            //{
            //    UpdataDBFunc();
            //    if (isSubmited > 0)
            //    {
            //        MessageBox.Show("单据上传成功！");
            //        isSubmited = 0;
            //    }
            //}
            //else
            //{
            //    tipForm.Tiplabel.Text = "您的单据还未经过审核不能提交！";
            //    tipForm.ShowDialog();
            //}

            UpdataDBFunc();
            if (isSubmited > 0)
            {
                MessageBox.Show("单据上传成功！");
                isSubmited = 0;
            }

        }


        //根据条码查询商品的方法
        private void FindGoodsByTM()
        {
            try
            {
                decimal tempcount = 0;
                if (!string.IsNullOrEmpty(textBox2.Text.Trim()))
                {
                    tempcount = decimal.Parse(textBox2.Text.Trim());

                }

                string temptxt = textBox1.Text.Trim();
                if (string.IsNullOrEmpty(temptxt))
                {
                    //MessageBox.Show("请输入需要查找的商品条码");
                    tipForm.Tiplabel.Text = "请输入需要查找的商品条码!";
                    tipForm.ShowDialog();
                    return;
                }
                using (hjnbhEntities db = new hjnbhEntities())
                {
                    var rules = db.hd_item_info.AsNoTracking().Where(t => t.tm.Contains(temptxt))
                            .Select(t => new {
                                noCode = t.item_id, 
                                BarCode = t.tm,
                                Goods = t.cname, 
                                unit = t.unit, 
                                spec = t.spec, 
                                retails = t.ls_price, 
                                pinyin = t.py ,
                                t.hpack_size,
                                t.jj_price,
                                t.pf_price,
                                
                            
                            })
                            .ToList();

                    //如果查出数据不至一条就弹出选择窗口，否则直接显示出来

                    if (rules.Count == 0)
                    {

                        this.textBox1.SelectAll();
                        tipForm.Tiplabel.Text = "没有查找到该商品!";
                        tipForm.ShowDialog();
                        return;
                    }
                    //查询到多条则弹出商品选择窗口，排除表格在正修改时发生判断
                    if (rules.Count > 1 && !dataGridView1.IsCurrentCellInEditMode)
                    {

                        if (rules.Count > 10)
                        {

                            if (DialogResult.No == MessageBox.Show("查询到多个类似的商品，数据量较大时可能造成几秒的卡顿，是否继续查询？", "提醒", MessageBoxButtons.YesNo))
                            {
                                return;
                            }
                        }

                        var form1 = new RRChooseGoodsForm();
                        ReplenishRequestForm.GetInstance.GoodsChooseList.Clear();

                        foreach (var item in rules)
                        {
                            #region 商品单位查询
                            //需要把单位编号转换为中文以便UI显示
                            int unitID = item.unit.HasValue ? (int)item.unit : 1;
                            string dw = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == unitID).Select(t => t.txt1).FirstOrDefault();
                            #endregion
                            ReplenishRequestForm.GetInstance.GoodsChooseList.Add(new RRGoodsModel
                            {
                                noCode = item.noCode,
                                barCodeTM = item.BarCode,
                                goods = item.Goods,
                                unit = dw,
                                spec = item.spec,
                                countNum = tempcount,
                                lsPrice = item.retails,
                                PinYin = item.pinyin,
                                JianShu = item.hpack_size,
                                jjprice = item.jj_price,
                                pfprice = item.pf_price.HasValue ? item.pf_price.Value : 0.00m
                            });

                        }


                        form1.dataGridView1.DataSource = ReplenishRequestForm.GetInstance.GoodsChooseList;

                        form1.ShowDialog();

                    }
                    //只查到一条如果没有重复的就直接上屏，除非表格正在修改数量
                    if (rules.Count == 1 && !dataGridView1.IsCurrentCellInEditMode)
                    {
                        RRGoodsModel newGoods_temp = new RRGoodsModel();
                        foreach (var item in rules)
                        {
                            #region 商品单位查询
                            //需要把单位编号转换为中文以便UI显示
                            int unitID = item.unit.HasValue ? (int)item.unit : 1;
                            string dw = db.mtc_t.AsNoTracking().Where(t => t.type == "DW" && t.id == unitID).Select(t => t.txt1).FirstOrDefault();
                            #endregion

                            newGoods_temp = new RRGoodsModel
                            {
                                noCode = item.noCode,
                                barCodeTM = item.BarCode,
                                goods = item.Goods,
                                countNum = tempcount,
                                unit = dw,
                                spec = item.spec,
                                lsPrice = item.retails,
                                PinYin = item.pinyin,
                                JianShu = item.hpack_size,
                                jjprice = item.jj_price,
                                pfprice = item.pf_price.HasValue?item.pf_price.Value:0.00m
                            };

                        }

                        if (ReplenishRequestForm.GetInstance.GoodsList.Count == 0)
                        {
                            ReplenishRequestForm.GetInstance.GoodsList.Add(newGoods_temp);
                        }
                        else
                        {

                            if (ReplenishRequestForm.GetInstance.GoodsList.Any(n => n.noCode == newGoods_temp.noCode))
                            {
                                var o = ReplenishRequestForm.GetInstance.GoodsList.Where(p => p.noCode == newGoods_temp.noCode);
                                foreach (var _item in o)
                                {
                                    _item.countNum += tempcount;
                                }
                                dataGridView1.Refresh();
                            }
                            else
                            {
                                ReplenishRequestForm.GetInstance.GoodsList.Add(newGoods_temp);
                            }

                        }
                        textBox1.Text = newGoods_temp.barCodeTM;
                    }
                }

                //每次查询后全选
                textBox1.SelectAll();
                //DeShowGoods();  //隐藏一些列
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("补货申请窗口查询商品时出现异常:", e);
                MessageBox.Show("数据库连接出错！");
                //string tip = ConnectionHelper.ToDo();
                //if (!string.IsNullOrEmpty(tip))
                //{
                //    MessageBox.Show(tip);
                //}
            }
        }


        #region 审核单据


        //处理审核逻辑(交给信息提示窗口处理)
        private void OnMakeTureFunc()
        {
            if (ReplenishRequestForm.GetInstance.isMK) return;  //如果已经审核过就不要才审核了
            if (ReplenishRequestForm.GetInstance.GoodsList.Count == 0)
            {
                tipForm.Tiplabel.Text = "请先填写需要审核的商品";
                tipForm.ShowDialog();
            }
            else
            {
                tipForm.code = 3;
                tipForm.Tiplabel.Text = "您是否确定审核此单？";
                tipForm.ESClabel.Text = "按ESC键返回，按Enter键确定……";
                tipForm.ShowDialog();
            }

        }


        #endregion

        #region 创建主单据
        //接收时间
        DateTime? time;
        //单据状态
        int status = 0;  //0为未发送，1为已发送

        //创建结算单据
        //补货申请单成单
        private BHInfoNoteModel BHNoteFunc()
        {
            var BhInfo = new BHInfoNoteModel();

            time = BhInfo.CTime = BhInfo.BHtime = System.DateTime.Now;  //制单时间
            BhInfo.ATime = System.DateTime.Now;
            //BhInfo.ATime = ReplenishRequestForm.GetInstance.MKtime;  //审核时间
            //BhInfo.OID = this.comboBox5.SelectedIndex;  //经办人ID,下拉框暂时不用
            BhInfo.OID = tempOID;
            BhInfo.OidStr = relusName;
            BhInfo.CID = HandoverModel.GetInstance.userID;  //制单人
            BhInfo.CidStr = HandoverModel.GetInstance.userName;  //制单人

            //if (HandoverModel.GetInstance.scode == 0)
            //{
            //    string PM = Microsoft.VisualBasic.Interaction.InputBox("没有查询到默认设置，请手动输入分店号或者先到系统设置中选择所在分店：", "分店号", "", -1, -1);
            //    //BhInfo.scode = string.IsNullOrEmpty(PM)?int.Parse(PM.Trim()):
            //    if (!string.IsNullOrEmpty(PM))
            //    {
            //        BhInfo.scode = int.Parse(PM.Trim());
            //    }
            //}
            //else
            //{
            //    BhInfo.scode = HandoverModel.GetInstance.scode; //仓库号

            //}

            BhInfo.scode = HandoverModel.GetInstance.scode; //仓库号

            switch (status)
            {
                case 0:
                    BhInfo.Bstatus = "未审"; //状态
                    break;
                case 1:
                    BhInfo.Bstatus = "已审"; //状态
                    break;
                case 2:
                    BhInfo.Bstatus = "反审"; //状态
                    break;
            }

            return BhInfo;
        }


        #endregion

        #region 向数据库上传补货单
        int isSubmited = 0;  //判断是否提交成功
        //单号计算方式，当前时间+00000+id
        long no_temp = Convert.ToInt64(System.DateTime.Now.ToString("yyyyMMdd") + "000000");

        private void UpdataDBFunc()
        {
            try
            {
                //if (isgo) return;
                using (var db = new hjnbhEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        #region 修改
                        if (ReplenishRequestForm.GetInstance.isUpdate)
                        {
                            string itemNO = ReplenishRequestForm.GetInstance.code_Temp;
                            var MXNote = db.hd_bh_detail.Where(t => t.b_no == itemNO).ToList();
                            if (MXNote != null)
                            {
                                db.hd_bh_detail.RemoveRange(MXNote);
                            }
                            string temp = ReplenishRequestForm.GetInstance.code_Temp;  //修改的单号
                            //主单
                            var BHnote = BHNoteFunc();
                            foreach (var item in ReplenishRequestForm.GetInstance.GoodsList)
                            {
                                var mainNote = db.hd_bh_info.Where(t => t.b_no == temp).FirstOrDefault();
                                if (mainNote != null)
                                {
                                    mainNote.cid = BHnote.CID; //制作人
                                    mainNote.bt_change_time = System.DateTime.Now; //修改时间
                                    mainNote.o_id = BHnote.OID;  //经办人
                                    mainNote.scode = BHnote.scode; //仓库id             
                                }

                                //明细
                                var addMx = new hd_bh_detail
                                {
                                    b_no = temp,
                                    item_id = item.noCode,
                                    tm = item.barCodeTM,
                                    cname = item.goods,
                                    spec = item.spec,
                                    unit = item.unit,
                                    amount = item.countNum,    //数量为输入值(如果不转换类型的话，这值总是0)
                                    ls_price = item.lsPrice,
                                    hpack_size = item.JianShu,
                                    jj_price = item.jjprice,
                                    pf_price = item.pfprice,
                                    scode = HandoverModel.GetInstance.scode

                                };
                                db.hd_bh_detail.Add(addMx);

                            }

                            isSubmited = db.SaveChanges();
                            scope.Complete();  //提交事务
                        }

                        #endregion
                        #region 新增

                        else
                        {
                            //主单
                            var BHnote = BHNoteFunc();
                            var HDBH = new hd_bh_info
                            {

                                cid = BHnote.CID,  //制作人
                                ctime = BHnote.CTime,
                                bh_time = BHnote.BHtime,   //补货时间
                                b_status = 0,  //状态
                                b_type = BHnote.BHtype, //单据类型
                                zd_time = BHnote.ZDtime,  //补货时间限制
                                bt_change_time = BHnote.changeTime, //修改时间
                                o_id = BHnote.OID,  //经办人
                                scode = BHnote.scode, //仓库id
                                //a_id = BHnote.AID,  //审核人
                                //a_time = BHnote.ATime,//审核时间
                                del_flag = (byte?)BHnote.delFlag,
                                sh_flag = 0  //0为保存，1为审核，2为反审
                            };

                            db.hd_bh_info.Add(HDBH);
                            db.SaveChanges(); //保存一次才能生效
                            string noteNO = label5.Text = "BHS" + (no_temp + HDBH.id -1).ToString();  //获取ID并生成补货单号
                            HDBH.b_no = noteNO;
                            //明细清单
                            foreach (var item in ReplenishRequestForm.GetInstance.GoodsList)
                            {
                                //部分字段没有赋值
                                var BHMX = new hd_bh_detail
                                {
                                    b_no = noteNO,
                                    item_id = item.noCode,
                                    tm = item.barCodeTM,
                                    cname = item.goods,
                                    spec = item.spec,
                                    unit = item.unit,
                                    amount = item.countNum,    //数量为输入值(如果不转换类型的话，这值总是0)
                                    ls_price = item.lsPrice,
                                    hpack_size = item.JianShu,
                                    jj_price = item.jjprice,
                                    pf_price = item.pfprice,
                                    scode = HandoverModel.GetInstance.scode
                                };

                                db.hd_bh_detail.Add(BHMX);
                            }

                            BHnote.Bno = noteNO;
                            //BHnote.Bstatus = "已发送";
                            isSubmited = db.SaveChanges();

                            scope.Complete();  //提交事务

                            ReplenishRequestForm.GetInstance.BHmainNoteList.Add(BHnote); //发送后的单据放入表单中
                        }
                        #endregion

                        label4.Text = "未审";   //状态UI

                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("补货申请窗口上传补货单时出现异常:", e);
                MessageBox.Show("数据库连接出错！");
                //string tip = ConnectionHelper.ToDo();
                //if (!string.IsNullOrEmpty(tip))
                //{
                //    MessageBox.Show(tip);
                //}
            }
        }

        #endregion


        #region 上下快捷键选择
        //小键盘向上
        private void UpFun()
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

        //小键盘向下
        private void DownFun()
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

        //删除单行
        private bool Dele()
        {
            //如果当前只有一行就直接清空
            if (dataGridView1.Rows.Count == 1)
            {
                int DELindex1_temp = dataGridView1.SelectedRows[0].Index;
                dataGridView1.Rows.RemoveAt(DELindex1_temp);
                return true;
            }
            //当前行数大于1行时删除选中行后把往上一行设置为选中状态
            if (dataGridView1.Rows.Count > 1)
            {
                int DELindex_temp = dataGridView1.SelectedRows[0].Index;
                dataGridView1.Rows.RemoveAt(DELindex_temp);
                try
                {
                    string de_temp = dataGridView1.CurrentRow.Cells[2].Value.ToString();

                    if (DELindex_temp - 1 >= 0)
                    {
                        dataGridView1.Rows[DELindex_temp - 1].Selected = true;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("补货新单界面删除选中行发生异常:", ex);
                }

            }
            return false;
        }


        #endregion



        #region 自动在数据表格首列绘制序号
        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SetDataGridViewRowXh(e, dataGridView1);
        }

        //在首列绘制序号，如果首列原有内容，会出现重叠，所以首列手动添加一个空列
        private void SetDataGridViewRowXh(DataGridViewRowPostPaintEventArgs e, DataGridView dataGridView)
        {
            SolidBrush solidBrush = new SolidBrush(Color.Black); //更改序号样式
            int xh = e.RowIndex + 1;
            e.Graphics.DrawString(xh.ToString(CultureInfo.CurrentUICulture), e.InheritedRowStyle.Font, solidBrush, e.RowBounds.Location.X + 5, e.RowBounds.Location.Y + 4);
        }
        #endregion


        //数量输入框回车事件
        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            //switch (e.KeyCode)
            //{
            //    case Keys.Enter:
            //        if (!string.IsNullOrEmpty(this.textBox2.Text.Trim()))
            //        {
            //            FindGoodsByTM();
            //        }

            //        this.textBox1.Focus();
            //        this.textBox1.SelectAll();
            //        break;
            //}
        }

        //条码输入框回车事件
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            //switch (e.KeyCode)
            //{
            //    case Keys.Enter:
            //        if (!string.IsNullOrEmpty(this.textBox1.Text.Trim()))
            //        {
            //            this.textBox2.Focus();
            //            this.textBox2.SelectAll();
            //        }
            //        break;
            //}
        }

        //限制数量输入框只能输入数字
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (e.KeyChar == 0x20) e.KeyChar = (char)0;  //禁止空格键  
            if ((e.KeyChar == 0x2D) && (((TextBox)sender).Text.Length == 0)) return;   //处理负数  
            if ((e.KeyChar < '0' && e.KeyChar != '.' || e.KeyChar > '9' && e.KeyChar != '.' || ((TextBox)(sender)).Text.IndexOf('.') >= 0 && e.KeyChar == '.') && e.KeyChar != (char)13 && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }

        //F4按钮点击
        private void button1_Click(object sender, EventArgs e)
        {
            //OnMakeTureFunc();
        }

        //判断有列表有内容时才允许审核
        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            if (dataGridView1.RowCount > 0)
            {
                if (button1.Enabled == false)
                {
                    button1.Enabled = true;

                }
            }
            else
            {
                if (button1.Enabled == true)
                {
                    button1.Enabled = false;

                }
            }

            ShowUIFunc(); //统计单数
        }
        //数据删除事件
        private void dataGridView1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            ShowUIFunc(); //统计单数
        }
        //F5关闭窗口，好像没什么用，我隐藏先
        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        //F6上传按钮
        private void button2_Click(object sender, EventArgs e)
        {
            OnUploadFunc();
        }
        //当商品条码输入框获取焦点时提示相关信息
        private void textBox1_Enter(object sender, EventArgs e)
        {
            Tipslabel.Text = "请输入需要查询的商品条码";
        }
        //当数量输入框获取焦点时提示相关信息
        private void textBox2_Enter(object sender, EventArgs e)
        {
            Tipslabel.Text = "请输入商品数量";
        }
        //删除按钮
        private void button4_Click(object sender, EventArgs e)
        {
            Dele();
        }
        //禁止列表获取焦点，防止快捷键冲突
        private void dataGridView1_Enter(object sender, EventArgs e)
        {
            textBox1.Focus();
        }

        private void comboBox5_Enter(object sender, EventArgs e)
        {
            Tipslabel.Text = "请按上下方向键选择经办人并按Enter键确认";
        }



        #region 查询经办人

        //输入工号录入经办人签名
        private void GetOidFunc()
        {
            try
            {
                string jbrStr = Microsoft.VisualBasic.Interaction.InputBox("请输入经办人工号：", "经办人审核", "", -1, -1);
                if (!string.IsNullOrEmpty(jbrStr))
                {
                    int.TryParse(jbrStr.Trim(), out tempOID);
                }

                if (tempOID == -1) return;
                using (var db = new hjnbhEntities())
                {
                    //查用户视图取名字
                    relusName = db.user_role_view.AsNoTracking().Where(t => t.role_id == 4 && t.usr_id == tempOID).Select(t => t.usr_name).FirstOrDefault();
                    label29.Text = relusName;
                }
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("补货申请窗口查询经办人时出现异常:", e);
                MessageBox.Show("补货申请窗口查询经办人时出现异常！请联系管理员！");
                //string tip = ConnectionHelper.ToDo();
                //if (!string.IsNullOrEmpty(tip))
                //{
                //    MessageBox.Show(tip);
                //}
            }
        }

        //开始查询经办人信息
        //private void textBox3_KeyDown(object sender, KeyEventArgs e)
        //{
        //    switch (e.KeyCode)
        //    {
        //        case Keys.Enter:
        //            GetOidFunc();
        //            label29.Text = relusName;

        //            break;
        //    }
        //}

        #endregion

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            UpdateNameFunc();
        }
        //调整表格1的列宽、同时隐藏不需要显示的列、禁止编辑、修改列名
        private void UpdateNameFunc()
        {
            try
            {
                //列名
                dataGridView1.Columns[1].HeaderText = "序";
                dataGridView1.Columns[2].HeaderText = "条码";
                dataGridView1.Columns[3].HeaderText = "品名";
                dataGridView1.Columns[4].HeaderText = "规格";
                dataGridView1.Columns[5].HeaderText = "单位";
                dataGridView1.Columns[6].HeaderText = "数量";
                dataGridView1.Columns[8].HeaderText = "零售价";
                dataGridView1.Columns[9].HeaderText = "拼音";
                dataGridView1.Columns[10].HeaderText = "现存";


                dataGridView1.Columns[0].Visible = false;
                dataGridView1.Columns[7].Visible = false;

                dataGridView1.Columns[9].Visible = false;
                dataGridView1.Columns[10].Visible = false;  //现存
                dataGridView1.Columns[11].Visible = false;
                dataGridView1.Columns[12].Visible = false;
                dataGridView1.Columns[13].Visible = false;


            }
            catch
            {
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            GetOidFunc();
        }

        //退出窗口时把状态、单号清空
        private void RequsetNoteForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ReplenishRequestForm.GetInstance.isUpdate = false;
            ReplenishRequestForm.GetInstance.Sta_Temp = string.Empty;
            ReplenishRequestForm.GetInstance.code_Temp = string.Empty;
            label4.Text = "未保存";
            label5.Text = "未保存";
        }

        private void button6_Click(object sender, EventArgs e)
        {

            EnterFunc();

        }

        /// <summary>
        /// 回车
        /// </summary>
        private void EnterFunc()
        {
            if (!string.IsNullOrEmpty(this.textBox1.Text.Trim()) && !string.IsNullOrEmpty(this.textBox2.Text.Trim()))
            {
                FindGoodsByTM();
            }
            else if (string.IsNullOrEmpty(this.textBox2.Text.Trim()))
            {
                MessageBox.Show("请输入申请数量！");
                this.textBox2.Focus();

            }
            else if (string.IsNullOrEmpty(this.textBox1.Text.Trim()))
            {
                MessageBox.Show("请输入商品条码！");
                this.textBox1.Focus();
            }

        }




    }
}
