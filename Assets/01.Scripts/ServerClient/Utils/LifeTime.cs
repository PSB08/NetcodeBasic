using UnityEngine;

public class LifeTime : MonoBehaviour
{
    [SerializeField] private float _lifeTime;
    private float _currentTime = 0;

    private void Update()
    {
        _currentTime += Time.deltaTime;
        if (_currentTime >= _lifeTime)
            Destroy(gameObject);
    }

}
