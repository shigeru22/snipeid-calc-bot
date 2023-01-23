// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Database.Exceptions;

public class DatabaseException : Exception
{
	public DatabaseException() : base("Database error occurred.") { }
	public DatabaseException(string message) : base($"Database error occurred: {message}") { }
}
