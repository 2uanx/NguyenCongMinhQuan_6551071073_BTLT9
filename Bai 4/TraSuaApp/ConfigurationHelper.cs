using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TraSuaApp.Models;

namespace TraSuaApp;

public static class ConfigurationHelper
{
    private static IConfiguration? _configuration;
    private static string? _resolvedConnectionString;

    public static IConfiguration Configuration
    {
        get
        {
            if (_configuration == null)
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json",  optional: true, reloadOnChange: true);

                _configuration = builder.Build();
            }
            return _configuration;
        }
    }

    public static string GetConnectionString()
    {
        if (_resolvedConnectionString != null)
            return _resolvedConnectionString;

        string configured = Configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=QL_TraSua;Trusted_Connection=True;TrustServerCertificate=True;";

        if (CanConnect(configured))
        {
            _resolvedConnectionString = configured;
            return configured;
        }

        string[] candidates = new[]
        {
            "Server=(localdb)\\MSSQLLocalDB;Database=QL_TraSua;Trusted_Connection=True;TrustServerCertificate=True;",
            "Server=.;Database=QL_TraSua;Trusted_Connection=True;TrustServerCertificate=True;",
            "Server=localhost;Database=QL_TraSua;Trusted_Connection=True;TrustServerCertificate=True;",
            "Server=.\\SQLEXPRESS;Database=QL_TraSua;Trusted_Connection=True;TrustServerCertificate=True;"
        };

        foreach (var candidate in candidates)
        {
            if (candidate != configured && CanConnect(candidate))
            {
                _resolvedConnectionString = candidate;
                return candidate;
            }
        }

        _resolvedConnectionString = configured;
        return configured;
    }

    private static bool CanConnect(string connectionString)
    {
        try
        {
            var csb = new SqlConnectionStringBuilder(connectionString)
            {
                ConnectTimeout = 2
            };
            using var conn = new SqlConnection(csb.ConnectionString);
            conn.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static PaymentConfig GetPaymentConfig()
    {
        var config = new PaymentConfig();
        var section = Configuration.GetSection("Payment");
        if (section.Exists())
        {
            config.BankId = section["BankId"] ?? "";
            config.AccountNo = section["AccountNo"] ?? "";
            config.AccountName = section["AccountName"] ?? "";
            config.Template = section["Template"] ?? "compact2";
            config.ShopName = section["ShopName"] ?? "";
        }
        return config;
    }
}
