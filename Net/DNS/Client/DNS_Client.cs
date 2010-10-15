using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net.NetworkInformation;
using System.Threading;

namespace LumiSoft.Net.DNS.Client
{
	/// <summary>
	/// Dns client.
	/// </summary>
	/// <example>
	/// <code>
	/// // Set dns servers
	/// Dns_Client.DnsServers = new string[]{"194.126.115.18"};
	/// 
	/// Dns_Client dns = Dns_Client();
	/// 
	/// // Get MX records.
	/// DnsServerResponse resp = dns.Query("lumisoft.ee",QTYPE.MX);
	/// if(resp.ConnectionOk &amp;&amp; resp.ResponseCode == RCODE.NO_ERROR){
	///		MX_Record[] mxRecords = resp.GetMXRecords();
	///		
	///		// Do your stuff
	///	}
	///	else{
	///		// Handle error there, for more exact error info see RCODE 
	///	}	 
	/// 
	/// </code>
	/// </example>
	public class Dns_Client : IDisposable
    {        
        #region class DnsTransaction

        /// <summary>
        /// This class represents DNS client transaction.
        /// </summary>
        private class DnsTransaction : IDisposable
        {
            private DateTime          m_CreateTime;
            private Dns_Client        m_pOwner        = null;
            private int               m_ID            = 1;
            private string            m_QName         = "";
            private int               m_QType         = 0;
            private byte[]            m_pQuery        = null;
            private TimerEx           m_pTimeoutTimer = null;
            private DnsServerResponse m_pResponse     = null;

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="owner">Owner DNS client.</param>
            /// <param name="id">Transaction ID.</param>
            /// <param name="qname">QNAME value.</param>
            /// <param name="qtype">QTYPE value.</param>
            /// <param name="timeout">Timeout in milliseconds.</param>
            /// <param name="query">Raw DNS query.</param>
            /// <exception cref="ArgumentNullException">Is raised when <b>owner</b> or <b>query</b> is null reference.</exception>
            public DnsTransaction(Dns_Client owner,int id,string qname,int qtype,int timeout,byte[] query)
            {
                if(owner == null){
                    throw new ArgumentNullException("owner");
                }
                if(query == null){
                    throw new ArgumentNullException("query");
                }

                m_pOwner = owner;
                m_ID     = id;
                m_pQuery = query;
                m_QName  = qname;
                m_QType  = qtype;

                m_CreateTime    = DateTime.Now;
                m_pTimeoutTimer = new TimerEx(timeout);
                m_pTimeoutTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_pTimeoutTimer_Elapsed);
            }
                        
            #region method Dispose

            /// <summary>
            /// Cleans up any resource being used.
            /// </summary>
            public void Dispose()
            {
                m_pTimeoutTimer.Dispose();
                m_pTimeoutTimer = null;

                m_pOwner.m_pTransactions.Remove(this.ID);
                m_pOwner = null;

                m_pQuery = null;
                m_pResponse = null;

                this.Timeout = null;
                this.Completed = null;
            }

            #endregion


            #region Evants handling

            #region method m_pTimeoutTimer_Elapsed

            private void m_pTimeoutTimer_Elapsed(object sender,System.Timers.ElapsedEventArgs e)
            {
                OnTimeout();
            }

            #endregion

            #endregion


            #region method Start

            /// <summary>
            /// Starts DNS transaction processing.
            /// </summary>
            public void Start()
            {         
                // Send parallel query to DNS server(s).
                foreach(string server in Dns_Client.DnsServers){
                    try{
                        if(Net_Utils.IsIPAddress(server)){
                            IPAddress ip = IPAddress.Parse(server);
                            if(ip.AddressFamily == AddressFamily.InterNetwork){
                                m_pOwner.m_pIPv4Socket.SendTo(m_pQuery,new IPEndPoint(ip,53));
                            }
                            else if(ip.AddressFamily == AddressFamily.InterNetworkV6){
                                m_pOwner.m_pIPv6Socket.SendTo(m_pQuery,new IPEndPoint(ip,53));
                            }
                        }
                    }
                    catch{
                    }
                }

                m_pTimeoutTimer.Start();
            }

