using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP
{
    /// <summary>
    /// This class represents IMAP SEARCH <b>UNKEYWORD <flag></b> key. Defined in RFC 3501 6.4.4.
    /// </summary>
    /// <remarks>Messages that do not have the specified keyword flag set.</remarks>
    public class IMAP_Search_Key_Unkeyword
    {
         private string m_Value = "";

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">String value.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        public IMAP_Search_Key_Unkeyword(string value)
        {
            if(value == null){
                throw new ArgumentNullException("value");
            }

            m_Value = value;
        }


        #region override method ToString

        /// <summary>
        /// Returns this as string.
        /// </summary>
        /// <returns>Returns this as string.</returns>
        public override string ToString()
        {
            return "UNKEYWORD " + m_Value;
        }

        #endregion


        #region Properties implementation

        /// <summary>
        /// Gets keyword value.
        /// </summary>
        public string Value
        {
            get{ return m_Value; }
        }

        #endregion
    }
}
