using System;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using OneMoreRoulette.Core;

namespace OneMoreRoulette.UI
{
    public class UiBinder : MonoBehaviour, IDisposable
    {
        [SerializeField] private Button _oneMoreButton;
        [SerializeField] private Button _stopButton;
        [SerializeField] private TMP_Text _roundText;
        [SerializeField] private TMP_Text _deadText;
        [SerializeField] private TMP_Text _bulletText;
        [SerializeField] private TMP_Text _rankText;
        [SerializeField] private TMP_Text _multiplierText;
        [SerializeField] private TMP_Text _carryNextText;
        [SerializeField] private TMP_Text _roundScoreText;
        [SerializeField] private TMP_Text _totalScoreText;

        private GameViewModel _viewModel;
        private RunController _controller;
        private readonly System.Collections.Generic.List<IDisposable> _disposables = new();

        private void OnEnable()
        {
            WireButtons();
        }

        private void OnDisable()
        {
            DisposeBindings();
            UnwireButtons();
        }

        public void Initialize(GameViewModel viewModel, RunController controller)
        {
            _viewModel = viewModel;
            _controller = controller;
            DisposeBindings();
            BindViewModel();
        }

        public void Dispose()
        {
            DisposeBindings();
            UnwireButtons();
        }

        private void WireButtons()
        {
            if (_oneMoreButton != null)
            {
                _oneMoreButton.onClick.AddListener(OnPressOneMore);
            }

            if (_stopButton != null)
            {
                _stopButton.onClick.AddListener(OnPressStop);
            }
        }

        private void UnwireButtons()
        {
            if (_oneMoreButton != null)
            {
                _oneMoreButton.onClick.RemoveListener(OnPressOneMore);
            }

            if (_stopButton != null)
            {
                _stopButton.onClick.RemoveListener(OnPressStop);
            }
        }

        private void OnPressOneMore()
        {
            _controller?.OnOneMoreAsync().Forget();
        }

        private void OnPressStop()
        {
            _controller?.OnStopAsync().Forget();
        }

        private void BindViewModel()
        {
            if (_viewModel == null)
            {
                return;
            }

            _disposables.Add(_viewModel.RoundIndex.Subscribe(x => SetText(_roundText, $"ROUND {x}")));
            _disposables.Add(_viewModel.DeadCount.Subscribe(x => SetText(_deadText, $"DEAD {x}/2")));
            _disposables.Add(_viewModel.BulletCount.Subscribe(x => SetText(_bulletText, $"BULLET {x}/6")));
            _disposables.Add(_viewModel.Rank.Subscribe(x => SetText(_rankText, $"RANK {x}")));
            _disposables.Add(_viewModel.Multiplier.Subscribe(x => SetText(_multiplierText, $"x{ x:F2}")));
            _disposables.Add(_viewModel.CarryNextMultiplier.Subscribe(x => SetText(_carryNextText, $"NEXT x{x:F2}")));
            _disposables.Add(_viewModel.RoundScore.Subscribe(x => SetText(_roundScoreText, x.ToString())));
            _disposables.Add(_viewModel.TotalScore.Subscribe(x => SetText(_totalScoreText, x.ToString())));
            _disposables.Add(_viewModel.State.Subscribe(OnStateChanged));
        }

        private void OnStateChanged(GameState state)
        {
            var interactable = state == GameState.Decision;
            if (_oneMoreButton != null)
            {
                _oneMoreButton.interactable = interactable;
            }

            if (_stopButton != null)
            {
                _stopButton.interactable = interactable;
            }
        }

        private void DisposeBindings()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }

            _disposables.Clear();
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }
    }
}