            #endregion


            #region method ProcessResponse

            /// <summary>
            /// Processes DNS server response through this transaction.
            /// </summary>
            /// <param name="response">DNS server response.</param>
            /// <exception cref="ArgumentNullException">Is raised when <b>response</b> is null reference.</exception>
            internal void ProcessResponse(DnsServerResponse response)
            {
                if(response == null){
                    throw new ArgumentNullException("response");
                }

                m_pResponse = response;

                OnCompleted();
            }

            #endregion


            #region Properties implementaion

            /// <summary>
            /// Gets DNS transaction ID.
            /// </summary>
            public int ID
            {
                get{ return m_ID; }
            }

            /// <summary>
            /// Gets QNAME value.
            /// </summary>
            public string QName
            {
                get{ return m_QName; }
            }

            /// <summary>
            /// Gets QTYPE value.
            /// </summary>
            public int QType
            {
                get{ return m_QType; }
            }

            /// <summary>
            /// Gets DNS server response. Value null means no response received yet.
            /// </summary>
            public DnsServerResponse Response
            {
                get{ return m_pResponse; }
            }

            #endregion

            #region Events implementation

            /// <summary>
            /// This event is raised when DNS transaction times out.
            /// </summary>
            public event EventHandler Timeout = null;

            #region method OnTimeout

            /// <summary>
            /// Raises <b>Timeout</b> event.
            /// </summary>
            private void OnTimeout()
            {
                if(this.Timeout != null){
                    this.Timeout(this,new EventArgs());
                }
            }

            #endregion

            /// <summary>
            /// This event is raised when DNS server response received.
            /// </summary>
            public event EventHandler Completed = null;

            #region method OnCompleted

            /// <summary>
            /// Raises <b>Completed</b> event.
            /// </summary>
            private void OnCompleted()
            {
                if(this.Completed != null){
                    this.Completed(this,new EventArgs());
                }
            }

            #endregion

            #endregion
        }

        #endregion
        
        private static IPAddress[] m_DnsServers  = null;
		private static bool        m_UseDnsCache = true;
		private static int         m_ID          = 100;
        // 
        private bool                           m_IsDisposed    = false;
        private Dictionary<int,DnsTransaction> m_pTransactions = null;
        private Socket                         m_pIPv4Socket   = null;
        private Socket                         m_pIPv6Socket   = null;

		/// <summary>
		/// Static constructor.
		/// </summary>
		static Dns_Client()
		{
			// Try to get system dns servers
			try{
				List<IPAddress> dnsServers = new List<IPAddress>();
                foreach(NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces()){
                    if(nic.OperationalStatus == OperationalStatus.Up){
                        foreach(IPAddress ip in nic.GetIPProperties().DnsAddresses){
                            if(ip.AddressFamily == AddressFamily.InterNetwork){
                                if(!dnsServers.Contains(ip)){
                                    dnsServers.Add(ip);
                                }
                            }
                        }
                    }
                }

                m_DnsServers = dnsServers.ToArray();
			}
			catch{
			}
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		public Dns_Client()
		{
            m_pTransactions = new Dictionary<int,DnsTransaction>();

            m_pIPv4Socket = new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp);
            m_pIPv4Socket.Bind(new IPEndPoint(IPAddress.Any,0));

            if(Socket.OSSupportsIPv6){
                m_pIPv6Socket = new Socket(AddressFamily.InterNetworkV6,SocketType.Dgram,ProtocolType.Udp);
                m_pIPv6Socket.Bind(new IPEndPoint(IPAddress.IPv6Any,0));

                StartWaitingIPv6Packet();
            }

            StartWaitingIPv4Packet();
        }

        #region method Dispose

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        public void Dispose()
        {
            if(m_IsDisposed){
                return;
            }
            m_IsDisposed = true;

            m_pIPv4Socket.Close();
            m_pIPv4Socket = null;

            if(m_pIPv6Socket != null){
                m_pIPv6Socket.Close();
                m_pIPv6Socket = null;
            }

            m_pTransactions = null;
        }

