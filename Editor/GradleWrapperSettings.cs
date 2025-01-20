using System.IO;
using UnityEditor;
using UnityEngine;

namespace Gilzoide.GradleWrapperGenerator.Editor
{
    public static class GradleWrapperSettings
    {
        const string SETTINGS_PATH = "Project/GradleWrapper";
        const string SETTINGS_LABEL = "Gradle Wrapper";
        const string SETTINGS_TITLE = "Gradle Version";
        static readonly string SETTINGS_HELP = "A Gradle Wrapper (gradlew) will be generated with the specified version when exporting Android projects."
            + (GradleWrapperGenerator.FindGradleVersion() is string version ? $"\nLeave this empty to use the default version {version}." : "");

        static readonly string GRADLE_VERSION_FILE_PATH = Path.Combine("ProjectSettings", "GradleVersion.txt");

        public static string GradleVersion
        {
            get => File.Exists(GRADLE_VERSION_FILE_PATH) ? File.ReadAllText(GRADLE_VERSION_FILE_PATH).Trim() : "";
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    File.Delete(GRADLE_VERSION_FILE_PATH);
                }
                else
                {
                    File.WriteAllText(GRADLE_VERSION_FILE_PATH, value);
                }
            }
        }

        [SettingsProvider]
        static SettingsProvider CreateSettingsProvider()
        {
            string currentVersion = GradleVersion;
            string defaultVersion = GradleWrapperGenerator.FindGradleVersion();
            return new SettingsProvider(SETTINGS_PATH, SettingsScope.Project)
            {
                label = SETTINGS_LABEL,
                guiHandler = searchContext =>
                {
                    EditorGUILayout.HelpBox(SETTINGS_HELP, MessageType.Info);
                    string newVersion = EditorGUILayout.TextField(SETTINGS_TITLE, currentVersion);
                    if (defaultVersion != null && string.IsNullOrWhiteSpace(newVersion))
                    {
                        Color c = GUI.color;
                        GUI.color = new Color(0.7f, 0.7f, 0.7f);
                        EditorGUI.LabelField(GUILayoutUtility.GetLastRect(), " ", defaultVersion);
                        GUI.color = c;
                    }
                    if (newVersion != currentVersion)
                    {
                        currentVersion = GradleVersion = newVersion;
                    }
                },
            };
        }
    }
}
