using System.Threading.Tasks;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Covenant.API;
using Covenant.API.Models;
using Scorpion.Api;

namespace Scorpion.Jobs
{
    public class ListListenersJob : Job
    {
        public ListListenersJob(IConsole console, CovenantAPI api) : base(console, api) { }

        public async Task<int> RunAsync()
        {
            IList<Profile> profiles = await new RequestBuilder(Api).GetProfiles();

            Console.WriteLine("Profiles: ");
            foreach (Profile profile in profiles)
            {
                Console.WriteLine($"      {profile.Name} - {profile.Id}");
            }

            IList<ListenerType> types = await new RequestBuilder(Api).GetListenerTypes();
            Console.WriteLine("Types: ");
            foreach (ListenerType ltype in types)
            {
                Console.WriteLine($"      {ltype.Name} - {ltype.Id}");
            }

            IList<Listener> listeners = await new RequestBuilder(Api).GetListeners();
            if (listeners.Count < 1)
            {
                Console.WriteLine("Listeners: None");
            }
            else
            {
                Console.WriteLine("Listeners: ");
            }
            foreach (Listener listener in listeners)
            {
                Console.WriteLine($"      {listener.Name} - {listener.Id}");
            }
            return await Task.FromResult(0);
        }
    }
}
