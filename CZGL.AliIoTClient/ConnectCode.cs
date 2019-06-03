using System;
using System.Collections.Generic;
using System.Text;

namespace CZGL.AliIoTClient
{
    public enum ConnectCode
    {
        conn_accepted = 0x00,
        conn_refused_prot_vers = 0x01,
        conn_refused_ident_rejected = 0x02,
        conn_refused_server_unavailable = 0x03,
        conn_refused_username_password = 0x04,
        conn_refused_not_authorized = 0x05,
        unknown_error = 0x06
    }
}
