using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;

namespace Gilzoide.GradleWrapperGenerator.Editor
{
    public static class GradleWrapperPaths
    {
        public static string FindJavaExecutable()
        {
#if UNITY_2019_3_OR_NEWER && UNITY_ANDROID
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
#if UNITY_2019_3_OR_NEWER && UNITY_ANDROID
            string gradleRoot = !string.IsNullOrEmpty(AndroidExternalToolsSettings.gradlePath)
                ? AndroidExternalToolsSettings.gradlePath
                : Path.Combine(GetUnityAndroidPlayerRoot(), "Tools", "gradle");
#else
            string gradleRoot = Path.Combine(GetUnityAndroidPlayerRoot(), "Tools", "gradle");
#endif
            return FindFirstFileWithPattern(Path.Combine(gradleRoot, "lib"), "gradle-launcher*.jar");
        }

        public static string FindGradleVersion()
        {
            string gradleJar = FindGradleJar();
            if (string.IsNullOrEmpty(gradleJar))
            {
                return null;
            }
            Match match = new Regex(@"\d+(\.\d+)*").Match(Path.GetFileName(gradleJar));
            if (match.Success)
            {
                return match.Value;
            }
            else
            {
                return null;
            }
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
            return Directory.EnumerateFiles(dir, pattern).FirstOrDefault();
        }
    }
}
