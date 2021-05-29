using UnityEngine;

public class PlayerController : MonoBehaviour {
    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private float _speed = 1.0f;
    
    private void Update() {
        var isMoving = false;
        var direction = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) {
            direction += Vector2.up;
            isMoving = true;
        }

        if (Input.GetKey(KeyCode.S)) {
            direction += Vector2.down;
            isMoving = true;
        }

        if (Input.GetKey(KeyCode.A)) {
            direction += Vector2.left;
            isMoving = true;
        }

        if (Input.GetKey(KeyCode.D)) {
            direction += Vector2.right;
            isMoving = true;
        }

        if (isMoving) {
            _rigidbody2D.AddForce(direction * (_speed * Time.deltaTime));
        }
    }
}
