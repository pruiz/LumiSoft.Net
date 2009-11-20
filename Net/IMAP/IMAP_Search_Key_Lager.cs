using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP
{
    /// <summary>
    /// This class represents IMAP SEARCH <b>LARGER (n)</b> key. Defined in RFC 3501 6.4.4.
    /// </summary>
    /// <remarks>Messages with an [RFC-2822] size larger than the specified number of octets.</remarks>
    public class IMAP_Search_Key_Lager : IMAP_Search_Key
    {
        private int m_Value = 0;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">Message size in bytes.</param>
        public IMAP_Search_Key_Lager(int value)
        {
            m_Value = value;
        }


        #region static method Parse

        /// <summary>
        /// Returns parsed IMAP SEARCH <b>LAGER (string)</b> key.
        /// </summary>
        /// <param name="r">String reader.</param>
        /// <returns>Returns parsed IMAP SEARCH <b>LAGER (string)</b> key.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>r</b> is null reference.</exception>
        /// <exception cref="ParseException">Is raised when parsing fails.</exception>
        internal static IMAP_Search_Key_Lager Parse(StringReader r)
        {
            if(r == null){
                throw new ArgumentNullException("r");
            }

            string word = r.ReadWord();
            if(!string.Equals(word,"LAGER",StringComparison.InvariantCultureIgnoreCase)){
                throw new ParseException("Parse error: Not a SEARCH 'LAGER' key.");
            }
            string value = r.ReadWord();
            if(value == null){
                throw new ParseException("Parse error: Invalid 'LAGER' value.");
            }
            int size = 0;
            if(!int.TryParse(value,out size)){
                throw new ParseException("Parse error: Invalid 'LAGER' value.");
            }

            return new IMAP_Search_Key_Lager(size);
        }

        #endregion


        #region override method ToString

        /// <summary>
        /// Returns this as string.
        /// </summary>
        /// <returns>Returns this as string.</returns>
        public override string ToString()
        {
            return "LARGER " + m_Value;
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
