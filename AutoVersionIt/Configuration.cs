using Microsoft.Extensions.Configuration;

namespace AutoVersionIt;

public static class Configuration
{
    public static IConfigurationBuilder FromEnvironmentSpecificJsonFile(this IConfigurationBuilder config, string? environmentName)
    {
        string configName;
        if (string.IsNullOrWhiteSpace(environmentName))
        {
            configName = "autoversion.json";
        }
        else
        {
            configName = string.Format("autoversion.{0}.json", environmentName.Trim().ToLowerInvariant());
        }
        
        config.AddJsonFile(configName, optional: false, reloadOnChange: false);

        return config;
    }
}