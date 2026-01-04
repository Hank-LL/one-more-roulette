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
        // Inspector で割り当てる UI 参照
        [Header("Game Buttons")]
        [SerializeField] private Button _oneMoreButton;
        [SerializeField] private Button _stopButton;
        [SerializeField] private Button[] _retryButtons;

        [Header("Game HUD")]
        [SerializeField] private TMP_Text _roundText;
        [SerializeField] private TMP_Text _deadText;
        [SerializeField] private TMP_Text _bulletText;
        [SerializeField] private TMP_Text _rankText;
        [SerializeField] private TMP_Text _multiplierText;
        [SerializeField] private TMP_Text _carryNextText;
        [SerializeField] private TMP_Text _roundScoreText;
        [SerializeField] private TMP_Text _totalScoreText;

        [Header("Result Window")]
        [SerializeField] private GameObject _resultWindow;
        [SerializeField] private TMP_Text _resultScoreText;

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

            // ResultWindowは初期状態で非表示
            if (_resultWindow != null)
            {
                _resultWindow.SetActive(false);
            }
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

            if (_retryButtons != null)
            {
                foreach (var btn in _retryButtons)
                {
                    if (btn != null)
                    {
                        btn.onClick.AddListener(OnPressRetry);
                    }
                }
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

            if (_retryButtons != null)
            {
                foreach (var btn in _retryButtons)
                {
                    if (btn != null)
                    {
                        btn.onClick.RemoveListener(OnPressRetry);
                    }
                }
            }
        }

        private void OnPressOneMore()
        {
            AudioManager.Instance?.PlayOneMore();
            _controller?.OnOneMoreAsync().Forget();
        }

        private void OnPressStop()
        {
            AudioManager.Instance?.PlayFire();
            _controller?.OnStopAsync().Forget();
        }

        private void OnPressRetry()
        {
            AudioManager.Instance?.PlayButtonClick();
            _controller?.OnRetryAsync().Forget();
        }

        private void BindViewModel()
        {
            // すべての UI を R3 へ購読させる
            if (_viewModel == null)
            {
                return;
            }

            _disposables.Add(_viewModel.RoundIndex.Subscribe(x => SetText(_roundText, $"Round {x}/5")));
            _disposables.Add(_viewModel.DeadCount.Subscribe(x => SetText(_deadText, $"{x}/2")));
            _disposables.Add(_viewModel.BulletCount.Subscribe(x => SetText(_bulletText, $"BULLET {x}/6")));
            _disposables.Add(_viewModel.Rank.Subscribe(x => SetText(_rankText, $"RANK {x}")));
            _disposables.Add(_viewModel.Multiplier.Subscribe(x => SetText(_multiplierText, $"Multi x{x:F2}")));
            _disposables.Add(_viewModel.CarryNextMultiplier.Subscribe(x => SetText(_carryNextText, $"NEXT x{x:F2}")));
            _disposables.Add(_viewModel.RoundScore.Subscribe(x => SetText(_roundScoreText, $"RoundScore {x}")));
            _disposables.Add(_viewModel.TotalScore.Subscribe(x => SetText(_totalScoreText, x.ToString())));
            _disposables.Add(_viewModel.State.Subscribe(OnStateChanged));
        }

        private void OnStateChanged(GameState state)
        {
            var isDecision = state == GameState.Decision;
            var isResult = state == GameState.Result;

            if (_oneMoreButton != null)
            {
                _oneMoreButton.interactable = isDecision;
                _oneMoreButton.gameObject.SetActive(!isResult);
            }

            if (_stopButton != null)
            {
                _stopButton.interactable = isDecision;
                _stopButton.gameObject.SetActive(!isResult);
            }

            // Result Windowの表示制御
            if (_resultWindow != null)
            {
                _resultWindow.SetActive(isResult);

                if (isResult)
                {
                    UpdateResultWindow();
                }
            }
        }

        private void UpdateResultWindow()
        {
            if (_viewModel == null)
            {
                return;
            }

            var totalScore = _viewModel.TotalScore.Value;
            SetText(_resultScoreText, $"{totalScore:N0}");
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
            target.text = value;
        }
    }
}
