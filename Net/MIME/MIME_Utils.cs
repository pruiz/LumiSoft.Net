using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using LumiSoft.Net.MIME;

namespace LumiSoft.Net.MIME
{
    /// <summary>
    /// Provides MIME related utility methods.
    /// </summary>
    public class MIME_Utils
    {
        #region static method DateTimeToRfc2822

		/// <summary>
		/// Converts date to RFC 2822 date time string.
		/// </summary>
		/// <param name="dateTime">Date time value to convert..</param>
		/// <returns>Returns RFC 2822 date time string.</returns>
		public static string DateTimeToRfc2822(DateTime dateTime)
		{            
            return dateTime.ToString("ddd, dd MMM yyyy HH':'mm':'ss ",System.Globalization.DateTimeFormatInfo.InvariantInfo) + dateTime.ToString("zzz").Replace(":","");
		}

		#endregion

        #region static method ParseRfc2822DateTime

        /// <summary>
        /// Parses RFC 2822 date-time from the specified value.
        /// </summary>
        /// <param name="value">RFC 2822 date-time string value.</param>
        /// <returns>Returns parsed datetime value.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public static DateTime ParseRfc2822DateTime(string value)
        {
            if(value == null){
                throw new ArgumentNullException(value);
            }

            /* RFC 2822 3.
             *      date-time       =       [ day-of-week "," ] date FWS time [CFWS]
             *      day-of-week     =       ([FWS] day-name) / obs-day-of-week
             *      day-name        =       "Mon" / "Tue" / "Wed" / "Thu" / "Fri" / "Sat" / "Sun"
             *      date            =       day month year 
             *      year            =       4*DIGIT / obs-year
             *      month           =       (FWS month-name FWS) / obs-month
             *      month-name      =       "Jan" / "Feb" / "Mar" / "Apr" / "May" / "Jun" / "Jul" / "Aug" / "Sep" / "Oct" / "Nov" / "Dec"
             *      day             =       ([FWS] 1*2DIGIT) / obs-day
             *      time            =       time-of-day FWS zone
             *      time-of-day     =       hour ":" minute [ ":" second ]
             *      hour            =       2DIGIT / obs-hour
             *      minute          =       2DIGIT / obs-minute
             *      second          =       2DIGIT / obs-second
             *      zone            =       (( "+" / "-" ) 4DIGIT) / obs-zone
             * 
             *      The date and time-of-day SHOULD express local time.
            */

            try{
                MIME_Reader r = new MIME_Reader(value);
                string v = r.Atom();
                // Skip optional [ day-of-week "," ] and read "day".
                if(v.Length == 3){
                    r.Char(true);
                    v = r.Atom();
                }
                int day    = Convert.ToInt32(v);
                v = r.Atom().ToLower();
                int month  = 1;
                if(v == "jan"){
                    month  = 1;
                }
                else if(v == "feb"){
                    month  = 2;
                }
                else if(v == "mar"){
                    month  = 3;
                }
                else if(v == "apr"){
                    month  = 4;
                }
                else if(v == "may"){
                    month  = 5;
                }
                else if(v == "jun"){
                    month  = 6;
                }
                else if(v == "jul"){
                    month  = 7;
                }
                else if(v == "aug"){
                    month  = 8;
                }
                else if(v == "sep"){
                    month  = 9;
                }
                else if(v == "oct"){
                    month  = 10;
                }
                else if(v == "nov"){
                    month  = 11;
                }
                else if(v == "dec"){
                    month  = 12;
                }
                else{
                    throw new ArgumentException("Invalid month-name value '" + value + "'.");
                }
                int year   = Convert.ToInt32(r.Atom());
                int hour   = Convert.ToInt32(r.Atom());
                r.Char(true);
                int minute = Convert.ToInt32(r.Atom());
                int second = 0;
                // We have optional "second".
                if(r.Peek(true) == ':'){
                    r.Char(true);
                    second = Convert.ToInt32(r.Atom());
                }
                int timeZoneMinutes = 0;
                v = r.Atom();
                // We have RFC 2822 date. For example: +2000.
                if(v[0] == '+' || v[0] == '-'){
                    if(v[0] == '+'){
                        timeZoneMinutes = (Convert.ToInt32(v.Substring(1,2)) * 60 + Convert.ToInt32(v.Substring(3,2)));
                    }
                    else{
                        timeZoneMinutes = -(Convert.ToInt32(v.Substring(1,2)) * 60 + Convert.ToInt32(v.Substring(3,2)));
                    }
                }
                // We have RFC 822 date with abbrevated time zone name. For example: GMT.
                else{
                    v = v.ToUpper();

                    #region time zones

                    // Alpha Time Zone (military).
                    if(v == "A"){
                        timeZoneMinutes = ((01 * 60) + 00);
                    }
                    // Australian Central Daylight Time.
                    else if(v == "ACDT"){
                        timeZoneMinutes = ((10 * 60) + 30);
                    }
                    // Australian Central Standard Time.
                    else if(v == "ACST"){
                        timeZoneMinutes = ((09 * 60) + 30);
                    }
                    // Atlantic Daylight Time.
                    else if(v == "ADT"){
                        timeZoneMinutes = -((03 * 60) + 00);
                    }
                    // Australian Eastern Daylight Time.
                    else if(v == "AEDT"){
                        timeZoneMinutes = ((11 * 60) + 00);
                    }
                    // Australian Eastern Standard Time.
                    else if(v == "AEST"){
                        timeZoneMinutes = ((10 * 60) + 00);
                    }
                    // Alaska Daylight Time.
                    else if(v == "AKDT"){
                        timeZoneMinutes = -((08 * 60) + 00);
                    }
                    // Alaska Standard Time.
                    else if(v == "AKST"){
                        timeZoneMinutes = -((09 * 60) + 00);
                    }
                    // Atlantic Standard Time.
                    else if(v == "AST"){
                        timeZoneMinutes = -((04 * 60) + 00);
                    }
                    // Australian Western Daylight Time.
                    else if(v == "AWDT"){
                        timeZoneMinutes = ((09 * 60) + 00);
                    }
                    // Australian Western Standard Time.
                    else if(v == "AWST"){
                        timeZoneMinutes = ((08 * 60) + 00);
                    }
                    // Bravo Time Zone (millitary).
                    else if(v == "B"){
                        timeZoneMinutes = ((02 * 60) + 00);
                    }
                    // British Summer Time.
                    else if(v == "BST"){
                        timeZoneMinutes = ((01 * 60) + 00);
                    }
                    // Charlie Time Zone (millitary).
                    else if(v == "C"){
                        timeZoneMinutes = ((03 * 60) + 00);
                    }
                    // Central Daylight Time.
                    else if(v == "CDT"){
                        timeZoneMinutes = -((05 * 60) + 00);
                    }
                    // Central European Daylight Time.
                    else if(v == "CEDT"){
                        timeZoneMinutes = ((02 * 60) + 00);
                    }
                    // Central European Summer Time.
                    else if(v == "CEST"){
                        timeZoneMinutes = ((02 * 60) + 00);
                    }
                    // Central European Time.
                    else if(v == "CET"){
                        timeZoneMinutes = ((01 * 60) + 00);
                    }
                    // Central Standard Time.
                    else if(v == "CST"){
                        timeZoneMinutes = -((06 * 60) + 00);
                    }
                    // Christmas Island Time.
                    else if(v == "CXT"){
                        timeZoneMinutes = ((01 * 60) + 00);
                    }
                    // Delta Time Zone (military).
                    else if(v == "D"){
                        timeZoneMinutes = ((04 * 60) + 00);
                    }
                    // Echo Time Zone (military).
                    else if(v == "E"){
                        timeZoneMinutes = ((05 * 60) + 00);
                    }
                    // Eastern Daylight Time.
                    else if(v == "EDT"){
                        timeZoneMinutes = -((04 * 60) + 00);
                    }
                    // Eastern European Daylight Time.
                    else if(v == "EEDT"){
                        timeZoneMinutes = ((03 * 60) + 00);
                    }
                    // Eastern European Summer Time.
                    else if(v == "EEST"){
                        timeZoneMinutes = ((03 * 60) + 00);
                    }
                    // Eastern European Time.
                    else if(v == "EET"){
                        timeZoneMinutes = ((02 * 60) + 00);
                    }
                    // Eastern Standard Time.
                    else if(v == "EST"){
                        timeZoneMinutes = -((05 * 60) + 00);
                    }
                    // Foxtrot Time Zone (military).
                    else if(v == "F"){
                        timeZoneMinutes = (06 * 60 + 00);
                    }
                    // Golf Time Zone (military).
                    else if(v == "G"){
                        timeZoneMinutes = ((07 * 60) + 00);
                    }
                    // Greenwich Mean Time.
                    else if(v == "GMT"){
                        timeZoneMinutes = 0000;
                    }
                    // Hotel Time Zone (military).
                    else if(v == "H"){
                        timeZoneMinutes = ((08 * 60) + 00);
                    }
                    // India Time Zone (military).
                    else if(v == "I"){
                        timeZoneMinutes = ((09 * 60) + 00);
                    }
                    // Irish Summer Time.
                    else if(v == "IST"){
                        timeZoneMinutes = ((01 * 60) + 00);
                    }
                    // Kilo Time Zone (millitary).
                    else if(v == "K"){
                        timeZoneMinutes = ((10 * 60) + 00);
                    }
                    // Lima Time Zone (millitary).
                    else if(v == "L"){
                        timeZoneMinutes = ((11 * 60) + 00);
                    }
                    // Mike Time Zone (millitary).
                    else if(v == "M"){
                        timeZoneMinutes = ((12 * 60) + 00);
                    }
                    // Mountain Daylight Time.
                    else if(v == "MDT"){
                        timeZoneMinutes = -((06 * 60) + 00);
                    }
                    // Mountain Standard Time.
                    else if(v == "MST"){
                        timeZoneMinutes = -((07 * 60) + 00);
                    }
                    // November Time Zone (military).
                    else if(v == "N"){
                        timeZoneMinutes = -((01 * 60) + 00);
                    }
                    // Newfoundland Daylight Time.
                    else if(v == "NDT"){
                        timeZoneMinutes = -((02 * 60) + 30);
                    }
                    // Norfolk (Island) Time.
                    else if(v == "NFT"){
                        timeZoneMinutes = ((11 * 60) + 30);
                    }
                    // Newfoundland Standard Time.
                    else if(v == "NST"){
                        timeZoneMinutes = -((03 * 60) + 30);
                    }
                    // Oscar Time Zone (military).
                    else if(v == "O"){
                        timeZoneMinutes = -((02 * 60) + 00);
                    }
                    // Papa Time Zone (military).
                    else if(v == "P"){
                        timeZoneMinutes = -((03 * 60) + 00);
                    }
                    // Pacific Daylight Time.
                    else if(v == "PDT"){
                        timeZoneMinutes = -((07 * 60) + 00);
                    }
                    // Pacific Standard Time.
                    else if(v == "PST"){
                        timeZoneMinutes = -((08 * 60) + 00);
                    }
                    // Quebec Time Zone (military).
                    else if(v == "Q"){
                        timeZoneMinutes = -((04 * 60) + 00);
                    }
                    // Romeo Time Zone (military).
                    else if(v == "R"){
                        timeZoneMinutes = -((05 * 60) + 00);
                    }
                    // Sierra Time Zone (military).
                    else if(v == "S"){
                        timeZoneMinutes = -((06 * 60) + 00);
                    } 
                    // Tango Time Zone (military).
                    else if(v == "T"){
                        timeZoneMinutes = -((07 * 60) + 00);
                    }
                    // Uniform Time Zone (military).
                    else if(v == ""){
                        timeZoneMinutes = -((08 * 60) + 00);
                    }
                    // Coordinated Universal Time.
                    else if(v == "UTC"){
                        timeZoneMinutes = 0000;
                    }
                    // Victor Time Zone (militray).
                    else if(v == "V"){
                        timeZoneMinutes = -((09 * 60) + 00);
                    }
                    // Whiskey Time Zone (military).
                    else if(v == "W"){
                        timeZoneMinutes = -((10 * 60) + 00);
                    }
                    // Western European Daylight Time.
                    else if(v == "WEDT"){
                        timeZoneMinutes = ((01 * 60) + 00);
                    }
                    // Western European Summer Time.
                    else if(v == "WEST"){
                        timeZoneMinutes = ((01 * 60) + 00);
                    }
                    // Western European Time.
                    else if(v == "WET"){
                        timeZoneMinutes = 0000;
                    }
                    // Western Standard Time.
                    else if(v == "WST"){
                        timeZoneMinutes = ((08 * 60) + 00);
                    }
                    // X-ray Time Zone (military).
                    else if(v == "X"){
                        timeZoneMinutes = -((11 * 60) + 00);
                    }
                    // Yankee Time Zone (military).
                    else if(v == "Y"){
                        timeZoneMinutes = -((12 * 60) + 00);
                    }
                    // Zulu Time Zone (military).
                    else if(v == "Z"){
                        timeZoneMinutes = 0000;
                    }

                    #endregion
                }
                        
                // Convert time to UTC and then back to local.
                DateTime timeUTC = new DateTime(year,month,day,hour,minute,second).AddMinutes(-(timeZoneMinutes));
                return new DateTime(timeUTC.Year,timeUTC.Month,timeUTC.Day,timeUTC.Hour,timeUTC.Minute,timeUTC.Second,DateTimeKind.Utc).ToLocalTime();
            }
            catch(Exception x){
                string dymmy = x.Message;
                throw new ArgumentException("Argumnet 'value' value '" + value + "' is not valid RFC 822/2822 date-time string.");
            }
        }

