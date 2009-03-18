using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using LumiSoft.Net.SIP.Stack;

namespace LumiSoft.Net.SIP.Debug
{
    /// <summary>
    /// SIP debug UI.
    /// </summary>
    public class wfrm_SIP_Debug : Form
    {
        private TabControl  m_pTab                     = null;
        private ToolStrip   m_pTabLog_Toolbar          = null;
        private RichTextBox m_pTabLog_Text             = null;
        private ToolStrip   m_pTabTransactions_Toolbar = null;
        private ListView    m_pTabTransactions_List    = null;
        private ToolStrip   m_pTabDialogs_Toolbar      = null;
        private ListView    m_pTabDialogs_List         = null;
        private ToolStrip   m_pTabFlows_Toolbar        = null;
        private ListView    m_pTabFlows_List           = null;

        private SIP_Stack m_pStack      = null;
        private bool      m_OddLogEntry = false;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="stack">SIP stack.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stack</b> is null reference.</exception>
        public wfrm_SIP_Debug(SIP_Stack stack)
        {
            if(stack == null){
                throw new ArgumentNullException("stack");
            }

            m_pStack = stack;
            m_pStack.Logger.WriteLog += new EventHandler<LumiSoft.Net.Log.WriteLogEventArgs>(Logger_WriteLog);

            InitUI();
        }

        #region mehtod InitUI

