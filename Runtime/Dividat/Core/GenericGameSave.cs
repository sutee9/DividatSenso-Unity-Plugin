using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace Dividat {
    public sealed class GenericGameSave
    {
        /**
         * The type attribute contains the fully qualified class name of the save game.
         */
        public string type;

        /**
         * Contains a JSON string with the actual save data, which can be de-serialized using the ParseGameSave method.
         */
        public string savePayload;

        public GenericGameSave(string type, string savePayload)
        {
            this.type = type;
            this.savePayload = savePayload;
        }

        /**
         * This Method infers the type of the save data that was loaded from the Dividat Play backend
         * and then parses it into the PlaySaveGame file. PlaySaveGame is a base class to be used by all
         * game saves. 
         * 
         * Note that due to limitations with WebGL this method has several differences in execution when run in WebGL and when
         * run in the editor. This Method uses System.Reflection and GetType functionality. When compiled into WebAssembly, these
         * functions only work if the correct assembly is specified. This method assumes that the code for the PlaySaveGame is part
         * of the game's normal project code and NOT placed into the DividatSenso package folder. Only if it is part of the normal 
         * project code, Unity will compile it into the standard "Assembly-CSharp" assembly, and be able to infer the type.
         */
        public bool ParseGameSave(out PlaySaveGame saveGame)
        {
            if (type == "")
            {
                saveGame = null;
                return false;
            }
            Assembly ass = null;
            #if UNITY_WEBGL && !UNITY_EDITOR
            ass = Assembly.Load("Assembly-CSharp");
            Type t = ass.GetType(type); //Need to load the assembly from unity issue 664765
            #else
            ass = Assembly.GetCallingAssembly();
            Type t = ass.GetType(type);
            #endif
            
            if (t == null)
            {
                saveGame = null;
                Debug.LogError("ERROR in GenericGameSave->ParseGameSave: GenericGameSave's type attribute is \"" + type + "\"." +
                    " The assembly Assembly-CSharp did not contain any class of this type to parse the save game into. Please make " +
                    "sure that this type is present in your project and that it is compatible with the save game data that was " +
                    "stored. If you renamed the type, delete existing save files or fix it in the JSON.");
                return false;
            }


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
}
