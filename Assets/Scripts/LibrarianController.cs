using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using HalfBlind.ScriptableVariables;
using Pathfinding;
using Spine;
using Spine.Unity;
using Unity.Mathematics;
using UnityEngine;
using Event = Spine.Event;

public class LibrarianController : MonoBehaviour {
    [SerializeField] private Path _path;
    [SerializeField] private Seeker _seeker;
    [SerializeField] private Rigidbody2D _rigidbody2D;
    
    [SerializeField] private SkeletonAnimation _skeleton;
    [SerializeField] private AnimationReferenceAsset _lookAroundStartAnimation;
    [SerializeField] private AnimationReferenceAsset _lookAroundLoopAnimation;
    [SerializeField] private AnimationReferenceAsset _lookAroundEndAnimation;
    [SerializeField] private AnimationReferenceAsset _huntStartAnimation;
    [SerializeField] private AnimationReferenceAsset _huntloopAnimation;
    [SerializeField] private AnimationReferenceAsset _walk;
    [SerializeField] private GlobalFloat _hunting;
    [SerializeField] private float _speedMultiplierHunting = 1.5f;
    
    [SerializeField] private float _huntingDiminishing = 0.5f;
    [SerializeField] private float _speed = 1.0f;

    [SerializeField] private ScriptableGameEvent _onLose;
    [SerializeField] private ScriptableGameEvent _onWin;
    [SerializeField] private GameObject _markerPrefab;

    private GameObject MarketInstance;
    private Vector3 _targetPosition;
    private int targetWaypointIndex = 1;
    private Transform _targetPlayer;
    private Vector3? _targetSound;
    private Transform _player;
    private bool _gameEnded = false;

    private void Awake() {
        _onLose.AddListener(OnLose);
        _onWin.AddListener(OnWin);
    }
    
    private void OnWin() {
        _gameEnded = true;
    }

    private void OnLose() {
        _gameEnded = true;
    }
    
    public void SetTargetPosition(Vector3 targetPos) => _targetPosition = targetPos;

    // Start is called before the first frame update
    private void Start() {
        // The method needs the correct signature.
        _skeleton.AnimationState.Event += HandleEvent;
        _player = FindObjectOfType<PlayerController>().transform;
        _targetPosition = transform.position;
        GetPath().Forget();
        UniTaskAsyncEnumerable.EveryUpdate().ForEachAsync(_ => {
            if (_gameEnded) {
                return;
            }
            
            var speedMultipler = 1.0f;
            _hunting.Value = Mathf.Clamp01(_hunting.Value - (Time.deltaTime * _huntingDiminishing));
            if (_hunting.Value <= 0.5f) {
                _targetPlayer = null;
            }

            if (_targetPlayer != null) {
                speedMultipler = _speedMultiplierHunting;
                var distanceToPlayer = Vector3.Distance(transform.position, _targetPlayer.position);
                if (distanceToPlayer <= 1.4f) {
                    _skeleton.AnimationState.SetAnimation(0, _lookAroundLoopAnimation.Animation, true);
                    Debug.Log("Player Lost!!!");
                    _hunting.Value = 0;
                    _onLose.SendEvent();
                    return;
                }
            }

            if (_targetSound != null) {
                speedMultipler = _speedMultiplierHunting;
            }
            
            if (_path != null && !_path.error) {
                if (targetWaypointIndex < _path.vectorPath.Count) {
                    var target = _path.vectorPath[targetWaypointIndex];
                    var position = transform.position;
                    var distance = target - position;
                    var distanceToTarget = Vector3.Distance(position, target);
                    if (distanceToTarget <= 0.4f) {
                        targetWaypointIndex++;
                        return;
                    }

                    if (_targetPlayer != null) {
                        PlayHuntAnimation();
                    }
                    else {
                        PlayWalkAnimation();
                    }
                    
                    var directionNormalized = distance.normalized;
                    var force = directionNormalized * (_speed * speedMultipler * Time.deltaTime);
                    if (force.magnitude > 1) {
                        if (force.x > 0) {
                            transform.localScale = Vector3.one;
                        }
                        if (force.x < 0) {
                            transform.localScale = new Vector3(-1, 1, 1);
                        }
                        
                        _rigidbody2D.AddForce(force);
                    }
                }
                else {
                    _targetSound = null;
                    if (MarketInstance != null) {
                        Destroy(MarketInstance);
                    }
                    if (_skeleton.AnimationState.Tracks.Items[0].Animation != _lookAroundStartAnimation.Animation && _skeleton.AnimationState.Tracks.Items[0].Animation != _lookAroundLoopAnimation.Animation) {
                        _skeleton.AnimationState.SetAnimation(0, _lookAroundStartAnimation.Animation, false);
                        _skeleton.AnimationState.AddAnimation(0, _lookAroundLoopAnimation.Animation, true, 0.0f);
                    }
                }
            }
        }, this.GetCancellationTokenOnDestroy());
    }

    private void PlayWalkAnimation() {
        if (_skeleton.AnimationState.Tracks.Items[0].Animation != _walk.Animation && _skeleton.AnimationState.Tracks.Items[0].Animation != _lookAroundEndAnimation.Animation) {
            _skeleton.AnimationState.SetAnimation(0, _lookAroundEndAnimation.Animation, false);
            _skeleton.AnimationState.AddAnimation(0, _walk.Animation, true, 0.0f);
        }
    }

    private void PlayHuntAnimation() {
        if (_skeleton.AnimationState.Tracks.Items[0].Animation != _huntStartAnimation.Animation && _skeleton.AnimationState.Tracks.Items[0].Animation != _huntloopAnimation.Animation) {
            _skeleton.AnimationState.SetAnimation(0, _huntStartAnimation.Animation, false);
            _skeleton.AnimationState.AddAnimation(0, _huntloopAnimation.Animation, true, 0.0f);
        }
    }
    
    private void HandleEvent(TrackEntry trackEntry, Event e) {
        if (string.CompareOrdinal(e.Data.Name, "sound") != 0) {
            return;
        }
        if (!AudioContainer.Instance.PlayAudio(e.String, transform.position)) {
            Debug.LogWarning($"Failed to play sound on track {trackEntry.Animation.Name} '{e.String}'");
        }
    }

    private async UniTaskVoid GetPath() {
        _path = await GetPathAsync();
        targetWaypointIndex = 1;
        await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: this.GetCancellationTokenOnDestroy());
        GetPath().Forget();
    }

    private async UniTask<Path> GetPathAsync() {
        var taskSource = new UniTaskCompletionSource<Path>();
        void OnCalculatePath(Path p) {
            taskSource.TrySetResult(p);
        }

        var targetPosition = _targetPosition;
        if (_targetPlayer != null) {
            targetPosition = _targetPlayer.position;
        } else if (_targetSound != null) {
            targetPosition = _targetSound.Value;
        }
        
        _seeker.StartPath(transform.position, targetPosition, OnCalculatePath);
        return await taskSource.Task;
    }

    public void SetPlayerTarget(Transform target) {
        if (_gameEnded) {
            return;
        }
        _hunting.Value = 1.0f;
        _targetPlayer = target;
    }

    public void SetTarget(Transform target) {
        if (_gameEnded) {
            return;
        }
        
        _hunting.Value = Mathf.Clamp01(_hunting.Value + 0.1f);
        if (_hunting.Value > 0.5f) {
            _targetPlayer = _player;
        }

        _targetSound = target.position;
        if (MarketInstance != null) {
            Destroy(MarketInstance);
        }

        MarketInstance = Instantiate(_markerPrefab, target.position, quaternion.identity);
    }
}
