using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Pathfinding;
using UnityEngine;

public class LibrarianController : MonoBehaviour {
    [SerializeField] private Path _path;
    [SerializeField] private Seeker _seeker;
    [SerializeField] private Rigidbody2D _rigidbody2D;
    //[SerializeField] private Transform _target;
    [SerializeField] private float _speed = 1.0f;

    public Vector3 _targetPosition;

    public void SetTargetPosition(Vector3 targetPos) => _targetPosition = targetPos;

    // Start is called before the first frame update
    private void Start() {
        _targetPosition = transform.position;
        GetPath().Forget();
        UniTaskAsyncEnumerable.EveryUpdate().ForEachAsync(_ => {
            if (_path != null && !_path.error) {
                if (_path.vectorPath.Count > 1) {
                    var target = _path.vectorPath[1];
                    var distance = target - transform.position;
                    if (distance.sqrMagnitude <= 0.1f) {
                        return;
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
                    GetPath().Forget();
                }
            }
        }, this.GetCancellationTokenOnDestroy());
    }

    private async UniTaskVoid GetPath() {
        _path = await GetPathAsync();
        GetPath().Forget();
    }

    private async UniTask<Path> GetPathAsync() {
        var taskSource = new UniTaskCompletionSource<Path>();
        void OnCalculatePath(Path p) {
            taskSource.TrySetResult(p);
        }

        _seeker.StartPath(transform.position, _targetPosition, OnCalculatePath);
        return await taskSource.Task;
    }
}
