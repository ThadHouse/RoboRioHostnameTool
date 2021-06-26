using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive;
using ReactiveUI;
using System.Collections.ObjectModel;
using RoboRioToolLib;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace RoboRioHostnameTool.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private IDeviceDiscoverer discoverer = new NativeWindowsDiscoverer();

        public string Greeting => "Welcome to Avalonia!";

        public ObservableCollection<NiDevice> Devices { get; } = new();

        private NiDevice? selectedDevice;
        public NiDevice? SelectedDevice
        {
            get => selectedDevice;
            set {
                this.RaiseAndSetIfChanged(ref selectedDevice, value, nameof(SelectedDevice));
            }
        }

        private string startText = "Start Search";
        public string StartText
        {
            get => startText;
            set => this.RaiseAndSetIfChanged(ref startText, value, nameof(StartText));
        }

        private bool canBlink = true;
        public bool CanBlink
        {
            get => canBlink;
            set => this.RaiseAndSetIfChanged(ref canBlink, value, nameof(CanBlink));
        }

        private bool canSearch = true;
        public bool CanSearch
        {
            get => canSearch;
            set => this.RaiseAndSetIfChanged(ref canSearch, value, nameof(CanSearch));
        }

        Task searcher;

        private async Task ReactToDevices()
        {
            while (true)
            {
                var device = await discoverer.GetDevice(default);
                if (Devices.Select(x => x.Location).Contains(device))
                {
                    continue;
                }
                var niDevice = new NiDevice(device);
                if (await niDevice.TryConnectAsync())
                {
                    Devices.Add(niDevice);
                    if (SelectedDevice == null)
                    {
                        SelectedDevice = niDevice;
                    }
                }

            }
        }

        public MainWindowViewModel()
        {
            searcher = ReactToDevices();
        }

        public void OnSearch()
        {
            bool startSearch = CanSearch;
            CanSearch = !startSearch;

            if (startSearch)
            {
                Devices.Clear();
                discoverer.Start();
            }
            else
            {
                discoverer.Stop();
            }
        }

        public async void OnBlinkLed()
        {
            CanBlink = false;
            Task? task = SelectedDevice?.BlinkLedAsync();
            if (task != null)
            {
                await task;
            }
            CanBlink = true;
        }
    }
}
