using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using IncrementalGame.Core;
using UnityEngine;

namespace IncrementalGame.Presentation
{
    public sealed class Prototype0Controller : MonoBehaviour
    {
        private const double ReloadSeconds = 0.65;
        private const double CollectorHiddenSeconds = 0.35;
        private const double ProjectileSpeedLogical = 900.0;
        private const double ProjectileRadiusLogical = 8.0;
        private const int MaximumContactsPerTick = 4;
        private static readonly SimulationBounds ProjectileBounds = new SimulationBounds(
            -100.0,
            LogicalSpace.Width + 100.0,
            -100.0,
            LogicalSpace.Height + 100.0);

        private static readonly Rect UpgradeButtonRect = new Rect(20f, 190f, 220f, 42f);
        private static readonly Rect LogButtonRect = new Rect(20f, 242f, 220f, 36f);
        private static readonly Rect UpgradePanelRect = new Rect(270f, 155f, 410f, 225f);

        [SerializeField] private Camera _gameCamera;
        [SerializeField] private CollectorTargetView _collectorView;
        [SerializeField] private Transform _gunTransform;
        [SerializeField] private LineRenderer _aimCone;
        [SerializeField] private PrototypeAudio _audio;
        [SerializeField] private int _sessionSeed = 20260828;

        private readonly List<ProjectileRuntime> _projectiles = new List<ProjectileRuntime>();
        private GameEconomy _economy;
        private ReloadState _reload;
        private CollectorState _collector;
        private DeterministicRandom _random;
        private IProjectileCollisionQuery _collisionQuery;
        private SimVector2 _lastValidAimDirection = new SimVector2(0.0, -1.0);
        private long _nextProjectileId;
        private int _missCount;
        private int _reloadRejectedCount;
        private int _reflectionCount;
        private bool _upgradePanelOpen;
        private int _floatingReward;
        private double _floatingRewardRemaining;
        private string _lastEvent = "READY";
        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _targetValueStyle;
        private GUIStyle _rewardStyle;

        public GameEconomy Economy => _economy;
        public bool UpgradePanelOpen => _upgradePanelOpen;
        public int ActiveProjectileCount => _projectiles.Count;
        public double SimulatedTimeSeconds { get; private set; }
        public uint SessionSeed => unchecked((uint)_sessionSeed);

        public void Configure(
            Camera gameCamera,
            CollectorTargetView collectorView,
            Transform gunTransform,
            LineRenderer aimCone,
            PrototypeAudio audio,
            int sessionSeed)
        {
            _gameCamera = gameCamera;
            _collectorView = collectorView;
            _gunTransform = gunTransform;
            _aimCone = aimCone;
            _audio = audio;
            _sessionSeed = sessionSeed;

            if (_economy != null)
            {
                _random = new DeterministicRandom(SessionSeed);
            }
        }

        private void Awake()
        {
            _gameCamera = _gameCamera != null ? _gameCamera : Camera.main;
            _collectorView = _collectorView != null
                ? _collectorView
                : FindFirstObjectByType<CollectorTargetView>();
            _audio = _audio != null ? _audio : GetComponent<PrototypeAudio>();
            _economy = new GameEconomy();
            _reload = new ReloadState(ReloadSeconds);
            _collector = new CollectorState(CollectorHiddenSeconds);
            _random = new DeterministicRandom(SessionSeed);
            _collisionQuery = new UnityProjectileCollisionQuery();
            Application.targetFrameRate = 120;
            Time.fixedDeltaTime = 1f / 60f;
            WriteLog($"session_start version={Application.version} commit={BuildMetadata.CommitHash} seed={SessionSeed}");
        }

        private void Update()
        {
            UpdateAimCone();

            if (Input.GetKeyDown(KeyCode.U))
            {
                SetUpgradePanelOpen(!_upgradePanelOpen);
            }

            if (Input.GetMouseButtonDown(0) && _gameCamera != null)
            {
                var mouse = Input.mousePosition;
                var pointerOverUi = IsScreenPointOverPrototypeUi(mouse);
                var world = _gameCamera.ScreenToWorldPoint(mouse);
                TryFireAtWorld(world, pointerOverUi);
            }
        }

        private void FixedUpdate()
        {
            SimulateTick(Time.fixedDeltaTime);
        }

