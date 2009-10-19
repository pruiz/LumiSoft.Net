using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP.Server
{
    /// <summary>
    /// 
    /// </summary>
    public class IMAP_e_MessagesInfo : EventArgs
    {
        private string                 m_Folder    = null;
        private List<IMAP_MessageInfo> m_pMessages = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="folder">Folder name with optional path.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>folder</b> is null reference.</exception>
        internal IMAP_e_MessagesInfo(string folder)
        {
            if(folder == null){
                throw new ArgumentNullException("folder");
            }

            m_Folder = folder;
        }


        #region Properties implementation

        /// <summary>
        /// Gets folder name with optional path.
        /// </summary>
        public string Folder
        {
            get{ return m_Folder; }
        }


        internal int Exists
        {
            get{ return 0; }
        }

        internal int Recent
        {
            get{ return 0; }
        }

        internal int Unseen
        {
            get{ return 0; }
        }

        internal int UidNext
        {
            get{ return 0; }
        }

        #endregion
    }
}
