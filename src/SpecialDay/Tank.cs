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


public class SDTank : SDBase
{
    public override void Setup()
    {
        LocalizeAnnounce("sd.tank_start");
        LocalizeAnnounce("sd.damage_enable",delay);
    }

    public override void MakeBoss(CCSPlayerController? tank, int count)
    {
        if(tank != null && tank.IsLegalAlive())
        {
            LocalizeAnnounce($"sd.tank",tank.PlayerName);

            // give the tank the HP and swap him
            tank.SetHealth(count * 100);
            tank.SetColour(Lib.RED);
            tank.SwitchTeam(CsTeam.CounterTerrorist);
        }

        else
        {
            Chat.Announce("[ERROR]: ","Error picking tank");
        }
    }

    public override void Start()
    {
        LocalizeAnnounce("sd.fight");
        Lib.SwapAllT();

        (CCSPlayerController? boss, int count) = PickBoss();
        MakeBoss(boss,count);
    }

    public override void End()
    {
        LocalizeAnnounce("sd.tank_end");
    }

    public override void SetupPlayer(CCSPlayerController player)
    {
        player.EventGunMenu();
    }
}