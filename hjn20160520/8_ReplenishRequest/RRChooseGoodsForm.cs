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
















    }
}
