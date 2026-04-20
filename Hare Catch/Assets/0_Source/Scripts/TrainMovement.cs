using UnityEngine;

public class TrainMovement : MonoBehaviour
{
    [SerializeField] private Transform[] _wagons;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _gap = 10f;

    [SerializeField] private float _resetX = 30f;

    private void Update()
    {
        for (int i = 0; i < _wagons.Length; i++)
        {
            Transform wagon = _wagons[i];

            wagon.Translate(Vector3.right * _speed * Time.deltaTime, Space.World);

            if (wagon.position.x > _resetX)
            {
                float minX = Mathf.Infinity;

                foreach (var w in _wagons)
                {
                    if (w.position.x < minX)
                    {
                        minX = w.position.x;
                    }
                }

                wagon.position = new Vector3(minX - _gap, wagon.position.y, wagon.position.z);
            }
        }
    }
}
