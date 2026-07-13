# Getting MontyGame onto the iPad

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

Re-run Step 1, then press ▶ in Xcode again. Signing is remembered.

---

## Things that will bite you

| Symptom | Cause | Fix |
| --- | --- | --- |
| Batch build "succeeds" instantly, no `Build/iOS` | Unity Editor was open, holding the project lock | Quit Unity, re-run |
| App vanishes / won't open after a week | Free Apple ID signing expires after 7 days | Reinstall from Xcode |
| "Untrusted Developer" on launch | Personal-team apps need manual trust | Settings ▸ General ▸ VPN & Device Management ▸ Trust |
| Xcode: "no account for team" | Apple ID not signed into Xcode | Xcode ▸ Settings ▸ Accounts ▸ + |
| UI tiny or huge on device | `DesignHeight` in `GameController.cs` is the single scale dial | Raise it to shrink the UI, lower it to grow it |
| Empty app / black screen | The project's build-settings scene list is empty; `IosBuild.cs` names the scene explicitly | Don't remove that line |

## Not tested yet

Nobody has run this on a device. The parts most likely to need a second pass:

- **Touch.** The whole UI is IMGUI, which Unity drives from mouse events; single taps map
  to clicks and should work, but nothing here has been touched by a finger yet.
- **Audio.** All sound is generated procedurally at runtime (`Sfx.cs`) — it should behave on
  iOS, but the chiptune loop is the thing to listen for first.
- **Performance.** IL2CPP on ARM should be fine for a board game, but the credits screen
  draws 90 confetti quads + 10 balloons through IMGUI every frame, which is the heaviest
  thing in the game.
