using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
	//fields
	
	[SerializeField] private float jumpHeight = 2f;
	[SerializeField] private float jumpLength = 3f;

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


	public float laneChangeSpeed = 1.0f;

	protected const float k_GroundingSpeed = 80f;

	protected const int k_StartingLane = 1;
	protected Vector3 m_TargetPosition = Vector3.zero;

	public GameObject interactiveCollider;


	private void Awake()
    {
    }
    // Update is called once per frame
    

	protected void Update()
	{
#if UNITY_EDITOR || UNITY_STANDALONE
		// Use key input in editor or standalone
		// disabled if it's tutorial and not thecurrent right tutorial level (see func TutorialMoveCheck)

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
		

#else
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
						if(TutorialMoveCheck(2) && diff.y < 0)
						{
							//Slide();
						}
						else if(TutorialMoveCheck(1))
						{
							Jump();
						}
					}
					else if(TutorialMoveCheck(0))
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
#endif

		Vector3 verticalTargetPosition = m_TargetPosition;

		if (m_Jumping)
		{
			
				// Same as with the sliding, we want a fixed jump LENGTH not fixed jump TIME. Also, just as with sliding,
				// we slightly modify length with speed to make it more playable.
				float correctJumpLength = jumpLength * (1.0f + trackManager.speedRatio);
				float ratio = (trackManager.worldDistance - m_JumpStart) / correctJumpLength;
				if (ratio >= 1.0f)
				{
					m_Jumping = false;
				}
				else
				{
					verticalTargetPosition.y = Mathf.Sin(ratio * Mathf.PI) * jumpHeight;
				}
			
			verticalTargetPosition.y = Mathf.MoveTowards(verticalTargetPosition.y, 0, k_GroundingSpeed * Time.deltaTime);

			if (Mathf.Approximately(verticalTargetPosition.y, 0f))
			{
				//m_Jumping = false;
			}
			
		}

		if (trackManager.isMoving)
		{

			characterCollider.transform.localPosition = Vector3.MoveTowards(characterCollider.transform.localPosition, verticalTargetPosition, laneChangeSpeed * Time.deltaTime);

			transform.Translate(0, 0, trackManager.currentSpeed * Time.deltaTime);
		}

	}


	public void Jump()
	{
		//if (!m_IsRunning)
		//	return;

		if (!m_Jumping)
		{
			//Debug.Log("Jump");

			float correctJumpLength = jumpLength * (1.0f + trackManager.speedRatio);

			m_JumpStart = trackManager.worldDistance;

			//character.animator.SetFloat(s_JumpingSpeedHash, animSpeed);
			m_Jumping = true;
		}
	}
	public void TapCloth()
    {
		//Debug.Log("TapCloth");
		if (interactiveCollider != null)
        {
			interactiveCollider.GetComponent<ClothInteractive>().currentCount++;

        }
    }


	public void ChangeLane(int direction)
	{
		//if (!m_IsRunning)
		//	return;

		int targetLane = m_CurrentLane + direction;

		if (targetLane < 0 || targetLane > 2)
			// Ignore, we are on the borders.
			return;

		m_CurrentLane = targetLane;
		m_TargetPosition = new Vector3((m_CurrentLane - 1) * trackManager.stepDistance, 0, 0);
	}

	void ChangeSpeed(int deltaSpeed)
    {
		if(trackManager.currentSpeed + deltaSpeed >= 0)
        {
			trackManager.currentSpeed += deltaSpeed;
		}
		
    }

}
