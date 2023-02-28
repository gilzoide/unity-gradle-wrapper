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
                FileName = FindJavaExecutable(),
                Arguments = $"-jar \"{FindGradleJar()}\" wrapper --gradle-version {gradleVersion} -b {emptyGradleScriptFile}",
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
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

        public static string FindJavaExecutable()
        {
#if UNITY_2019_3_OR_NEWER
            string javaRoot = !string.IsNullOrEmpty(AndroidExternalToolsSettings.jdkRootPath)
                ? AndroidExternalToolsSettings.jdkRootPath
                : Path.Combine(GetUnityAndroidPlayerRoot(), "OpenJDK");
#else
            string javaRoot = Path.Combine(GetUnityAndroidPlayerRoot(), "OpenJDK");
#endif
#if UNITY_EDITOR_WIN
            string javaExe = Path.Combine(javaRoot, "bin", "java.exe");
#else
            string javaExe = Path.Combine(javaRoot, "bin", "java");
#endif
            if (File.Exists(javaExe))
            {
                return javaExe;
            }

            Debug.LogWarning($"[{nameof(GradleWrapperGenerator)}] Unable to find Java executable at '{javaExe}'. Falling back to system's `java` command");
            return "java";
        }

        public static string FindGradleJar()
        {
#if UNITY_2019_3_OR_NEWER
            string gradleRoot = !string.IsNullOrEmpty(AndroidExternalToolsSettings.gradlePath)
                ? AndroidExternalToolsSettings.gradlePath
                : Path.Combine(GetUnityAndroidPlayerRoot(), "Tools", "gradle");
#else
            string gradleRoot = Path.Combine(GetUnityAndroidPlayerRoot(), "Tools", "gradle");
#endif
            return FindFirstFileWithPattern(Path.Combine(gradleRoot, "lib"), "gradle-launcher*.jar");
        }

        public static string GetUnityAndroidPlayerRoot()
        {
            string unityRoot = Path.GetDirectoryName(EditorApplication.applicationPath);
#if UNITY_EDITOR_OSX
            return Path.Combine(unityRoot, "PlaybackEngines", "AndroidPlayer");
#else
            return Path.Combine(unityRoot, "Data", "PlaybackEngines", "AndroidPlayer");
#endif
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
