using System.Collections.Generic;
using UnityEngine;
using Chess.Core.Moves;
using Chess.Core.Rules;

namespace Chess.Unity
{
    public sealed class AudioController : MonoBehaviour
    {
        public GameController Game;
        public InputController Input;
        public AudioSource AudioSource;
        public AudioClip MoveClip;
        public AudioClip CaptureClip;
        public AudioClip CheckClip;
        public AudioClip SelectClip;
        public AudioClip CheckmateClip;
        public AudioClip VictoryClip;
        public AudioClip DefeatClip;
        public AudioClip DrawClip;

        private void OnEnable()
        {
            if (Game != null)
            {
                Game.OnMoveApplied += HandleMoveApplied;
                Game.OnGameOver += HandleGameOver;
            }
            if (Input != null) Input.OnSelectionChanged += HandleSelectionChanged;
        }

        private void OnDisable()
        {
            if (Game != null)
            {
                Game.OnMoveApplied -= HandleMoveApplied;
                Game.OnGameOver -= HandleGameOver;
            }
            if (Input != null) Input.OnSelectionChanged -= HandleSelectionChanged;
        }

        private void HandleMoveApplied(Move appliedMove, string sanText, UndoInfo undoInfo)
        {
            if (!SettingsStore.LoadSoundEnabled()) return;
            if (AudioSource == null) return;
            GameResult resultAfterMove = GameStateChecker.Evaluate(Game.State);
            AudioClipKind clipKind = AudioClipSelector.SelectMoveKind(appliedMove, Game.State, resultAfterMove);
            PlayClip(ResolveClip(clipKind));
        }

        private void HandleGameOver(GameResult resultAfterMove)
        {
            if (!SettingsStore.LoadSoundEnabled()) return;
            if (AudioSource == null) return;
            AudioClipKind clipKind = AudioClipSelector.SelectOutcomeKind(resultAfterMove, Game.HumanColor);
            PlayClip(ResolveClip(clipKind));
        }

        private void HandleSelectionChanged(int selectedSquareIndex, IReadOnlyList<Chess.Core.Moves.Move> legalMovesFromSelection)
        {
            if (!SettingsStore.LoadSoundEnabled()) return;
            if (AudioSource == null || SelectClip == null) return;
            PlayClip(SelectClip);
        }

        private void PlayClip(AudioClip clipToPlay)
        {
            if (clipToPlay != null) AudioSource.PlayOneShot(clipToPlay);
        }

        private AudioClip ResolveClip(AudioClipKind clipKind)
        {
            return clipKind switch
            {
                AudioClipKind.Move => MoveClip,
                AudioClipKind.Capture => CaptureClip,
                AudioClipKind.Check => CheckClip,
                AudioClipKind.Select => SelectClip,
                AudioClipKind.Checkmate => CheckmateClip,
                AudioClipKind.Victory => VictoryClip,
                AudioClipKind.Defeat => DefeatClip,
                AudioClipKind.Draw => DrawClip,
                _ => null
            };
        }
    }
}