        #endregion


        #region static method UnfoldHeader

        /// <summary>
        /// Unfolds folded header field.
        /// </summary>
        /// <param name="value">Header field.</param>
        /// <returns>Returns unfolded header field.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        public static string UnfoldHeader(string value)
        {
            if(value == null){
                throw new ArgumentNullException("value");
            }

            /* RFC 2822 2.2.3 Long Header Fields.
				The process of moving from this folded multiple-line representation
				of a header field to its single line representation is called
				"unfolding". Unfolding is accomplished by simply removing any CRLF
				that is immediately followed by WSP.
            */

            return value.Replace("\r\n","");
        }

        #endregion


        #region static method CreateMessageID

		/// <summary>
		/// Creates Rfc 2822 3.6.4 message-id. Syntax: '&lt;' id-left '@' id-right '&gt;'.
		/// </summary>
		/// <returns></returns>
		public static string CreateMessageID()
		{
			return "<" + Guid.NewGuid().ToString().Replace("-","").Substring(16) + "@" + Guid.NewGuid().ToString().Replace("-","").Substring(16) + ">";
		}

		#endregion


        #region static method ParseHeaders

		/// <summary>
		/// Parses headers from message or mime entry.
		/// </summary>
		/// <param name="entryStrm">Stream from where to read headers.</param>
		/// <returns>Returns header lines.</returns>
		internal static string ParseHeaders(Stream entryStrm)
		{
			/* Rfc 2822 3.1.  GENERAL DESCRIPTION
				A message consists of header fields and, optionally, a body.
				The  body  is simply a sequence of lines containing ASCII charac-
				ters.  It is separated from the headers by a null line  (i.e.,  a
				line with nothing preceding the CRLF).
			*/

			byte[] crlf = new byte[]{(byte)'\r',(byte)'\n'};
			MemoryStream msHeaders = new MemoryStream();
			StreamLineReader r = new StreamLineReader(entryStrm);
			byte[] lineData = r.ReadLine();
			while(lineData != null){
				if(lineData.Length == 0){
					break;
				}

				msHeaders.Write(lineData,0,lineData.Length);
				msHeaders.Write(crlf,0,crlf.Length);
				lineData = r.ReadLine();
			}

			return System.Text.Encoding.Default.GetString(msHeaders.ToArray());
		}

