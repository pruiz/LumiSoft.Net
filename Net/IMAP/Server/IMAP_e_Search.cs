using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP.Server
{
    /// <summary>
    /// This class provides data for <b cref="IMAP_Session.Search">IMAP_Session.Search</b> event.
    /// </summary>
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


        //public void AddMessage(int seqNo,long uid)
        //{
        //}


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
                
        internal event EventHandler Matched = null;

        #endregion
    }
}
