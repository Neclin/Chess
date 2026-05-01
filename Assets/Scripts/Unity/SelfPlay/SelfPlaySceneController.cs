using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Chess.Core.Board;
using Chess.Core.Moves;
using Chess.Core.Notation;
using Chess.Core.Rules;
using Chess.Core.Search;
using Chess.Tools;
using Chess.Unity.UI;

namespace Chess.Unity
{
    public sealed class SelfPlaySceneController : MonoBehaviour
    {
        [Header("Board and panels")]
        public BoardView Board;
        public MoveHistoryPanel History;
        public CapturedPiecesPanel Captures;

        [Header("Header / scoreboard labels")]
        public TMP_Text HeaderLabel;
        public TMP_Text ScoreboardLabel;

        [Header("Pre-game picker / in-game controls / post-game dialog")]
        public GameObject PickerPanel;
        public GameObject PlayControlsPanel;
        public Button PauseButton;
        public TMP_Text PauseButtonLabel;
        public Button StopButton;
        public SelfPlaySummaryDialog SummaryDialog;

        [Header("Auto-start session on scene load (skip picker — for quick visual verification)")]
        public bool AutoStartOnSceneLoad = false;
        public string AutoStartConfigA = "baseline";
        public string AutoStartConfigB = "full";
        public int AutoStartGames = 3;
        public int AutoStartDepth = 3;
        public int AutoStartTimeMs = 0;

        private BoardState _liveBoard;
        private EngineConfig _configurationA;
        private EngineConfig _configurationB;
        private int _gamesRequested;
        private int _currentGameIndex;
        private int _aWins, _aLosses, _draws;
        private int _bWins, _bLosses;
        private bool _isPaused;
        private bool _isStopRequested;
        private bool _isSearchInFlight;
        private string _sessionTimestamp;
        private string _sessionLogDirectory;
        private readonly List<SelfPlayArena.MatchResult> _sessionResults = new List<SelfPlayArena.MatchResult>();
        private readonly List<Move> _movesInCurrentGame = new List<Move>();
        private readonly ConcurrentQueue<Action> _mainThreadCallbacks = new ConcurrentQueue<Action>();

        private void Start()
        {
            if (Board != null) Board.Build();
            if (PauseButton != null) PauseButton.onClick.AddListener(TogglePause);
            if (StopButton != null) StopButton.onClick.AddListener(RequestStop);
            if (AutoStartOnSceneLoad)
            {
                EngineConfig autoConfigA = SelfPlayArena.CloneNamedConfig(AutoStartConfigA, AutoStartDepth, AutoStartTimeMs);
                EngineConfig autoConfigB = SelfPlayArena.CloneNamedConfig(AutoStartConfigB, AutoStartDepth, AutoStartTimeMs);
                BeginSession(autoConfigA, autoConfigB, AutoStartGames);
            }
            else
            {
                ShowPicker();
            }
        }

        private void OnDestroy()
        {
            if (PauseButton != null) PauseButton.onClick.RemoveListener(TogglePause);
            if (StopButton != null) StopButton.onClick.RemoveListener(RequestStop);
        }

        private void Update()
        {
            while (_mainThreadCallbacks.TryDequeue(out var callback)) callback();
        }

        public void BeginSession(EngineConfig configurationA, EngineConfig configurationB, int gamesRequested)
        {
            _configurationA = configurationA;
            _configurationB = configurationB;
            _gamesRequested = Mathf.Max(1, gamesRequested);
            _currentGameIndex = 0;
            _aWins = _aLosses = _bWins = _bLosses = _draws = 0;
            _isPaused = false;
            _isStopRequested = false;
            _sessionResults.Clear();

            _sessionTimestamp = DateTime.Now.ToString("yyyyMMdd-HHmm");
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            _sessionLogDirectory = Path.Combine(projectRoot, "Logs", $"selfplay-{_sessionTimestamp}");
            Directory.CreateDirectory(_sessionLogDirectory);

            if (PickerPanel != null) PickerPanel.SetActive(false);
            if (PlayControlsPanel != null) PlayControlsPanel.SetActive(true);
            if (SummaryDialog != null) SummaryDialog.gameObject.SetActive(false);
            UpdatePauseButtonLabel();

            StartNextGame();
        }

