using System;
using HalfBlind.ScriptableVariables;
using Spine;
using Spine.Unity;
using Spine.Unity.AttachmentTools;
using Unity.Mathematics;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    [SerializeField] private SoundArea _runningAreaPrefab;
    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private float _speed = 1.0f;
    [SerializeField] private ScriptableGameEvent _onLose;
    [SerializeField] private ScriptableGameEvent _onWin;
    [SerializeField] private ScriptableGameEvent _onPanicAttack;
    [SerializeField] private AnimationReferenceAsset _idleAnimationReferenceAsset;
    [SerializeField] private AnimationReferenceAsset _walkAnimationReferenceAsset;
    [SerializeField] private AnimationReferenceAsset _runAnimationReferenceAsset;
    [SerializeField] private SkeletonAnimation _skeleton;

    private float _instantiateCooldown = 0;
    private bool _canMove = true;
    private float _panicAttackDuration = 0;

    private void Awake() {
        _onLose.AddListener(OnLose);
        _onWin.AddListener(OnWin);
        _onPanicAttack.AddListener(OnPanicAttack);
    }

    private void OnDestroy() {
        _onLose.RemoveListener(OnLose);
        _onWin.RemoveListener(OnWin);
        _onPanicAttack.RemoveListener(OnPanicAttack);        
    }

    private void OnPanicAttack() {
        _panicAttackDuration = 5.0f;
    }

    private void OnWin() {
        _canMove = false;
    }

    private void OnLose() {
        _canMove = false;
    }

    private void Update() {
        if (!_canMove) {
            return;
        }

        _panicAttackDuration -= Time.deltaTime;
        var isInPanic = _panicAttackDuration > 0;
        _skeleton.skeleton.SetSkin(isInPanic ? "panic_attack" : "normal");
        var isMoving = false;
        var speedMultiplier = 1.0f;
        var direction = Vector2.zero;
        _instantiateCooldown -= Time.deltaTime;
        
        if (Input.GetKey(KeyCode.LeftShift)) {
            speedMultiplier = 2.0f;
            if (_instantiateCooldown <= 0) {
                Instantiate(_runningAreaPrefab, transform.position, quaternion.identity);
                _instantiateCooldown = 1.0f;
            }
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
            if (isInPanic) {
                direction += Vector2.down;
            }
            else {
                direction += Vector2.up;
            }
            isMoving = true;
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
            if (isInPanic) {
                direction += Vector2.up;
            }
            else {
                direction += Vector2.down;
            }
            isMoving = true;
        }

        if (Input.GetKey(KeyCode.A)  || Input.GetKey(KeyCode.LeftArrow)) {
            if (isInPanic) {
                direction += Vector2.right;
            }
            else {
                direction += Vector2.left;
            }
            
            isMoving = true;
        }

        if (Input.GetKey(KeyCode.D)  || Input.GetKey(KeyCode.RightArrow)) {
            if (isInPanic) {
                direction += Vector2.left;
            }
            else {
                direction += Vector2.right;
            }

            isMoving = true;
        }

        if (isMoving) {
            SetAnimation(_skeleton.AnimationState, speedMultiplier > 1 ? _runAnimationReferenceAsset : _walkAnimationReferenceAsset, true);
            var force = direction * (_speed * speedMultiplier * Time.deltaTime);
            if (force.x > 0) {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (force.x < 0) {
                transform.localScale = Vector3.one;
            }
            
            _rigidbody2D.AddForce(force);
        }
        else {
            SetAnimation(_skeleton.AnimationState, _idleAnimationReferenceAsset, true);
        }
    }

    private static void SetAnimation(Spine.AnimationState animationState, AnimationReferenceAsset animation, bool loop) {
        if (animationState.Tracks.Items[0].Animation != animation.Animation) {
            animationState.SetAnimation(0, animation.Animation, loop);
        }
    }
}
