using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Scorpion.RestClient;

namespace Scorpion.Commands
{
    [Command("add-user")]
    public class AddUserCommand
    {
        public ScorpionCommand Parent { get; set; }

        [Argument(0, name: "Username")]
        public string UserName { get; set; }
        [Argument(1, name: "Password")]
        public string Password { get; set; }

        public async Task<int> OnExecuteAsync(IConsole console)
        {
            if (Parent == null) throw new ArgumentNullException(nameof(Parent));
            if (String.IsNullOrEmpty(UserName)) throw new ArgumentNullException(nameof(UserName));
            if (String.IsNullOrEmpty(Password))
            {
                Console.Write("Enter Password: ");
                Password = Parent.HttpRestClient.GetPassword();
                Console.WriteLine();
            }
            await Parent.HttpRestClient.EnsureAuthenticated();

            CovenantUserLogin userData = new CovenantUserLogin()
            {
                UserName = UserName,
                Password = Password
            };

            CovenantUser user = await Parent.HttpRestClient.ApiUsersPostAsync(userData);
            // TODO: add the ability to update the user with extra data (email, etc)
            Console.WriteLine($"Added User:");
            Console.WriteLine($"    UserName: {user.UserName}");
            Console.WriteLine($"    ID: {user.Id}");
            return 0;
        }
    }
}