        public void TogglePause()
        {
            _isPaused = !_isPaused;
            UpdatePauseButtonLabel();
            if (!_isPaused && !_isSearchInFlight && _liveBoard != null && !_isStopRequested)
                RequestNextMove();
        }

        public void RequestStop()
        {
            _isStopRequested = true;
            if (!_isSearchInFlight)
                FinaliseSessionAfterCurrentGame(adjudicateCurrentGameAsDraw: true);
        }

        public void RestartSession()
        {
            BeginSession(_configurationA, _configurationB, _gamesRequested);
        }

        public void ReturnToPicker()
        {
            if (SummaryDialog != null) SummaryDialog.gameObject.SetActive(false);
            if (PlayControlsPanel != null) PlayControlsPanel.SetActive(false);
            ShowPicker();
        }

        private void ShowPicker()
        {
            if (PickerPanel != null) PickerPanel.SetActive(true);
            if (HeaderLabel != null) HeaderLabel.text = "Pick two configs to start a session";
            if (ScoreboardLabel != null) ScoreboardLabel.text = string.Empty;
        }

        private void StartNextGame()
        {
            _liveBoard = FenParser.Parse(SelfPlayArena.StartPositionFen);
            MinimaxSearch.ResetTTStats();

            bool configurationAPlaysWhite = _currentGameIndex % 2 == 0;
            string whiteName = configurationAPlaysWhite ? _configurationA.Name : _configurationB.Name;
            string blackName = configurationAPlaysWhite ? _configurationB.Name : _configurationA.Name;

            _movesInCurrentGame.Clear();
            if (Board != null) Board.Sync(_liveBoard);
            if (History != null) History.ResetForExternalDriver(_liveBoard.SideToMove, _liveBoard.FullmoveNumber);
            if (Captures != null) Captures.ResetForExternalDriver();

            UpdateHeader(whiteName, blackName);
            UpdateScoreboard();

            RequestNextMove();
        }

        private void RequestNextMove()
        {
            if (_isPaused || _isStopRequested) return;

            GameResult positionResult = GameStateChecker.Evaluate(_liveBoard);
            if (positionResult != GameResult.Ongoing)
            {
                HandleGameOver(positionResult);
                return;
            }

            EngineConfig sideToMoveConfig = (_currentGameIndex % 2 == 0)
                ? (_liveBoard.SideToMove == PieceColor.White ? _configurationA : _configurationB)
                : (_liveBoard.SideToMove == PieceColor.White ? _configurationB : _configurationA);

            BoardState boardSnapshot = _liveBoard.Clone();
            _isSearchInFlight = true;
            Task.Run(() =>
            {
                Move chosenMove;
                try
                {
                    chosenMove = SelfPlayArena.ChooseMove(boardSnapshot, sideToMoveConfig);
                }
                catch (Exception searchException)
                {
                    Debug.LogError($"[SelfPlay] search threw {searchException.GetType().Name}: {searchException.Message}");
                    _mainThreadCallbacks.Enqueue(() => { _isSearchInFlight = false; });
                    return;
                }
                _mainThreadCallbacks.Enqueue(() => ApplyChosenMove(chosenMove));
            });
        }

        private void ApplyChosenMove(Move chosenMove)
        {
            _isSearchInFlight = false;

            string sanText = San.ToSan(_liveBoard, chosenMove);
            UndoInfo undoInfo = MoveExecutor.MakeMove(_liveBoard, chosenMove);
            _movesInCurrentGame.Add(chosenMove);

            if (Board != null) Board.Sync(_liveBoard);
            if (History != null) History.AppendSanMove(sanText);
            if (Captures != null && undoInfo.CapturedPiece != PieceType.None)
            {
                PieceColor capturerColor = undoInfo.CapturedColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
                Captures.RecordCapture(undoInfo.CapturedPiece, capturerColor);
            }

            if (_isStopRequested)
            {
                FinaliseSessionAfterCurrentGame(adjudicateCurrentGameAsDraw: true);
                return;
            }

            GameResult result = GameStateChecker.Evaluate(_liveBoard);
            if (result != GameResult.Ongoing)
            {
                HandleGameOver(result);
                return;
            }

            if (!_isPaused) RequestNextMove();
        }

