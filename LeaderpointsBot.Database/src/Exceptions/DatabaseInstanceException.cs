// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Database.Exceptions;

public class DatabaseInstanceException : DatabaseException
{
	public DatabaseInstanceException() : base("Database instance error occurred.") { }
	public DatabaseInstanceException(string message) : base($"Database instance error occurred: {message}") { }
}
