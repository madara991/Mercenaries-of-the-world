using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TreeEditor;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UIElements;

public class FirstPersonController : MonoBehaviour
{
	public float MoveSpeed = 2.0f;
	public float SprintSpeed = 6f;
	public float speedChangeRate = 10f;
	public int health;
	[Space(30)]
	public float rotationVelocity;
	public float sensitivity = 0.5f;
	public float smoothRotation;
	private float _cameraPitch = 0.0f;
	public float TopClamp = 70.0f;
	public float BottomClamp = -30.0f;

	[Space(30)]
	public float JumpHeight = 1.2f;
	public float Gravity = -15.0f;
	public float JumpTimeout = 0.50f;
	public float FallTimeout = 0.15f;

	[Space(30)]
	public bool Grounded = true;
	public float GroundedOffset = -0.14f;
	public float GroundedRadius = 0.28f;
	public LayerMask GroundLayers;


	private CharacterController _characterController;
	private UiPlayer uiPlayer;
	private Animator animator;
	private float _speed;
	private float _jumpTimeoutDelta, _fallTimeoutDelta;
	private float _verticalVelocity;
	private float _terminalVelocity = 53.0f;
	float _mouseX;
	float _mouseY;
	float currentVelocityX;
	float currentVelocityY;
	float _cameraPitchLerp;

	[Range(0, 1)] public float FootstepAudioVolume = 0.5f;
	public AudioClip LandingAudioClip;
	public AudioClip[] FootstepAudioClips;
	public float footstepsVolume = 0.5f;
	public float runStepInterval = 0.4f;
	public float walkStepInterval = 0.8f;



	public Transform DirectionAim;
	private void Start()
	{
		_characterController = GetComponent<CharacterController>();
		uiPlayer = GetComponent<UiPlayer>();
		animator = GetComponent<Animator>();
		_jumpTimeoutDelta = JumpTimeout;
		_fallTimeoutDelta = FallTimeout;
	}

	private void Update()
	{
		GroundedCheck();
		Move();
		JumpAndGravity();
		RotationHandle();

		if (!isFootstepsPlay)
		{
			StartCoroutine(PlayFootsteps());
		}
			


	}


	
	private void Move()
	{
		float horizontalInput = Input.GetAxisRaw("Horizontal");
		float verticalInput = Input.GetAxis("Vertical");
		Vector3 moveDirection = transform.right * horizontalInput + transform.forward * verticalInput;
		moveDirection = moveDirection.normalized;

		
		
		_speed = Input.GetKey(KeyCode.LeftShift)
			? Mathf.Lerp(_speed, SprintSpeed, speedChangeRate * Time.deltaTime)
			: Mathf.Lerp(_speed, MoveSpeed, speedChangeRate * Time.deltaTime);

		_characterController.Move(moveDirection * _speed * Time.deltaTime);

		if (moveDirection == Vector3.zero) // return to origin speed
			_speed = 0f;

	}

	void RotationHandle()
	{
		float targetMouseX = Input.GetAxis("Mouse X") * rotationVelocity * sensitivity;
		float targetMouseY = Input.GetAxis("Mouse Y") * rotationVelocity * sensitivity;

		_mouseX = Mathf.SmoothDamp(_mouseX, targetMouseX, ref currentVelocityX, smoothRotation);
		_mouseY = Mathf.SmoothDamp(_mouseY, targetMouseY, ref currentVelocityY, smoothRotation);

		_cameraPitch -= _mouseY;
		_cameraPitch = Mathf.Clamp(_cameraPitch, BottomClamp, TopClamp);
		_cameraPitchLerp = Mathf.Lerp(_cameraPitchLerp, _cameraPitch, smoothRotation);



		DirectionAim.transform.localEulerAngles = new Vector3(_cameraPitchLerp , 0f, 0f); // Camera rotation in X axis (up & down)
		

		transform.Rotate(Vector3.up, _mouseX);                                           // Character rotation in Y axis ( left & right)



	}

	public void TakeDamage(int amount)
	{
		if (health <= 0)
		{
			Debug.Log("player is Die");
			return;
		}

		health -= amount;
		

	}
	private void JumpAndGravity()
	{
		if (Grounded)
		{
			// Reset the fall timeout timer
			_fallTimeoutDelta = FallTimeout;

			// Stop our velocity dropping infinitely when grounded
			if (_verticalVelocity < 0.0f)
			{
				_verticalVelocity = -2f;
			}

			// Jump
			if (Input.GetKeyDown(KeyCode.Space) && _jumpTimeoutDelta <= 0.0f)
			{
				// The square root of H * -2 * G = how much velocity needed to reach desired height
				_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
			}

			// Jump timeout
			if (_jumpTimeoutDelta >= 0.0f)
			{
				_jumpTimeoutDelta -= Time.deltaTime;
			}
		}
		else
		{
			// Reset the jump timeout timer
			_jumpTimeoutDelta = JumpTimeout;

			// Fall timeout
			if (_fallTimeoutDelta >= 0.0f)
			{
				_fallTimeoutDelta -= Time.deltaTime;
			}
		}

		// Apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
		if (_verticalVelocity < _terminalVelocity)
		{
			_verticalVelocity += Gravity * Time.deltaTime;
		}

		_characterController.Move(new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

	}
	private void GroundedCheck()
	{
		// Set sphere position, with offset
		Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
		Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
	}
	private bool isFootstepsPlay;
	private IEnumerator PlayFootsteps()
	{
		if(!_characterController.isGrounded)
			yield return null;
		
		while (_speed > 0.1f)
		{
			if (FootstepAudioClips.Length > 0)
			{
				isFootstepsPlay = true;
				// Calculate time between footsteps based on speed
				float interval = _speed > MoveSpeed ? runStepInterval : walkStepInterval;
				
				var index = UnityEngine.Random.Range(0, FootstepAudioClips.Length);
				AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_characterController.center), footstepsVolume);
				yield return new WaitForSeconds(interval);

				isFootstepsPlay = false;
			    
			}
			
		}
	}
}



