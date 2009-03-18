using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

using LumiSoft.Net.IO;

namespace LumiSoft.Net
{
    /// <summary>
    /// Common utility methods.
    /// </summary>
    public class Net_Utils
    {
        #region static method GetLocalHostName

        /// <summary>
        /// Gets local host name or argument <b>hostName</b> value if it's specified.
        /// </summary>
        /// <param name="hostName">Host name or null.</param>
        /// <returns>Returns local host name or argument <b>hostName</b> value if it's specified.</returns>
        public static string GetLocalHostName(string hostName)
        {
            if(string.IsNullOrEmpty(hostName)){
                return System.Net.Dns.GetHostName();
            }
            else{
                return hostName;
            }
        }

        #endregion

        #region static method CompareArray

        /// <summary>
        /// Compares if specified array itmes equals.
        /// </summary>
        /// <param name="array1">Array 1.</param>
        /// <param name="array2">Array 2</param>
        /// <returns>Returns true if both arrays are equal.</returns>
        public static bool CompareArray(Array array1,Array array2)
        {
            return CompareArray(array1,array2,array2.Length);
        }

        /// <summary>
        /// Compares if specified array itmes equals.
        /// </summary>
        /// <param name="array1">Array 1.</param>
        /// <param name="array2">Array 2</param>
        /// <param name="array2Count">Number of bytes in array 2 used for compare.</param>
        /// <returns>Returns true if both arrays are equal.</returns>
        public static bool CompareArray(Array array1,Array array2,int array2Count)
        {
            if(array1 == null && array2 == null){
                return true;
            }
            if(array1 == null && array2 != null){
                return false;
            }
            if(array1 != null && array2 == null){
                return false;
            }            
            if(array1.Length != array2Count){
                return false;
            }
            else{
                for(int i=0;i<array1.Length;i++){
                    if(!array1.GetValue(i).Equals(array2.GetValue(i))){
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion

        #region static method ReverseArray

        /// <summary>
        /// Reverses the specified array elements.
        /// </summary>
        /// <param name="array">Array elements to reverse.</param>
        /// <returns>Returns array with reversed items.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>array</b> is null.</exception>
        public static Array ReverseArray(Array array)
        {
            if(array == null){
                throw new ArgumentNullException("array");
            }

            Array.Reverse(array);

            return array;
        }

        #endregion

        #region static method StreamCopy

        /// <summary>
        /// Copies <b>source</b> stream data to <b>target</b> stream.
        /// </summary>
        /// <param name="source">Source stream. Reading starts from stream current position.</param>
        /// <param name="target">Target stream. Writing starts from stream current position.</param>
        /// <param name="blockSize">Specifies transfer block size in bytes.</param>
        /// <returns>Returns number of bytes copied.</returns>
        public static long StreamCopy(Stream source,Stream target,int blockSize)
        {
            if(source == null){
                throw new ArgumentNullException("source");
            }
            if(target == null){
                throw new ArgumentNullException("target");
            }
            if(blockSize < 1024){
                throw new ArgumentException("Argument 'blockSize' value must be >= 1024.");
            }

            byte[] buffer      = new byte[blockSize];
            long   totalReaded = 0;            
            while(true){
                int readedCount = source.Read(buffer,0,buffer.Length);
                // We reached end of stream, we readed all data sucessfully.
                if(readedCount == 0){
                    return totalReaded;
                }
                else{
                    target.Write(buffer,0,readedCount);
                    totalReaded += readedCount;
                }
            }
        }

        #endregion


        #region static method IsIPAddress

        /// <summary>
        /// Gets if the specified string value is IP address.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <returns>Returns true if specified value is IP address.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        public static bool IsIPAddress(string value)
        {
            if(value == null){
                throw new ArgumentNullException("value");
            }

            IPAddress ip = null;

            return IPAddress.TryParse(value,out ip);
        }

        #endregion

        #region static method IsMulticastAddress

        /// <summary>
        /// Gets if the specified IP address is multicast address.
        /// </summary>
        /// <param name="ip">IP address.</param>
        /// <returns>Returns true if <b>ip</b> is muticast address, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>ip</b> s null reference.</exception>
        public static bool IsMulticastAddress(IPAddress ip)
        {
            if(ip == null){
                throw new ArgumentNullException("ip");
            }

            // IPv4 multicast 224.0.0.0 to 239.255.255.255

            if(ip.IsIPv6Multicast){
                return true;
            }
            else if(ip.AddressFamily == AddressFamily.InterNetwork){
                byte[] bytes = ip.GetAddressBytes();
                if(bytes[0] >= 224 && bytes[0] <= 239){
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region static method IsPrivateIP

        /// <summary>
        /// Gets if specified IP address is private LAN IP address. For example 192.168.x.x is private ip.
        /// </summary>
        /// <param name="ip">IP address to check.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>ip</b> is null reference.</exception>
        /// <returns>Returns true if IP is private IP.</returns>
        public static bool IsPrivateIP(string ip)
        {
            if(ip == null){
                throw new ArgumentNullException("ip");
            }

            return IsPrivateIP(IPAddress.Parse(ip));
        }

        /// <summary>
        /// Gets if specified IP address is private LAN IP address. For example 192.168.x.x is private ip.
        /// </summary>
        /// <param name="ip">IP address to check.</param>
        /// <returns>Returns true if IP is private IP.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>ip</b> is null reference.</exception>
        public static bool IsPrivateIP(IPAddress ip)
        {
            if(ip == null){
                throw new ArgumentNullException("ip");
            }

			if(ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork){
				byte[] ipBytes = ip.GetAddressBytes();

				/* Private IPs:
					First Octet = 192 AND Second Octet = 168 (Example: 192.168.X.X) 
					First Octet = 172 AND (Second Octet >= 16 AND Second Octet <= 31) (Example: 172.16.X.X - 172.31.X.X)
					First Octet = 10 (Example: 10.X.X.X)
					First Octet = 169 AND Second Octet = 254 (Example: 169.254.X.X)

				*/

				if(ipBytes[0] == 192 && ipBytes[1] == 168){
					return true;
				}
				if(ipBytes[0] == 172 && ipBytes[1] >= 16 && ipBytes[1] <= 31){
					return true;
				}
				if(ipBytes[0] == 10){
					return true;
				}
				if(ipBytes[0] == 169 && ipBytes[1] == 254){
					return true;
				}
			}

			return false;
        }

        #endregion

        #region static method ParseIPEndPoint

        /// <summary>
        /// Parses IPEndPoint from the specified string value.
        /// </summary>
        /// <param name="value">IPEndPoint string value.</param>
        /// <returns>Returns parsed IPEndPoint.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public static IPEndPoint ParseIPEndPoint(string value)
        {
            if(value == null){
                throw new ArgumentNullException("value");
            }

            try{
                string[] ip_port = value.Split(':');

                return new IPEndPoint(IPAddress.Parse(ip_port[0]),Convert.ToInt32(ip_port[1]));
            }
            catch(Exception x){
                throw new ArgumentException("Invalid IPEndPoint value.","value",x);
            }
        }

        #endregion


        #region static method IsInteger

		/// <summary>
		/// Checks if specified string is integer(int/long).
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Returns true if specified string is integer.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
		public static bool IsInteger(string value)
		{
            if(value == null){
                throw new ArgumentNullException("value");
            }

            long l = 0;

            return long.TryParse(value,out l);
		}

		#endregion

        #region static method IsAscii

        /// <summary>
        /// Gets if the specified string is ASCII string.
        /// </summary>
        /// <param name="value">String value.</param>
        /// <returns>Returns true if specified string is ASCII string, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        public static bool IsAscii(string value)
        {
            if(value == null){
                throw new ArgumentNullException("value");
            }

            foreach(char c in value){
				if((int)c > 127){ 
					return false;
				}
			}

			return true;
        }

        #endregion


        #region static method IsIoCompletionPortsSupported

        /// <summary>
        /// Gets if IO completion ports supported by OS.
        /// </summary>
        /// <returns></returns>
        public static bool IsIoCompletionPortsSupported()
        {
            Socket s = new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp);
            try{                            
                SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                e.SetBuffer(new byte[0],0,0);
                e.RemoteEndPoint = new IPEndPoint(IPAddress.Loopback,111);
                s.SendToAsync(e);

                return true;
            }
            catch(NotSupportedException nX){
                string dummy = nX.Message;
                
                return false;
            }
            finally{
                s.Close();
            }
        }

        #endregion

        #region static method CreateSocket

        /// <summary>
        /// Creates new socket for the specified end point.
        /// </summary>
        /// <param name="localEP">Local end point.</param>
        /// <param name="protocolType">Protocol type.</param>
        /// <returns>Retruns newly created socket.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>localEP</b> is null reference.</exception>
        public static Socket CreateSocket(IPEndPoint localEP,ProtocolType protocolType)
        {
            if(localEP == null){
                throw new ArgumentNullException("localEP");
            }

            SocketType socketType = SocketType.Stream;
            if(protocolType == ProtocolType.Udp){
                socketType = SocketType.Dgram;
            }
                        
            if(localEP.AddressFamily == AddressFamily.InterNetwork){
                Socket socket = new Socket(AddressFamily.InterNetwork,socketType,protocolType);
                socket.Bind(localEP);

                return socket;
            }
            else if(localEP.AddressFamily == AddressFamily.InterNetworkV6){
                Socket socket = new Socket(AddressFamily.InterNetworkV6,socketType,protocolType);
                socket.Bind(localEP);

                return socket;
            }
            else{
                throw new ArgumentException("Invalid IPEndPoint address family.");
            }
        }

        #endregion


        #region static method ToHex

        /// <summary>
		/// Converts specified data to HEX string.
		/// </summary>
		/// <param name="data">Data to convert.</param>
		/// <returns>Returns hex string.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>data</b> is null reference.</exception>
		public static string ToHex(byte[] data)
		{
            if(data == null){
                throw new ArgumentNullException("data");
            }

			return BitConverter.ToString(data).ToLower().Replace("-","");
		}

		/// <summary>
		/// Converts specified string to HEX string.
		/// </summary>
		/// <param name="text">String to convert.</param>
		/// <returns>Returns hex string.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>text</b> is null reference.</exception>
		public static string ToHex(string text)
		{
            if(text == null){
                throw new ArgumentNullException("text");
            }

			return BitConverter.ToString(Encoding.Default.GetBytes(text)).ToLower().Replace("-","");
		}

		#endregion

        #region static method FromHex

		/// <summary>
		/// Converts hex byte data to normal byte data. Hex data must be in two bytes pairs, for example: 0F,FF,A3,... .
		/// </summary>
		/// <param name="hexData">Hex data.</param>
		/// <returns>Returns decoded data.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>hexData</b> is null reference.</exception>
		public static byte[] FromHex(byte[] hexData)
		{
            if(hexData == null){
                throw new ArgumentNullException("hexData");
            }

			if(hexData.Length < 2 || (hexData.Length / (double)2 != Math.Floor(hexData.Length / (double)2))){
				throw new Exception("Illegal hex data, hex data must be in two bytes pairs, for example: 0F,FF,A3,... .");
			}

			MemoryStream retVal = new MemoryStream(hexData.Length / 2);
			// Loop hex value pairs
			for(int i=0;i<hexData.Length;i+=2){
				byte[] hexPairInDecimal = new byte[2];
				// We need to convert hex char to decimal number, for example F = 15
				for(int h=0;h<2;h++){
					if(((char)hexData[i + h]) == '0'){
						hexPairInDecimal[h] = 0;
					}
					else if(((char)hexData[i + h]) == '1'){
						hexPairInDecimal[h] = 1;
					}
					else if(((char)hexData[i + h]) == '2'){
						hexPairInDecimal[h] = 2;
					}
					else if(((char)hexData[i + h]) == '3'){
						hexPairInDecimal[h] = 3;
					}
					else if(((char)hexData[i + h]) == '4'){
						hexPairInDecimal[h] = 4;
					}
					else if(((char)hexData[i + h]) == '5'){
						hexPairInDecimal[h] = 5;
					}
					else if(((char)hexData[i + h]) == '6'){
						hexPairInDecimal[h] = 6;
					}
					else if(((char)hexData[i + h]) == '7'){
						hexPairInDecimal[h] = 7;
					}
					else if(((char)hexData[i + h]) == '8'){
						hexPairInDecimal[h] = 8;
					}
					else if(((char)hexData[i + h]) == '9'){
						hexPairInDecimal[h] = 9;
					}
					else if(((char)hexData[i + h]) == 'A' || ((char)hexData[i + h]) == 'a'){
						hexPairInDecimal[h] = 10;
					}
					else if(((char)hexData[i + h]) == 'B' || ((char)hexData[i + h]) == 'b'){
						hexPairInDecimal[h] = 11;
					}
					else if(((char)hexData[i + h]) == 'C' || ((char)hexData[i + h]) == 'c'){
						hexPairInDecimal[h] = 12;
					}
					else if(((char)hexData[i + h]) == 'D' || ((char)hexData[i + h]) == 'd'){
						hexPairInDecimal[h] = 13;
					}
					else if(((char)hexData[i + h]) == 'E' || ((char)hexData[i + h]) == 'e'){
						hexPairInDecimal[h] = 14;
					}
					else if(((char)hexData[i + h]) == 'F' || ((char)hexData[i + h]) == 'f'){
						hexPairInDecimal[h] = 15;
					}
				}

				// Join hex 4 bit(left hex cahr) + 4bit(right hex char) in bytes 8 it
				retVal.WriteByte((byte)((hexPairInDecimal[0] << 4) | hexPairInDecimal[1]));
			}

			return retVal.ToArray();
		}

		#endregion

        #region static method FromBase64

        /// <summary>
        /// Decodes specified base64 data.
        /// </summary>
        /// <param name="data">Base64 string.</param>
        /// <returns>Returns decoded data.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>data</b> is null reference.</exception>
        public static byte[] FromBase64(string data)
        {
            if(data == null){
                throw new ArgumentNullException("data");
            }

            Base64 base64 = new Base64();

            return base64.Decode(data,true);
        }

        /// <summary>
        /// Decodes specified base64 data.
        /// </summary>
        /// <param name="data">Base64 data.</param>
        /// <returns>Returns decoded data.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>data</b> is null reference.</exception>
        public static byte[] FromBase64(byte[] data)
        {
            if(data == null){
                throw new ArgumentNullException("data");
            }

            Base64 base64 = new Base64();

            return base64.Decode(data,0,data.Length,true);
        }

        #endregion


        #region static method ComputeMd5

        /// <summary>
        /// Computes md5 hash.
        /// </summary>
        /// <param name="text">Text to hash.</param>
        /// <param name="hex">Specifies if md5 value is returned as hex string.</param>
        /// <returns>Returns md5 value or md5 hex value.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>text</b> is null reference.</exception>
        public static string ComputeMd5(string text,bool hex)
        {
            if(text == null){
                throw new ArgumentNullException("text");
            }

            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();			
			byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(text));

            if(hex){
			    return ToHex(System.Text.Encoding.Default.GetString(hash)).ToLower();
            }
            else{
                return System.Text.Encoding.Default.GetString(hash);
            }
        }

        #endregion

    }
}
