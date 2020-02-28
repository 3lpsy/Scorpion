using System;
using McMaster.Extensions.CommandLineUtils;
using Scorpion.Api;
using Scorpion.Commands.User;
using Scorpion.Commands.Listener;
using Scorpion.Commands.Meta;
using Scorpion.Commands.Grunt;

namespace Scorpion.Commands
{

  [Subcommand(
      typeof(ListUsersCommand),
      typeof(AddUserCommand),
      typeof(RemoveUserCommand),
      typeof(GetTokenCommand),
      typeof(ListListenersCommand),
      typeof(AddListenerCommand),
      typeof(ListImplantTemplatesCommand),

      typeof(RemoveListenerCommand),
      typeof(SetupCommand)

  )]
  public class ScorpionCommand : IDisposable
  {

    public ApiFactory ApiFactory { get; set; }

    [Option("-u|--username", Description = "Covenant Username")]
    public string UserName { get; }

    [Option("-p|--password", Description = "Covenant Password")]
    public string Password { get; }

    [Option("-s|--server", Description = "Covenant Base URL")]
    public string BaseUrl { get; }

    [Option("-i|--ignore-ssl", Description = "Ignore Covenant SSL Errros")]
    public bool IgnoreSSL { get; }

    public void LoadRootOptions()
    {
      if (!String.IsNullOrEmpty(UserName)) {
        ApiFactory.Profile.UserName = UserName;
      }
      if (!String.IsNullOrEmpty(Password)) {
        ApiFactory.Profile.Password = Password;
      }
      if (!String.IsNullOrEmpty(BaseUrl)) {
        ApiFactory.Profile.BaseUrl = BaseUrl;
      }
      if (IgnoreSSL) {
        ApiFactory.Profile.IgnoreSSL = IgnoreSSL;
      }
    }

    public ScorpionCommand(ApiFactory apiFactory)
    {
      ApiFactory = apiFactory;
    }

    private int OnExecute(CommandLineApplication application)
    {
      application.ShowHelp(false);
      return 0;
    }

    public void Dispose()
    {
    }
  }
}