        #endregion


        #region method Query

        /// <summary>
		/// Queries server with specified query.
		/// </summary>
		/// <param name="queryText">Query text. It depends on queryType.</param>
		/// <param name="queryType">Query type.</param>
		/// <returns>Returns DSN server response.</returns>
		public DnsServerResponse Query(string queryText,DNS_QType queryType)
		{
            return Query(queryText,queryType,2000);
        }

		/// <summary>
		/// Queries server with specified query.
		/// </summary>
		/// <param name="queryText">Query text. It depends on queryType.</param>
		/// <param name="queryType">Query type.</param>
        /// <param name="timeout">Query timeout in milli seconds.</param>
		/// <returns>Returns DSN server response.</returns>
		public DnsServerResponse Query(string queryText,DNS_QType queryType,int timeout)
		{
			if(queryType == DNS_QType.PTR){
				string ip = queryText;

				// See if IP is ok.
				IPAddress ipA = IPAddress.Parse(ip);		
				queryText = "";

				// IPv6
				if(ipA.AddressFamily == AddressFamily.InterNetworkV6){
					// 4321:0:1:2:3:4:567:89ab
					// would be
					// b.a.9.8.7.6.5.0.4.0.0.0.3.0.0.0.2.0.0.0.1.0.0.0.0.0.0.0.1.2.3.4.IP6.ARPA
					
					char[] ipChars = ip.Replace(":","").ToCharArray();
					for(int i=ipChars.Length - 1;i>-1;i--){
						queryText += ipChars[i] + ".";
					}
					queryText += "IP6.ARPA";
				}
				// IPv4
				else{
					// 213.35.221.186
					// would be
					// 186.221.35.213.in-addr.arpa

					string[] ipParts = ip.Split('.');
					//--- Reverse IP ----------
					for(int i=3;i>-1;i--){
						queryText += ipParts[i] + ".";
					}
					queryText += "in-addr.arpa";
				}
			}

			return QueryServer(timeout,queryText,queryType,1);
		}

		#endregion

        #region method GetHostAddresses

        /// <summary>
        /// Gets specified host IP addresses(A and AAAA).
        /// </summary>
        /// <param name="host">Host name.</param>
        /// <returns>Returns specified host IP addresses.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>host</b> is null reference.</exception>
        public IPAddress[] GetHostAddresses(string host)
        {
            if(host == null){
                throw new ArgumentNullException("host");
            }

            List<IPAddress> retVal = new List<IPAddress>();

            // This is probably NetBios name
			if(host.IndexOf(".") == -1){
				return System.Net.Dns.GetHostEntry(host).AddressList;
			}
            else{
                DnsServerResponse response = Query(host,DNS_QType.A);
                if(response.ResponseCode != DNS_RCode.NO_ERROR){
                    throw new DNS_ClientException(response.ResponseCode);
                }

                foreach(DNS_rr_A record in response.GetARecords()){
                    retVal.Add(record.IP);
                }

                response = Query(host,DNS_QType.AAAA);
                if(response.ResponseCode != DNS_RCode.NO_ERROR){
                    throw new DNS_ClientException(response.ResponseCode);
                }

                foreach(DNS_rr_AAAA record in response.GetAAAARecords()){
                    retVal.Add(record.IP);
                }
            }

            return retVal.ToArray();
        }

        #endregion


        #region static method Resolve

        /// <summary>
        /// Resolves host names to IP addresses.
        /// </summary>
        /// <param name="hosts">Host names to resolve.</param>
        /// <returns>Returns specified hosts IP addresses.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>hosts</b> is null.</exception>
        public static IPAddress[] Resolve(string[] hosts)
        {
            if(hosts == null){
                throw new ArgumentNullException("hosts");
            }

            List<IPAddress> retVal = new List<IPAddress>();
            foreach(string host in hosts){
                IPAddress[] addresses = Resolve(host);
                foreach(IPAddress ip in addresses){
                    if(!retVal.Contains(ip)){
                        retVal.Add(ip);
                    }
                }
            }

            return retVal.ToArray();
        }

