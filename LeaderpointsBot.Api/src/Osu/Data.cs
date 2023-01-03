namespace LeaderpointsBot.Api.Osu;

public static class OsuDataTypes
{
	public struct OsuApiHeaderData
	{
		public string Authorization { get; set; }
	}

	public struct OsuApiTokenRequestRawData
	{
		public int client_id { get; set; }
		public string client_secret { get; set; }
		public string grant_type { get; set; }
		public string scope { get; set; }

		public OsuApiTokenRequestData ToStandardData()
		{
			return new()
			{
				ClientID = client_id,
				ClientSecret = client_secret,
				GrantType = grant_type,
				Scope = scope
			};
		}
	}

	public struct OsuApiTokenRequestData
	{
		public int ClientID { get; set; }
		public string ClientSecret { get; set; }
		public string GrantType { get; set; }
		public string Scope { get; set; }

		public OsuApiTokenRequestRawData ToRawData()
		{
			return new()
			{
				client_id = ClientID,
				client_secret = ClientSecret,
				grant_type = GrantType,
				scope = Scope
			};
		}
	}

	public struct OsuApiTokenResponseRawData
	{
		public string token_type { get; set; }
		public int expires_in { get; set; }
		public string access_token { get; set; }

		public OsuApiTokenResponseData ToStandardData()
		{
			return new()
			{
				TokenType = token_type,
				ExpiresIn = expires_in,
				AccessToken = access_token
			};
		}
	}

	public struct OsuApiTokenResponseData
	{
		public string TokenType { get; set; }
		public int ExpiresIn { get; set; }
		public string AccessToken { get; set; }

		public OsuApiTokenResponseRawData ToRawData()
		{
			return new()
			{
				token_type = TokenType,
				expires_in = ExpiresIn,
				access_token = AccessToken
			};
		}
	}

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
			return new()
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

	public struct OsuApiUserResponseData
	{
		public string AvatarUrl { get; set; }
		public string CountryCode { get; set; }
		public string CoverURL { get; set; }
		public string DefaultGroup { get; set; }
		public string? Discord { get; set; }
		public bool HasSupported { get; set; }
		public int ID { get; set; }
		public string? Interests { get; set; }
		public bool IsActive { get; set; }
		public bool IsBot { get; set; }
		public bool IsDeleted { get; set; }
		public bool IsOnline { get; set; }
		public bool IsSupporter { get; set; }
		public DateTime JoinDate { get; set; }
		public object Kudosu { get; set; } // Kudosu, not interested for now
		public DateTime? LastVisit { get; set; }
		public string? Location { get; set; }
		public int MaxBlocks { get; set; }
		public int MaxFriends { get; set; }
		public string? Occupation { get; set; }
		public object PlayMode { get; set; } // GameMode, not interested for now
		public string[] PlayStyle { get; set; }
		public bool PMFriendsOnly { get; set; }
		public int PostCount { get; set; }
		public string? ProfileColour { get; set; }
		public object ProfileOrder { get; set; } // ProfilePage[], not interested for now
		public string? Title { get; set; }
		public string? TitleUrl { get; set; }
		public string? Twitter { get; set; }
		public string Username { get; set; }
		public string? Website { get; set; }

		public OsuApiUserResponseRawData ToRawData()
		{
			return new()
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
