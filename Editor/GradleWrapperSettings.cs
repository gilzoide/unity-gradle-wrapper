using System.IO;
using UnityEditor;

namespace Gilzoide.GradleWrapperGenerator.Editor
{
    public static class GradleWrapperSettings
    {
        const string SETTINGS_PATH = "Project/GradleWrapper";
        const string SETTINGS_LABEL = "Gradle Wrapper";
        const string SETTINGS_TITLE = "Gradle Version";
        const string SETTINGS_HELP = "If " + SETTINGS_TITLE + " is not empty, a Gradle Wrapper (gradlew) will be generated with the specified version when exporting Android projects.";

        static readonly string GRADLE_VERSION_FILE_PATH = Path.Combine("ProjectSettings", "GradleVersion.txt");

        public static string GradleVersion
        {
            get => File.Exists(GRADLE_VERSION_FILE_PATH) ? File.ReadAllText(GRADLE_VERSION_FILE_PATH) : "";
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
            return new SettingsProvider(SETTINGS_PATH, SettingsScope.Project)
            {
                label = SETTINGS_LABEL,
                guiHandler = searchContext =>
                {
                    EditorGUILayout.HelpBox(SETTINGS_HELP, MessageType.Info);
                    string newVersion = EditorGUILayout.TextField(SETTINGS_TITLE, currentVersion);
                    if (newVersion != currentVersion)
                    {
                        currentVersion = GradleVersion = newVersion;
                    }
                },
            };
        }
    }
}
