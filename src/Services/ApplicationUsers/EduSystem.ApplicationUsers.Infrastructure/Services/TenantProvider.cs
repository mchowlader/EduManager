using EduSystem.Shared.Infrastructure.MultiTenancy;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduSystem.ApplicationUsers.Infrastructure.Services;

public class TenantProvider(IConfiguration configuration) : ITenantProvider
{
    private readonly string _masterConnectionString = configuration.GetConnectionString("MasterDBConnection") 
        ?? throw new InvalidOperationException("Master connection string not found");

    public async Task<IEnumerable<(long Id, string Slug, string EncryptedConnectionString)>> GetActiveTenantsAsync()
    {
        var tenants = new List<(long Id, string Slug, string EncryptedConnectionString)>();

        using var connection = new SqlConnection(_masterConnectionString);
        await connection.OpenAsync();

        var query = "SELECT Id, Slug, ConnectionString FROM Tenants WHERE IsActive = 1";
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            tenants.Add((
                reader.GetInt64(0),
                reader.GetString(1),
                reader.GetString(2)
            ));
        }

        return tenants;
    }
}
