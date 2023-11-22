
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
        Lib.announce(MUTE_PREFIX,"All t's are muted for the first 30 seconds");

        Lib.mute_all();

        if(JailPlugin.global_ctx != null)
        {
            mute_timer = JailPlugin.global_ctx.AddTimer(30.0f,unmute_all,CSTimer.TimerFlags.STOP_ON_MAPCHANGE);
        }

        mute_active = true;
    }

    public void unmute_all()
    {
        Lib.announce(MUTE_PREFIX,"T's may now speak quietly");

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



    public void round_start()
    {
        Lib.kill_timer(ref mute_timer);

        mute_t();
    }

    public void round_end()
    {
        Lib.kill_timer(ref mute_timer);

        Lib.unmute_all();
    }

    public void connect(CCSPlayerController? player)
    {
        // just connected mute them
        player.mute();
    }

    public void spawn(CCSPlayerController? player)
    {
        if(!player.is_valid() || player == null)
        {
            return;
        }

        // no mute active or on ct unmute
		if(!mute_active || player.TeamNum == Lib.TEAM_CT)
		{
            player.unmute();
		}
    }   

    public void death(CCSPlayerController? player)
    {
        // mute on death
        if(!player.is_valid() || player == null)
        {
            return;
        }

        player.PrintToChat($"{MUTE_PREFIX}You are muted until the end of the round");

        player.mute();
    }

    public void switch_team(CCSPlayerController? player,int new_team)
    {
        if(!player.is_valid() || player == null)
        {
            return;
        }

        // player not alive mute
		if(!player.PawnIsAlive)
		{
            player.mute();
		}

		// player is alive
		else
		{
            // on ct fine to unmute
			if(new_team == Lib.TEAM_CT)
			{
                player.unmute();
			}

			// mute timer active, mute the client
			else if(mute_active)
			{
				player.mute();
			}
		}
    }

    CSTimer.Timer? mute_timer = null;

    static readonly String MUTE_PREFIX = $" {ChatColors.Green}[MUTE]: {ChatColors.White}";

    // has the mute timer finished?
    bool mute_active = false;
};