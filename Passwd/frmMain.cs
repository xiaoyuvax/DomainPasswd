using Passwd;
using Passwd.SystemDirectoryServicesImpl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DomainPasswd
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtNewPwd.Text == txtConfirmPwd.Text)
                {
                    using (SecureString oldPwd = new SecureString(), newPwd = new SecureString())
                    {
                        txtOldPwd.Text.ToCharArray().ToList().ForEach(i => oldPwd.AppendChar(i));
                        txtNewPwd.Text.ToCharArray().ToList().ForEach(i => newPwd.AppendChar(i));

                        if (Program.ExecuteFromCommandLine(new string[] { (chkSSL.Checked ? " -s" : ""), "-u " + txtDomainUser.Text }, false, oldPwd, newPwd) == 0)
                            MessageBox.Show("Password changed successfully!", "Done!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        else
                            MessageBox.Show("Changing password failed!\r\r" + Program.Log, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else throw new Exception("New password not match!");


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