		/// <summary>
		/// Resolves host name to IP addresses.
		/// </summary>
		/// <param name="host">Host name or IP address.</param>
		/// <returns>Return specified host IP addresses.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>host</b> is null.</exception>
		public static IPAddress[] Resolve(string host)
		{
            if(host == null){
                throw new ArgumentNullException("host");
            }

			// If hostName_IP is IP
			try{
				return new IPAddress[]{IPAddress.Parse(host)};
			}
			catch{
			}

			// This is probably NetBios name
			if(host.IndexOf(".") == -1){
				return System.Net.Dns.GetHostEntry(host).AddressList;
			}
			else{
				// hostName_IP must be host name, try to resolve it's IP
				using(Dns_Client dns = new Dns_Client()){
				    DnsServerResponse resp = dns.Query(host,DNS_QType.A);
				    if(resp.ResponseCode == DNS_RCode.NO_ERROR){
					    DNS_rr_A[] records = resp.GetARecords();
					    IPAddress[] retVal = new IPAddress[records.Length];
					    for(int i=0;i<records.Length;i++){
						    retVal[i] = records[i].IP;
					    }

					    return retVal;
				    }
				    else{
					    throw new Exception(resp.ResponseCode.ToString());
				    }
                }
			}
		}

		#endregion


        #region method StartWaitingIPv4Packet

        /// <summary>
        /// Starts waiting DNS server response.
        /// </summary>
        private void StartWaitingIPv4Packet()
        {
            byte[] buffer = new byte[8000];
            EndPoint rtpRemoteEP = new IPEndPoint(IPAddress.Any,0);
            m_pIPv4Socket.BeginReceiveFrom(
                buffer,
                0,
                buffer.Length,
                SocketFlags.None,
                ref rtpRemoteEP,
                new AsyncCallback(this.IPv4ReceiveCompleted),
                buffer
            );
        }

        #endregion

        #region method StartWaitingIPv6Packet

        /// <summary>
        /// Starts waiting DNS server response.
        /// </summary>
        private void StartWaitingIPv6Packet()
        {
            byte[] buffer = new byte[8000];
            EndPoint rtpRemoteEP = new IPEndPoint(IPAddress.IPv6Any,0);
            m_pIPv6Socket.BeginReceiveFrom(
                buffer,
                0,
                buffer.Length,
                SocketFlags.None,
                ref rtpRemoteEP,
                new AsyncCallback(this.IPv6ReceiveCompleted),
                buffer
            );
        }

        #endregion

        #region method IPv4ReceiveCompleted

        /// <summary>
        /// Is called when IPv4 socket has received data.
        /// </summary>
        /// <param name="ar">The result of the asynchronous operation.</param>
        private void IPv4ReceiveCompleted(IAsyncResult ar)
        {
            try{
                if(m_IsDisposed){
                    return;
                }

                EndPoint remoteEP = new IPEndPoint(IPAddress.Any,0);
                int count = m_pIPv4Socket.EndReceiveFrom(ar,ref remoteEP);

                byte[] response = new byte[count];
                Array.Copy((byte[])ar.AsyncState,response,count);

                DnsServerResponse serverResponse = ParseQuery(response);
                DnsTransaction transaction = null;
                // Pass response to transaction.
                if(m_pTransactions.TryGetValue(serverResponse.ID,out transaction)){
                    transaction.ProcessResponse(serverResponse);
                }
                // No such transaction or transaction has timed out before answer received.
                //else{
                //}

                // Cache query.
                if(m_UseDnsCache && serverResponse.ResponseCode == DNS_RCode.NO_ERROR){
	                DnsCache.AddToCache(transaction.QName,transaction.QType,serverResponse);
		        }
            }
            catch{
                // Skip receiving socket errors.                
            }

            try{
                StartWaitingIPv4Packet();
            }
            catch{
            }
        }

        #endregion

        #region method IPv6ReceiveCompleted

