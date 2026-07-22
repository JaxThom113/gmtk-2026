using UnityEngine;

public interface IHealth
{
    public float CurrentHealth { get; set; }
    public float MaxHealth { get; set; }

    public void TakeDamage(float damage);
}
