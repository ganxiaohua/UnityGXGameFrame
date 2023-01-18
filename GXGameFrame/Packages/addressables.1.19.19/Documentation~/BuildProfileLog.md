---
uid: addressables-build-profile-log
---

# Build Profiling

The Addressables build process always creates a .json log file that contains build performance information. You can find the log file in your project folder at `Library/com.unity.addressables/AddressablesBuildTEP.json`.

View the log file with the chrome://tracing tool in Google Chrome or another [Chromium]-based browser.

![](images/addr_diagnostics_1.png)

*A sample log file displayed in chrome://tracing*

__To view the build profile:__

1. Open a [Chromium]-based browser.
2. Enter [chrome://tracing] in the browser to open the [Trace Event Profiling Tool].
3. Click the __Load__ button.
4. In the file browser, navigate to your Unity project’s `Library/com.unity.addressables` folder.
5. Open the `AddressablesBuildTEP.json` file.

See [Unity Scriptable Build Pipeline] for more information about build performance logging.

[chrome://tracing]: chrome://tracing
[Chromium]: https://www.chromium.org/Home
[Trace Event Profiling Tool]: https://www.chromium.org/developers/how-tos/trace-event-profiling-tool
[Unity Scriptable Build Pipeline]: https://docs.unity3d.com/Packages/com.unity.scriptablebuildpipeline@latest