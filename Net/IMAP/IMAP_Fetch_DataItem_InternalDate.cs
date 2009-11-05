using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP
{
    /// <summary>
    /// This class represents FETCH INTERNALDATE data item. Defined in RFC 3501.
    /// </summary>
    public class IMAP_Fetch_DataItem_InternalDate : IMAP_Fetch_DataItem
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public IMAP_Fetch_DataItem_InternalDate()
        {
        }


        #region override method ToString

        /// <summary>
        /// Returns this as string.
        /// </summary>
        /// <returns>Returns this as string.</returns>
        public override string ToString()
        {
            return "INTERNALDATE";
        }

        #endregion
    }
}
