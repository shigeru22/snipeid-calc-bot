// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using LeaderpointsBot.Utils.Process;

namespace LeaderpointsBot.Database.Exceptions;

public class DatabaseException : Exception
{
	public DatabaseException() : base(ErrorMessages.DatabaseError.Message) { }
	public DatabaseException(string message) : base(message) { }
}
