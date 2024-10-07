using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour 
{
    private int attackDamage;
    public int health;
	public float SpeedUpMotion = 1.5f; // when low health speed up
	public float pushForce = 5;
    [HideInInspector]
    public int maxHealth;
    private float originPushForce;
    private bool canPushPlayer;
    private Animator animator;
    private EnemyPathfinding enemyAi;
    private GameObject player;
    public bool isDie;
    private Action OnAttackMomentEvent;

    

    [SerializeField] private AudioSource breathingSound;
    [SerializeField] private AudioSource attackSound;
    [SerializeField] private AudioClip[] attackClips;
    [SerializeField] private GameObject holdBreathingSound;

    private void OnEnable()
    {
		OnAttackMomentEvent += takeDamagePlayer;
		OnAttackMomentEvent += ActivePush;
        OnAttackMomentEvent += bloodScreenEffect;
       
    }
    private void OnDisable()
    {
		OnAttackMomentEvent -= takeDamagePlayer;
		OnAttackMomentEvent -= ActivePush;
		OnAttackMomentEvent -= bloodScreenEffect;
		
	}

    private void Start()
    {
       
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player");
        if (player == null)
            Debug.LogWarning("Not found player by tag");
        enemyAi = GetComponent<EnemyPathfinding>();
        maxHealth = health;
        originPushForce = pushForce;

		BreathingAudio();
	}
    private void Update()
    {

        PushForce();
	}


    public void TakeDamage(int amount)
	{
        if (isDie)
            return;
        enemyAi.TakeDamageAI();

		if (health < (maxHealth / 2))
			animator.SetFloat("speedUp", SpeedUpMotion);

		if (health <= 0)
        {
            isDie = true;
            Die();
            return;
        }
        health -= amount;

        

	}


    public void AttackEvent() // As event in animation clip
    {
        float distance = Vector3.Distance(player.transform.position, transform.position);
        if (distance <= enemyAi.attackRange)
        {
			OnAttackMomentEvent?.Invoke();
		}

    }

    void takeDamagePlayer()
    {
		player.GetComponent<FirstPersonController>().TakeDamage(attackDamage);

	}

    void ActivePush()
    {
        canPushPlayer = true;
	}
    private float currentVelocityPush;
	// Note:  we cant use rigidbody.Addforce becouse we use CharacterController Component
	void PushForce()
    {
        if (!canPushPlayer)
            return;
        

		var _characterController =  player.GetComponent<CharacterController>();
		Vector3 directionForce = (player.transform.position - transform.position).normalized;
        Vector3 pushDir = directionForce * pushForce * Time.deltaTime;

        if(pushForce > 0.1f)
        {
			pushForce = Mathf.Lerp(pushForce, 0f,Time.deltaTime * 2f);
			_characterController.Move(pushDir);
			return;
		}
        pushForce = originPushForce;
		canPushPlayer = false;
	}
    void bloodScreenEffect()
    {
        player.GetComponent<UiPlayer>().ActiveBloodScreen();
    }
	void Die()
    {
		Destroy(gameObject, 3.5f);
		enemyAi.Die();
        player.GetComponent<UiPlayer>().SetNumberKills();
        GameManager.Instance.gameObject.GetComponent<RespawnSystem>().RemoveSpawnPositionUsed(transform);

	}


    void BreathingAudio()
    {
        float randomDeley = UnityEngine.Random.Range(0f, 6f); // for avoid play in same time
        breathingSound.PlayDelayed(randomDeley);
	}
    public void AttackAudio()
    {
        //breathingSound.Pause();
        if (attackSound.isPlaying)
            return;

		int randomSound = UnityEngine.Random.Range(0, attackClips.Length);
        attackSound.clip = attackClips[randomSound];
		attackSound.Play();
	}
   
}
