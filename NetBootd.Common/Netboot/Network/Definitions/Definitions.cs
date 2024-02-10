using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netboot
{
    public enum DHCPMessageType
    {
        Discover = 1,
        Offer,
        Request,
        Decline,
        Ack,
        Nak,
        Release,
        Inform,
        ForceRenew,
        LeaseQuery,
        LeaseUnassigned,
        LeaseUnknown,
        LeaseActive,
        BulkLeaseQuery,
        BulkLeaseQueryDone,
        ActiveLeaseQuery,
        LeaseQueryStatus,
        Tls
    }

}
