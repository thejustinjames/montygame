using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tiny procedural sound-effects engine — generates simple tones/sweeps in code
/// (no audio files needed) and plays them. Call Sfx.Play("coin"), etc.
/// </summary>
public static class Sfx
{
    const int SR = 44100;
    static AudioSource src;
    static AudioSource musicSrc;
    static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
    static System.Random rng = new System.Random(1);

    public static bool FxOn = true;
    public static bool MusicOn = true;

    static void Ensure()
    {
        if (src != null) return;
        var go = new GameObject("_Sfx");
        Object.DontDestroyOnLoad(go);
        src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0f;   // 2D, no distance attenuation
        src.volume = 1f;
        // Sounds are silent without an AudioListener — add one if the scene lacks it
        if (Object.FindFirstObjectByType<AudioListener>() == null)
            go.AddComponent<AudioListener>();
    }

    public static void Play(string name, float volume = 0.6f)
    {
        if (!FxOn) return;
        Ensure();
        if (!cache.TryGetValue(name, out var clip)) { clip = Build(name); cache[name] = clip; }
        if (clip != null) src.PlayOneShot(clip, volume);
    }

    // ---- background music (procedural stomp-stomp-clap beat) ----
    public static void StartMusic()
    {
        Ensure();
        if (musicSrc == null)
        {
            var go = new GameObject("_Music");
            Object.DontDestroyOnLoad(go);
            musicSrc = go.AddComponent<AudioSource>();
            musicSrc.loop = true;
            musicSrc.playOnAwake = false;
            musicSrc.spatialBlend = 0f;
            musicSrc.volume = 0.4f;
            musicSrc.clip = BuildBeat();
        }
        if (MusicOn && !musicSrc.isPlaying) musicSrc.Play();
    }

    public static void SetMusic(bool on)
    {
        MusicOn = on;
        if (musicSrc == null) { StartMusic(); return; }
        if (on) { if (!musicSrc.isPlaying) musicSrc.Play(); }
        else musicSrc.Stop();
    }

    public static void SetFx(bool on) => FxOn = on;

    static void Kick(float[] b, float atSec)
    {
        int start = (int)(atSec * SR);
        int len = (int)(0.22f * SR);
        float phase = 0f;
        for (int i = 0; i < len && start + i < b.Length; i++)
        {
            float u = (float)i / len;
            float freq = Mathf.Lerp(165f, 45f, Mathf.Clamp01(u * 6f));
            phase += 2f * Mathf.PI * freq / SR;
            b[start + i] += Mathf.Sin(phase) * 0.95f * Mathf.Exp(-u * 9f);
        }
    }

    static void Clap(float[] b, float atSec)
    {
        for (int k = 0; k < 3; k++)
        {
            int start = (int)((atSec + k * 0.012f) * SR);
            int len = (int)(0.10f * SR);
            for (int i = 0; i < len && start + i < b.Length; i++)
            {
                float u = (float)i / len;
                b[start + i] += (float)(rng.NextDouble() * 2 - 1) * 0.45f * Mathf.Exp(-u * 16f);
            }
        }
    }

    // A single square/pulse-wave chiptune note (midi < 0 = rest)
    static void MelNote(float[] b, float atSec, float dur, int midi, float amp, float duty)
    {
        if (midi < 0) return;
        int start = (int)(atSec * SR);
        int len = (int)(dur * SR);
        float freq = 440f * Mathf.Pow(2f, (midi - 69) / 12f);
        float ph = 0f;
        for (int i = 0; i < len && start + i < b.Length; i++)
        {
            ph += freq / SR; ph -= (int)ph;                       // 0..1 phase
            float t = (float)i / SR;
            float env = Mathf.Min(1f, i / (0.008f * SR))          // attack
                      * Mathf.Min(1f, (len - i) / (0.03f * SR))   // release
                      * (0.75f + 0.25f * Mathf.Exp(-t * 2.5f));   // gentle decay
            b[start + i] += (ph < duty ? 1f : -1f) * amp * env;
        }
    }

