using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using RoboRioToolLib;

namespace RoboRioHostnameTool
{
    class Program
    {
        // echo 255 > /sys/class/leds/nilrt:wifi:primary/brightness
        static async Task Main(int teamNumber = -1)
        {
            if (teamNumber < 1)
            {
                Console.WriteLine("Must pass a team number");
                return;
            }

            if (teamNumber > ushort.MaxValue)
            {
                Console.WriteLine("Team number too large");
                return;
            }

            IDeviceDiscoverer discover = new NativeWindowsDiscoverer();
            discover.Start();
            CancellationTokenSource cts = new CancellationTokenSource();
            HashSet<NiDeviceLocation> devices = new();
            cts.CancelAfter(2000);
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    NiDeviceLocation device = await discover.GetDevice(cts.Token);
                    if (devices.Add(device))
                    {
                        Console.WriteLine(device);
                    }
                }
            }
            catch (OperationCanceledException)
            {

            }
            discover.Stop();

            foreach (var device in devices)
            {
                using var d = new NiDevice(device);
                if (await d.TryConnectAsync())
                {
                    Console.WriteLine("Blinking radio LED on device...");
                    await d.BlinkLedAsync();
                    Console.WriteLine("Would you like to set the team number on the device that blinked? type yes to confirm");
                    var input = Console.ReadLine();
                    if (input?.StartsWith("yes", StringComparison.InvariantCultureIgnoreCase) ?? false)
                    {
                        await d.SetTeamNumber((ushort)teamNumber);
                        Console.WriteLine("Team number set. You need to reboot the device for this to take affect.");
                    }
                    
                }
            }
        }
    }
}
