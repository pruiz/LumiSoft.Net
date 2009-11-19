using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP
{
    /// <summary>
    /// 
    /// </summary>
    class IMAP_Search_Key_And
    {
        private List<IMAP_Search_Key> m_pKeys = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public IMAP_Search_Key_And()
        {
            m_pKeys = new List<IMAP_Search_Key>();
        }


        public static void Parse()
        {
            IMAP_Search_Key_And retVal = new IMAP_Search_Key_And();
            string key = "";
            StringReader r = null;

            // AND

            // ANSWERED
            if(string.Equals(key,"ANSWERED")){
                retVal.m_pKeys.Add(IMAP_Search_Key_Answered.Parse(r));
            }
            // BCC
            else if(string.Equals(key,"BCC")){
                // retVal.m_pKeys.Add(IMAP_Search_Key_Bcc.Parse(r));
            }
            // BEFORE
            else if(string.Equals(key,"BEFORE")){
                // retVal.m_pKeys.Add(IMAP_Search_Key_Before.Parse(r));
            }
            // BODY
            else if(string.Equals(key,"BODY")){
                // retVal.m_pKeys.Add(IMAP_Search_Key_Body.Parse(r));
            }
            // CC
            else if(string.Equals(key,"CC")){
                // retVal.m_pKeys.Add(IMAP_Search_Key_Cc.Parse(r));
            }
            // DELETED
            else if(string.Equals(key,"DELETED")){
                retVal.m_pKeys.Add(IMAP_Search_Key_Deleted.Parse(r));
            }
            // DRAFT
            else if(string.Equals(key,"DRAFT")){
                retVal.m_pKeys.Add(IMAP_Search_Key_Draft.Parse(r));
            }
            // FLAGGED
            else if(string.Equals(key,"FLAGGED")){
                retVal.m_pKeys.Add(IMAP_Search_Key_Flagged.Parse(r));
            }
            // FROM
            else if(string.Equals(key,"FROM")){
                // retVal.m_pKeys.Add(IMAP_Search_Key_From.Parse(r));
            }
            // HEADER
            else if(string.Equals(key,"HEADER")){
                // retVal.m_pKeys.Add(IMAP_Search_Key_Header.Parse(r));
            }
            // KEYWORD
            else if(string.Equals(key,"KEYWORD")){
                // retVal.m_pKeys.Add(IMAP_Search_Key_Keyword.Parse(r));
            }
            // LAGER
            else if(string.Equals(key,"LAGER")){
                // retVal.m_pKeys.Add(IMAP_Search_Key_Lager.Parse(r));
            }
            // NEW
            else if(string.Equals(key,"NEW")){
                retVal.m_pKeys.Add(IMAP_Search_Key_New.Parse(r));
            }
            // NOT
            else if(string.Equals(key,"NOT")){
                // retVal.m_pKeys.Add(IMAP_Search_Key_Not.Parse(r));
            }
            // OLD
            else if(string.Equals(key,"OLD")){
                retVal.m_pKeys.Add(IMAP_Search_Key_Old.Parse(r));
            }
            // ON
            else if(string.Equals(key,"ON")){
                // retVal.m_pKeys.Add(IMAP_Search_Key_On.Parse(r));
            }
            // OR
            else if(string.Equals(key,"OR")){
                // retVal.m_pKeys.Add(IMAP_Search_Key_Or.Parse(r));
            }
            // RECENT
            else if(string.Equals(key,"RECENT")){
                retVal.m_pKeys.Add(IMAP_Search_Key_Recent.Parse(r));
            }
            // SEEN
            else if(string.Equals(key,"SEEN")){
                retVal.m_pKeys.Add(IMAP_Search_Key_Seen.Parse(r));
            }
            // SENTBEFORE
            else if(string.Equals(key,"SENTBEFORE")){
                // retVal.m_pKeys.Add(IMAP_Search_Key_SentBefore.Parse(r));
            }
            // SENTON
            else if(string.Equals(key,"SENTON")){
                // retVal.m_pKeys.Add(IMAP_Search_Key_SentOn.Parse(r));
            }
            // SENTSINCE
            else if(string.Equals(key,"SENTSINCE")){
                // retVal.m_pKeys.Add(IMAP_Search_Key_SentSince.Parse(r));
            }
            // SEQSET
            else if(string.Equals(key,"SEQSET")){
                // retVal.m_pKeys.Add(IMAP_Search_Key_SeqSet.Parse(r));
            }
            // SINCE
            else if(string.Equals(key,"SINCE")){
                // retVal.m_pKeys.Add(IMAP_Search_Key_Since.Parse(r));
            }
            // TO
            else if(string.Equals(key,"TO")){
                // retVal.m_pKeys.Add(IMAP_Search_Key_To.Parse(r));
            }
            // UID
            else if(string.Equals(key,"UID")){
                // retVal.m_pKeys.Add(IMAP_Search_Key_Uid.Parse(r));
            }
            // UNANSWERED
            else if(string.Equals(key,"UNANSWERED")){
                retVal.m_pKeys.Add(IMAP_Search_Key_Unanswered.Parse(r));
            }
            // UNDELETED
            else if(string.Equals(key,"UNDELETED")){
                retVal.m_pKeys.Add(IMAP_Search_Key_Undeleted.Parse(r));
            }
            // UNDRAFT
            else if(string.Equals(key,"UNDRAFT")){
                retVal.m_pKeys.Add(IMAP_Search_Key_Undraft.Parse(r));
            }
            // UNFLAGGED
            else if(string.Equals(key,"UNFLAGGED")){
                retVal.m_pKeys.Add(IMAP_Search_Key_Unflagged.Parse(r));
            }
            // UNKEYWORD
            else if(string.Equals(key,"UNKEYWORD")){
                // retVal.m_pKeys.Add(IMAP_Search_Key_Unkeyword.Parse(r));
            }
            // UNSEEN
            else if(string.Equals(key,"UNSEEN")){
                retVal.m_pKeys.Add(IMAP_Search_Key_Unseen.Parse(r));
            }
            else{
                // Check if we hae sequence-set. Because of IMAP specification sucks a little here, why the hell they didn't 
                // do the keyword(SEQSET) for it, like UID. Now we just have to try if it is sequence-set or BAD key. 
            }
        }


        #region Properties implementation

        /// <summary>
        /// Gets AND-ded keys collection.
        /// </summary>
        public List<IMAP_Search_Key> Keys
        {
            get{ return m_pKeys; }
        }

        #endregion
    }
}
