using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class cameraState : MonoBehaviour
{
    
    public Transform cameraPos;

    private int originFOV = 60;
    public int WeaponFOV = 45;
    public float FOVSpeedChange;
    private float FOV;

	private Weapon weapon;
    private void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            Debug.Log("x");
		weapon =player.GetComponent<Weapon>();
        if (weapon == null)
            Debug.Log("xx");
    }

    private void Update()
    {
        AimingWithWeapon();
	}
    void LateUpdate()
    {
        transform.position = cameraPos.position;
        transform.eulerAngles = cameraPos.eulerAngles;
    }

    public void SetCameraTarget(Transform cameraTarget)
    {
        cameraPos = cameraTarget;
    }
	public void AimingWithWeapon()
    {
        Camera.main.fieldOfView = FOV;

		if (weapon.isAiming)
			FOV = Mathf.Lerp(FOV, WeaponFOV, FOVSpeedChange * Time.deltaTime);
		else
			FOV = Mathf.Lerp(FOV, originFOV, FOVSpeedChange * Time.deltaTime);

	}
}
