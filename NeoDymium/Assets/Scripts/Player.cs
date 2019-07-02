using UnityEngine;

public class Player : MonoBehaviour
{
	public static Player inst;

    public float moveSpeed;
	public Transform cam;
	[Space (10)]
	public string punchAnimationName;
	public AnimationClip[] punchAnimationClips;
	public Collider armCollider;
	[Space (10)]
	public int maxHealth;
	public int currentHealth;
	public int[] comboDamage;
	public int bulletDamage;

	int comboStep;
	bool canPunch;
	float comboTimer;

	Rigidbody rb;
	Animator anim;

	void Awake () 
	{
		inst = this;
	}

	void Start () 
	{
		canPunch = true;

		rb = GetComponent<Rigidbody> ();
		anim = GetComponent<Animator> ();

		armCollider.enabled = false;

		comboTimer = 0;
		comboStep = 0;

		currentHealth = maxHealth;
	}

	void Update () 
	{
		rb.velocity = Vector3.zero;
		transform.eulerAngles = new Vector3 (0, cam.eulerAngles.y, 0);
		if (Input.GetKey (key: KeyCode.W))
			rb.velocity += transform.forward * moveSpeed;
		if (Input.GetKey (key: KeyCode.A))
			rb.velocity += -transform.right * moveSpeed;
		if (Input.GetKey (key: KeyCode.S))
			rb.velocity += -transform.forward * moveSpeed;
		if (Input.GetKey (key: KeyCode.D))
			rb.velocity += transform.right * moveSpeed;

		if (Input.GetKeyDown (key: KeyCode.R) && canPunch)
			Punch ();
		
		if (comboTimer > 0) 
			comboTimer = Mathf.Max (comboTimer - Time.deltaTime, 0);
		else 
			comboStep = 0;
	}

	void Punch () 
	{
		canPunch = false;
		comboStep = Mathf.Min (++comboStep, 3);
		anim.Play (punchAnimationName + comboStep);
		armCollider.enabled = true;
		comboTimer = 0.5f + punchAnimationClips[comboStep - 1].length;
		Invoke ("CanPunch", punchAnimationClips[comboStep - 1].length);
		if (comboStep == 3)
			comboStep = 0;
	}

	void CanPunch () 
	{
		canPunch = true;
		armCollider.enabled = false;
	}

	void OnCollisionEnter (Collision other) 
	{
		if (other.gameObject.layer == 10) //layer name is EnemyProj
			currentHealth -= bulletDamage;
	}

	public int CalculateDamage () 
	{
		return comboDamage[comboStep];
	}
}