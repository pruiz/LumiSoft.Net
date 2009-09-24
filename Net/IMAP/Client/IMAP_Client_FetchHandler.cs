using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP.Client
{
    /// <summary>
    /// This class provides IMAP FETCH response handling methods.
    /// </summary>
    public class IMAP_Client_FetchHandler
    {
        private int m_CurrentSeqNo = -1;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public IMAP_Client_FetchHandler()
        {            
        }


        #region method SetCurrentSeqNo

        /// <summary>
        /// Sets <b>CurrentSeqNo</b> property value.
        /// </summary>
        /// <param name="seqNo">Message sequnece number.</param>
        internal void SetCurrentSeqNo(int seqNo)
        {
            m_CurrentSeqNo = seqNo;
        }

        #endregion


        #region Properties implementation

        /// <summary>
        /// Gets current message sequence number. Value -1 means no current message.
        /// </summary>
        public int CurrentSeqNo
        {
            get{ return m_CurrentSeqNo; }
        }

        #endregion

        #region Events implementation

        /// <summary>
        /// This event is raised when current message changes and next message FETCH data-items will be returned.
        /// </summary>
        public event EventHandler NextMessage = null;

        #region method OnNextMessage

        /// <summary>
        /// Raises <b>NextMessage</b> event.
        /// </summary>
        internal void OnNextMessage()
        {
            if(this.NextMessage != null){
                this.NextMessage(this,new EventArgs());
            }
        }

        #endregion

        /// <summary>
        /// Is raised when current message FETCH BODY[] data-item is returned.
        /// </summary>
        public event EventHandler<IMAP_Client_Fetch_Body_EArgs> Body = null;

        #region method OnBody

        /// <summary>
        /// Raises <b>Body</b> event.
        /// </summary>
        /// <param name="eArgs">Event args.</param>
        internal void OnBody(IMAP_Client_Fetch_Body_EArgs eArgs)
        {
            if(this.Body != null){
                this.Body(this,eArgs);
            }
        }

        #endregion

        /// <summary>
        /// Is raised when current message FETCH FLAGS data-item is returned.
        /// </summary>
        public event EventHandler<EventArgs<string[]>> Flags = null;

        #region method OnFlags

        /// <summary>
        /// Raises <b>Flags</b> event.
        /// </summary>
        /// <param name="date">Message flags.</param>
        internal void OnFlags(string[] flags)
        {
            if(this.Flags != null){
                this.Flags(this,new EventArgs<string[]>(flags));
            }
        }

        #endregion

        /// <summary>
        /// Is raised when current message FETCH INTERNALDATE data-item is returned.
        /// </summary>
        public event EventHandler<EventArgs<DateTime>> InternalDate = null;

        #region method OnInternalDate

        /// <summary>
        /// Raises <b>InternalDate</b> event.
        /// </summary>
        /// <param name="date">Message IMAP server internal date.</param>
        internal void OnInternalDate(DateTime date)
        {
            if(this.InternalDate != null){
                this.InternalDate(this,new EventArgs<DateTime>(date));
            }
        }

        #endregion

        /// <summary>
        /// Is raised when current message FETCH RFC822 data-item is returned.
        /// </summary>
        public event EventHandler<IMAP_Client_Fetch_Rfc822_EArgs> Rfc822 = null;

        #region method OnRfc822

        /// <summary>
        /// Raises <b>Rfc822</b> event.
        /// </summary>
        /// <param name="eArgs">Event args.</param>
        internal void OnRfc822(IMAP_Client_Fetch_Rfc822_EArgs eArgs)
        {
            if(this.Rfc822 != null){
                this.Rfc822(this,eArgs);
            }
        }

        #endregion

        /// <summary>
        /// Is raised when current message FETCH RFC822.HEADER data-item is returned.
        /// </summary>
        public event EventHandler<EventArgs<string>> Rfc822Header = null;

        #region method OnRfc822Header

        /// <summary>
        /// Raises <b>Rfc822Text</b> event.
        /// </summary>
        /// <param name="header">Message header.</param>
        internal void OnRfc822Header(string header)
        {
            if(this.Rfc822Header != null){
                this.Rfc822Header(this,new EventArgs<string>(header));
            }
        }

        #endregion

        /// <summary>
        /// Is raised when current message FETCH RFC822.SIZE data-item is returned.
        /// </summary>
        public event EventHandler<EventArgs<int>> Rfc822Size = null;

        #region method OnSize

        /// <summary>
        /// Raises <b>Rfc822Size</b> event.
        /// </summary>
        /// <param name="size">Message size in bytes.</param>
        internal void OnSize(int size)
        {
            if(this.Rfc822Size != null){
                this.Rfc822Size(this,new EventArgs<int>(size));
            }
        }

        #endregion

        /// <summary>
        /// Is raised when current message FETCH RFC822.TEXT data-item is returned.
        /// </summary>
        public event EventHandler<EventArgs<string>> Rfc822Text = null;

        #region method OnRfc822Text

        /// <summary>
        /// Raises <b>Rfc822Text</b> event.
        /// </summary>
        /// <param name="text">Message body text.</param>
        internal void OnRfc822Text(string text)
        {
            if(this.Rfc822Text != null){
                this.Rfc822Text(this,new EventArgs<string>(text));
            }
        }

        #endregion

        /// <summary>
        /// Is raised when current message FETCH UID data-item is returned.
        /// </summary>
        public event EventHandler<EventArgs<long>> UID = null;

        #region method OnUID

        /// <summary>
        /// Raises <b>UID</b> event.
        /// </summary>
        /// <param name="uid">Message UID value.</param>
        internal void OnUID(long uid)
        {
            if(this.UID != null){
                this.UID(this,new EventArgs<long>(uid));
            }
        }

        #endregion

        #endregion
    }
}
