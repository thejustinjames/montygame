using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Turn-based Snakes-&-Ladders game loop over the 100-square board.
/// Mirrors MontyGame.Core's rules: roll 1-6, walk square-to-square, take
/// ladders up / snakes down (once), collect stars, reach 100 to face the boss
/// and win. Draws its own ROLL button + HUD with IMGUI (no editor setup).
/// </summary>
public class GameController : MonoBehaviour
{
    private Transform token;
    private int square = 1;
    private int stars = 0;
    private int lastRoll = 0;
    private bool busy = false;
    private bool won = false;
    private string message = "Help Dino climb to 100 and beat the boss!";
    private readonly HashSet<int> collected = new HashSet<int>();
    private readonly System.Random rng = new System.Random();

    private GUIStyle labelStyle, bigStyle, buttonStyle, boxStyle;

    IEnumerator Start()
    {
        yield return null; // let the board finish building
        var t = GameObject.FindWithTag("Player");
        token = t.transform;
        square = 1;
        token.position = BoardLayout.SquareToWorld(1) + new Vector3(0, 0, -2);
    }

    IEnumerator DoTurn(int roll)
    {
        busy = true;
        lastRoll = roll;
        message = $"Rolled a {roll}!";

        int target = Mathf.Min(square + roll, BoardLayout.Squares);

        // walk one square at a time
        while (square < target)
        {
            yield return Hop(square + 1);
            square++;
            TryCollect(square);
        }

        // ladder or snake (applied once)
        if (BoardLayout.Ladders.TryGetValue(square, out int up))
        {
            message = "Time Portal! Zoom UP!";
            yield return new WaitForSeconds(0.35f);
            yield return HopTo(up);
            square = up;
            TryCollect(square);
        }
        else if (BoardLayout.Snakes.TryGetValue(square, out int down))
        {
            message = "Whirlpool! Swept back down!";
            yield return new WaitForSeconds(0.35f);
            yield return HopTo(down);
            square = down;
        }

        // win / boss
        if (square >= BoardLayout.Boss)
        {
            message = "BOSS! Dino dodges the T-Rex...";
            yield return new WaitForSeconds(0.8f);
            won = true;
            message = $"VICTORY! Reached 100 with {stars} stars. Dino is home!";
        }
        else
        {
            message = $"On square {square}. Roll again!";
        }

        busy = false;
    }

    void TryCollect(int sq)
    {
        if (BoardLayout.Collectibles.Contains(sq) && !collected.Contains(sq))
        {
            collected.Add(sq);
            stars++;
            var star = GameObject.Find($"Star_{sq}");
            if (star != null)
            {
                var sr = star.GetComponent<SpriteRenderer>();
                if (sr != null) sr.enabled = false; // hide but keep findable
            }
            message = $"Collected a star!  ({stars} total)";
        }
    }

    IEnumerator Hop(int toSquare) { yield return HopTo(toSquare); }

    IEnumerator HopTo(int toSquare)
    {
        Vector3 start = token.position;
        Vector3 end = BoardLayout.SquareToWorld(toSquare) + new Vector3(0, 0, -2);
        float dur = 0.16f, t = 0f;
        Vector3 baseScale = token.localScale;
        while (t < dur)
        {
            t += Time.deltaTime;
            float f = t / dur;
            token.position = Vector3.Lerp(start, end, f);
            token.localScale = baseScale * (1f + 0.25f * Mathf.Sin(f * Mathf.PI)); // little pop
            yield return null;
        }
        token.position = end;
        token.localScale = baseScale;
    }

    void EnsureStyles()
    {
        if (labelStyle != null) return;
        labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
        labelStyle.normal.textColor = Color.white;
        bigStyle = new GUIStyle(GUI.skin.label) { fontSize = 26, fontStyle = FontStyle.Bold };
        bigStyle.normal.textColor = new Color(1f, 0.9f, 0.3f);
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 24, fontStyle = FontStyle.Bold };
        boxStyle = new GUIStyle(GUI.skin.box);
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, new Color(0, 0, 0, 0.6f));
        tex.Apply();
        boxStyle.normal.background = tex;
    }

    void OnGUI()
    {
        EnsureStyles();

        GUI.Box(new Rect(15, 15, 360, 120), GUIContent.none, boxStyle);
        GUI.Label(new Rect(28, 22, 340, 30), $"Square {square} / 100    ⭐ {stars}", labelStyle);
        string roll = lastRoll > 0 ? $"Dice: {lastRoll}" : "";
        GUI.Label(new Rect(28, 50, 340, 30), roll, labelStyle);
        GUI.Label(new Rect(28, 78, 340, 50), message, labelStyle);

        if (!busy && !won)
        {
            if (GUI.Button(new Rect(15, 145, 200, 60), "ROLL  🎲", buttonStyle))
                StartCoroutine(DoTurn(rng.Next(1, 7)));
        }
        else if (won)
        {
            GUI.Label(new Rect(15, 145, 360, 40), "🏆 YOU WIN! 🏆", bigStyle);
            if (GUI.Button(new Rect(15, 190, 200, 55), "PLAY AGAIN  ▶", buttonStyle))
                ResetGame();
        }
    }

    void ResetGame()
    {
        won = false;
        busy = false;
        lastRoll = 0;
        square = 1;
        stars = 0;
        collected.Clear();
        token.position = BoardLayout.SquareToWorld(1) + new Vector3(0, 0, -2);
        foreach (int n in BoardLayout.Collectibles)
        {
            var star = GameObject.Find($"Star_{n}");
            if (star != null)
            {
                var sr = star.GetComponent<SpriteRenderer>();
                if (sr != null) sr.enabled = true;
            }
        }
        message = "Help Dino climb to 100 and beat the boss!";
    }
}