		#endregion

        #region static method ParseHeaderField

		/// <summary>
		/// Parse header specified header field value.
		/// 
		/// Use this method only if you need to get only one header field, otherwise use
		/// MimeParser.ParseHeaderField(string fieldName,string headers).
		/// This avoid parsing headers multiple times.
		/// </summary>
		/// <param name="fieldName">Header field which to parse. Eg. Subject: .</param>
		/// <param name="entryStrm">Stream from where to read headers.</param>
		/// <returns></returns>
		public static string ParseHeaderField(string fieldName,Stream entryStrm)
		{
			return ParseHeaderField(fieldName,ParseHeaders(entryStrm));
		}

		/// <summary>
		/// Parse header specified header field value.
		/// </summary>
		/// <param name="fieldName">Header field which to parse. Eg. Subject: .</param>
		/// <param name="headers">Full headers string. Use MimeParser.ParseHeaders() to get this value.</param>
		public static string ParseHeaderField(string fieldName,string headers)
		{
			/* Rfc 2822 2.2 Header Fields
				Header fields are lines composed of a field name, followed by a colon
				(":"), followed by a field body, and terminated by CRLF.  A field
				name MUST be composed of printable US-ASCII characters (i.e.,
				characters that have values between 33 and 126, inclusive), except
				colon.  A field body may be composed of any US-ASCII characters,
				except for CR and LF.  However, a field body may contain CRLF when
				used in header "folding" and  "unfolding" as described in section
				2.2.3.  All field bodies MUST conform to the syntax described in
				sections 3 and 4 of this standard. 
				
			   Rfc 2822 2.2.3 (Multiline header fields)
				The process of moving from this folded multiple-line representation
				of a header field to its single line representation is called
				"unfolding". Unfolding is accomplished by simply removing any CRLF
				that is immediately followed by WSP.  Each header field should be
				treated in its unfolded form for further syntactic and semantic
				evaluation.
				
				Example:
					Subject: aaaaa<CRLF>
					<TAB or SP>aaaaa<CRLF>
			*/

			using(TextReader r = new StreamReader(new MemoryStream(System.Text.Encoding.Default.GetBytes(headers)))){
				string line = r.ReadLine();
				while(line != null){
					// Find line where field begins
					if(line.ToUpper().StartsWith(fieldName.ToUpper())){
						// Remove field name and start reading value
						string fieldValue = line.Substring(fieldName.Length).Trim();

						// see if multi line value. See commnt above.
						line = r.ReadLine();
						while(line != null && (line.StartsWith("\t") || line.StartsWith(" "))){
							fieldValue += line;
							line = r.ReadLine();
						}

						return fieldValue;
					}

					line = r.ReadLine();
				}
			}

			return "";
		}

		#endregion
    }
}
