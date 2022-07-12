const Discord = require("discord.js");
const { LogSeverity, log } = require("../log");
const { greet, agree, disagree, notUnderstood } = require("../messages/msg");

/**
 * Sends message based on what message was received.
 *
 * @param { Discord.Client } client
 * @param { string } channelId
 * @param { string[] } contents
 *
 * @returns { Promise<void> }
 */
async function sendMessage(client, channelId, contents) {
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
    log(LogSeverity.LOG, "sendMessage", "Chat response sent to channel: #" + channel.name);
  }
  else {
    log(LogSeverity.LOG, "sendMessage", "Unknown command \"" + contents[1] + "\" response sent to channel: #" + channel.name);
  }

  await channel.send(reply);
}

module.exports = {
  sendMessage
};
