using System.Collections.Generic;
using SimpleJSON;

namespace Dividat {
      public abstract class Setting
    {
        public class Time : Setting
        {
            public Time(float t) { value = t; }
            public readonly float value;
        }

        public class Float : Setting
        {
            public Float(float f) { value = f; }
            public readonly float value;
        }

        public class Percentage : Setting
        {
            public Percentage(float f) { value = f; }
            public readonly float value;
        }


        public class String : Setting
        {
            public String(string s) { value = s; }
            public readonly string value;
        }

        public class Json : Setting
        {
            public Json(string t, JSONNode j) { type = t; value = j; }
            public readonly string type;
            public readonly JSONNode value;
        }

        public static Setting FromJSON(JSONNode json)
        {
            switch (json["type"].Value)
            {
                case "Time":
                    return new Time(json["value"].AsFloat);
                case "Float":
                    return new Float(json["value"].AsFloat);
                case "Percentage":
                    return new Percentage(json["value"].AsFloat);
                case "String":
                    return new String(json["value"].Value);
                default:
                    return new Json(json["type"].Value, json["value"]);
            }
        }
    }

    [System.Serializable]
    public class Settings : Dictionary<string, Setting>
    {
        public static Settings FromJSON(JSONNode json)
        {
            Settings settings = new Settings();
            foreach (var key in json.Keys)
            {
                settings.Add(key, Setting.FromJSON(json[key]));
            }
            return settings;
        }

        public static Settings FromString(string jsonString)
        {
            return FromJSON(JSON.Parse(jsonString));
        }
    }
}