        /// <summary>
        /// Is called when IPv6 socket has received data.
        /// </summary>
        /// <param name="ar">The result of the asynchronous operation.</param>
        private void IPv6ReceiveCompleted(IAsyncResult ar)
        {
            try{
                if(m_IsDisposed){
                    return;
                }

                EndPoint remoteEP = new IPEndPoint(IPAddress.Any,0);
                int count = m_pIPv6Socket.EndReceiveFrom(ar,ref remoteEP);

                byte[] response = new byte[count];
                Array.Copy((byte[])ar.AsyncState,response,count);

                DnsServerResponse serverResponse = ParseQuery(response);
                DnsTransaction transaction = null;
                // Pass response to transaction.
                if(m_pTransactions.TryGetValue(serverResponse.ID,out transaction)){
                    transaction.ProcessResponse(serverResponse);
                }
                // No such transaction or transaction has timed out before answer received.
                //else{
                //}

                // Cache query.
                if(m_UseDnsCache && serverResponse.ResponseCode == DNS_RCode.NO_ERROR){
	                DnsCache.AddToCache(transaction.QName,transaction.QType,serverResponse);
		        }
            }
            catch{
                // Skip receiving socket errors.                
            }

            try{
                StartWaitingIPv6Packet();
            }
            catch{
            }
        }

        #endregion


        #region method QueryServer

        /// <summary>
		/// Sends query to server.
		/// </summary>
        /// <param name="timeout">Query timeout in milli seconds.</param>
		/// <param name="qname">Query text.</param>
		/// <param name="qtype">Query type.</param>
		/// <param name="qclass">Query class.</param>
		/// <returns>Returns DNS server response.</returns>
		private DnsServerResponse QueryServer(int timeout,string qname,DNS_QType qtype,int qclass)
		{	
			if(m_DnsServers == null || m_DnsServers.Length == 0){
				throw new Exception("Dns server isn't specified !");
			}

			// See if query is in cache
			if(m_UseDnsCache){
				DnsServerResponse resopnse = DnsCache.GetFromCache(qname,(int)qtype);
				if(resopnse != null){
					return resopnse;
				}
			}                           

			int    queryID = Dns_Client.ID;
			byte[] query   = CreateQuery(queryID,qname,qtype,qclass);
            
            // Create transcation and start processing it.            
            using(DnsTransaction transaction = new DnsTransaction(this,queryID,qname,(int)qtype,timeout,query)){
                ManualResetEvent wait = new ManualResetEvent(false);

                transaction.Timeout += delegate(object s,EventArgs e){
                    wait.Set();
                };
                transaction.Completed += delegate(object s,EventArgs e){
                    wait.Set();
                };
                m_pTransactions.Add(transaction.ID,transaction);

                // Start transaction processing and wait transaction to complete.
                transaction.Start();                
                wait.WaitOne();

                // DNS server response received.
                if(transaction.Response != null){
                    return transaction.Response;
                }
                // No server response - timeout.
                else{
                    throw new Exception("Timeout - no response from DNS server.");
                }
            }
		}

		#endregion

		#region method CreateQuery

