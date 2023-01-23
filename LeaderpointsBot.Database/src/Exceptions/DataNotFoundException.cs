// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Database.Exceptions;

public class DataNotFoundException : DatabaseException
{
	public DataNotFoundException() : base("Data with specified query not found in database.") { }
	public DataNotFoundException(string message) : base($"Data with specified query not found in database: {message}") { }
}
