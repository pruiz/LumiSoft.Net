using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP.Client
{
    /// <summary>
    /// This class represents FETCH BODY.PEEK[] data item. Defined in RFC 3501.
    /// </summary>
    public class IMAP_Client_FetchArg_BodyPeek : IMAP_Client_FetchArg
    {
        private string m_Section  = null;
        private int    m_Offset   = -1;
        private int    m_MaxCount = -1;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public IMAP_Client_FetchArg_BodyPeek()
        {
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="section">Body section. Value null means not specified.</param>
        /// <param name="offset">Data returning offset. Value -1 means not specified.</param>
        /// <param name="maxCount">Maximum number of bytes to return. Value -1 means not specified.</param>
        public IMAP_Client_FetchArg_BodyPeek(string section,int offset,int maxCount)
        {
            m_Section  = section;
            m_Offset   = offset;
            m_MaxCount = maxCount;
        }


        #region override method ToString

        /// <summary>
        /// Returns this as string.
        /// </summary>
        /// <returns>Returns this as string.</returns>
        public override string ToString()
        {
            StringBuilder retVal = new StringBuilder();
            retVal.Append("BODY.PEEK[");
            if(m_Section != null){
                retVal.Append(m_Section);
            }
            retVal.Append("]");                        
            if(m_Offset > -1){
                retVal.Append("<" + m_Offset);
                if(m_MaxCount > -1){
                    retVal.Append("." + m_MaxCount);
                }
                retVal.Append(">");
            }

            return retVal.ToString();
        }

        #endregion
    }
}
