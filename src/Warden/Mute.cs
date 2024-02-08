
// TODO: we want to just copy hooks from other plugin and name them in here
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CSTimer = CounterStrikeSharp.API.Modules.Timers;

public class Mute
{
    void MuteT()
    {
        if(Config.muteTAllways || !Config.thirtySecMute)
        {
            return;
        }

        Chat.LocalizeAnnounce(MUTE_PREFIX,"mute.thirty");

        Lib.MuteT();

        if(JailPlugin.globalCtx != null)
        {
            muteTimer = JailPlugin.globalCtx.AddTimer(30.0f,UnMuteAll,CSTimer.TimerFlags.STOP_ON_MAPCHANGE);
        }

        muteActive = true;
    }

    public void UnMuteAll()
    {
        Chat.LocalizeAnnounce(MUTE_PREFIX,"mute.speak_quietly");

        // Go through and unmute all alive players!
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(player.IsLegal() && player.PawnIsAlive)
            {
                player.UnMute();
            }
        }

        muteTimer = null;

        muteActive = false;
    }



    public void RoundStart()
    {
        Lib.KillTimer(ref muteTimer);

        MuteT();
    }

    public void RoundEnd()
    {
        Lib.KillTimer(ref muteTimer);

        Lib.UnMuteAll();
    }

    public void Connect(CCSPlayerController? player)
    {
        // just connected mute them
        player.Mute();
    }

    public void ApplyListenFlags(CCSPlayerController player)
    {
        // default to listen all
        player.ListenAll();

        // if ct cannot hear team, change listen flags to team only
        if(player.IsCt() && Config.ctVoiceOnly)
        {
            player.ListenTeam();
        }
    }

    public void Spawn(CCSPlayerController? player)
    {
        if(!player.IsLegal())
        {
            return;
        }

        ApplyListenFlags(player);

        if(Config.muteTAllways && player.IsT())
        {
            player.Mute();
            return;
        }

        // no mute active or on ct unmute
		if(!muteActive || player.IsCt())
		{
            player.UnMute();
		}
    }   

    public void Death(CCSPlayerController? player)
    {
        // mute on death
        if(!player.IsLegal())
        {
            return;
        }

        // warden with no forced removal let them keep speaking
        if(JailPlugin.IsWarden(player) && !Config.wardenForceRemoval)
        {
            return;
        }

        if(Config.muteDead)
        {
            player.LocalisePrefix(MUTE_PREFIX,"mute.end_round");
            player.Mute();
        }
    }

    public void SwitchTeam(CCSPlayerController? player,int new_team)
    {
        if(!player.IsLegal())
        {
            return;
        }

        ApplyListenFlags(player);

        // player not alive mute
		if(!player.IsLegalAlive())
		{
            player.Mute();
		}

		// player is alive
		else
		{
            // on ct fine to unmute
			if(new_team == Player.TEAM_CT)
			{
                player.UnMute();
			}

            else
            {
                // mute timer active, mute the client
                if(muteActive || Config.muteTAllways)
                {
                    player.Mute();
                }
            }
		}
    }

    public JailConfig Config = new JailConfig();


    CSTimer.Timer? muteTimer = null;

    public static String MUTE_PREFIX = $" {ChatColors.Green}[MUTE]: {ChatColors.White}";

    // has the mute timer finished?
    bool muteActive = false;
};