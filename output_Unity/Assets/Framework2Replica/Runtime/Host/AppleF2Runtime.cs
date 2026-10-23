using System;
using System.Threading;
using System.Threading.Tasks;
using ArrowOut.Runtime;
using MahjongSlide.Withdrawal;
using UnityEngine;

namespace Apple.Framework2
{
    // Apple reports game/ad events. The imported runtime owns money, tasks and rewards.
    public sealed class AppleF2Runtime : IDisposable
    {
        internal static Framework2ProductConfig StartupConfiguration;
        public static Action OpenHostSettings;
        public static Action OpenHostTerms;
        public static Action OpenHostPrivacy;
        public static Action<string, object[]> Analytics;
        private static AppleF2Runtime activeInstance;
        private MahjongFramework2Runtime runtime;
        private GameObject root;
        private bool disposed;
        private bool winPending;
        public bool Ready => !disposed && runtime != null && runtime.Ready;
        public bool HasOpenPanels => Ready && runtime.HasOpenPanels;
        public bool HasPendingGameplayPresentation => Ready && runtime.HasPendingGameplayPresentation;
        public int LinearLevelNumber => runtime != null ? runtime.LinearLevelNumber : 1;
        public long BalanceMinorUnits => runtime != null ? runtime.BalanceMinorUnits : 0L;
        public float GetGameplayTopInset(float height) => runtime != null ? runtime.GetGameplayTopInset(height) : 0f;
        public event Action<int> HintGranted;
        public static async Task<Framework2ProductConfig> LoadProductConfigAsync()
        {
            var source = new TaskCompletionSource<TextAsset>();
            ResourceRequest request = Resources.LoadAsync<TextAsset>("Framework2Replica/ProductConfig");
            if (request.isDone) source.SetResult(request.asset as TextAsset);
            else request.completed += _ => source.TrySetResult(request.asset as TextAsset);
            TextAsset json = await source.Task;
            if (json == null) throw new InvalidOperationException("Original Framework 2 product configuration is missing.");
            Framework2ProductConfig config = JsonUtility.FromJson<Framework2ProductConfig>(json.text);
            config.economy.ValidateOrThrow();
            return config;
        }
        public static async Task<AppleF2Runtime> CreateAsync(Transform parent, Framework2ProductConfig config,
            Action<string, Action<bool>> rewarded, Action<string, Action<bool>> interstitial,
            Func<bool> gameplayActive, Func<DateTime> localNow = null, Func<string> region = null,
            string storageNamespace = "DJS.Apple.B.", CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(storageNamespace)) throw new ArgumentException("A separate Framework 2 storage namespace is required.", nameof(storageNamespace));
            if (config == null) config = await LoadProductConfigAsync();
            cancellationToken.ThrowIfCancellationRequested();
            var instance = new AppleF2Runtime();
            activeInstance = instance;
            WZSDK.Framework2Host.ApplicationScope = storageNamespace.EndsWith(".", StringComparison.Ordinal) ? storageNamespace : storageNamespace + ".";
            StartupConfiguration = config;
            WaterSort.WaterSortCurrencyService.ApplyDatabase(config.currencyDatabase);
            // Cancellation is delivered on the captured Unity context so that
            // shutdown also covers a factory still awaiting its first UI load.
            using var cancellation = cancellationToken.Register(instance.Dispose, useSynchronizationContext: true);
            try
            {
                instance.root = await AddressableAssetService.InstantiateAsync("integration/f2runtime", parent, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                if (instance.root == null) throw new InvalidOperationException("Original Framework 2 runtime prefab is missing.");
                MahjongFrameworkResources.SetActive(instance.root.GetComponent<MahjongFrameworkResources>());
                instance.runtime = instance.root.GetComponent<MahjongFramework2Runtime>();
                if (instance.runtime == null) throw new InvalidOperationException("Original Framework 2 runtime component is missing.");
                await instance.runtime.InitializeAsync(parent, config,
                    instance.GuardAdCallback(rewarded), instance.GuardAdCallback(interstitial), localNow,
                    region ?? (() => "US"), gameplayActive, null, null, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                if (!instance.Ready) throw new OperationCanceledException("Framework 2 was unloaded during initialization.");
                instance.runtime.HintGranted += instance.HandleHint;
                return instance;
            }
            catch { instance.Dispose(); throw; }
            finally { if (activeInstance == instance) StartupConfiguration = null; }
        }
        private void HandleHint(int count) => HintGranted?.Invoke(count);
        private Action<string, Action<bool>> GuardAdCallback(Action<string, Action<bool>> show)
        {
            if (show == null) return null;
            return (placement, completed) =>
            {
                if (disposed) return;
                bool answered = false;
                show(placement, success =>
                {
                    if (disposed || answered) return;
                    answered = true;
                    completed?.Invoke(success);
                });
            };
        }
        public void OnBegin(int commercialLevel)
        {
            if (!Ready) return;
            // Replaying an Apple scene does not advance or rewind source progression.
            runtime.OnLevelStarted(runtime.LinearLevelNumber);
            runtime.OnRoundCompleted(false, commercialLevel);
            runtime.BeginWithdrawalGuide();
        }
        public void OnTriple(Vector2? screenPosition = null) { if (Ready) runtime.OnPairEliminated(screenPosition); }
        public void OnWin(Action nextLevel)
        {
            if (!Ready) throw new InvalidOperationException("Framework 2 is not initialized.");
            if (winPending) return;
            winPending = true;
            runtime.ShowSettlementReward(() => { winPending = false; if (!disposed) nextLevel?.Invoke(); });
        }
        public void Tick(float deltaTime) { if (Ready) runtime.TickGameplay(deltaTime); }
        public void OpenWithdrawal() { if (Ready) runtime.OpenWithdrawal(); }
        public void Save() { WZSDK.SPlayerPrefs.Save(); UnityEngine.PlayerPrefs.Save(); }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            winPending = false;
            if (runtime != null) { runtime.HintGranted -= HandleHint; runtime.Shutdown(); }
            if (root != null) root.GetComponentInChildren<WZSDK.UserDataManager>(true)?.Dispose();
            Save();
            if (root != null) { root.SetActive(false); UnityEngine.Object.Destroy(root); }
            runtime = null; root = null;
            if (activeInstance == this)
            {
                activeInstance = null;
                StartupConfiguration = null;
                WZSDK.GameApp.viewManager = null;
                WZSDK.GameApp.BasePanel = null;
                MahjongFrameworkResources.SetActive(null);
                AddressableAssetService.ReleaseCache();
            }
        }
    }
}
