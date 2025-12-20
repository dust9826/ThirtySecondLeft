using System;
using System.Collections;
using UnityEngine;
public class GhostTrail : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _targetRenderer;
    [SerializeField] private float _ghostInterval = 0.05f;
    [SerializeField] private float _ghostDuration = 0.3f;
    [SerializeField] private Color _ghostColor = new Color(1f, 1f, 1f, 0.5f);
    
    private bool _isTrailActive;

    private void Awake()
    {
        if(_targetRenderer == null) _targetRenderer = GetComponent<SpriteRenderer>();
    }

    public void StartTrail(float duration = 0.3f)
    {
        if (!_isTrailActive)
            StartCoroutine(SpawnGhosts(duration));
    }
    
    private IEnumerator SpawnGhosts(float duration)
    {
        _isTrailActive = true;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            SpawnGhost();
            yield return new WaitForSeconds(_ghostInterval);
            elapsed += _ghostInterval;
        }
        
        _isTrailActive = false;
    }
    
    private void SpawnGhost()
    {
        GameObject ghost = new GameObject("Ghost");
        ghost.transform.position = _targetRenderer.transform.position;
        ghost.transform.rotation = _targetRenderer.transform.rotation;
        ghost.transform.localScale = _targetRenderer.transform.lossyScale;
        
        SpriteRenderer sr = ghost.AddComponent<SpriteRenderer>();
        sr.sprite = _targetRenderer.sprite;
        sr.color = _ghostColor;
        sr.sortingLayerName = _targetRenderer.sortingLayerName;
        sr.sortingOrder = _targetRenderer.sortingOrder - 1;
        
        StartCoroutine(FadeGhost(sr, _ghostDuration));
    }
    
    private IEnumerator FadeGhost(SpriteRenderer sr, float duration)
    {
        float elapsed = 0f;
        Color startColor = sr.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / duration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
        
        Destroy(sr.gameObject);
    }
}