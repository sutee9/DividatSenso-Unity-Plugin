
namespace Dividat
{
    /**
    * Any and all save game data that should by serialized to the Dividat Backend must extend from this baseclass.
    * 
    * The version number attribute can be used to when upgrading the save game format, to be able to convert from an
    * old to a new save game version. No functionality is implemented to deal with this feature, it is just used as a baseline to be able to do this.
    */
    public abstract class PlaySaveGame
    {
        public int version = 1;

        public PlaySaveGame(int version)
        {
            this.version = version;
        }

        public PlaySaveGame()
        {
            version = 1;
        }
    }
}
