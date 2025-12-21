using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private Transform _camera;
    [SerializeField] private float _parallaxFactor = 0.5f;  // 0 = 고정, 1 = 카메라와 동일

    [Header("Smoothing")]
    [SerializeField] private float _smoothTime = 0.1f;  // 부드러움 정도
    
    private Vector3 _startPos;
    private Vector3 _cameraStartPos;
    private Vector3 _velocity = Vector3.zero;
    private Vector3 _targetPos;
    
    private void Start()
    {
        _startPos = transform.position;
        _cameraStartPos = _camera.position;
    }
    
    private void LateUpdate()
    {
        // 카메라 이동량 계산
        Vector3 cameraDelta = _camera.position - _cameraStartPos;
        
        // 목표 위치
        _targetPos = _startPos + new Vector3(
            cameraDelta.x * _parallaxFactor,
            cameraDelta.y * _parallaxFactor,
            0
        );
        
        // 부드럽게 이동
        transform.position = Vector3.SmoothDamp(
            transform.position,
            _targetPos,
            ref _velocity,
            _smoothTime
        );
    }
}