        private void OnGUI()
        {
            EnsureGuiStyles();
            GUI.Label(new Rect(20f, 18f, 620f, 40f), "ONE BOARD SHOOTER — PROTOTYPE 0", _titleStyle);
            GUI.Label(
                new Rect(20f, 62f, 560f, 126f),
                $"Gold       {_economy.Gold:0}\n" +
                $"Lifetime   {_economy.LifetimeGold:0}\n" +
                $"Hits       {Math.Min(_economy.HitCount, 10)}/10\n" +
                $"Value Lv.  {_economy.CollectorValueLevel}\n" +
                $"Reload     {(_reload.IsReady ? "READY" : _reload.RemainingSeconds.ToString("0.00", CultureInfo.InvariantCulture) + "s")}",
                _labelStyle);

            DrawCollectorLabels();

            if (GUI.Button(UpgradeButtonRect, "UPGRADES  [U]"))
            {
                SetUpgradePanelOpen(true);
            }

            if (GUI.Button(LogButtonRect, "COPY LOG PATH"))
            {
                GUIUtility.systemCopyBuffer = GetLogPath();
                _lastEvent = "LOG PATH COPIED";
            }

            GUI.Label(new Rect(20f, Screen.height - 72f, 900f, 52f),
                $"v{Application.version}  commit {BuildMetadata.CommitHash}  seed {SessionSeed}\n" +
                $"Left click: fire   Aim at the cyan wall for reflected reward   Last: {_lastEvent}",
                _labelStyle);

            if (!_upgradePanelOpen)
            {
                return;
            }

            GUI.Box(UpgradePanelRect, "UPGRADE — SIMULATION PAUSED");
            GUI.Label(new Rect(295f, 205f, 350f, 65f),
                "Collector Value +1\nCost: 10 Gold\nDirect reward: 1 → 2", _labelStyle);

            var oldEnabled = GUI.enabled;
            GUI.enabled = _economy.Gold >= GameEconomy.FirstCollectorUpgradeCost &&
                          _economy.CollectorValueLevel == 0;
            if (GUI.Button(new Rect(295f, 285f, 165f, 48f), "PURCHASE"))
            {
                TryPurchaseFirstUpgrade();
            }

            GUI.enabled = oldEnabled;
            if (GUI.Button(new Rect(480f, 285f, 165f, 48f), "CLOSE  [U]"))
            {
                SetUpgradePanelOpen(false);
            }
        }

        public bool TryFireAtWorld(Vector2 worldAim, bool pointerOverUi)
        {
            if (pointerOverUi || _upgradePanelOpen || _gunTransform == null)
            {
                return false;
            }

            var originLogical = LogicalSpace.ToLogical(_gunTransform.position);
            var aimLogical = LogicalSpace.ToLogical(worldAim);
            var aimDelta = aimLogical - originLogical;
            var distance = aimDelta.Magnitude;
            var baseDirection = ResolveAimDirection(aimDelta, distance);

            if (!_reload.TryFire())
            {
                _reloadRejectedCount += 1;
                WriteLog($"fire_rejected reason=reload sim_time={SimulatedTimeSeconds:0.000}");
                return false;
            }

            var maximumSpread = AimCalculator.GetSpreadDegrees(distance);
            var offset = _random.NextSignedOffset(maximumSpread);
            var direction = AimCalculator.RotateDegrees(baseDirection, offset);
            SpawnProjectile(originLogical, direction * ProjectileSpeedLogical, 1);
            _audio?.PlayFire();
            _lastEvent = $"FIRE offset={offset:+0.00;-0.00;0.00}°";
            WriteLog(
                $"fire projectile={_nextProjectileId} offset_degrees={offset:0.000} " +
                $"sim_time={SimulatedTimeSeconds:0.000}");
            return true;
        }

        public void SimulateTick(double deltaSeconds)
        {
            if (_upgradePanelOpen)
            {
                return;
            }

            SimulatedTimeSeconds += deltaSeconds;
            _floatingRewardRemaining = Math.Max(0.0, _floatingRewardRemaining - deltaSeconds);
            if (_reload.Tick(deltaSeconds))
            {
                _audio?.PlayReady();
                _lastEvent = "RELOAD READY";
                WriteLog("reload_ready");
            }

            if (_collector.Tick(deltaSeconds))
            {
                _collectorView?.SetAvailable(true);
                _lastEvent = "COLLECTOR RESTORED";
                WriteLog("collector_restored");
            }

            SimulateProjectiles(deltaSeconds);
        }

