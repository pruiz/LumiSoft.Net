using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP
{
    /// <summary>
    /// This class represents FETCH ENVELOPE data item. Defined in RFC 3501.
    /// </summary>
    public class IMAP_Fetch_DataItem_Envelope : IMAP_Fetch_DataItem
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public IMAP_Fetch_DataItem_Envelope()
        {
        }


        #region override method ToString

        /// <summary>
        /// Returns this as string.
        /// </summary>
        /// <returns>Returns this as string.</returns>
        public override string ToString()
        {
            return "ENVELOPE";
        }

        #endregion
    }
}
