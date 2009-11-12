using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP
{
    /// <summary>
    /// This class represents IMAP SEARCH <b>HEADER (field-name) (string)</b> key. Defined in RFC 3501 6.4.4.
    /// </summary>
    /// <remarks>Messages that have a header with the specified field-name (as
    /// defined in [RFC-2822]) and that contains the specified string
    /// in the text of the header (what comes after the colon).  If the
    /// string to search is zero-length, this matches all messages that
    /// have a header line with the specified field-name regardless of
    /// the contents.
    /// </remarks>
    public class IMAP_Search_Key_Header
    {
        private string m_FieldName = "";
        private string m_Value     = "";

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="fieldName">Header field name. For example: 'Subject'.</param>
        /// <param name="value">String value.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>fieldName</b> or <b>value</b> is null reference.</exception>
        public IMAP_Search_Key_Header(string fieldName,string value)
        {
            if(fieldName == null){
                throw new ArgumentNullException("fieldName");
            }
            if(value == null){
                throw new ArgumentNullException("value");
            }

            m_FieldName = fieldName;
            m_Value     = value;
        }


        #region override method ToString

        /// <summary>
        /// Returns this as string.
        /// </summary>
        /// <returns>Returns this as string.</returns>
        public override string ToString()
        {
            return "HEADER " + TextUtils.QuoteString(m_FieldName) + " " + TextUtils.QuoteString(m_Value);
        }

        #endregion


        #region Properties implementation

        /// <summary>
        /// Gets header field name.
        /// </summary>
        public string FieldName
        {
            get{ return m_FieldName; }
        }

        /// <summary>
        /// Gets filter value.
        /// </summary>
        public string Value
        {
            get{ return m_Value; }
        }

        #endregion
    }
}
