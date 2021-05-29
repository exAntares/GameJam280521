using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

public class LibrarianTargetTrigger : MonoBehaviour {
    [SerializeField] private LibrarianController _librarianController;
    [SerializeField] private string _otherTag;
    
    void Start() {
        this.GetAsyncCollisionStay2DTrigger().ForEachAsync(OnCollisionStay2D);
        this.GetAsyncCollisionExit2DTrigger().ForEachAsync(OnCollisionLeave2D);
    }

    private void OnCollisionStay2D(Collision2D obj) {
        if (obj.otherCollider.CompareTag(_otherTag)) {
            _librarianController.SetPlayerTarget(obj.otherCollider.transform);
        }
    }
    
    private void OnCollisionLeave2D(Collision2D obj) {
        if (obj.otherCollider.CompareTag(_otherTag)) {
            _librarianController.SetPlayerTarget(null);
        }
    }
}
