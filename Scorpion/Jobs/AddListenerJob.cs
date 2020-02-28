using System.Threading.Tasks;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Covenant.API;
using Covenant.API.Models;
using Scorpion.Api;
using System;

namespace Scorpion.Jobs
{
    public class AddListenerJob : Job
    {
        public AddListenerJob(IConsole console, CovenantAPI api) : base(console, api) { }

        public async Task<int> RunAsync(dynamic httpListenerData)
        {

            var request = new RequestBuilder(Api);
            int profileId;
            int listenerTypeId;

            if (httpListenerData.ProfileId == 0 && !string.IsNullOrEmpty(httpListenerData.ProfileName))
            {
                Profile pulledProfile = await request.GetProfileByName(httpListenerData.ProfileName);
                profileId = (int)pulledProfile.Id;
            }
            else
            {
                profileId = httpListenerData.ProfileId;
            }

            if (httpListenerData.ListenerTypeId == 0 && !string.IsNullOrEmpty(httpListenerData.ListenerTypeName))
            {
                ListenerType pulledListenerType = await request.GetListenerTypeByName(httpListenerData.ListenerTypeName);
                listenerTypeId = (int)pulledListenerType.Id;
            }
            else
            {
                listenerTypeId = httpListenerData.ListenerTypeId;
            }
            ListenerStatus status;

            if (httpListenerData.Status == "Active")
            {
                status = ListenerStatus.Active;
            }
            else
            {
                status = ListenerStatus.Uninitialized;
            }
            HttpListener httpListener = new HttpListener(
                httpListenerData.UseSSL,
                httpListenerData.Urls,
                httpListenerData.Name,
                httpListenerData.Guid,
                httpListenerData.Description,
                httpListenerData.BindAddress,
                httpListenerData.BindPort,
                httpListenerData.ConnectAddresses,
                httpListenerData.ConnectPort,
                profileId,
                listenerTypeId,
                status
            );

            Console.WriteLine($"Name: {httpListener.Name}");
            Console.WriteLine($"Guid: {httpListener.Guid}");
            Console.WriteLine($"Description: {httpListener.Description}");
            Console.WriteLine($"UseSSL: {httpListener.UseSSL}");
            Console.WriteLine($"Urls: {String.Join(",", httpListener.Urls)} ({httpListener.Urls.Count})");
            Console.WriteLine($"BindAddress: {httpListener.BindAddress}");
            Console.WriteLine($"BindPort: {httpListener.BindPort}");
            Console.WriteLine($"ConnectAddresses: {String.Join(",", httpListener.ConnectAddresses)} ({httpListener.ConnectAddresses.Count})");
            Console.WriteLine($"ConnectPort: {httpListener.ConnectPort}");

            httpListener.Validate();

            HttpListener listener = await request.CreateHttpListener(httpListener);
            return await Task.FromResult(0);
        }
    }
}
