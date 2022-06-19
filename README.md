# Gradle Wrapper Generator
Automatically generate a [Gradle Wrapper (gradlew)](https://docs.gradle.org/current/userguide/gradle_wrapper.html)
when exporting Android projects in Unity.

Just choose the desired version in `Gradle Wrapper` Project Settings and voilà!
The plugin will use the JDK and Gradle configured in Unity for generating the
wrapper, so there's no need to install or configure anything else.

The `Gradle Version` setting gets stored in the
`ProjectSettings/GradleVersion.txt` file.


## Installing
Install via [Unity Package Manager](https://docs.unity3d.com/Manual/upm-ui-giturl.html)
using this repository URL:

```
https://github.com/gilzoide/unity-gradle-wrapper.git
```

Alternatively, add the following into your `Packages/manifest.json` file:

```json
{
  "dependencies": {
...
    "com.gilzoide.gradle-wrapper": "https://github.com/gilzoide/unity-gradle-wrapper.git",
...
  }
}
```


## Why
If you open an exported project in Android Studio, it will
generate a Gradle Wrapper if none is found in the project.
It will use one of the newest versions of Gradle, which may be incompatible
with the project's build scripts, and may not be able to build.

Also, even though Unity bundles it's own version of Gradle and all others are
not officially supported, sometimes projects need Gradle features that are only
available in newer versions.
In this scenario, every developer would need to download this specific version
and configure Unity to use it.
On top of that, when dealing with multiple projects, chances are not all of
them need the same version of Gradle.

With `gradlew`, all you need to do is run it, either from Android Studio or
from the console - the wrapper will take care of downloading and running the
right version.
