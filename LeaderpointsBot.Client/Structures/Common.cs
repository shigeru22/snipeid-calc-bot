// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Client.Structures;

public static class Common
{
	public enum ResponseMessageType
	{
		Error,
		Text,
		Embed
	}

	public static class CachingPrefix
	{
		public const string CLIENT = "CID_";
		public const string GUILD = "GID_";
	}
}
