using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Windows.Sdk;
using static Microsoft.Windows.Sdk.Constants;
using System.Threading.Channels;
using System.Threading;

namespace RoboRioToolLib
{
    public class NativeWindowsDiscoverer : IDeviceDiscoverer
    {
        private unsafe struct PinnedStorage
        {
            public DNS_SERVICE_BROWSE_REQUEST_FIXED BrowseRequest;
            public DNS_SERVICE_CANCEL Cancel;
            public fixed char Name[18];
            public GCHandle DiscovererHandle;
        }

        private readonly PinnedStorage[] pinnedStorage;
        private unsafe PinnedStorage* Storage => (PinnedStorage*)Unsafe.AsPointer(ref pinnedStorage[0]);

        private readonly Channel<NiDeviceLocation> foundDeviceChannel = Channel.CreateUnbounded<NiDeviceLocation>();

        public unsafe NativeWindowsDiscoverer()
        {
            pinnedStorage = GC.AllocateArray<PinnedStorage>(1, true);
            var storage = Storage;
            char* NamePointer = (char*)Unsafe.AsPointer(ref storage->Name[0]);
            "_ni-rt._tcp.local".AsSpan().CopyTo(new Span<char>(NamePointer, 18));
            storage->DiscovererHandle = GCHandle.Alloc(this);
            storage->BrowseRequest.InterfaceIndex = 0;
            storage->BrowseRequest.pQueryContext = Storage;
            storage->BrowseRequest.QueryName = NamePointer;
            storage->BrowseRequest.Version = 2;
            storage->BrowseRequest.Callback = &DnsQueryCompletion;
        }


        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private unsafe static void DnsQueryCompletion(void* context, DNS_QUERY_RESULT_WIDE* queryResults)
        {
            PinnedStorage* storage = (PinnedStorage*)context;
            var discoverer = (NativeWindowsDiscoverer)storage->DiscovererHandle.Target!;

            DNS_RECORDW* current = queryResults->pQueryRecords;
            while (current != null)
            {
                if (current->wType == DNS_TYPE_A)
                {
                    var rawIp = current->Data.A.IpAddress;
                    var ip = new IPAddress(rawIp);
                    discoverer.foundDeviceChannel.Writer.TryWrite(new NiDeviceLocation(current->pName.ToString(), ip));
                }
                current = current->pNext;
            }

            PInvoke.DnsFree(queryResults->pQueryRecords, DNS_FREE_TYPE.DnsFreeRecordList);
        }

        public unsafe bool Start()
        {
            Storage->Cancel.reserved = null;
            int result = PInvoke.DnsServiceBrowse(&Storage->BrowseRequest, &Storage->Cancel);
            return result == DNS_REQUEST_PENDING;
        }

        public unsafe bool Stop()
        {
            return PInvoke.DnsServiceBrowseCancel(&Storage->Cancel) == ERROR_SUCCESS;
        }

        public async Task<NiDeviceLocation> GetDevice(CancellationToken token)
        {
            return await foundDeviceChannel.Reader.ReadAsync(token).ConfigureAwait(false);
        }
    }
}
