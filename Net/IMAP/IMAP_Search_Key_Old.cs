using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP
{
    /// <summary>
    /// This class represents IMAP SEARCH <b>OLD</b> key. Defined in RFC 3501 6.4.4.
    /// </summary>
    /// <remarks>Messages that do not have the \Recent flag set.  This is
    /// functionally equivalent to "NOT RECENT" (as opposed to "NOT NEW").</remarks>
    public class IMAP_Search_Key_Old
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public IMAP_Search_Key_Old()
        {
        }


        #region override method ToString

        /// <summary>
        /// Returns this as string.
        /// </summary>
        /// <returns>Returns this as string.</returns>
        public override string ToString()
        {
            return "OLD";
        }

        #endregion
    }
}
