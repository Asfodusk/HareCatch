using UnityEngine;
using Yarn.Unity;

public class MouseClickRaycast : MonoBehaviour
{
    [SerializeField] private CameraFollow _cameraFollow;
    [SerializeField] private DialogueRunner _dialogueRunner;
    [SerializeField] private string _dialogueNodeName = "FirstD";
    
    private InMemoryVariableStorage _variableStorage;
    private bool _dialogueStarted = false;
    
    private Vector3 _savedPosition;
    private Quaternion _savedRotation;
    private Transform _savedParent;
    
    private void Start()
    {
        if (_cameraFollow == null)
        {
            _cameraFollow = GetComponent<CameraFollow>();
            if (_cameraFollow == null)
            {
                Debug.LogError("MouseClickRaycast: Не найден компонент CameraFollow.");
            }
        }
        
        if (_dialogueRunner == null)
        {
            _dialogueRunner = FindObjectOfType<DialogueRunner>();
        }
        
        if (_dialogueRunner != null)
        {
            _dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
            _variableStorage = _dialogueRunner.GetComponent<InMemoryVariableStorage>();
            if (_variableStorage == null)
            {
                _variableStorage = FindObjectOfType<InMemoryVariableStorage>();
            }
        }
    }
    
    private void Update()
    {
        if (_dialogueStarted) return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }
    
    private void HandleMouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            ProcessHitObject(hit.transform);
        }
    }
    
    private void ProcessHitObject(Transform hitTransform)
    {
        if (_cameraFollow == null) return;
        
        if ((hitTransform.CompareTag("Left") || hitTransform.CompareTag("Right")) && !_dialogueStarted)
        {
            _savedPosition = transform.position;
            _savedRotation = transform.rotation;
            _savedParent = transform.parent;

            Debug.Log($"Попали в {hitTransform.name}! Запускаем диалог.");
            _cameraFollow.SetTarget(hitTransform);
            
            StartDialogue(hitTransform.name);
        }
    }
    
    private void StartDialogue(string targetObjectName)
    {
        if (_dialogueRunner == null) return;
        
        if (_variableStorage != null)
        {
            _variableStorage.SetValue("$speaker_name", targetObjectName);
        }

        _dialogueRunner.StartDialogue(_dialogueNodeName);
        _dialogueStarted = true;
    }
    
    private void OnDialogueComplete()
    {
        Debug.Log("Диалог завершен. Возвращаем камеру.");
        
        if (_cameraFollow != null)
        {
            _cameraFollow.SetTarget(null);
        }
        
        transform.parent = _savedParent;
        transform.position = _savedPosition;
        transform.rotation = _savedRotation;
        
        ResetDialogue();
    }
    
    private void OnDestroy()
    {
        if (_dialogueRunner != null)
        {
            _dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
        }
    }
    
    public void ResetDialogue()
    {
        _dialogueStarted = false;
    }
}
