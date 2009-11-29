using System;

namespace LumiSoft.Net.DNS
{
	/// <summary>
	/// This is base class for DNS records.
	/// </summary>
	public abstract class DNS_rr
	{
		private DNS_QType m_Type = DNS_QType.A;
		private int       m_TTL  = -1;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="recordType">Record type (A,MX, ...).</param>
		/// <param name="ttl">TTL (time to live) value in seconds.</param>
		public DNS_rr(DNS_QType recordType,int ttl)
		{
			m_Type = recordType;
			m_TTL  = ttl;
        }


        #region Properties Implementation

        /// <summary>
		/// Gets record type (A,MX,...).
		/// </summary>
		public DNS_QType RecordType
		{
			get{ return m_Type; }
		}

		/// <summary>
		/// Gets TTL (time to live) value in seconds.
		/// </summary>
		public int TTL
		{
			get{ return m_TTL; }
		}

		#endregion
	}
}
