#if UNITY_ANDROID
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Android;
using Debug = UnityEngine.Debug;

namespace Gilzoide.GradleWrapperGenerator.Editor
{
    public class GradleWrapperGenerator : IPostGenerateGradleAndroidProject
    {
        static readonly string EXE_EXTENSION = Path.GetExtension(EditorApplication.applicationPath);
        static readonly string UNITY_ANDROID_PATH = Path.Combine(
            EditorApplication.applicationContentsPath, "PlaybackEngines", "AndroidPlayer"
        );
        static readonly string UNITY_JAVA_EXE = Path.Combine(
            UNITY_ANDROID_PATH, "OpenJDK", "bin", "java" + EXE_EXTENSION
        );
        static readonly string GRADLE_JAR = FindFirstFileWithPattern(
            Path.Combine(UNITY_ANDROID_PATH, "Tools", "gradle", "lib"),
            "gradle-launcher*.jar"
        );

        public int callbackOrder => 0;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            if (!EditorUserBuildSettings.exportAsGoogleAndroidProject)
            {
                return;
            }

            string gradleVersion = GradleWrapperSettings.GradleVersion;
            if (string.IsNullOrWhiteSpace(gradleVersion))
            {
                return;
            }

            // Use project's root folder instead of /unityLibrary
            path = Path.GetDirectoryName(path);

            // Using an empty build script makes Gradle skip configuring the
            // whole project.
            // Useful in case the build scripts are incompatible with the
            // version of Gradle that will generate the wrapper.
            string emptyGradleScriptFile = "empty.gradle";
            string emptyGradleScriptPath = Path.Combine(path, emptyGradleScriptFile);
            File.WriteAllText(emptyGradleScriptPath, "");

            var startInfo = new ProcessStartInfo()
            {
                FileName = GetJavaExecutable(),
                Arguments = $"-jar \"{GRADLE_JAR}\" wrapper --gradle-version {gradleVersion} -b {emptyGradleScriptFile}",
                RedirectStandardError = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = path,
            };
            Debug.Log($"[{nameof(GradleWrapperGenerator)}] Running `\"{startInfo.FileName}\" {startInfo.Arguments}`");
            using (var process = Process.Start(startInfo))
            {
                string stderr = process.StandardError.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(stderr))
                {
                    Debug.LogError($"[{nameof(GradleWrapperGenerator)}] {stderr.Trim()}");
                }

                process.WaitForExit();
            }

            File.Delete(emptyGradleScriptPath);
        }

        public static string GetJavaExecutable()
        {
#if UNITY_2019_3_OR_NEWER
            if (!string.IsNullOrEmpty(AndroidExternalToolsSettings.jdkRootPath))
            {
                string javaExecutable = Path.Combine(AndroidExternalToolsSettings.jdkRootPath, "bin", "java" + EXE_EXTENSION);
                if (File.Exists(javaExecutable))
                {
                    return javaExecutable;
                }

                Debug.LogWarning($"[{nameof(GradleWrapperGenerator)}] Unable to find Java executable in External Tools JDK path '{AndroidExternalToolsSettings.jdkRootPath}'. Searched for executable file '{javaExecutable}'");
            }
#endif
            if (File.Exists(UNITY_JAVA_EXE))
            {
                return UNITY_JAVA_EXE;
            }

            Debug.LogWarning($"[{nameof(GradleWrapperGenerator)}] Unable to find Java executable installed with Unity editor. Falling back to system's `java` command");
            return "java";
        }

        public static string FindFirstFileWithPattern(string dir, string pattern)
        {
            string[] files = Directory.GetFiles(dir, pattern);
            if (files.Length == 0)
            {
                throw new FileNotFoundException($"Couldn't find any file with pattern '{pattern}' in '{dir}'");
            }
            return files[0];
        }
    }
}
#endif
