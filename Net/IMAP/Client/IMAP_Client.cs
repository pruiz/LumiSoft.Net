using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Security.Principal;

using LumiSoft.Net.IO;
using LumiSoft.Net.TCP;
using LumiSoft.Net.IMAP;
using LumiSoft.Net.MIME;

namespace LumiSoft.Net.IMAP.Client
{
    /// <summary>
    /// IMAP v4 Client. Defined in RFC 3501.
    /// </summary>
    public class IMAP_Client : TCP_Client
    {
        #region class _FetchResponseReader

        /// <summary>
        /// This class implements FETCH response reader.
        /// </summary>
        internal class _FetchResponseReader
        {
            private IMAP_Client              m_pImap        = null;
            private string                   m_FetchLine    = null;
            private StringReader             m_pFetchReader = null;
            private IMAP_Client_FetchHandler m_pHandler     = null;

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="imap">IMAP client.</param>
            /// <param name="fetchLine">Initial FETCH response line.</param>
            /// <param name="handler">Fetch data-items handler.</param>
            /// <exception cref="ArgumentNullException">Is raised when <b>imap</b>,<b>fetchLine</b> or <b>handler</b> is null reference.</exception>
            public _FetchResponseReader(IMAP_Client imap,string fetchLine,IMAP_Client_FetchHandler handler)
            {
                if(imap == null){
                    throw new ArgumentNullException("imap");
                }
                if(fetchLine == null){
                    throw new ArgumentNullException("fetchLine");
                }
                if(handler == null){
                    throw new ArgumentNullException("handler");
                }

                m_pImap     = imap;
                m_FetchLine = fetchLine;
                m_pHandler  = handler;
            }

            #region method Start

            /// <summary>
            /// Starts reading FETCH response.
            /// </summary>
            public void Start()
            {
                // * seqNo FETCH 1data-item/(1*data-item)

                int seqNo = Convert.ToInt32(m_FetchLine.Split(' ')[1]);

                // Notify that current message has changed.
                m_pHandler.SetCurrentSeqNo(seqNo);
                m_pHandler.OnNextMessage();

                m_pFetchReader = new StringReader(m_FetchLine.Split(new char[]{' '},4)[3]);
                if(m_pFetchReader.StartsWith("(")){
                    m_pFetchReader.ReadSpecifiedLength(1);
                }

                // Read data-items.
                while(m_pFetchReader.Available > 0){
                    m_pFetchReader.ReadToFirstChar();
//*
                    #region BODY

                    if(m_pFetchReader.StartsWith("BODY ",false)){
                    }

                    #endregion

                    #region BODY[<section>]<<origin octet>>

                    else if(m_pFetchReader.StartsWith("BODY[",false)){
                        // Eat BODY word.
                        m_pFetchReader.ReadWord();

                        // Read body-section.
                        string section = m_pFetchReader.ReadParenthesized();

                        // Read origin if any.
                        int offset = -1;
                        if(m_pFetchReader.StartsWith("<")){
                            offset = Convert.ToInt32(m_pFetchReader.ReadParenthesized().Split(' ')[0]);
                        }


                        // Get Message store stream.
                        IMAP_Client_Fetch_Body_EArgs eArgs = new IMAP_Client_Fetch_Body_EArgs(section,offset);
                        m_pHandler.OnBody(eArgs);

                        // We don't have BODY[].
                        m_pFetchReader.ReadToFirstChar();
                        if(m_pFetchReader.StartsWith("NIL",false)){
                            // Eat NIL.
                            m_pFetchReader.ReadWord();
                        }
                        // BODY[] value is returned as string-literal.
                        else if(m_pFetchReader.StartsWith("{",false)){
                            if(eArgs.Stream == null){
                                m_pImap.ReadStringLiteral(Convert.ToInt32(m_pFetchReader.ReadParenthesized()),new JunkingStream());
                            }
                            else{
                                m_pImap.ReadStringLiteral(Convert.ToInt32(m_pFetchReader.ReadParenthesized()),eArgs.Stream);
                            }
                            
                            // Read continuing FETCH line.
                            m_pFetchReader = new StringReader(m_pImap.ReadLine());
                        }
                        // BODY[] is quoted-string.
                        else{
                            m_pFetchReader.ReadWord();
                        }

                        // Notify that message storing has completed.
                        eArgs.OnStoringCompleted();
                    }

                    #endregion
//*
                    #region BODYSTRUCTURE

                    else if(m_pFetchReader.StartsWith("BODYSTRUCTURE ",false)){
                    }

                    #endregion

                    #region ENVELOPE

                    else if(m_pFetchReader.StartsWith("ENVELOPE ",false)){
                        m_pHandler.OnEnvelope(IMAP_Envelope.Parse(this));
                    }

                    #endregion

                    #region  FLAGS

                    else if(m_pFetchReader.StartsWith("FLAGS ",false)){
                        // Eat FLAGS word.
                        m_pFetchReader.ReadWord();

                        string   flagsList = m_pFetchReader.ReadParenthesized();
                        string[] flags     = new string[0];
                        if(!string.IsNullOrEmpty(flagsList)){
                            flags = flagsList.Split(' ');
                        }

                        m_pHandler.OnFlags(flags);
                    }

                    #endregion

                    #region INTERNALDATE

                    else if(m_pFetchReader.StartsWith("INTERNALDATE ",false)){
                         // Eat INTERNALDATE word.
                        m_pFetchReader.ReadWord();

                        m_pHandler.OnInternalDate(IMAP_Utils.ParseDate(m_pFetchReader.ReadWord()));
                    }

                    #endregion

                    #region RFC822

                    else if(m_pFetchReader.StartsWith("RFC822 ",false)){
                        // Eat RFC822 word.
                        m_pFetchReader.ReadWord(false,new char[]{' '},false);
                        m_pFetchReader.ReadToFirstChar();

                        // Get Message store stream.
                        IMAP_Client_Fetch_Rfc822_EArgs eArgs = new IMAP_Client_Fetch_Rfc822_EArgs();
                        m_pHandler.OnRfc822(eArgs);

                        // We don't have RFC822.
                        if(m_pFetchReader.StartsWith("NIL",false)){
                            // Eat NIL.
                            m_pFetchReader.ReadWord();
                        }
                        // RFC822 value is returned as string-literal.
                        else if(m_pFetchReader.StartsWith("{",false)){
                            if(eArgs.Stream == null){
                                m_pImap.ReadStringLiteral(Convert.ToInt32(m_pFetchReader.ReadParenthesized()),new JunkingStream());
                            }
                            else{
                                m_pImap.ReadStringLiteral(Convert.ToInt32(m_pFetchReader.ReadParenthesized()),eArgs.Stream);
                            }
                            
                            // Read continuing FETCH line.
                            m_pFetchReader = new StringReader(m_pImap.ReadLine());
                        }
                        // RFC822 is quoted-string.
                        else{
                            m_pFetchReader.ReadWord();
                        }

                        // Notify that message storing has completed.
                        eArgs.OnStoringCompleted();
                    }

                    #endregion

                    #region RFC822.HEADER

                    else if(m_pFetchReader.StartsWith("RFC822.HEADER ",false)){
                        // Eat RFC822.HEADER word.
                        m_pFetchReader.ReadWord(false,new char[]{' '},false);
                        m_pFetchReader.ReadToFirstChar();
                        
                        string text = null;
                        // We don't have HEADER.
                        if(m_pFetchReader.StartsWith("NIL",false)){
                            // Eat NIL.
                            m_pFetchReader.ReadWord();

                            text = null;
                        }
                        // HEADER value is returned as string-literal.
                        else if(m_pFetchReader.StartsWith("{",false)){
                            text = m_pImap.ReadStringLiteral(Convert.ToInt32(m_pFetchReader.ReadParenthesized()));
                            
                            // Read continuing FETCH line.
                            m_pFetchReader = new StringReader(m_pImap.ReadLine());
                        }
                        // HEADER is quoted-string.
                        else{
                            text = m_pFetchReader.ReadWord();
                        }

                        m_pHandler.OnRfc822Header(text);
                    }

                    #endregion

                    #region RFC822.SIZE

                    else if(m_pFetchReader.StartsWith("RFC822.SIZE ",false)){
                        // Eat RFC822.SIZE word.
                        m_pFetchReader.ReadWord(false,new char[]{' '},false);

                        m_pHandler.OnSize(Convert.ToInt32(m_pFetchReader.ReadWord()));
                    }

                    #endregion

                    #region RFC822.TEXT

                    else if(m_pFetchReader.StartsWith("RFC822.TEXT ",false)){
                        // Eat RFC822.TEXT word.
                        m_pFetchReader.ReadWord(false,new char[]{' '},false);
                        m_pFetchReader.ReadToFirstChar();
                        
                        string text = null;
                        // We don't have TEXT.
                        if(m_pFetchReader.StartsWith("NIL",false)){
                            // Eat NIL.
                            m_pFetchReader.ReadWord();

                            text = null;
                        }
                        // TEXT value is returned as string-literal.
                        else if(m_pFetchReader.StartsWith("{",false)){
                            text = m_pImap.ReadStringLiteral(Convert.ToInt32(m_pFetchReader.ReadParenthesized()));
                            
                            // Read continuing FETCH line.
                            m_pFetchReader = new StringReader(m_pImap.ReadLine());
                        }
                        // TEXT is quoted-string.
                        else{
                            text = m_pFetchReader.ReadWord();
                        }

                        m_pHandler.OnRfc822Text(text);
                    }

                    #endregion

                    #region UID

                    else if(m_pFetchReader.StartsWith("UID ",false)){
                        // Eat UID word.
                        m_pFetchReader.ReadWord();

                        m_pHandler.OnUID(Convert.ToInt64(m_pFetchReader.ReadWord()));
                    }

                    #endregion

                    #region Fetch closing ")"

                    else if(m_pFetchReader.StartsWith(")",false)){
                        break;
                    }

                    #endregion

                    else{
                        throw new NotSupportedException("Not supported IMAP FETCH data-item '" + m_pFetchReader.ReadToEnd() + "'.");
                    }
                }
            }

            #endregion


            #region method GetReader

            /// <summary>
            /// Gets FETCH current line data reader.
            /// </summary>
            internal StringReader GetReader()
            {
                return m_pFetchReader;
            }

            #endregion

            #region method ReadString

            /// <summary>
            /// Reads string. Quoted-string-string-literal and NIL supported.
            /// </summary>
            /// <returns>Returns readed string.</returns>
            internal string ReadString()
            {                        
                m_pFetchReader.ReadToFirstChar();
                // NIL string.
                if(m_pFetchReader.StartsWith("NIL",false)){
                    m_pFetchReader.ReadWord();

                    return null;
                }
                // string-literal.
                else if(m_pFetchReader.StartsWith("{")){
                    string retVal = m_pImap.ReadStringLiteral(Convert.ToInt32(m_pFetchReader.ReadParenthesized()));

                    // Read continuing FETCH line.
                    m_pFetchReader = new StringReader(m_pImap.ReadLine());

                    return retVal;
                }
                // quoted-string or atom.
                else{
                    return MIME_Encoding_EncodedWord.DecodeS(m_pFetchReader.ReadWord());
                }
            }

            #endregion
        }

        #endregion

        private GenericIdentity            m_pAuthenticatedUser = null;
        private string                     m_GreetingText       = "";
        private int                        m_CommandIndex       = 1;
        private IMAP_Client_SelectedFolder m_pSelectedFolder    = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public IMAP_Client()
        {
        }


        #region override method Disconnect

		/// <summary>
		/// Closes connection to IMAP server.
		/// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected.</exception>
		public override void Disconnect()
		{
            if(this.IsDisposed){
                throw new ObjectDisposedException(this.GetType().Name);
            }
            if(!this.IsConnected){
                throw new InvalidOperationException("IMAP client is not connected.");
            }

			try{
                // Send LOGOUT command to server.                
                WriteLine((m_CommandIndex++).ToString("d5") + " LOGOUT");
			}
			catch{
			}

            try{
                base.Disconnect(); 
            }
            catch{
            }
		}

		#endregion


        #region method StartTls

        /// <summary>
        /// Switches connection to secure connection.
        /// </summary>
        /// <exception cref="InvalidOperationException">Is raised when connection is already secure and this method is called or
        /// when IMAP client is not in valid state(not-connected or not-authenticated state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public void StartTls()
        {
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(this.IsSecureConnection){
                throw new InvalidOperationException("Connection is already secure.");
            }
            if(this.IsAuthenticated){
                throw new InvalidOperationException("STARTTLS is only valid in not-authenticated state.");
            }

            /* RFC 3501 6.2.1. STARTTLS Command.

                Arguments:  none

                Responses:  no specific response for this command

                Result:     OK - starttls completed, begin TLS negotiation
                            BAD - command unknown or arguments invalid

                A [TLS] negotiation begins immediately after the CRLF at the end
                of the tagged OK response from the server.  Once a client issues a
                STARTTLS command, it MUST NOT issue further commands until a
                server response is seen and the [TLS] negotiation is complete.

                The server remains in the non-authenticated state, even if client
                credentials are supplied during the [TLS] negotiation.  This does
                not preclude an authentication mechanism such as EXTERNAL (defined
                in [SASL]) from using client identity determined by the [TLS]
                negotiation.

                Once [TLS] has been started, the client MUST discard cached
                information about server capabilities and SHOULD re-issue the
                CAPABILITY command.  This is necessary to protect against man-in-
                the-middle attacks which alter the capabilities list prior to
                STARTTLS.  The server MAY advertise d
            */

            SendCommand((m_CommandIndex++).ToString("d5") + " STARTTLS\r\n");

            IMAP_r_ServerStatus response = ReadResponse(null,null,null,null,null,null,null,null,null,null,null,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }

            SwitchToSecure();
        }

