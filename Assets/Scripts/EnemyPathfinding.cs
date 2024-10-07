using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPathfinding : MonoBehaviour
{
	// NOTE: MAKE SURE BEFORE START SCENE TO SET ENEMY ON GROUND PERFECT TO AVOID LOSING DESTNATION 


	private Enemy enemy;
	private NavMeshAgent agent;
	private Transform player;

	private Animator animator;

	private int attackAnimationHash = Animator.StringToHash("attack");
	private int hit1AnimationHash = Animator.StringToHash("hit");
	private int movementAnimationHash = Animator.StringToHash("walk");

	public float attackRange = 2.0f;
	
	public float detectAngle = 45.0f;
	public float detectRadius = 10.0f;
	public float randomMovementRadius = 5.0f;
	public float wallAvoidanceDistance = 1.0f;
	public LayerMask detectObjectMask;
	public LayerMask avoidanceAbleMask;
	public bool CanSearchingWithClimbing;
	public float minIdleTime = 2.0f; // Minimum time the enemy stops moving
	public float maxIdleTime = 5.0f; // Maximum time the enemy stops moving
	public float returnToIdleTime = 8f;
	public Vector3 directionCast;

	private bool isDetecetd;
	private bool isGetAttack;
	private Vector3 originSearchPlace;
	private bool isMovingRandomly;
	private bool isIdle;

	
	float hitAnimationProbability = 0.10f; // 10% Probability for active hit animation

	
	
	// respons system
	// ui wepon 
	// main menu
	// some edit AND SE IT  on AI ENREMY

	// desgin system and senario 
	// return desgin map + add rooms
	// add some actions in game when playing
	// opcinal : add bose big

	private void Start()
	{
		agent = GetComponent<NavMeshAgent>();
		animator = GetComponent<Animator>();
		enemy = GetComponent<Enemy>();
		player = GameObject.FindGameObjectWithTag("Player").transform;

		if (player == null)
		{
			Debug.LogError("Player not found! Make sure the player object is tagged as 'Player'.");
		}
		
		
		agent.stoppingDistance = attackRange;
		originSearchPlace = transform.position;

		
		
		SetRandomDestination();
		StartCoroutine(IdleRoutine());
	}

	private void FixedUpdate()
	{
		if (enemy.isDie) 
			return;

		
		
		if (IsPlayerInSight()) 
		
		{
			float distance = Vector3.Distance(transform.position, player.position);
			

			if (agent.remainingDistance <= attackRange && distance <= attackRange)
			{
				Debug.Log("ready to attake?");
				ReadyToAttack();
			}
			else
			{
				
				MoveTowardsTarget();
				
			}
		}
		else if (!isIdle)
		{
			RandomMovement();
		}
	}

	bool IsPlayerInSight()
	{
		Vector3 direction = (player.position - transform.position).normalized;


		if (Physics.CheckSphere(transform.position, detectRadius, detectObjectMask))
		{
			if (isDetecetd || isGetAttack)
				return true;

			if (Vector3.Angle(transform.forward, direction) < detectAngle)
			{
				isDetecetd = true;
				return true;
			}
		}
		else
			if (isGetAttack)
			return true;

		isDetecetd = false;

		return false;
	}

	
	void MoveTowardsTarget()
	{
		agent.isStopped = false;
		agent.SetDestination(player.position);
		animator.SetBool(movementAnimationHash, true);
		isMovingRandomly = false;

		//Debug.Log("Moving towards player.");
	}

	void RandomMovement()
	{
		if (!isMovingRandomly || agent.remainingDistance < 0.5f)
		{
			SetRandomDestination();
			//Debug.Log("New random destination set: " + randomDestination);
		}

		
	}

	void SetRandomDestination()
	{
		
		Vector3 randomDirection = Random.insideUnitSphere * randomMovementRadius;
		randomDirection += originSearchPlace;
		
		NavMeshHit hit;

		if (NavMesh.Raycast(randomDirection, agent.destination, out hit, NavMesh.AllAreas))
		{
				Debug.Log("obtecle");
		}
		if (NavMesh.SamplePosition(randomDirection, out hit, randomMovementRadius, NavMesh.AllAreas))
		{
			if (agent.isOnNavMesh) // Ensure agent is on the NavMesh
			{
				if (CanSearchingWithClimbing && isDestinationHeight(hit.position))
				{
					return; // Skip setting the destination if height criteria are met
				}

				agent.SetDestination(hit.position);
				isMovingRandomly = true;
				
			}
			
		}
		
	}

	IEnumerator RetrunToIdle()
	{
		yield return new WaitForSeconds(returnToIdleTime); // if get attack than i dont found it >>> back to normal state
		if (isGetAttack && !isDetecetd)
		{
			isGetAttack = false;
		}
	}

	bool IsWallInFront()
	{
		RaycastHit hit;
		Vector3 direction = transform.forward;

		if (Physics.Raycast(transform.position, direction, out hit, wallAvoidanceDistance))
		{
			if (hit.collider.gameObject.layer == avoidanceAbleMask)
			{
				Debug.Log("Wall detected in front.");
				return true;
			}
		}
		return false;
	}

	void ReadyToAttack() // attack real is made by animation event 
	{
		agent.isStopped = true;
		animator.SetTrigger(attackAnimationHash);
		Debug.Log("x attack");
		enemy.AttackAudio();
		
	}

	// if radiant searshing have walls that need enemy for Climbing 
	bool isDestinationHeight(Vector3 TargetPosition)
	{
		if(TargetPosition.y > (agent.height / 2))
		{
			return true;
		}
		return false;
	}

	IEnumerator IdleRoutine()
	{
		while (true)
		{
			isIdle = true;
			agent.isStopped = true;
			animator.SetBool(movementAnimationHash, false);

			float idleTime = Random.Range(minIdleTime, maxIdleTime);
			yield return new WaitForSeconds(idleTime);

			isIdle = false;
			agent.isStopped = false;
			animator.SetBool(movementAnimationHash, true);

			float movementTime = Random.Range(minIdleTime, maxIdleTime);
			yield return new WaitForSeconds(movementTime);
		}
	}

	public void TakeDamageAI()
	{
		if (enemy.isDie) return;

		int randomHit = Random.Range(1, 3);
		if(Random.value < hitAnimationProbability)
		{
			animator.SetInteger(hit1AnimationHash, randomHit);
		}
		isGetAttack = true;
		StartCoroutine(RetrunToIdle());
		
			
	}

	public void Die()
	{
		agent.isStopped = true;
		animator.SetBool("die", true);
		// Additional death logic
	}

	private void OnDrawGizmos()
	{
		Vector3 origin = transform.position;

		Vector3 leftEdge = Quaternion.Euler(0, -detectAngle, 0) * transform.forward;
		Vector3 rightEdge = Quaternion.Euler(0, detectAngle, 0) * transform.forward;

		Vector3 leftPoint = origin + leftEdge * detectRadius;
		Vector3 rightPoint = origin + rightEdge * detectRadius;

		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(origin, leftPoint);
		Gizmos.DrawLine(origin, rightPoint);
		Gizmos.DrawLine(leftPoint, rightPoint);

		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(origin, detectRadius);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(origin, randomMovementRadius);

		
	}
}
