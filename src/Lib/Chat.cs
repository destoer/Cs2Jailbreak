using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;


public static class Chat
{
    // chat + centre text print
    static public void Announce(String prefix,String str)
    {
        Server.PrintToChatAll(prefix + str);
        PrintCentreAll(str);
    }

    static public void PrintPrefix(this CCSPlayerController? player, String prefix, String str)
    {
        if(player.IsLegal() && player.IsConnected() && !player.IsBot)
        {
            player.PrintToChat(prefix + str);
        }
    }

    static public void Announce(this CCSPlayerController? player,String prefix,String str)
    {
        if(player.IsLegal() && player.IsConnected() && !player.IsBot)
        {
            player.PrintPrefix(prefix,str);
            player.PrintToCenter(str);
        }
    }

    // TODO: i dont think there is a builtin func for this...
    static public void PrintCentreAll(String str)
    {
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(!player.IsLegal())
            {
                continue;
            }

            player.PrintToCenter(str);
        }
    }

    static public void PrintConsoleAll(String str, bool admin_only = false)
    {
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(!player.IsLegal() || !player.IsConnected() || player.IsBot)
            {
                continue;
            }

            if(admin_only && !player.IsGenericAdmin())
            {
                return;
            }

            player.PrintToConsole(str);
        }
    }


    static public void LocalizeAnnounce(this CCSPlayerController? player,String prefix, String name, params Object[] args)
    {
        player.Announce(prefix,Localize(name,args));
    }

    static public void LocalizeAnnounce(String prefix, String name, params Object[] args)
    {
        String str = Localize(name,args);

        Server.PrintToChatAll(prefix + str);
        PrintCentreAll(str);
    }

    public static String Localize(String name, params Object[] args)
    {
        return JailPlugin.Localize(name,args);
    }

    static public void Localize(this CCSPlayerController? player,String name, params Object[] args)
    {
        if(player.IsLegal())
        {
            player.PrintToChat(Localize(name,args));
        }    
    }

    static public void LocalisePrefix(this CCSPlayerController? player,String prefix, String name, params Object[] args)
    {
        if(player.IsLegal())
        {
            player.PrintToChat(prefix + Localize(name,args));
        }    
    }
}