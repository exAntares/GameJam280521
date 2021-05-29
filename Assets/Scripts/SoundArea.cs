using UnityEngine;

public class SoundArea : MonoBehaviour {
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private AnimationCurve _easing;
    [SerializeField] private Vector2 _scaleRemap = new Vector2(0, 15);
    [SerializeField] private Vector2 _alphaRemap = Vector2.right;
    [SerializeField] private float _duration = 1.0f;
    
    private float t = 0;
    
    private void Update() {
        var maxSize = 15.0f;
        t += Time.deltaTime;
        t = Mathf.Clamp(t, 0, _duration);
        var lerp = _easing.Evaluate(t / _duration);
        var scale = Mathf.Lerp(_scaleRemap.x, _scaleRemap.y, t);
        var alpha = Mathf.Lerp(_alphaRemap.x, _alphaRemap.y, t);
        transform.localScale = Vector3.one * scale;
        var spriteRendererColor = _spriteRenderer.color;
        spriteRendererColor.a = alpha;
        _spriteRenderer.color = spriteRendererColor;
        if (lerp >= 1.0f) {
            Destroy(gameObject);
        }
    }
}
