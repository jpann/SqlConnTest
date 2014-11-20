using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using log4net;
using log4net.Config;

namespace SqlConnTest
{
    public partial class Main : Form
    {
        private OleDbConnection mConn = null;
        private string mQueryText = "";
        private int mQueryInterval = 15; // ms
        private string mConnString = "";

        private static ILog mLog = LogManager.GetLogger(typeof(Program));

        #region Logging Methods
        public void LogMessage(string Message)
        {
            ListViewItem oItem = new ListViewItem(DateTime.Now.ToString());
            oItem.SubItems.Add(Message);
            lstLog.Items.Add(oItem);

            mLog.Info(Message);
        }

        public void LogMessage(string Message, Exception er)
        {
            ListViewItem oItem = new ListViewItem(DateTime.Now.ToString());
            oItem.SubItems.Add(Message);
            oItem.SubItems.Add(string.Format("{0}: {1}", er.GetType().ToString(), er.Message));
            oItem.BackColor = Color.Red;
            lstLog.Items.Add(oItem);

            mLog.Error(Message, er);
        }

        public void LogConfig()
        {
            mLog.InfoFormat("ConnectionString: {0}", mConnString);
            mLog.InfoFormat("QueryText: {0}", mQueryText);
            mLog.InfoFormat("QueryInterval: {0} seconds", mQueryInterval / 1000);
        }
        #endregion

        #region Sql Methods
        public void Connect(string ConnectionString)
        {
            try
            {
                LogMessage("Connecting...");
                //if (mConn == null)
                mConn = new OleDbConnection(ConnectionString);
                mConn.Open();
                LogMessage(string.Format("Connection to {0} open!", mConn.DataSource));
            }
            catch (Exception er)
            {
                string sMsg = string.Format("Failed to connect: {0}. Closing application.", er.Message);
                LogMessage(sMsg, er);
            }
        }
        #endregion

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            mLog.Info(string.Format("{0} loaded.", Assembly.GetExecutingAssembly().FullName));

            InitializeComponent();

            XmlConfigurator.Configure();

            mQueryText = Properties.Settings.Default.QueryText;
            mQueryInterval = Properties.Settings.Default.QueryInterval * 1000;
            mConnString = Properties.Settings.Default.ConnectionString;

            LogConfig();

            Connect(mConnString);

            queryTimer.Interval = mQueryInterval;
            queryTimer.Enabled = true;
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            queryTimer.Enabled = false;
            Application.Exit();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.ApplicationExitCall)
            {
                LogMessage("Minimizing application...");

                e.Cancel = true;
                this.Hide();

                notifyIcon.ShowBalloonTip(100, "SQL Tester Minimized", "The Sql tester has been minimized. Double click this icon to restore it.", ToolTipIcon.Info);
            }
            else if (e.CloseReason == CloseReason.ApplicationExitCall
                || e.CloseReason == CloseReason.WindowsShutDown
                || e.CloseReason == CloseReason.TaskManagerClosing)
            {
                LogMessage("Closing application...");

                if (mConn != null)
                {
                    if (mConn.State != ConnectionState.Closed || mConn.State != ConnectionState.Broken)
                    {
                        mConn.Close();
                        mConn.Dispose();
                    }
                }
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;

            this.Activate();

            this.TopLevel = true;
            this.Show();
        }

        private void queryTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                using (OleDbCommand oCmd = new OleDbCommand(mQueryText, mConn))
                {
                    LogMessage(string.Format("Executing timed query: {0}...", mQueryText));

                    oCmd.ExecuteNonQuery();

                    LogMessage("Timed query executed successfully!");
                }
            }
            catch (Exception er)
            {
                string sMsg = string.Format("Failed to execute timed query: {0}", er.Message);
                LogMessage(sMsg, er);

                Connect(mConnString);
            }
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogMessage((e.ExceptionObject as Exception).Message, (e.ExceptionObject as Exception));
            MessageBox.Show((e.ExceptionObject as Exception).Message, "Unhandled Exception");
        }
    }
}
