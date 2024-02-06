
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

    void mute_t()
    {
        if(Config.muteTAllways || !Config.thirtySecMute)
        {
            return;
        }

        Chat.localize_announce(MUTE_PREFIX,"mute.thirty");

        Lib.mute_t();

        if(JailPlugin.global_ctx != null)
        {
            mute_timer = JailPlugin.global_ctx.AddTimer(30.0f,unmute_all,CSTimer.TimerFlags.STOP_ON_MAPCHANGE);
        }

        mute_active = true;
    }

    public void unmute_all()
    {
        Chat.localize_announce(MUTE_PREFIX,"mute.speak_quietly");

        // Go through and unmute all alive players!
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(player.is_valid() && player.PawnIsAlive)
            {
                player.unmute();
            }
        }

        mute_timer = null;

        mute_active = false;
    }



    public void RoundStart()
    {
        Lib.KillTimer(ref mute_timer);

        mute_t();
    }

    public void RoundEnd()
    {
        Lib.KillTimer(ref mute_timer);

        Lib.unmute_all();
    }

    public void Connect(CCSPlayerController? player)
    {
        // just connected mute them
        player.mute();
    }

    public void apply_listen_flags(CCSPlayerController player)
    {
        // default to listen all
        player.listen_all();

        // if ct cannot hear team, change listen flags to team only
        if(player.IsCt() && Config.ctVoiceOnly)
        {
            player.listen_team();
        }
    }

    public void Spawn(CCSPlayerController? player)
    {
        if(!player.is_valid())
        {
            return;
        }

        apply_listen_flags(player);

        if(Config.muteTAllways && player.IsT())
        {
            player.mute();
            return;
        }

        // no mute active or on ct unmute
		if(!mute_active || player.IsCt())
		{
            player.unmute();
		}
    }   

    public void Death(CCSPlayerController? player)
    {
        // mute on death
        if(!player.is_valid())
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
            player.localise_prefix(MUTE_PREFIX,"mute.end_round");
            player.mute();
        }
    }

    public void SwitchTeam(CCSPlayerController? player,int new_team)
    {
        if(!player.is_valid())
        {
            return;
        }

        apply_listen_flags(player);

        // player not alive mute
		if(!player.is_valid_alive())
		{
            player.mute();
		}

		// player is alive
		else
		{
            // on ct fine to unmute
			if(new_team == Player.TEAM_CT)
			{
                player.unmute();
			}

            else
            {
                // mute timer active, mute the client
                if(mute_active || Config.muteTAllways)
                {
                    player.mute();
                }
            }
		}
    }

    public JailConfig Config = new JailConfig();


    CSTimer.Timer? mute_timer = null;

    static readonly String MUTE_PREFIX = $" {ChatColors.Green}[MUTE]: {ChatColors.White}";

    // has the mute timer finished?
    bool mute_active = false;
};