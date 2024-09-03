using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [Header("참조용 변수들")]
    [SerializeField] private Transform _barTrm;
    [SerializeField] private Health health;

    private void Awake()
    {
        health.OnHealthChangedEvent += HandleHealthChanged;
    }

    public void HandleHealthChanged()
    {
        float ratio = Mathf.Clamp((float)health.currentHealth.Value / health.maxHealth, 0, 1);
        _barTrm.localScale = new Vector3(ratio, 1, 1);
    }


}
