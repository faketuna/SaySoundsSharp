using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;

namespace SaySoundsSharp;

public class SaySoundsSharp: BasePlugin {

    public override string ModuleName => "SaySoundsSharp";

    public override string ModuleVersion => "0.0.1";

    public override string ModuleDescription => "CounterStrikeSharp implementation of SaySounds";

    public override string ModuleAuthor => "faketuna";

    private SaySoundConfig? saySoundConfig;

    private FakeConVar<string> soundPath = new("saysounds_sound_path", "Sound path of say sound", "soundevents/soundevents_saysounds.vsndevts");

    private const string saySoundMessageFormat = "%player% played %soundname%";

    public override void Load(bool hotReload) {
        AddCommandListener("say", CommandListener_Say);
        AddCommandListener("say_team", CommandListener_SayTeam);
        saySoundConfig = new SaySoundConfig(this.ModuleDirectory + "/config/saysounds.txt");
        RegisterListener<Listeners.OnServerPrecacheResources>((ResourceManifest res) => {
            res.AddResource(soundPath.Value);
        });
    }

    private HookResult CommandListener_Say(CCSPlayerController? client, CommandInfo commandInfo) {
        if(client == null)
            return HookResult.Continue;
        
        
        UserSaySoundInput saySound = SaySoundUtil.processUserInput(commandInfo.ArgString);

        string? sound = saySoundConfig!.saySounds!.GetValueOrDefault(saySound.soundName, null);

        if(sound == null)
            return HookResult.Continue;

        playSaySound(client, saySound, sound);

        printSaySoundNotification(client, saySound.soundName);
        return HookResult.Handled;
    }

    private HookResult CommandListener_SayTeam(CCSPlayerController? client, CommandInfo commandInfo) {
        if(client == null)
            return HookResult.Continue;
        
        
        UserSaySoundInput saySound = SaySoundUtil.processUserInput(commandInfo.ArgString);

        string? sound = saySoundConfig!.saySounds!.GetValueOrDefault(saySound.soundName, null);

        if(sound == null)
            return HookResult.Continue;

        playSaySound(client, saySound, sound);

        printSaySoundNotification(client, saySound.soundName);
        return HookResult.Handled;
    }

    private void playSaySound(CCSPlayerController client, UserSaySoundInput saySound, string soundName) {
        var parameters = new Dictionary<string, float>
        {
            { "volume", saySound.volume },
            { "pitch", saySound.pitch }
        };
        client.EmitSound(soundName, parameters);
    }

    private void printSaySoundNotification(CCSPlayerController client, string soundName) {
        foreach(CCSPlayerController cl in Utilities.GetPlayers()) {
            if(!cl.IsValid || cl.IsBot || cl.IsHLTV)
                continue;
            
            cl.PrintToChat(" " + saySoundMessageFormat.Replace("%player%", $"{ChatColors.LightPurple}{client.PlayerName}{ChatColors.Default}").Replace("%soundname%", $"{ChatColors.Lime}{soundName}{ChatColors.Default}"));
        }
    }
}