		/// <summary>
		/// Creates new query.
		/// </summary>
		/// <param name="ID">Query ID.</param>
		/// <param name="qname">Query text.</param>
		/// <param name="qtype">Query type.</param>
		/// <param name="qclass">Query class.</param>
		/// <returns></returns>
		private byte[] CreateQuery(int ID,string qname,DNS_QType qtype,int qclass)
		{
			byte[] query = new byte[512];

			//---- Create header --------------------------------------------//
			// Header is first 12 bytes of query

			/* 4.1.1. Header section format
										  1  1  1  1  1  1
			0  1  2  3  4  5  6  7  8  9  0  1  2  3  4  5
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                      ID                       |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|QR|   Opcode  |AA|TC|RD|RA|   Z    |   RCODE   |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                    QDCOUNT                    |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                    ANCOUNT                    |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                    NSCOUNT                    |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                    ARCOUNT                    |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			
			QR  A one bit field that specifies whether this message is a
                query (0), or a response (1).
				
			OPCODE          A four bit field that specifies kind of query in this
                message.  This value is set by the originator of a query
                and copied into the response.  The values are:

                0               a standard query (QUERY)

                1               an inverse query (IQUERY)

                2               a server status request (STATUS)
				
			*/

			//--------- Header part -----------------------------------//
			query[0]  = (byte) (ID >> 8); query[1]  = (byte) (ID & 0xFF);
			query[2]  = (byte) 1;         query[3]  = (byte) 0;
			query[4]  = (byte) 0;         query[5]  = (byte) 1;
			query[6]  = (byte) 0;         query[7]  = (byte) 0;
			query[8]  = (byte) 0;         query[9]  = (byte) 0;
			query[10] = (byte) 0;         query[11] = (byte) 0;
			//---------------------------------------------------------//

			//---- End of header --------------------------------------------//


			//----Create query ------------------------------------//

			/* 	Rfc 1035 4.1.2. Question section format
											  1  1  1  1  1  1
			0  1  2  3  4  5  6  7  8  9  0  1  2  3  4  5
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                                               |
			/                     QNAME                     /
			/                                               /
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                     QTYPE                     |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                     QCLASS                    |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			
			QNAME
				a domain name represented as a sequence of labels, where
				each label consists of a length octet followed by that
				number of octets.  The domain name terminates with the
				zero length octet for the null label of the root.  Note
				that this field may be an odd number of octets; no
				padding is used.
			*/
			string[] labels = qname.Split(new char[] {'.'});
			int position = 12;
					
			// Copy all domain parts(labels) to query
			// eg. lumisoft.ee = 2 labels, lumisoft and ee.
			// format = label.length + label(bytes)
			foreach(string label in labels){
				// add label lenght to query
				query[position++] = (byte)(label.Length); 

				// convert label string to byte array
				byte[] b = Encoding.ASCII.GetBytes(label);
				b.CopyTo(query,position);

				// Move position by label length
				position += b.Length;
			}

			// Terminate domain (see note above)
			query[position++] = (byte) 0; 
			
			// Set QTYPE 
			query[position++] = (byte) 0;
			query[position++] = (byte)qtype;
				
			// Set QCLASS
			query[position++] = (byte) 0;
			query[position++] = (byte)qclass;
			//-------------------------------------------------------//
			
			return query;
		}

		#endregion

		#region method GetQName

		internal static bool GetQName(byte[] reply,ref int offset,ref string name)
		{				
			try{
				// Do while not terminator
				while(reply[offset] != 0){
					
					// Check if it's pointer(In pointer first two bits always 1)
					bool isPointer = ((reply[offset] & 0xC0) == 0xC0);
					
					// If pointer
					if(isPointer){
						// Pointer location number is 2 bytes long
						// 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7  # byte 2 # 0 | 1 | 2 | | 3 | 4 | 5 | 6 | 7
						// empty | < ---- pointer location number --------------------------------->
						int pStart = ((reply[offset] & 0x3F) << 8) | (reply[++offset]);
						offset++;
			
						return GetQName(reply,ref pStart,ref name);
					}
					else{
						// label length (length = 8Bit and first 2 bits always 0)
						// 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7
						// empty | lablel length in bytes 
						int labelLength = (reply[offset] & 0x3F);
						offset++;
						
						// Copy label into name 
						name += Encoding.ASCII.GetString(reply,offset,labelLength);
						offset += labelLength;
					}
									
					// If the next char isn't terminator,
					// label continues - add dot between two labels
					if (reply[offset] != 0){
						name += ".";
					}					
				}

				// Move offset by terminator length
				offset++;

				return true;
			}
			catch(Exception x){
				return false;
			}
		}

		#endregion

		#region method ParseQuery

