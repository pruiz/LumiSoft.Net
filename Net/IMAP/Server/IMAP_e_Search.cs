using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP.Server
{
    /// <summary>
    /// This class provides data for <b cref="IMAP_Session.Search">IMAP_Session.Search</b> event.
    /// </summary>
    /// <remarks>
    /// IMAP SEARCH handler application should provide message UID per each search criteria matched message
    /// by calling <see cref="IMAP_e_Search.AddMessage(long)"/> method.</remarks>
    public class IMAP_e_Search : EventArgs
    {
        private IMAP_Search_Key m_pCriteria = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="criteria">Serach criteria.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>criteria</b> is null reference.</exception>
        internal IMAP_e_Search(IMAP_Search_Key criteria)
        {
            if(criteria == null){
                throw new ArgumentNullException("criteria");
            }

            m_pCriteria = criteria;
        }


        #region method AddMessage

        /// <summary>
        /// Adds message which matches search criteria.
        /// </summary>
        /// <param name="uid">Message UID value.</param>
        public void AddMessage(long uid)
        {
            OnMatched(uid);
        }

        #endregion


        #region Properties implementation

        /// <summary>
        /// Gets search criteria.
        /// </summary>
        public IMAP_Search_Key Criteria
        {
            get{ return m_pCriteria; }
        }

        #endregion

        #region Events implementation
                
        /// <summary>
        /// Is raised when new message matches search criteria.
        /// </summary>
        internal event EventHandler<EventArgs<long>> Matched = null;

        #region method OnMatched

        /// <summary>
        /// Raises <b>Matched</b> event.
        /// </summary>
        /// <param name="uid">Message UID.</param>
        private void OnMatched(long uid)
        {
            if(this.Matched != null){
                this.Matched(this,new EventArgs<long>(uid));
            }
        }

        #endregion

        #endregion
    }
}