        /// <summary>
        /// Creates and initializes UI.
        /// </summary>
        private void InitUI()
        {
            this.ClientSize = new Size(600,300);
            this.Text = "SIP Debug";
            this.FormClosed += new FormClosedEventHandler(wfrm_Debug_FormClosed);
                        
            m_pTab = new TabControl();
            m_pTab.Dock = DockStyle.Fill;

            #region tabpage Log

            m_pTab.TabPages.Add("log","Log");

            m_pTabLog_Toolbar = new ToolStrip();
            m_pTabLog_Toolbar.Dock = DockStyle.Top;
            m_pTab.TabPages["log"].Controls.Add(m_pTabLog_Toolbar);
            // Log button
            ToolStripButton tabLog_Toolbar_Log = new ToolStripButton("Log");
            tabLog_Toolbar_Log.Name = "log";
            tabLog_Toolbar_Log.Tag = "log";
            tabLog_Toolbar_Log.Checked = true;
            tabLog_Toolbar_Log.Click += new EventHandler(delegate(object sender,EventArgs e){
                tabLog_Toolbar_Log.Checked = !tabLog_Toolbar_Log.Checked;
            });
            m_pTabLog_Toolbar.Items.Add(tabLog_Toolbar_Log);
            // Log Data button
            ToolStripButton tabLog_Toolbar_LogData = new ToolStripButton("Log Data");
            tabLog_Toolbar_LogData.Name = "logdata";
            tabLog_Toolbar_LogData.Tag = "logdata";
            tabLog_Toolbar_LogData.Checked = true;
            tabLog_Toolbar_LogData.Click += new EventHandler(delegate(object sender,EventArgs e){
                tabLog_Toolbar_LogData.Checked = !tabLog_Toolbar_LogData.Checked;
            });
            m_pTabLog_Toolbar.Items.Add(tabLog_Toolbar_LogData);
            // Clear button
            ToolStripButton tabLog_Toolbar_Clear = new ToolStripButton("Clear");
            tabLog_Toolbar_Clear.Tag = "clear";
            tabLog_Toolbar_Clear.Click += new EventHandler(m_pTabLog_Toolbar_Click);
            m_pTabLog_Toolbar.Items.Add(tabLog_Toolbar_Clear);
            // Filter
            m_pTabLog_Toolbar.Items.Add(new ToolStripLabel("Filter:")); 
            ToolStripTextBox tabLog_Toolbar_Filter = new ToolStripTextBox();
            tabLog_Toolbar_Filter.Name = "filter";
            tabLog_Toolbar_Filter.AutoSize = false;
            tabLog_Toolbar_Filter.Size = new Size(150,20);
            m_pTabLog_Toolbar.Items.Add(tabLog_Toolbar_Filter);
            
            m_pTabLog_Text = new RichTextBox();
            m_pTabLog_Text.Size = new Size(m_pTab.TabPages["log"].Width,m_pTab.TabPages["log"].Height - 25);
            m_pTabLog_Text.Location = new Point(0,25);
            m_pTabLog_Text.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            m_pTabLog_Text.BorderStyle = BorderStyle.None;
            m_pTab.TabPages["log"].Controls.Add(m_pTabLog_Text);

            #endregion

            #region tabpage Transaction

            m_pTab.TabPages.Add("transactions","Transactions");

            m_pTabTransactions_Toolbar = new ToolStrip();
            m_pTabTransactions_Toolbar.Dock = DockStyle.Top;
            ToolStripButton tabTransactions_Toolbar_Refresh = new ToolStripButton("Refresh");
            tabTransactions_Toolbar_Refresh.Tag = "refresh";
            tabTransactions_Toolbar_Refresh.Click += new EventHandler(m_pTabTransactions_Toolbar_Click);
            m_pTabTransactions_Toolbar.Items.Add(tabTransactions_Toolbar_Refresh);
            m_pTab.TabPages["transactions"].Controls.Add(m_pTabTransactions_Toolbar);

            m_pTabTransactions_List = new ListView();
            m_pTabTransactions_List.Size = new Size(m_pTab.TabPages["transactions"].Width,m_pTab.TabPages["transactions"].Height - 25);
            m_pTabTransactions_List.Location = new Point(0,25);
            m_pTabTransactions_List.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            m_pTabTransactions_List.View = View.Details;
            m_pTabTransactions_List.Columns.Add("Is Server");
            m_pTabTransactions_List.Columns.Add("Method",80);
            m_pTabTransactions_List.Columns.Add("State",80);
            m_pTabTransactions_List.Columns.Add("Create Time",80);
            m_pTabTransactions_List.Columns.Add("ID",100);
            m_pTab.TabPages["transactions"].Controls.Add(m_pTabTransactions_List);

            #endregion

            #region tabpage Dialogs

            m_pTab.TabPages.Add("dialogs","Dialogs");

            m_pTabDialogs_Toolbar = new ToolStrip();
            m_pTabDialogs_Toolbar.Dock = DockStyle.Top;
            ToolStripButton tabDialogs_Toolbar_Refresh = new ToolStripButton("Refresh");
            tabDialogs_Toolbar_Refresh.Tag = "refresh";
            tabDialogs_Toolbar_Refresh.Click += new EventHandler(m_pTabDialogs_Toolbar_Click);
            m_pTabDialogs_Toolbar.Items.Add(tabDialogs_Toolbar_Refresh);
            m_pTab.TabPages["dialogs"].Controls.Add(m_pTabDialogs_Toolbar);

            m_pTabDialogs_List = new ListView();
            m_pTabDialogs_List.Size = new Size(m_pTab.TabPages["dialogs"].Width,m_pTab.TabPages["dialogs"].Height - 25);
            m_pTabDialogs_List.Location = new Point(0,25);
            m_pTabDialogs_List.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            m_pTabDialogs_List.View = View.Details;
            m_pTabDialogs_List.Columns.Add("ID",120);
            m_pTabDialogs_List.Columns.Add("Type",100);
            m_pTabDialogs_List.Columns.Add("State",80);
            m_pTabDialogs_List.Columns.Add("Create Time",80);
            m_pTab.TabPages["dialogs"].Controls.Add(m_pTabDialogs_List);

            #endregion

            #region tabpage Flows

            m_pTab.TabPages.Add("flows","Flows");

            m_pTabFlows_Toolbar = new ToolStrip();
            m_pTabFlows_Toolbar.Dock = DockStyle.Top;
            ToolStripButton tabFlows_Toolbar_Refresh = new ToolStripButton("Refresh");
            tabFlows_Toolbar_Refresh.Tag = "refresh";
            tabFlows_Toolbar_Refresh.Click += new EventHandler(m_pTabFlows_Toolbar_Click);
            m_pTabFlows_Toolbar.Items.Add(tabFlows_Toolbar_Refresh);
            m_pTab.TabPages["flows"].Controls.Add(m_pTabFlows_Toolbar);

            m_pTabFlows_List = new ListView();
            m_pTabFlows_List.Size = new Size(m_pTab.TabPages["flows"].Width,m_pTab.TabPages["flows"].Height - 25);
            m_pTabFlows_List.Location = new Point(0,25);
            m_pTabFlows_List.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            m_pTabFlows_List.View = View.Details;
            m_pTabFlows_List.Columns.Add("Transport");
            m_pTabFlows_List.Columns.Add("Local EP",130);
            m_pTabFlows_List.Columns.Add("Remote EP",130);
            m_pTabFlows_List.Columns.Add("Last Activity",80);
            m_pTab.TabPages["flows"].Controls.Add(m_pTabFlows_List);

            #endregion
                        
            this.Controls.Add(m_pTab);
        }
                                                                