		/// <summary>
		/// Parses query.
		/// </summary>
		/// <param name="reply">Dns server reply.</param>
		/// <returns></returns>
		private DnsServerResponse ParseQuery(byte[] reply)
		{	
			//--- Parse headers ------------------------------------//

			/* RFC 1035 4.1.1. Header section format
			 
											1  1  1  1  1  1
			  0  1  2  3  4  5  6  7  8  9  0  1  2  3  4  5
			 +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			 |                      ID                       |
			 +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			 |QR|   Opcode  |AA|TC|RD|RA|   Z    |   RCODE   |
			 +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			 |                    QDCOUNT                    |
			 +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			 |                    ANCOUNT                    |
			 +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			 |                    NSCOUNT                    |
			 +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			 |                    ARCOUNT                    |
			 +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			 
			QDCOUNT
				an unsigned 16 bit integer specifying the number of
				entries in the question section.

			ANCOUNT
				an unsigned 16 bit integer specifying the number of
				resource records in the answer section.
				
			NSCOUNT
			    an unsigned 16 bit integer specifying the number of name
                server resource records in the authority records section.

			ARCOUNT
			    an unsigned 16 bit integer specifying the number of
                resource records in the additional records section.
				
			*/
		
			// Get reply code
			int       id                     = (reply[0]  << 8 | reply[1]);
			OPCODE    opcode                 = (OPCODE)((reply[2] >> 3) & 15);
			DNS_RCode replyCode              = (DNS_RCode)(reply[3]  & 15);	
			int       queryCount             = (reply[4]  << 8 | reply[5]);
			int       answerCount            = (reply[6]  << 8 | reply[7]);
			int       authoritiveAnswerCount = (reply[8]  << 8 | reply[9]);
			int       additionalAnswerCount  = (reply[10] << 8 | reply[11]);
			//---- End of headers ---------------------------------//
		
			int pos = 12;

			//----- Parse question part ------------//
			for(int q=0;q<queryCount;q++){
				string dummy = "";
				GetQName(reply,ref pos,ref dummy);
				//qtype + qclass
				pos += 4;
			}
			//--------------------------------------//

			// 1) parse answers
			// 2) parse authoritive answers
			// 3) parse additional answers
			List<DNS_rr> answers = ParseAnswers(reply,answerCount,ref pos);
			List<DNS_rr> authoritiveAnswers = ParseAnswers(reply,authoritiveAnswerCount,ref pos);
			List<DNS_rr> additionalAnswers = ParseAnswers(reply,additionalAnswerCount,ref pos);

			return new DnsServerResponse(true,id,replyCode,answers,authoritiveAnswers,additionalAnswers);
		}

		#endregion

		#region method ParseAnswers