    static void Hat(float[] b, float atSec, float amp)
    {
        int start = (int)(atSec * SR);
        int len = (int)(0.03f * SR);
        for (int i = 0; i < len && start + i < b.Length; i++)
        {
            float u = (float)i / len;
            b[start + i] += (float)(rng.NextDouble() * 2 - 1) * amp * Mathf.Exp(-u * 30f);
        }
    }

    // Long 1980s-arcade chiptune loop: pulse melody + square bass + drums.
    static AudioClip BuildBeat()
    {
        float bpm = 132f;
        float beat = 60f / bpm;
        float eighth = beat / 2f;

        // 8-bar melody, one entry per eighth note (-1 = rest)
        int[] mel = {
            72,72,79,79,84,84,79,-1,   // bar 1
            77,77,76,76,74,74,72,-1,   // bar 2
            79,79,77,77,76,76,74,-1,   // bar 3
            72,74,76,79,84,-1,-1,-1,   // bar 4
            81,81,84,84,88,88,84,-1,   // bar 5
            83,83,81,81,79,79,77,-1,   // bar 6
            84,84,83,83,81,81,79,-1,   // bar 7
            76,79,84,88,84,79,76,-1,   // bar 8
        };
        int[] bassRoots = { 48, 53, 55, 48, 45, 53, 55, 48 }; // per bar
        int bars = 8;

        float total = bars * 4 * beat;
        float[] b = new float[(int)(total * SR) + SR / 5];

        for (int i = 0; i < mel.Length; i++)
            MelNote(b, i * eighth, eighth * 0.95f, mel[i], 0.16f, 0.3f);

        for (int bar = 0; bar < bars; bar++)
        {
            int root = bassRoots[bar];
            MelNote(b, bar * 4 * beat, beat * 2f * 0.95f, root, 0.16f, 0.5f);
            MelNote(b, bar * 4 * beat + beat * 2f, beat * 2f * 0.95f, root, 0.16f, 0.5f);
            Kick(b, bar * 4 * beat);
            Kick(b, bar * 4 * beat + beat * 2f);
            for (int e = 0; e < 8; e++) Hat(b, bar * 4 * beat + e * eighth, 0.06f);
        }

        for (int i = 0; i < b.Length; i++) b[i] = Mathf.Clamp(b[i], -1f, 1f);
        var clip = AudioClip.Create("arcade", b.Length, 1, SR, false);
        clip.SetData(b, 0);
        return clip;
    }

    static float[] Buf(float seconds) => new float[Mathf.Max(1, (int)(seconds * SR))];

    // Add a tone (optionally sweeping f0->f1) with a decay or plateau envelope.
    static void Tone(float[] b, float startSec, float durSec, float f0, float f1, float amp, bool decay)
    {
        int start = (int)(startSec * SR);
        int len = (int)(durSec * SR);
        float phase = 0f;
        for (int i = 0; i < len && start + i < b.Length; i++)
        {
            float u = (float)i / len;
            float freq = Mathf.Lerp(f0, f1, u);
            phase += 2f * Mathf.PI * freq / SR;
            float env = decay ? Mathf.Exp(-u * 4f) : Mathf.Min(1f, Mathf.Min(u * 8f, (1f - u) * 8f) + 0.15f);
            b[start + i] += Mathf.Sin(phase) * amp * env;
        }
    }

    static void NoiseTick(float[] b, float durSec, float amp)
    {
        int len = (int)(durSec * SR);
        for (int i = 0; i < len && i < b.Length; i++)
        {
            float u = (float)i / len;
            b[i] += (float)(rng.NextDouble() * 2 - 1) * amp * Mathf.Exp(-u * 12f);
        }
    }

