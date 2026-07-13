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
        public bool pteroUsed = false; // a pterodactyl may carry each player off only once per game
    }

    // --- how many players can share the board, and where they sit in a square ---
    public const int MaxPlayers = 6;
    public const float TokenSize = BoardLayout.Cell * 0.40f;  // small enough that 6 fit in one square
    // 3 x 2 within a square, so six tokens never fully cover each other
    public static readonly Vector3[] TokenOffsets =
    {
        new Vector3(-0.26f,  0.15f, -2.0f), new Vector3(0f,  0.15f, -2.1f), new Vector3(0.26f,  0.15f, -2.2f),
        new Vector3(-0.26f, -0.17f, -2.3f), new Vector3(0f, -0.17f, -2.4f), new Vector3(0.26f, -0.17f, -2.5f),
    };
    public static readonly Color[] TokenColors =
    {
        new Color(1f, 0.85f, 0.2f),   // yellow
        new Color(0.3f, 0.85f, 0.9f), // cyan
        new Color(1f, 0.5f, 0.75f),   // pink
        new Color(0.5f, 0.95f, 0.45f),// green
        new Color(1f, 0.6f, 0.25f),   // orange
        new Color(0.75f, 0.6f, 1f),   // purple
    };

    private Player[] players;
    private Transform[] allTokens;       // one per possible player, built by GameBootstrap
    private int current = 0;             // whose turn
    private int lastRoll = 0;
    private bool busy = false;
    private bool won = false;
    private int winner = -1;
    private string message = "How many players?";
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
    private string popEquation = "";// e.g. "6 + 2 = 8" for chained rolls
    private Color dieTint = Color.white;  // the HULK rolls a green die
    static readonly Color HulkDie = new Color(0.45f, 1f, 0.45f);

    // --- dynamic diamond bonus (moves to stay ahead of the player) ---
    private Transform diamond;
    private int diamondSquare = -1;
    private int rollsSinceDiamond = 0;

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

    // --- setup screens: how many players, then everyone picks a character ---
    private bool choosingCount = true;  // true until the player count is chosen
    private bool selecting = false;     // true until every player has picked
    private bool confirmingReset = false; // showing the "start a new game?" prompt
    private int picking = 0;            // which player is currently choosing
    private int[] chosen;               // avatar index each player picked (-1 = not yet)
    private const int AvatarCount = 7;
    private Texture2D[] avatarTex;      // avatar_1..avatar_7
    private static readonly string[] AvatarNames =
        { "Cat", "(retired)", "Rex", "Robo", "Saucer", "Goldbot", "Bleep" };
    // Slot 2 was an earlier drawing of the same cat — retired, kept on disk but not offered.
    private static readonly int[] VisibleAvatars = { 1, 3, 4, 5, 6, 7 };

    // --- random flying hazards/bonuses ---
    class Flyer { public Transform tr; public SpriteRenderer sr; public Vector2 vel; public int type; public float cd; }
    private readonly System.Collections.Generic.List<Flyer> flyers = new System.Collections.Generic.List<Flyer>();
    const float FlyBound = 9f;        // roam area around the board
    const int PteroTarget = 10;       // pterodactyl grabs you back to 10
    const int ShipTarget = 90;        // spaceship zooms you up to 90
    private string bigMsg = "";       // big centered flash message
    private Color bigMsgColor = Color.white;

    // --- the HULK: drops in at 100 and stomps back DOWN the board like a
    //     third player moving in reverse. Land on him and he throws you to 50.
    private Transform hulkToken;
    private int hulkSquare = -1;                        // < 1 = not on the board
    const int HulkStart = BoardLayout.Squares;          // he drops in at 100
    const int HulkRange = 20;                           // ...and roams no more than 20 squares
    const int HulkFloor = BoardLayout.Squares - HulkRange;
    const int HulkThrowTo = 50;                         // smashed players land here
    const int HulkAppearPercent = 35;                   // chance he shows up on any given turn
    static readonly Vector3 HulkOffset = new Vector3(0f, 0.22f, -2.6f);
    static readonly Color HulkGreen = new Color(0.45f, 1f, 0.35f);

    IEnumerator Start()
    {
        yield return null; // let the board finish building

        allTokens = new Transform[MaxPlayers];
        for (int i = 0; i < MaxPlayers; i++)
        {
            var go = GameObject.Find($"Token_{i}");
            if (go != null) allTokens[i] = go.transform;
        }

        cam = Camera.main;

        avatarTex = new Texture2D[AvatarCount + 1];
        for (int i = 1; i <= AvatarCount; i++) avatarTex[i] = Resources.Load<Texture2D>($"avatar_{i}");

        HideAllTokens();
        choosingCount = true;
        selecting = false;
        message = "How many players?";

        // spawn the random flyers
        SpawnFlyer(0); SpawnFlyer(0);   // pterodactyls
        SpawnFlyer(1); SpawnFlyer(1);   // spaceships

        Sfx.StartMusic();
    }

    void SpawnFlyer(int type)
    {
        var tex = Resources.Load<Texture2D>(type == 0 ? "fly_ptero" : "fly_ship");
        var go = new GameObject(type == 0 ? "Flyer_Ptero" : "Flyer_Ship");
        var sr = go.AddComponent<SpriteRenderer>();
        if (tex != null)
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);
        sr.sortingOrder = -50; // fly behind the floating tiles

        go.transform.position = new Vector3(RandRange(-FlyBound, FlyBound), RandRange(-FlyBound, FlyBound), 1f);
        float target = type == 0 ? 1.1f : 1.3f;
        float w = (sr.sprite != null) ? sr.sprite.bounds.size.x : 1f;
        if (w > 0.001f) go.transform.localScale = Vector3.one * (target / w);

        flyers.Add(new Flyer { tr = go.transform, sr = sr, vel = RandomDir() * RandRange(1.5f, 3f), type = type });
    }

    Vector2 RandomDir()
    {
        float a = RandRange(0f, Mathf.PI * 2f);
        return new Vector2(Mathf.Cos(a), Mathf.Sin(a));
    }

    void Update()
    {
        if (flyers.Count == 0) return;

        foreach (var f in flyers)
        {
            f.tr.position += (Vector3)(f.vel * Time.deltaTime);
            Vector3 p = f.tr.position;
            bool wrapped = false;
            if (p.x > FlyBound) { p.x = -FlyBound; wrapped = true; }
            else if (p.x < -FlyBound) { p.x = FlyBound; wrapped = true; }
            if (p.y > FlyBound) { p.y = -FlyBound; wrapped = true; }
            else if (p.y < -FlyBound) { p.y = FlyBound; wrapped = true; }
            f.tr.position = p;
            if (wrapped) f.vel = RandomDir() * RandRange(1.5f, 3f); // totally random new heading
            if (f.sr != null) f.sr.flipX = f.vel.x < 0f;
            if (f.cd > 0f) f.cd -= Time.deltaTime;
        }

        // collisions only between turns (never mid-animation / dialog)
        if (players == null || choosingCount || selecting || won || busy || confirmingReset) return;
        foreach (var f in flyers)
        {
            if (f.cd > 0f) continue;
            foreach (var pl in players)
            {
                if (pl.square < 1) continue;
                if (Vector2.Distance(f.tr.position, pl.token.position) < 0.6f)
                {
                    f.cd = 5f;
                    StartCoroutine(FlyerHit(pl, f.type));
                    return; // one hit at a time
                }
            }
        }
    }

    IEnumerator FlyerHit(Player pl, int type)
    {
        busy = true;

        // Pterodactyl: a coin shields you (consumes one), otherwise back to 10 —
        // but he can only carry each player off ONCE per game.
        if (type == 0)
        {
            if (pl.pteroUsed)
            {
                bigMsg = "Too slow!"; bigMsgColor = new Color(0.6f, 0.9f, 1f);
                message = $"The pterodactyl swoops at {pl.name} — but he's had his turn. Nothing happens!";
                Sfx.Play("shield", 0.5f);
                yield return new WaitForSeconds(1.5f);
                bigMsg = "";
                busy = false;
                yield break;
            }
            if (pl.stars > 0)
            {
                pl.stars--;
                bigMsg = "Shielded!"; bigMsgColor = new Color(0.5f, 1f, 0.6f);
                message = $"{pl.name}'s coin shielded them from the pterodactyl!";
                Sfx.Play("shield");
                yield return new WaitForSeconds(1.5f);
                bigMsg = "";
                busy = false;
                yield break;
            }
            pl.pteroUsed = true;   // that's his one grab of this player
            bigMsg = "Oh no!"; bigMsgColor = new Color(1f, 0.5f, 0.45f);
            message = $"A pterodactyl grabbed {pl.name} — back to {PteroTarget}! (Only once per game.)";
            Sfx.Play("hit");
            followTarget = pl.token; zoomed = true;
            yield return new WaitForSeconds(0.7f);
            yield return CarryTo(pl, PteroTarget, "fly_ptero");
            pl.square = PteroTarget;
        }
        else // Spaceship: up to 90 — but never DOWN. Past 90 already? It just flies by.
        {
            if (pl.square >= ShipTarget)
            {
                bigMsg = "Whoosh!"; bigMsgColor = new Color(0.6f, 0.9f, 1f);
                message = $"A spaceship buzzes {pl.name} — already past {ShipTarget}, so it flies on by!";
                Sfx.Play("shield", 0.5f);
                yield return new WaitForSeconds(1.5f);
                bigMsg = "";
                busy = false;
                yield break;
            }
            bigMsg = "Being beamed up!"; bigMsgColor = new Color(0.5f, 0.9f, 1f);
            message = $"A spaceship zoomed {pl.name} up to {ShipTarget}!";
            Sfx.Play("up");
            followTarget = pl.token; zoomed = true;
            yield return new WaitForSeconds(0.7f);
            yield return CarryTo(pl, ShipTarget, "fly_ship");
            pl.square = ShipTarget;
        }

        yield return new WaitForSeconds(0.5f);
        zoomed = false;
        yield return new WaitForSeconds(0.7f);
        bigMsg = "";
        busy = false;
    }

    void HideAllTokens()
    {
        if (allTokens == null) return;
        foreach (var t in allTokens)
        {
            if (t == null) continue;
            var sr = t.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }
    }

    // The player count is locked in: build that many players and start the picking.
    void StartGameWith(int count)
    {
        count = Mathf.Clamp(count, 1, MaxPlayers);
        players = new Player[count];
        for (int i = 0; i < count; i++)
            players[i] = new Player
            {
                name = $"Player {i + 1}",
                token = allTokens[i],
                offset = TokenOffsets[i],
                color = TokenColors[i],
            };
        foreach (var p in players) PlaceToken(p);

        chosen = new int[count];
        for (int i = 0; i < count; i++) chosen[i] = -1;

        HideAllTokens();
        choosingCount = false;
        selecting = true;
        picking = 0;
        current = 0;
        message = "Player 1: choose your character";
    }

    bool AvatarTaken(int avatarIndex)
    {
        if (chosen == null) return false;
        for (int i = 0; i < chosen.Length; i++)
            if (i != picking && chosen[i] == avatarIndex) return true;
        return false;
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
        float w = sr.sprite.bounds.size.x;
        if (w > 0.001f) p.token.localScale = Vector3.one * (TokenSize / w);
    }

    void PickAvatar(int avatarIndex)
    {
        if (AvatarTaken(avatarIndex)) return;   // someone already has this one
        chosen[picking] = avatarIndex;
        AssignAvatar(players[picking], avatarIndex);

        if (picking < players.Length - 1)
        {
            picking++;
            message = $"Player {picking + 1}: choose a different character";
        }
        else
        {
            selecting = false;   // everyone has picked -> play
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

    // One die throw: tumbles + bounces across the screen, settles on `roll`.
    // `who` is whoever is throwing it — a player, or the HULK (whose die is green).
    IEnumerator RollDie(int roll, string who, bool first)
    {
        rolling = true;
        if (first) message = $"{who} is rolling...";
        float rollTime = 2.8f;
        float cy = Screen.height * 0.42f;
        dieGroundY = cy;
        float baseSize = Mathf.Min(Screen.width, Screen.height) * 0.28f;
        float margin = baseSize * 0.7f;
        float L = margin, Rw = Screen.width - margin;
        float startOff = (float)rng.NextDouble() * (Rw - L);
        float spinDir = rng.Next(0, 2) == 0 ? 1f : -1f;
        float bounces = 4f;
        float elapsed = 0f, step = 0.10f, acc = 0f;
        while (elapsed < rollTime)
        {
            float dt = Time.deltaTime;
            elapsed += dt; acc += dt;
            float prog = elapsed / rollTime;
            float ease = 1f - Mathf.Pow(1f - prog, 2f);

            float travel = (Rw - L) * 2.6f * ease;
            float pingX = L + Mathf.PingPong(startOff + spinDir * travel, Rw - L);
            float settle = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.78f, 1f, prog));
            float x = Mathf.Lerp(pingX, Screen.width * 0.5f, settle);

            float h = Mathf.Abs(Mathf.Sin(prog * Mathf.PI * bounces)) * (1f - prog);
            dieHeight = h;
            diePos = new Vector2(x, cy - h * Screen.height * 0.22f);
            dieScale = 1f + h * 0.30f;
            dieSquash = Mathf.Clamp01(1f - h * 5f) * 0.5f - Mathf.Clamp01(h) * 0.12f;
            dieAngle += spinDir * 1000f * dt * (0.3f + (1f - prog));

            if (acc >= step)
            {
                acc = 0f;
                diceFace = rng.Next(1, 7);
                step += 0.02f;
                Sfx.Play("roll", 0.35f);
            }
            yield return null;
        }

        diceFace = roll;
        dieAngle = 0f; dieScale = 1f; dieHeight = 0f; dieSquash = 0f;
        diePos = new Vector2(Screen.width * 0.5f, cy);
        yield return new WaitForSeconds(0.9f);
        rolling = false;
    }

    IEnumerator DoTurn()
    {
        busy = true;
        Player p = players[current];

        // Roll — with roll-again on a 6 (sum them: 6 + 2 = 8). No effects trigger
        // "in between" because we move once by the total.
        var rolls = new System.Collections.Generic.List<int>();
        int roll;
        do
        {
            roll = rng.Next(1, 7);
            yield return RollDie(roll, p.name, rolls.Count == 0);
            rolls.Add(roll);
            if (roll == 6 && rolls.Count < 4)
            {
                message = $"{p.name} rolled a 6 — ROLL AGAIN!";
                Sfx.Play("shield", 0.5f);
                yield return new WaitForSeconds(1.0f);
            }
        } while (roll == 6 && rolls.Count < 4);

        int total = 0;
        foreach (int r in rolls) total += r;
        lastRoll = total;

        // Show the result big (an equation like "6 + 2 = 8" when chained)
        popEquation = rolls.Count > 1 ? string.Join(" + ", rolls) + " = " + total : "";
        popping = true;
        message = rolls.Count > 1 ? $"{p.name} rolled {popEquation}!" : $"{p.name} rolled a {total}!";
        float pt = 0f;
        while (pt < 0.8f)
        {
            pt += Time.deltaTime;
            float f = pt / 0.8f;
            popScale = 1.25f - 0.25f * Mathf.Cos(Mathf.Min(f * 2f, 1f) * Mathf.PI);
            yield return null;
        }
        popScale = 1f;
        yield return new WaitForSeconds(2.2f);
        popping = false;
        popEquation = "";

        // Zoom in and move by the TOTAL (effects only apply on the final landing)
        followTarget = p.token;
        zoomed = true;
        yield return new WaitForSeconds(1.1f);

        int target = Mathf.Min(p.square + total, BoardLayout.Squares);
        while (p.square < target)
        {
            yield return HopTo(p, p.square + 1);
            p.square++;
            yield return new WaitForSeconds(RandRange(0.04f, 0.22f));
        }
        TryCollect(p, p.square);   // coin only on the square you LAND on
        TryDiamond(p);             // diamond if you land on it

        if (BoardLayout.Ladders.TryGetValue(p.square, out int up))
        {
            bool hulk = BoardLayout.HulkLadders.Contains(p.square);
            message = hulk ? $"The HULK carries {p.name} up!" : $"A spaceship flies {p.name} up!";
            Sfx.Play("up");
            yield return new WaitForSeconds(0.4f);
            yield return CarryTo(p, up, BoardLayout.LadderMarker(p.square));
            p.square = up;
            TryCollect(p, p.square);
        }
        else if (BoardLayout.Snakes.TryGetValue(p.square, out int down))
        {
            bool trex = BoardLayout.TrexSnakes.Contains(p.square);
            message = trex ? $"A T-REX drags {p.name} down!" : $"A vortex sucks {p.name} down!";
            Sfx.Play("down");
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
            message = $"🏆 {p.name} WINS with {p.stars} coins! 🏆";
            Sfx.Play("win", 0.8f);
            zoomed = false; // pull back to celebrate on the full board
            busy = false;
            yield break;
        }

        // Hold on the landing spot, then zoom back out slowly for the next player
        yield return new WaitForSeconds(0.8f);
        zoomed = false;
        yield return new WaitForSeconds(1.4f);

        yield return HulkTurn();   // the Hulk takes his (reverse) turn

        current = (current + 1) % players.Length;

        // every few rolls the diamond (re)appears ahead of the next player
        rollsSinceDiamond += rolls.Count;
        if (diamondSquare < 0 || rollsSinceDiamond >= 3) { MoveDiamondAhead(); rollsSinceDiamond = 0; }

        message = $"{players[current].name}'s turn — press ROLL!";
        busy = false;
    }

    // The HULK's turn. He appears at 100, then each turn rolls 1-6 and stomps
    // that many squares DOWN the board (the reverse of a player). He never goes
    // below HulkFloor — if his roll would take him past it he leaves the board.
    IEnumerator HulkTurn()
    {
        EnsureHulk();

        if (hulkSquare < 1)
        {
            if (rng.Next(0, 100) >= HulkAppearPercent) yield break;

            hulkSquare = HulkStart;
            hulkToken.gameObject.SetActive(true);
            hulkToken.position = BoardLayout.SquareToWorld(hulkSquare) + HulkOffset;
            bigMsg = "HULK SMASH!"; bigMsgColor = HulkGreen;
            message = $"The HULK lands on {HulkStart} — and he's hunting the nearest player!";
            Sfx.Play("roar", 0.9f);
            followTarget = hulkToken; zoomed = true;
            yield return new WaitForSeconds(1.8f);
            bigMsg = "";
            zoomed = false;
            yield return new WaitForSeconds(1.0f);
            yield break;
        }

        // He rolls his own die — green — then hunts the nearest player with it.
        int steps = rng.Next(1, 7);
        dieTint = HulkDie;
        followTarget = hulkToken; zoomed = true;
        yield return RollDie(steps, "The HULK", true);
        dieTint = Color.white;

        Player prey = NearestPlayer();
        int dir = (prey != null && prey.square > hulkSquare) ? +1 : -1;  // chase up or down the board
        string way = dir > 0 ? "UP" : "DOWN";

        bigMsg = $"HULK MOVES {steps}!"; bigMsgColor = HulkGreen;
        message = prey != null
            ? $"The HULK stomps {way} {steps} squares, hunting {prey.name}!"
            : $"The HULK stomps {way} {steps} squares!";
        Sfx.Play("down");
        yield return new WaitForSeconds(1.3f);
        bigMsg = "";

        int target = hulkSquare + dir * steps;
        // he prowls the top of the board only — past either end and he loses interest
        if (target < HulkFloor || target > BoardLayout.Squares)
        {
            hulkSquare = -1;
            hulkToken.gameObject.SetActive(false);
            message = "The HULK gets bored and thumps away. Phew!";
            yield return new WaitForSeconds(1.2f);
            zoomed = false;
            yield return new WaitForSeconds(1.0f);
            yield break;
        }

        while (hulkSquare != target)
        {
            yield return HulkHopTo(hulkSquare + dir);
            hulkSquare += dir;
            yield return new WaitForSeconds(RandRange(0.04f, 0.18f));
        }

        // Landed on a player? SMASH — thrown all the way back to 50.
        foreach (var pl in players)
        {
            if (pl.square != hulkSquare) continue;
            bigMsg = "HULK SMASH!"; bigMsgColor = new Color(1f, 0.5f, 0.45f);
            message = $"The HULK caught {pl.name} — thrown back to {HulkThrowTo}!";
            Sfx.Play("smash", 0.95f);
            yield return new WaitForSeconds(0.9f);
            followTarget = pl.token;
            yield return CarryTo(pl, HulkThrowTo, "ladder_hulk");
            pl.square = HulkThrowTo;
            bigMsg = "";
            break;
        }

        yield return new WaitForSeconds(0.7f);
        zoomed = false;
        yield return new WaitForSeconds(1.0f);
    }

    // Whoever is closest to him along the board — that's who he goes after.
    Player NearestPlayer()
    {
        Player best = null;
        int bestDist = int.MaxValue;
        foreach (var pl in players)
        {
            if (pl.square < 1) continue;   // still waiting to get on the board
            int d = Mathf.Abs(pl.square - hulkSquare);
            if (d < bestDist) { bestDist = d; best = pl; }
        }
        return best;
    }

    void EnsureHulk()
    {
        if (hulkToken != null) return;
        var go = new GameObject("Hulk");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 7;   // he towers over the player tokens
        var tex = Resources.Load<Texture2D>("ladder_hulk");
        if (tex != null)
        {
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                      new Vector2(0.5f, 0.5f), tex.width);
            float target = BoardLayout.Cell * 0.85f;
            float w = sr.sprite.bounds.size.x;
            if (w > 0.001f) go.transform.localScale = Vector3.one * (target / w);
        }
        go.SetActive(false);
        hulkToken = go.transform;
    }

    IEnumerator HulkHopTo(int toSquare)
    {
        Vector3 start = hulkToken.position;
        Vector3 end = BoardLayout.SquareToWorld(toSquare) + HulkOffset;
        float dur = RandRange(0.28f, 0.50f), t = 0f;
        Vector3 baseScale = hulkToken.localScale;
        while (t < dur)
        {
            t += Time.deltaTime;
            float f = t / dur;
            Vector3 pos = Vector3.Lerp(start, end, f);
            pos.y += Mathf.Sin(f * Mathf.PI) * 0.25f;   // heavy stomping arc
            hulkToken.position = new Vector3(pos.x, pos.y, start.z);
            hulkToken.localScale = baseScale * (1f + 0.18f * Mathf.Sin(f * Mathf.PI));
            yield return null;
        }
        hulkToken.position = end;
        hulkToken.localScale = baseScale;
    }

    // Diamond bonus: same effect as a coin, but it hops ahead of the player.
    void MoveDiamondAhead()
    {
        int baseSq = Mathf.Max(0, players[current].square);
        int sq = -1;
        for (int tries = 0; tries < 25; tries++)
        {
            int cand = Mathf.Min(baseSq + rng.Next(3, 13), 99);
            if (cand < 2) continue;
            if (BoardLayout.Ladders.ContainsKey(cand) || BoardLayout.Snakes.ContainsKey(cand)
                || BoardLayout.Collectibles.Contains(cand)) continue;
            sq = cand; break;
        }
        if (sq < 0) return;
        diamondSquare = sq;
        EnsureDiamond();
        // set position while inactive so the Bobber anchors to the new spot on enable
        diamond.gameObject.SetActive(false);
        diamond.position = BoardLayout.SquareToWorld(sq) + new Vector3(-0.22f, 0.22f, -1.2f);
        diamond.gameObject.SetActive(true);
    }

    void EnsureDiamond()
    {
        if (diamond != null) return;
        var tex = Resources.Load<Texture2D>("diamond");
        var go = new GameObject("Diamond");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 4;
        if (tex != null)
        {
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);
            float target = BoardLayout.Cell * 0.4f;
            float w = sr.sprite.bounds.size.x;
            if (w > 0.001f) go.transform.localScale = Vector3.one * (target / w);
        }
        go.AddComponent<Bobber>();
        diamond = go.transform;
    }

    void TryDiamond(Player p)
    {
        if (diamondSquare > 0 && p.square == diamondSquare)
        {
            p.stars++;
            Sfx.Play("coin");
            message = $"{p.name} grabbed a DIAMOND! It shields against pterodactyls. ({p.stars})";
            if (diamond != null) diamond.gameObject.SetActive(false);
            diamondSquare = -1;
        }
    }

    void TryCollect(Player p, int sq)
    {
        if (BoardLayout.Collectibles.Contains(sq) && !starsTaken.Contains(sq))
        {
            starsTaken.Add(sq);
            p.stars++;
            Sfx.Play("coin");
            var star = GameObject.Find($"Star_{sq}");
            if (star != null)
            {
                var sr = star.GetComponent<SpriteRenderer>();
                if (sr != null) sr.enabled = false;
            }
            message = $"{p.name} grabbed a coin! It shields against pterodactyls. ({p.stars})";
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
        Color oldColor = GUI.color;
        Color oldBg = GUI.backgroundColor;
        GUI.color = dieTint;             // green while the HULK is rolling
        GUI.backgroundColor = dieTint;
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
        GUI.color = oldColor;
        GUI.backgroundColor = oldBg;
    }

    void DrawNumberPop(int face, float scale)
    {
        Rect r = DieRect();
        Vector2 ctr = r.center;

        // chained roll -> show the equation "6 + 2 = 8" on a wide plaque
        if (!string.IsNullOrEmpty(popEquation))
        {
            float w = Screen.width * 0.55f * scale, h = r.height * 1.1f * scale;
            var plaque = new Rect(ctr.x - w / 2f, ctr.y - h / 2f, w, h);
            GUI.Box(plaque, GUIContent.none, boxStyle);
            var st = new GUIStyle(GUI.skin.label)
            { fontSize = 70, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            st.normal.textColor = Color.white;
            GUI.Label(plaque, popEquation, st);
            return;
        }

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

        if (choosingCount) { DrawPlayerCount(); DrawSystemButtons(); return; }
        if (players == null) return;
        if (selecting) { DrawGallery(); DrawSystemButtons(); return; }

        // the status panel grows with the number of players
        const float rowH = 28f;
        float listH = players.Length * rowH;
        float panelH = listH + 66f;
        GUI.Box(new Rect(15, 15, 380, panelH), GUIContent.none, boxStyle);

        // per-player status
        for (int i = 0; i < players.Length; i++)
        {
            var p = players[i];
            turnStyle.normal.textColor = p.color;
            string arrow = (i == current && !won) ? "▶ " : "   ";
            string where = p.square < 1 ? "start" : $"square {p.square}";
            GUI.Label(new Rect(28, 22 + i * rowH, 360, rowH),
                      $"{arrow}{p.name}:  {where}   coins {p.stars}", turnStyle);
        }

        GUI.Label(new Rect(28, 26 + listH, 360, 50), message, labelStyle);

        if (!won) { DrawTurnBanner(); DrawTurnArrow(); }

        float btnY = 15 + panelH + 10;
        if (!busy && !won && !confirmingReset)
        {
            var cp = players[current];
            Color oldbg = GUI.backgroundColor;
            GUI.backgroundColor = cp.color;   // ROLL button in the current player's colour
            if (GUI.Button(new Rect(15, btnY, 300, 62), $"{cp.name.ToUpper()}: ROLL  🎲", buttonStyle))
            {
                Sfx.Play("click");
                StartCoroutine(DoTurn());
            }
            GUI.backgroundColor = oldbg;
        }
        else if (won)
        {
            if (GUI.Button(new Rect(15, btnY, 250, 55), "PLAY AGAIN  ▶", buttonStyle))
                ResetGame();
        }

        // drawn last so it's on top: shadow + tumbling die, then the number pop
        if (rolling) { DrawDieShadow(); DrawPipDie(diceFace, dieAngle); }
        else if (popping) DrawNumberPop(diceFace, popScale);

        if (!string.IsNullOrEmpty(bigMsg)) DrawBigMsg();

        DrawSystemButtons();
    }

    Texture2D CurrentAvatar()
    {
        int idx = chosen[current];
        if (avatarTex != null && idx >= 1 && idx <= AvatarCount) return avatarTex[idx];
        return null;
    }

    void DrawTurnBanner()
    {
        var p = players[current];
        float bw = 500f, bh = 82f;
        var box = new Rect((Screen.width - bw) / 2f, 12f, bw, bh);

        Color oc = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(box, Texture2D.whiteTexture);
        GUI.color = new Color(p.color.r, p.color.g, p.color.b, 1f);
        // coloured underline bar
        GUI.DrawTexture(new Rect(box.x, box.yMax - 6, bw, 6), Texture2D.whiteTexture);
        GUI.color = oc;

        var av = CurrentAvatar();
        if (av != null) GUI.DrawTexture(new Rect(box.x + 12, box.y + 9, 64, 64), av, ScaleMode.ScaleToFit, true);

        var st = new GUIStyle(GUI.skin.label)
        { fontSize = 34, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
        st.normal.textColor = p.color;
        GUI.Label(new Rect(box.x + 88, box.y, bw - 96, bh), $"{p.name.ToUpper()}'S TURN", st);
    }

    void DrawTurnArrow()
    {
        if (cam == null) return;
        var p = players[current];
        Vector3 sp = cam.WorldToScreenPoint(p.token.position);
        if (sp.z < 0) return;
        float bob = Mathf.Sin(Time.time * 4f) * 8f;
        float gy = Screen.height - sp.y;
        var st = new GUIStyle(GUI.skin.label)
        { fontSize = 46, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        st.normal.textColor = p.color;
        GUI.Label(new Rect(sp.x - 34f, gy - 96f - bob, 68f, 52f), "▼", st);
    }

    void DrawBigMsg()
    {
        var st = new GUIStyle(GUI.skin.label)
        { fontSize = 72, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        var r = new Rect(0, Screen.height * 0.60f, Screen.width, 110);
        // drop shadow for readability over the busy board
        st.normal.textColor = new Color(0, 0, 0, 0.7f);
        GUI.Label(new Rect(r.x + 4, r.y + 4, r.width, r.height), bigMsg, st);
        st.normal.textColor = bigMsgColor;
        GUI.Label(r, bigMsg, st);
    }

    void DrawSystemButtons()
    {
        float w = 150f, h = 44f, m = 15f;
        var newRect = new Rect(Screen.width - w - m, m, w, h);
        var quitRect = new Rect(Screen.width - w - m, m + h + 8f, w, h);
        var musicRect = new Rect(Screen.width - w - m, m + 2 * (h + 8f), w, h);
        var sfxRect = new Rect(Screen.width - w - m, m + 3 * (h + 8f), w, h);
        if (!confirmingReset && GUI.Button(newRect, "NEW GAME", buttonStyle)) confirmingReset = true;
        if (!confirmingReset && GUI.Button(quitRect, "QUIT", buttonStyle)) QuitGame();
        if (!confirmingReset && GUI.Button(musicRect, $"Music: {(Sfx.MusicOn ? "On" : "Off")}", buttonStyle))
            Sfx.SetMusic(!Sfx.MusicOn);
        if (!confirmingReset && GUI.Button(sfxRect, $"SFX: {(Sfx.FxOn ? "On" : "Off")}", buttonStyle))
            Sfx.SetFx(!Sfx.FxOn);
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

        // grid of the avatars on offer, at most 4 per row
        int count = VisibleAvatars.Length;
        int cols = Mathf.Min(4, count);
        int rows = Mathf.CeilToInt(count / (float)cols);
        float cell = Mathf.Min(Screen.width / (cols + 1.2f), Screen.height / (rows + 1.4f));
        float gap = cell * 0.28f;
        float y0 = Screen.height * 0.22f;

        for (int n = 0; n < count; n++)
        {
            int i = VisibleAvatars[n];               // avatar slot (1..AvatarCount)
            int row = n / cols;
            int inRow = Mathf.Min(cols, count - row * cols); // last row may be short — centre it
            float rowW = inRow * cell + (inRow - 1) * gap;
            float cx = (Screen.width - rowW) / 2f + (n % cols) * (cell + gap);
            float cy = y0 + row * (cell + gap);
            var r = new Rect(cx, cy, cell, cell);

            bool taken = AvatarTaken(i);
            GUI.enabled = !taken;

            // tile background
            GUI.color = taken ? new Color(0.3f, 0.3f, 0.3f, 0.8f) : new Color(1f, 1f, 1f, 0.10f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;

            if (avatarTex[i] != null)
            {
                var pad = cell * 0.12f;
                GUI.DrawTexture(new Rect(r.x + pad, r.y + pad, cell - 2 * pad, cell - 2 * pad),
                                avatarTex[i], ScaleMode.ScaleToFit, true);
            }
            GUI.Label(new Rect(r.x, r.yMax - 26, cell, 24),
                      taken ? "TAKEN" : AvatarNames[i - 1], nameStyle);

            if (GUI.Button(r, GUIContent.none, GUIStyle.none) && !taken && !confirmingReset)
                PickAvatar(i);

            GUI.enabled = true;
        }
    }

    // First screen: how many people are playing?
    void DrawPlayerCount()
    {
        Color oldc = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = oldc;

        var title = new GUIStyle(GUI.skin.label)
        { fontSize = 40, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        title.normal.textColor = Color.white;
        GUI.Label(new Rect(0, Screen.height * 0.16f, Screen.width, 60), "How many players?", title);

        var sub = new GUIStyle(GUI.skin.label)
        { fontSize = 20, alignment = TextAnchor.MiddleCenter };
        sub.normal.textColor = new Color(1f, 1f, 1f, 0.75f);
        GUI.Label(new Rect(0, Screen.height * 0.16f + 56, Screen.width, 34),
                  "1 player = beat the board on your own", sub);

        var numStyle = new GUIStyle(GUI.skin.button)
        { fontSize = 54, fontStyle = FontStyle.Bold };

        float cell = Mathf.Min(Screen.width / 8f, Screen.height / 4.5f);
        float gap = cell * 0.25f;
        float totalW = MaxPlayers * cell + (MaxPlayers - 1) * gap;
        float x0 = (Screen.width - totalW) / 2f;
        float y = Screen.height * 0.42f;

        for (int n = 1; n <= MaxPlayers; n++)
        {
            var r = new Rect(x0 + (n - 1) * (cell + gap), y, cell, cell);
            Color oldbg = GUI.backgroundColor;
            GUI.backgroundColor = TokenColors[n - 1];   // each count in the colour of the player it adds
            if (GUI.Button(r, n.ToString(), numStyle))
            {
                Sfx.Play("click");
                StartGameWith(n);
            }
            GUI.backgroundColor = oldbg;
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
        diamondSquare = -1;
        rollsSinceDiamond = 0;
        if (diamond != null) diamond.gameObject.SetActive(false);
        hulkSquare = -1;
        if (hulkToken != null) hulkToken.gameObject.SetActive(false);

        // all the way back to "how many players?" for a fresh game
        foreach (var p in players) { p.square = 0; p.stars = 0; PlaceToken(p); }
        HideAllTokens();
        players = null;
        chosen = null;
        picking = 0;
        selecting = false;
        choosingCount = true;
        message = "How many players?";
        starsTaken.Clear();
        foreach (int n in BoardLayout.Collectibles)
        {
            var star = GameObject.Find($"Star_{n}");
            if (star != null)
            {
                var sr = star.GetComponent<SpriteRenderer>();
                if (sr != null) sr.enabled = true;
            }
        }
    }
}
