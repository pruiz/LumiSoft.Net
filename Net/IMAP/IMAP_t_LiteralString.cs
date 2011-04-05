using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP
{
    /// <summary>
    /// This class implements IMAP "literal" string. Defined in RFC 3501.
    /// </summary>
    public class IMAP_t_LiteralString
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="charset">Charset to use encode string value.</param>
        /// <param name="value">String value.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>charset</b> is null reference.</exception>
        public IMAP_t_LiteralString(Encoding charset,string value)
        {
            if(charset == null){
                throw new ArgumentNullException("charset");
            }
        }
    }
}
