using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.SIP.Stack
{
    /// <summary>
    /// This class represents REFER dialog. Defined in RFC 3515.
    /// </summary>
    public class SIP_Dialog_Refer //: SIP_Dialog
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        private SIP_Dialog_Refer()
        {
        }


        private void CreateNotify(string statusLine)
        {
            // TODO: Block for UAC ? because UAS can generate NOTIFY requests only.
        }
                       

        #region Properties implementation

        #endregion

        #region Events implementation

        /// <summary>
        /// Is raised when NOTIFY request received.
        /// </summary>
        public event EventHandler Notify = null;

        #region method OnNotify

        /// <summary>
        /// Raises <b>Notify</b> event.
        /// </summary>
        private void OnNotify()
        {
            if(this.Notify != null){
                this.Notify(this,new EventArgs());
            }
        }

        #endregion

        #endregion
    }
}
