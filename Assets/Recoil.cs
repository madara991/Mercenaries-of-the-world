
using UnityEngine;

public class Recoil : MonoBehaviour
{
    public Transform weaponHandle;
    private Vector3 currentRotation;
    private Vector3 targetRotation;
	public bool isActive;

	[SerializeField] private float recoilX;
    [SerializeField] private float recoilY;
    [SerializeField] private float recoilZ;


    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;

    private Weapon myWeapon;
    
    // Update is called once per frame
    private void Start()
    {
        myWeapon = GetComponent<Weapon>();
    }
    void Update()    
    {
		if (!isActive && !myWeapon.isAiming)
			return;

		// back to origin position 
		targetRotation = Vector3.Lerp(targetRotation, Vector3.zero , returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.deltaTime);
		weaponHandle.localRotation = Quaternion.Euler(currentRotation);
    }


    public void RecoilFire() 
    {
        if (myWeapon.isAiming)
            return;
		// set direction recoil gun realtive to dirctions of camera forward
		Transform cameraDirction = Camera.main.transform; 
		Vector3 recoilDirection = cameraDirction.forward * -recoilZ + cameraDirction.up * recoilX + cameraDirction.right * Random.Range(-recoilY, recoilY);

		// Apply the recoil direction to the target rotation
		targetRotation += recoilDirection;
	}
}
