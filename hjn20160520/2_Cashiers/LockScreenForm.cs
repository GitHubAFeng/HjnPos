using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hjn20160520._2_Cashiers
{
    public partial class LockScreenForm : Form
    {
        //收银窗口
        Cashiers CForm;

        public LockScreenForm()
        {
            InitializeComponent();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void LockScreenForm_Load(object sender, EventArgs e)
        {
            CForm = new Cashiers();
        }

        private void LockScreenForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    CForm.Show();
                    this.Close();
                    break;
            }
        }
    }
}
