# Scorpion

This is an attempt to generate a CLI interface for a Covenant C2 server. It's also an opportunity for me to experiment with Dotnet Core. 

## TODO:
- Ability to add user
- Ability to remove user but not the last admin.
- Ability to list listeners
- Ability to add listeners
- Ability to remove listeners
- Ability to create and generate launcher 
- Ability to list grunts
- Ability to interact with grunts
- Centralized Logging Class/Implementation
- Centralized Profile/Credential Class
- Ability to Authenticate Once and use old token.

## Supported Environment Variables:

- COVENANT_BASE_URL ("https://127.0.0.1:7443")
- COVENANT_TOKEN ("")
- COVENANT_USERNAME ("")
- COVENANT_PASSWORD ("")
- COVENANT_IGNORE_SSL ("")
- COVENANT_HTTP_DEBUG ("") - will be phased out

## Usage
```
Usage: Scorpion [options] [command]

Options:
  -?|-h|--help        Show help information

Commands:
  add-user
  generate-endpoints
  list-users

Run 'Scorpion [command] --help' for more information about a command.
```