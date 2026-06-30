using Domain.Abstractions;

namespace Domain.SystemConfigurations;

public class SystemConfiguration : Entity<SystemConfigurationId>
{
    private SystemConfiguration()
    {
    }

    private SystemConfiguration(SystemConfigurationId id, ConfigName configName, ConfigJsonValue configJsonValue) :
        base(id)
    {
        ConfigName = configName;
        ConfigJsonValue = configJsonValue;
    }

    public static SystemConfiguration Create(ConfigName configName, ConfigJsonValue configJsonValue)
    {
        return new SystemConfiguration(SystemConfigurationId.New, configName, configJsonValue);
    }

    public ConfigName ConfigName { get; private set; }
    public ConfigJsonValue ConfigJsonValue { get; private set; }
}