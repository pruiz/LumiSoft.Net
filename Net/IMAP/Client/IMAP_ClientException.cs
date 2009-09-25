using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP.Client
{
    /// <summary>
    /// IMAP client exception.
    /// </summary>
    public class IMAP_ClientException : Exception
    {
        private string m_StatusCode   = "";
        private string m_ResponseText = "";
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="responseLine">IMAP server response line.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>responseLine</b> is null.</exception>
        public IMAP_ClientException(string responseLine) : base(responseLine)
        {
            if(responseLine == null){
                throw new ArgumentNullException("responseLine");
            }

            // <status-code> SP <response-text>
            string[] code_text = responseLine.Split(new char[]{ },2);
            m_StatusCode = code_text[0];
            if(code_text.Length == 2){
                m_ResponseText = code_text[1];
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="responseCode">IMAP response code(BAD,NO).</param>
        /// <param name="responseText">Response text.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>responseCode</b> or <b>responseText</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public IMAP_ClientException(string responseCode,string responseText) : base(responseCode + " " + responseText)
        {
            if(responseCode == null){
                throw new ArgumentNullException("responseCode");
            }
            if(responseCode == string .Empty){
                throw new ArgumentException("Argument 'responseCode' value must be specified.","responseCode");
            }
            if(responseText == null){
                throw new ArgumentNullException("responseText");
            }
            if(responseText == string .Empty){
                throw new ArgumentException("Argument 'responseText' value must be specified.","responseText");
            }

            m_StatusCode   = responseCode;
            m_ResponseText = responseText;
        }


        #region Properties Implementation

        /// <summary>
        /// Gets IMAP server error status code.
        /// </summary>
        public string StatusCode
        {
            get{ return m_StatusCode; }
        }

        /// <summary>
        /// Gets IMAP server response text after status code.
        /// </summary>
        public string ResponseText
        {
            get{ return m_ResponseText; }
        }

        #endregion

    }
}
