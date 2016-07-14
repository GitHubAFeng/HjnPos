using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hjn20160520._8_ReplenishRequest
{
    /// <summary>
    /// 补货清单 商品选择窗口
    /// </summary>
    public partial class RRChooseGoodsForm : Form
    {
        //制单窗口
        RequsetNoteForm RNForm;
        public RRChooseGoodsForm()
        {
            InitializeComponent();
        }

        private void RRChooseGoodsForm_Load(object sender, EventArgs e)
        {
            RNForm = new RequsetNoteForm();
        }

        //快捷键
        private void RRChooseGoodsForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    OnEnterClick();
                    break;
                case Keys.Escape:
                    this.Close();
                    break;


            }
        }


        //按ENTER选择商品
        private void OnEnterClick()
        {
            if (dataGridView1.SelectedRows[0] != null)
            {


                int temp_index = dataGridView1.CurrentRow.Index;

                ReplenishRequestForm.GetInstance.UserChooseGoods(temp_index);
                //每次选择完都要清空该列表，防止商品重复出现
                ReplenishRequestForm.GetInstance.GoodsChooseList.Clear();
                this.Close();//关闭窗体
            }
            else
            {
                MessageBox.Show("没有选中任何商品");

            }

            RNForm.textBox1.Text = "";


        }

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
                dataGridView1.Columns[2].HeaderText = "条码";
                dataGridView1.Columns[3].HeaderText = "品名";
                dataGridView1.Columns[4].HeaderText = "规格";
                dataGridView1.Columns[5].HeaderText = "单位";
                dataGridView1.Columns[8].HeaderText = "零售价";
                dataGridView1.Columns[9].HeaderText = "拼音";
                dataGridView1.Columns[10].HeaderText = "现存";


                //隐藏      
                 dataGridView1.Columns[0].Visible = false;
                 dataGridView1.Columns[1].Visible = false;
                 dataGridView1.Columns[6].Visible = false;
                 dataGridView1.Columns[7].Visible = false;
                 dataGridView1.Columns[10].Visible = false;
                 dataGridView1.Columns[11].Visible = false;

            }
            catch
            {
            }
        }
















    }
}
