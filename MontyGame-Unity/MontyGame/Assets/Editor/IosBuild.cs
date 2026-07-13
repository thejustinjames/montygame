#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Builds the iPad Xcode project from the command line, so no editor clicking is
/// needed (same philosophy as the rest of this project — everything from scripts).
///
///   Unity -batchmode -quit -projectPath . -executeMethod IosBuild.Build
///
/// The output is an Xcode PROJECT, not a finished app: Unity cannot sign it. Open
/// Build/iOS/Unity-iPhone.xcodeproj, pick your Apple ID team, plug in the iPad, Run.
/// See Docs/IPAD_DEPLOY.md.
/// </summary>
public static class IosBuild
{
    const string OutDir = "Build/iOS";

    public static void Build()
    {
        // --- who the app is, on the device ---
        PlayerSettings.companyName = "Daddy";
        PlayerSettings.productName = "MontyGame";
        PlayerSettings.SetApplicationIdentifier(
            NamedBuildTarget.iOS, "com.justinjames.montygame");

        // A free Apple ID can sign for a device, but Unity can't do it — Xcode must.
        PlayerSettings.iOS.appleEnableAutomaticSigning = true;

        // --- how it presents on an iPad ---
        // AutoRotation (not a fixed orientation) — otherwise iOS locks to ONE landscape
        // direction and the game is upside down whenever the iPad is held the other way.
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.useAnimatedAutorotation = true;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.statusBarHidden = true;

        PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPadOnly;
        PlayerSettings.iOS.targetOSVersionString = "15.0";
        PlayerSettings.iOS.requiresFullScreen = true;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetArchitecture(NamedBuildTarget.iOS, 1);  // ARM64

        // The project's only scene, and it's essentially empty: GameBootstrap builds
        // the whole game in code on Play. The scene just has to BE in the build —
        // the project's own build-settings list is empty, hence naming it here.
        var scenes = new[] { "Assets/TestLevel_5Tiles.unity" };

        var opts = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = OutDir,
            target = BuildTarget.iOS,
            options = BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(opts);
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"✓ iOS Xcode project written to {OutDir} " +
                      $"({report.summary.totalSize / (1024 * 1024)} MB)");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"✗ iOS build {report.summary.result}: " +
                           $"{report.summary.totalErrors} errors");
            EditorApplication.Exit(1);
        }
    }
}
#endif