    // A monster ROAR: a low growling fundamental (+ harmonics) that swells and
    // falls, wobbled fast for the "grrr", with rasp noise on top. The signal is
    // deliberately driven past 1.0 so the clip in Make() adds a throaty grit.
    static void Roar(float[] b, float amp)
    {
        float dur = (float)b.Length / SR;
        float ph1 = 0f, ph2 = 0f, ph3 = 0f;
        for (int i = 0; i < b.Length; i++)
        {
            float t = (float)i / SR;
            float u = t / dur;

            // pitch swells up as he winds up, then sags as the roar dies
            float f = Mathf.Lerp(70f, 125f, Mathf.Sin(Mathf.Clamp01(u * 1.7f) * Mathf.PI * 0.5f));
            f *= 1f - 0.35f * Mathf.Clamp01((u - 0.55f) / 0.45f);
            f *= 1f + 0.10f * Mathf.Sin(2f * Mathf.PI * 23f * t);   // growl wobble
            f *= 1f + 0.04f * Mathf.Sin(2f * Mathf.PI * 5.5f * t);  // slower chest rumble

            ph1 += 2f * Mathf.PI * f / SR;
            ph2 += 2f * Mathf.PI * f * 2f / SR;
            ph3 += 2f * Mathf.PI * f * 3f / SR;

            float body = Mathf.Sin(ph1) + 0.6f * Mathf.Sin(ph2) + 0.35f * Mathf.Sin(ph3);
            float rasp = (float)(rng.NextDouble() * 2 - 1)
                       * 0.4f * (0.35f + 0.65f * Mathf.Abs(Mathf.Sin(ph1)));

            float env = Mathf.Min(1f, u / 0.05f)          // snarling attack
                      * Mathf.Min(1f, (1f - u) / 0.30f);  // long tail
            b[i] += (body + rasp) * 0.55f * amp * env;
        }
    }

    static AudioClip Make(string name, float[] b)
    {
        // soft clip to avoid harshness
        for (int i = 0; i < b.Length; i++) b[i] = Mathf.Clamp(b[i], -1f, 1f);
        var clip = AudioClip.Create(name, b.Length, 1, SR, false);
        clip.SetData(b, 0);
        return clip;
    }

    static AudioClip Build(string name)
    {
        float[] b;
        switch (name)
        {
            case "click": b = Buf(0.06f); Tone(b, 0, 0.06f, 620, 620, 0.3f, true); break;
            case "roll": b = Buf(0.05f); NoiseTick(b, 0.05f, 0.25f); break;
            case "coin":
                b = Buf(0.22f);
                Tone(b, 0f, 0.06f, 988, 988, 0.35f, true);
                Tone(b, 0.06f, 0.15f, 1319, 1319, 0.35f, true);
                break;
            case "up": b = Buf(0.5f); Tone(b, 0, 0.5f, 380, 1100, 0.32f, false); break;
            case "down": b = Buf(0.5f); Tone(b, 0, 0.5f, 820, 200, 0.32f, false); break;
            case "hit":
                b = Buf(0.5f);
                Tone(b, 0, 0.5f, 540, 110, 0.35f, false);
                NoiseTick(b, 0.12f, 0.2f);
                break;
            case "shield":
                b = Buf(0.25f);
                Tone(b, 0f, 0.12f, 1500, 1500, 0.3f, true);
                Tone(b, 0.08f, 0.16f, 2100, 2100, 0.25f, true);
                break;
            case "roar":            // the Hulk arrives
                b = Buf(1.5f);
                Roar(b, 1.15f);     // >1 on purpose — Make() clips it into a snarl
                break;
            case "smash":           // ...and catches somebody: roar + a heavy thud
                b = Buf(1.6f);
                Roar(b, 1.1f);
                Tone(b, 0f, 0.45f, 220, 55, 0.5f, true);
                NoiseTick(b, 0.18f, 0.35f);
                break;
            case "win":
                b = Buf(0.7f);
                float[] notes = { 523, 659, 784, 1047 };
                for (int k = 0; k < notes.Length; k++) Tone(b, k * 0.13f, 0.18f, notes[k], notes[k], 0.3f, true);
                break;
            default: b = Buf(0.05f); Tone(b, 0, 0.05f, 440, 440, 0.2f, true); break;
        }
        return Make(name, b);
    }
}
