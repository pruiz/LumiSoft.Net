using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP
{
    /// <summary>
    /// This class represents IMAP server untagged status(OK,NO,BAD,PREAUTH and BYE) response. Defined in RFC 3501 7.1.
    /// </summary>
    public class IMAP_r_u_ServerStatus : IMAP_r_u
    {
        private string m_ResponseCode         = "";
        private string m_OptionalResponseCode = null;
        private string m_OptionalResponseArgs = null;
        private string m_ResponseText         = "";

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="responseCode">Response code.</param>
        /// <param name="optResponseCode">Optional response code(Response code between []).</param>
        /// <param name="optResponseArgs">Optional response arguments string.</param>
        /// <param name="responseText">Response text after response-code.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>responseCode</b> or <b>responseText</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public IMAP_r_u_ServerStatus(string responseCode,string optResponseCode,string optResponseArgs,string responseText)
        {
            if(responseCode == null){
                throw new ArgumentNullException("responseCode");
            }
            if(responseCode == string.Empty){
                throw new ArgumentException("The argument 'responseCode' value must be specified.","responseCode");
            }
            if(responseText == null){
                throw new ArgumentNullException("responseText");
            }
            if(responseText == string.Empty){
                throw new ArgumentException("The argument 'responseText' value must be specified.","responseText");
            }

            m_ResponseCode         = responseCode;
            m_OptionalResponseCode = optResponseCode;
            m_OptionalResponseArgs = optResponseArgs;
            m_ResponseText         = responseText;
        }


        #region static method Parse

        /// <summary>
        /// Parses IMAP command completion status response from response line.
        /// </summary>
        /// <param name="responseLine">Response line.</param>
        /// <returns>Returns parsed IMAP command completion status response.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>responseLine</b> is null reference value.</exception>
        public static IMAP_r_u_ServerStatus Parse(string responseLine)
        {
            if(responseLine == null){
                throw new ArgumentNullException("responseLine");
            }

            string[] parts           = responseLine.Split(new char[]{' '},3);
            string   commandTag      = parts[0];
            string   responseCode    = parts[1];
            string   optResponseCode = null;
            string   optResponseArgs = null;
            string   responseText    = parts[2];

            // Optional status code.
            if(parts[2].StartsWith("[")){
                StringReader r = new StringReader(parts[2]);
                string[] code_args = r.ReadParenthesized().Split(new char[]{' '},2);
                optResponseCode = code_args[0];
                if(code_args.Length == 2){
                    optResponseArgs = code_args[1];
                }
                responseText    = r.ReadToEnd();
            }

            return new IMAP_r_u_ServerStatus(responseCode,optResponseCode,optResponseArgs,responseText);
        }

        #endregion


        #region override method ToString

        /// <summary>
        /// Returns this as string.
        /// </summary>
        /// <returns>Returns this as string.</returns>
        public override string ToString()
        {
            StringBuilder retVal = new StringBuilder();
            retVal.Append("* " + m_ResponseCode + " ");
            if(!string.IsNullOrEmpty(m_OptionalResponseCode)){
                retVal.Append("[" + m_OptionalResponseCode);
                if(!string.IsNullOrEmpty(m_OptionalResponseArgs)){
                    retVal.Append(" " + m_OptionalResponseArgs);
                }
                retVal.Append("]");
            }
            retVal.Append(" " + m_ResponseText + "\r\n");

            return retVal.ToString();
        }

        #endregion


        #region Properties implementation

        /// <summary>
        /// Gets IMAP server status response code(OK,NO,BAD,PREAUTH,BYE).
        /// </summary>
        public string ResponseCode
        {
            get{ return m_ResponseCode; }
        }

        /// <summary>
        /// Gets IMAP server status response optiona response-code(ALERT,BADCHARSET,CAPABILITY,PARSE,PERMANENTFLAGS,
        /// READ-ONLY,READ-WRITE,TRYCREATE,UIDNEXT,UIDVALIDITY,UNSEEN).
        /// Value null means not specified. For more info see RFC 3501 7.1.
        /// </summary>
        public string OptionalResponseCode
        {
            get{ return m_OptionalResponseCode; }
        }

        /// <summary>
        /// Gets optional response aruments string. Value null means not specified. For more info see RFC 3501 7.1.
        /// </summary>
        public string OptionalResponseArgs
        {
            get{ return m_OptionalResponseArgs; }
        }

        /// <summary>
        /// Gets response human readable text after response-code.
        /// </summary>
        public string ResponseText
        {
            get{ return m_ResponseText; }
        }

        #endregion
    }
}
