using System;
using UnityEngine;
using Unity.Cinemachine;

[ExecuteInEditMode]
public class CinemachineParallax : CinemachineExtension
{
    [Serializable]
    public class ParallaxLayer
    {
        public Transform transform;
        [Range(0f, 2f)] public float parallaxFactor;
        [Range(0f, 0.3f)] public float smoothTime = 0.0f;
        public Vector3 offset;
        
        public Vector3 startPos;
        [HideInInspector] public Vector3 velocity;
    }
    
    [SerializeField] private ParallaxLayer[] _layers;
    
    private Vector3 _lastCameraPos;
    private bool _initialized;
    
    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }
    
    private void Initialize()
    {
        if (_initialized) return;
        
        foreach (var layer in _layers)
        {
            if (layer.transform != null)
                layer.startPos = layer.transform.position;
        }
        _initialized = true;
    }
    
    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Finalize) return;
        
        Initialize();
        
        Vector3 cameraPos = state.RawPosition;
        
        foreach (var layer in _layers)
        {
            if (layer.transform == null) continue;
            
            Vector3 targetPos = new Vector3(
                layer.startPos.x + cameraPos.x * layer.parallaxFactor,
                layer.startPos.y + cameraPos.y * layer.parallaxFactor,
                layer.transform.position.z
            );
            
            if (layer.smoothTime > 0 && Application.isPlaying)
            {
                layer.transform.position = Vector3.SmoothDamp(
                    layer.transform.position,
                    targetPos,
                    ref layer.velocity,
                    layer.smoothTime
                );
            }
            else
            {
                layer.transform.position = targetPos;
            }

            layer.transform.position += layer.offset;
        }
    }
}