        #endregion


        #region Events handling

        #region method Logger_WriteLog

        /// <summary>
        /// Is raised when SIP stack has new log entry.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void Logger_WriteLog(object sender,LumiSoft.Net.Log.WriteLogEventArgs e)
        {
            if(!this.Visible){
                return;
            }

            m_pTabLog_Text.BeginInvoke(new MethodInvoker(delegate(){
                if(!((ToolStripButton)m_pTabLog_Toolbar.Items["log"]).Checked){
                    return;
                }

                string text = e.LogEntry.Text + "\n";
                if(((ToolStripButton)m_pTabLog_Toolbar.Items["logdata"]).Checked && e.LogEntry.Data != null){
                    text = text + "<begin>\r\n" + Encoding.Default.GetString(e.LogEntry.Data) + "<end>\r\n";
                }

                if(!IsAstericMatch(m_pTabLog_Toolbar.Items["filter"].Text,text)){
                    return;
                }
               
                if(m_OddLogEntry){
                    m_OddLogEntry = false;
                    m_pTabLog_Text.SelectionColor = Color.Gray;                    
                }
                else{
                    m_OddLogEntry = true;
                    m_pTabLog_Text.SelectionColor = Color.LightSeaGreen;
                }

                m_pTabLog_Text.AppendText(text);
            }));
        }

        #endregion


        #region method m_pTabLog_Toolbar_Click

        /// <summary>
        /// This method is called when when log toolbar button is pressed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pTabLog_Toolbar_Click(object sender,EventArgs e)
        {
            ToolStripButton button = (ToolStripButton)sender;

            if(button.Tag.ToString() == "clear"){
                m_pTabLog_Text.Text = "";
            }
        }

        #endregion

        #region method m_pTabTransactions_Toolbar_Click

        /// <summary>
        /// This method is called when when transactions toolbar button is pressed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pTabTransactions_Toolbar_Click(object sender,EventArgs e)
        {
            ToolStripButton button = (ToolStripButton)sender;

            if(button.Tag.ToString() == "refresh"){
                m_pTabTransactions_List.Items.Clear();

                foreach(SIP_ClientTransaction tr in m_pStack.TransactionLayer.ClientTransactions){
                    try{
                        ListViewItem it = new ListViewItem("false");
                        it.SubItems.Add(tr.Method);
                        it.SubItems.Add(tr.State.ToString());
                        it.SubItems.Add(tr.CreateTime.ToString("HH:mm:ss"));
                        it.SubItems.Add(tr.ID);
                        m_pTabTransactions_List.Items.Add(it);
                    }
                    catch{
                    }
                }

                foreach(SIP_ServerTransaction tr in m_pStack.TransactionLayer.ServerTransactions){
                    try{
                        ListViewItem it = new ListViewItem("true");
                        it.SubItems.Add(tr.Method);
                        it.SubItems.Add(tr.State.ToString());
                        it.SubItems.Add(tr.CreateTime.ToString("HH:mm:ss"));
                        it.SubItems.Add(tr.ID);
                        m_pTabTransactions_List.Items.Add(it);
                    }
                    catch{
                    }
                }
            }
        }

