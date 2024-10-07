using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface  IDamageable
{
    public int health { get; set; }
    void TakeDamage(int amount);
    void Die();
    

}
