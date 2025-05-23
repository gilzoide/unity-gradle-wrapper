#if UNITY_ANDROID
using System.Diagnostics;
using System.IO;
using UnityEditor.Android;
using Debug = UnityEngine.Debug;

namespace Gilzoide.GradleWrapperGenerator.Editor
{
    public class GradleWrapperGenerator : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 0;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string gradleVersion = GradleWrapperSettings.GradleVersion;
            if (string.IsNullOrWhiteSpace(gradleVersion))
            {
                gradleVersion = GradleWrapperPaths.FindGradleVersion();
            }
            if (string.IsNullOrWhiteSpace(gradleVersion))
            {
                Debug.LogWarning($"[{nameof(GradleWrapperGenerator)}] Unable to find Gradle version. Skipping Gradle Wrapper generation.");
                return;
            }

            // Use project's root folder instead of /unityLibrary
            path = Path.GetDirectoryName(path);

            // Using an empty build script makes Gradle skip configuring the whole project.
            // Useful in case build scripts are incompatible with the version of Gradle that will generate the wrapper.
            string emptyGradleScriptFile = "empty.gradle";
            string emptyGradleScriptPath = Path.Combine(path, emptyGradleScriptFile);
            File.WriteAllText(emptyGradleScriptPath, "");

            var startInfo = new ProcessStartInfo()
            {
                FileName = GradleWrapperPaths.FindJavaExecutable(),
                Arguments = $"-jar \"{GradleWrapperPaths.FindGradleJar()}\" wrapper --gradle-version {gradleVersion} -b {emptyGradleScriptFile} --no-daemon",
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
    }
}
#endif
