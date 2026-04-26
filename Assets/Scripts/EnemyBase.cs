using UnityEngine;

public class EnemyBase : MonoBehaviour, IDamageable
{
    public float Health = 100f;
    
    public void TakeDamage(float damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
