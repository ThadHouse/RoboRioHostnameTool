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
                selectedDevice = value;
                RefreshSelections();
            }
        }

        private int teamNumber = 1;
        public int TeamNumber
        {
            get => teamNumber;
            set
            {
                teamNumber = value;
            }
        }

        private string startText = "Start Search";
        public string StartText
        {
            get => startText;
            set => this.RaiseAndSetIfChanged(ref startText, value, nameof(StartText));
        }

        private bool isBlinking = false;

        public bool CanBlink
        {
            get => !isBlinking && SelectedDevice != null;
        }

        private bool canSearch = true;
        public bool CanSearch
        {
            get => canSearch;
            set => this.RaiseAndSetIfChanged(ref canSearch, value, nameof(CanSearch));
        }

        public bool Searching => !CanSearch;

        Task searcher;
        Task? waitTask;
        
        private void RefreshSelections()
        {
            this.RaisePropertyChanged(nameof(CanBlink));
            this.RaisePropertyChanged(nameof(SelectedDevice));
        }

        private async Task StopDelay()
        {
            await Task.Delay(5000);
            if (Searching)
            {
                StopSearch();
            }
        }

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

        private void StartSearch()
        {
            bool isSearching = Searching;
            if (isSearching) return;
            CanSearch = false;
            StartText = "Stop Search";
            Devices.Clear();
            discoverer.Start();
            waitTask = StopDelay();
        }

        private void StopSearch()
        {
            bool isSearching = Searching;
            if (!isSearching) return;
            CanSearch = true;
            StartText = "Start Search";
            discoverer.Stop();
        }

        public void OnSearch()
        {
            bool startSearch = CanSearch;

            if (startSearch)
            {
                StartSearch();
            }
            else
            {
                StopSearch();
            }
        }

        public async Task OnBlinkLed()
        {
            isBlinking = true;
            RefreshSelections();
            Task? task = SelectedDevice?.BlinkLedAsync();
            if (task != null)
            {
                await task;
            }

            isBlinking = false;
            RefreshSelections();
        }

        public async void OnSetTeamNumber()
        {

        }
    }
}
