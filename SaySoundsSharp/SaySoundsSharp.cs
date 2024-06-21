using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;

namespace SaySoundsSharp;

public class SaySoundsSharp: BasePlugin {

    public override string ModuleName => "SaySoundsSharp";

    public override string ModuleVersion => "0.0.1";

    public override string ModuleDescription => "CounterStrikeSharp implementation of SaySounds";

    public override string ModuleAuthor => "faketuna";
    
    private readonly string tmp_soundEvtName = "soundevent.vsndevts";

    public override void Load(bool hotReload) {
        AddCommandListener("say", CommandListener_Say);
        AddCommandListener("say_team", CommandListener_SayTeam);
        RegisterListener<Listeners.OnServerPrecacheResources>((ResourceManifest res) => {
            res.AddResource(tmp_soundEvtName);
        });
    }

    private HookResult CommandListener_Say(CCSPlayerController? client, CommandInfo commandInfo) {
        if(client == null)
            return HookResult.Continue;

        playSaySound(client, commandInfo.ArgString);

        return HookResult.Continue;
    }

    private HookResult CommandListener_SayTeam(CCSPlayerController? client, CommandInfo commandInfo) {
        if(client == null)
            return HookResult.Continue;

        playSaySound(client, commandInfo.ArgString);

        return HookResult.Continue;
    }

    private void playSaySound(CCSPlayerController client, string commandArgs) {
        UserSaySoundInput saySound = SaySoundUtil.processUserInput(commandArgs);

        var parameters = new Dictionary<string, float>
        {
            { "volume", saySound.volume },
            { "pitch", saySound.pitch }
        };
        client.EmitSound(saySound.soundName, parameters);

    }
}