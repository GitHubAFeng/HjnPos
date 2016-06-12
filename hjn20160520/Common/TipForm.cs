using hjn20160520._2_Cashiers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hjn20160520.Common
{
    public partial class TipForm : Form
    {

       

        //功能识别码，默认值0为关闭窗口，1为退货功能
        public int code = 0;

        public TipForm()
        {
            InitializeComponent();
        }

        private void TipForm_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.timer1.Start();

            
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        //热键
        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
        {
            int WM_KEYDOWN = 256;
            int WM_SYSKEYDOWN = 260;
            if (msg.Msg == WM_KEYDOWN | msg.Msg == WM_SYSKEYDOWN)
            {
                switch (keyData)
                {

                    //回车
                    case Keys.Enter:
                        OnEnterClick();
                        
                        break;
                    //退出
                    case Keys.Escape:

                        this.Close();
                        break;
                        //单品退货
                    case  Keys.Shift:

                        break;

                }

            }
            return false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.label1.Visible = !this.label1.Visible;
        }



        //处理回车键逻辑
        private void OnEnterClick(int code = 0)
        {
            code = this.code;
            switch (code)
            {
                case 0:
                    this.Close();
                    break;
                case 1:
                    Cashiers.GetInstance.ShowRDForm();
                    this.Close();
                    break;
            }
        }

        //处理Shift键逻辑
        private void OnShiftClick(int code)
        {
            code = this.code;
            switch (code)
            {
                case 1:

                    break;
            }
        }







    }
}
