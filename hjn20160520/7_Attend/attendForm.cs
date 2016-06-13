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
            //this.FormBorderStyle = FormBorderStyle.None;
            this.timer1.Enabled = true;
        }


        //热键
        private void attendForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
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


        //根据员工ID查询员工信息
        private bool ShowUserByID()
        {
            bool isOK = false;
            string temp_text = textBox1.Text.Trim();
            using (var db = new hjnbhEntities())
            {
                var userInfos = db.users.Where(t => t.login_id == temp_text);


            }

            return isOK;
        }











    }
}
