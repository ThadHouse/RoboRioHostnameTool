using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Windows.Sdk;

#pragma warning disable CS1591, CS1573, CS0465, CS0649, CS8019, CS1570, CS1584, CS1658

namespace Microsoft.Windows.Sdk {
    internal unsafe struct DNS_SERVICE_BROWSE_REQUEST_FIXED
    {
        /// <summary>The structure version must be either **DNS_QUERY_REQUEST_VERSION1** or **DNS_QUERY_REQUEST_VERSION2**. The value determines which of `pBrowseCallback` or `pBrowseCallbackV2` is active.</summary>
        internal uint Version;
        /// <summary>A value that contains the interface index over which the query is sent. If `InterfaceIndex` is 0, then all interfaces will be considered.</summary>
        internal uint InterfaceIndex;
        /// <summary>A pointer to a string that represents the service type whose matching services you wish to browse for. It takes the generalized form "\_\<ServiceType \>.\_\<TransportProtocol \>.local". For example, "_http._tcp.local", which defines a query to browse for http services on the local link.</summary>
        internal PCWSTR QueryName;
        internal delegate* unmanaged[Stdcall]<void*, DNS_QUERY_RESULT_WIDE*, void> Callback;
        /// <summary>A pointer to a user context.</summary>
        internal unsafe void* pQueryContext;
    }

    internal partial struct DNS_QUERY_RESULT_WIDE
    {
        /// <summary>The structure version must be one of the following:</summary>
        internal uint Version;
        /// <summary>
        /// <para>The return status of the call to <a href="https://docs.microsoft.com/windows/desktop/api/windns/nf-windns-dnsqueryex">DnsQueryEx</a>.</para>
        /// <para>If the query was completed asynchronously and this structure was returned directly from <a href="https://docs.microsoft.com/windows/desktop/api/windns/nf-windns-dnsqueryex">DnsQueryEx</a>, <b>QueryStatus</b> contains <b>DNS_REQUEST_PENDING</b>.</para>
        /// <para>If the query was completed synchronously or if this structure was returned by the <a href="https://docs.microsoft.com/windows/desktop/api/windns/nc-windns-dns_query_completion_routine">DNS_QUERY_COMPLETION_ROUTINE</a> DNS callback, <b>QueryStatus</b> contains ERROR_SUCCESS if successful or the appropriate DNS-specific error code as defined in Winerror.h.</para>
        /// <para><see href="https://docs.microsoft.com/windows/win32/api//windns/ns-windns-dns_query_result#members">Read more on docs.microsoft.com</see>.</para>
        /// </summary>
        internal int QueryStatus;
        /// <summary>A value that contains a bitmap of <a href="https://docs.microsoft.com/windows/desktop/DNS/dns-constants">DNS Query  Options</a> that were used in the DNS query. Options can be combined and all options override <b>DNS_QUERY_STANDARD</b></summary>
        internal ulong QueryOptions;
        /// <summary>
        /// <para>A pointer to a <a href="https://docs.microsoft.com/windows/win32/api/windns/ns-windns-dns_recorda">DNS_RECORD</a> structure.</para>
        /// <para>If the query was completed asynchronously and this structure was returned directly from <a href="https://docs.microsoft.com/windows/desktop/api/windns/nf-windns-dnsqueryex">DnsQueryEx</a>, <b>pQueryRecords</b> is NULL.</para>
        /// <para>If the query was completed synchronously or if this structure was returned by the <a href="https://docs.microsoft.com/windows/desktop/api/windns/nc-windns-dns_query_completion_routine">DNS_QUERY_COMPLETION_ROUTINE</a> DNS callback, <b>pQueryRecords</b> contains a list of Resource Records (RR) that comprise the response.</para>
        /// <para><div class="alert"><b>Note</b>  Applications must free returned RR sets with the <a href="https://docs.microsoft.com/windows/desktop/api/windns/nf-windns-dnsrecordlistfree">DnsRecordListFree</a> function.</div> <div> </div></para>
        /// <para><see href="https://docs.microsoft.com/windows/win32/api//windns/ns-windns-dns_query_result#members">Read more on docs.microsoft.com</see>.</para>
        /// </summary>
        internal unsafe DNS_RECORDW* pQueryRecords;
        /// <summary></summary>
        internal unsafe void* Reserved;
    }

    internal static partial class PInvoke {
        [DllImport("DnsApi", ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern unsafe int DnsServiceBrowse(DNS_SERVICE_BROWSE_REQUEST_FIXED* pRequest, DNS_SERVICE_CANCEL* pCancel);
    }
}
