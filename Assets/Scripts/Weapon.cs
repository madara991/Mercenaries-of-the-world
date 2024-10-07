using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
	public int attakeDamage;

	public int bulletspeed;
	public float timeBetweenShoots;
	private float currentShootTime;
	private float timeTaken; // The time it takes for the bullet to reach the target
							 // instead of (the collision process each time)
	public int numberBullets;
	public int maxNumberBullets = 30;

	public bool isRealoding;



	[SerializeField]
	public bool isAiming;
	public Transform aimTarget; // only for debugging point of aim of hit 
	

	public GameObject bulletPref;
	private GameObject bulletClone;
	public GameObject flashEffect;
	public GameObject bloodEffect;
	public Transform nozzle;

	private Recoil _recoil;
	private UiPlayer UiPlayer;
	private Animator animator;

	public AudioSource akmGunAudio;
	public AudioSource hitAudio;

	


	
	private void OnEnable()
	{
		_recoil = GetComponent<Recoil>();
		if (_recoil == null)
			Debug.LogWarning("Error: not found 'Recoil camera' object");
		animator = GetComponent<Animator>();

		numberBullets = maxNumberBullets;

		SetBulletSpeed();
	}
	private void Start()
	{
		UiPlayer = GetComponent<UiPlayer>();
		
	}
	void Update()
	{

		if (Input.GetMouseButton(0) && Time.time >= currentShootTime + timeBetweenShoots)
		{
			Shoot();
			//Instantiate(bloodEffect, Vector3.zero,Quaternion.identity);
			currentShootTime = Time.time;
		}
		if (Input.GetMouseButtonDown(1))
		{
			AimingWeaponToggle(true);
		}
		else
		if(Input.GetMouseButtonUp(1))
		{
			AimingWeaponToggle(false);
		}
	}

	// I created a new technology instead of wasting effort and energy (each collision of bullet) that exist 
	// on the "Weapon" script.

	// its just full controll to bullet from this script
	
	
	void Shoot()
	{
		if (isRealoding)
			return;
		Debug.Log(numberBullets);
		if(numberBullets <= 1)
		{
			Debug.Log("finsh");
			isRealoding = true;
			animator.SetTrigger("reloading");
		}

		

		Vector2 centarScreen = new Vector2(Screen.width / 2, Screen.height/2);
		Ray ray = Camera.main.ScreenPointToRay(centarScreen);

		var bulletClone = Instantiate(this.bulletPref, nozzle.position, nozzle.rotation);

		Instantiate(flashEffect, nozzle.position, transform.rotation, transform);

		numberBullets--;


		if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
		{
			Debug.DrawRay(ray.origin, hit.point - ray.origin , Color.blue); // for detect screen center
																			

			timeTaken = Vector3.Distance(nozzle.position, hit.point) / bulletspeed;

			if (hit.collider.CompareTag("Enemy"))
			{

				var _enemy = hit.collider.GetComponent<Enemy>();
				if (_enemy != null)
				{

					StartCoroutine(TakeReactions(_enemy,hit));
					Debug.Log("hit !");
				}

			}
			
		}

		_recoil.RecoilFire();
		
		GunAudio();
	}

	IEnumerator TakeReactions(Enemy _enemy , RaycastHit _hit)
	{
		yield return new WaitForSeconds(timeTaken);

		_enemy.TakeDamage(attakeDamage);
		
		Instantiate(bloodEffect, _hit.point, Quaternion.identity);

		UiAimEffect();
		hitAudio.Play();

	}

	// as event in animation clip
	public void ReloadingFinshEvent()
	{
		isRealoding = false;
	}
	
	void AimingWeaponToggle(bool aimmigAtive)
	{
		isAiming = aimmigAtive;
		var _animator = GetComponent<Animator>();
		_animator.SetBool("isAiming", aimmigAtive);
		_recoil.isActive = !aimmigAtive;
	}


	

	void SetBulletSpeed()
	{
		bulletPref.GetComponent<Bullet>().speedBullet = this.bulletspeed;
	}
	void GunAudio()
	{
		float randomPitch = UnityEngine.Random.Range(0.9f, 1.15f);
		float randomVolume = UnityEngine.Random.Range(0.7f, 0.9f);
		akmGunAudio.pitch = randomPitch;
		akmGunAudio.volume = randomVolume;
		akmGunAudio.Play();
	}
	
	void UiAimEffect()
	{
		UiPlayer.PointHit();
	}
	
}
