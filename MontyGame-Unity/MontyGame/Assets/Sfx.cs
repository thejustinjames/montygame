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
    static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
    static System.Random rng = new System.Random(1);

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
        Ensure();
        if (!cache.TryGetValue(name, out var clip)) { clip = Build(name); cache[name] = clip; }
        if (clip != null) src.PlayOneShot(clip, volume);
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
