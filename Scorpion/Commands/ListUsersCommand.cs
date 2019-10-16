using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;

namespace Scorpion.Commands
{
    [Command("list")]
    public class ListUsersCommand
    {
        public ScorpionCommand Parent { get; set; }

        public int OnExecute(IConsole console)
        {
            if (Parent == null) throw new ArgumentNullException(nameof(Parent));
            if (console == null) throw new ArgumentNullException(nameof(console));

            var users = new List<string>();
            users.Add("John");
            users.Add("Jim");

            users.ForEach(item => console.WriteLine(item));
            return 0;
        }
    }
}