        #endregion

        #region method m_pTabDialogs_Toolbar_Click

        /// <summary>
        /// This method is called when when dialogs toolbar button is pressed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pTabDialogs_Toolbar_Click(object sender,EventArgs e)
        {
            ToolStripButton button = (ToolStripButton)sender;

            if(button.Tag.ToString() == "refresh"){
                m_pTabDialogs_List.Items.Clear();

                foreach(SIP_Dialog dialog in m_pStack.TransactionLayer.Dialogs){
                    try{
                        ListViewItem it = new ListViewItem(dialog.ID);
                        m_pTabDialogs_List.Items.Add(it);
                    }
                    catch{
                    }
                }
            }
        }

        #endregion

        #region method m_pTabFlows_Toolbar_Click

        /// <summary>
        /// This method is called when when flows toolbar button is pressed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pTabFlows_Toolbar_Click(object sender,EventArgs e)
        {
            ToolStripButton button = (ToolStripButton)sender;

            if(button.Tag.ToString() == "refresh"){
                m_pTabFlows_List.Items.Clear();

                foreach(SIP_Flow flow in m_pStack.TransportLayer.Flows){
                    try{
                        ListViewItem it = new ListViewItem(flow.Transport);
                        it.SubItems.Add(flow.LocalEP.ToString());
                        it.SubItems.Add(flow.RemoteEP.ToString());
                        it.SubItems.Add(flow.LastActivity.ToString("HH:mm:ss"));
                        m_pTabFlows_List.Items.Add(it);
                    }
                    catch{
                    }
                }
            }
        }

        #endregion


        #region method wfrm_Debug_FormClosed

        /// <summary>
        /// This method is called when debug window is closed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void wfrm_Debug_FormClosed(object sender,FormClosedEventArgs e)
        {
            m_pStack.Logger.WriteLog -= new EventHandler<LumiSoft.Net.Log.WriteLogEventArgs>(Logger_WriteLog);
        }

        #endregion

        #endregion


        #region static method IsAstericMatch

        /// <summary>
		/// Checks if text matches to search pattern.
		/// </summary>
		/// <param name="pattern"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public static bool IsAstericMatch(string pattern,string text)
		{
            pattern = pattern.ToLower();
			text = text.ToLower();

			if(pattern == ""){
				pattern = "*";
			}

			while(pattern.Length > 0){
				// *xxx[*xxx...]
				if(pattern.StartsWith("*")){
					// *xxx*xxx
					if(pattern.IndexOf("*",1) > -1){
						string indexOfPart = pattern.Substring(1,pattern.IndexOf("*",1) - 1);
						if(text.IndexOf(indexOfPart) == -1){
							return false;
						}

                        text = text.Substring(text.IndexOf(indexOfPart) + indexOfPart.Length);
                        pattern = pattern.Substring(pattern.IndexOf("*", 1));
					}
					// *xxx   This is last pattern	
					else{				
						return text.EndsWith(pattern.Substring(1));
					}
				}
				// xxx*[xxx...]
				else if(pattern.IndexOfAny(new char[]{'*'}) > -1){
					string startPart = pattern.Substring(0,pattern.IndexOfAny(new char[]{'*'}));
		
					// Text must startwith
					if(!text.StartsWith(startPart)){
						return false;
					}

					text = text.Substring(text.IndexOf(startPart) + startPart.Length);
					pattern = pattern.Substring(pattern.IndexOfAny(new char[]{'*'}));
				}
				// xxx
				else{
					return text == pattern;
				}
			}

            return true;
		}

		#endregion
    }
}
