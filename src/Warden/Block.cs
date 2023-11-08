

public class Block
{
    public void block_all()
    {
        if(!block_state)
        {
            Lib.announce(Warden.WARDEN_PREFIX,"Block enabled");
            Lib.block_all();
            block_state = true;
        }
    }

    public void unblock_all()
    {
        if(block_state)
        {
            Lib.announce(Warden.WARDEN_PREFIX,"No block enabled");
            Lib.unblock_all();
            block_state = false;
        }
    }

    public void round_start()
    {
        // TODO: for now we just assume no block
        // we wont have a cvar
        unblock_all();
    }
 
    bool block_state = false;
}