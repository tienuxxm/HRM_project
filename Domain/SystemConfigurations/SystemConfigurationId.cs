namespace Domain.SystemConfigurations;

public record SystemConfigurationId(Guid Value)
{
    public static SystemConfigurationId New => new SystemConfigurationId(Guid.NewGuid());
}