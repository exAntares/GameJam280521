using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

public class LibrarianTargetTrigger : MonoBehaviour {
    [SerializeField] private LibrarianController _librarianController;
    [SerializeField] private string _otherTag;
    
    void Start() {
        this.GetAsyncTriggerStay2DTrigger()
            .ForEachAsync(OnStay2D);
        this.GetAsyncTriggerExit2DTrigger().ForEachAsync(OnLeave2D);
    }

    private void OnStay2D(Collider2D obj) {
        if (obj.CompareTag(_otherTag)) {
            _librarianController.SetPlayerTarget(obj.transform);
        }
    }
    
    private void OnLeave2D(Collider2D obj) {
        if (obj.CompareTag(_otherTag)) {
            _librarianController.SetPlayerTarget(null);
        }
    }
}
