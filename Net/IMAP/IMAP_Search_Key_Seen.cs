using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP
{
    /// <summary>
    /// This class represents IMAP SEARCH <b>SEEN</b> key. Defined in RFC 3501 6.4.4.
    /// </summary>
    /// <remarks>Messages that have the \Seen flag set.</remarks>
    public class IMAP_Search_Key_Seen
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public IMAP_Search_Key_Seen()
        {
        }


        #region override method ToString

        /// <summary>
        /// Returns this as string.
        /// </summary>
        /// <returns>Returns this as string.</returns>
        public override string ToString()
        {
            return "SEEN";
        }

        #endregion
    }
}
