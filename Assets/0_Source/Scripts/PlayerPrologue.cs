using System.Collections;
using UnityEngine;

// Управление игроком в прологе — он «на рельсах»:
//  W — шаг к телефону. Нужно stepsToPhone нажатий (по умолчанию 3), чтобы дойти.
//      Дойдя до телефона, W больше не двигает.
//  S — игрока «ведёт» назад, но он тут же возвращается на место (уйти нельзя).
//  A/D — камера «пробует» повернуться и возвращается обратно (повернуться нельзя).
//
// Скрипт вешается на объект-риг игрока (тот, чьё движение = вид игрока; камера — он сам
// или его потомок). Пока идёт любая анимация — ввод игнорируется (_busy).
public class PlayerPrologue : MonoBehaviour
{
    [Header("Движение к телефону")]
    [Tooltip("Точка, где игрок должен остановиться (пустой объект ПЕРЕД звонящим телефоном).")]
    [SerializeField] private Transform phoneTarget;
    [Tooltip("Сколько нажатий W нужно, чтобы дойти до телефона.")]
    [SerializeField] private int stepsToPhone = 3;
    [Tooltip("Длительность одного шага, сек.")]
    [SerializeField] private float stepDuration = 0.5f;

    [Header("«Пружина» при попытке уйти / повернуть")]
    [Tooltip("На сколько игрока дёргает назад при S (метры), прежде чем вернуть.")]
    [SerializeField] private float backNudgeDistance = 0.4f;
    [Tooltip("На сколько градусов камера «пробует» повернуться при A/D.")]
    [SerializeField] private float turnNudgeAngle = 18f;
    [Tooltip("Длительность дёрганья «туда и обратно», сек.")]
    [SerializeField] private float nudgeDuration = 0.35f;

    [Header("Управление")]
    [Tooltip("Можно ли сейчас двигаться/реагировать на ввод. Выключи на время катсцены.")]
    [SerializeField] private bool inputEnabled = true;

    private Vector3 _homePosition;     // текущая «домашняя» точка (сдвигается с шагами W)
    private Quaternion _homeRotation;  // зафиксированный поворот (не меняется — повернуться нельзя)
    private Vector3 _startPosition;    // позиция в самом начале (для расчёта шагов к телефону)
    private int _step;                 // сколько шагов W уже сделано
    private bool _busy;                // идёт анимация — ввод игнорируется

    // Дошёл ли игрок до телефона (для скрипта-обработчика клика по телефону).
    public bool AtPhone => _step >= stepsToPhone;

    private void Awake()
    {
        _startPosition = transform.position;
        _homePosition = transform.position;
        _homeRotation = transform.rotation;
    }

    private void Update()
    {
        if (_busy || !inputEnabled) return;

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (!AtPhone) StartCoroutine(StepForward());
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine(NudgeBack());
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            StartCoroutine(NudgeTurn(-turnNudgeAngle));
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            StartCoroutine(NudgeTurn(+turnNudgeAngle));
        }
    }

    // Шаг к телефону: после stepsToPhone шагов игрок стоит на phoneTarget.
    private IEnumerator StepForward()
    {
        _busy = true;
        _step++;

        Vector3 from = transform.position;
        Vector3 to = from;
        if (phoneTarget != null && stepsToPhone > 0)
            to = Vector3.Lerp(_startPosition, phoneTarget.position, (float)_step / stepsToPhone);

        yield return MoveOverTime(from, to, stepDuration);

        _homePosition = to; // новая «домашняя» точка
        _busy = false;
    }

    // Дёрнуть назад и вернуть — уйти нельзя.
    private IEnumerator NudgeBack()
    {
        _busy = true;
        Vector3 back = _homePosition - transform.forward * backNudgeDistance;
        yield return MoveOverTime(_homePosition, back, nudgeDuration * 0.5f);
        yield return MoveOverTime(back, _homePosition, nudgeDuration * 0.5f);
        transform.position = _homePosition;
        _busy = false;
    }

    // Дёрнуть в поворот и вернуть — повернуться нельзя.
    private IEnumerator NudgeTurn(float angle)
    {
        _busy = true;
        Quaternion turned = _homeRotation * Quaternion.Euler(0f, angle, 0f);
        yield return RotateOverTime(_homeRotation, turned, nudgeDuration * 0.5f);
        yield return RotateOverTime(turned, _homeRotation, nudgeDuration * 0.5f);
        transform.rotation = _homeRotation;
        _busy = false;
    }

    private IEnumerator MoveOverTime(Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / duration));
            yield return null;
        }
        transform.position = to;
    }

    private IEnumerator RotateOverTime(Quaternion from, Quaternion to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(from, to, Mathf.SmoothStep(0f, 1f, t / duration));
            yield return null;
        }
        transform.rotation = to;
    }

    // Включить/выключить ввод (напр. отключить на время финальной катсцены).
    public void SetInputEnabled(bool value) => inputEnabled = value;
}
