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
    /// <summary>
    /// 当班
    /// </summary>
    public partial class DutyWorkForm : Form
    {
        //MainFormXP main;
        public delegate void DutyWorkFormHandle();
        public event DutyWorkFormHandle UIChanged;  //UI更新事件

        public DutyWorkForm()
        {
            InitializeComponent();
        }

        private void DutyWorkForm_Load(object sender, EventArgs e)
        {
            //main = this.Owner as MainFormXP;


            //这两段是防止出错的
            label4.Text = System.DateTime.Now.ToString();
            HandoverModel.GetInstance.workTime = System.DateTime.Now;

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
                    this.Close();
                    break;
                case Keys.Escape:
                    this.Close();
                    break;
            }


        }


        DateTime worktime = new DateTime();

        //处理员工当班的逻辑
        private void WorkFunc()
        {
            if (worktime == DateTime.MinValue)
            {
                worktime = System.DateTime.Now;
            }

            string txt_temp = textBox1.Text.Trim();  //钱箱余额
            HandoverModel.GetInstance.SaveMoney = Convert.ToDecimal(txt_temp);
            HandoverModel.GetInstance.isWorking = true;  //当班
            HandoverModel.GetInstance.workTime = worktime;
            timer1.Enabled = false;
            //main.label11.Text = "正在当班中…";
            UIChanged();
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            worktime = System.DateTime.Now;
            label4.Text = worktime.ToString();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            WorkFunc();

        }




    }
}
