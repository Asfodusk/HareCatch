using UnityEngine;

// Данные пассажира для системы проверки билетов.
// Вешается на КОРНЕВОЙ объект НПС — тот же, где тег "Left"/"Right" и BoxCollider
// (именно в него попадает клик-рейкаст из MouseClickRaycast).
//
// Деньги/карма заданы отдельно для каждого решения, поэтому "правильность" билета
// определяет дизайнер: ставит положительную награду за верный выбор и штраф за неверный.
public class PassengerTicket : MonoBehaviour
{
    [Header("Билет пассажира")]
    [Tooltip("Картинка билета этого пассажира (PNG, импортированный как Sprite). " +
             "Показывается игроку рядом с образцом во время проверки.")]
    [SerializeField] private Sprite ticket;

    [Header("Награда за «Одобрить»")]
    [Tooltip("Сколько денег получит игрок, если ОДОБРИТ пассажира. Может быть отрицательным (штраф).")]
    [SerializeField] private int approveMoney = 10;
    [Tooltip("Сколько кармы получит игрок, если ОДОБРИТ пассажира. Может быть отрицательной.")]
    [SerializeField] private int approveKarma = 0;

    [Header("Награда за «Выгнать»")]
    [Tooltip("Сколько денег получит игрок, если ВЫГОНИТ пассажира. Может быть отрицательным.")]
    [SerializeField] private int kickMoney = 0;
    [Tooltip("Сколько кармы получит игрок, если ВЫГОНИТ пассажира. Может быть отрицательной.")]
    [SerializeField] private int kickKarma = -10;

    public Sprite Ticket => ticket;
    public int ApproveMoney => approveMoney;
    public int ApproveKarma => approveKarma;
    public int KickMoney => kickMoney;
    public int KickKarma => kickKarma;
}