        public void SetUpgradePanelOpen(bool isOpen)
        {
            _upgradePanelOpen = isOpen;
            _lastEvent = isOpen ? "UPGRADES OPEN" : "UPGRADES CLOSED";
        }

        public bool TryPurchaseFirstUpgrade()
        {
            var purchased = _economy.TryPurchaseFirstCollectorValueUpgrade();
            if (purchased)
            {
                _lastEvent = "COLLECTOR VALUE +1 PURCHASED";
                WriteLog("upgrade_purchased id=collector_value_1 cost=10");
            }

            return purchased;
        }

        public bool IsScreenPointOverPrototypeUi(Vector2 screenPoint)
        {
            var guiPoint = new Vector2(screenPoint.x, Screen.height - screenPoint.y);
            return UpgradeButtonRect.Contains(guiPoint) ||
                   LogButtonRect.Contains(guiPoint) ||
                   (_upgradePanelOpen && UpgradePanelRect.Contains(guiPoint));
        }

        public long SpawnProjectileForTest(SimVector2 position, SimVector2 velocity, int reflections)
        {
            return SpawnProjectile(position, velocity, reflections).State.Id;
        }

        public ProjectileState GetProjectileStateForTest(long projectileId)
        {
            for (var index = 0; index < _projectiles.Count; index += 1)
            {
                if (_projectiles[index].State.Id == projectileId)
                {
                    return _projectiles[index].State;
                }
            }

            return null;
        }

        private ProjectileRuntime SpawnProjectile(
            SimVector2 position,
            SimVector2 velocity,
            int reflections)
        {
            _nextProjectileId += 1;
            var state = new ProjectileState(
                _nextProjectileId,
                position,
                velocity,
                ProjectileRadiusLogical,
                reflections,
                _nextProjectileId,
                1.0);
            var viewObject = new GameObject($"Projectile {_nextProjectileId}");
            var shape = viewObject.AddComponent<PrototypeShapeView>();
            shape.Configure(new Color(1f, 0.78f, 0.18f), Vector2.one * 0.16f);
            viewObject.transform.position = LogicalSpace.ToWorld(position);
            var runtime = new ProjectileRuntime(state, viewObject);
            _projectiles.Add(runtime);
            return runtime;
        }

        private void SimulateProjectiles(double deltaSeconds)
        {
            Physics2D.SyncTransforms();

            for (var index = _projectiles.Count - 1; index >= 0; index -= 1)
            {
                var projectile = _projectiles[index];
                var result = ProjectileSimulation.Step(
                    projectile.State,
                    deltaSeconds,
                    _collisionQuery,
                    MaximumContactsPerTick,
                    ProjectileBounds);

                if (result.ReflectionCount > 0)
                {
                    _reflectionCount += result.ReflectionCount;
                    _lastEvent = "WALL REFLECTION";
                    WriteLog($"reflect projectile={projectile.State.Id} count={result.ReflectionCount}");
                }

                if (result.CollectorContact)
                {
                    ResolveCollectorContact(projectile.State);
                }

                if (result.ContactGuardReached)
                {
                    WriteLog($"projectile_contact_guard projectile={projectile.State.Id}");
                }

                if (result.Missed)
                {
                    _missCount += 1;
                    WriteLog(
                        $"miss projectile={projectile.State.Id} sim_time={SimulatedTimeSeconds:0.000}");
                }

                if (!projectile.State.Alive)
                {
                    Destroy(projectile.View);
                    _projectiles.RemoveAt(index);
                    continue;
                }

                projectile.View.transform.position = LogicalSpace.ToWorld(projectile.State.Position);
            }
        }

