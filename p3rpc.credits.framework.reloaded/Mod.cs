using p3rpc.credits.framework.interfaces;
using p3rpc.credits.framework.reloaded.Configuration;
using p3rpc.credits.framework.reloaded.Template;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Unreal.ObjectsEmitter.Interfaces;

namespace p3rpc.credits.framework.reloaded
{
    /// <summary>
    /// Your mod logic goes here.
    /// </summary>
    public class Mod : ModBase, IExports // <= Do not Remove.
    {
        /// <summary>
        /// Provides access to the mod loader API.
        /// </summary>
        private readonly IModLoader _modLoader;

        /// <summary>
        /// Provides access to the Reloaded.Hooks API.
        /// </summary>
        /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
        private readonly IReloadedHooks? _hooks;

        /// <summary>
        /// Provides access to the Reloaded logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Entry point into the mod, instance that created this class.
        /// </summary>
        private readonly IMod owner;

        /// <summary>
        /// Provides access to this mod's configuration.
        /// </summary>
        private Config _configuration;

        /// <summary>
        /// The configuration of the currently executing mod.
        /// </summary>
        private readonly IModConfig _modConfig;
        private readonly CreditsApi _credits;
        private readonly IUnreal _unreal;

        public Mod(ModContext context)
        {
            _modLoader = context.ModLoader;
            _hooks = context.Hooks;
            _logger = context.Logger;
            owner = context.Owner;
            _configuration = context.Configuration;
            _modConfig = context.ModConfig;
            _unreal = GetDependency<IUnreal>("Unreal from Unreal Objects Emitter");
            var objects = GetDependency<IUObjects>("Objects from Unreal Objects Emitter");
            this._credits = new(objects, this._unreal);
            this._modLoader.AddOrReplaceController<ICreditsApi>(this.owner, _credits);
        }

        public Type[] GetTypes() => [typeof(ICreditsApi)];

        private IControllerType GetDependency<IControllerType>(string modName) where IControllerType : class
        {
            var controller = _modLoader.GetController<IControllerType>();
            if (controller == null || !controller.TryGetTarget(out var target))
                throw new Exception($"[{_modConfig.ModName}] Could not get controller for \"{modName}\". This depedency is likely missing.");
            return target;

        }


        #region Standard Overrides
        public override void ConfigurationUpdated(Config configuration)
        {
            // Apply settings from configuration.
            // ... your code here.
            _configuration = configuration;
            _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
        }
        #endregion

        #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Mod() { }
#pragma warning restore CS8618
        #endregion
    }
}