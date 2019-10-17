using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Scorpion.RestClient;
using McMaster.Extensions.CommandLineUtils;

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
                Console.WriteLine($"      {user.Id} - {user.UserName} - {user.Email}");
            }
            return 0;
        }
    }
}
