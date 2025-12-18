using p3rpc.credits.framework.reloaded.Template.Configuration;

namespace p3rpc.credits.framework.reloaded.Configuration
{
    /// <summary>
    /// Configuration class for the Credits Framework mod.
    /// Inherits from the base Configurable class.
    /// </summary>
    public class Config : Configurable<Config> { }

    /// <summary>
    /// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
    /// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
    /// </summary>
    public class ConfiguratorMixin : ConfiguratorMixinBase
    {
        // 
    }
}
