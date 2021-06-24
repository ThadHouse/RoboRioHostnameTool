using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoboRioToolLib
{
    public interface IDeviceDiscoverer
    {
        Task<NiDeviceLocation> GetDevice(CancellationToken token);
        bool Start();
        bool Stop();
    }
}
