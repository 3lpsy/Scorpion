# Scorpion

This project is designed to to setup some intial assets in Covenant at boot. It is not meant to be a replacement for Elite or a complete remote CLI. This projects primary feature is the ability to generate an aggressive amount of obfuscated SMB grunt binaries via the "setup" command.

This project needs to be run from a Windows System with the v4 .Net SDks and v2.2 Dotnet Core SDK. The project uses dotnet 2.2 to work but triggers builds via msbuild.exe. In addition, the Covenant.dll is required as a local dependency as apparent in Scorpion.csproj.

## Setup

The setup command will do the following:

- Generate a Listener (optional, can use existing listener)
- Generate a standard unobfuscated HTTP Grunt hosted at /default.exe
- Generate a simple hosted HTA to download and trigger HTTP grunt at /default.hta
- Build and Obfuscate 20-30 SMB Grunt Binary and host them at /{someid}.exe

Assuming you've just setup the Covenant server, you can can run the following:

```
$ git clone https://github.com/3lpsy/Scorpion
$ git clone https://github.com/cobbr/Covenant
$ cs Covenant/Covenant
$ dotnet build
$ cd ../../Scorpion/Scorpion
$ dotnet run -- -u user -p pass -s https://COVENANT:7443 -i setup --connect-address SOME_IP --connect-port SOME_PORT
```

You can also customize the listener first via the "addlistener" command and the pass the listener name via "--listener somename".

## History

This was initially an attempt to generate a CLI interface for a Covenant C2 server. However, the project was abandoned until I actually found a need for it. The currnt main goal of this project is to facilitate the generation of a listener and multiple obfuscated SMB grunts automatically. This project is not meant to be used on actual engagements and is instead targeted at lab envrionments. This project is also likely to be unmaintained once it has served the purpose it was designed to fufill which was to help automate Grunt generation for Red Team Ops.

## Supported Environment Variables:

- COVENANT_BASE_URL ("https://127.0.0.1:7443")
- COVENANT_USERNAME ("")
- COVENANT_PASSWORD ("")
- COVENANT_IGNORE_SSL ("")
- COVENANT_TOKEN ("")

## Contributing

If for whatever reason you feel compelled to contribute to the project, just open a PR. If I don't respond within a day or two, contact me on twitter @3lpsy.

## Usage

```
Usage: Scorpion [options] [command]

Options:
  -u|--username    Covenant Username
  -p|--password    Covenant Password
  -s|--server      Covenant Base URL
  -i|--ignore-ssl  Ignore Covenant SSL Errros
  -?|-h|--help     Show help information

Commands:
  addlistener      Add HTTP Listener
  adduser          Add new user
  listeners        List Listeners
  profiles         List Profiles
  rmlistener       Remove listener by Name
  rmuser           Remove user by username
  setup            Setup
  templates        List Implant Templates
  token            Print covenant token to console
  users            List users

Run 'Scorpion [command] --help' for more information about a command.
```
