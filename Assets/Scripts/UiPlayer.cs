using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiPlayer : MonoBehaviour
{
	private Weapon weapon;

	public GameObject pointAimUI;
	public GameObject HitAimUI;

	public Animation bloodScreenAnim;

	public Text numberBulletsText;
	public Text numberKills;
	private int killsCount = 0; 


	private void Start()
	{
		weapon = GetComponent<Weapon>();
	}
	private void Update()
	{
		CountAmmo();
	}
	void CountAmmo()
	{
		SetNumberBulletsText();

		

	}
	public void PointHit()
	{
		pointAimUI.SetActive(false);
		HitAimUI.SetActive(true);
		StartCoroutine(StayTime());

	}
	public void PointNonHit()
	{
		pointAimUI.SetActive(true);
		HitAimUI.SetActive(false);
	}

	public void ActiveBloodScreen()
	{
		if(!bloodScreenAnim.isPlaying)
		{
			bloodScreenAnim.Play();
		}
			
	}

	public void SetNumberBulletsText()
	{
		if (weapon.numberBullets <= 0)
		{
			weapon.numberBullets = weapon.maxNumberBullets;
			
			// stop shooting
		}

		numberBulletsText.text = weapon.numberBullets + " / " + weapon.maxNumberBullets;
	}
	
	IEnumerator StayTime()
	{
		yield return new WaitForSeconds(0.07f);
		PointNonHit();
	}

	public void SetNumberKills()
	{
		numberKills.text = "Kills: " + killsCount++;
	}
}
