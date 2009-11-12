using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP
{
    /// <summary>
    /// This class represents IMAP SEARCH <b>SMALLER (n)</b> key. Defined in RFC 3501 6.4.4.
    /// </summary>
    /// <remarks>Messages with an [RFC-2822] size smaller than the specified number of octets.</remarks>
    public class IMAP_Search_Key_Smaller
    {
        private int m_Value = 0;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">Message size in bytes.</param>
        public IMAP_Search_Key_Smaller(int value)
        {
            m_Value = value;
        }


        #region override method ToString

        /// <summary>
        /// Returns this as string.
        /// </summary>
        /// <returns>Returns this as string.</returns>
        public override string ToString()
        {
            return "SMALLER " + m_Value;
        }

        #endregion


        #region Properties implementation

        /// <summary>
        /// Gets value.
        /// </summary>
        public int Value
        {
            get{ return m_Value; }
        }

        #endregion
    }
}
