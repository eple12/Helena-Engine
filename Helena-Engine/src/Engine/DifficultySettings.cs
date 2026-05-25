namespace H.Engine;

/// <summary>
/// The 11 difficulty levels of Helena-Engine.
/// MARTIN (0) is the weakest; MAXIMUM (10) always plays the engine's best move.
/// </summary>
public enum DifficultyLevel
{
    MARTIN           = 0,
    BEGINNER         = 1,
    NOVICE           = 2,
    INTERMEDIATE     = 3,
    CLUB             = 4,
    ADVANCED         = 5,
    EXPERT           = 6,
    CANDIDATE_MASTER = 7,
    MASTER           = 8,
    IM               = 9,
    MAXIMUM          = 10,
}

/// <summary>
/// Per-level configuration for the probabilistic move-selection system.
/// </summary>
public readonly struct DifficultyConfig
{
    /// <summary>Difficulty enum value.</summary>
    public DifficultyLevel Level       { get; init; }

    /// <summary>Human-readable name used in UCI output and the 'difficulty' command.</summary>
    public string          Name        { get; init; }

    /// <summary>Short description shown in 'difficulty list'.</summary>
    public string          Description { get; init; }

    /// <summary>
    /// Maximum rank offset from the best move that can ever be selected.
    /// 0 → only the best move is ever played (MAXIMUM).
    /// 6 → the engine may play the 1st through 7th best moves.
    /// Moves ranked beyond MaxN always have probability zero.
    /// </summary>
    public int             MaxN        { get; init; }

    /// <summary>
    /// Softmax temperature in centipawns.
    /// weight[i] = exp(−evalDiff[i] / Temperature), where evalDiff[i] = best_score − score[i] ≥ 0.
    /// Higher temperature → flatter distribution (more random).
    /// Temperature = 0 is reserved for MAXIMUM (never used in the selection code).
    /// </summary>
    public double          Temperature { get; init; }

    public DifficultyConfig(
        DifficultyLevel level,
        string name,
        string description,
        int maxN,
        double temperature)
    {
        Level       = level;
        Name        = name;
        Description = description;
        MaxN        = maxN;
        Temperature = temperature;
    }
}

/// <summary>
/// Static table of all difficulty configurations.
///
/// Selection algorithm (for levels other than MAXIMUM):
///   1. All legal root moves are scored with a quiescence search.
///   2. Moves are ranked best-to-worst by that score.
///   3. Only the top (MaxN + 1) candidates are eligible.
///   4. A softmax over those candidates (temperature = Temperature cp) is used
///      to draw one move probabilistically.
///
/// Result: large eval gaps suppress bad moves naturally, while near-equal
/// alternatives get picked with realistic frequency — no pure randomness.
/// </summary>
public static class DifficultySettings
{
    public static readonly DifficultyConfig[] Configs =
    [
        // Level                  Name                 Description                                       MaxN  Temp
        new(DifficultyLevel.MARTIN,           "Martin",           "Complete beginner (Chess.com Martin level)",        6, 400.0),
        new(DifficultyLevel.BEGINNER,         "Beginner",         "Beginner (~500 ELO)",                               5, 260.0),
        new(DifficultyLevel.NOVICE,           "Novice",           "Novice (~800-1000 ELO)",                            4, 170.0),
        new(DifficultyLevel.INTERMEDIATE,     "Intermediate",     "Intermediate (~1100-1300 ELO)",                     3, 110.0),
        new(DifficultyLevel.CLUB,             "Club",             "Club player (~1400-1600 ELO)",                      3,  70.0),
        new(DifficultyLevel.ADVANCED,         "Advanced",         "Advanced (~1700-1900 ELO)",                         2,  45.0),
        new(DifficultyLevel.EXPERT,           "Expert",           "Expert (~2000-2100 ELO)",                           2,  26.0),
        new(DifficultyLevel.CANDIDATE_MASTER, "Candidate Master", "Candidate Master (~2200 ELO)",                      1,  15.0),
        new(DifficultyLevel.MASTER,           "Master",           "Master (~2300-2400 ELO)",                           1,   8.0),
        new(DifficultyLevel.IM,               "IM",               "International Master (~2400-2500 ELO)",             1,   4.0),
        new(DifficultyLevel.MAXIMUM,          "Maximum",          "Full engine strength (always plays the best move)", 0,   0.0),
    ];

    /// <summary>Returns the config for the given difficulty level.</summary>
    public static DifficultyConfig Get(DifficultyLevel level)
        => Configs[(int)level];
}
