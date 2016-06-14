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

namespace hjn20160520._3_DutyWork
{
    public partial class DutyWorkForm : Form
    {


        public DutyWorkForm()
        {
            InitializeComponent();
        }

        private void DutyWorkForm_Load(object sender, EventArgs e)
        {
            label4.Text = System.DateTime.Now.ToString();
            textBox1.Focus();
            textBox1.SelectAll();
        }

        //快捷键
        private void DutyWorkForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    WorkFunc();
                    break;
                case Keys.Escape:
                    this.Close();
                    break;
            }


        }

        //处理员工当班的逻辑
        private void WorkFunc()
        {
            string txt_temp = textBox1.Text.Trim();  //钱箱余额
            HandoverModel.GetInstance.SaveMoney = float.Parse(txt_temp);
            HandoverModel.GetInstance.isWorking = true;  //当班

        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            //限制只能输入数字
            const char Delete = (char)8;
            if (!(e.KeyChar >= '0' && e.KeyChar <= '9') && e.KeyChar != Delete)
            {
                e.Handled = true;
            }

        }




    }
}
