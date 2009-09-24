using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP
{
    /// <summary>
    /// IMAP server command completion response type. Defined in RFC 3501 2.2.2.
    /// </summary>
    class IMAP_CompletionResponseType
    {
        // Server completed command successfully.
        // OK

        // Server failed to complete command.
        // NO

        // Unrecognized command or command syntax error.
        // BAD
    }
}
