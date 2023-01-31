// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Utils.Process;

public enum ProcessName
{
	Client = 1,
	Api,
	Database,
	Util
}

public readonly struct ProcessResult
{
	public ProcessName? Source { get; init; }
	public ushort Code { get; init; }
	public string Message { get; init; }

	public string ErrorCode
	{
		get
		{
			char prefix = Source switch
			{
				ProcessName.Client => 'C',
				ProcessName.Api => 'A',
				ProcessName.Database => 'D',
				ProcessName.Util => 'U',
				_ => 'E'
			};

			return $"{prefix}{Code.ToString().PadLeft(4, '0')}";
		}
	}
}
