using Discord.WebSocket;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Commands;

public static class CommandsFactory
{
	public static string GetPingMessage(DiscordSocketClient client)
	{
		Log.WriteVerbose("GetPingMessage", "Returning latency from client as ping message.");
		return $"Pong! ({ client.Latency } ms)";
	}
}
