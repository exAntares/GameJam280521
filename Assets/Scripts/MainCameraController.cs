using System;
using UnityEngine;

public class MainCameraController : MonoBehaviour {
    [SerializeField] private Transform _target;

    private void Awake() {
        if (_target == null) {
            var findWithTag = GameObject.FindWithTag("Player");
            if(findWithTag != null) {
                _target = findWithTag.transform;
            }
        }
    }

    private void FixedUpdate() {
        if (!_target) {
            return;
        }
        
        var targetPosition = _target.position;
        var myTransform = transform;
        var transformPosition = myTransform.position;
        transformPosition.x = targetPosition.x;
        transformPosition.y = targetPosition.y;
        myTransform.position = transformPosition;
    }
}
