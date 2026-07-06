using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Two-player (pass-and-play) Snakes-&-Ladders loop over the 100-square board.
/// Dino and Cat take turns: roll 1-6, walk square-to-square, take ladders up /
/// snakes down (once), grab stars (first to land takes it), first to 100 wins.
/// Draws its own ROLL button + HUD with IMGUI (no editor setup).
/// </summary>
public class GameController : MonoBehaviour
{
    class Player
    {
        public string name;
        public Transform token;
        public Vector3 offset;
        public Color color;
        public int square = 1;
        public int stars = 0;
    }

    private Player[] players;
    private int current = 0;             // whose turn
    private int lastRoll = 0;
    private bool busy = false;
    private bool won = false;
    private int winner = -1;
    private string message = "Dino goes first — press ROLL!";
    private readonly HashSet<int> starsTaken = new HashSet<int>();
    private readonly System.Random rng = new System.Random();

    // --- dice roll animation ---
    private bool rolling = false;   // tumbling pip die
    private bool popping = false;   // number pops up
    private int diceFace = 1;
    private float dieAngle = 0f;    // tumble rotation
    private float popScale = 1f;    // number pop bounce

    // --- cinematic camera ---
    private Camera cam;
    private Transform followTarget;
    private bool zoomed = false;
    const float BoardSize = 6.2f;   // whole-board view (matches bootstrap)
    const float ZoomSize = 2.8f;    // close-up on the active player
    const float CamLerp = 1.8f;     // smoothing speed (lower = slower, gentler zoom)

    private GUIStyle labelStyle, bigStyle, buttonStyle, boxStyle, turnStyle, dieBodyStyle, dieNumStyle;
    private Texture2D dieBodyTex, pipTex;

    IEnumerator Start()
    {
        yield return null; // let the board finish building

        players = new[]
        {
            new Player { name = "Dino", token = GameObject.Find("Token_0").transform,
                         offset = new Vector3(-0.20f, 0.12f, -2.0f), color = new Color(1f, 0.85f, 0.2f) },
            new Player { name = "Cat",  token = GameObject.Find("Token_1").transform,
                         offset = new Vector3(0.20f, -0.12f, -2.1f), color = new Color(0.3f, 0.85f, 0.9f) },
        };
        foreach (var p in players) PlaceToken(p);

        cam = Camera.main;
    }

    void PlaceToken(Player p)
    {
        p.token.position = BoardLayout.SquareToWorld(p.square) + p.offset;
    }

    // Random float in [a, b) — used to make hop timing feel human/irregular
    float RandRange(float a, float b) => a + (float)rng.NextDouble() * (b - a);

