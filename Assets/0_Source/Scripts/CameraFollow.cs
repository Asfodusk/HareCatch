using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _defaultFaceHeight = 1.6f;
    [SerializeField] private float _defaultDistance = 0.3f;
    [SerializeField] private float _smoothSpeed = 5f;
    
    private float _currentFaceHeight;
    private float _currentDistance;
    
    // Публичные свойства
    public Transform Target 
    { 
        get => _target;
        set => SetTarget(value);
    }
    
    public float SmoothSpeed
    {
        get => _smoothSpeed;
        set => _smoothSpeed = Mathf.Max(0.1f, value);
    }
    
    private void Awake()
    {
        ResetToDefaultParameters();
    }
    
    private void LateUpdate()
    {
        if (_target == null) return;
        
        CheckSpecialTargetParameters();
        
        Vector3 facePosition = CalculateFacePosition();
        Vector3 desiredPosition = CalculateDesiredPosition(facePosition);
        
        ApplySmoothMovement(desiredPosition);
        LookAtFace(facePosition);
    }
    
    private void CheckSpecialTargetParameters()
    {
        if (_target.name == "Babka")
        {
            _currentFaceHeight = -0.04f;
            _currentDistance = 0.22f;
        }
        else
        {
            ResetToDefaultParameters();
        }
    }
    
    private void ResetToDefaultParameters()
    {
        _currentFaceHeight = _defaultFaceHeight;
        _currentDistance = _defaultDistance;
    }
    
    private Vector3 CalculateFacePosition()
    {
        return _target.position + Vector3.up * _currentFaceHeight;
    }
    
    private Vector3 CalculateDesiredPosition(Vector3 facePosition)
    {
        Vector3 desiredPosition;
        
        if (_target.CompareTag("Right"))
        {
            desiredPosition = facePosition + new Vector3(_currentDistance, 0, 0);
        }
        else
        {
            desiredPosition = facePosition + new Vector3(-_currentDistance, 0, 0);
        }
        
        return desiredPosition;
    }
    
    private void ApplySmoothMovement(Vector3 targetPosition)
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, _smoothSpeed * Time.deltaTime);
    }
    
    private void LookAtFace(Vector3 facePosition)
    {
        transform.LookAt(facePosition);
    }
    
    public void SetTarget(Transform newTarget)
    {
        _target = newTarget;
    
        if (newTarget != null)
        {
            Debug.Log($"CameraFollow: Цель изменена на {newTarget.name}");
        }
        else
        {
            Debug.Log("CameraFollow: Цель сброшена.");
        }
    }

}