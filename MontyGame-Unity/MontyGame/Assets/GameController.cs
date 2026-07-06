using UnityEngine;
using System.Collections;

/// <summary>
/// Turn-based board-game loop for the 5-tile MontyGame demo.
/// Mirrors the rules proven in MontyGame.Core (roll 1-6, hop tile-to-tile,
/// apply the tile effect once, win on the gold tile). Draws its own ROLL
/// button + status text with IMGUI so NO Unity editor setup is needed.
///
/// Board (matches the colored tiles the bootstrap builds):
///   1 green  - start
///   2 green  - normal
///   3 purple - TIME PORTAL  -> zoom forward to tile 4
///   4 blue   - WHIRLPOOL    -> swept back to tile 2
///   5 gold   - GOAL         -> win!
/// </summary>
public class GameController : MonoBehaviour
{
    const int NumTiles = 5;

    private Transform player;
    private PlayerController playerCtrl;
    private Rigidbody2D playerRb;
    private Vector3[] landing = new Vector3[NumTiles + 1];

    private int playerTile = 1;
    private int lastRoll = 0;
    private bool busy = false;
    private bool won = false;
    private string message = "Press ROLL to help Dino find the Portal Key!";
    private readonly System.Random rng = new System.Random();

    private GUIStyle labelStyle, buttonStyle, boxStyle;

    IEnumerator Start()
    {
        // Wait one frame so GameBootstrap has finished building the scene
        yield return null;

        var p = GameObject.FindWithTag("Player");
        player = p.transform;
        playerCtrl = p.GetComponent<PlayerController>();
        playerRb = p.GetComponent<Rigidbody2D>();

        for (int i = 1; i <= NumTiles; i++)
        {
            var tile = GameObject.Find($"Tile_{i}");
            landing[i] = tile.transform.position + new Vector3(0, 0.7f, 0);
        }

        playerTile = 1;
        SnapToTile(1);
    }

    // ---- Tile effects (applied ONCE per landing, so no infinite loops) ----
    int EffectTarget(int tile)
    {
        if (tile == 3) return 4; // portal forward
        if (tile == 4) return 2; // whirlpool back
        return 0;
    }
    string EffectText(int tile)
    {
        if (tile == 3) return "Time Portal! Zoom forward!";
        if (tile == 4) return "Whirlpool! Swept back!";
        return "";
    }

    IEnumerator DoTurn(int roll)
    {
        busy = true;
        lastRoll = roll;
        if (playerCtrl) playerCtrl.enabled = false;
        if (playerRb) { playerRb.bodyType = RigidbodyType2D.Kinematic; playerRb.linearVelocity = Vector2.zero; }

        message = $"Rolled a {roll}!";
        int target = Mathf.Min(playerTile + roll, NumTiles);

        // Hop forward one tile at a time
        while (playerTile < target)
        {
            yield return Hop(landing[playerTile + 1]);
            playerTile++;
        }

        // Apply this tile's effect once (unless it's the goal)
        int eff = EffectTarget(playerTile);
        if (eff != 0 && playerTile != NumTiles)
        {
            message = EffectText(playerTile);
            yield return new WaitForSeconds(0.5f);
            yield return Hop(landing[eff]);
            playerTile = eff;
        }

        // Win check
        if (playerTile == NumTiles)
        {
            won = true;
            message = "You found the Portal Key! Dino is going home!";
        }
        else
        {
            message = $"Landed on tile {playerTile}. Roll again!";
        }

        if (playerRb) playerRb.bodyType = RigidbodyType2D.Dynamic;
        if (playerCtrl) playerCtrl.enabled = true;
        busy = false;
    }

    IEnumerator Hop(Vector3 end)
    {
        Vector3 start = player.position;
        float dur = 0.35f, t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float f = t / dur;
            Vector3 pos = Vector3.Lerp(start, end, f);
            pos.y += Mathf.Sin(f * Mathf.PI) * 1.2f; // cute hop arc
            player.position = pos;
            yield return null;
        }
        player.position = end;
        yield return new WaitForSeconds(0.08f);
    }

    void SnapToTile(int tile)
    {
        if (player != null) player.position = landing[tile];
    }

    void EnsureStyles()
    {
        if (labelStyle != null) return;
        labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
        labelStyle.normal.textColor = Color.white;
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 24, fontStyle = FontStyle.Bold };
        boxStyle = new GUIStyle(GUI.skin.box);
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, new Color(0, 0, 0, 0.55f));
        tex.Apply();
        boxStyle.normal.background = tex;
    }

    void OnGUI()
    {
        EnsureStyles();

        GUI.Box(new Rect(15, 15, 520, 110), GUIContent.none, boxStyle);
        string rollTxt = lastRoll > 0 ? $"   Dice: {lastRoll}" : "";
        GUI.Label(new Rect(30, 25, 500, 30), $"Tile {playerTile} of {NumTiles}{rollTxt}", labelStyle);
        GUI.Label(new Rect(30, 58, 500, 60), message, labelStyle);

        if (!busy && !won)
        {
            if (GUI.Button(new Rect(30, 135, 200, 60), "ROLL  🎲", buttonStyle))
                StartCoroutine(DoTurn(rng.Next(1, 7)));
        }
        else if (won)
        {
            if (GUI.Button(new Rect(30, 135, 200, 60), "PLAY AGAIN  ▶", buttonStyle))
            {
                won = false;
                lastRoll = 0;
                playerTile = 1;
                SnapToTile(1);
                message = "Press ROLL to help Dino find the Portal Key!";
            }
        }
    }
}
