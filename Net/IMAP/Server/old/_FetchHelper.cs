using System;
using System.IO;
using System.Collections;
using System.Text;

using LumiSoft.Net.MIME;
using LumiSoft.Net.Mail;

namespace LumiSoft.Net.IMAP.Server
{
	/// <summary>
	/// FETCH command helper methods.
	/// </summary>
	internal class FetchHelper
	{				
		#region method ParseHeaderFields

		/// <summary>
		/// Returns requested header fields lines.
		/// Note: Header terminator blank line is included.
		/// </summary>
		/// <param name="fieldsStr">Header fields to get.</param>
		/// <param name="entity">Entity which header field lines to get.</param>
		/// <returns></returns>
		public static byte[] ParseHeaderFields(string fieldsStr,MIME_Entity entity)
		{
			return ParseHeaderFields(fieldsStr,System.Text.Encoding.Default.GetBytes(entity.Header.ToString()));
		}

		/// <summary>
		/// Returns requested header fields lines.
		/// Note: Header terminator blank line is included.
		/// </summary>
		/// <param name="fieldsStr">Header fields to get.</param>
		/// <param name="data">Message data.</param>
		/// <returns></returns>
		public static byte[] ParseHeaderFields(string fieldsStr,byte[] data)
		{
			fieldsStr = fieldsStr.Trim();
			if(fieldsStr.StartsWith("(")){
				fieldsStr = fieldsStr.Substring(1,fieldsStr.Length - 1);
			}
			if(fieldsStr.EndsWith(")")){
				fieldsStr = fieldsStr.Substring(0,fieldsStr.Length - 1);
			}

			string retVal = "";

			string[] fields = fieldsStr.Split(' ');
            using(MemoryStream mStrm = new MemoryStream(data)){
				TextReader r = new StreamReader(mStrm);
				string line = r.ReadLine();
				
				bool fieldFound = false;
				// Loop all header lines
				while(line != null){ 
					// End of header
					if(line.Length == 0){
						break;
					}

					// Field continues
					if(fieldFound && line.StartsWith("\t")){
						retVal += line + "\r\n";
					}
					else{
						fieldFound = false;

						// Check if wanted field
						foreach(string field in fields){
							if(line.Trim().ToLower().StartsWith(field.Trim().ToLower())){
								retVal += line + "\r\n";
								fieldFound = true;
							}
						}
					}

					line = r.ReadLine();
				}
			}

			// Add header terminating blank line
			retVal += "\r\n"; 

			return System.Text.Encoding.ASCII.GetBytes(retVal);
		}

		#endregion

		#region method ParseHeaderFieldsNot

		/// <summary>
		/// Returns header fields lines except requested.
		/// Note: Header terminator blank line is included.
		/// </summary>
		/// <param name="fieldsStr">Header fields to skip.</param>
		/// <param name="entity">Entity which header field lines to get.</param>
		/// <returns></returns>
		public static byte[] ParseHeaderFieldsNot(string fieldsStr,MIME_Entity entity)
		{
			return ParseHeaderFieldsNot(fieldsStr,System.Text.Encoding.Default.GetBytes(entity.Header.ToString()));
		}

		/// <summary>
		/// Returns header fields lines except requested.
		/// Note: Header terminator blank line is included.
		/// </summary>
		/// <param name="fieldsStr">Header fields to skip.</param>
		/// <param name="data">Message data.</param>
		/// <returns></returns>
		public static byte[] ParseHeaderFieldsNot(string fieldsStr,byte[] data)
		{
			fieldsStr = fieldsStr.Trim();
			if(fieldsStr.StartsWith("(")){
				fieldsStr = fieldsStr.Substring(1,fieldsStr.Length - 1);
			}
			if(fieldsStr.EndsWith(")")){
				fieldsStr = fieldsStr.Substring(0,fieldsStr.Length - 1);
			}

			string retVal = "";

			string[] fields = fieldsStr.Split(' ');
            using(MemoryStream mStrm = new MemoryStream(data)){
				TextReader r = new StreamReader(mStrm);
				string line = r.ReadLine();
				
				bool fieldFound = false;
				// Loop all header lines
				while(line != null){ 
					// End of header
					if(line.Length == 0){
						break;
					}

					// Filed continues
					if(fieldFound && line.StartsWith("\t")){
						retVal += line + "\r\n";
					}
					else{
						fieldFound = false;

						// Check if wanted field
						foreach(string field in fields){
							if(line.Trim().ToLower().StartsWith(field.Trim().ToLower())){								
								fieldFound = true;
							}
						}

						if(!fieldFound){
							retVal += line + "\r\n";
						}
					}

					line = r.ReadLine();
				}
			}

			return System.Text.Encoding.ASCII.GetBytes(retVal);
		}

