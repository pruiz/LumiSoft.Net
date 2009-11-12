using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP
{
    /// <summary>
    /// This class represents IMAP SEARCH <b>UID (sequence set)</b> key. Defined in RFC 3501 6.4.4.
    /// </summary>
    /// <remarks>Messages with unique identifiers corresponding to the specified
    /// unique identifier set.  Sequence set ranges are permitted.</remarks>
    public class IMAP_Search_Key_Uid
    {
        private IMAP_SequenceSet m_pSeqSet = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="seqSet">IMAP sequence-set.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>seqSet</b> is null reference.</exception>
        public IMAP_Search_Key_Uid(IMAP_SequenceSet seqSet)
        {
            if(seqSet == null){
                throw new ArgumentNullException("seqSet");
            }

            m_pSeqSet = seqSet;
        }


        #region override method ToString

        /// <summary>
        /// Returns this as string.
        /// </summary>
        /// <returns>Returns this as string.</returns>
        public override string ToString()
        {
            return "UID " + m_pSeqSet.ToSequenceSetString();
        }

        #endregion


        #region Properties implementation

        /// <summary>
        /// Gets sequence-set value.
        /// </summary>
        public IMAP_SequenceSet Value
        {
            get{ return m_pSeqSet; }
        }

        #endregion
    }
}
