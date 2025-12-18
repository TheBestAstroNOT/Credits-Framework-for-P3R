using p3rpc.credits.framework.interfaces;
using p3rpc.credits.framework.reloaded.Configuration;
using p3rpc.credits.framework.reloaded.Template;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Reloaded.Mod.Interfaces.Structs.Enums;
using System.ComponentModel;
using System.Text.Json;
using Unreal.ObjectsEmitter.Interfaces;
using UnrealEssentials.Interfaces;

namespace p3rpc.credits.framework.reloaded
{
    /// <summary>
    /// Main mod class for the Credits Framework. Handles initialization and mod loading events.
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
        private readonly IUnrealEssentials _unrealessentials;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mod"/> class.
        /// Sets up dependencies and registers the Credits API.
        /// </summary>
        /// <param name="context">The mod context containing configuration and dependencies.</param>
        public Mod(ModContext context)
        {
            _modLoader = context.ModLoader;
            _hooks = context.Hooks;
            _logger = context.Logger;
            owner = context.Owner;
            _configuration = context.Configuration;
            _modConfig = context.ModConfig;
            _unreal = GetDependency<IUnreal>("Unreal from Unreal Objects Emitter");
            _unrealessentials = GetDependency<IUnrealEssentials>("Unreal Essentials");
            _hooks = GetDependency<IReloadedHooks>("IReloadedHooks");
            var objects = GetDependency<IUObjects>("Objects from Unreal Objects Emitter");
            this._credits = new(objects, _unreal, _unrealessentials);
            this._modLoader.AddOrReplaceController<ICreditsApi>(this.owner, _credits);
            this._modLoader.ModLoading += OnModLoading;
        }

        /// <summary>
        /// Event handler that is called when a mod is loading.
        /// Automatically loads credits from JSON files in the mod's Credits directory.
        /// </summary>
        /// <param name="mod">The mod that is loading.</param>
        /// <param name="config">The configuration of the loading mod.</param>
        private void OnModLoading(IModV1 mod, IModConfigV1 config)
        {
            if (!config.ModDependencies.Contains(_modConfig.ModId))
            {
                _credits.AddManualCredit(new CreditEntry { ModID = config.ModId, FirstColumnName = config.ModAuthor, FirstCommand = 5 });
                _credits.ToggleConfigbyModID(config.ModId, config.ModName, "autoheader", true);
                _logger.WriteLine($"[{_modConfig.ModId}] Enabled Autoheader and AutoAuthor for {config.ModId}.", System.Drawing.Color.Orange);
                return;
            }

            var modDir = _modLoader.GetDirectoryForModId(config.ModId);
            _logger.WriteLine($"[{config.ModId}] Attempting to load credits",  System.Drawing.Color.RebeccaPurple);
            var CreditsDir = Path.Join(modDir, "Credits");
            int AutoCredits = 1;
            if (Directory.Exists(CreditsDir))
            {
                foreach (var file in Directory.EnumerateFiles(CreditsDir, "*.json", SearchOption.AllDirectories))
                {
                    try
                    {
                        if (Path.GetFileName(file).Equals("config.json", StringComparison.CurrentCultureIgnoreCase))
                        {
                            var json = File.ReadAllText(file);
                            ConfigJson configjson = JsonSerializer.Deserialize<ConfigJson>(json);
                            if (configjson.AutoModAuthor)
                            {
                                _credits.DeleteCredit(config.ModId);
                                _credits.AddManualCredit(new CreditEntry { ModID=config.ModId, FirstColumnName=config.ModAuthor, FirstCommand=5});
                                _credits.ToggleConfigbyModID(config.ModId, config.ModName, "autoheader", true);
                                _logger.WriteLine($"[{_modConfig.ModId}] Enabled Autoheader and AutoAuthor for {config.ModId}. Removed all existing credits for the mod.", System.Drawing.Color.Orange);
                                AutoCredits = 0; //Disable autoheader if autoauthor is enabled
                                return; //Exit if autoauthor is enabled
                            }
                            if (configjson.AutoModHeader)
                            {
                                _credits.ToggleConfigbyModID(config.ModId, config.ModName, "autoheader", true);
                                _logger.WriteLine($"[{_modConfig.ModId}] Enabled Autoheader for {config.ModId}", System.Drawing.Color.Orange);
                            }
                            else
                            {
                                AutoCredits = 2;
                                _logger.WriteLine($"[{_modConfig.ModId}] Disabled Autoheader for {config.ModId}", System.Drawing.Color.Orange);
                            }
                        }
                        else
                        {
                            var json = File.ReadAllText(file);
                            List<CreditEntry>? creditEntries = JsonSerializer.Deserialize<List<CreditEntry>>(json);
                            if(creditEntries is null)
                            {
                                _logger.WriteLine($"[{_modConfig.ModId}] Failed to parse credits file at: {file}", System.Drawing.Color.Orange);
                                continue;
                            }
                            foreach (CreditEntry entry in creditEntries)
                            {
                                entry.ModID = config.ModId;
                                _credits.AddManualCredit(entry);
                                AutoCredits = (AutoCredits != 2) ? 0 : 2;
                                _logger.WriteLine($"[{_modConfig.ModId}] Loaded credits from: {Path.GetFileName(file)} from {config.ModId}", System.Drawing.Color.Orange);
                            }
                        }
                    }
                    catch
                    {
                        _logger.WriteLine($"[{_modConfig.ModId}] Failed to parse credits file at: {file}", System.Drawing.Color.Orange);
                    }
                }

                //Toggle an autoheader if not disabled or if the config file is missing
                if (AutoCredits!=0 && AutoCredits != 2)
                {
                    _credits.ToggleConfigbyModID(config.ModId, config.ModName, "autoheader", true);
                    _logger.WriteLine($"[{_modConfig.ModId}] Enabled Autoheader for {config.ModId}", System.Drawing.Color.Orange);
                }
            }
        }

        /// <summary>
        /// Gets the types exported by this mod.
        /// </summary>
        /// <returns>An array containing the ICreditsApi type.</returns>
        public Type[] GetTypes() => [typeof(ICreditsApi)];

        /// <summary>
        /// Gets a dependency from the mod loader.
        /// </summary>
        /// <typeparam name="IControllerType">The type of controller to retrieve.</typeparam>
        /// <param name="modName">The name of the mod dependency.</param>
        /// <returns>The controller instance.</returns>
        /// <exception cref="Exception">Thrown when the dependency cannot be found.</exception>
        private IControllerType GetDependency<IControllerType>(string modName) where IControllerType : class
        {
            var controller = _modLoader.GetController<IControllerType>();
            if (controller == null || !controller.TryGetTarget(out var target))
                throw new Exception($"[{_modConfig.ModName}] Could not get controller for \"{modName}\". This depedency is likely missing.");
            return target;

        }

        /// <summary>
        /// Represents the configuration JSON file format for controlling automatic header and author behavior.
        /// </summary>
        public struct ConfigJson
        {
            /// <summary>
            /// Gets or sets whether to automatically add a header for this mod's credits section.
            /// </summary>
            public bool AutoModHeader { get; set; }

            /// <summary>
            /// Gets or sets whether to automatically add the mod author to the credits.
            /// When true, removes all existing credits for the mod and adds only the author.
            /// </summary>
            public bool AutoModAuthor { get; set; }
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