		#endregion


		#region static method GetMimeEntity

		/// <summary>
		/// Gets specified mime entity. Returns null if specified mime entity doesn't exist.
		/// </summary>
		/// <param name="message">Mail message.</param>
		/// <param name="mimeEntitySpecifier">Mime entity specifier. Nested mime entities are pointed by '.'. 
		/// For example: 1,1.1,2.1, ... .</param>
		/// <returns></returns>
		public static MIME_Entity GetMimeEntity(Mail_Message message,string mimeEntitySpecifier)
		{
			// TODO: nested rfc 822 message

			// For single part message there is only one entity with value 1.
			// Example:
			//		header
			//		entity -> 1
			
			// For multipart message, entity counting starts from MainEntity.ChildEntities
			// Example:
			//		header
			//		multipart/mixed
			//			text/plain  -> 1
			//			application/pdf  -> 2
			//          ...

			// Single part
			if(message.ContentType == null || message.ContentType.Type.ToLower() != "multipart"){
				if(Convert.ToInt32(mimeEntitySpecifier) == 1){
					return message;
				}
				else{
					return null;
				}
			}
			// multipart
			else{
                /*
                MIME_Entity currentEntity = message;

                string[] parts = mimeEntitySpecifier.Split('.');
                for(int i=0;i<parts.Length;i++){
                    int partSpecifier = Convert.ToInt32(parts[i]) - 1; // Enitites are zero base, mimeEntitySpecifier is 1 based.

                    currentEntity

                    // Last mime part.
                    if(i == (parts.Length - 1)){
                    }
                    // Not a last mime part.
                    else{
                    }
                }*/
                
				MIME_Entity entity = message;
				string[] parts = mimeEntitySpecifier.Split('.');
				foreach(string part in parts){
					int mEntryNo = Convert.ToInt32(part) - 1; // Enitites are zero base, mimeEntitySpecifier is 1 based.
                    if(entity.Body is MIME_b_Multipart){
                        MIME_b_Multipart multipart = (MIME_b_Multipart)entity.Body;
                        if(mEntryNo > -1 && mEntryNo < multipart.BodyParts.Count){
						    entity = multipart.BodyParts[mEntryNo];
					    }
                        else{
                            return null;
                        }
                    }
			        else{
                        return null;
                    }
				}

				return entity;
			}			
		}

		#endregion

		#region static method GetMimeEntityHeader

		/// <summary>
		/// Gets specified mime entity header.
		/// Note: Header terminator blank line is included.
		/// </summary>
		/// <param name="entity">Mime entity.</param>
		/// <returns></returns>
		public static byte[] GetMimeEntityHeader(MIME_Entity entity)
		{
			return System.Text.Encoding.ASCII.GetBytes(entity.Header.ToString() + "\r\n");
		}

		/// <summary>
		/// Gets requested mime entity header. Returns null if specified mime entity doesn't exist.
		/// Note: Header terminator blank line is included.
		/// </summary>
		/// <param name="message">Mail message.</param>
		/// <param name="mimeEntitySpecifier">Mime entity specifier. Nested mime entities are pointed by '.'. 
		/// For example: 1,1.1,2.1, ... .</param>
		/// <returns>Returns requested mime entity data or NULL if requested entry doesn't exist.</returns>
		public static byte[] GetMimeEntityHeader(Mail_Message message,string mimeEntitySpecifier)
		{
			MIME_Entity mEntry = GetMimeEntity(message,mimeEntitySpecifier);
			if(mEntry != null){
				return GetMimeEntityHeader(mEntry);
			}
			else{
				return null;
			}
		}

		#endregion

		#region static method GetMimeEntityData

		/// <summary>
		/// Gets requested mime entity data. Returns null if specified mime entity doesn't exist.
		/// </summary>
		/// <param name="message">Mail message.</param>
		/// <param name="mimeEntitySpecifier">Mime entity specifier. Nested mime entities are pointed by '.'. 
		/// For example: 1,1.1,2.1, ... .</param>
		/// <returns>Returns requested mime entity data or NULL if requested entry doesn't exist.</returns>
		public static byte[] GetMimeEntityData(Mail_Message message,string mimeEntitySpecifier)
		{
			MIME_Entity entity = GetMimeEntity(message,mimeEntitySpecifier);
			if(entity != null){
                if(entity.Body is MIME_b_SinglepartBase){
                    return ((MIME_b_SinglepartBase)entity.Body).EncodedData;
                }
			}
			
            return null;			
		}

		#endregion


		#region static method Escape

		private static string Escape(string text)
		{
			text = text.Replace("\\","\\\\");
			text = text.Replace("\"","\\\"");

			return text;
		}

		#endregion

	}
}
