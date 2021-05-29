using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

public class LibrarianTargetTrigger : MonoBehaviour {
    [SerializeField] private LibrarianController _librarianController;
    [SerializeField] private string _otherTag;
    [SerializeField] private bool _setAsPlayer;
    
    void Start() {
        this.GetAsyncTriggerStay2DTrigger()
            .ForEachAsync(OnStay2D);
    }

    private void OnStay2D(Collider2D obj) {
        if (obj.CompareTag(_otherTag)) {
            if (_setAsPlayer) {
                _librarianController.SetPlayerTarget(obj.transform);
            }
            else {
                _librarianController.SetTarget(obj.transform);
            }
        }
    }
}
