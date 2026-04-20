using UnityEngine;


public class InvisibleBarrier : MonoBehaviour, IMovementBarrier
{
    [SerializeField] private bool allowForward = true;  // можно ли дальше вперёд отсюда
    [SerializeField] private bool allowBackward = true;  // можно ли дальше назад отсюда

    //реализуем свойства интерфейса (возвращаем всякое)
    public bool CanMoveForward => allowForward;
    public bool CanMoveBackward => allowBackward;
    public float BarrierXPosition => transform.position.x;

    private void OnTriggerEnter(Collider other)
    {
        //Отладка отладка отладка, потом удалю если не забуду
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered barrier at X = " + BarrierXPosition);
        }
    }


    //эта часть делает барьер невидимым (безпонятия как она работает, я её завайпкодил)
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void OnEnable()
    {
        if (meshRenderer != null)
        {
            if (Application.isPlaying)
                meshRenderer.enabled = false;
        }
    }
}