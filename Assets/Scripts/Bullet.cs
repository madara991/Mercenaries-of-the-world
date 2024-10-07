using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [HideInInspector]
    public float speedBullet;
    public GameObject bloodEffect;

    private void Update()
    {
		Vector2 centarScreen = new Vector2(Screen.width / 2, Screen.height / 2);
		Ray ray = Camera.main.ScreenPointToRay(centarScreen);
        
        
		transform.position +=  ray.direction * speedBullet * Time.deltaTime;
    }

	// All important operations upon collision are in the "Weapon" script.
	// I created a new Technique instead of wasting effort and energy (each collision) that exist 
    // on the "Weapon" script.
	private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider == null)
            Destroy(gameObject);
        else
            Destroy(gameObject, 2f);

	}
}
