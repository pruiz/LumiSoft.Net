using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace LumiSoft.Net.WebDav
{
    /// <summary>
    /// This class represent WeDav 'response' element.
    /// </summary>
    public class WebDav_Response
    {
        private string                m_HRef       = null;
        private List<WebDav_PropStat> m_pPropStats = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal WebDav_Response()
        {
            m_pPropStats = new List<WebDav_PropStat>();
        }


        #region method Parse

        /// <summary>
        /// Parses WebDav_Response from 'DAV:response' element.
        /// </summary>
        /// <param name="reponseNode">The 'DAV:response' element</param>
        /// <exception cref="ArgumentNullException">Is raised when when <b>responseNode</b> is null reference.</exception>
        internal void Parse(XmlNode reponseNode)
        {
            if(reponseNode == null){
                throw new ArgumentNullException("responseNode");
            }

            // TODO:
            //if(!string.Equals(reponseNode.LocalName,"response",StringComparison.InvariantCultureIgnoreCase)){
            //}

            foreach(XmlNode node in reponseNode.ChildNodes){
                if(string.Equals(node.LocalName,"href",StringComparison.InvariantCultureIgnoreCase)){
                    m_HRef = node.ChildNodes[0].Value;
                }
                else if(string.Equals(node.LocalName,"propstat",StringComparison.InvariantCultureIgnoreCase)){
                    WebDav_PropStat propstat = new WebDav_PropStat();
                    propstat.Parse(node);
                    m_pPropStats.Add(propstat);
                }
            }            
        }

        #endregion


        #region Properties implementation

        /// <summary>
        /// Gets response href.
        /// </summary>
        public string HRef
        {
            get{ return m_HRef; }
        }

        /// <summary>
        /// Gets 'propstat' elements.
        /// </summary>
        public WebDav_PropStat[] PropStats
        {
            get{ return m_pPropStats.ToArray(); }
        }

        #endregion
    }
}
