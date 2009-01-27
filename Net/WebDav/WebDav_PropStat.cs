using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace LumiSoft.Net.WebDav
{
    /// <summary>
    /// This class represents WebDav 'propstat' element.
    /// </summary>
    public class WebDav_PropStat
    {
        private string      m_Status              = null;
        private string      m_ResponseDescription = null;
        private WebDav_Prop m_pProp               = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal WebDav_PropStat()
        {
        }


        #region method Parse

        /// <summary>
        /// Parses WebDav_PropStat from 'DAV:propstat' element.
        /// </summary>
        /// <param name="reponseNode">The 'DAV:propstat' element</param>
        /// <exception cref="ArgumentNullException">Is raised when when <b>propstatNode</b> is null reference.</exception>
        internal void Parse(XmlNode propstatNode)
        {
            if(propstatNode == null){
                throw new ArgumentNullException("propstatNode");
            }

            // TODO:
            //if(!string.Equals(reponseNode.LocalName,"propstat",StringComparison.InvariantCultureIgnoreCase)){
            //}

            foreach(XmlNode node in propstatNode.ChildNodes){
                if(string.Equals(node.LocalName,"status",StringComparison.InvariantCultureIgnoreCase)){
                    m_Status = node.ChildNodes[0].Value;
                }
                else if(string.Equals(node.LocalName,"prop",StringComparison.InvariantCultureIgnoreCase)){
                    m_pProp = new WebDav_Prop();
                    m_pProp.Parse(node);
                }                
            }
        }

        #endregion


        #region Properties implementation

        /// <summary>
        /// Gets property HTTP status.
        /// </summary>
        public string Status
        {
            get{ return m_Status; }
        }

        /// <summary>
        /// Gets human-readable status property description.
        /// </summary>
        public string ResponseDescription
        {
            get{ return m_ResponseDescription; }
        }
        
        /// <summary>
        /// Gets 'prop' element value.
        /// </summary>
        public WebDav_Prop Prop
        {
            get{ return m_pProp; }
        }
        
        #endregion
    }
}
