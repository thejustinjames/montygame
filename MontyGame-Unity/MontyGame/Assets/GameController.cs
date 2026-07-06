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

    private GUIStyle labelStyle, bigStyle, buttonStyle, boxStyle, turnStyle;

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
    }

    void PlaceToken(Player p)
    {
        p.token.position = BoardLayout.SquareToWorld(p.square) + p.offset;
    }

    IEnumerator DoTurn(int roll)
    {
        busy = true;
        lastRoll = roll;
        Player p = players[current];
        message = $"{p.name} rolled a {roll}!";

        int target = Mathf.Min(p.square + roll, BoardLayout.Squares);

        while (p.square < target)
        {
            yield return HopTo(p, p.square + 1);
            p.square++;
            TryCollect(p, p.square);
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
            yield return new WaitForSeconds(0.8f);
            won = true;
            winner = current;
            message = $"🏆 {p.name} WINS with {p.stars} stars! 🏆";
            busy = false;
            yield break;
        }

        // pass to the other player
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
        float dur = 0.16f, t = 0f;
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
    }

    void ResetGame()
    {
        won = false;
        busy = false;
        winner = -1;
        lastRoll = 0;
        current = 0;
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
