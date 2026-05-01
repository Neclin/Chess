#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Chess.Tools
{
    public static class SelfPlayWindow
    {
        [MenuItem("Tools/Chess/Self-play arena")]
        public static void RunSinglePreviewGame()
        {
            var fullOptimisationConfig = new EngineConfig
            {
                Name = "full",
                UseTranspositionTable = true,
                UseMoveOrdering = true,
                UseMagicBitboards = true,
                Depth = 4
            };
            var baselineConfig = new EngineConfig
            {
                Name = "baseline",
                UseTranspositionTable = false,
                UseMoveOrdering = false,
                UseMagicBitboards = false,
                Depth = 4
            };

            string runTimestamp = DateTime.Now.ToString("yyyyMMdd-HHmm");
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string outputDirectory = Path.Combine(projectRoot, "Logs", $"selfplay-{runTimestamp}");

            Debug.Log($"[SelfPlay] Running 1 game: {fullOptimisationConfig.Name} vs {baselineConfig.Name} at depth {fullOptimisationConfig.Depth} (output → {outputDirectory})");

            var matchResults = SelfPlayArena.RunMatch(fullOptimisationConfig, baselineConfig, games: 1);
            SelfPlayArena.DumpMatchPgnsToDirectory(matchResults, outputDirectory, eventName: "Unity self-play preview");

            var resultRow = matchResults[0];
            Debug.Log($"[SelfPlay] {resultRow.White} (W) vs {resultRow.Black} (B): {resultRow.Outcome} after {resultRow.Plies} plies. PGN: {Path.Combine(outputDirectory, $"game-0001-{resultRow.White}-vs-{resultRow.Black}.pgn")}");
            Debug.Log($"[SelfPlay] {SelfPlayArena.Summarise(matchResults, fullOptimisationConfig.Name)}");
        }
    }
}
#endif