		/// <summary>
		/// Parses specified count of answers from query.
		/// </summary>
		/// <param name="reply">Server returned query.</param>
		/// <param name="answerCount">Number of answers to parse.</param>
		/// <param name="offset">Position from where to start parsing answers.</param>
		/// <returns></returns>
		private List<DNS_rr> ParseAnswers(byte[] reply,int answerCount,ref int offset)
		{
			/* RFC 1035 4.1.3. Resource record format
			 
										   1  1  1  1  1  1
			 0  1  2  3  4  5  6  7  8  9  0  1  2  3  4  5
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                                               |
			/                                               /
			/                      NAME                     /
			|                                               |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                      TYPE                     |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                     CLASS                     |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                      TTL                      |
			|                                               |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                   RDLENGTH                    |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--|
			/                     RDATA                     /
			/                                               /
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			*/

			List<DNS_rr> answers = new List<DNS_rr>();
			//---- Start parsing answers ------------------------------------------------------------------//
			for(int i=0;i<answerCount;i++){        
				string name = "";
				if(!GetQName(reply,ref offset,ref name)){
					break;
				}

				int type     = reply[offset++] << 8  | reply[offset++];
				int rdClass  = reply[offset++] << 8  | reply[offset++];
				int ttl      = reply[offset++] << 24 | reply[offset++] << 16 | reply[offset++] << 8  | reply[offset++];
				int rdLength = reply[offset++] << 8  | reply[offset++];
                				
                if((DNS_QType)type == DNS_QType.A){
                    answers.Add(DNS_rr_A.Parse(reply,ref offset,rdLength,ttl));
                }
                else if((DNS_QType)type == DNS_QType.NS){
                    answers.Add(DNS_rr_NS.Parse(reply,ref offset,rdLength,ttl));
                }
                else if((DNS_QType)type == DNS_QType.CNAME){
                    answers.Add(DNS_rr_CNAME.Parse(reply,ref offset,rdLength,ttl));
                }
                else if((DNS_QType)type == DNS_QType.SOA){
                    answers.Add(DNS_rr_SOA.Parse(reply,ref offset,rdLength,ttl));
                }
                else if((DNS_QType)type == DNS_QType.PTR){
                    answers.Add(DNS_rr_PTR.Parse(reply,ref offset,rdLength,ttl));
                }
                else if((DNS_QType)type == DNS_QType.HINFO){
                    answers.Add(DNS_rr_HINFO.Parse(reply,ref offset,rdLength,ttl));
                }
                else if((DNS_QType)type == DNS_QType.MX){
                    answers.Add(DNS_rr_MX.Parse(reply,ref offset,rdLength,ttl));
                }
                else if((DNS_QType)type == DNS_QType.TXT){
                    answers.Add(DNS_rr_TXT.Parse(reply,ref offset,rdLength,ttl));
                }
                else if((DNS_QType)type == DNS_QType.AAAA){
                    answers.Add(DNS_rr_AAAA.Parse(reply,ref offset,rdLength,ttl));
                }
                else if((DNS_QType)type == DNS_QType.SRV){
                    answers.Add(DNS_rr_SRV.Parse(reply,ref offset,rdLength,ttl));
                }
                else if((DNS_QType)type == DNS_QType.NAPTR){
                    answers.Add(DNS_rr_NAPTR.Parse(reply,ref offset,rdLength,ttl));
                }
                else if((DNS_QType)type == DNS_QType.SPF){
                    answers.Add(DNS_rr_SPF.Parse(reply,ref offset,rdLength,ttl));
                }
                else{
                    // Unknown record, skip it.
                    offset += rdLength;
                }
			}

			return answers;
		}

		#endregion

        #region method ReadCharacterString

        /// <summary>
        /// Reads character-string from spefcified data and offset.
        /// </summary>
        /// <param name="data">Data from where to read.</param>
        /// <param name="offset">Offset from where to start reading.</param>
        /// <returns>Returns readed string.</returns>
        internal static string ReadCharacterString(byte[] data,ref int offset)
        {
            /* RFC 1035 3.3.
                <character-string> is a single length octet followed by that number of characters. 
                <character-string> is treated as binary information, and can be up to 256 characters 
                in length (including the length octet).
            */

            int dataLength = (int)data[offset++];
            string retVal = Encoding.Default.GetString(data,offset,dataLength);
            offset += dataLength;

            return retVal;
        }

        #endregion


        #region Properties Implementation

        /// <summary>
		/// Gets or sets dns servers.
		/// </summary>
        /// <exception cref="ArgumentNullException">Is raised when null value is passed.</exception>
		public static string[] DnsServers
		{
			get{
                string[] retVal = new string[m_DnsServers.Length];
                for(int i=0;i<m_DnsServers.Length;i++){
                    retVal[i] = m_DnsServers[i].ToString();
                }

                return retVal; 
            }

			set{
                if(value == null){
                    throw new ArgumentNullException();
                }

                IPAddress[] retVal = new IPAddress[value.Length];
                for(int i=0;i<value.Length;i++){
                    retVal[i] = IPAddress.Parse(value[i]);
                }

                m_DnsServers = retVal; 
            }
		}

		/// <summary>
		/// Gets or sets if to use dns caching.
		/// </summary>
		public static bool UseDnsCache
		{
			get{ return m_UseDnsCache; }

			set{ m_UseDnsCache = value; }
		}

		/// <summary>
		/// Get next query ID.
		/// </summary>
		internal static int ID
		{
			get{
				if(m_ID >= 65535){
					m_ID = 100;
				}
				return m_ID++; 
			}
		}

		#endregion

	}
}
