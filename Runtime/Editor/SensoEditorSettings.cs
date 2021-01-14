using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Net;

namespace Dividat {

    class SensoEditorSettings : EditorWindow
    {
        static string companionRoomName = "";
        public const string COMP_ROOM_KEY = "DividatCompanionRoomName";
        public const string PLAYDEV_BASE_URL = "https://lab.dividat.ch/e/IkG_l2uAE/playDev.html#play";
        public const string AVATAR_APP_URL = "external/ws-avatar/index.html";

        

        [MenuItem("Window/Dividat/Senso Editor Companion")]
        public static void ShowWindow()
        {
            if (EditorPrefs.HasKey(COMP_ROOM_KEY)) {
                companionRoomName = EditorPrefs.GetString(COMP_ROOM_KEY);
            }
            else
            {
                EditorPrefs.SetString(COMP_ROOM_KEY, "unity-avatar-" + Random.Range(1000000, 10000000));
            }
            EditorWindow.GetWindow(typeof(SensoEditorSettings));

            EditorStyles.label.wordWrap = true;
            EditorStyles.label.margin = new RectOffset(5, 5, 5, 20);
            EditorStyles.boldLabel.margin = new RectOffset(10, 5, 5, 20);
            EditorStyles.miniButton.margin = new RectOffset(5, 5, 10, 2);

        }

        void OnGUI()
        {
            companionRoomName = EditorPrefs.GetString(COMP_ROOM_KEY);
            EditorStyles.label.wordWrap = true;
            EditorStyles.label.margin = new RectOffset(5, 5, 5, 12);
            EditorStyles.miniButton.margin = new RectOffset(5, 5, 10, 2);

            EditorGUILayout.LabelField("Dividat Senso - Editor Companion", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("The editor companion allows you to test Apps with the Senso inside the Editor.");


            if (GUILayout.Button("Start PlayDev Companion", EditorStyles.miniButton))
            {
                Application.OpenURL(GetCompanionURL());
            }
            EditorGUILayout.LabelField("Launch companion if Chrome is your standard browser.");

            if (GUILayout.Button("Copy PlayDev Companion URL", EditorStyles.miniButton))
            {
                EditorGUIUtility.systemCopyBuffer = GetCompanionURL();
            }
            EditorGUILayout.LabelField("Paste copied URL into Chrome.");

            EditorGUILayout.LabelField("To establish a connection with Senso from the editor, a Chrome browser window with a " +
                "companion app must be open. To open this companion app, press 'Start PlayDev Companion'. If chrome is " +
                "not your default browser, press 'Copy PlayDev Companion URL' and paste it into Chrome. In order for the setup to work, " +
                "please download and install the Dividat Senso driver on your system from https://github.com/dividat/driver", EditorStyles.helpBox);
            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField("Check the Senso Driver", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Use the links and info below to check if the Senso driver is properly running on your system");
            if (GUILayout.Button("Open PlayDev to check Setup", EditorStyles.miniButton))
            {
                Application.OpenURL(PLAYDEV_BASE_URL);
            }
            if (GUILayout.Button("Copy Url to PlayDev to check Setup", EditorStyles.miniButton))
            {
                EditorGUIUtility.systemCopyBuffer = PLAYDEV_BASE_URL;
            }
            if (GUILayout.Button("Copy Room Name", EditorStyles.miniButton))
            {
                EditorGUIUtility.systemCopyBuffer = companionRoomName;
            }
            EditorGUILayout.LabelField("Websocket Room: " + companionRoomName);
        }

        void OpenURL(string url)
        {
            Application.OpenURL(url);
        }

        string GetCompanionURL()
        {
            companionRoomName = EditorPrefs.GetString(COMP_ROOM_KEY);
            return PLAYDEV_BASE_URL + "/"
            + UnityEngine.Networking.UnityWebRequest.EscapeURL(AVATAR_APP_URL) + "/"
            + UnityEngine.Networking.UnityWebRequest.EscapeURL("{\"room\":{\"type\":\"String\",\"value\":\"" + companionRoomName + "\"}}");
        }
    }

}