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
        public int square = 0;   // 0 = off the board, waiting to roll on
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
    private Vector2 diePos;         // die screen position while tumbling
    private float dieScale = 1f;    // grows at bounce apex (fake 3D)
    private float dieHeight = 0f;   // 0 = on ground, 1 = bounce apex
    private float dieGroundY = 0f;  // resting baseline for the shadow
    private float dieSquash = 0f;   // +squash (wide/short), -stretch (tall/thin)

    // --- cinematic camera ---
    private Camera cam;
    private Transform followTarget;
    private bool zoomed = false;
    const float BoardSize = 6.2f;   // whole-board view (matches bootstrap)
    const float ZoomSize = 2.8f;    // close-up on the active player
    const float CamLerp = 1.8f;     // smoothing speed (lower = slower, gentler zoom)

    private GUIStyle labelStyle, bigStyle, buttonStyle, boxStyle, turnStyle, dieBodyStyle, dieNumStyle;
    private Texture2D dieBodyTex, pipTex, shadowTex;
    private Texture2D[] dieFaces; // realistic die images die_1..die_6

    // --- character select gallery ---
    private bool selecting = true;      // true until both players have picked
    private bool confirmingReset = false; // showing the "start a new game?" prompt
    private int picking = 0;            // which player is currently choosing
    private readonly int[] chosen = { -1, -1 }; // avatar index each player picked
    private const int AvatarCount = 6;
    private Texture2D[] avatarTex;      // avatar_1..avatar_6
    private static readonly string[] AvatarNames =
        { "Dino", "Cat", "Rex", "Robo", "Rocket", "Star" };

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

        // load avatar gallery images and hide tokens until both players pick
        avatarTex = new Texture2D[AvatarCount + 1];
        for (int i = 1; i <= AvatarCount; i++) avatarTex[i] = Resources.Load<Texture2D>($"avatar_{i}");
        foreach (var p in players)
        {
            var sr = p.token.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }
        selecting = true;
        picking = 0;
        message = "Player 1: choose your character";
    }

    // Give a player the avatar they picked: sets name, token sprite + scale
    void AssignAvatar(Player p, int avatarIndex)
    {
        p.name = AvatarNames[avatarIndex - 1];
        var tex = avatarTex[avatarIndex];
        var sr = p.token.GetComponent<SpriteRenderer>();
        if (sr == null || tex == null) return;
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                  new Vector2(0.5f, 0.5f), tex.width);
        sr.enabled = true;
        float target = BoardLayout.Cell * 0.55f;
        float w = sr.sprite.bounds.size.x;
        if (w > 0.001f) p.token.localScale = Vector3.one * (target / w);
    }

    void PickAvatar(int avatarIndex)
    {
        if (avatarIndex == chosen[0]) return; // can't pick the same as player 1
        chosen[picking] = avatarIndex;
        AssignAvatar(players[picking], avatarIndex);
        if (picking == 0)
        {
            picking = 1;
            message = "Player 2: choose a different character";
        }
        else
        {
            selecting = false; // both picked -> start the game
            current = 0;
            message = $"{players[0].name} goes first — press ROLL!";
        }
    }

    void PlaceToken(Player p)
    {
        p.token.position = WorldForSquare(p.square) + p.offset;
    }

    // World position for a square; squares < 1 sit just off the board below the start
    Vector3 WorldForSquare(int sq)
    {
        if (sq < 1) return BoardLayout.SquareToWorld(1) + new Vector3(0f, -1.25f, 0f);
        return BoardLayout.SquareToWorld(sq);
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

        // 1) Throw the die: it tumbles + bounces across the screen, then settles
        rolling = true;
        message = $"{p.name} is rolling...";
        float rollTime = 2.8f;
        float cy = Screen.height * 0.42f;
        dieGroundY = cy;
        float baseSize = Mathf.Min(Screen.width, Screen.height) * 0.28f;
        float margin = baseSize * 0.7f;
        float L = margin, Rw = Screen.width - margin;
        float startOff = (float)rng.NextDouble() * (Rw - L);
        float spinDir = rng.Next(0, 2) == 0 ? 1f : -1f;
        float bounces = 4f;                                   // more, further bounces
        float elapsed = 0f, step = 0.10f, acc = 0f;
        while (elapsed < rollTime)
        {
            float dt = Time.deltaTime;
            elapsed += dt; acc += dt;
            float prog = elapsed / rollTime;
            float ease = 1f - Mathf.Pow(1f - prog, 2f);       // ease-out travel

            // horizontal: zip across, bouncing off the walls (ping-pong), then settle center
            float travel = (Rw - L) * 2.6f * ease;            // travels much further
            float pingX = L + Mathf.PingPong(startOff + spinDir * travel, Rw - L);
            float settle = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.78f, 1f, prog));
            float x = Mathf.Lerp(pingX, Screen.width * 0.5f, settle);

            // vertical bounces, decaying to the ground
            float h = Mathf.Abs(Mathf.Sin(prog * Mathf.PI * bounces)) * (1f - prog);
            dieHeight = h;
            diePos = new Vector2(x, cy - h * Screen.height * 0.22f);

            dieScale = 1f + h * 0.30f;                          // bigger up high = 3D
            // squash on landing (h~0), slight stretch when rising
            dieSquash = Mathf.Clamp01(1f - h * 5f) * 0.5f - Mathf.Clamp01(h) * 0.12f;
            dieAngle += spinDir * 1000f * dt * (0.3f + (1f - prog)); // spin, slowing

            if (acc >= step)
            {
                acc = 0f;
                diceFace = rng.Next(1, 7);
                step += 0.02f;
            }
            yield return null;
        }

        // 2) Settle flat and centered on the rolled face for a good beat
        diceFace = roll;
        dieAngle = 0f;
        dieScale = 1f;
        dieHeight = 0f;
        dieSquash = 0f;
        diePos = new Vector2(Screen.width * 0.5f, cy);
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
            bool hulk = BoardLayout.HulkLadders.Contains(p.square);
            message = hulk ? $"The HULK carries {p.name} up!" : $"A spaceship flies {p.name} up!";
            yield return new WaitForSeconds(0.4f);
            yield return CarryTo(p, up, BoardLayout.LadderMarker(p.square));
            p.square = up;
            TryCollect(p, p.square);
        }
        else if (BoardLayout.Snakes.TryGetValue(p.square, out int down))
        {
            bool trex = BoardLayout.TrexSnakes.Contains(p.square);
            message = trex ? $"A T-REX drags {p.name} down!" : $"A vortex sucks {p.name} down!";
            yield return new WaitForSeconds(0.4f);
            yield return CarryTo(p, down, BoardLayout.SnakeMarker(p.square));
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

    // Carry the player to a square with a vehicle/creature riding alongside
    IEnumerator CarryTo(Player p, int toSquare, string markerTex)
    {
        GameObject carrier = null;
        var tex = Resources.Load<Texture2D>(markerTex);
        if (tex != null)
        {
            carrier = new GameObject("Carrier");
            var csr = carrier.AddComponent<SpriteRenderer>();
            csr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                       new Vector2(0.5f, 0.5f), tex.width);
            csr.sortingOrder = 6; // above the token
            float tgt = BoardLayout.Cell * 0.95f;
            float w = csr.sprite.bounds.size.x;
            if (w > 0.001f) carrier.transform.localScale = Vector3.one * (tgt / w);
        }

        Vector3 start = p.token.position;
        Vector3 end = BoardLayout.SquareToWorld(toSquare) + p.offset;
        float dur = 1.3f, t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float f = t / dur;
            Vector3 pos = Vector3.Lerp(start, end, f);
            pos.y += Mathf.Sin(f * Mathf.PI) * 0.5f; // gentle arc
            p.token.position = new Vector3(pos.x, pos.y, start.z);
            if (carrier != null)
                carrier.transform.position = new Vector3(pos.x, pos.y + 0.45f, start.z - 1f);
            yield return null;
        }
        p.token.position = end;
        if (carrier != null) Destroy(carrier);
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

        // realistic die face images (die_1..die_6 in Resources)
        dieFaces = new Texture2D[7];
        for (int i = 1; i <= 6; i++) dieFaces[i] = Resources.Load<Texture2D>($"die_{i}");
        shadowTex = Resources.Load<Texture2D>("dieshadow");
    }

    Texture2D FaceTex(int face)
    {
        if (dieFaces != null && face >= 1 && face <= 6) return dieFaces[face];
        return null;
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

    void DrawDieShadow()
    {
        if (shadowTex == null) return;
        float baseSize = Mathf.Min(Screen.width, Screen.height) * 0.28f;
        // small & dark when grounded, big & faint when high in the air
        float w = baseSize * Mathf.Lerp(1.05f, 0.55f, dieHeight);
        float hgt = w * 0.30f;
        float alpha = Mathf.Lerp(0.42f, 0.12f, dieHeight);
        var sr = new Rect(diePos.x - w / 2f, dieGroundY + baseSize * 0.48f - hgt / 2f, w, hgt);
        Color old = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, alpha);
        GUI.DrawTexture(sr, shadowTex);
        GUI.color = old;
    }

    void DrawPipDie(int face, float angle)
    {
        // draw at the current tumble position/scale, with squash-and-stretch
        float baseSize = Mathf.Min(Screen.width, Screen.height) * 0.28f;
        float size = baseSize * dieScale;
        float w = size * (1f + 0.30f * dieSquash);   // wider when squashed
        float hgt = size * (1f - 0.30f * dieSquash); // shorter when squashed
        Rect r = new Rect(diePos.x - w / 2f, diePos.y - hgt / 2f, w, hgt);
        Texture2D tex = FaceTex(face);
        Matrix4x4 old = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, r.center);
        if (tex != null)
        {
            GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit, true);
        }
        else // fallback if images didn't load
        {
            GUI.Box(r, GUIContent.none, dieBodyStyle);
            float pip = r.width * 0.16f;
            foreach (var p in Pips(face))
                GUI.DrawTexture(new Rect(r.x + p.x * r.width - pip / 2f,
                                         r.y + p.y * r.height - pip / 2f, pip, pip), pipTex);
        }
        GUI.matrix = old;
    }

    void DrawNumberPop(int face, float scale)
    {
        Rect r = DieRect();
        Vector2 ctr = r.center;
        var big = new Rect(ctr.x - r.width * scale / 2f, ctr.y - r.height * scale / 2f,
                           r.width * scale, r.height * scale);
        Texture2D tex = FaceTex(face);
        if (tex != null)
            GUI.DrawTexture(big, tex, ScaleMode.ScaleToFit, true);
        else
        {
            GUI.Box(big, GUIContent.none, dieBodyStyle);
            GUI.Label(big, face.ToString(), dieNumStyle);
        }
    }

    void OnGUI()
    {
        EnsureStyles();
        if (players == null) return;

        if (selecting) { DrawGallery(); DrawSystemButtons(); return; }

        GUI.Box(new Rect(15, 15, 380, 150), GUIContent.none, boxStyle);

        // per-player status
        for (int i = 0; i < players.Length; i++)
        {
            var p = players[i];
            turnStyle.normal.textColor = p.color;
            string arrow = (i == current && !won) ? "▶ " : "   ";
            string where = p.square < 1 ? "start" : $"square {p.square}";
            GUI.Label(new Rect(28, 22 + i * 28, 360, 28),
                      $"{arrow}{p.name}:  {where}   ⭐ {p.stars}", turnStyle);
        }

        GUI.Label(new Rect(28, 88, 360, 50), message, labelStyle);

        if (!busy && !won && !confirmingReset)
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

        // drawn last so it's on top: shadow + tumbling die, then the number pop
        if (rolling) { DrawDieShadow(); DrawPipDie(diceFace, dieAngle); }
        else if (popping) DrawNumberPop(diceFace, popScale);

        DrawSystemButtons();
    }

    void DrawSystemButtons()
    {
        float w = 150f, h = 44f, m = 15f;
        var newRect = new Rect(Screen.width - w - m, m, w, h);
        var quitRect = new Rect(Screen.width - w - m, m + h + 8f, w, h);
        if (!confirmingReset && GUI.Button(newRect, "NEW GAME", buttonStyle)) confirmingReset = true;
        if (!confirmingReset && GUI.Button(quitRect, "QUIT", buttonStyle)) QuitGame();
        DrawConfirm();
    }

    void DrawConfirm()
    {
        if (!confirmingReset) return;

        Color oc = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.65f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = oc;

        float w = 470f, h = 220f;
        var box = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
        GUI.Box(box, GUIContent.none, boxStyle);

        var t = new GUIStyle(GUI.skin.label)
        { fontSize = 26, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        t.normal.textColor = Color.white;
        GUI.Label(new Rect(box.x, box.y + 30, w, 40), "Start a new game?", t);

        float bw = 175f, bh = 58f, gap = 28f;
        var yes = new Rect(box.center.x - bw - gap / 2f, box.yMax - 86f, bw, bh);
        var no = new Rect(box.center.x + gap / 2f, box.yMax - 86f, bw, bh);
        if (GUI.Button(yes, "YES", buttonStyle)) { confirmingReset = false; ResetGame(); }
        if (GUI.Button(no, "NO", buttonStyle)) { confirmingReset = false; }
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void DrawGallery()
    {
        // dim the board behind the gallery
        Color oldc = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = oldc;

        var title = new GUIStyle(GUI.skin.label)
        { fontSize = 34, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        title.normal.textColor = (picking == 0) ? new Color(1f, 0.85f, 0.2f) : new Color(0.3f, 0.85f, 0.9f);
        GUI.Label(new Rect(0, Screen.height * 0.08f, Screen.width, 50),
                  $"Player {picking + 1}: choose your character", title);

        var nameStyle = new GUIStyle(GUI.skin.label)
        { fontSize = 18, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        nameStyle.normal.textColor = Color.white;

        // 3 x 2 grid of avatars
        int cols = 3, rows = 2;
        float cell = Mathf.Min(Screen.width / 4.2f, Screen.height / 3.4f);
        float gap = cell * 0.28f;
        float gridW = cols * cell + (cols - 1) * gap;
        float gridH = rows * cell + (rows - 1) * gap;
        float x0 = (Screen.width - gridW) / 2f;
        float y0 = Screen.height * 0.22f;

        for (int i = 1; i <= AvatarCount; i++)
        {
            int idx = i - 1;
            float cx = x0 + (idx % cols) * (cell + gap);
            float cy = y0 + (idx / cols) * (cell + gap);
            var r = new Rect(cx, cy, cell, cell);

            bool takenByP1 = (chosen[0] == i);
            GUI.enabled = !takenByP1;

            // tile background
            GUI.color = takenByP1 ? new Color(0.3f, 0.3f, 0.3f, 0.8f) : new Color(1f, 1f, 1f, 0.10f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;

            if (avatarTex[i] != null)
            {
                var pad = cell * 0.12f;
                GUI.DrawTexture(new Rect(r.x + pad, r.y + pad, cell - 2 * pad, cell - 2 * pad),
                                avatarTex[i], ScaleMode.ScaleToFit, true);
            }
            GUI.Label(new Rect(r.x, r.yMax - 26, cell, 24),
                      takenByP1 ? "TAKEN" : AvatarNames[idx], nameStyle);

            if (GUI.Button(r, GUIContent.none, GUIStyle.none) && !takenByP1 && !confirmingReset)
                PickAvatar(i);

            GUI.enabled = true;
        }
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
        confirmingReset = false;
        // back to character select for a fresh game
        selecting = true;
        picking = 0;
        chosen[0] = chosen[1] = -1;
        foreach (var p in players)
        {
            var sr = p.token.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }
        message = "Player 1: choose your character";
        starsTaken.Clear();
        foreach (var p in players) { p.square = 0; p.stars = 0; PlaceToken(p); }
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
