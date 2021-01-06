using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Dividat {
    public sealed class GenericGameSave
    {
        public string type;
        public string savePayload;

        public GenericGameSave(string type, string savePayload)
        {
            this.type = type;
            this.savePayload = savePayload;
        }

        public bool ParseGameSave(out PlaySaveGame saveGame)
        {
            Type t = Type.GetType(type);
            Debug.Log("GenericGameSave.ParseGameSave->Type: " + t);
            if (t.IsSubclassOf(typeof(PlaySaveGame)))
            {
                saveGame = (PlaySaveGame)JsonUtility.FromJson(savePayload, t);
                return true;
            }
            else
            {
                saveGame = null;
                return false;
            }
        }

        public static GenericGameSave Wrap(PlaySaveGame savegamePayload)
        {
            return new GenericGameSave(savegamePayload.GetType().ToString(), JsonUtility.ToJson(savegamePayload));
        }
    }

    public abstract class PlaySaveGame {
        
    }

    public class ExampleSaveGame : PlaySaveGame {
       public int number;
    }
}
