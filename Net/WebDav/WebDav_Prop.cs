using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace LumiSoft.Net.WebDav
{
    /// <summary>
    /// This class represents WebDav 'prop' element.
    /// </summary>
    public class WebDav_Prop
    {
        private List<WebDav_p> m_pProperties = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public WebDav_Prop()
        {
            m_pProperties = new List<WebDav_p>();
        }


        #region method Parse

        /// <summary>
        /// Parses WebDav_Prop from 'DAV:prop' element.
        /// </summary>
        /// <param name="reponseNode">The 'DAV:prop' element</param>
        /// <exception cref="ArgumentNullException">Is raised when when <b>propNode</b> is null reference.</exception>
        internal void Parse(XmlNode propNode)
        {
            if(propNode == null){
                throw new ArgumentNullException("propNode");
            }

            // TODO:
            //if(!string.Equals(reponseNode.LocalName,"prop",StringComparison.InvariantCultureIgnoreCase)){
            //}

            foreach(XmlNode node in propNode.ChildNodes){
                // Resource type property.
                if(string.Equals(node.LocalName,"resourcetype",StringComparison.InvariantCultureIgnoreCase)){
                    WebDav_p_ResourceType prop = new WebDav_p_ResourceType();
                    prop.Parse(node);
                    m_pProperties.Add(prop);
                }
                // Default name-value property.
                else{
                    m_pProperties.Add(new WebDav_p_Default("",node.LocalName,node.InnerXml));
                }
            }
        }

        #endregion


        #region Properties implementation

        /// <summary>
        /// Gets properties.
        /// </summary>
        public WebDav_p[] Properties
        {
            get{ return m_pProperties.ToArray(); }
        }

        /// <summary>
        /// Gets WebDav 'resourcetype' property value. Returns null if no such property available.
        /// </summary>
        public WebDav_p_ResourceType Prop_ResourceType
        {
            get{
                foreach(WebDav_p property in m_pProperties){
                    if(property is WebDav_p_ResourceType){
                        return (WebDav_p_ResourceType)property;
                    }
                }

                return null;
            }
        }
        
        #endregion
    }
}
