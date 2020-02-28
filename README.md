# Scorpion

This is an attempt to generate a CLI interface for a Covenant C2 server. The main goal is to setup a way to automatically generate a listener and multiple obfuscated SMB grunts automatically. This project is not meant to be used on actual engagements and is instead targeted at lab envrionments.

## TODO:
- Ability to remove listeners
- Ability to create and generate launcher 
- Ability to list grunts
- Ability to interact with grunts
- Centralized Logging Class/Implementation
- Centralized Profile/Credential Class
- Ability to Authenticate Once and use old token.

## Supported Environment Variables:

- COVENANT_BASE_URL ("https://127.0.0.1:7443")
- COVENANT_USERNAME ("")
- COVENANT_PASSWORD ("")
- COVENANT_IGNORE_SSL ("")
- COVENANT_TOKEN ("")


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
  rmuser           Remove user by username
  token            Print covenant token to console
  users            List users

Run 'Scorpion [command] --help' for more information about a command.

```
