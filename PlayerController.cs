using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    public Transform OriginPoint;
    public Transform OriginPoint1;
    public Transform OriginPoint2;
    public Transform EOP;

    public float speed = 0.07f;
	public float jumpForce = 300f;
	public float gravityJump = 1.2f;
	public float gravitySlide = 5f;
	public float throwForce = 300f;
	public GameObject JetPack;
	public float jetPackForce = 50f;
	public ParticleSystem JetPackFire;
	public Transform throwPoint;
	public Transform smokePoint;
	public GameObject Bullet;
	public GameObject smokeFx;
	public GameObject jumpFx;
	public GameObject Magnet;
	public float magnetTimer = 10f;

	public AudioClip soundJump;
	public AudioClip soundThrow;
	public AudioClip soundCollectBullet;
	public AudioClip soundEatFruit;

	public BoxCollider2D boxColl1;
	public BoxCollider2D boxColl2;
	public Transform checkGround;
	public LayerMask LayerGround;
	public string walkTrigger = "Walk";
	public string isGroundBool = "isGround";
	public string slideBool = "Slide";
	public string thrownTrigger = "Thrown";
	public string dieTrigger = "Die";

	//private 
	private Animator anim;
	private Rigidbody2D rig;
	private bool play = false;
	private bool die = false;
	private bool isGrounded = true;
	[HideInInspector]
	public bool isUsingJetPack = false;
	private bool isJumpHold = false;
	private float gravityNormal;
	private bool isCannonFiring = false;
	private bool isBoost = false;
	private float timeStuck = 0.1f;

	void Awake(){
		Magnet.SetActive (false);		//Turn of the magnet when begin game
	}

	// Use this for initialization
	void Start () {
		//Set up variables
		rig = GetComponent<Rigidbody2D> ();
		gravityNormal = rig.gravityScale;		//save normal gravity scale
		anim = GetComponent<Animator> ();
		JetPack.SetActive (false);		//disable Jet pack object

		if(GlobalValue.isUsingJetpack){
			isUsingJetPack = true;

			rig.velocity = Vector2.zero;
			JetPack.SetActive (true);
		}
	}
	
	// Update is called once per frame
	void Update () {
		//This is the Controller for PC
		if (!die) {		//stop doing anything when player dead -
            See();

            if (Input.GetKeyDown (KeyCode.UpArrow)) {		//Only jump when player on the ground
				Jump();
			}
			if (Input.GetKeyUp (KeyCode.UpArrow)) {
				JumpOff ();
			}
			if (Input.GetKeyDown (KeyCode.RightArrow)) {
				Attack ();
			}
			if (Input.GetKeyDown (KeyCode.DownArrow)) {
				Slide (true);
			} 
			if (Input.GetKeyUp (KeyCode.DownArrow)) {
				Slide (false);
			}
			anim.SetFloat ("Height", rig.velocity.y);
		}
	}

	void FixedUpdate(){
		if (!die) {		//stop doing anything when player dead
			if (play && !isCannonFiring) {		//only moving when play varible is true and not fire by the Big Cannon
				transform.Translate (new Vector3 (speed, 0, 0));		//moving the player with speed 
			}
			if (isUsingJetPack && isJumpHold) {		//if player got JetPack mode, the Jump button will be used to raise the player up
				rig.AddForce (new Vector2 (0, jetPackForce));
			}

			//check if the player grounded
			if (Physics2D.OverlapCircle (checkGround.transform.position, 0.2f, LayerGround)) {
				anim.SetBool (isGroundBool, true);		//set animator
				isGrounded = true;
				isCannonFiring = false;	//if player fired out of the Cannon and hit the ground, allow moving
			} else {
				anim.SetBool (isGroundBool, false);		//set animator
				isGrounded = false;
			}

			if (rig.velocity.y == 0 && !isGrounded && !isUsingJetPack) {
				timeStuck -= Time.fixedDeltaTime;
				if (timeStuck <= 0)
					GameManager.instance.GameOver ();
			} else
				timeStuck = 0.1f;
		}
	} 

    void See(){

        RaycastHit2D hit1 = Physics2D.Raycast(OriginPoint.position,Vector2.right,1.2f);
        RaycastHit2D hit2 = Physics2D.Raycast(OriginPoint1.position, Vector2.right, 0.5f);
        RaycastHit2D hit3 = Physics2D.Raycast(EOP.position, Vector2.down, 0.5f);
        RaycastHit2D hit4 = Physics2D.Raycast(OriginPoint2.position, Vector2.right, 10f);

        if (hit1==true && hit1.collider.CompareTag("Monster")){
            Jump();
        }
        if (hit4 == true && hit4.collider.CompareTag("FloatingDirtBlock")==true) { 
            Slide(true);
        }
        else{
            Slide(false);
        }
        if (hit2 == true && hit2.collider.CompareTag("Enemy")){
            Jump();
        }
        if (hit3.collider == false){
            Jump();
        }

    }

	//Called by The Big Cannon
	public void CannonFire(){
		isCannonFiring = true;
		anim.SetTrigger (walkTrigger);
	}

	//Called by GameManager script
	public void Play(){
		if (anim != null)
			anim.SetTrigger (walkTrigger);
		play = true;
	}

	//Called by Controller UI and PC
	public void Jump(){
		if (!die) {		//stop doing anything when player dead
			isJumpHold = true;		//flag this bool to tell that user are holding the Jump buttons
			if (isUsingJetPack) {	//if using jet pack
				JetPackFire.emissionRate = 100f;	//inscrease fx effect
				JetPack.GetComponent<AudioSource> ().volume = 0.85f;		//inscrease jet pack sound volume
				rig.gravityScale = gravityNormal;

			}
			else if (isGrounded) {
				SoundManager.PlaySfx (soundJump);
				rig.gravityScale = gravityJump;
				rig.velocity = Vector2.zero;
				rig.AddForce (new Vector2 (0, jumpForce));
				Instantiate (jumpFx, smokePoint.position, Quaternion.identity);
			}
		}
	}

	//Called by Controller UI and PC
	public void JumpOff(){
		isJumpHold = false;
		if (isUsingJetPack) {
			JetPackFire.emissionRate = 25f;		
			JetPack.GetComponent<AudioSource> ().volume = 0.3f;
		} else
			rig.gravityScale = gravityNormal;
	}

	//
	//Slide
	//
	public void Slide(bool slide){
		if (!die) {
			anim.SetBool (slideBool, slide);
			if (slide) {
				boxColl1.enabled = false;		//turn the body collider off when sliding to avoid hit other collider
				boxColl2.enabled = false;
				StartCoroutine (CreateSmoke (0.1f));	//create smoke when sliding
				rig.gravityScale = gravitySlide;		//apply new gravity when sliding
			} else {
				boxColl1.enabled = true;		//turn the body collider on again
				boxColl2.enabled = true;
				StopAllCoroutines ();
				if(!isJumpHold)
					rig.gravityScale = gravityNormal;
			}
		}
	}

	IEnumerator CreateSmoke(float time){
		yield return new WaitForSeconds (time);
		if (isGrounded)	//only create smoke when player on the ground
			Instantiate (smokeFx, smokePoint.transform.position, Quaternion.identity);
		StartCoroutine (CreateSmoke (0.1f));	//create smoke when sliding
	}
	//
	//Called by Controller UI and PC
	public void Attack(){
		if (!die && !isCannonFiring) {
			if (GameManager.Bullets > 0) {		//only allow throw the bullet when the amount of bullet greater then zero
				GameManager.Bullets--;
				SoundManager.PlaySfx (soundThrow);
				anim.SetTrigger (thrownTrigger);		//set trigger to throw
				GameObject obj = Instantiate (Bullet, throwPoint.position, Quaternion.AngleAxis (30, Vector3.forward)) as GameObject;
				obj.GetComponent<Rigidbody2D> ().AddRelativeForce (new Vector2 (throwForce, 0));
			}
		}
	}

	//Called by GameManager script
	public void Dead(){
		if (!die) {

			die = true;
			anim.SetTrigger (dieTrigger);
			JetPack.SetActive (false);		//hide Jetpack when dead
			StopAllCoroutines ();
			//			rig.isKinematic = true;
			rig.velocity = Vector2.zero;
			rig.gravityScale = 0.5f;

			var boxCo = GetComponents<BoxCollider2D> ();
			foreach (var box in boxCo) {
				box.enabled = false;
			}
			var CirCo = GetComponents<CircleCollider2D> ();
			foreach (var cir in CirCo) {
				cir.enabled = false;
			}

		}
	}

	//Detect the gameobjects via their tag
	void OnTriggerEnter2D(Collider2D other){
		
		if (other.gameObject.CompareTag ("Fruit")) {
			other.GetComponent<CircleCollider2D> ().enabled = false;
			SoundManager.PlaySfx (soundEatFruit, 0.5f);
			GameManager.Hearts++;
			other.gameObject.GetComponent<Animator> ().SetTrigger ("Collected");
		}
		else if (other.gameObject.CompareTag ("Bullet")) {
			SoundManager.PlaySfx (soundCollectBullet);
			GameManager.Bullets += 10;
			Destroy (other.gameObject);
		}
		else if (other.gameObject.CompareTag ("Magnet")) {
			SoundManager.PlaySfx (soundCollectBullet);
			Magnet.SetActive (true);
			StartCoroutine (WaitAndDisableMagnet (magnetTimer));
			Destroy (other.gameObject);
		}
		else if (other.gameObject.CompareTag ("Star")) {
			other.GetComponent<CircleCollider2D> ().enabled = false;
			SoundManager.PlaySfx (soundCollectBullet, 0.7f);
			GameManager.Stars++;
			GameManager.Score += 10;
			other.gameObject.GetComponent<Animator> ().SetTrigger ("Collected");
		}

		else if (other.gameObject.CompareTag ("Bridge")) {
//			Debug.Log ("bridge");
			other.gameObject.SendMessage ("Work", SendMessageOptions.DontRequireReceiver);
		}
		else if (other.gameObject.CompareTag ("JetPack")) {
			isUsingJetPack = !isUsingJetPack;
			rig.velocity = Vector2.zero;
			JetPack.SetActive (isUsingJetPack);
			Destroy (other.gameObject);
		}
		else if (other.gameObject.CompareTag ("SpeedBoost")) {
			if (!isBoost) {
				isBoost = true;
				speed *= 1.45f;
			} else {
				isBoost = false;
				speed /= 1.45f;
			}

			Destroy (other.gameObject);
		} 
	}

	//Disable the Magnet after the time delay
	IEnumerator WaitAndDisableMagnet(float time){
        yield return new WaitForSeconds(time);
        Magnet.SetActive(false);
	}

	//Detect the Enemy collider and send Game over to GameManager script
	void OnCollisionEnter2D(Collision2D other){
		if (other.gameObject.CompareTag ("Enemy")) {
			GameManager.instance.GameOver ();
		}
	}
}
