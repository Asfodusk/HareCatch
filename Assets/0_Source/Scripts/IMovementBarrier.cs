//делаем барьер барьеристым
public interface IMovementBarrier
{
    bool CanMoveForward { get; }

    bool CanMoveBackward { get; }

    float BarrierXPosition { get; }
}