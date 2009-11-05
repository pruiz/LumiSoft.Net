using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP.Server
{
    /// <summary>
    /// This class provides data for <b cref="IMAP_Session.Fetch">IMAP_Session.Fetch</b> event.
    /// </summary>
    /// <remarks>
    /// IMAP FETCH handler application should provide requested data for each message in <see cref="IMAP_e_Fetch.MessagesInfo"/>
    /// by calling <see cref="IMAP_e_Fetch.AddData(IMAP_MessageInfo,Stream)"/> method.
    /// </remarks>
    public class IMAP_e_Fetch : EventArgs
    {
        #region class e_NewMessageData

        /// <summary>
        /// 
        /// </summary>
        internal class e_NewMessageData : EventArgs
        {            
            private IMAP_MessageInfo m_pMsgInfo = null;
            private Stream           m_pMsgData = null;

            /// <summary>
            /// Sedfault constructor.
            /// </summary>
            /// <param name="msgInfo">Message info.</param>
            /// <param name="msgData">Message data stream.</param>
            /// <exception cref="ArgumentNullException">Is raised when <b>msgInfo</b> is null reference.</exception>
            public e_NewMessageData(IMAP_MessageInfo msgInfo,Stream msgData)
            {
                if(msgInfo == null){
                    throw new ArgumentNullException("msgInfo");
                }

                m_pMsgInfo = msgInfo;
                m_pMsgData = msgData;
            }


            #region Properties implementation

            /// <summary>
            /// Gets message info.
            /// </summary>
            public IMAP_MessageInfo MessageInfo
            {
                get{ return m_pMsgInfo; }
            }

            /// <summary>
            /// Gets message data stream.
            /// </summary>
            public Stream MessageData
            {
                get{ return m_pMsgData; }
            }

            #endregion
        }

        #endregion

        private IMAP_r_ServerStatus m_pResponse     = null;
        private IMAP_MessageInfo[]  m_pMessagesInfo = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="messagesInfo">Messages info.</param>
        /// <param name="response">Default IMAP server response.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>messagesInfo</b> or <b>response</b> is null reference.</exception>
        internal IMAP_e_Fetch(IMAP_MessageInfo[] messagesInfo,IMAP_r_ServerStatus response)
        {
            if(messagesInfo == null){
                throw new ArgumentNullException("messagesInfo");
            }
            if(response == null){
                throw new ArgumentNullException("response");
            }

            m_pMessagesInfo = messagesInfo;
            m_pResponse     = response;
        }


        #region method AddData

        internal void AddData(IMAP_MessageInfo msgInfo)
        {
            OnNewMessageData(msgInfo,null);
        }

        public void AddData(IMAP_MessageInfo msgInfo,Stream stream)
        {
            if(msgInfo == null){
                throw new ArgumentNullException("msgInfo");
            }
            if(stream == null){
                throw new ArgumentNullException("stream");
            }

            // TODO: Accpet more data than requested: like Header needed, FullMessage passed.

            OnNewMessageData(msgInfo,stream);
        }

        #endregion


        #region Properties impelementation

        /// <summary>
        /// Gets or sets IMAP server response to this operation.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when null reference value set.</exception>
        public IMAP_r_ServerStatus Response
        {
            get{ return m_pResponse; }

            set{ 
                if(value == null){
                    throw new ArgumentNullException("value");
                }

                m_pResponse = value; 
            }
        }

        /// <summary>
        /// Gets messages info.
        /// </summary>
        public IMAP_MessageInfo[] MessagesInfo
        {
            get{ return m_pMessagesInfo; }
        }

        #endregion

        #region Events implementation

        /// <summary>
        /// 
        /// </summary>
        internal event EventHandler<IMAP_e_Fetch.e_NewMessageData> NewMessageData = null;

        #region method OnNewMessageData

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msgInfo"></param>
        /// <param name="stream"></param>
        private void OnNewMessageData(IMAP_MessageInfo msgInfo,Stream stream)
        {
            if(this.NewMessageData != null){
                this.NewMessageData(this,new e_NewMessageData(msgInfo,stream));
            }
        }

        #endregion

        #endregion
    }
}
