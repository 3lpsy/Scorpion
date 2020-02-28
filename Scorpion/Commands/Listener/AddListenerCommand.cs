using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Covenant.API;
using Covenant.API.Models;
using Scorpion.Commands;
using Scorpion.Jobs;
using System.Linq;
using Scorpion.Exceptions;


// dotnet run -- --username jim --password mypass --server https://127.0.0.1:7443 -i addlistener --name HTTP-80-80 --address 0.0.0.0 --port 80 --connect-addresses 10.8.0.98 --connect-port 80 --profile-name CustomHttpProfile --type-name HTTP --urls http://10.8.0.98
namespace Scorpion.Commands.Listener
{
    [Command("addlistener", Description = "Add HTTP Listener")]
    public class AddListenerCommand : Command
    {

        [Option("--name", Description = "Name")]
        public string Name { get; set; }

        [Option("--description", Description = "Description")]
        public string Description { get; set; } = "Some Listener";

        [Option("--status", Description = "Status")]
        public string Status { get; set; } = "Active";

        [Option("--address", Description = "Bind Address")]
        public string BindAddress { get; set; } = "0.0.0.0";

        [Option("--port", Description = "Bind Port")]
        public int BindPort { get; set; } = 80;

        [Option("--connect-addresses", Description = "Connection Addresses (CSV)")]
        public string ConnectAddressesCSV { get; set; }

        [Option("--connect-port", Description = "Connection Port")]
        public int ConnectPort { get; set; } = 80;

        [Option("--profile-id", Description = "Profile ID (alt: --profile-name)")]
        public int ProfileId { get; set; } = 0;

        [Option("--profile-name", Description = "Profile Name")]
        public string ProfileName { get; set; } = "CustomHttpProfile";

        [Option("--type-id", Description = "Listener Type ID (alt: --type-name)")]
        public int ListenerTypeId { get; set; } = 0;

        [Option("--type-name", Description = "Listener Type Name")]
        public string ListenerTypeName { get; set; } = "HTTP";

        [Option("--urls", Description = "Urls (comma separated)")]
        public string URLsList { get; set; }

        [Option("--use-ssl", Description = "Use ssl (not supported)")]
        public bool UseSSL { get; set; } = false;

        [Option("--ssl-certificate", Description = "SSL Certificate (not supported)")]
        public string SSLCertificate { get; set; }

        [Option("--ssl-password", Description = "SSL Password (not supported)")]
        public string SSLCertificatePassword { get; set; }

        [Option("--ssl-hash", Description = "SSL Certififcate Hash (not supported)")]
        public string SSLCertificateHash { get; set; }

        public async Task<int> OnExecuteAsync(IConsole console)
        {
            await SetupCommandAsync(console, true, true);
            return await RunAsync(console);
        }
        public async Task<int> RunAsync(IConsole console)
        {

            if (String.IsNullOrEmpty(Name))
            {
                Name = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
            }
            var aGuid = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);


            if (String.IsNullOrEmpty(BindAddress)) throw new MissingParameterException(nameof(BindAddress));
            if (BindPort > 65535 || BindPort < 1) throw new MissingParameterException(nameof(BindPort));

            if (Status != "Active" && Status != "Uninitialized") throw new MissingParameterException(nameof(Status));


            if (String.IsNullOrEmpty(ConnectAddressesCSV)) throw new MissingParameterException(nameof(ConnectAddressesCSV));
            if (ConnectPort > 65535 || ConnectPort < 1) throw new MissingParameterException(nameof(ConnectPort));

            List<string> ConnectAddresses = new List<string>(ConnectAddressesCSV.Split(","));

            List<string> Urls;

            if (String.IsNullOrEmpty(URLsList))
            {
                Urls = ConnectAddresses.Select(c => $"http://{c}:{ConnectPort}").ToList();
            }
            else
            {
                Urls = new List<string>(URLsList.Split(","));
            }

            Urls = new List<string>();

            if (ProfileId == 0 && String.IsNullOrEmpty(ProfileName)) throw new MissingParameterException(nameof(ProfileName));
            if (ListenerTypeId == 0 && String.IsNullOrEmpty(ListenerTypeName)) throw new MissingParameterException(nameof(ListenerTypeName));


            UseSSL = false;

            return await new AddListenerJob(console, Api).RunAsync(new
            {
                Name = Name,
                Description = Description,
                Guid = aGuid,
                BindAddress = BindAddress,
                BindPort = BindPort,
                Urls = Urls,
                ConnectAddresses = ConnectAddresses,
                ConnectPort = ConnectPort,
                ProfileId = ProfileId,
                ProfileName = ProfileName,
                ListenerTypeId = ListenerTypeId,
                ListenerTypeName = ListenerTypeName,
                Status = Status,
                UseSSL = UseSSL,
                SSLCertificate = SSLCertificate,
                SSLCertificatePassword = SSLCertificatePassword,
                SSLCertificateHash = SSLCertificateHash
            });
        }
    }
}
