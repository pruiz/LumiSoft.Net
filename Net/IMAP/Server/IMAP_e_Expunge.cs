using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP.Server
{
    /// <summary>
    /// This class provides data for <b cref="IMAP_Session.Expunge">IMAP_Session.Expunge</b> event.
    /// </summary>
    public class IMAP_e_Expunge : EventArgs
    {
        private string           m_Folder   = null;
        private IMAP_MessageInfo m_pMsgInfo = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="folder">Folder name with optional path.</param>
        /// <param name="msgInfo">Message info.</param>
        /// <exception cref="ArgumentNullException">Is riased when <b>folder</b>,<b>msgInfo</b> is null reference.</exception>
        internal IMAP_e_Expunge(string folder,IMAP_MessageInfo msgInfo)
        {
            if(folder == null){
                throw new ArgumentNullException("folder");
            }
            if(msgInfo == null){
                throw new ArgumentNullException("msgInfo");
            }

            m_Folder   = folder;
            m_pMsgInfo = msgInfo;
        }


        #region Properties implementation

        /// <summary>
        /// Gets folder name with optional path.
        /// </summary>
        public string Folder
        {
            get{ return m_Folder; }
        }

        /// <summary>
        /// Gets message info.
        /// </summary>
        public IMAP_MessageInfo MessageInfo
        {
            get{ return m_pMsgInfo; }
        }

        #endregion
    }
}
