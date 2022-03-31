# SnipeID Calculation Bot

This is the source code for the SnipeID bot (currently in [osu!snipe Indonesia](https://discord.gg/8F9c4AxSc2)). What it does:

1. Either retrieves message by [Bathbot](https://discordapp.com/oauth2/authorize?client_id=297073686916366336&scope=bot&permissions=268823616) for `<osc` command, or mentioning the bot with the calculation command (for example, `@SnipeID count`).
2. If Bathbot reply is received, parses username and top map leaderboards from its embed. Otherwise with the calculation command, will retrieve the top map leaderboards count from [osu!Stats](https://osustats.ppy.sh/) API.
3. Calculates and shows the points, and grants user roles in the server based on points received.

Other features:

- **User verification** (`@BOT_NAME link [osu! ID]`)

    Links Discord user with osu! ID and grants verified role for that user in the server.

- **Points leaderboard** (`@BOT_NAME lb` or `@BOT_NAME leaderboard`)

    Shows Top 50 leaderboard of recently achieved points for all users.
## Setup

For development purposes, create the development environment. Make sure to install [Node.js](https://nodejs.org/en/download/) (tested using [v16.14.0](https://nodejs.org/dist/v16.14.0/)) and [PostgreSQL](https://www.postgresql.org/download/) (tested using v13.3). Afterwards, do these steps.

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
    - Add Reactions

    The generated URL at the bottom could be used to invite your previously created bot to the server.

5. Clone the repository.
6. Copy `.env-template` and rename the file to `.env`.
7. Fill `.env` with the following values:

    - `BOT_NAME`: Bot's name. Will be used for quick usages in the code.
    - `BOT_TOKEN`: Token for the bot. May be obtained from Build-A-Bot section in Bot settings.
    - `CHANNEL_ID`: Channel ID to listen Bathbot command on. To copy the ID, enable [Developer Mode](https://techswift.org/2020/09/17/how-to-enable-developer-mode-in-discord/) in Discord Advanced Settings, right-click on your specified channel and click `Copy ID`.
    - `LEADERBOARD_CHANNEL_ID`: Channel ID to listen for leaderboard commands (`@BOT_NAME lb` or `@BOT_NAME leaderboard`).
    - `OSUHOW_EMOJI_ID`: (Optional) Emoji ID for receiving certain points ( ͡° ͜ʖ ͡°).
    - `SERVER_ID`: Server ID, used for role and member fetching.
    - `VERIFIED_ROLE_ID`: Verified Role ID.
    - `DB_HOST`: Hostname for PostgreSQL server.
    - `DB_PORT`: Port for PostgreSQL server, usually the default is `5432`.
    - `DB_USERNAME`: Username for the database.
    - `DB_PASSWORD`: Password for the database.
    - `DB_DATABASE`: Database name.
    - `OSU_CLIENT_ID`: osu! Client ID.
    - `OSU_CLIENT_SECRET`: osu! Client secret.

8. Install the dependencies for the project.

    ```shell
    $ npm install
    ```

9. Initialize the database.

    ```shell
    $ npm run init-db
    ```

10. Start the client.

    ```shell
    $ npm start
    ```

In case of errors, take note of errors displayed and try again. If the error occurs during `init-db` command, also drop all tables (if exists) and run the command again.

For production purposes, deploy as a worker to [Heroku](https://heroku.com), as web to [Railway](https://railway.app), or any other platforms. The `Procfile` file in the repository is used for Railway and may be modified for any platform. Use their dashboard's environment variable settings to apply environment variables based on the `.env` file.

## License

[MIT](LICENSE)
