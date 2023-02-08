// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Utils.Process;

public record ErrorMessages
{
	public static readonly ProcessResult ClientError = new ProcessResult
	{
		Source = ProcessName.Client,
		Code = 1,
		Message = "Main client exception occurred."
	};
	public static readonly ProcessResult ClientActionSkipUpdateError = new ProcessResult
	{
		Source = ProcessName.Client,
		Code = 101,
		Message = "Client asked to skip data update."
	};
	public static readonly ProcessResult ClientCommandInterruptError = new ProcessResult
	{
		Source = ProcessName.Client,
		Code = 301,
		Message = "Client interrupted the process."
	};
	public static readonly ProcessResult ClientCommandSendMessageError = new ProcessResult
	{
		Source = ProcessName.Client,
		Code = 302,
		Message = "Client interrupted the process and needs to send message to sender."
	};
	public static readonly ProcessResult ClientEmbedDescriptionError = new ProcessResult
	{
		Source = ProcessName.Client,
		Code = 501,
		Message = "Specified embed description is invalid."
	};

	public static readonly ProcessResult ApiError = new ProcessResult
	{
		Source = ProcessName.Api,
		Code = 1,
		Message = "API library exception occurred."
	};
	public static readonly ProcessResult ApiInstanceError = new ProcessResult
	{
		Source = ProcessName.Api,
		Code = 2,
		Message = "API library instance exception occurred."
	};
	public static readonly ProcessResult ApiResponseError = new ProcessResult
	{
		Source = ProcessName.Api,
		Code = 3,
		Message = "API returned non-OK status code."
	};
	public static readonly ProcessResult ApiTypeChangedError = new ProcessResult
	{
		Source = ProcessName.Api,
		Code = 4,
		Message = "API returned data type has changed."
	};

	public static readonly ProcessResult OsuApiTokenRequestError = new ProcessResult
	{
		Source = ProcessName.Api,
		Code = 101,
		Message = "An unhandled error occurred while requesting token."
	};
	public static readonly ProcessResult OsuApiTokenRevokationError = new ProcessResult
	{
		Source = ProcessName.Api,
		Code = 102,
		Message = "An unhandled error occurred while revoking token."
	};
	public static readonly ProcessResult OsuApiTokenRevokedError = new ProcessResult
	{
		Source = ProcessName.Api,
		Code = 103,
		Message = "Token either already revoked or expired."
	};

	public static readonly ProcessResult OsuApiDataRequestError = new ProcessResult
	{
		Source = ProcessName.Api,
		Code = 201,
		Message = "An unhandled exception occurred while requesting osu!api data."
	};
	public static readonly ProcessResult OsuApiDataNullError = new ProcessResult
	{
		Source = ProcessName.Api,
		Code = 202,
		Message = "osu!api response parsed as null."
	};

	public static readonly ProcessResult OsuStatsDataRequestError = new ProcessResult
	{
		Source = ProcessName.Api,
		Code = 301,
		Message = "An unhandled exception occurred while requesting osu!stats data."
	};
	public static readonly ProcessResult OsuStatsDataNullError = new ProcessResult
	{
		Source = ProcessName.Api,
		Code = 302,
		Message = "osu!stats response parsed as null."
	};

	public static readonly ProcessResult DatabaseError = new ProcessResult
	{
		Source = ProcessName.Database,
		Code = 1,
		Message = "Database library exception occurred."
	};
	public static readonly ProcessResult DatabaseInstanceError = new ProcessResult
	{
		Source = ProcessName.Database,
		Code = 2,
		Message = "Database client instance exception occurred."
	};
	public static readonly ProcessResult DatabaseConnectionError = new ProcessResult
	{
		Source = ProcessName.Database,
		Code = 101,
		Message = "Database connection error occurred."
	};
	public static readonly ProcessResult DatabaseAuthenticationError = new ProcessResult
	{
		Source = ProcessName.Database,
		Code = 102,
		Message = "Database authentication error occurred."
	};
	public static readonly ProcessResult DatabaseQueryError = new ProcessResult
	{
		Source = ProcessName.Database,
		Code = 201,
		Message = "Database query error occurred."
	};
	public static readonly ProcessResult DatabaseNotFoundError = new ProcessResult
	{
		Source = ProcessName.Database,
		Code = 301,
		Message = "Data with specified query not found in database."
	};
	public static readonly ProcessResult DatabaseDuplicateError = new ProcessResult
	{
		Source = ProcessName.Database,
		Code = 302,
		Message = "Duplicated record found with the specified query."
	};

	public static readonly ProcessResult UtilError = new ProcessResult
	{
		Source = ProcessName.Util,
		Code = 1,
		Message = "Utilities library exception occurred."
	};

	public static readonly ProcessResult UtilParserEmbedTitleError = new ProcessResult
	{
		Source = ProcessName.Util,
		Code = 101,
		Message = "Specified embed title is invalid."
	};
	public static readonly ProcessResult UtilParserEmbedDescriptionError = new ProcessResult
	{
		Source = ProcessName.Util,
		Code = 102,
		Message = "Specified embed description is invalid."
	};
}
