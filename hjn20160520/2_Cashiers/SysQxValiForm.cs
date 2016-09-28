using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace hjn20160520._2_Cashiers
{
    /// <summary>
    /// 权限验证
    /// </summary>
    public partial class SysQxValiForm : Form
    {

        public delegate void SysQxValiFormHandle(bool isValied);
        public event SysQxValiFormHandle Changed;  //通知是否验证通过

        public SysQxValiForm()
        {
            InitializeComponent();
        }

        private void SysQxValiForm_Load(object sender, EventArgs e)
        {
            this.textBox3.Text = "";
            this.textBox2.Focus();
            this.textBox2.SelectAll();
        }

        private void SysQxValiForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.Close();
                    break;
                case Keys.Enter:
                    ValiFunc();
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ValiFunc();
        }



        private void ValiFunc()
        {
            try
            {
                int qxid_temp = 0; //权限工号
                int qxzm_temp = 0;  //权限密码

                if (!string.IsNullOrEmpty(textBox2.Text.Trim()))
                {
                    int.TryParse(textBox2.Text.Trim(), out qxid_temp);
                }

                if (!string.IsNullOrEmpty(textBox3.Text.Trim()))
                {
                    int.TryParse(textBox3.Text.Trim(), out qxzm_temp);
                }


                //检证用户权限    
                using (var db = new hjnbhEntities())
                {
                    var res = db.hd_sys_qx.AsNoTracking().Where(t => (t.usr_id == qxid_temp) && (t.zm == qxzm_temp)).FirstOrDefault();

                    if (res != null)
                    {
                        Changed(true);
                        this.Close();
                    }
                    else
                    {
                        Changed(false);

                        MessageBox.Show("验证失败！权限工号或者密码错误！");
                    }

                }
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("权限验证窗口进行验证时出现异常:", e);
                MessageBox.Show("权限验证出现异常！请检查输入是否正常，必要时候请联系管理员！");

            }
        }









    }
}
