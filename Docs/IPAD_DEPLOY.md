# Getting MontyGame onto the iPad

**Status: done — the game is installed and playing on "JJ iPad Pro" (2026-07-14).**
Board, sound, dice, and the win celebration all confirmed working on device.

The goal: a real app icon on Monty's iPad that launches the game. This is a **local
test install**, not the App Store.

**One-time setup you must do yourself** (it needs your Apple ID — I can't type it for you):
sign in to Xcode → **Xcode ▸ Settings ▸ Accounts ▸ + ▸ Apple ID**. A free Apple ID is
enough. Everything else below is scripted.

> ⚠️ **Free Apple ID = the app stops working after 7 days.** It's a signing limit, not a
> bug: reinstall from Xcode (one click) and it works for another 7. A paid Apple Developer
> account ($99/yr) extends this to a year. For "Monty plays it this weekend", free is fine.

---

## What's already done

- Unity **iOS Build Support** module: installed.
- **Xcode 26.5**: installed.
- `Assets/Editor/IosBuild.cs`: an editor script that configures and builds everything from
  the command line — bundle id `com.justinjames.montygame`, **iPad only**, **landscape**,
  fullscreen, no status bar, IL2CPP/ARM64, iOS 15+.
- The IMGUI layer is now **resolution-scaled** (`DesignHeight` in `GameController.cs`), so
  the HUD stays finger-sized on a Retina iPad instead of rendering as a speck.

## Step 1 — build the Xcode project (scripted)

**Close the Unity Editor first.** A batch build against a project the editor has open
aborts after a few lines — and does so quietly, which looks like success.

```bash
cd MontyGame-Unity/MontyGame
/Applications/Unity/Hub/Editor/6000.5.2f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit -projectPath "$PWD" \
  -executeMethod IosBuild.Build \
  -logFile /tmp/ios-build.log
grep -E "✓ iOS Xcode project|error CS|Build.*Failed" /tmp/ios-build.log
```

This takes a while the first time (IL2CPP converts all the C# to C++). The output is
`MontyGame-Unity/MontyGame/Build/iOS/` — an **Xcode project, not a finished app**. Unity
cannot sign it; only Xcode can, which is why the last mile is manual.

## Step 2 — sign and install (Xcode, ~2 minutes)

1. `open MontyGame-Unity/MontyGame/Build/iOS/Unity-iPhone.xcodeproj`
2. Plug the iPad into the Mac. **Unlock it**, and tap **Trust This Computer**.
3. In Xcode's left sidebar click the blue **Unity-iPhone** project ▸ **Signing &
   Capabilities** tab ▸ target **Unity-iPhone**:
   - tick **Automatically manage signing**
   - **Team:** pick your Apple ID (it'll say *(Personal Team)*)
   - if it complains the bundle id is taken, change it to something unique, e.g.
     `com.<yourname>.montygame`
4. Top toolbar: set the run destination to **the iPad** (not a simulator).
5. Press **▶ Run**.

**First run on the iPad will refuse to launch** with "Untrusted Developer" — that's
expected. On the iPad: **Settings ▸ General ▸ VPN & Device Management ▸ [your Apple ID] ▸
Trust**. Then tap the icon.

## Rebuilding after a code change

> 🔴 **THE ONE THAT WILL KEEP CATCHING YOU.**
> **▶ in Xcode does NOT pick up C# changes.** It only recompiles the *already-generated*
> iOS project. Unity is what turns C# into that project, so **every C# change needs Step 1
> re-run first**, then ▶. Skip it and you'll test yesterday's game and hunt a bug that
> isn't there — this happened with the win screen, which simply wasn't in the build.

So: **Step 1 (Unity, with the editor closed) → ▶ in Xcode.** Signing is remembered.

To check what's actually *in* a build before blaming the code, grep the IL2CPP string table
(literals live here — **not** in the generated C++, which is a misleading place to look):

```bash
grep -a "CONGRATULATIONS YOU WON" Build/iOS/Data/Managed/Metadata/global-metadata.dat
```

## Debugging on the device

Launch the installed app and stream its Unity logs — this is how the two device-only bugs
below were found:

```bash
xcrun devicectl list devices                       # get the iPad's UDID
xcrun devicectl device process launch --device <UDID> --console com.justinjames.montygame
```

Release IL2CPP builds print `NullReferenceException` with **no stack trace**. To get one,
build with `-devBuild` appended to the Unity command in Step 1.

---

## Things that will bite you

| Symptom | Cause | Fix |
| --- | --- | --- |
| **Code change didn't take effect** | Pressed ▶ in Xcode without re-running the Unity build | Re-run Step 1 first — see above |
| Batch build "succeeds" instantly, no `Build/iOS` | Unity Editor was open, holding the project lock | Quit Unity, re-run |
| **No UI at all on device** (no buttons, no player select) — worked fine in the editor | **Engine-code stripping removed the IMGUI module**, so `GUI.skin` is null on device and `EnsureStyles()` threw every frame. The editor never strips, so it's invisible until you run on hardware. | `IosBuild.cs` sets `stripEngineCode = false` + Minimal managed stripping. Don't re-enable them: this game is 100% IMGUI. |
| **No sound on device**, fine on the Mac | Unity's default iOS audio session is `SoloAmbient`, which iOS **silences when the device is muted** | `Assets/Plugins/iOS/MontyAudioSession.mm` sets the session to `Playback`, which plays even when muted |
| **The die "swirls"/orbits instead of tumbling** | `GUIUtility.RotateAroundPivot` under a scaled `GUI.matrix` rotates about the wrong point (see `UNITY_SHELL.md`) | Use `SetRotatedMatrix()` |
| App vanishes / won't open after a week | Free Apple ID signing expires after 7 days | Reinstall from Xcode (press ▶ again) |
| "Untrusted Developer" on launch | Personal-team apps need manual trust | Settings ▸ General ▸ VPN & Device Management ▸ Trust |
| Xcode: "no account for team" | Apple ID not signed into Xcode | Xcode ▸ Settings ▸ Accounts ▸ + |
| "iPad not available, pairing in progress" | The trust dialog is waiting on the iPad | Unlock the iPad, tap **Trust This Computer** |
| Xcode wants to download iOS platform support | Xcode ships without the iOS SDK | Let it (several GB, one-off). Separate from Unity's iOS module. |
| UI tiny or huge on device | `DesignHeight` in `GameController.cs` is the single scale dial | Raise it to shrink the UI, lower it to grow it |
| Empty app / black screen | The project's build-settings scene list is empty; `IosBuild.cs` names the scene explicitly | Don't remove that line |

## Still worth doing

- **App icon.** It installs with Unity's default grey logo — Xcode warns about the missing
  1024×1024 icon. Monty's own Cat drawing would make a lovely home-screen icon.
- **Performance.** Fine in play, but the credits and win screens draw 90 confetti quads +
  10 balloons + 3 Hulks through IMGUI every frame — the heaviest thing in the game.
