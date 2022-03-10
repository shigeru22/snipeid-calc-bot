# SnipeID Calculation Bot

This is the source code for the SnipeID bot (currently in [osu!snipe Indonesia](https://discord.gg/8F9c4AxSc2)). What it does:

1. Retrieves message by [Bathbot](https://discordapp.com/oauth2/authorize?client_id=297073686916366336&scope=bot&permissions=268823616) for `<osc` command.
2. Parses username and top map leaderboards from its embed.
3. Calculates and shows the points, which used for specifying user roles in the server.

Better data sources is yet to be used later on.

## Contributing

For development purposes, create the development environment. Make sure to install [Node.js](https://nodejs.org/en/download/) (tested using [v16.14.0](https://nodejs.org/dist/v16.14.0/)). Afterwards, do these steps.

1. Create your own bot for development purposes in [Discord Developer Portal](https://discord.com/developers/applications).
2. Invite both [Bathbot](https://discordapp.com/oauth2/authorize?client_id=297073686916366336&scope=bot&permissions=268823616) and your previously created bot to your development server.
3. Clone the repository.
4. Copy `.env-template` and rename the file to `.env`.
5. Fill `.env` with the following values:

    - `BOT_TOKEN`: Token for the bot. May be obtained from Build-A-Bot section in Bot settings.
    - `CHANNEL_ID`: Channel ID to listen Bathbot command on. In [osu!snipe Indonesia](https://discord.gg/8F9c4AxSc2), it's a `#verification` channel.
        To copy the ID, enable [Developer Mode](https://techswift.org/2020/09/17/how-to-enable-developer-mode-in-discord/) in Discord Advanced Settings, right-click on your specified channel and click `Copy ID`.
    - `OSUHOW_EMOJI_ID`: (Optional) Emoji ID for receiving certain points ( ͡° ͜ʖ ͡°).
6. Install the dependencies for the project.

    ```shell
    $ npm install
    ```

7. Start the client.

    ```shell
    $ npm start
    ```

For production purposes, deploy as a worker to [Heroku](https://heroku.com) or any platform, and use their dashboard's environment variable settings based on `.env` file.

## License

[MIT](LICENSE)
