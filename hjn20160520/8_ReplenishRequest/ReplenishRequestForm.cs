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
    public partial class ReplenishRequestForm : Form
    {

        //主菜单
        MainForm mainForm;

        public ReplenishRequestForm()
        {
            InitializeComponent();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void ReplenishRequestForm_Load(object sender, EventArgs e)
        {
            mainForm = new MainForm();
            this.FormBorderStyle = FormBorderStyle.None;

        }


        #region 热键注册

        //重写热键方法
        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
        {
            int WM_KEYDOWN = 256;
            int WM_SYSKEYDOWN = 260;
            if (msg.Msg == WM_KEYDOWN | msg.Msg == WM_SYSKEYDOWN)
            {
                switch (keyData)
                {
                    //删除DEL键
                    case Keys.Escape:

                        mainForm.Show();
                        this.Close();

                        break;

                    //回车
                    case Keys.Enter:


                        break;

                }

            }
            return false;
        }

        #endregion








    }
}