        #endregion

        #region method Login

        /// <summary>
        /// Authenticates user using IMAP-LOGIN method.
        /// </summary>
        /// <param name="user">User name.</param>
        /// <param name="password">Password.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>user</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="InvalidOperationException">Is raised when user is already authenticated and this method is called or
        /// when IMAP client is not in valid state(not-connected state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public void Login(string user,string password)
        {
            if(user == null){
                throw new ArgumentNullException("user");
            }
            if(user == string.Empty){
                throw new ArgumentException("Argument 'user' value must be specified.");
            }
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(this.IsAuthenticated){
                throw new InvalidOperationException("Re-authentication error, you are already authenticated.");
            }

            /* RFC 3501 6.2.3.  LOGIN Command

                Arguments:  user name
                            password

                Responses:  no specific responses for this command

                Result:     OK - login completed, now in authenticated state
                            NO - login failure: user name or password rejected
                            BAD - command unknown or arguments invalid

                The LOGIN command identifies the client to the server and carries
                the plaintext password authenticating this user.

                A server MAY include a CAPABILITY response code in the tagged OK
                response to a successful LOGIN command in order to send
                capabilities automatically.  It is unnecessary for a client to
                send a separate CAPABILITY command if it recognizes these
                automatic capabilities.
            */
            
            string cmd = (m_CommandIndex++).ToString("d5") + " LOGIN " + TextUtils.QuoteString(user) + " " + TextUtils.QuoteString(password) + "\r\n";      
            this.TcpStream.Write(cmd);
            // Log manually, remove password.
            LogAddWrite(cmd.Length,(m_CommandIndex - 1).ToString("d5") + " LOGIN " + TextUtils.QuoteString(user) + " <PASSWORD-REMOVED>");

            IMAP_r_ServerStatus response = ReadResponse(null,null,null,null,null,null,null,null,null,null,null,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }

            m_pAuthenticatedUser = new GenericIdentity(TextUtils.QuoteString(user),"IMAP-LOGIN");
        }

        #endregion
//*
        #region method Authenticate
        /*
        /// <summary>
        /// Authenticates user.
        /// </summary>
        /// <param name="user">User name.</param>
        /// <param name="password">User password.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>user</b> or <b>password</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public void Authenticate(string user,string password)
        {
            if(user == null){
                throw new ArgumentNullException("user");
            }
            if(user == string.Empty){
                throw new ArgumentException("Argument 'user' value must be specified.","user");
            }
            if(password == null){
                throw new ArgumentNullException("password");
            }

            // TODO:
            throw new NotImplementedException();
        }*/

        #endregion


        #region method GetNamespaces

        /// <summary>
        /// Gets IMAP server namespaces.
        /// </summary>
        /// <returns>Returns namespaces responses.</returns>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected or not-authenticated state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public IMAP_r_u_Namespace[] GetNamespaces()
        {           
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }

            /* RFC 2342 5. NAMESPACE Command.
                Arguments: none

                Response:  an untagged NAMESPACE response that contains the prefix
                           and hierarchy delimiter to the server's Personal
                           Namespace(s), Other Users' Namespace(s), and Shared
                           Namespace(s) that the server wishes to expose. The
                           response will contain a NIL for any namespace class
                           that is not available. Namespace_Response_Extensions
                           MAY be included in the response.
                           Namespace_Response_Extensions which are not on the IETF
                           standards track, MUST be prefixed with an "X-".

                Result:    OK - Command completed
                           NO - Error: Can't complete command
                           BAD - argument invalid
                
                Example:
                    < A server that contains a Personal Namespace and a single Shared Namespace. >

                    C: A001 NAMESPACE
                    S: * NAMESPACE (("" "/")) NIL (("Public Folders/" "/"))
                    S: A001 OK NAMESPACE command completed
            */

            SendCommand((m_CommandIndex++).ToString("d5") + " NAMESPACE\r\n");

            List<IMAP_r_u_Namespace> retVal = new List<IMAP_r_u_Namespace>();
            IMAP_r_ServerStatus response = ReadResponse(null,null,null,null,null,null,null,null,null,null,null,retVal,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }

            return retVal.ToArray();
        }

        #endregion

        #region method GetFolders

        /// <summary>
        /// Gets folders list.
        /// </summary>
        /// <param name="filter">Folders filter. If this value is null, all folders are returned.</param>
        /// <returns>Returns folders list.</returns>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected or not-authenticated state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        /// <remarks>
        /// The character "*" is a wildcard, and matches zero or more
        /// characters at this position.  The character "%" is similar to "*",
        /// but it does not match a hierarchy delimiter.  If the "%" wildcard
        /// is the last character of a mailbox name argument, matching levels
        /// of hierarchy are also returned.
        /// </remarks>
        public IMAP_r_u_List[] GetFolders(string filter)
        {
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }

            /* RFC 3501 6.3.8. LIST Command.
                Arguments:  reference name
                            mailbox name with possible wildcards

                Responses:  untagged responses: LIST

                Result:     OK - list completed
                            NO - list failure: can't list that reference or name
                            BAD - command unknown or arguments invalid

                The LIST command returns a subset of names from the complete set
                of all names available to the client.  Zero or more untagged LIST
                replies are returned, containing the name attributes, hierarchy
                delimiter, and name; see the description of the LIST reply for
                more detail.

                An empty ("" string) reference name argument indicates that the
                mailbox name is interpreted as by SELECT.  The returned mailbox
                names MUST match the supplied mailbox name pattern.  A non-empty
                reference name argument is the name of a mailbox or a level of
                mailbox hierarchy, and indicates the context in which the mailbox
                name is interpreted.

                An empty ("" string) mailbox name argument is a special request to
                return the hierarchy delimiter and the root name of the name given
                in the reference.  The value returned as the root MAY be the empty
                string if the reference is non-rooted or is an empty string.  In
                all cases, a hierarchy delimiter (or NIL if there is no hierarchy)
                is returned.  This permits a client to get the hierarchy delimiter
                (or find out that the mailbox names are flat) even when no
                mailboxes by that name currently exist.

                The reference and mailbox name arguments are interpreted into a
                canonical form that represents an unambiguous left-to-right
                hierarchy.  The returned mailbox names will be in the interpreted
                form.

                Note: The interpretation of the reference argument is
                implementation-defined.  It depends upon whether the
                server implementation has a concept of the "current
                working directory" and leading "break out characters",
                which override the current working directory.

                For example, on a server which exports a UNIX or NT
                filesystem, the reference argument contains the current
                working directory, and the mailbox name argument would
                contain the name as interpreted in the current working
                directory.

                If a server implementation has no concept of break out
                characters, the canonical form is normally the reference
                name appended with the mailbox name.  Note that if the
                server implements the namespace convention (section
                5.1.2), "#" is a break out character and must be treated
                as such.

                If the reference argument is not a level of mailbox
                hierarchy (that is, it is a \NoInferiors name), and/or
                the reference argument does not end with the hierarchy
                delimiter, it is implementation-dependent how this is
                interpreted.  For example, a reference of "foo/bar" and
                mailbox name of "rag/baz" could be interpreted as
                "foo/bar/rag/baz", "foo/barrag/baz", or "foo/rag/baz".
                A client SHOULD NOT use such a reference argument except
                at the explicit request of the user.  A hierarchical
                browser MUST NOT make any assumptions about server
                interpretation of the reference unless the reference is
                a level of mailbox hierarchy AND ends with the hierarchy
                delimiter.

                Any part of the reference argument that is included in the
                interpreted form SHOULD prefix the interpreted form.  It SHOULD
                also be in the same form as the reference name argument.  This
                rule permits the client to determine if the returned mailbox name
                is in the context of the reference argument, or if something about
                the mailbox argument overrode the reference argument.  Without
                this rule, the client would have to have knowledge of the server's
                naming semantics including what characters are "breakouts" that
                override a naming context.  

                    For example, here are some examples of how references
                    and mailbox names might be interpreted on a UNIX-based
                    server:

                        Reference     Mailbox Name  Interpretation
                        ------------  ------------  --------------
                        ~smith/Mail/  foo.*         ~smith/Mail/foo.*
                        archive/      %             archive/%
                        #news.        comp.mail.*   #news.comp.mail.*
                        ~smith/Mail/  /usr/doc/foo  /usr/doc/foo
                        archive/      ~fred/Mail/*  ~fred/Mail/*

                    The first three examples demonstrate interpretations in
                    the context of the reference argument.  Note that
                    "~smith/Mail" SHOULD NOT be transformed into something
                    like "/u2/users/smith/Mail", or it would be impossible
                    for the client to determine that the interpretation was
                    in the context of the reference.

            The character "*" is a wildcard, and matches zero or more
            characters at this position.  The character "%" is similar to "*",
            but it does not match a hierarchy delimiter.  If the "%" wildcard
            is the last character of a mailbox name argument, matching levels
            of hierarchy are also returned.  If these levels of hierarchy are
            not also selectable mailboxes, they are returned with the
            \Noselect mailbox name attribute (see the description of the LIST
            response for more details).

            The special name INBOX is included in the output from LIST, if
            INBOX is supported by this server for this user and if the
            uppercase string "INBOX" matches the interpreted reference and
            mailbox name arguments with wildcards as described above.  The
            criteria for omitting INBOX is whether SELECT INBOX will return
            failure; it is not relevant whether the user's real INBOX resides
            on this or some other server.

            Example:    C: A101 LIST "" ""
                        S: * LIST (\Noselect) "/" ""
                        S: A101 OK LIST Completed
                        C: A102 LIST #news.comp.mail.misc ""
                        S: * LIST (\Noselect) "." #news.
                        S: A102 OK LIST Completed
                        C: A103 LIST /usr/staff/jones ""
                        S: * LIST (\Noselect) "/" /
                        S: A103 OK LIST Completed
                        C: A202 LIST ~/Mail/ %
                        S: * LIST (\Noselect) "/" ~/Mail/foo
                        S: * LIST () "/" ~/Mail/meetings
                        S: A202 OK LIST completed
            */
            
            if(filter != null){
                SendCommand((m_CommandIndex++).ToString("d5") + " LIST \"\" " + TextUtils.QuoteString(IMAP_Utils.Encode_IMAP_UTF7_String(filter)) + "\r\n");
            }
            else{
                SendCommand((m_CommandIndex++).ToString("d5") + " LIST \"\" \"*\"\r\n");
            }
            
            List<IMAP_r_u_List> retVal = new List<IMAP_r_u_List>();
            IMAP_r_ServerStatus response = ReadResponse(null,null,null,retVal,null,null,null,null,null,null,null,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }

