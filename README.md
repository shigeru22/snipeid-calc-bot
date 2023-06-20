# LeaderpointsBot

This is LeaderpointsBot, previously (and still currently) lives as SnipeID bot in [osu!snipe Indonesia](https://discord.gg/8F9c4AxSc2) server. What it does:

1. Either retrieves message by [Bathbot](https://discordapp.com/oauth2/authorize?client_id=297073686916366336&scope=bot&permissions=268823616) for `<osc` command, or mentioning the bot with the calculation command (`@SnipeID count` or `/count` interaction).
2. If Bathbot reply is received, parses username and top map leaderboards from its embed. Otherwise with the calculation command, will retrieve the top map leaderboards count from [osu!Stats](https://osustats.ppy.sh/) API.
3. Calculates and shows the points, and grants user roles in the server based on points received.

Other features:

- **User verification** (`@BOT_NAME link [osu! ID]` or `/link [osu! ID]`)

    Links Discord user with osu! ID and grants verified role for that user in the server.

- **Points leaderboard** (`@BOT_NAME lb` or `@BOT_NAME leaderboard` or `/serverleaderboard`)

    Shows Top 50 leaderboard of recently achieved points for all users in server.

## Setup

Since v2, this bot client has been rewritten in C# using [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) and uses [PostgreSQL v15](https://www.postgresql.org/download). Development environment could be created with the following steps.

1. Create your bot in [Discord Developer Portal](https://discord.com/developers/applications).
2. Create a new osu! OAuth application. osu! Client ID and secret may be obtained from osu! Account Settings, inside OAuth section, and create the application using `New OAuth Application` button. ID and secret of the application is located inside `Edit` button modal.
3. Create a new database in your PostgreSQL server with any name. Make sure to note the username, password, port, hostname, and name of the created database.
4. Invite both [Bathbot](https://discordapp.com/oauth2/authorize?client_id=297073686916366336&scope=bot&permissions=268823616) and your previously created bot to your development server.

    For the bot's invite and permissions, head to `OAuth` > `URL Generator`, checking the scope as `bot`, and add the permissions with the following:

    - Manage Roles
    - Send Messages
    - Manage Messages
    - Embed Links
    - Read Message History
    - Add Reactions (WIP)

    The generated URL at the bottom could be used to invite your previously created bot to the server.

5. Clone the repository.
6. Copy `appsettings.json.template` inside `/LeaderpointsBot.Utils` directory and rename the file to `appsettings.json`.
7. Fill `appsettings.json` with the following values:

    ```json
    {
        "client": {
            "botToken": string, // Discord bot token
            "useReply": boolean, // Use reply to text (message) commands
            "logging": {
                "useUtc": boolean, // Use UTC time for logging
                "logSeverity": number // Log severity [1: critical, 2: error, 3: info, 4: verbose, 5: debug]
            }
        },
        "database": {
            "hostname": string, // Database hostname
            "port": number, // Database port
            "username": string, // Database username
            "password": string, // Database password
            "databaseName": string, // Database name
            "caFilePath": string // Certificate used to connect (relative to client's working directory)
        },
        "osuApi": {
            "clientId": number, // osu!api v2 client ID
            "clientSecret": string, // osu!api v2 client secret
            "useRespektiveStats": boolean // [deprecated] Use respektive osu!stats API
        }
    }
    ```

    **Note:** Configuration using environment variables and arguments are also supported. Run client with `--help` for more information (template is available as `.env-template` at root project directory).

8. Restore solution.

    ```shell
    $ dotnet restore
    ```

9. Initialize interactions and database if not already.

    ```shell
    $ dotnet run --project LeaderpointsBot.Client -- --init-interactions --init-db
    ```

    **Note:** If migrating from previous version (v1, using Node.js), run database migration using `--migrate-db` argument.

10. Start the client.

    ```shell
    $ dotnet run --project LeaderpointsBot.Client
    ```

In case of errors, take note of errors displayed and try again.

For production purposes, use the provided `Dockerfile` to create the image, or self-deploy using the following command.

```shell
$ dotnet publish -c Release -o ./bin
```

And run the client using this command.

```shell
$ dotnet ./bin/LeaderpointsBot.Client.dll
```

## License

[MIT](LICENSE)
