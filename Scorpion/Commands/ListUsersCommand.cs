using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Scorpion.RestClient;

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

            await Parent.HttpRestClient.EnsureAuthenticated();

            ICollection<CovenantUser> users = await Parent.HttpRestClient.ApiUsersGetAsync();

            Console.WriteLine("Users: ");
            foreach (CovenantUser user in users)
            {
                Console.WriteLine($"      {user.UserName} - {user.Id}");
            }
            return 0;
        }
    }
}
