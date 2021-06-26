using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RoboRioToolLib
{
    public class NiDevice : IDisposable
    {
        public NiDeviceLocation Location { get; private set; }
        private SshClient? sshClient;

        public NiDevice(NiDeviceLocation location)
        {
            Location = location;
        }

        public async Task<bool> TryConnectAsync()
        {
            if (sshClient != null)
            {
                return true;
            }

            KeyboardInteractiveAuthenticationMethod authMethodAdmin = new KeyboardInteractiveAuthenticationMethod("admin");
            PasswordAuthenticationMethod pauthAdmin = new PasswordAuthenticationMethod("admin", "");

            authMethodAdmin.AuthenticationPrompt += (sender, e) =>
            {
                foreach (
                    AuthenticationPrompt p in
                        e.Prompts.Where(
                            p => p.Request.IndexOf("Password:") != -1))
                {
                    p.Response = "";
                }
            };

            var m_adminConnectionInfo = new ConnectionInfo(Location.Address.ToString(), "admin", pauthAdmin, authMethodAdmin) { Timeout = TimeSpan.FromSeconds(2) };

            try
            {
                var localSshClient = new SshClient(m_adminConnectionInfo);
                await Task.Run(() => localSshClient.Connect()).ConfigureAwait(false);
                sshClient = localSshClient;
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
            catch (SshOperationTimeoutException)
            {
                return false;
            }
        }

        public void Dispose()
        {
            sshClient?.Dispose();
            sshClient = null;
        }

        public async Task BlinkLedAsync()
        {
            if (sshClient == null)
            {
                return;
            }

            var command = sshClient.CreateCommand("for i in 1 2 3 4 5  ;  do ` echo 255 > /sys/class/leds/nilrt:wifi:primary/brightness; sleep 0.5; echo 0 > /sys/class/leds/nilrt:wifi:primary/brightness; sleep 0.5 ` ; done");
            await Task<string>.Factory.FromAsync(command.BeginExecute(), command.EndExecute).ConfigureAwait(false);
        }

        public async Task SetTeamNumber(ushort number)
        {
            if (sshClient == null)
            {
                return;
            }

            var commandString = $"/usr/local/natinst/bin/nirtcfg --file=/etc/natinst/share/ni-rt.ini --set section=systemsettings,token=host_name,value=roborio-{number}-FRC ; sync";

            var command = sshClient.CreateCommand(commandString);
            await Task<string>.Factory.FromAsync(command.BeginExecute(), command.EndExecute).ConfigureAwait(false);
        }

        public override string ToString()
        {
            return $"{Location.Name} ({Location.Address})";
        }
    }
}
