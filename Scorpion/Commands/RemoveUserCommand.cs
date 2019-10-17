using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Scorpion.RestClient;

namespace Scorpion.Commands
{
    [Command("remove-user")]
    public class RemoveUserCommand
    {
        public ScorpionCommand Parent { get; set; }

        [Argument(0, name: "Username")]
        public string UserName { get; set; }

        public async Task<int> OnExecuteAsync(IConsole console)
        {
            if (Parent == null) throw new ArgumentNullException(nameof(Parent));
            if (String.IsNullOrEmpty(UserName)) throw new ArgumentNullException(nameof(UserName));
            await Parent.HttpRestClient.EnsureAuthenticated();

            // Is this the best way to find a user by username?
            ICollection<CovenantUser> users = await Parent.HttpRestClient.ApiUsersGetAsync();

            List<CovenantUser> usersLists = new List<CovenantUser>(users);

            CovenantUser user = usersLists.Find(u => u.UserName == UserName);
            if (user == null)
            {
                Console.WriteLine($"Could not find user or user already delated: {UserName}");
                return 1;
            }

            await Parent.HttpRestClient.ApiUsersByIdDeleteAsync(user.Id);

            // CovenantUser user = await Parent.HttpRestClient.ApiUsersPostAsync(userData);
            // TODO: add the ability to update the user with extra data (email, etc)
            Console.WriteLine($"User removed: {user.UserName}");
            return 0;
        }
    }
}
