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