        private void ResolveCollectorContact(ProjectileState state)
        {
            if (!_collector.TryAcceptHit())
            {
                state.Alive = false;
                return;
            }

            var reward = _economy.ResolveCollectorHit(state.HasReflected);
            _floatingReward = reward;
            _floatingRewardRemaining = 0.6;
            _collectorView?.SetAvailable(false);
            _audio?.PlayHit();
            state.Alive = false;
            _lastEvent = $"COLLECTOR +{reward} GOLD";
            WriteLog(
                $"collector_hit projectile={state.Id} reflected={state.HasReflected} reward={reward} " +
                $"gold={_economy.Gold:0} lifetime={_economy.LifetimeGold:0}");
        }

        private void OnApplicationQuit()
        {
            WriteLog(
                $"session_end duration={SimulatedTimeSeconds:0.000} shots={_nextProjectileId} " +
                $"hits={_economy.HitCount} misses={_missCount} reload_rejected={_reloadRejectedCount} " +
                $"reflections={_reflectionCount} gold={_economy.Gold:0} " +
                $"lifetime={_economy.LifetimeGold:0}");
        }

        private void UpdateAimCone()
        {
            if (_aimCone == null || _gunTransform == null || _gameCamera == null)
            {
                return;
            }

            var mouseWorld = (Vector2)_gameCamera.ScreenToWorldPoint(Input.mousePosition);
            var originLogical = LogicalSpace.ToLogical(_gunTransform.position);
            var targetLogical = LogicalSpace.ToLogical(mouseWorld);
            var aimDelta = targetLogical - originLogical;
            var distance = aimDelta.Magnitude;
            var direction = ResolveAimDirection(aimDelta, distance);

            _aimCone.enabled = true;
            var maximumSpread = AimCalculator.GetSpreadDegrees(distance);
            var left = AimCalculator.RotateDegrees(direction, -maximumSpread);
            var right = AimCalculator.RotateDegrees(direction, maximumSpread);
            var originWorld = (Vector2)_gunTransform.position;
            const float lineLength = 4.2f;
            _aimCone.SetPosition(0, originWorld + LogicalSpace.DirectionToWorld(left) * lineLength);
            _aimCone.SetPosition(1, originWorld);
            _aimCone.SetPosition(2, originWorld + LogicalSpace.DirectionToWorld(right) * lineLength);
        }

        private SimVector2 ResolveAimDirection(SimVector2 aimDelta, double distance)
        {
            if (distance >= AimCalculator.NearDistance)
            {
                _lastValidAimDirection = aimDelta.Normalized;
            }

            return _lastValidAimDirection;
        }

        private void EnsureGuiStyles()
        {
            if (_titleStyle != null)
            {
                return;
            }

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.9f, 0.96f, 1f) }
            };
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                normal = { textColor = new Color(0.84f, 0.9f, 0.96f) }
            };
            _targetValueStyle = new GUIStyle(_labelStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.62f, 1f, 0.7f) }
            };
            _rewardStyle = new GUIStyle(_targetValueStyle)
            {
                fontSize = 20,
                normal = { textColor = new Color(1f, 0.82f, 0.28f) }
            };
        }

        private void DrawCollectorLabels()
        {
            if (_collectorView == null || _gameCamera == null)
            {
                return;
            }

            var screen = _gameCamera.WorldToScreenPoint(_collectorView.transform.position);
            var guiY = Screen.height - screen.y;
            var baseGold = 1 + _economy.CollectorValueLevel;
            GUI.Label(
                new Rect(screen.x - 60f, guiY + 34f, 120f, 30f),
                $"{baseGold} GOLD",
                _targetValueStyle);

            if (_floatingRewardRemaining > 0.0)
            {
                GUI.Label(
                    new Rect(screen.x - 60f, guiY - 64f, 120f, 34f),
                    $"+{_floatingReward}",
                    _rewardStyle);
            }
        }

        private static void WriteLog(string message)
        {
            Debug.Log($"[Prototype0] {message}");
            try
            {
                var path = GetLogPath();
                File.AppendAllText(path, $"{DateTime.UtcNow:O} {message}{Environment.NewLine}");
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Prototype0] Local log unavailable: {exception.Message}");
            }
        }

        private static string GetLogPath() =>
            Path.Combine(Application.persistentDataPath, "prototype0.log");

        private sealed class ProjectileRuntime
        {
            public ProjectileRuntime(ProjectileState state, GameObject view)
            {
                State = state;
                View = view;
            }

            public ProjectileState State { get; }
            public GameObject View { get; }
        }
    }
}
