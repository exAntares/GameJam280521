using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using HalfBlind.ScriptableVariables;
using Pathfinding;
using Spine.Unity;
using UnityEngine;

public class LibrarianController : MonoBehaviour {
    [SerializeField] private Path _path;
    [SerializeField] private Seeker _seeker;
    [SerializeField] private Rigidbody2D _rigidbody2D;
    
    [SerializeField] private SkeletonAnimation _skeleton;
    [SerializeField] private AnimationReferenceAsset _lookAroundStartAnimation;
    [SerializeField] private AnimationReferenceAsset _lookAroundLoopAnimation;
    [SerializeField] private AnimationReferenceAsset _lookAroundEndAnimation;
    [SerializeField] private AnimationReferenceAsset _walk;
    [SerializeField] private GlobalFloat _hunting;
    
    [SerializeField] private float _huntingDiminishing = 0.5f;
    [SerializeField] private float _speed = 1.0f;

    private Vector3 _targetPosition;
    private int targetWaypointIndex = 1;
    private Transform _targetPlayer;
    private Transform _targetSound;

    public void SetTargetPosition(Vector3 targetPos) => _targetPosition = targetPos;

    // Start is called before the first frame update
    private void Start() {
        _targetPosition = transform.position;
        GetPath().Forget();
        UniTaskAsyncEnumerable.EveryUpdate().ForEachAsync(_ => {
            _hunting.Value = Mathf.Clamp01(_hunting.Value - (Time.deltaTime * _huntingDiminishing));
            if (_hunting.Value <= 0.5f) {
                _targetPlayer = null;
            }
            if (_path != null && !_path.error) {
                if (targetWaypointIndex < _path.vectorPath.Count) {
                    var target = _path.vectorPath[targetWaypointIndex];
                    var distance = target - transform.position;
                    if (distance.sqrMagnitude <= 0.1f) {
                        targetWaypointIndex++;
                        return;
                    }

                    if (_skeleton.AnimationState.Tracks.Items[0].Animation != _walk.Animation && _skeleton.AnimationState.Tracks.Items[0].Animation != _lookAroundEndAnimation.Animation) {
                        _skeleton.AnimationState.SetAnimation(0, _lookAroundEndAnimation.Animation, false);
                        _skeleton.AnimationState.AddAnimation(0, _walk.Animation, true, _lookAroundEndAnimation.Animation.Duration);
                    }
                    var directionNormalized = distance.normalized;
                    var force = directionNormalized * (_speed * Time.deltaTime);
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
                    if (_skeleton.AnimationState.Tracks.Items[0].Animation != _lookAroundStartAnimation.Animation && _skeleton.AnimationState.Tracks.Items[0].Animation != _lookAroundLoopAnimation.Animation) {
                        _skeleton.AnimationState.SetAnimation(0, _lookAroundStartAnimation.Animation, false);
                        _skeleton.AnimationState.AddAnimation(0, _lookAroundLoopAnimation.Animation, true, _lookAroundStartAnimation.Animation.Duration);
                    }
                }
            }
        }, this.GetCancellationTokenOnDestroy());
    }

    private async UniTaskVoid GetPath() {
        _path = await GetPathAsync();
        targetWaypointIndex = 1;
        await UniTask.Delay(TimeSpan.FromSeconds(1.0f));
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
            targetPosition = _targetSound.position;
        }
        
        _seeker.StartPath(transform.position, targetPosition, OnCalculatePath);
        return await taskSource.Task;
    }

    public void SetPlayerTarget(Transform target) {
        _hunting.Value = 1.0f;
        _targetPlayer = target;
    }

    public void SetTarget(Transform target) {
        _hunting.Value = Mathf.Clamp01(_hunting.Value + 0.1f);
        _targetSound = target;
    }
}
