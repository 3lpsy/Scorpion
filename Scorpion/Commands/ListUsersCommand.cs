using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Covenant.API;
using Covenant.API.Models;

namespace Scorpion.Commands
{
    [Command("list-users")]
    public class ListUsersCommand
    {
        public ScorpionCommand Parent { get; set; }

        public async Task<int> OnExecuteAsync(IConsole console)
        {
            if (Parent == null) throw new ArgumentNullException(nameof(Parent));
            if (console == null) throw new ArgumentNullException(nameof(console));

            // await Parent.HttpRestClient.EnsureAuthenticated();

            // ICollection<CovenantUser> users = await Parent.HttpRestClient.ApiUsersGetAsync();

            var api = await Parent.ApiFactory.AuthenticatedApi();
            var result = await api.ApiUsersGetWithHttpMessagesAsync();
            IList<CovenantUser> users = result.Body;
            Console.WriteLine("Users: ");

            foreach (CovenantUser user in users)
            {
                Console.WriteLine($"      {user.UserName} - {user.Id}");
            }

            // Console.WriteLine("Users: ");
            // foreach (CovenantUser user in users)
            // {
            //     Console.WriteLine($"      {user.UserName} - {user.Id}");
            // }
            return 0;
        }
    }
}
