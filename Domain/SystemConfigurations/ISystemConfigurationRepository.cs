namespace Domain.SystemConfigurations;

public interface ISystemConfigurationRepository
{
    void Add(SystemConfiguration systemConfiguration);
    void AddRange(List<SystemConfiguration> systemConfigurations);
    Task<SystemConfiguration?> GetConfigByName(ConfigName configName, CancellationToken cancellationToken = default);
    Task<List<SystemConfiguration>?> GetAll(CancellationToken cancellationToken = default);
}