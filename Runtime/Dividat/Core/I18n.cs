using UnityEngine;
using System;
using SimpleJSON;

public static class I18n
{

    private static JSONNode dict;

    public static String Get(String term)
    {
        EnsureDict();

        if (dict[term] != null)
        {
            return dict[term];
        }
        else
        {
            return "i18n:" + term;
        }
    }

    /*
     * Make sure dict is loaded.
     */
    private static void EnsureDict()
    {
        // Return if set before
        if (dict != null)
        {
            return;
        }

        // Try to load current system language
        if (dict != null)
        {
            dict = LoadDict(Application.systemLanguage);
            return;
        }

        // Try to load default language
        if (dict == null)
        {
            dict = LoadDict(SystemLanguage.German);
            return;
        }


        // Set empty dict
        dict = new JSONObject();
    }

    /*
     * Load and parse dict from file.
     */
    private static JSONNode LoadDict(SystemLanguage language)
    {
        String dictCode = GetDictCode(language);
        String dictPath = "I18n/" + dictCode;


        TextAsset rawDict = Resources.Load(dictPath) as TextAsset;

        if (rawDict != null && rawDict.text != null)
        {
            return JSON.Parse(rawDict.text);
        }
        else
        {
            return null;
        }

    }

    /*
     * Get the dict code for a language.
     */
    private static String GetDictCode(SystemLanguage language)
    {
        switch (language)
        {
            case SystemLanguage.Dutch:
                return "nl";
            case SystemLanguage.Finnish:
                return "fi";
            case SystemLanguage.French:
                return "fr";
            case SystemLanguage.German:
                return "de";
            case SystemLanguage.Italian:
                return "it";
            case SystemLanguage.English:
            default:
                return "en";
        }
    }

}
