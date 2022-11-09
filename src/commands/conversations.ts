import { TextChannel } from "discord.js";
import { Log } from "../utils/log";
import { greet, agree, disagree, notUnderstood } from "../messages/msg";

class Conversations {
  /**
   * Sends message based on what message was received.
   *
   * @param { TextChannel } channel Discord channel to send message to.
   * @param { string[] } contents Array of message contents (splitted by spaces).
   *
   * @returns { Promise<void> } Promise object with no return value.
   */
  static async sendMessage(channel: TextChannel, contents: string[]): Promise<void> {
    await channel.sendTyping();

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
      Log.info("sendMessage", `Chat response sent to channel: #${ channel.name }`);
    }
    else {
      Log.info("sendMessage", `Unknown command "${ contents[1] }" response sent to channel: #${ channel.name }`);
    }

    await channel.send(reply);
  }
}

export default Conversations;
