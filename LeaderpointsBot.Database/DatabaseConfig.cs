// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Database;

public struct DatabaseConfig
{
	private string hostName;
	private int port;
	private string username;
	private string password;
	private string databaseName;
	private string? caFilePath;

	public string HostName { readonly get => hostName; set => hostName = value; }
	public int Port { readonly get => port; set => port = value; }
	public string Username { readonly get => username; set => username = value; }
	public string Password { readonly get => password; set => password = value; }
	public string DatabaseName { readonly get => databaseName; set => databaseName = value; }
	public string? CAFilePath { readonly get => caFilePath; set => caFilePath = value; }

	public DatabaseConfig(string hostName, int port, string username, string password, string databaseName, string? caFilePath)
	{
		this.hostName = hostName;
		this.port = port;
		this.username = username;
		this.password = password;
		this.databaseName = databaseName;
		this.caFilePath = caFilePath;
	}

	public readonly string ToConnectionString()
	{
		string connectionString = $"Host={hostName};Port={port};Username={username};Password={password};Database={databaseName}";

		if (!string.IsNullOrEmpty(caFilePath))
		{
			connectionString += $";SSL Certificate={Path.GetFullPath(caFilePath)}";
		}

		return connectionString;
	}
}
