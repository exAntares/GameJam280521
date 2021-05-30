using HalfBlind.ScriptableVariables;
using UnityEngine;

public class PathfinderRecalculator : MonoBehaviour {
    [SerializeField] private ScriptableGameEvent _recalculate;


    // Start is called before the first frame update
    private void Start() {
        _recalculate.AddListener(OnRecalculate);
    }

    private static void OnRecalculate() {
        AstarPath.active.Scan();
    }

    private void OnDestroy() {
        _recalculate.RemoveListener(OnRecalculate);
    }
}
