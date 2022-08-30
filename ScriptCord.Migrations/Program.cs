using DbUp;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Reading from config");
        var configBuilder = GetConfigurationBuilder();
        var connection = GetConnectionString(configBuilder, "DefaultConnection");

        Console.WriteLine("Ensuring postgresql database connection");
        EnsureDatabase.For.PostgresqlDatabase(connection);

        Console.WriteLine("Upgrading the database with new migrations");
        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connection)
            .WithTransaction()
            .WithScriptsFromFileSystem("Core")
#if DEBUG
            .WithScriptsFromFileSystem("Test")
#endif
            .WithVariablesDisabled()
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();
        if (!result.Successful)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(result.Error);
            Console.ReadLine();
            System.Environment.Exit(1);
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Successfully updated database for ScriptCord");
        Console.ResetColor();
    }

    private static string GetConnectionString(IConfigurationBuilder builder, string name)
        => builder.Build().GetConnectionString(name);

    private static IConfigurationBuilder GetConfigurationBuilder()
    {
        var environment = Environment.GetEnvironmentVariable("ENVIRONMENT_TYPE");
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.{environment}.json");
    }
}