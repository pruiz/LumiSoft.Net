using System;

using LumiSoft.Net.Mail;

namespace LumiSoft.Net.IMAP.Server
{
	/// <summary>
	/// IMAP SEARCH message matcher. You can use this class to see if message values match to search criteria.
	/// </summary>
	public class IMAP_SearchMatcher
	{
		private SearchGroup m_pSearchCriteria = null;

		/// <summary>
		/// Deault constuctor.
		/// </summary>
		/// <param name="mainSearchGroup">SEARCH command main search group.</param>
		internal IMAP_SearchMatcher(SearchGroup mainSearchGroup)
		{
			m_pSearchCriteria = mainSearchGroup;
		}


		#region method Matches
		
		/// <summary>
		/// Gets if specified values match search criteria.
		/// </summary>
		/// <param name="no">Message sequence number.</param>
		/// <param name="uid">Message UID.</param>
		/// <param name="size">Message size in bytes.</param>
		/// <param name="internalDate">Message INTERNALDATE (dateTime when server stored message).</param>
		/// <param name="flags">Message flags.</param>
		/// <param name="header">Message header. This is only needed if this.IsHeaderNeeded is true.</param>
		/// <param name="bodyText">Message body text (must be decoded unicode text). This is only needed if this.IsBodyTextNeeded is true.</param>
		/// <returns></returns>
		public bool Matches(int no,int uid,int size,DateTime internalDate,IMAP_MessageFlags flags,string header,string bodyText)
		{
			// Parse header only if it's needed
			Mail_Message m = null;
            if(m_pSearchCriteria.IsHeaderNeeded()){
				m = new Mail_Message();
				m.Header.Parse(header);
			}

			return m_pSearchCriteria.Match(no,uid,size,internalDate,flags,m,bodyText);
		}

		#endregion


		#region Properties Implementation

		/// <summary>
		/// Gets if header is needed for matching.
		/// </summary>
		public bool IsHeaderNeeded
		{
			get{ return m_pSearchCriteria.IsHeaderNeeded(); }
		}

		/// <summary>
		/// Gets if body text is needed for matching.
		/// </summary>
		public bool IsBodyTextNeeded
		{
			get{ return m_pSearchCriteria.IsBodyTextNeeded(); }
		}

		#endregion

	}
}
