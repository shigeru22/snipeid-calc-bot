using System.Diagnostics.CodeAnalysis;

namespace LeaderpointsBot.Api.Osu;

public static class OsuDataTypes
{
	public struct OsuApiHeaderData
	{
		public string Authorization { get; set; }
	}

	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public readonly struct OsuApiTokenRequestRawData
	{
		public int client_id { get; init; }
		public string client_secret { get; init; }
		public string grant_type { get; init; }
		public string scope { get; init; }

		public OsuApiTokenRequestData ToStandardData()
		{
			return new OsuApiTokenRequestData()
			{
				ClientID = client_id,
				ClientSecret = client_secret,
				GrantType = grant_type,
				Scope = scope
			};
		}
	}

	public readonly struct OsuApiTokenRequestData
	{
		public int ClientID { get; init; }
		public string ClientSecret { get; init; }
		public string GrantType { get; init; }
		public string Scope { get; init; }

		public OsuApiTokenRequestRawData ToRawData()
		{
			return new OsuApiTokenRequestRawData()
			{
				client_id = ClientID,
				client_secret = ClientSecret,
				grant_type = GrantType,
				scope = Scope
			};
		}
	}

	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public struct OsuApiTokenResponseRawData
	{
		public string token_type { get; set; }
		public int expires_in { get; set; }
		public string access_token { get; set; }

		public OsuApiTokenResponseData ToStandardData()
		{
			return new OsuApiTokenResponseData()
			{
				TokenType = token_type,
				ExpiresIn = expires_in,
				AccessToken = access_token
			};
		}
	}

	public readonly struct OsuApiTokenResponseData
	{
		public string TokenType { get; init; }
		public int ExpiresIn { get; init; }
		public string AccessToken { get; init; }

		public OsuApiTokenResponseRawData ToRawData()
		{
			return new OsuApiTokenResponseRawData()
			{
				token_type = TokenType,
				expires_in = ExpiresIn,
				access_token = AccessToken
			};
		}
	}

	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public struct OsuApiUserResponseRawData
	{
		public string avatar_url { get; set; }
		public string country_code { get; set; }
		public string cover_url { get; set; }
		public string default_group { get; set; }
		public string? discord { get; set; }
		public bool has_supported { get; set; }
		public int id { get; set; }
		public string? interests { get; set; }
		public bool is_active { get; set; }
		public bool is_bot { get; set; }
		public bool is_deleted { get; set; }
		public bool is_online { get; set; }
		public bool is_supporter { get; set; }
		public DateTime join_date { get; set; }
		public object kudosu { get; set; } // Kudosu, not interested for now
		public DateTime? last_visit { get; set; }
		public string? location { get; set; }
		public int max_blocks { get; set; }
		public int max_friends { get; set; }
		public string? occupation { get; set; }
		public object playmode { get; set; } // GameMode, not interested for now
		public string[] playstyle { get; set; }
		public bool pm_friends_only { get; set; }
		public int post_count { get; set; }
		public string? profile_colour { get; set; }
		public object profile_order { get; set; } // ProfilePage[], not interested for now
		public string? title { get; set; }
		public string? title_url { get; set; }
		public string? twitter { get; set; }
		public string username { get; set; }
		public string? website { get; set; }

		public OsuApiUserResponseData ToStandardData()
		{
			return new OsuApiUserResponseData()
			{
				AvatarUrl = avatar_url,
				CountryCode = country_code,
				CoverURL = cover_url,
				DefaultGroup = default_group,
				Discord = discord,
				HasSupported = has_supported,
				ID = id,
				Interests = interests,
				IsActive = is_active,
				IsBot = is_bot,
				IsDeleted = is_deleted,
				IsOnline = is_online,
				IsSupporter = is_supporter,
				JoinDate = join_date,
				Kudosu = kudosu,
				LastVisit = last_visit,
				Location = location,
				MaxBlocks = max_blocks,
				MaxFriends = max_friends,
				Occupation = occupation,
				PlayMode = playmode,
				PlayStyle = playstyle,
				PMFriendsOnly = pm_friends_only,
				PostCount = post_count,
				ProfileColour = profile_colour,
				ProfileOrder = profile_order,
				Title = title,
				TitleUrl = title_url,
				Twitter = twitter,
				Username = username,
				Website = website
			};
		}
	}

	public readonly struct OsuApiUserResponseData
	{
		public string AvatarUrl { get; init; }
		public string CountryCode { get; init; }
		public string CoverURL { get; init; }
		public string DefaultGroup { get; init; }
		public string? Discord { get; init; }
		public bool HasSupported { get; init; }
		public int ID { get; init; }
		public string? Interests { get; init; }
		public bool IsActive { get; init; }
		public bool IsBot { get; init; }
		public bool IsDeleted { get; init; }
		public bool IsOnline { get; init; }
		public bool IsSupporter { get; init; }
		public DateTime JoinDate { get; init; }
		public object Kudosu { get; init; } // Kudosu, not interested for now
		public DateTime? LastVisit { get; init; }
		public string? Location { get; init; }
		public int MaxBlocks { get; init; }
		public int MaxFriends { get; init; }
		public string? Occupation { get; init; }
		public object PlayMode { get; init; } // GameMode, not interested for now
		public string[] PlayStyle { get; init; }
		public bool PMFriendsOnly { get; init; }
		public int PostCount { get; init; }
		public string? ProfileColour { get; init; }
		public object ProfileOrder { get; init; } // ProfilePage[], not interested for now
		public string? Title { get; init; }
		public string? TitleUrl { get; init; }
		public string? Twitter { get; init; }
		public string Username { get; init; }
		public string? Website { get; init; }

		public OsuApiUserResponseRawData ToRawData()
		{
			return new OsuApiUserResponseRawData()
			{
				avatar_url = AvatarUrl,
				country_code = CountryCode,
				cover_url = CoverURL,
				default_group = DefaultGroup,
				discord = Discord,
				has_supported = HasSupported,
				id = ID,
				interests = Interests,
				is_active = IsActive,
				is_bot = IsBot,
				is_deleted = IsDeleted,
				is_online = IsOnline,
				is_supporter = IsSupporter,
				join_date = JoinDate,
				kudosu = Kudosu,
				last_visit = LastVisit,
				location = Location,
				max_blocks = MaxBlocks,
				max_friends = MaxFriends,
				occupation = Occupation,
				playmode = PlayMode,
				playstyle = PlayStyle,
				pm_friends_only = PMFriendsOnly,
				post_count = PostCount,
				profile_colour = ProfileColour,
				profile_order = ProfileOrder,
				title = Title,
				title_url = TitleUrl,
				twitter = Twitter,
				username = Username,
				website = Website
			};
		}
	}
}
