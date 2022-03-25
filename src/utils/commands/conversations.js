const Discord = require("discord.js");
const { greet, agree, disagree, notUnderstood } = require("../messages/msg");

async function sendMessage(client, channelId, contents) {
  if(!(client instanceof Discord.Client)) {
    console.log("[ERROR] sendMessage :: client must be a Discord.Client object instance.");
    process.exit(1);
  }

  if(typeof(channelId) !== "string") {
    console.log("[ERROR] sendMessage :: channelId must be string in Snowflake ID format.");
    process.exit(1);
  }

  const channel = client.channels.cache.get(channelId);

  let reply = "";
  let isUnderstood = true;

  if(contents[1] === "hi" || contents[1] === "hello") {
    reply = greet();
  }
  else if(contents[1].includes("right")) {
    const val = Math.random();
    if(val >= 0.5) {
      reply = agree();
    }
    else {
      reply = disagree();
    }
  }
  else {
    reply = notUnderstood();
    isUnderstood = false;
  }

  if(isUnderstood) {
    console.log("[LOG] Chat response sent to channel: #" + channel.name);
  }
  else {
    console.log("[LOG] Unknown command \"" + contents[1] + "\" response sent to channel: #" + channel.name);
  }
  await channel.send(reply);
}

module.exports = {
  sendMessage
};