            return retVal.ToArray();
        }

        #endregion

        #region method CreateFolder

        /// <summary>
        /// Creates new folder.
        /// </summary>
        /// <param name="folder">Folder name with path.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>folder</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected or not-authenticated state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public void CreateFolder(string folder)
        {
            if(folder == null){
                throw new ArgumentNullException("folder");
            }
            if(folder == string.Empty){
                throw new ArgumentException("Argument 'folder' value must be specified.","folder");
            }
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }

            /* RFC 3501 6.3.3. CREATE Command.
                Arguments:  mailbox name

                Responses:  no specific responses for this command

                Result:     OK - create completed
                            NO - create failure: can't create mailbox with that name
                            BAD - command unknown or arguments invalid

                The CREATE command creates a mailbox with the given name.  An OK
                response is returned only if a new mailbox with that name has been
                created.  It is an error to attempt to create INBOX or a mailbox
                with a name that refers to an extant mailbox.  Any error in
                creation will return a tagged NO response.

                If the mailbox name is suffixed with the server's hierarchy
                separator character (as returned from the server by a LIST
                command), this is a declaration that the client intends to create
                mailbox names under this name in the hierarchy.  Server
                implementations that do not require this declaration MUST ignore
                the declaration.  In any case, the name created is without the
                trailing hierarchy delimiter.

                If the server's hierarchy separator character appears elsewhere in
                the name, the server SHOULD create any superior hierarchical names
                that are needed for the CREATE command to be successfully
                completed.  In other words, an attempt to create "foo/bar/zap" on
                a server in which "/" is the hierarchy separator character SHOULD
                create foo/ and foo/bar/ if they do not already exist.

                If a new mailbox is created with the same name as a mailbox which
                was deleted, its unique identifiers MUST be greater than any
                unique identifiers used in the previous incarnation of the mailbox
                UNLESS the new incarnation has a different unique identifier
                validity value.  See the description of the UID command for more
                detail.

                Example:    C: A003 CREATE owatagusiam/
                            S: A003 OK CREATE completed
                            C: A004 CREATE owatagusiam/blurdybloop
                            S: A004 OK CREATE completed

                    Note: The interpretation of this example depends on whether
                    "/" was returned as the hierarchy separator from LIST.  If
                    "/" is the hierarchy separator, a new level of hierarchy
                    named "owatagusiam" with a member called "blurdybloop" is
                    created.  Otherwise, two mailboxes at the same hierarchy
                    level are created.
            */

            SendCommand((m_CommandIndex++).ToString("d5") + " CREATE " + TextUtils.QuoteString(IMAP_Utils.Encode_IMAP_UTF7_String(folder)) + "\r\n");

            IMAP_r_ServerStatus response = ReadResponse(null,null,null,null,null,null,null,null,null,null,null,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }
        }

        #endregion

        #region method DeleteFolder

        /// <summary>
        /// Deletes specified folder.
        /// </summary>
        /// <param name="folder">Folder name with path.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>folder</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected or not-authenticated state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public void DeleteFolder(string folder)
        {
            if(folder == null){
                throw new ArgumentNullException("folder");
            }
            if(folder == string.Empty){
                throw new ArgumentException("Argument 'folder' value must be specified.","folder");
            }
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }

            /* RFC 3501 6.3.4. DELETE Command.
                Arguments:  mailbox name

                Responses:  no specific responses for this command

                Result:     OK - delete completed
                            NO - delete failure: can't delete mailbox with that name
                            BAD - command unknown or arguments invalid

                The DELETE command permanently removes the mailbox with the given
                name.  A tagged OK response is returned only if the mailbox has
                been deleted.  It is an error to attempt to delete INBOX or a
                mailbox name that does not exist.

                The DELETE command MUST NOT remove inferior hierarchical names.
                For example, if a mailbox "foo" has an inferior "foo.bar"
                (assuming "." is the hierarchy delimiter character), removing
                "foo" MUST NOT remove "foo.bar".  It is an error to attempt to
                delete a name that has inferior hierarchical names and also has
                the \Noselect mailbox name attribute (see the description of the
                LIST response for more details).

                It is permitted to delete a name that has inferior hierarchical
                names and does not have the \Noselect mailbox name attribute.  In
                this case, all messages in that mailbox are removed, and the name
                will acquire the \Noselect mailbox name attribute.

                The value of the highest-used unique identifier of the deleted
                mailbox MUST be preserved so that a new mailbox created with the
                same name will not reuse the identifiers of the former
                incarnation, UNLESS the new incarnation has a different unique
                identifier validity value.  See the description of the UID command
                for more detail.

                Examples:   C: A682 LIST "" *
                            S: * LIST () "/" blurdybloop
                            S: * LIST (\Noselect) "/" foo
                            S: * LIST () "/" foo/bar
                            S: A682 OK LIST completed
                            C: A683 DELETE blurdybloop
                            S: A683 OK DELETE completed
                            C: A684 DELETE foo
                            S: A684 NO Name "foo" has inferior hierarchical names
                            C: A685 DELETE foo/bar
                            S: A685 OK DELETE Completed
                            C: A686 LIST "" *
                            S: * LIST (\Noselect) "/" foo
                            S: A686 OK LIST completed
                            C: A687 DELETE foo
                            S: A687 OK DELETE Completed
            */

            SendCommand((m_CommandIndex++).ToString("d5") + " DELETE " + TextUtils.QuoteString(IMAP_Utils.Encode_IMAP_UTF7_String(folder)) + "\r\n");

            IMAP_r_ServerStatus response = ReadResponse(null,null,null,null,null,null,null,null,null,null,null,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }
        }

        #endregion

        #region method RenameFolder

        /// <summary>
        /// Renames exisiting folder name.
        /// </summary>
        /// <param name="folder">Folder name with path to rename.</param>
        /// <param name="newFolder">New folder name with path.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>folder</b> or <b>newFolder</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected or not-authenticated state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public void RenameFolder(string folder,string newFolder)
        {
            if(folder == null){
                throw new ArgumentNullException("folder");
            }
            if(folder == string.Empty){
                throw new ArgumentException("Argument 'folder' name must be specified.","folder");
            }
            if(newFolder == null){
                throw new ArgumentNullException("newFolder");
            }
            if(newFolder == string.Empty){
                throw new ArgumentException("Argument 'newFolder' name must be specified.","newFolder");
            }
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }

            /* RFC 3501 6.3.5. RENAME Command.
                Arguments:  existing mailbox name
                            new mailbox name

                Responses:  no specific responses for this command

                Result:     OK - rename completed
                            NO - rename failure: can't rename mailbox with that name,
                                 can't rename to mailbox with that name
                            BAD - command unknown or arguments invalid

                The RENAME command changes the name of a mailbox.  A tagged OK
                response is returned only if the mailbox has been renamed.  It is
                an error to attempt to rename from a mailbox name that does not
                exist or to a mailbox name that already exists.  Any error in
                renaming will return a tagged NO response.

                If the name has inferior hierarchical names, then the inferior
                hierarchical names MUST also be renamed.  For example, a rename of
                "foo" to "zap" will rename "foo/bar" (assuming "/" is the
                hierarchy delimiter character) to "zap/bar".

                If the server's hierarchy separator character appears in the name,
                the server SHOULD create any superior hierarchical names that are
                needed for the RENAME command to complete successfully.  In other
                words, an attempt to rename "foo/bar/zap" to baz/rag/zowie on a
                server in which "/" is the hierarchy separator character SHOULD
                create baz/ and baz/rag/ if they do not already exist.

                The value of the highest-used unique identifier of the old mailbox
                name MUST be preserved so that a new mailbox created with the same
                name will not reuse the identifiers of the former incarnation,
                UNLESS the new incarnation has a different unique identifier
                validity value.  See the description of the UID command for more
                detail.

                Renaming INBOX is permitted, and has special behavior.  It moves
                all messages in INBOX to a new mailbox with the given name,
                leaving INBOX empty.  If the server implementation supports
                inferior hierarchical names of INBOX, these are unaffected by a
                rename of INBOX.

                Examples:   C: A682 LIST "" *
                            S: * LIST () "/" blurdybloop
                            S: * LIST (\Noselect) "/" foo
                            S: * LIST () "/" foo/bar
                            S: A682 OK LIST completed
                            C: A683 RENAME blurdybloop sarasoop
                            S: A683 OK RENAME completed
                            C: A684 RENAME foo zowie
                            S: A684 OK RENAME Completed
                            C: A685 LIST "" *
                            S: * LIST () "/" sarasoop
                            S: * LIST (\Noselect) "/" zowie
                            S: * LIST () "/" zowie/bar
                            S: A685 OK LIST completed
            */

            SendCommand((m_CommandIndex++).ToString("d5") + " RENAME " + TextUtils.QuoteString(IMAP_Utils.Encode_IMAP_UTF7_String(folder)) + " " + TextUtils.QuoteString(IMAP_Utils.Encode_IMAP_UTF7_String(newFolder)) + "\r\n");

            IMAP_r_ServerStatus response = ReadResponse(null,null,null,null,null,null,null,null,null,null,null,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }
        }

        #endregion

        #region method GetSubscribedFolders

        /// <summary>
        /// Get user subscribed folders list.
        /// </summary>
        /// <param name="filter">Folders filter. If this value is null, all folders are returned.</param>
        /// <returns>Returns subscribed folders list.</returns>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected or not-authenticated state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        /// <remarks>
        /// The character "*" is a wildcard, and matches zero or more
        /// characters at this position.  The character "%" is similar to "*",
        /// but it does not match a hierarchy delimiter.  If the "%" wildcard
        /// is the last character of a mailbox name argument, matching levels
        /// of hierarchy are also returned.
        /// </remarks>
        public IMAP_r_u_LSub[] GetSubscribedFolders(string filter)
        {            
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }

            /* RFC 3501 6.3.9. LSUB Command.
                Arguments:  reference name
                            mailbox name with possible wildcards

                Responses:  untagged responses: LSUB

                Result:     OK - lsub completed
                            NO - lsub failure: can't list that reference or name
                            BAD - command unknown or arguments invalid

                The LSUB command returns a subset of names from the set of names
                that the user has declared as being "active" or "subscribed".
                Zero or more untagged LSUB replies are returned.  The arguments to
                LSUB are in the same form as those for LIST.

                The returned untagged LSUB response MAY contain different mailbox
                flags from a LIST untagged response.  If this should happen, the
                flags in the untagged LIST are considered more authoritative.

                A special situation occurs when using LSUB with the % wildcard.
                Consider what happens if "foo/bar" (with a hierarchy delimiter of
                "/") is subscribed but "foo" is not.  A "%" wildcard to LSUB must
                return foo, not foo/bar, in the LSUB response, and it MUST be
                flagged with the \Noselect attribute.

                The server MUST NOT unilaterally remove an existing mailbox name
                from the subscription list even if a mailbox by that name no
                longer exists.

                Example:    C: A002 LSUB "#news." "comp.mail.*"
                            S: * LSUB () "." #news.comp.mail.mime
                            S: * LSUB () "." #news.comp.mail.misc
                            S: A002 OK LSUB completed
                            C: A003 LSUB "#news." "comp.%"
                            S: * LSUB (\NoSelect) "." #news.comp.mail
                            S: A003 OK LSUB completed
            */

            if(filter != null){
                SendCommand((m_CommandIndex++).ToString("d5") + " LSUB \"\" " + TextUtils.QuoteString(IMAP_Utils.Encode_IMAP_UTF7_String(filter)) + "\r\n");
            }
            else{
                SendCommand((m_CommandIndex++).ToString("d5") + " LSUB \"\" \"*\"\r\n");
            }
            
            List<IMAP_r_u_LSub> retVal  = new List<IMAP_r_u_LSub>();
            IMAP_r_ServerStatus response = ReadResponse(null,null,null,null,retVal,null,null,null,null,null,null,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }

            return retVal.ToArray();
        }

        #endregion

        #region method SubscribeFolder

        /// <summary>
        /// Subscribes specified folder.
        /// </summary>
        /// <param name="folder">Foler name with path.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>folder</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected or not-authenticated state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public void SubscribeFolder(string folder)
        {
            if(folder == null){
                throw new ArgumentNullException("folder");
            }
            if(folder == string.Empty){
                throw new ArgumentException("Argument 'folder' value must be specified.","folder");
            }
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }

            /* RFC 3501 6.3.6. SUBSCRIBE Command.
                Arguments:  mailbox

                Responses:  no specific responses for this command

                Result:     OK - subscribe completed
                            NO - subscribe failure: can't subscribe to that name
                            BAD - command unknown or arguments invalid

                The SUBSCRIBE command adds the specified mailbox name to the
                server's set of "active" or "subscribed" mailboxes as returned by
                the LSUB command.  This command returns a tagged OK response only
                if the subscription is successful.

                A server MAY validate the mailbox argument to SUBSCRIBE to verify
                that it exists.  However, it MUST NOT unilaterally remove an
                existing mailbox name from the subscription list even if a mailbox
                by that name no longer exists.

                    Note: This requirement is because a server site can
                    choose to routinely remove a mailbox with a well-known
                    name (e.g., "system-alerts") after its contents expire,
                    with the intention of recreating it when new contents
                    are appropriate.


                Example:    C: A002 SUBSCRIBE #news.comp.mail.mime
                            S: A002 OK SUBSCRIBE completed
            */

            SendCommand((m_CommandIndex++).ToString("d5") + " SUBSCRIBE " + TextUtils.QuoteString(IMAP_Utils.Encode_IMAP_UTF7_String(folder)) + "\r\n");

            IMAP_r_ServerStatus response = ReadResponse(null,null,null,null,null,null,null,null,null,null,null,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }
        }

        #endregion

        #region method UnsubscribeFolder

        /// <summary>
        /// Unsubscribes specified folder.
        /// </summary>
        /// <param name="folder">Foler name with path.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>folder</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected or not-authenticated state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public void UnsubscribeFolder(string folder)
        {
            if(folder == null){
                throw new ArgumentNullException("folder");
            }
            if(folder == string.Empty){
                throw new ArgumentException("Argument 'folder' value must be specified.","folder");
            }
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }

            /* RFC 3501 6.3.7. UNSUBSCRIBE Command.
                Arguments:  mailbox name

                Responses:  no specific responses for this command

                Result:     OK - unsubscribe completed
                            NO - unsubscribe failure: can't unsubscribe that name
                            BAD - command unknown or arguments invalid

                The UNSUBSCRIBE command removes the specified mailbox name from
                the server's set of "active" or "subscribed" mailboxes as returned
                by the LSUB command.  This command returns a tagged OK response
                only if the unsubscription is successful.

                Example:    C: A002 UNSUBSCRIBE #news.comp.mail.mime
                            S: A002 OK UNSUBSCRIBE completed
            */

            SendCommand((m_CommandIndex++).ToString("d5") + " UNSUBSCRIBE " + TextUtils.QuoteString(IMAP_Utils.Encode_IMAP_UTF7_String(folder)) + "\r\n");

            IMAP_r_ServerStatus response = ReadResponse(null,null,null,null,null,null,null,null,null,null,null,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }
        }

        #endregion

        #region method FolderStatus

        /// <summary>
        /// Gets the specified folder status.
        /// </summary>
        /// <param name="folder">Folder name with path.</param>
        /// <returns>Returns STATUS responses.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>folder</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected or not-authenticated state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public IMAP_r_u_Status[] FolderStatus(string folder)
        {
            if(folder == null){
                throw new ArgumentNullException("folder");
            }
            if(folder == string.Empty){
                throw new ArgumentException("Argument 'folder' value must be specified.","folder");
            }
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }

            /* RFC 3501 6.3.10. STATUS Command.
                Arguments:  mailbox name
                            status data item names

                Responses:  untagged responses: STATUS

                Result:     OK - status completed
                            NO - status failure: no status for that name
                            BAD - command unknown or arguments invalid

                The STATUS command requests the status of the indicated mailbox.
                It does not change the currently selected mailbox, nor does it
                affect the state of any messages in the queried mailbox (in
                particular, STATUS MUST NOT cause messages to lose the \Recent
                flag).

                The STATUS command provides an alternative to opening a second
                IMAP4rev1 connection and doing an EXAMINE command on a mailbox to
                query that mailbox's status without deselecting the current
                mailbox in the first IMAP4rev1 connection.

                Unlike the LIST command, the STATUS command is not guaranteed to
                be fast in its response.  Under certain circumstances, it can be
                quite slow.  In some implementations, the server is obliged to
                open the mailbox read-only internally to obtain certain status
                information.  Also unlike the LIST command, the STATUS command
                does not accept wildcards.

                Note: The STATUS command is intended to access the
                status of mailboxes other than the currently selected
                mailbox.  Because the STATUS command can cause the
                mailbox to be opened internally, and because this
                information is available by other means on the selected
                mailbox, the STATUS command SHOULD NOT be used on the
                currently selected mailbox.

                The STATUS command MUST NOT be used as a "check for new
                messages in the selected mailbox" operation (refer to
                sections 7, 7.3.1, and 7.3.2 for more information about
                the proper method for new message checking).

                Because the STATUS command is not guaranteed to be fast
                in its results, clients SHOULD NOT expect to be able to
                issue many consecutive STATUS commands and obtain
                reasonable performance.

                The currently defined status data items that can be requested are:

                MESSAGES
                    The number of messages in the mailbox.

                RECENT
                    The number of messages with the \Recent flag set.

                UIDNEXT
                    The next unique identifier value of the mailbox.  Refer to
                    section 2.3.1.1 for more information.

                UIDVALIDITY
                    The unique identifier validity value of the mailbox.  Refer to
                    section 2.3.1.1 for more information.

                UNSEEN
                    The number of messages which do not have the \Seen flag set.


                Example:    C: A042 STATUS blurdybloop (UIDNEXT MESSAGES)
                            S: * STATUS blurdybloop (MESSAGES 231 UIDNEXT 44292)
                            S: A042 OK STATUS completed
            */

            SendCommand((m_CommandIndex++).ToString("d5") + " STATUS " + TextUtils.QuoteString(IMAP_Utils.Encode_IMAP_UTF7_String(folder)) + " (MESSAGES RECENT UIDNEXT UIDVALIDITY UNSEEN)\r\n");

            List<IMAP_r_u_Status> retVal  = new List<IMAP_r_u_Status>();
            IMAP_r_ServerStatus response = ReadResponse(null,null,null,null,null,null,null,null,retVal,null,null,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }

            return retVal.ToArray();
        }

        #endregion

        #region method SelectFolder

        /// <summary>
        /// Selects specified folder.
        /// </summary>
        /// <param name="folder">Folder name with path.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>folder</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected or not-authenticated state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public void SelectFolder(string folder)
        {
            if(folder == null){
                throw new ArgumentNullException("folder");
            }
            if(folder == string.Empty){
                throw new ArgumentException("Argument 'folder' value must be specified.","folder");
            }
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }

            /* RFC 3501 6.3.1.  SELECT Command.
                Arguments:  mailbox name

                Responses:  REQUIRED untagged responses: FLAGS, EXISTS, RECENT
                            REQUIRED OK untagged responses:  UNSEEN,  PERMANENTFLAGS,
                            UIDNEXT, UIDVALIDITY

                Result:     OK - select completed, now in selected state
                            NO - select failure, now in authenticated state: no
                                 such mailbox, can't access mailbox
                            BAD - command unknown or arguments invalid

                The SELECT command selects a mailbox so that messages in the
                mailbox can be accessed.  Before returning an OK to the client,
                the server MUST send the following untagged data to the client.
                Note that earlier versions of this protocol only required the
                FLAGS, EXISTS, and RECENT untagged data; consequently, client
                implementations SHOULD implement default behavior for missing data
                as discussed with the individual item.

                    FLAGS       Defined flags in the mailbox.  See the description
                                of the FLAGS response for more detail.

                    <n> EXISTS  The number of messages in the mailbox.  See the
                                description of the EXISTS response for more detail.

                    <n> RECENT  The number of messages with the \Recent flag set.
                                See the description of the RECENT response for more
                                detail.

                    OK [UNSEEN <n>]
                                The message sequence number of the first unseen
                                message in the mailbox.  If this is missing, the
                                client can not make any assumptions about the first
                                unseen message in the mailbox, and needs to issue a
                                SEARCH command if it wants to find it.

                    OK [PERMANENTFLAGS (<list of flags>)]
                                A list of message flags that the client can change
                                permanently.  If this is missing, the client should
                                assume that all flags can be changed permanently.

                    OK [UIDNEXT <n>]
                                The next unique identifier value.  Refer to section
                                2.3.1.1 for more information.  If this is missing,
                                the client can not make any assumptions about the
                                next unique identifier value.

                    OK [UIDVALIDITY <n>]
                            The unique identifier validity value.  Refer to
                            section 2.3.1.1 for more information.  If this is
                            missing, the server does not support unique
                            identifiers.

                Only one mailbox can be selected at a time in a connection;
                simultaneous access to multiple mailboxes requires multiple
                connections.  The SELECT command automatically deselects any
                currently selected mailbox before attempting the new selection.
                Consequently, if a mailbox is selected and a SELECT command that
                fails is attempted, no mailbox is selected.
            */

            // Close open folder.
            m_pSelectedFolder = null;
                        
            SendCommand((m_CommandIndex++).ToString("d5") + " SELECT " + TextUtils.QuoteString(IMAP_Utils.Encode_IMAP_UTF7_String(folder)) + "\r\n");

            IMAP_Client_SelectedFolder folderInfo = new IMAP_Client_SelectedFolder(folder);

            IMAP_r_ServerStatus response = ReadResponse(null,folderInfo,null,null,null,null,null,null,null,null,null,null,null);
            if(response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                m_pSelectedFolder = folderInfo;
 
                // Mark folder as read-only if optional response code "READ-ONLY" specified.
                if(response.OptionalResponseCode != null && response.OptionalResponseCode.Equals("READ-ONLY",StringComparison.InvariantCultureIgnoreCase)){
                    m_pSelectedFolder.SetReadOnly(true);
                }
            }
            else{
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }
        }

        #endregion

        #region method ExamineFolder

        /// <summary>
        /// Selects folder as read-only, no changes to messages or flags not possible.
        /// </summary>
        /// <param name="folder">Folder name with path.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>folder</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected or not-authenticated state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public void ExamineFolder(string folder)
        {
            if(folder == null){
                throw new ArgumentNullException("folder");
            }
            if(folder == string.Empty){
                throw new ArgumentException("Argument 'folder' value must be specified.","folder");
            }
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }

            /* RFC 3501 6.3.2.  EXAMINE Command.
                Arguments:  mailbox name

                Responses:  REQUIRED untagged responses: FLAGS, EXISTS, RECENT
                            REQUIRED OK untagged responses:  UNSEEN,  PERMANENTFLAGS,
                            UIDNEXT, UIDVALIDITY

                Result:     OK - examine completed, now in selected state
                            NO - examine failure, now in authenticated state: no
                                 such mailbox, can't access mailbox
                            BAD - command unknown or arguments invalid

                The EXAMINE command is identical to SELECT and returns the same
                output; however, the selected mailbox is identified as read-only.
                No changes to the permanent state of the mailbox, including
                per-user state, are permitted; in particular, EXAMINE MUST NOT
                cause messages to lose the \Recent flag.

                The text of the tagged OK response to the EXAMINE command MUST
                begin with the "[READ-ONLY]" response code.
            */

            // Close open folder.
            m_pSelectedFolder = null;
                        
            SendCommand((m_CommandIndex++).ToString("d5") + " EXAMINE " + TextUtils.QuoteString(IMAP_Utils.Encode_IMAP_UTF7_String(folder)) + "\r\n");

            IMAP_Client_SelectedFolder folderInfo = new IMAP_Client_SelectedFolder(folder);

            IMAP_r_ServerStatus response = ReadResponse(null,folderInfo,null,null,null,null,null,null,null,null,null,null,null);
            if(response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                m_pSelectedFolder = folderInfo;
 
                // Mark folder as read-only if optional response code "READ-ONLY" specified.
                if(response.OptionalResponseCode != null && response.OptionalResponseCode.Equals("READ-ONLY",StringComparison.InvariantCultureIgnoreCase)){
                    m_pSelectedFolder.SetReadOnly(true);
                }
            }
            else{
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }
        }

        #endregion

        #region method GetFolderQuotaRoots

        /// <summary>
        /// Gets specified folder quota roots and their quota resource usage.
        /// </summary>
        /// <param name="folder">Folder name with path.</param>
        /// <returns>Returns quota-roots and their resource limit entries.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>folder</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected or not-authenticated state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public IMAP_r[] GetFolderQuotaRoots(string folder)
        {
            if(folder == null){
                throw new ArgumentNullException("folder");
            }
            if(folder == string.Empty){
                throw new ArgumentException("Argument 'folder' value must be specified.","folder");
            }
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }

            /* RFC 2087 4.3. GETQUOTAROOT Command.
                Arguments:  mailbox name

                Data:       untagged responses: QUOTAROOT, QUOTA

                Result:     OK - getquota completed
                            NO - getquota error: no such mailbox, permission denied
                            BAD - command unknown or arguments invalid

                The GETQUOTAROOT command takes the name of a mailbox and returns the
                list of quota roots for the mailbox in an untagged QUOTAROOT
                response.  For each listed quota root, it also returns the quota
                root's resource usage and limits in an untagged QUOTA response.

                Example:    C: A003 GETQUOTAROOT INBOX
                            S: * QUOTAROOT INBOX ""
                            S: * QUOTA "" (STORAGE 10 512)
                            S: A003 OK Getquota completed
            */

            SendCommand((m_CommandIndex++).ToString("d5") + " GETQUOTAROOT " + TextUtils.QuoteString(IMAP_Utils.Encode_IMAP_UTF7_String(folder)) + "\r\n");
            
            List<IMAP_r_u_Quota> quota = new List<IMAP_r_u_Quota>();
            List<IMAP_r_u_QuotaRoot> quotaRoot = new List<IMAP_r_u_QuotaRoot>();
            IMAP_r_ServerStatus response = ReadResponse(null,null,null,null,null,null,null,null,null,quota,quotaRoot,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }

            List<IMAP_r> retVal = new List<IMAP_r>();
            retVal.AddRange(quotaRoot.ToArray());
            retVal.AddRange(quota.ToArray());

            return retVal.ToArray();
        }

        #endregion

        #region method GetFolderQuota

        /// <summary>
        /// Gets the specified folder quota-root resource limit entries.
        /// </summary>
        /// <param name="quotaRootName">Quota root name.</param>
        /// <returns>Returns quota-root resource limit entries.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>quotaRootName</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected or not-authenticated state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public IMAP_r_u_Quota[] GetFolderQuota(string quotaRootName)
        {
            if(quotaRootName == null){
                throw new ArgumentNullException("quotaRootName");
            }
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }

            /* RFC 2087 4.2. GETQUOTA Command.
                Arguments:  quota root

                Data:       untagged responses: QUOTA
    
                Result:     OK - getquota completed
                            NO - getquota  error:  no  such  quota  root,  permission denied
                            BAD - command unknown or arguments invalid
                
                The GETQUOTA command takes the name of a quota root and returns the
                quota root's resource usage and limits in an untagged QUOTA response.

                Example:    C: A003 GETQUOTA ""
                            S: * QUOTA "" (STORAGE 10 512)
                            S: A003 OK Getquota completed
            */

            SendCommand((m_CommandIndex++).ToString("d5") + " GETQUOTA " + TextUtils.QuoteString(IMAP_Utils.Encode_IMAP_UTF7_String(quotaRootName)) +"\r\n");

            List<IMAP_r_u_Quota> retVal  = new List<IMAP_r_u_Quota>();
            IMAP_r_ServerStatus response = ReadResponse(null,null,null,null,null,null,null,null,null,retVal,null,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }

            return retVal.ToArray();
        }

        #endregion                
