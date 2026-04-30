using System;
using System.Diagnostics;
using System.IO;

namespace Chess.Tools
{
    public sealed class StockfishBridge : IDisposable
    {
        private const string FallbackBinaryPath = "C:/Tools/stockfish/stockfish.exe";
        private const string NodesSearchedPrefix = "Nodes searched:";
        private const string BestMovePrefix = "bestmove ";

        private readonly Process stockfishProcess;

        public StockfishBridge()
        {
            string binaryPath = ResolveStockfishBinaryPath();

            stockfishProcess = new Process
            {
                StartInfo = new ProcessStartInfo(binaryPath)
                {
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            stockfishProcess.Start();

            SendCommand("uci");
            ReadUntilLineStartsWith("uciok");
            SendCommand("isready");
            ReadUntilLineStartsWith("readyok");
        }

        public long Perft(string fen, int depth)
        {
            SendCommand($"position fen {fen}");
            SendCommand($"go perft {depth}");

            string outputLine;
            while ((outputLine = stockfishProcess.StandardOutput.ReadLine()) != null)
            {
                if (outputLine.StartsWith(NodesSearchedPrefix))
                {
                    string nodeCountText = outputLine.Substring(NodesSearchedPrefix.Length).Trim();
                    return long.Parse(nodeCountText);
                }
            }

            throw new InvalidOperationException(
                $"Stockfish closed stdout before reporting perft node count for fen=\"{fen}\" depth={depth}.");
        }

        public string BestMove(string fen, int movetimeMilliseconds)
        {
            SendCommand($"position fen {fen}");
            SendCommand($"go movetime {movetimeMilliseconds}");

            string outputLine;
            while ((outputLine = stockfishProcess.StandardOutput.ReadLine()) != null)
            {
                if (outputLine.StartsWith(BestMovePrefix))
                {
                    return outputLine.Substring(BestMovePrefix.Length).Split(' ')[0];
                }
            }

            throw new InvalidOperationException(
                $"Stockfish closed stdout before reporting bestmove for fen=\"{fen}\".");
        }

        public void Dispose()
        {
            if (!stockfishProcess.HasExited)
            {
                try { SendCommand("quit"); } catch (IOException) { }
                stockfishProcess.WaitForExit(2000);
                if (!stockfishProcess.HasExited) stockfishProcess.Kill();
            }
            stockfishProcess.Dispose();
        }

        private void SendCommand(string command)
        {
            stockfishProcess.StandardInput.WriteLine(command);
            stockfishProcess.StandardInput.Flush();
        }

        private void ReadUntilLineStartsWith(string token)
        {
            string outputLine;
            while ((outputLine = stockfishProcess.StandardOutput.ReadLine()) != null)
            {
                if (outputLine.StartsWith(token)) return;
            }
            throw new InvalidOperationException(
                $"Stockfish closed stdout before sending \"{token}\".");
        }

        private static string ResolveStockfishBinaryPath()
        {
            string envPath = Environment.GetEnvironmentVariable("STOCKFISH_PATH");
            if (!string.IsNullOrEmpty(envPath) && File.Exists(envPath)) return envPath;
            if (File.Exists(FallbackBinaryPath)) return FallbackBinaryPath;
            throw new FileNotFoundException(
                "Stockfish not found. Set STOCKFISH_PATH or install at " + FallbackBinaryPath + ".");
        }
    }
}
