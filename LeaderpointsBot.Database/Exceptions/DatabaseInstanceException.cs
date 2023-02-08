// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using LeaderpointsBot.Utils.Process;

namespace LeaderpointsBot.Database.Exceptions;

public class DatabaseInstanceException : DatabaseException
{
	public DatabaseInstanceException() : base(ErrorMessages.DatabaseInstanceError.Message) { }
	public DatabaseInstanceException(string message) : base(message) { }
}
