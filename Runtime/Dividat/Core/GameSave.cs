using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dividat {

    [System.Serializable]
    public abstract class GameSaveWrapper<T>
    {
        public string gameSaveType;
        public string gameSaveContent;

        protected GameSaveWrapper()
        {
            gameSaveType = this.GetType().ToString();
        }

        public static Type GetType(string memoryString)
        {
            GameSaveBase typeDetermination = JsonUtility.FromJson<GameSaveBase>(memoryString);
            return Type.GetType(typeDetermination.gameSaveType);
        }

        public static T CreateFromString(string memoryString)
        {
            return JsonUtility.FromJson<T>(memoryString);
        }
    }

    public class GameSaveBase : GameSaveWrapper<GameSaveBase>{

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static object FromJson(string memoryContent, Type type)
        {
            return JsonUtility.FromJson(memoryContent, type);
        }
    }

    public class GameSaveExample : GameSaveBase
    {
        public int whatever;
    }

}