// MontyGame CLI playthrough simulator (Docs/WORLD_1_LAYOUT.md, "Core Loop").
// `--auto` drives a full seeded 2-player World 1 game to completion, narrating every
// turn and story beat, and exits 0 once a player wins.
using MontyGame.Core;

const int DefaultSeed = 42;

if (!args.Contains("--auto"))
{
    PrintUsage();
    return 1;
}

return RunAutoPlaythrough(ParseSeed(args));

static int ParseSeed(string[] args)
{
    var seedIndex = Array.IndexOf(args, "--seed");
    return seedIndex >= 0 && seedIndex + 1 < args.Length && int.TryParse(args[seedIndex + 1], out var seed)
        ? seed
        : DefaultSeed;
}

static void PrintUsage()
{
    Console.WriteLine("MontyGame CLI — World 1 playthrough simulator");
    Console.WriteLine();
    Console.WriteLine("Usage: dotnet run --project src/MontyGame.Cli -- --auto [--seed <number>]");
    Console.WriteLine("  --auto           Play a full seeded 2-player World 1 game to completion.");
    Console.WriteLine($"  --seed <number>  Random seed for a reproducible playthrough (default: {DefaultSeed}).");
}

static int RunAutoPlaythrough(int seed)
{
    var board = World1.CreateDinoJungleBoard();
    var random = new SystemRandom(seed);
    var playerNames = new[] { "Dino", "Cat" };
    var engine = new GameEngine(board, random, playerNames);

    Console.WriteLine("=================================================");
    Console.WriteLine(" MontyGame — World 1: Dino Jungle");
    Console.WriteLine($" Seed: {seed} | Players: {string.Join(", ", playerNames)}");
    Console.WriteLine("=================================================");
    Console.WriteLine();
    Console.WriteLine("Welcome to Dino Jungle! Dino and Cat are lost! Help them find the Portal");
    Console.WriteLine("Key to return home. Watch out for obstacles and use the magical portals");
    Console.WriteLine("to leap ahead!");
    Console.WriteLine();

    var narratedStoryBeats = new HashSet<int>();
    var turnNumber = 0;

    while (engine.Status != GameStatus.Complete)
    {
        turnNumber++;

        var result = engine.Status == GameStatus.AwaitingBossChallenge
            ? engine.ResolveBossChallenge(challengePassed: random.Next(0, 2) == 0)
            : engine.TakeTurn();

        PrintTurn(turnNumber, result);
        NarrateStoryBeats(result.FinalPosition, narratedStoryBeats);
    }

    Console.WriteLine("You found the Portal Key! Dino and Cat are going home! *Celebration sequence* THE END!");
    Console.WriteLine($"*** {engine.Winner!.Name} wins in {turnNumber} turns! ***");
    return 0;
}

static void PrintTurn(int turnNumber, TurnResult result)
{
    Console.WriteLine($"Turn {turnNumber}: {result.PlayerName}");

    if (result.DiceRoll is int roll)
    {
        Console.WriteLine($"  Rolls a {roll} -> moves from tile {result.StartingPosition} to tile {result.LandedTile.Number} ({result.LandedTile.Name})");
        PrintLandingEffect(result);
    }
    else
    {
        PrintBossResolution(result);
    }

    if (result.WonGame)
    {
        Console.WriteLine($"  *** {result.PlayerName} reaches tile {result.FinalPosition} — VICTORY! ***");
    }

    Console.WriteLine();
}

static void PrintLandingEffect(TurnResult result)
{
    switch (result.TileEffect.Outcome)
    {
        case TileEffectOutcome.Moved:
            Console.WriteLine($"  {result.LandedTile.Type} effect! Swept from tile {result.LandedTile.Number} to tile {result.TileEffect.Position}.");
            break;
        case TileEffectOutcome.CardDrawn:
            Console.WriteLine($"  Mystery tile! Draws {result.TileEffect.DrawnCard} ({result.TileEffect.DrawnCard!.Value.Spaces()} spaces) -> tile {result.TileEffect.Position}.");
            break;
        case TileEffectOutcome.BossChallenge:
            Console.WriteLine("  A Giant T-Rex blocks the path! Time for a dodge challenge...");
            break;
        case TileEffectOutcome.None:
            break;
    }
}

static void PrintBossResolution(TurnResult result)
{
    Console.WriteLine(result.AwaitingBossChallenge
        ? $"  {result.PlayerName} dodges bravely but the T-Rex is still blocking the way. Try again next turn!"
        : $"  {result.PlayerName} times a perfect dodge and slips past the T-Rex to tile {result.FinalPosition}!");
}

static void NarrateStoryBeats(int reachedPosition, HashSet<int> alreadyNarrated)
{
    var storyBeats = new (int Tile, string Message)[]
    {
        (12, "Great job! You're halfway there. The temple ruins ahead hold more secrets. Keep going!"),
        (23, "Oh no! A Giant T-Rex is guarding the final chamber! Stay calm and dodge its attacks. You can do this!"),
    };

    foreach (var (tile, message) in storyBeats)
    {
        if (reachedPosition >= tile && alreadyNarrated.Add(tile))
        {
            Console.WriteLine($"  >> {message}");
            Console.WriteLine();
        }
    }
}
