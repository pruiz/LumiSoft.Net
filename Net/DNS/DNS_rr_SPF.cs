using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.DNS
{
    /// <summary>
    /// This class represent SPF resource record. Defined in RFC 4408.
    /// </summary>
	[Serializable]
    public class DNS_rr_SPF : DNS_rr
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public DNS_rr_SPF() : base(DNS_QType.SPF,1)
        {
        }


        #region Properties implementation

        #endregion
    }
}
