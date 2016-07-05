using hjn20160520._2_Cashiers;
using hjn20160520._8_ReplenishRequest;
using hjn20160520._9_VIPCard;
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

        //默认值0为ESC与Enter关闭窗口 
        //功能识别码，1为退货功能,2 会员办理按ESC键清空内容并退出，按回车键继续办理,3 审核功能 esc退出 enter确认审核
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

                        OnEscClick();
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



        //处理回车键逻辑，复用信息提示窗口，实现不同的提示功能扩展
        private void OnEnterClick(int code = 0)
        {
            code = this.code;
            switch (code)
            {
                    //0为正常关闭提示窗口
                case 0:
                    this.Close();
                    break;
                    //1为处理退货功能
                case 1:
                    //Cashiers.GetInstance.ShowRDForm();
                    this.Close();
                    break;
                //    2为处理会员办理
                case 2:
                    this.Close();
                    break;
                    //3为审核功能
                case 3:
                    ReplenishRequestForm.GetInstance.isMK = true;
                    ReplenishRequestForm.GetInstance.MKtime = System.DateTime.Now;
                    ReplenishRequestForm.GetInstance.FreezeRNForm();  //审核通过并冻结
                    ReplenishRequestForm.GetInstance.MKDate();  //审核日
                    this.Close();
                    break;
            }
        }


        //处理ESC逻辑，复用信息提示窗口，实现不同的提示功能扩展
        private void OnEscClick(int code = 0)
        {
            code = this.code;
            switch (code)
            {
                //0为正常关闭提示窗口
                case 0:
                    this.Close();
                    break;
                //1为处理退货功能
                case 1:
                    //Cashiers.GetInstance.ShowRDForm();
                    this.Close();
                    break;
                //    2为处理会员办理
                case 2:

                    VIPCardForm.GetInstance.ClearTextBoxOnESC();
                    VIPCardForm.GetInstance.Close();
                    this.Close();
                    break;
                //3为审核功能
                case 3:

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
                //1为处理退货功能
                case 1:

                    break;
            }
        }







    }
}