    void LateUpdate()
    {
        if (cam == null) return;

        // Zoomed + following the active token, or pulled back to the whole board
        Vector3 wantPos = (zoomed && followTarget != null)
            ? new Vector3(followTarget.position.x, followTarget.position.y, -10f)
            : new Vector3(0f, 0f, -10f);
        float wantSize = zoomed ? ZoomSize : BoardSize;

        float k = 1f - Mathf.Exp(-CamLerp * Time.deltaTime); // frame-rate independent
        cam.transform.position = Vector3.Lerp(cam.transform.position, wantPos, k);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, wantSize, k);
    }

    IEnumerator DoTurn(int roll)
    {
        busy = true;
        lastRoll = roll;
        Player p = players[current];

        // 1) Tumble the pip die slowly so little kids can watch each face
        rolling = true;
        message = $"{p.name} is rolling...";
        float rollTime = 2.4f;
        float elapsed = 0f, step = 0.14f, acc = 0f;
        while (elapsed < rollTime)
        {
            float dt = Time.deltaTime;
            elapsed += dt; acc += dt;
            dieAngle += 360f * dt * (1f - elapsed / rollTime); // gentle spin, slowing
            if (acc >= step)
            {
                acc = 0f;
                diceFace = rng.Next(1, 7);
                step += 0.03f; // faces change slower and slower
            }
            yield return null;
        }

        // 2) Settle flat on the rolled face for a good beat
        diceFace = roll;
        dieAngle = 0f;
        yield return new WaitForSeconds(0.9f);
        rolling = false;

        // 3) Pop the number up big and hold it long so kids can read it
        popping = true;
        message = $"{p.name} rolled a {roll}!";
        float pt = 0f;
        while (pt < 0.8f)
        {
            pt += Time.deltaTime;
            float f = pt / 0.8f;
            popScale = 1.25f - 0.25f * Mathf.Cos(Mathf.Min(f * 2f, 1f) * Mathf.PI); // overshoot then settle
            yield return null;
        }
        popScale = 1f;
        yield return new WaitForSeconds(2.2f); // long hold on the big number
        popping = false;

        // 4) Zoom in on the active player and follow them (slow, gentle)
        followTarget = p.token;
        zoomed = true;
        yield return new WaitForSeconds(1.1f);

        int target = Mathf.Min(p.square + roll, BoardLayout.Squares);

        while (p.square < target)
        {
            yield return HopTo(p, p.square + 1);
            p.square++;
            TryCollect(p, p.square);
            // tiny irregular pause, like a hand setting the piece down
            yield return new WaitForSeconds(RandRange(0.04f, 0.22f));
        }

        if (BoardLayout.Ladders.TryGetValue(p.square, out int up))
        {
            message = $"{p.name} hits a Time Portal — zoom UP!";
            yield return new WaitForSeconds(0.35f);
            yield return HopTo(p, up);
            p.square = up;
            TryCollect(p, p.square);
        }
        else if (BoardLayout.Snakes.TryGetValue(p.square, out int down))
        {
            message = $"{p.name} hits a Whirlpool — swept down!";
            yield return new WaitForSeconds(0.35f);
            yield return HopTo(p, down);
            p.square = down;
        }

        if (p.square >= BoardLayout.Boss)
        {
            message = $"{p.name} reaches the BOSS! Dodging the T-Rex...";
            yield return new WaitForSeconds(0.9f);
            won = true;
            winner = current;
            message = $"🏆 {p.name} WINS with {p.stars} stars! 🏆";
            zoomed = false; // pull back to celebrate on the full board
            busy = false;
            yield break;
        }

        // Hold on the landing spot, then zoom back out slowly for the next player
        yield return new WaitForSeconds(0.8f);
        zoomed = false;
        yield return new WaitForSeconds(1.4f);

        current = 1 - current;
        message = $"{players[current].name}'s turn — press ROLL!";
        busy = false;
    }

    void TryCollect(Player p, int sq)
    {
        if (BoardLayout.Collectibles.Contains(sq) && !starsTaken.Contains(sq))
        {
            starsTaken.Add(sq);
            p.stars++;
            var star = GameObject.Find($"Star_{sq}");
            if (star != null)
            {
                var sr = star.GetComponent<SpriteRenderer>();
                if (sr != null) sr.enabled = false;
            }
            message = $"{p.name} grabbed a star!  ({p.stars})";
        }
    }

    IEnumerator HopTo(Player p, int toSquare)
    {
        Vector3 start = p.token.position;
        Vector3 end = BoardLayout.SquareToWorld(toSquare) + p.offset;
        // random per-hop duration so movement feels like a real hand, not a machine
        float dur = RandRange(0.30f, 0.65f);
        float t = 0f;
        Vector3 baseScale = p.token.localScale;
        while (t < dur)
        {
            t += Time.deltaTime;
            float f = t / dur;
            p.token.position = Vector3.Lerp(start, end, f);
            p.token.localScale = baseScale * (1f + 0.25f * Mathf.Sin(f * Mathf.PI));
            yield return null;
        }
        p.token.position = end;
        p.token.localScale = baseScale;
    }

    void EnsureStyles()
    {
        if (labelStyle != null) return;
        labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 19, fontStyle = FontStyle.Bold };
        labelStyle.normal.textColor = Color.white;
        turnStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
        bigStyle = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold };
        bigStyle.normal.textColor = new Color(1f, 0.9f, 0.3f);
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 22, fontStyle = FontStyle.Bold };
        boxStyle = new GUIStyle(GUI.skin.box);
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, new Color(0, 0, 0, 0.6f));
        tex.Apply();
        boxStyle.normal.background = tex;

        // big white die face + dark pip number
        dieBodyTex = new Texture2D(1, 1);
        dieBodyTex.SetPixel(0, 0, new Color(0.98f, 0.98f, 0.95f, 1f));
        dieBodyTex.Apply();
        dieBodyStyle = new GUIStyle(GUI.skin.box);
        dieBodyStyle.normal.background = dieBodyTex;
        dieNumStyle = new GUIStyle(GUI.skin.label)
        { fontSize = 150, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        dieNumStyle.normal.textColor = new Color(0.15f, 0.15f, 0.2f);

        // round pip (dot) texture for real die faces
        int d = 32; pipTex = new Texture2D(d, d, TextureFormat.RGBA32, false);
        Vector2 c = new Vector2((d - 1) / 2f, (d - 1) / 2f);
        for (int y = 0; y < d; y++)
            for (int x = 0; x < d; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), c);
                bool inside = dist <= d * 0.44f;
                pipTex.SetPixel(x, y, inside ? new Color(0.13f, 0.13f, 0.2f, 1f) : new Color(0, 0, 0, 0));
            }
        pipTex.Apply();
        pipTex.filterMode = FilterMode.Bilinear;
    }

    Rect DieRect()
    {
        float size = Mathf.Min(Screen.width, Screen.height) * 0.28f;
        float cx = Screen.width * 0.5f, cy = Screen.height * 0.42f;
        return new Rect(cx - size / 2f, cy - size / 2f, size, size);
    }

    // 3x3 pip layout (fx, fy in 0..1) for each die face
    static readonly Vector2 TL = new(0.26f, 0.26f), TC = new(0.5f, 0.26f), TR = new(0.74f, 0.26f);
    static readonly Vector2 ML = new(0.26f, 0.5f), MC = new(0.5f, 0.5f), MR = new(0.74f, 0.5f);
    static readonly Vector2 BL = new(0.26f, 0.74f), BC = new(0.5f, 0.74f), BR = new(0.74f, 0.74f);

    Vector2[] Pips(int face)
    {
        switch (face)
        {
            case 1: return new[] { MC };
            case 2: return new[] { TL, BR };
            case 3: return new[] { TL, MC, BR };
            case 4: return new[] { TL, TR, BL, BR };
            case 5: return new[] { TL, TR, MC, BL, BR };
            case 6: return new[] { TL, ML, BL, TR, MR, BR };
            default: return new[] { MC };
        }
    }

    void DrawPipDie(int face, float angle)
    {
        Rect r = DieRect();
        Matrix4x4 old = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, r.center);
        GUI.Box(r, GUIContent.none, dieBodyStyle);         // white die body
        float pip = r.width * 0.16f;
        foreach (var p in Pips(face))
        {
            var pr = new Rect(r.x + p.x * r.width - pip / 2f,
                              r.y + p.y * r.height - pip / 2f, pip, pip);
            GUI.DrawTexture(pr, pipTex);
        }
        GUI.matrix = old;
    }

    void DrawNumberPop(int face, float scale)
    {
        Rect r = DieRect();
        Vector2 ctr = r.center;
        var big = new Rect(ctr.x - r.width * scale / 2f, ctr.y - r.height * scale / 2f,
                           r.width * scale, r.height * scale);
        GUI.Box(big, GUIContent.none, dieBodyStyle);
        GUI.Label(big, face.ToString(), dieNumStyle);
    }

    void OnGUI()
    {
        EnsureStyles();
        if (players == null) return;

        GUI.Box(new Rect(15, 15, 380, 150), GUIContent.none, boxStyle);

        // per-player status
        for (int i = 0; i < players.Length; i++)
        {
            var p = players[i];
            turnStyle.normal.textColor = p.color;
            string arrow = (i == current && !won) ? "▶ " : "   ";
            GUI.Label(new Rect(28, 22 + i * 28, 360, 28),
                      $"{arrow}{p.name}:  square {p.square}   ⭐ {p.stars}", turnStyle);
        }

        GUI.Label(new Rect(28, 88, 360, 50), message, labelStyle);

        if (!busy && !won)
        {
            string who = players[current].name;
            if (GUI.Button(new Rect(15, 172, 250, 55), $"{who}: ROLL  🎲", buttonStyle))
                StartCoroutine(DoTurn(rng.Next(1, 7)));
        }
        else if (won)
        {
            if (GUI.Button(new Rect(15, 172, 250, 55), "PLAY AGAIN  ▶", buttonStyle))
                ResetGame();
        }

        // drawn last so it's on top: tumbling pip die, then the number pop
        if (rolling) DrawPipDie(diceFace, dieAngle);
        else if (popping) DrawNumberPop(diceFace, popScale);
    }

    void ResetGame()
    {
        won = false;
        busy = false;
        winner = -1;
        lastRoll = 0;
        current = 0;
        zoomed = false;
        followTarget = null;
        starsTaken.Clear();
        foreach (var p in players) { p.square = 1; p.stars = 0; PlaceToken(p); }
        foreach (int n in BoardLayout.Collectibles)
        {
            var star = GameObject.Find($"Star_{n}");
            if (star != null)
            {
                var sr = star.GetComponent<SpriteRenderer>();
                if (sr != null) sr.enabled = true;
            }
        }
        message = "Dino goes first — press ROLL!";
    }
}