//*
        #region method SetQuota

        private void SetQuota()
        {
            /* RFC 2087 4.1. SETQUOTA Command.
                Arguments:  quota root
                            list of resource limits

                Data:       untagged responses: QUOTA

                Result:     OK - setquota completed
                            NO - setquota error: can't set that data
                            BAD - command unknown or arguments invalid

                The SETQUOTA command takes the name of a mailbox quota root and a
                list of resource limits. The resource limits for the named quota root
                are changed to be the specified limits.  Any previous resource limits
                for the named quota root are discarded.

                If the named quota root did not previously exist, an implementation
                may optionally create it and change the quota roots for any number of
                existing mailboxes in an implementation-defined manner.

                Example:    C: A001 SETQUOTA "" (STORAGE 512)
                            S: * QUOTA "" (STORAGE 10 512)
                            S: A001 OK Setquota completed
            */
        }

        #endregion

        #region method GetFolderAcl

        /// <summary>
        /// Gets the specified folder ACL entries.
        /// </summary>
        /// <param name="folder">Folder name with path.</param>
        /// <returns>Returns folder ACL entries.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>folder</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected or not-authenticated state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public IMAP_r_u_Acl[] GetFolderAcl(string folder)
        {
            if(folder == null){
                throw new ArgumentNullException("folder");
            }
            if(folder == string.Empty){
                throw new ArgumentException("Argument 'folder' value must be specified.","folder");
            }
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }

            /* RFC 4314 3.3. GETACL Command.
                Arguments:  mailbox name

                Data:       untagged responses: ACL

                Result:     OK - getacl completed
                            NO - getacl failure: can't get acl
                            BAD - arguments invalid

                The GETACL command returns the access control list for mailbox in an
                untagged ACL response.

                Some implementations MAY permit multiple forms of an identifier to
                reference the same IMAP account.  Usually, such implementations will
                have a canonical form that is stored internally.  An ACL response
                caused by a GETACL command MAY include a canonicalized form of the
                identifier that might be different from the one used in the
                corresponding SETACL command.

                Example:    C: A002 GETACL INBOX
                            S: * ACL INBOX Fred rwipsldexta
                            S: A002 OK Getacl complete                
            */
                          
            SendCommand((m_CommandIndex++).ToString("d5") + " GETACL " + TextUtils.QuoteString(IMAP_Utils.Encode_IMAP_UTF7_String(folder)) + "\r\n");

            List<IMAP_r_u_Acl> retVal  = new List<IMAP_r_u_Acl>();
            IMAP_r_ServerStatus response = ReadResponse(null,null,null,null,null,retVal,null,null,null,null,null,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }

            return retVal.ToArray();
        }

        #endregion

        #region method SetFolderAcl

        /// <summary>
        /// Sets the specified folder ACL.
        /// </summary>
        /// <param name="folder">Folder name with path.</param>
        /// <param name="user">User name.</param>
        /// <param name="setType">Specifies how flags are set.</param>
        /// <param name="permissions">ACL permissions.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>folder</b> or <b>user</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected or not-authenticated state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public void SetFolderAcl(string folder,string user,IMAP_Flags_SetType setType,IMAP_ACL_Flags permissions)
        {
            if(folder == null){
                throw new ArgumentNullException("folder");
            }
            if(folder == string.Empty){
                throw new ArgumentException("Argument 'folder' value must be specified.","folder");
            }
            if(user == null){
                throw new ArgumentNullException("user");
            }
            if(user == string.Empty){
                throw new ArgumentException("Argument 'user' value must be specified.","user");
            }
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }

            /* RFC 4314 3.1. SETACL Command.
                Arguments:  mailbox name
                            identifier
                            access right modification

                Data:       no specific data for this command

                Result:     OK - setacl completed
                            NO - setacl failure: can't set acl
                            BAD - arguments invalid

                The SETACL command changes the access control list on the specified
                mailbox so that the specified identifier is granted permissions as
                specified in the third argument.

                The third argument is a string containing an optional plus ("+") or
                minus ("-") prefix, followed by zero or more rights characters.  If
                the string starts with a plus, the following rights are added to any
                existing rights for the identifier.  If the string starts with a
                minus, the following rights are removed from any existing rights for
                the identifier.  If the string does not start with a plus or minus,
                the rights replace any existing rights for the identifier.

                Note that an unrecognized right MUST cause the command to return the
                BAD response.  In particular, the server MUST NOT silently ignore
                unrecognized rights.

                Example:    C: A035 SETACL INBOX/Drafts John lrQswicda
                            S: A035 BAD Uppercase rights are not allowed
            
                            C: A036 SETACL INBOX/Drafts John lrqswicda
                            S: A036 BAD The q right is not supported
            */

            StringBuilder command = new StringBuilder();
            command.Append((m_CommandIndex++).ToString("d5"));            
            command.Append(" SETACL");
            command.Append(" " + TextUtils.QuoteString(IMAP_Utils.Encode_IMAP_UTF7_String(folder)));
            command.Append(" " + TextUtils.QuoteString(user));
            if(setType == IMAP_Flags_SetType.Add){
                command.Append(" +" + IMAP_Utils.ACL_to_String(permissions));
            }
            else if(setType == IMAP_Flags_SetType.Remove){
                command.Append(" -" + IMAP_Utils.ACL_to_String(permissions));
            }
            else if(setType == IMAP_Flags_SetType.Replace){
                command.Append(" " + IMAP_Utils.ACL_to_String(permissions));
            }
            else{
                throw new NotSupportedException("Not supported argument 'setType' value '" + setType.ToString() + "'.");
            }

            SendCommand(command.ToString());

            IMAP_r_ServerStatus response = ReadResponse(null,null,null,null,null,null,null,null,null,null,null,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            } 
        }

        #endregion

        #region method DeleteFolderAcl

        /// <summary>
        /// Deletes the specified folder user ACL entry.
        /// </summary>
        /// <param name="folder">Folder name with path.</param>
        /// <param name="user">User name.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>folder</b> or <b>user</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected or not-authenticated state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public void DeleteFolderAcl(string folder,string user)
        {
            if(folder == null){
                throw new ArgumentNullException("folder");
            }
            if(folder == string.Empty){
                throw new ArgumentException("Argument 'folder' value must be specified.","folder");
            }
            if(user == null){
                throw new ArgumentNullException("user");
            }
            if(user == string.Empty){
                throw new ArgumentException("Argument 'user' value must be specified.","user");
            }
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }

            /* RFC 4314 3.2. DELETEACL Command.
                Arguments:  mailbox name
                            identifier

                Data:       no specific data for this command

                Result:     OK - deleteacl completed
                            NO - deleteacl failure: can't delete acl
                            BAD - arguments invalid

                The DELETEACL command removes any <identifier,rights> pair for the
                specified identifier from the access control list for the specified
                mailbox.

                Example:    C: B001 getacl INBOX
                            S: * ACL INBOX Fred rwipslxetad -Fred wetd $team w
                            S: B001 OK Getacl complete
                            C: B002 DeleteAcl INBOX Fred
                            S: B002 OK Deleteacl complete
            */

            SendCommand((m_CommandIndex++).ToString("d5") + " DELETEACL " + TextUtils.QuoteString(user) + " " + TextUtils.QuoteString(IMAP_Utils.Encode_IMAP_UTF7_String(folder)) + "\r\n");

            IMAP_r_ServerStatus response = ReadResponse(null,null,null,null,null,null,null,null,null,null,null,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }
        }

        #endregion

        #region method GetFolderRights

        /// <summary>
        /// Gets rights which can be set for the specified identifier.
        /// </summary>
        /// <param name="folder">Folder name with path.</param>
        /// <param name="identifier">ACL entry identifier. Normally this is user or group name.</param>
        /// <returns>Returns LISTRIGHTS responses.</returns>
        /// <exception cref="ArgumentNullException">Is raised when<b>folder</b> or <b>identifier</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected or not-authenticated state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public IMAP_r_u_ListRights[] GetFolderRights(string folder,string identifier)
        {
            if(folder == null){
                throw new ArgumentNullException("folder");
            }
            if(folder == string.Empty){
                throw new ArgumentException("Argument 'folder' value must be specified.","folder");
            }
            if(identifier == null){
                throw new ArgumentNullException("identifier");
            }
            if(identifier == string.Empty){
                throw new ArgumentException("Argument 'identifier' value must be specified.","identifier");
            }
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }

            /* RFC 4314 3.4. LISTRIGHTS Command.
                Arguments:  mailbox name
                            identifier

                Data:       untagged responses: LISTRIGHTS

                Result:     OK - listrights completed
                            NO - listrights failure: can't get rights list
                            BAD - arguments invalid

                The LISTRIGHTS command takes a mailbox name and an identifier and
                returns information about what rights can be granted to the
                identifier in the ACL for the mailbox.

                Some implementations MAY permit multiple forms of an identifier to
                reference the same IMAP account.  Usually, such implementations will
                have a canonical form that is stored internally.  A LISTRIGHTS
                response caused by a LISTRIGHTS command MUST always return the same
                form of an identifier as specified by the client.  This is to allow
                the client to correlate the response with the command.

                Example:    C: a001 LISTRIGHTS ~/Mail/saved smith
                            S: * LISTRIGHTS ~/Mail/saved smith la r swicdkxte
                            S: a001 OK Listrights completed

                Example:    C: a005 listrights archive/imap anyone
                            S: * LISTRIGHTS archive.imap anyone ""
                               l r s w i p k x t e c d a 0 1 2 3 4 5 6 7 8 9
                            S: a005 Listrights successful
            */

            SendCommand((m_CommandIndex++).ToString("d5") + " LISTRIGHTS " + TextUtils.QuoteString(IMAP_Utils.Encode_IMAP_UTF7_String(folder)) + " " + TextUtils.QuoteString(identifier) + "\r\n");

            List<IMAP_r_u_ListRights> retVal = new List<IMAP_r_u_ListRights>();
            IMAP_r_ServerStatus response = ReadResponse(null,null,null,null,null,null,null,retVal,null,null,null,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }

            return retVal.ToArray();
        }

        #endregion

        #region method GetFolderMyRights

        /// <summary>
        /// Gets myrights to the specified folder.
        /// </summary>
        /// <param name="folder">Folder name with path.</param>
        /// <returns>Returns MYRIGHTS responses.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>folder</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected or not-authenticated state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public IMAP_Response_MyRights[] GetFolderMyRights(string folder)
        {
            if(folder == null){
                throw new ArgumentNullException("folder");
            }
            if(folder == string.Empty){
                throw new ArgumentException("Argument 'folder' value must be specified.","folder");
            }
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }

            /* RFC 4314 3.5. MYRIGHTS Command.
                Arguments:  mailbox name

                Data:       untagged responses: MYRIGHTS

                Result:     OK - myrights completed
                            NO - myrights failure: can't get rights
                            BAD - arguments invalid

                The MYRIGHTS command returns the set of rights that the user has to
                mailbox in an untagged MYRIGHTS reply.

                Example:    C: A003 MYRIGHTS INBOX
                            S: * MYRIGHTS INBOX rwiptsldaex
                            S: A003 OK Myrights complete
            */

            
            SendCommand((m_CommandIndex++).ToString("d5") + " MYRIGHTS " + TextUtils.QuoteString(IMAP_Utils.Encode_IMAP_UTF7_String(folder)) + "\r\n");

            List<IMAP_Response_MyRights> retVal = new List<IMAP_Response_MyRights>();
            IMAP_r_ServerStatus response = ReadResponse(null,null,null,null,null,null,retVal,null,null,null,null,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }

            return retVal.ToArray();
        }

        #endregion

        #region method StoreMessage

        /// <summary>
        /// Stores specified message to the specified folder.
        /// </summary>
        /// <param name="folder">Folder name with path.</param>
        /// <param name="flags">Message flags.</param>
        /// <param name="internalDate">Message internal data. DateTime.MinValu means server will allocate it.</param>
        /// <param name="message">Message stream.</param>
        /// <param name="count">Number of bytes send from <b>message</b> stream.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>folder</b> or <b>stream</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected or not-authenticated state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public void StoreMessage(string folder,IMAP_MessageFlags flags,DateTime internalDate,Stream message,int count)
        {
            if(folder == null){
                throw new ArgumentNullException("folder");
            }
            if(folder == string.Empty){
                throw new ArgumentException("Argument 'folder' value must be specified.","folder");
            }
            if(message == null){
                throw new ArgumentNullException("message");
            }
            if(count < 1){
                throw new ArgumentException("Argument 'count' value must be >= 1.","count");
            }
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }

            /* RFC 3501 6.3.11. APPEND Command.
                Arguments:  mailbox name
                            OPTIONAL flag parenthesized list
                            OPTIONAL date/time string
                            message literal

                Responses:  no specific responses for this command

                Result:     OK - append completed
                            NO - append error: can't append to that mailbox, error
                                 in flags or date/time or message text
                            BAD - command unknown or arguments invalid

                The APPEND command appends the literal argument as a new message
                to the end of the specified destination mailbox.  This argument
                SHOULD be in the format of an [RFC-2822] message.  8-bit
                characters are permitted in the message.  A server implementation
                that is unable to preserve 8-bit data properly MUST be able to
                reversibly convert 8-bit APPEND data to 7-bit using a [MIME-IMB]
                content transfer encoding.

                Note: There MAY be exceptions, e.g., draft messages, in
                which required [RFC-2822] header lines are omitted in
                the message literal argument to APPEND.  The full
                implications of doing so MUST be understood and
                carefully weighed.

                If a flag parenthesized list is specified, the flags SHOULD be set
                in the resulting message; otherwise, the flag list of the
                resulting message is set to empty by default.  In either case, the
                Recent flag is also set.

                If a date-time is specified, the internal date SHOULD be set in
                the resulting message; otherwise, the internal date of the
                resulting message is set to the current date and time by default.

                If the append is unsuccessful for any reason, the mailbox MUST be
                restored to its state before the APPEND attempt; no partial
                appending is permitted.

                If the destination mailbox does not exist, a server MUST return an
                error, and MUST NOT automatically create the mailbox.  Unless it
                is certain that the destination mailbox can not be created, the
                server MUST send the response code "[TRYCREATE]" as the prefix of
                the text of the tagged NO response.  This gives a hint to the
                client that it can attempt a CREATE command and retry the APPEND
                if the CREATE is successful.

                If the mailbox is currently selected, the normal new message
                actions SHOULD occur.  Specifically, the server SHOULD notify the
                client immediately via an untagged EXISTS response.  If the server
                does not do so, the client MAY issue a NOOP command (or failing
                that, a CHECK command) after one or more APPEND commands.

                Example:    C: A003 APPEND saved-messages (\Seen) {310}
                            S: + Ready for literal data
                            C: Date: Mon, 7 Feb 1994 21:52:25 -0800 (PST)
                            C: From: Fred Foobar <foobar@Blurdybloop.COM>
                            C: Subject: afternoon meeting
                            C: To: mooch@owatagu.siam.edu
                            C: Message-Id: <B27397-0100000@Blurdybloop.COM>
                            C: MIME-Version: 1.0
                            C: Content-Type: TEXT/PLAIN; CHARSET=US-ASCII
                            C:
                            C: Hello Joe, do you think we can meet at 3:30 tomorrow?
                            C:
                            S: A003 OK APPEND completed

                Note: The APPEND command is not used for message delivery,
                because it does not provide a mechanism to transfer [SMTP]
                envelope information.
            */

            StringBuilder command = new StringBuilder();
            command.Append((m_CommandIndex++).ToString("d5"));            
            command.Append(" APPEND");
            command.Append(" " + TextUtils.QuoteString(IMAP_Utils.Encode_IMAP_UTF7_String(folder)));
            if(flags != IMAP_MessageFlags.None){
                command.Append(" (" + IMAP_Utils.MessageFlagsToString(flags) + ")");
            }
            if(internalDate != DateTime.MinValue){
                command.Append(" " + TextUtils.QuoteString(IMAP_Utils.DateTimeToString(internalDate)));
            }
            command.Append(" {" + count + "}\r\n");

            SendCommand(command.ToString());

            // We must get + here.
            IMAP_r_ServerStatus response = ReadResponse(null,null,null,null,null,null,null,null,null,null,null,null,null);
            if(!response.ResponseCode.Equals("+",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }

            int    countSent = 0;
            byte[] buffer    = new byte[32000];
            while(countSent < count){
                int readedCount = message.Read(buffer,0,Math.Min(buffer.Length,count - countSent));
                if(readedCount == 0){
                    throw new ArgumentException("Argument 'stream' has less data than specified in 'count'.","stream");
                }
                else{
                    this.TcpStream.Write(buffer,0,readedCount);
                    countSent += readedCount;
                }
            }

            LogAddWrite(count,"Wrote " + count + " bytes.");

            // Send command line terminating CRLF.
            WriteLine("\r\n");

            response = ReadResponse(null,null,null,null,null,null,null,null,null,null,null,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            } 
        }

        #endregion


        #region method CloseFolder

        /// <summary>
        /// Closes selected folder, all messages marked as Deleted will be expunged.
        /// </summary>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected, not-authenticated or not-selected state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public void CloseFolder()
        {            
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }
            if(m_pSelectedFolder == null){
                throw new InvalidOperationException("Not selected state, you need to select some folder first.");
            }

            /* RFC 3501 6.4.2. CLOSE Command.
                Arguments:  none

                Responses:  no specific responses for this command

                Result:     OK - close completed, now in authenticated state
                            BAD - command unknown or arguments invalid

                The CLOSE command permanently removes all messages that have the
                \Deleted flag set from the currently selected mailbox, and returns
                to the authenticated state from the selected state.  No untagged
                EXPUNGE responses are sent.

                No messages are removed, and no error is given, if the mailbox is
                selected by an EXAMINE command or is otherwise selected read-only.

                Even if a mailbox is selected, a SELECT, EXAMINE, or LOGOUT
                command MAY be issued without previously issuing a CLOSE command.
                The SELECT, EXAMINE, and LOGOUT commands implicitly close the
                currently selected mailbox without doing an expunge.  However,
                when many messages are deleted, a CLOSE-LOGOUT or CLOSE-SELECT
                sequence is considerably faster than an EXPUNGE-LOGOUT or
                EXPUNGE-SELECT because no untagged EXPUNGE responses (which the
                client would probably ignore) are sent.

                Example:    C: A341 CLOSE
                            S: A341 OK CLOSE completed

            */

            SendCommand((m_CommandIndex++).ToString("d5") + " CLOSE\r\n");

            IMAP_r_ServerStatus response = ReadResponse(null,null,null,null,null,null,null,null,null,null,null,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }

            m_pSelectedFolder = null;
        }

        #endregion

        #region method Fetch

        /// <summary>
        /// Fetches specified message items.
        /// </summary>
        /// <param name="uid">Specifies if argument <b>seqSet</b> contains messages UID or sequence numbers.</param>
        /// <param name="seqSet">Sequence set of messages to fetch.</param>
        /// <param name="items">Fetch items to fetch.</param>
        /// <param name="handler">Fetch responses handler.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>seqSet</b>,<b>items</b> or <b>handler</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected, not-authenticated or not-selected state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public void Fetch(bool uid,IMAP_SequenceSet seqSet,IMAP_Fetch_DataItem[] items,IMAP_Client_FetchHandler handler)
        {
            if(seqSet == null){
                throw new ArgumentNullException("seqSet");
            }
            if(items == null){
                throw new ArgumentNullException("items");
            }
            if(items.Length < 1){
                throw new ArgumentException("Argument 'items' must conatain at least 1 value.","items");
            }
            if(handler == null){
                throw new ArgumentNullException("handler");
            }
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }
            if(m_pSelectedFolder == null){
                throw new InvalidOperationException("Not selected state, you need to select some folder first.");
            }

            /* RFC 3501 6.4.5. FETCH Command.
                Arguments:  sequence set
                            message data item names or macro

                Responses:  untagged responses: FETCH

                Result:     OK - fetch completed
                            NO - fetch error: can't fetch that data
                            BAD - command unknown or arguments invalid

                The FETCH command retrieves data associated with a message in the
                mailbox.  The data items to be fetched can be either a single atom
                or a parenthesized list.

                Most data items, identified in the formal syntax under the
                msg-att-static rule, are static and MUST NOT change for any
                particular message.  Other data items, identified in the formal
                syntax under the msg-att-dynamic rule, MAY change, either as a
                result of a STORE command or due to external events.

                    For example, if a client receives an ENVELOPE for a
                    message when it already knows the envelope, it can
                    safely ignore the newly transmitted envelope.
            */

            StringBuilder command = new StringBuilder();
            command.Append((m_CommandIndex++).ToString("d5"));
            if(uid){
                command.Append(" UID");
            }
            command.Append(" FETCH " + seqSet.ToSequenceSetString() + " (");
            for(int i=0;i<items.Length;i++){
                if(i > 0){
                    command.Append(" ");
                }
                command.Append(items[i].ToString());
            }
            command.Append(")\r\n");
     
            SendCommand(command.ToString());

            IMAP_r_ServerStatus response = ReadResponse(null,null,null,null,null,null,null,null,null,null,null,null,handler);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }
        }

        #endregion

        #region method Search

        /// <summary>
        /// Searches message what matches specified search criteria.
        /// </summary>
        /// <param name="uid">If true then UID SERACH, otherwise normal SEARCH.</param>
        /// <param name="charset">Charset used in search criteria. Value null means ASCII. The UTF-8 is reccomended value non ASCII searches.</param>
        /// <param name="criteria">Search criteria.</param>
        /// <returns>Returns search expression matehced messages sequence-numbers or UIDs(This depends on argument <b>uid</b> value).</returns>
        /// <exception cref="ArgumentNullException">Is rised when <b>criteria</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected, not-authenticated or not-selected state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public int[] Search(bool uid,string charset,string criteria)
        {
            if(criteria == null){
                throw new ArgumentNullException("criteria");
            }
            if(criteria == string.Empty){
                throw new ArgumentException("Argument 'criteria' value must be specified.","criteria");
            }
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }
            if(m_pSelectedFolder == null){
                throw new InvalidOperationException("Not selected state, you need to select some folder first.");
            }

            StringBuilder command = new StringBuilder();
            command.Append((m_CommandIndex++).ToString("d5"));
            if(uid){
                command.Append(" UID");
            }
            command.Append(" SEARCH");
            if(!string.IsNullOrEmpty(charset)){
                command.Append(" CHARSET " + charset);
            }
            command.Append(" " + criteria + "\r\n");

            SendCommand(command.ToString());

            List<int> retVal = new List<int>();
            IMAP_r_ServerStatus response = ReadResponse(null,null,retVal,null,null,null,null,null,null,null,null,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }
           
            return retVal.ToArray();
        }

        #endregion
                
        #region method StoreMessageFlags

        /// <summary>
        /// Stores specified message flags to the sepcified messages.
        /// </summary>
        /// <param name="uid">Specifies if <b>seqSet</b> contains UIDs or sequence-numbers.</param>
        /// <param name="seqSet">Messages sequence-set.</param>
        /// <param name="setType">Specifies how flags are set.</param>
        /// <param name="flags">Message flags.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>seqSet</b> is null reference.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected, not-authenticated or not-selected state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public void StoreMessageFlags(bool uid,IMAP_SequenceSet seqSet,IMAP_Flags_SetType setType,IMAP_MessageFlags flags)
        {
            if(seqSet == null){
                throw new ArgumentNullException("seqSet");
            }
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }
            if(m_pSelectedFolder == null){
                throw new InvalidOperationException("Not selected state, you need to select some folder first.");
            }

            /* RFC 3501 6.4.6. STORE Command.
                Arguments:  sequence set
                            message data item name
                            value for message data item

                Responses:  untagged responses: FETCH

                Result:     OK - store completed
                            NO - store error: can't store that data
                            BAD - command unknown or arguments invalid

                The STORE command alters data associated with a message in the
                mailbox.  Normally, STORE will return the updated value of the
                data with an untagged FETCH response.  A suffix of ".SILENT" in
                the data item name prevents the untagged FETCH, and the server
                SHOULD assume that the client has determined the updated value
                itself or does not care about the updated value.

                    Note: Regardless of whether or not the ".SILENT" suffix
                    was used, the server SHOULD send an untagged FETCH
                    response if a change to a message's flags from an
                    external source is observed.  The intent is that the
                    status of the flags is determinate without a race
                    condition.

                The currently defined data items that can be stored are:

                FLAGS <flag list>
                    Replace the flags for the message (other than \Recent) with the
                    argument.  The new value of the flags is returned as if a FETCH
                    of those flags was done.

                FLAGS.SILENT <flag list>
                    Equivalent to FLAGS, but without returning a new value.

                +FLAGS <flag list>
                    Add the argument to the flags for the message.  The new value
                    of the flags is returned as if a FETCH of those flags was done.

                +FLAGS.SILENT <flag list>
                    Equivalent to +FLAGS, but without returning a new value.

                -FLAGS <flag list>
                    Remove the argument from the flags for the message.  The new
                    value of the flags is returned as if a FETCH of those flags was
                    done.

                -FLAGS.SILENT <flag list>
                    Equivalent to -FLAGS, but without returning a new value.


                Example:    C: A003 STORE 2:4 +FLAGS (\Deleted)
                            S: * 2 FETCH (FLAGS (\Deleted \Seen))
                            S: * 3 FETCH (FLAGS (\Deleted))
                            S: * 4 FETCH (FLAGS (\Deleted \Flagged \Seen))
                            S: A003 OK STORE completed
            */

            StringBuilder command = new StringBuilder();
            command.Append((m_CommandIndex++).ToString("d5"));
            if(uid){
                command.Append(" UID");
            }
            command.Append(" STORE");
            command.Append(" " + seqSet.ToSequenceSetString());
            if(setType == IMAP_Flags_SetType.Add){
                command.Append(" +FLAGS.SILENT");
            }
            else if(setType == IMAP_Flags_SetType.Remove){
                command.Append(" -FLAGS.SILENT");
            }
            else if(setType == IMAP_Flags_SetType.Replace){
                command.Append(" FLAGS.SILENT");
            }
            else{
                throw new NotSupportedException("Not supported argument 'setType' value '" + setType.ToString() + "'.");
            }
            command.Append(" (" + IMAP_Utils.MessageFlagsToString(flags) + ")\r\n");

            SendCommand(command.ToString());

            IMAP_r_ServerStatus response = ReadResponse(null,null,null,null,null,null,null,null,null,null,null,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            } 
        }

        #endregion

        #region method CopyMessages

        /// <summary>
        /// Copies specified messages from current selected folder to the specified target folder.
        /// </summary>
        /// <param name="uid">Specifies if <b>seqSet</b> contains UIDs or message-numberss.</param>
        /// <param name="seqSet">Messages sequence set.</param>
        /// <param name="targetFolder">Target folder name with path.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>seqSet</b> or <b>targetFolder</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected, not-authenticated or not-selected state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public void CopyMessages(bool uid,IMAP_SequenceSet seqSet,string targetFolder)
        {
            if(seqSet == null){
                throw new ArgumentNullException("seqSet");
            }
            if(targetFolder == null){
                throw new ArgumentNullException("folder");
            }
            if(targetFolder == string.Empty){
                throw new ArgumentException("Argument 'folder' value must be specified.","folder");
            }
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }
            if(m_pSelectedFolder == null){
                throw new InvalidOperationException("Not selected state, you need to select some folder first.");
            }

            /* RFC 3501 6.4.7. COPY Command.
                Arguments:  sequence set
                            mailbox name

                Responses:  no specific responses for this command

                Result:     OK - copy completed
                            NO - copy error: can't copy those messages or to that
                                 name
                            BAD - command unknown or arguments invalid

                The COPY command copies the specified message(s) to the end of the
                specified destination mailbox.  The flags and internal date of the
                message(s) SHOULD be preserved, and the Recent flag SHOULD be set,
                in the copy.

                If the destination mailbox does not exist, a server SHOULD return
                an error.  It SHOULD NOT automatically create the mailbox.  Unless
                it is certain that the destination mailbox can not be created, the
                server MUST send the response code "[TRYCREATE]" as the prefix of
                the text of the tagged NO response.  This gives a hint to the
                client that it can attempt a CREATE command and retry the COPY if
                the CREATE is successful.

                If the COPY command is unsuccessful for any reason, server
                implementations MUST restore the destination mailbox to its state
                before the COPY attempt.

                Example:    C: A003 COPY 2:4 MEETING
                            S: A003 OK COPY completed
            */

            if(uid){
                SendCommand((m_CommandIndex++).ToString("d5") + " UID COPY " + seqSet.ToSequenceSetString() + " " + TextUtils.QuoteString(IMAP_Utils.Encode_IMAP_UTF7_String(targetFolder)) + "\r\n");
            }
            else{
                SendCommand((m_CommandIndex++).ToString("d5") + " COPY " + seqSet.ToSequenceSetString() + " " + TextUtils.QuoteString(IMAP_Utils.Encode_IMAP_UTF7_String(targetFolder)) + "\r\n");
            }

            IMAP_r_ServerStatus response = ReadResponse(null,null,null,null,null,null,null,null,null,null,null,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }
        }

        #endregion
                
        #region method MoveMessages
        
        /// <summary>
        /// Moves specified messages from current selected folder to the specified target folder.
        /// </summary>
        /// <param name="uid">Specifies if <b>seqSet</b> contains UIDs or message-numberss.</param>
        /// <param name="seqSet">Messages sequence set.</param>
        /// <param name="targetFolder">Target folder name with path.</param>
        /// <param name="expunge">If ture messages are expunged from selected folder, otherwise they are marked as <b>Deleted</b>.
        /// Note: If true - then all messages marked as <b>Deleted</b> are expunged !</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>seqSet</b> or <b>targetFolder</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected, not-authenticated or not-selected state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public void MoveMessages(bool uid,IMAP_SequenceSet seqSet,string targetFolder,bool expunge)
        {
            if(seqSet == null){
                throw new ArgumentNullException("seqSet");
            }
            if(targetFolder == null){
                throw new ArgumentNullException("folder");
            }
            if(targetFolder == string.Empty){
                throw new ArgumentException("Argument 'folder' value must be specified.","folder");
            }
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }
            if(m_pSelectedFolder == null){
                throw new InvalidOperationException("Not selected state, you need to select some folder first.");
            }

            CopyMessages(uid,seqSet,targetFolder);
            StoreMessageFlags(uid,seqSet,IMAP_Flags_SetType.Add,IMAP_MessageFlags.Deleted);
            if(expunge){
                Expunge();
            }
        }

        #endregion

        #region method Expunge

        /// <summary>
        /// Deletes all messages in selected folder which has "Deleted" flag set.
        /// </summary>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected, not-authenticated or not-selected state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public void Expunge()
        {
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }
            if(!this.IsAuthenticated){
                throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
            }
            if(m_pSelectedFolder == null){
                throw new InvalidOperationException("Not selected state, you need to select some folder first.");
            }

            /* RFC 3501 6.4.3. EXPUNGE Command.
                Arguments:  none

                Responses:  untagged responses: EXPUNGE

                Result:     OK - expunge completed
                            NO - expunge failure: can't expunge (e.g., permission
                                 denied)
                            BAD - command unknown or arguments invalid

                The EXPUNGE command permanently removes all messages that have the
                \Deleted flag set from the currently selected mailbox.  Before
                returning an OK to the client, an untagged EXPUNGE response is
                sent for each message that is removed.

                Example:    C: A202 EXPUNGE
                            S: * 3 EXPUNGE
                            S: * 3 EXPUNGE
                            S: * 5 EXPUNGE
                            S: * 8 EXPUNGE
                            S: A202 OK EXPUNGE completed

                Note: In this example, messages 3, 4, 7, and 11 had the
                \Deleted flag set.  See the description of the EXPUNGE
                response for further explanation.
            */

            SendCommand((m_CommandIndex++).ToString("d5") + " EXPUNGE\r\n");

            IMAP_r_ServerStatus response = ReadResponse(null,null,null,null,null,null,null,null,null,null,null,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }
        }

        #endregion


        #region method Capability

        /// <summary>
        /// Gets IMAP server capabilities.
        /// </summary>
        /// <returns>Returns CAPABILITIES responses.</returns>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public IMAP_r_u_Capability[] Capability()
        {
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }

            /* RFC 3501 6.1.1. CAPABILITY Command.
                Arguments:  none

                Responses:  REQUIRED untagged response: CAPABILITY

                Result:     OK - capability completed
                            BAD - command unknown or arguments invalid

                The CAPABILITY command requests a listing of capabilities that the
                server supports.  The server MUST send a single untagged
                CAPABILITY response with "IMAP4rev1" as one of the listed
                capabilities before the (tagged) OK response.

                A capability name which begins with "AUTH=" indicates that the
                server supports that particular authentication mechanism.  All
                such names are, by definition, part of this specification.  For
                example, the authorization capability for an experimental
                "blurdybloop" authenticator would be "AUTH=XBLURDYBLOOP" and not
                "XAUTH=BLURDYBLOOP" or "XAUTH=XBLURDYBLOOP".

                Other capability names refer to extensions, revisions, or
                amendments to this specification.  See the documentation of the
                CAPABILITY response for additional information.  No capabilities,
                beyond the base IMAP4rev1 set defined in this specification, are
                enabled without explicit client action to invoke the capability.

                Client and server implementations MUST implement the STARTTLS,
                LOGINDISABLED, and AUTH=PLAIN (described in [IMAP-TLS])
                capabilities.  See the Security Considerations section for
                important information.

                See the section entitled "Client Commands -
                Experimental/Expansion" for information about the form of site or
                implementation-specific capabilities.

                Example:    C: abcd CAPABILITY
                            S: * CAPABILITY IMAP4rev1 STARTTLS AUTH=GSSAPI LOGINDISABLED
                            S: abcd OK CAPABILITY completed
                            C: efgh STARTTLS
                            S: efgh OK STARTLS completed
                               <TLS negotiation, further commands are under [TLS] layer>
                            C: ijkl CAPABILITY
                            S: * CAPABILITY IMAP4rev1 AUTH=GSSAPI AUTH=PLAIN
                            S: ijkl OK CAPABILITY completed
            */

            SendCommand((m_CommandIndex++).ToString("d5") + " CAPABILITY\r\n");

            List<IMAP_r_u_Capability> retVal = new List<IMAP_r_u_Capability>();
            IMAP_r_ServerStatus response = ReadResponse(retVal,null,null,null,null,null,null,null,null,null,null,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }

            return retVal.ToArray();
        }

        #endregion

        #region method Noop

        /// <summary>
        /// Sends NOOP command to IMAP server.
        /// </summary>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not in valid state(not-connected state).</exception>
        /// <exception cref="IMAP_ClientException">Is raised when server refuses to complete this command and returns error.</exception>
        public void Noop()
        {
            if(!this.IsConnected){
                throw new InvalidOperationException("Not connected, you need to connect first.");
            }

            /* RFC 3501 6.1.2. NOOP Command.

                Arguments:  none

                Responses:  no specific responses for this command (but see below)

                Result:     OK - noop completed
                            BAD - command unknown or arguments invalid

                The NOOP command always succeeds.  It does nothing.

                Since any command can return a status update as untagged data, the
                NOOP command can be used as a periodic poll for new messages or
                message status updates during a period of inactivity (this is the
                preferred method to do this).  The NOOP command can also be used
                to reset any inactivity autologout timer on the server.            
            */

            SendCommand((m_CommandIndex++).ToString("d5") + " NOOP\r\n");

            IMAP_r_ServerStatus response = ReadResponse(null,null,null,null,null,null,null,null,null,null,null,null,null);
            if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
            }
        }

        #endregion


        #region override method OnConnected

        /// <summary>
        /// This method is called after TCP client has sucessfully connected.
        /// </summary>
        protected override void OnConnected()
        {
            // Read greeting text. It's untagged status response.
            SmartStream.ReadLineAsyncOP args = new SmartStream.ReadLineAsyncOP(new byte[32000],SizeExceededAction.JunkAndThrowException);
            this.TcpStream.ReadLine(args,false);
            if(args.Error != null){
                throw args.Error;
            }
            string line = args.LineUtf8;
            LogAddRead(args.BytesInBuffer,line);

            m_GreetingText = line.Split(new char[]{' '},3)[2];
        }

        #endregion
                        

        #region method SendCommand

        /// <summary>
        /// Send specified command to the IMAP server.
        /// </summary>
        /// <param name="command">Command to send.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>command</b> is null reference value.</exception>
        private void SendCommand(string command)
        {
            if(command == null){
                throw new ArgumentNullException("command");
            }
                                  
            this.TcpStream.Write(command);
            LogAddWrite(command.Length,command.TrimEnd());
        }

        #endregion

        #region method ReadResponse

        /// <summary>
        /// Reads IMAP server responses.
        /// </summary>
        /// <param name="folderInfo">Folder info where to store folder related data.
        /// This applies to SELECT or EXAMINE command only. This value can be null.
        /// </param>
        /// <param name="capability">List wehere to store CAPABILITY command result. This value can be null.</param>
        /// <param name="search">List wehere to store SEARCH command result. This value can be null.</param>
        /// <param name="list">List where to store LIST command result. This value can be null.</param>
        /// <param name="lsub">List where to store LSUB command result. This value can be null.</param>
        /// <param name="acl">List where to store ACL command result. This value can be null.</param>
        /// <param name="myRights">List where to store MYRIGHTS command result. This value can be null.</param>
        /// <param name="listRights">List where to store LISTRIGHTS command result. This value can be null.</param>
        /// <param name="status">List where to store STATUS command result. This value can be null.</param>
        /// <param name="quota">List where to store QUOTA command result. This value can be null.</param>
        /// <param name="quotaRoot">List where to store QUOTAROOT command result. This value can be null.</param>
        /// <param name="nspace">List where to store NAMESPACE command result. This value can be null.</param>
        /// <param name="fetchHandler">Fetch data-items handler.</param>
        /// <returns>Returns command completion status response.</returns>
        private IMAP_r_ServerStatus ReadResponse(List<IMAP_r_u_Capability> capability,IMAP_Client_SelectedFolder folderInfo,List<int> search,List<IMAP_r_u_List> list,List<IMAP_r_u_LSub> lsub,List<IMAP_r_u_Acl> acl,List<IMAP_Response_MyRights> myRights,List<IMAP_r_u_ListRights> listRights,List<IMAP_r_u_Status> status,List<IMAP_r_u_Quota> quota,List<IMAP_r_u_QuotaRoot> quotaRoot,List<IMAP_r_u_Namespace> nspace,IMAP_Client_FetchHandler fetchHandler)
        {
            /* RFC 3501 2.2.2.
                The protocol receiver of an IMAP4rev1 client reads a response line
                from the server.  It then takes action on the response based upon the
                first token of the response, which can be a tag, a "*", or a "+".
             
                The client MUST be prepared to accept any response at all times.
            */
                        
            SmartStream.ReadLineAsyncOP args = new SmartStream.ReadLineAsyncOP(new byte[32000],SizeExceededAction.JunkAndThrowException);

            while(true){
                // Read response line.
                this.TcpStream.ReadLine(args,false);
                if(args.Error != null){
                    throw args.Error;
                }
                string responseLine = args.LineUtf8;

                // Log
                LogAddRead(args.BytesInBuffer,responseLine);

                // Untagged response.
                if(responseLine.StartsWith("*")){
                    string[] parts = responseLine.Split(new char[]{' '},4);
                    string   word  = responseLine.Split(' ')[1];

                    #region Untagged status responses. RFC 3501 7.1.

                    // OK,NO,BAD,PREAUTH,BYE

                    if(word.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                        IMAP_r_u_ServerStatus response = IMAP_r_u_ServerStatus.Parse(responseLine);

                        // Process optional response-codes(7.2). ALERT,BADCHARSET,CAPABILITY,PARSE,PERMANENTFLAGS,READ-ONLY,
                        // READ-WRITE,TRYCREATE,UIDNEXT,UIDVALIDITY,UNSEEN

                        if(!string.IsNullOrEmpty(response.OptionalResponseCode)){
                            if(response.OptionalResponseCode.Equals("PERMANENTFLAGS",StringComparison.InvariantCultureIgnoreCase)){
                                if(folderInfo != null){
                                    StringReader r = new StringReader(response.OptionalResponseArgs);

                                    folderInfo.SetPermanentFlags(r.ReadParenthesized().Split(' '));
                                }
                            }
                            else if(response.OptionalResponseCode.Equals("READ-ONLY",StringComparison.InvariantCultureIgnoreCase)){
                                if(folderInfo != null){
                                    folderInfo.SetReadOnly(true);
                                }
                            }
                            else if(response.OptionalResponseCode.Equals("READ-WRITE",StringComparison.InvariantCultureIgnoreCase)){
                                if(folderInfo != null){
                                    folderInfo.SetReadOnly(true);
                                }
                            }
                            else if(response.OptionalResponseCode.Equals("UIDNEXT",StringComparison.InvariantCultureIgnoreCase)){
                                if(folderInfo != null){
                                    folderInfo.SetUidNext(Convert.ToInt64(response.OptionalResponseArgs));
                                }
                            }
                            else if(response.OptionalResponseCode.Equals("UIDVALIDITY",StringComparison.InvariantCultureIgnoreCase)){
                                if(folderInfo != null){
                                    folderInfo.SetUidValidity(Convert.ToInt64(response.OptionalResponseArgs));
                                }
                            }
                            else if(response.OptionalResponseCode.Equals("UNSEEN",StringComparison.InvariantCultureIgnoreCase)){
                                if(folderInfo != null){
                                    folderInfo.SetFirstUnseen(Convert.ToInt32(response.OptionalResponseArgs));
                                }
                            }
                            // We don't care about other response codes.                            
                        }

                        OnUntaggedStatusResponse(response);
                    }
                    else if(word.Equals("NO",StringComparison.InvariantCultureIgnoreCase)){
                        OnUntaggedStatusResponse(IMAP_r_u_ServerStatus.Parse(responseLine));
                    }
                    else if(word.Equals("BAD",StringComparison.InvariantCultureIgnoreCase)){
                        OnUntaggedStatusResponse(IMAP_r_u_ServerStatus.Parse(responseLine));
                    }
                    else if(word.Equals("PREAUTH",StringComparison.InvariantCultureIgnoreCase)){
                        OnUntaggedStatusResponse(IMAP_r_u_ServerStatus.Parse(responseLine));
                    }
                    else if(word.Equals("BYE",StringComparison.InvariantCultureIgnoreCase)){
                        OnUntaggedStatusResponse(IMAP_r_u_ServerStatus.Parse(responseLine));
                    }

                    #endregion

                    #region Untagged server and mailbox status. RFC 3501 7.2.

                    // CAPABILITY,LIST,LSUB,STATUS,SEARCH,FLAGS

                    #region CAPABILITY

                    else if(word.Equals("CAPABILITY",StringComparison.InvariantCultureIgnoreCase)){
                        if(capability != null){
                            capability.Add(IMAP_r_u_Capability.Parse(responseLine));
                        }
                    }

                    #endregion

                    #region LIST

                    else if(word.Equals("LIST",StringComparison.InvariantCultureIgnoreCase)){
                        if(list != null){
                            list.Add(IMAP_r_u_List.Parse(responseLine));
                        }
                    }

                    #endregion

                    #region LSUB

                    else if(word.Equals("LSUB",StringComparison.InvariantCultureIgnoreCase)){
                        if(lsub != null){
                            lsub.Add(IMAP_r_u_LSub.Parse(responseLine));
                        }
                    }

                    #endregion

                    #region STATUS

                    else if(word.Equals("STATUS",StringComparison.InvariantCultureIgnoreCase)){
                        if(status != null){
                            status.Add(IMAP_r_u_Status.Parse(responseLine));
                        }
                    }

                    #endregion

                    #region SEARCH

                    else if(word.Equals("SEARCH",StringComparison.InvariantCultureIgnoreCase)){
                        /* RFC 3501 7.2.5.  SEARCH Response
                            Contents:   zero or more numbers

                            The SEARCH response occurs as a result of a SEARCH or UID SEARCH
                            command.  The number(s) refer to those messages that match the
                            search criteria.  For SEARCH, these are message sequence numbers;
                            for UID SEARCH, these are unique identifiers.  Each number is
                            delimited by a space.

                            Example:    S: * SEARCH 2 3 6
                        */
                        
                        if(search != null){
                            if(responseLine.Split(' ').Length > 2){
                                foreach(string value in responseLine.Split(new char[]{' '},3)[2].Split(' ')){
                                    search.Add(Convert.ToInt32(value));
                                }
                            }
                        }
                    }

                    #endregion

                    #region FLAGS

                    else if(word.Equals("FLAGS",StringComparison.InvariantCultureIgnoreCase)){
                        /* RFC 3501 7.2.6. FLAGS Response.                         
                            Contents:   flag parenthesized list

                            The FLAGS response occurs as a result of a SELECT or EXAMINE
                            command.  The flag parenthesized list identifies the flags (at a
                            minimum, the system-defined flags) that are applicable for this
                            mailbox.  Flags other than the system flags can also exist,
                            depending on server implementation.

                            The update from the FLAGS response MUST be recorded by the client.

                            Example:    S: * FLAGS (\Answered \Flagged \Deleted \Seen \Draft)
                        */

                        if(folderInfo != null){
                            StringReader r = new StringReader(responseLine.Split(new char[]{' '},3)[2]);

                            folderInfo.SetFlags(r.ReadParenthesized().Split(' '));
                        }
                    }

                    #endregion

                    #endregion

                    #region Untagged mailbox size. RFC 3501 7.3.

                    // EXISTS,RECENT

                    // TODO: May this values exist other command than SELECT and EXAMINE ?
                    // Update local cached value.
                    // OnMailboxSize

                    else if(Net_Utils.IsInteger(word) && parts[2].Equals("EXISTS",StringComparison.InvariantCultureIgnoreCase)){
                        if(folderInfo != null){
                            folderInfo.SetMessagesCount(Convert.ToInt32(word));
                        }
                    }
                    else if(Net_Utils.IsInteger(word) && parts[2].Equals("RECENT",StringComparison.InvariantCultureIgnoreCase)){
                        if(folderInfo != null){
                            folderInfo.SetRecentMessagesCount(Convert.ToInt32(word));
                        }
                    }
                                        
                    #endregion

                    #region Untagged message status. RFC 3501 7.4.

                    // EXPUNGE,FETCH

                    else if(Net_Utils.IsInteger(word) && parts[2].Equals("EXPUNGE",StringComparison.InvariantCultureIgnoreCase)){
                        OnMessageExpunged(IMAP_r_u_Expunge.Parse(responseLine));
                    }
                    else if(Net_Utils.IsInteger(word) && parts[2].Equals("FETCH",StringComparison.InvariantCultureIgnoreCase)){
                        // User din't provide us FETCH handler, make dummy one which eats up all fetch responses.
                        if(fetchHandler == null){
                            fetchHandler = new IMAP_Client_FetchHandler();
                        }

                        _FetchResponseReader r = new _FetchResponseReader(this,responseLine,fetchHandler);
                        r.Start();                        
                    }

                    #endregion

                    #region Untagged acl realted. RFC 4314.

                    else if(word.Equals("ACL",StringComparison.InvariantCultureIgnoreCase)){
                        if(acl != null){
                            acl.Add(IMAP_r_u_Acl.Parse(responseLine));
                        }
                    }
                    else if(word.Equals("LISTRIGHTS",StringComparison.InvariantCultureIgnoreCase)){
                        if(listRights != null){
                            listRights.Add(IMAP_r_u_ListRights.Parse(responseLine));
                        }
                    }
                    else if(word.Equals("MYRIGHTS",StringComparison.InvariantCultureIgnoreCase)){
                        if(myRights != null){
                            myRights.Add(IMAP_Response_MyRights.Parse(responseLine));
                        }
                    }

                    #endregion

                    #region Untagged quota related. RFC 2087.

                    else if(word.Equals("QUOTA",StringComparison.InvariantCultureIgnoreCase)){
                        if(quota != null){
                            quota.Add(IMAP_r_u_Quota.Parse(responseLine));
                        }
                    }
                    else if(word.Equals("QUOTAROOT",StringComparison.InvariantCultureIgnoreCase)){
                        if(quotaRoot != null){
                            quotaRoot.Add(IMAP_r_u_QuotaRoot.Parse(responseLine));
                        }
                    }

                    #endregion

                    #region Untagged namespace related. RFC 2342.

                    else if(word.Equals("NAMESPACE",StringComparison.InvariantCultureIgnoreCase)){
                        if(nspace != null){
                            nspace.Add(IMAP_r_u_Namespace.Parse(responseLine));
                        }
                    }

                    #endregion
                }
                // Command continuation response.
                else if(responseLine.StartsWith("+")){
                    return new IMAP_r_ServerStatus("+","+",null,null,"+");
                }
                // Completion status response.
                else{
                    // Command response reading has completed.
                    return IMAP_r_ServerStatus.Parse(responseLine);
                }
            }
        }

        #endregion

        #region method ReadStringLiteral

        /// <summary>
        /// Reads IMAP <b>string-literal</b> from remote endpoint.
        /// </summary>
        /// <param name="count">Number of bytes to read.</param>
        /// <returns>Returns readed string-literal.</returns>
        private string ReadStringLiteral(int count)
        {
            /* RFC 3501 4.3.            
                string-literal = {bytes_count} CRLF      - Number of bytes after CRLF.
                quoted-string  = DQUOTE string DQUOTE    - Normal quoted-string.
            */

            string retVal = this.TcpStream.ReadFixedCountString(count);            
            LogAddRead(count,"Readed string-literal " + count.ToString() + " bytes.");

            return retVal;
        }

        /// <summary>
        /// Reads IMAP <b>string-literal</b> from remote endpoint.
        /// </summary>
        /// <param name="count">Number of bytes to read.</param>
        /// <param name="stream">Stream where to store readed data.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null reference.</exception>
        private void ReadStringLiteral(int count,Stream stream)
        {
            if(stream == null){
                throw new ArgumentNullException("stream");
            }

            this.TcpStream.ReadFixedCount(stream,count);
            LogAddRead(count,"Readed string-literal " + count.ToString() + " bytes.");
        }

        #endregion


        #region Properties implementation

        /// <summary>
        /// Gets session authenticated user identity, returns null if not authenticated.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and IMAP client is not connected.</exception>
        public override GenericIdentity AuthenticatedUserIdentity
        {
            get{ 
                if(this.IsDisposed){
                    throw new ObjectDisposedException(this.GetType().Name);
                }
                if(!this.IsConnected){
				    throw new InvalidOperationException("You must connect first.");
			    }

                return m_pAuthenticatedUser; 
            }
        }

        /// <summary>
        /// Get IMAP server greeting text.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and IMAP client is not connected.</exception>
        public string GreetingText
        {
            get{ 
                if(this.IsDisposed){
                    throw new ObjectDisposedException(this.GetType().Name);
                }
                if(!this.IsConnected){
				    throw new InvalidOperationException("You must connect first.");
			    }

                return m_GreetingText; 
            }
        }

        /// <summary>
        /// Gets IMAP server folder separator.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and IMAP client is not connected.</exception>
        public char FolderSeparator
        {
            get{ 
                if(this.IsDisposed){
                    throw new ObjectDisposedException(this.GetType().Name);
                }
                if(!this.IsConnected){
				    throw new InvalidOperationException("You must connect first.");
			    }

                SendCommand((m_CommandIndex++).ToString("d5") + " LIST \"\" \"\"\r\n");

                List<IMAP_r_u_List> retVal = new List<IMAP_r_u_List>();
                IMAP_r_ServerStatus response = ReadResponse(null,null,null,retVal,null,null,null,null,null,null,null,null,null);
                if(!response.ResponseCode.Equals("OK",StringComparison.InvariantCultureIgnoreCase)){
                    throw new IMAP_ClientException(response.ResponseCode,response.ResponseText);
                }

                if(retVal.Count == 0){
                    throw new Exception("Unexpected result: IMAP server didn't return LIST response for [... LIST \"\" \"\"].");
                }
                else{
                    return retVal[0].HierarchyDelimiter;
                }
            }
        }

        /// <summary>
        /// Gets selected folder. Returns null if no folder selected.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and IMAP client is not connected.</exception>
        public IMAP_Client_SelectedFolder SelectedFolder
        {
            get{ 
                if(this.IsDisposed){
                    throw new ObjectDisposedException(this.GetType().Name);
                }
                if(!this.IsConnected){
				    throw new InvalidOperationException("You must connect first.");
			    }

                return m_pSelectedFolder; 
            }
        }

        #endregion

        #region Events implementation
        
        /// <summary>
        /// This event is raised when IMAP server sends untagged status response.
        /// </summary>
        public event EventHandler<EventArgs<IMAP_r_u>> UntaggedStatusResponse = null;

        #region method OnUntaggedStatusResponse

        /// <summary>
        /// Raises <b>UntaggedStatusResponse</b> event.
        /// </summary>
        /// <param name="response">Untagged response.</param>
        private void OnUntaggedStatusResponse(IMAP_r_u response)
        {
            if(this.UntaggedStatusResponse != null){
                this.UntaggedStatusResponse(this,new EventArgs<IMAP_r_u>(response));
            }
        }

        #endregion
                
        /// <summary>
        /// This event is raised when IMAP server expunges message and sends EXPUNGE response.
        /// </summary>
        public event EventHandler<EventArgs<IMAP_r_u_Expunge>> MessageExpunged = null;

        #region method OnMessageExpunged

        /// <summary>
        /// Raises <b>MessageExpunged</b> event.
        /// </summary>
        /// <param name="response">Expunge response.</param>
        private void OnMessageExpunged(IMAP_r_u_Expunge response)
        {
            if(this.MessageExpunged != null){
                this.MessageExpunged(this,new EventArgs<IMAP_r_u_Expunge>(response));
            }
        }

        #endregion
                
        #endregion
    }
}
