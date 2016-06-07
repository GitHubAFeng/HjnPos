using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hjn20160520._7_Attend
{
    public partial class attendForm : Form
    {

        //主菜单窗口
        MainForm mainForm;


        public attendForm()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label19.Text = System.DateTime.Now.ToString();
        }

        private void attendForm_Load(object sender, EventArgs e)
        {
            mainForm = new MainForm();
            this.FormBorderStyle = FormBorderStyle.None;
            this.timer1.Enabled = true;
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


                        break;
                        //退出
                    case Keys.Escape:

                        mainForm.Show();
                        this.Close();

                        break;

                }

            }
            return false;
        }















    }
}
