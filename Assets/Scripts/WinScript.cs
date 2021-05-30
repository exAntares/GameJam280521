using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using HalfBlind.ScriptableVariables;
using UnityEngine;

public class WinScript : MonoBehaviour {
    [SerializeField] private ScriptableGameEvent _onWin;
    
    private void Start() {
        this.GetAsyncTriggerEnter2DTrigger()
            .ForEachAsync(OnTriggerEnter2D);
    }

    private void OnTriggerEnter2D(Collider2D obj) {
        if (obj.CompareTag("Player")) {
            Debug.Log("Player Won!!");
            _onWin.SendEvent();
        }
    }
}
