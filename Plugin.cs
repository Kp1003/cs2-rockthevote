using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using cs2_rockthevote.Features;
using Microsoft.Extensions.DependencyInjection;
using static CounterStrikeSharp.API.Core.Listeners;

namespace cs2_rockthevote
{
    public class PluginDependencyInjection : IPluginServiceCollection<Plugin>
    {
        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            var di = new DependencyManager<Plugin, Config>();
            di.LoadDependencies(typeof(Plugin).Assembly);
            di.AddIt(serviceCollection);
            serviceCollection.AddScoped<StringLocalizer>();
        }
    }

    public partial class Plugin : BasePlugin, IPluginConfig<Config>
    {
        public override string ModuleName => "RockTheVote";
        public override string ModuleVersion => "1.8.8-host_workshop_map_change";
        public override string ModuleAuthor => "abnerfs (Updated by kMagic)";
        public override string ModuleDescription => "https://github.com/Kp1003/cs2-server-plugins";


        private readonly DependencyManager<Plugin, Config> _dependencyManager;
        private readonly NominationCommand _nominationManager;
        private readonly ChangeMapManager _changeMapManager;
        private readonly VotemapCommand _votemapManager;
        private readonly RockTheVoteCommand _rtvManager;
        private readonly TimeLeftCommand _timeLeft;
        private readonly NextMapCommand _nextMap;

        public Plugin(DependencyManager<Plugin, Config> dependencyManager,
            NominationCommand nominationManager,
            ChangeMapManager changeMapManager,
            VotemapCommand voteMapManager,
            RockTheVoteCommand rtvManager,
            TimeLeftCommand timeLeft,
            NextMapCommand nextMap)
        {
            _dependencyManager = dependencyManager;
            _nominationManager = nominationManager;
            _changeMapManager = changeMapManager;
            _votemapManager = voteMapManager;
            _rtvManager = rtvManager;
            _timeLeft = timeLeft;
            _nextMap = nextMap;
        }

        public Config? Config { get; set; }

        public string Localize(string prefix, string key, params object[] values)
        {
            return $"{Localizer[prefix]} {Localizer[key, values]}";
        }

        public override void Load(bool hotReload)
        {
            _dependencyManager.OnPluginLoad(this);
            RegisterListener<OnMapStart>(_dependencyManager.OnMapStart);
            
            AddCommand("css_rtv", "Rock the vote", (player, info) => {
                if (player != null) _rtvManager.CommandHandler(player);
            });
            
            AddCommand("css_nominate", "Nominate a map", (player, info) => {
                if (player != null) {
                    var map = info.ArgCount > 1 ? info.GetArg(1).Trim() : "";
                    _nominationManager.CommandHandler(player, map);
                }
            });
            
            AddCommand("css_votemap", "Vote for a map", (player, info) => {
                if (player != null) {
                    var map = info.ArgCount > 1 ? info.GetArg(1).Trim() : "";
                    _votemapManager.CommandHandler(player, map);
                }
            });
            
            AddCommand("css_timeleft", "Check time left", (player, info) => {
                if (player != null) _timeLeft.CommandHandler(player);
            });
            
            AddCommand("css_nextmap", "Check next map", (player, info) => {
                if (player != null) _nextMap.CommandHandler(player);
            });
        }

        public void OnConfigParsed(Config config)
        {
            Config = config;

            if (Config.Version < 9)
                Console.WriteLine("[RockTheVote] please delete it from addons/counterstrikesharp/configs/plugins/RockTheVote and let the plugin recreate it on load");

            if (Config.Version < 7)
                throw new Exception("Your config file is too old, please delete it from addons/counterstrikesharp/configs/plugins/RockTheVote and let the plugin recreate it on load");

            _dependencyManager.OnConfigParsed(config);
        }
    }
}