using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP
{
    /// <summary>
    /// This class represents IMAP SEARCH <b>SINCE (date)</b> key. Defined in RFC 3501 6.4.4.
    /// </summary>
    /// <remarks>Messages whose internal date (disregarding time and timezone)
    /// is within or later than the specified date.</remarks>
    public class IMAP_Search_Key_Since
    {
        private DateTime m_Date;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">Date value.</param>
        public IMAP_Search_Key_Since(DateTime value)
        {
            m_Date = value;
        }


        #region override method ToString

        /// <summary>
        /// Returns this as string.
        /// </summary>
        /// <returns>Returns this as string.</returns>
        public override string ToString()
        {
            return "SINCE " + m_Date.ToString("dd-MMM-yyyy");
        }

        #endregion


        #region Properties implementation

        /// <summary>
        /// Gets date value.
        /// </summary>
        public DateTime Date
        {
            get{ return m_Date; }
        }

        #endregion
    }
}
