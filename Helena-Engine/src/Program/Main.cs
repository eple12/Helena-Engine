namespace H.Program;

using H.Core;
using H.Engine;

// This class holds all the global objects (The main board that we can interact with in the console, etc.)
public static class Main
{
    public static Board MainBoard = new();
    public static EnginePlayer MainEnginePlayer = new(MainBoard);
}