using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP
{
    /// <summary>
    /// This class represents IMAP QUOTA response. Defined in RFC 2087 5.1.
    /// </summary>
    public class IMAP_Response_Quota : IMAP_Response
    {
        private string             m_QuotaRootName = "";
        private IMAP_Quota_Entry[] m_pEntries      = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="quotaRootName">Qouta root name.</param>
        /// <param name="entries">Resource limit entries.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>quotaRootName</b> or <b>entries</b> is null reference.</exception>
        public IMAP_Response_Quota(string quotaRootName,IMAP_Quota_Entry[] entries)
        {
            if(quotaRootName == null){
                throw new ArgumentNullException("quotaRootName");
            }
            if(entries == null){
                throw new ArgumentNullException("entries");
            }

            m_QuotaRootName = quotaRootName;
            m_pEntries      = entries;
        }


        #region static method Parse

        /// <summary>
        /// Parses QUOTA response from quota-response string.
        /// </summary>
        /// <param name="response">QUOTA response string.</param>
        /// <returns>Returns parsed QUOTA response.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>response</b> is null reference.</exception>
        public static IMAP_Response_Quota Parse(string response)
        {
            if(response == null){
                throw new ArgumentNullException("response");
            }

            /* RFC 2087 5.1. QUOTA Response.
                Data:       quota root name
                            list of resource names, usages, and limits

                This response occurs as a result of a GETQUOTA or GETQUOTAROOT
                command. The first string is the name of the quota root for which
                this quota applies.

                The name is followed by a S-expression format list of the resource
                usage and limits of the quota root.  The list contains zero or
                more triplets.  Each triplet conatins a resource name, the current
                usage of the resource, and the resource limit.

                Resources not named in the list are not limited in the quota root.
                Thus, an empty list means there are no administrative resource
                limits in the quota root.

                Example:    S: * QUOTA "" (STORAGE 10 512)
            */

            StringReader r = new StringReader(response);
            // Eat "*"
            r.ReadWord();
            // Eat "QUOTA"
            r.ReadWord();

            string                 name    = r.ReadWord();
            string[]               items   = r.ReadParenthesized().Split(' ');
            List<IMAP_Quota_Entry> entries = new List<IMAP_Quota_Entry>();
            for(int i=0;i<items.Length;i+=3){
                entries.Add(new IMAP_Quota_Entry(items[i],Convert.ToInt64(items[i + 1]),Convert.ToInt64(items[i + 2])));
            }

            return new IMAP_Response_Quota(name,entries.ToArray());
        }

        #endregion


        #region Properties impelemntation

        /// <summary>
        /// Gets quota root name.
        /// </summary>
        public string QuotaRootName
        {
            get{ return m_QuotaRootName; }
        }

        /// <summary>
        /// Gets resource limit entries.
        /// </summary>
        public IMAP_Quota_Entry[] Entries
        {
            get{ return m_pEntries; }
        }

        #endregion
    }
}
