using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP
{
    /// <summary>
    /// This class represents IMAP MYRIGHTS response. Defined in RFC 4314 3.8.
    /// </summary>
    public class IMAP_Response_MyRights
    {
        private string     m_FolderName = "";
        private List<char> m_pRights    = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="folder">Folder name with path.</param>
        /// <param name="rights">Rights values.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>folder</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public IMAP_Response_MyRights(string folder,char[] rights)
        {
            if(folder == null){
                throw new ArgumentNullException("folder");
            }
            if(folder == string.Empty){
                throw new ArgumentException("Argument 'folder' value must be specified.","folder");
            }

            m_FolderName = folder;
            
            m_pRights = new List<char>();
            if(rights != null){
                m_pRights.AddRange(rights);
            }
        }


        #region static method Parse

        /// <summary>
        /// Parses MYRIGHTS response from MYRIGHTS-response string.
        /// </summary>
        /// <param name="myRightsResponse">MYRIGHTS response line.</param>
        /// <returns>Returns parsed MYRIGHTS response.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>myRightsResponse</b> is null reference.</exception>
        public static IMAP_Response_MyRights Parse(string myRightsResponse)
        {
            if(myRightsResponse == null){
                throw new ArgumentNullException("myRightsResponse");
            }

            /* RFC 4314 3.8. MYRIGHTS Response.
                Data:       mailbox name
                            rights

                The MYRIGHTS response occurs as a result of a MYRIGHTS command.  The
                first string is the mailbox name for which these rights apply.  The
                second string is the set of rights that the client has.

                Section 2.1.1 details additional server requirements related to
                handling of the virtual "d" and "c" rights.
             
                Example:    C: A003 MYRIGHTS INBOX
                            S: * MYRIGHTS INBOX rwiptsldaex
                            S: A003 OK Myrights complete
            */

            StringReader r = new StringReader(myRightsResponse);
            // Eat "*"
            r.ReadWord();
            // Eat "MYRIGHTS"
            r.ReadWord();

            string folder = IMAP_Utils.Decode_IMAP_UTF7_String(r.ReadWord(true));
            char[] rights = r.ReadToEnd().Trim().ToCharArray();

            return new IMAP_Response_MyRights(folder,rights);
        }

        #endregion


        #region Properties implementation

        /// <summary>
        /// Gets folder name.
        /// </summary>
        public string FolderName
        {
            get{ return m_FolderName; }
        }

        /// <summary>
        /// Gets rights list.
        /// </summary>
        public List<char> Rights
        {
            get{ return m_pRights; }
        }

        #endregion
    }
}
