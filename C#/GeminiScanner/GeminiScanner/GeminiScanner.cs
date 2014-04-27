using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GeminiScanner
{
    public partial class GeminiScanner : Form
    {
        private ClipboardParser cbParser = new ClipboardParser();

        public GeminiScanner()
        {
            InitializeComponent();
            ClipboardNotification.ClipboardUpdate += new EventHandler(ClipboardChanged);
            cbParser.ParsedMessage += MessageParsed;
        }

        private void ClipboardChanged(object sender, EventArgs e)
        {
            IDataObject iData = Clipboard.GetDataObject();
            if (iData.GetDataPresent(DataFormats.Text))
            {
                cbParser.parseMessage((String)iData.GetData(DataFormats.Text));
            }
        }

        private void MessageParsed(object sender, ParsedMessageArgs e)
        {
            string fmtString = "{0}: {1} {2}";
            textClip.Text += String.Format(fmtString, e.action, e.path, e.message) + Environment.NewLine;
        }

        private void GeminiScanner_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(1);
                this.ShowInTaskbar = false;
            }
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            notifyIcon.Visible = false;
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            cbParser.Dispose();
            //notifyIcon.Visible = false;
            Application.Exit();
        }

        private void GeminiScanner_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.WindowState = FormWindowState.Minimized;
                notifyIcon.Visible = true;
                this.ShowInTaskbar = false;
                e.Cancel = true;
            }
        }

        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textClip.Text = "";
        }

        private void closeAllFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cbParser.CloseFiles();
        }
    }
}
