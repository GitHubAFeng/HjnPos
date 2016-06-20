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
    public partial class RequsetNoteForm : Form
    {
        public RequsetNoteForm()
        {
            InitializeComponent();
        }

        private void RequsetNoteForm_Load(object sender, EventArgs e)
        {

            textBox1.Focus();
        }


        //重写热键方法，实现ESC退出，Enter选择
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
                    //按回车
                    case Keys.Enter:

                        break;

                }

            }
            return false;
        }














    }
}
