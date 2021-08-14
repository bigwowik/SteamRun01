using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class PlayerMovement : MonoBehaviour
{
	//fields

	public TrackManager trackManager;
	public CharacterCollider characterCollider;

	//components


	protected float m_JumpStart;
	protected bool m_Jumping;

	protected bool m_Sliding;
	protected float m_SlideStart;

	protected bool m_IsInvincible;
	protected bool m_IsRunning;

	protected int m_CurrentLane = k_StartingLane;


	protected bool m_IsSwiping;
	protected Vector2 m_StartingTouch;



	protected const float k_GroundingSpeed = 80f;

	protected const int k_StartingLane = 0;
	protected Vector3 m_TargetPosition = Vector3.zero;

	public GameObject interactiveCollider;

	public GameObject leftCamera;
	public GameObject rightCamera;

	float lastClickTime;


	public GameObject shield;

	public Transform steamPoint;

	public GameObject steamFlow;

	private Animator animator;



	float animationSpeedStartValue = 5f; //для какой велечины делались анимации

	private void Awake()
    {
		animator = GetComponentInChildren<Animator>();


	}
    private void Start()
    {
		GameManager.Instance.onGameStateChanged.AddListener(OnStartLevel);

    }
    // Update is called once per frame
    

	public void GodModeButton()
    {
		if (!trackManager.godMode)
		{
			Debug.Log("GodMode on.");
			trackManager.currentLives = 9999;
			trackManager.livesText.text = trackManager.currentLives + "";
			trackManager.godMode = true;
        }
        else
        {
			Debug.Log("GodMode off.");
			trackManager.currentLives = 2;
			trackManager.livesText.text = trackManager.currentLives + "";
			trackManager.godMode = false;
		}
	}

	void OnStartLevel(GameManager.GameState currentGameState, GameManager.GameState previusGameState)
	{
		if ((currentGameState == GameManager.GameState.LevelsRunning || currentGameState == GameManager.GameState.EndlessRunning) && (previusGameState == GameManager.GameState.PAUSED || previusGameState == GameManager.GameState.FAILURE))
		{
			return; //no reset after pause
		}
		else if ((currentGameState == GameManager.GameState.LevelsRunning || currentGameState == GameManager.GameState.EndlessRunning) && !trackManager.wasDied)
		{
			//reset on start new level
			Debug.Log(" 0- 0 -0 on start level.");
			SetTransformPosition(new Vector3(0, 0, 0));
			//transform.position = new Vector3(0, 0, 0);
			ChangeLane(-1);
		}

	}

	protected void Update()
	{
		//#if UNITY_EDITOR || UNITY_STANDALONE
		// Use key input in editor or standalone
		// disabled if it's tutorial and not thecurrent right tutorial level (see func TutorialMoveCheck)

		if (!trackManager.isMoving || GameManager.Instance.CurrentGameState == GameManager.GameState.PAUSED)  // не уверен на счет этого, обращение с синглотну в апдейте
			return;

		if (Input.GetKeyDown(KeyCode.LeftArrow) || (Input.GetKeyDown(KeyCode.A)))
		{
			ChangeLane(-1);
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow) || (Input.GetKeyDown(KeyCode.D)))
		{
			ChangeLane(1);
		}
		else if (Input.GetKeyDown(KeyCode.UpArrow) || (Input.GetKeyDown(KeyCode.W)))
		{
			Jump();
		}
		else if (Input.GetKeyDown(KeyCode.Space))
		{
			//Debug.Log("Space");
			TapCloth();
		}
		else if (Input.GetKeyDown(KeyCode.P))
		{
			//Debug.Log("Space");
			ChangeSpeed(+1);
		}
		else if (Input.GetKeyDown(KeyCode.O))
		{
			//Debug.Log("Space");
			ChangeSpeed(-1);
		}
		else if (Input.GetKeyDown(KeyCode.Tab))
		{
			GodModeButton();
		}

//#else
// Use touch input on mobile
		if (Input.touchCount == 1)
        {
			if(m_IsSwiping)
			{
				Vector2 diff = Input.GetTouch(0).position - m_StartingTouch;

				// Put difference in Screen ratio, but using only width, so the ratio is the same on both
                // axes (otherwise we would have to swipe more vertically...)
				diff = new Vector2(diff.x/Screen.width, diff.y/Screen.width);

				if(diff.magnitude > 0.01f) //we set the swip distance to trigger movement to 1% of the screen width
				{
					if(Mathf.Abs(diff.y) > Mathf.Abs(diff.x))
					{
						if(diff.y < 0)
						{

							TapCloth();

						}
						else
						{
							Jump();
						}
					}
					else
					{
						if(diff.x < 0)
						{
							ChangeLane(-1);
						}
						else
						{
							ChangeLane(1);
						}
					}
						
					m_IsSwiping = false;
                }
                else
                {
					
				}
            }

        	// Input check is AFTER the swip test, that way if TouchPhase.Ended happen a single frame after the Began Phase
			// a swipe can still be registered (otherwise, m_IsSwiping will be set to false and the test wouldn't happen for that began-Ended pair)
			if(Input.GetTouch(0).phase == TouchPhase.Began)
			{
				m_StartingTouch = Input.GetTouch(0).position;
				m_IsSwiping = true;
				
			}
			else if(Input.GetTouch(0).phase == TouchPhase.Ended)
			{
				m_IsSwiping = false;
				
			}
        }
//#endif

		Vector3 verticalTargetPosition = m_TargetPosition;

		if (m_Jumping)
		{
			
				// Same as with the sliding, we want a fixed jump LENGTH not fixed jump TIME. Also, just as with sliding,
				// we slightly modify length with speed to make it more playable.
				float correctJumpLength = trackManager.jumpLength * (1.0f + trackManager.speedRatio);
				float ratio = (trackManager.worldDistance - m_JumpStart) / correctJumpLength;
				if (ratio >= 1.0f)
				{
					m_Jumping = false;
				}
				else
				{
					verticalTargetPosition.y = Mathf.Sin(ratio * Mathf.PI) * trackManager.jumpHeight;
				}
			
			verticalTargetPosition.y = Mathf.MoveTowards(verticalTargetPosition.y, 0, k_GroundingSpeed * Time.deltaTime);

			if (Mathf.Approximately(verticalTargetPosition.y, 0f))
			{
				//m_Jumping = false;
			}
			
		}

		if (trackManager.isMoving)
		{

			characterCollider.transform.localPosition = Vector3.MoveTowards(characterCollider.transform.localPosition, verticalTargetPosition, trackManager.currentSpeed * trackManager.horizontalSpeedRatio * Time.deltaTime);


			//transform.position += new Vector3(0, 0, trackManager.currentSpeed * Time.deltaTime);
			SetTransformPosition(transform.position + new Vector3(0, 0, trackManager.currentSpeed * Time.deltaTime));


		}

        if (trackManager.isProtectedByShield)
        {
			shield.SetActive(true);

        }
        else
        {
			shield.SetActive(false);
		}

		var animSpeedMultiplayer = 0.05f * trackManager.currentSpeed + 0.75f;
		animator.SetFloat("AnimationSpeed", animSpeedMultiplayer);  //0.4f чтобы не сильно увеличивалась скорость

	}
	//to debug
	public void SetTransformPosition(Vector3 newPos)
    {
		//Debug.Log(gameObject.name + " _player new position - " + newPos);
		transform.position = newPos;
    }



	public void Jump()
	{
		//if (!m_IsRunning)
		//	return;

		if (!m_Jumping)
		{
			//Debug.Log("Jump");
			animator.SetTrigger("Jump");

			float correctJumpLength = trackManager.jumpLength * (1.0f + trackManager.speedRatio);

			m_JumpStart = trackManager.worldDistance;

			//character.animator.SetFloat(s_JumpingSpeedHash, animSpeed);
			m_Jumping = true;
		}
	}
	public void TapCloth()
	{
		float timeSinceLastClick = Time.time - lastClickTime;
		Debug.Log("timeSinceLastClick : " + timeSinceLastClick);

		//Debug.Log("TapCloth");


		if (timeSinceLastClick <= TrackManager.Instance.timeToDoubleTap)
		{
			if (interactiveCollider != null)
			{
				interactiveCollider.GetComponent<ClothInteractive>().DoubleTap();
				characterCollider.CheckClothes(interactiveCollider.GetComponent<ClothInteractive>());
				interactiveCollider = null;


				StopCoroutine(checkClothReset);
			}
			//double tap
			Debug.Log("Double Tap");
			var particles1 = Instantiate(steamFlow, steamPoint);
			var particles2 = Instantiate(steamFlow, steamPoint);

			if (!m_Jumping)
			{
				animator.SetTrigger("Steam2");
			}

		}
		else
		{
			if (interactiveCollider != null)
			{
				interactiveCollider.GetComponent<ClothInteractive>().SingleTap();
				checkClothReset = CheckCurrentClothes();
				StartCoroutine(checkClothReset);
			}
			//single tap
			Debug.Log("Single Tap");
			Instantiate(steamFlow, steamPoint);

			if (!m_Jumping)
			{
				animator.SetTrigger("Steam1");
			}
		}
		SetCameraInpulse();

		lastClickTime = Time.time;

	}
	IEnumerator checkClothReset;
	IEnumerator CheckCurrentClothes()
	{
		yield return new WaitForSeconds(TrackManager.Instance.timeToDoubleTap);
		Debug.Log("Single Tap Reset");
		if (interactiveCollider != null)
		{
			characterCollider.CheckClothes(interactiveCollider.GetComponent<ClothInteractive>());
			interactiveCollider = null;
		}
		yield return null;


	}


	public void ChangeLane(int direction)
	{
		//if (!m_IsRunning)
		//	return;

		if (!m_Jumping)
		{
			if(direction < 0)
				animator.SetTrigger("Left");
			else
				animator.SetTrigger("Right");
		}



		int targetLane = m_CurrentLane + direction;

		if (targetLane < 0 || targetLane > 1)
			// Ignore, we are on the borders.
			return;

		m_CurrentLane = targetLane;
		m_TargetPosition = new Vector3((m_CurrentLane) * trackManager.horizontalStepDistance, 0, 0);
		SetCamera(m_CurrentLane);
	}

	void ChangeSpeed(int deltaSpeed)
    {
		if(trackManager.currentSpeed + deltaSpeed >= 0)
        {
			trackManager.currentSpeed += deltaSpeed;
		}
		
    }

	void SetCamera(int line)
    {
		if(line == 0)
        {
			leftCamera.SetActive(true);
			rightCamera.SetActive(false);
        }
        else
        {

			rightCamera.SetActive(true);
			leftCamera.SetActive(false);

		}
    }

	void SetCameraInpulse()
    {
		var source = GetComponent<CinemachineImpulseSource>();
		source.GenerateImpulse();
	}

}
