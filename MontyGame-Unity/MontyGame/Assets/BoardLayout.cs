using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Shared data + math for the 10x10 (100-square) MontyGame board.
/// Both GameBootstrap (rendering) and GameController (movement) use this so
/// the visual board and the game logic can never disagree.
///
/// Numbering is "boustrophedon" like classic Snakes & Ladders:
///   square 1 is bottom-left, counting right along the bottom row, then the
///   next row up counts right-to-left, and so on up to 100 (top row).
/// </summary>
public static class BoardLayout
{
    public const int Grid = 10;          // 10 x 10
    public const int Squares = 100;      // reach 100 to win
    public const float Cell = 1f;        // world units per square
    public const float OriginX = -4.5f;  // centers the grid on (0,0)
    public const float OriginY = -4.5f;

    // Ladders / Portals: land on the key -> zoom UP to the value.
    public static readonly Dictionary<int, int> Ladders = new Dictionary<int, int>
    {
        { 3, 22 }, { 8, 26 }, { 20, 41 }, { 28, 77 },
        { 36, 57 }, { 51, 72 }, { 71, 92 },
    };

    // Snakes / Whirlpools: land on the key -> slide DOWN to the value.
    public static readonly Dictionary<int, int> Snakes = new Dictionary<int, int>
    {
        { 17, 6 }, { 47, 26 }, { 62, 18 }, { 87, 24 }, { 95, 56 },
    };

    // Collectible "objects" (stars) scattered up the board.
    public static readonly HashSet<int> Collectibles = new HashSet<int>
    {
        5, 14, 33, 45, 58, 66, 73, 84, 91,
    };

    public const int Boss = 100; // boss guards the final square

    /// <summary>World-space center of a given square number (1..100).</summary>
    public static Vector3 SquareToWorld(int n)
    {
        n = Mathf.Clamp(n, 1, Squares);
        int row = (n - 1) / Grid;          // 0 = bottom row
        int idx = (n - 1) % Grid;
        int col = (row % 2 == 0) ? idx : (Grid - 1 - idx); // snake back-and-forth
        float x = OriginX + col * Cell;
        float y = OriginY + row * Cell;
        return new Vector3(x, y, 0);
    }
}
