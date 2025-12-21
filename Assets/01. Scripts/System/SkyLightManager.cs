using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SkyLightManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _player;
    [SerializeField] private Light2D _light;
    
    [Header("Position Range")]
    [SerializeField] private float _startX = 0f;    // 시작 위치 (밝음)
    [SerializeField] private float _endX = 100f;    // 끝 위치 (어두움)
    
    [Header("Intensity Range")]
    [SerializeField] private float _maxIntensity = 1f;   // 시작 밝기
    [SerializeField] private float _minIntensity = 0.1f; // 끝 밝기
    
    private void Update()
    {
        float t = Mathf.InverseLerp(_startX, _endX, _player.position.x);
        _light.intensity = Mathf.Lerp(_maxIntensity, _minIntensity, t);
    }
}
