using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using EduSystem.Shared.Infrastructure.Utilities;
using Serilog;

namespace EduSystem.Shared.Infrastructure.LogHelper;

public class LoggingHelper
{
    private readonly ILogger _logger;
    private readonly string _serviceName;
    private static int _sequenceCounter = 0;
    private static readonly object _lock = new object();
    private static readonly string _processSalt = RandomNumberGenerator
        .GetInt32(0, 1296)
        .ToString()
        .PadLeft(3, '0');

    public LoggingHelper(string serviceName)
    {
        _serviceName = serviceName;

        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.File(
                path: $"logs/{_serviceName}-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level}] {UniqueCode} - {Message}{NewLine}{Exception}",
                shared: true
            )
            .CreateLogger();
    }

    /// <summary>
    /// Generate unique code: YYYYMMDDHHMMSS-SERVICECODE-SEQ
    /// Example: 20241225143052-ORD-001
    /// </summary>
    ///

    public string GenerateUniqueCode()
    {
        lock (_lock)
        {
            var timestamp = DateTimeHelper.Now.ToString("yyMMddHHmmssfff");
            var serviceCode = GetServiceCode(_serviceName);
            _sequenceCounter = (_sequenceCounter + 1) % 1000;
            var sequence = _sequenceCounter.ToString("D3");

            return $"ERR-{timestamp}-{serviceCode}{_processSalt}-{sequence}";
        }
    }

    private string GetServiceCode(string serviceName)
    {
        var letters = serviceName.ToUpper()
            .Where(c => char.IsLetter(c))
            .ToArray();

        if (letters.Length >= 3)
            return new string(letters.Take(3).ToArray());

        var serviceCode = serviceName.Length >= 3
            ? serviceName.Substring(0, 3).ToUpper()
            : serviceName.ToUpper().PadRight(3, 'X');

        return serviceCode;
    }

    public ILogger Logger => _logger;
}
