using Unity.Mathematics;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    [SerializeField] private SoundArea _runningAreaPrefab;
    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private float _speed = 1.0f;
    private float _instantiateCooldown = 0;
    private void Update() {
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
            direction += Vector2.up;
            isMoving = true;
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
            direction += Vector2.down;
            isMoving = true;
        }

        if (Input.GetKey(KeyCode.A)  || Input.GetKey(KeyCode.LeftArrow)) {
            direction += Vector2.left;
            isMoving = true;
        }

        if (Input.GetKey(KeyCode.D)  || Input.GetKey(KeyCode.RightArrow)) {
            direction += Vector2.right;
            isMoving = true;
        }

        if (isMoving) {
            var force = direction * (_speed * speedMultiplier * Time.deltaTime);
            if (force.x > 0) {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (force.x < 0) {
                transform.localScale = Vector3.one;
            }
            
            _rigidbody2D.AddForce(force);
        }
    }
}