        private void HandleGameOver(GameResult result)
        {
            bool configurationAPlaysWhite = _currentGameIndex % 2 == 0;
            string whiteName = configurationAPlaysWhite ? _configurationA.Name : _configurationB.Name;
            string blackName = configurationAPlaysWhite ? _configurationB.Name : _configurationA.Name;

            var matchResult = new SelfPlayArena.MatchResult
            {
                White = whiteName,
                Black = blackName,
                OpeningFen = SelfPlayArena.StartPositionFen,
                Outcome = result,
                Plies = _movesInCurrentGame.Count
            };
            matchResult.Moves.AddRange(_movesInCurrentGame);
            _sessionResults.Add(matchResult);

            UpdateScoreboardCounts(matchResult);

            string pgnFileName = SanitiseFileName($"game-{_currentGameIndex + 1:D4}-{whiteName}-vs-{blackName}.pgn");
            string pgnFilePath = Path.Combine(_sessionLogDirectory, pgnFileName);
            File.WriteAllText(pgnFilePath, SelfPlayArena.WriteMatchPgn(matchResult, eventName: $"{_configurationA.Name}-vs-{_configurationB.Name}", round: _currentGameIndex + 1));

            UpdateScoreboard();
            UpdateHeader(whiteName, blackName, gameOutcome: result);

            _currentGameIndex++;
            if (_currentGameIndex < _gamesRequested && !_isStopRequested)
            {
                StartNextGame();
            }
            else
            {
                FinaliseSessionAfterCurrentGame(adjudicateCurrentGameAsDraw: false);
            }
        }

        private void UpdateScoreboardCounts(SelfPlayArena.MatchResult matchResult)
        {
            switch (matchResult.Outcome)
            {
                case GameResult.WhiteWinsByCheckmate:
                    if (matchResult.White == _configurationA.Name) { _aWins++; _bLosses++; }
                    else { _bWins++; _aLosses++; }
                    break;
                case GameResult.BlackWinsByCheckmate:
                    if (matchResult.Black == _configurationA.Name) { _aWins++; _bLosses++; }
                    else { _bWins++; _aLosses++; }
                    break;
                default:
                    _draws++;
                    break;
            }
        }

        private void FinaliseSessionAfterCurrentGame(bool adjudicateCurrentGameAsDraw)
        {
            if (PlayControlsPanel != null) PlayControlsPanel.SetActive(false);
            if (SummaryDialog != null)
            {
                SummaryDialog.Show(
                    aName: _configurationA.Name, aWins: _aWins, aLosses: _aLosses,
                    bName: _configurationB.Name, bWins: _bWins, bLosses: _bLosses,
                    draws: _draws,
                    pgnFolderPath: _sessionLogDirectory);
            }
        }

        private void UpdateHeader(string whiteName, string blackName, GameResult? gameOutcome = null)
        {
            if (HeaderLabel == null) return;
            int gameNumber = _currentGameIndex + 1;
            string outcomeSuffix = gameOutcome.HasValue ? $" — {gameOutcome.Value}" : string.Empty;
            HeaderLabel.text = $"Game {gameNumber}/{_gamesRequested} — {whiteName} (W) vs {blackName} (B){outcomeSuffix}";
        }

        private void UpdateScoreboard()
        {
            if (ScoreboardLabel == null) return;
            ScoreboardLabel.text = $"{_configurationA.Name}: {_aWins}W {_aLosses}L  |  {_configurationB.Name}: {_bWins}W {_bLosses}L  |  draws: {_draws}";
        }

        private void UpdatePauseButtonLabel()
        {
            if (PauseButtonLabel != null) PauseButtonLabel.text = _isPaused ? "Resume" : "Pause";
        }

        private static string SanitiseFileName(string fileName)
        {
            foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(invalidCharacter, '_');
            return fileName;
        }
    }
}
