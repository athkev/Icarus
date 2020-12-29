using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//while on attack animation disable flip()
//only able to flip at the next attack animation
//use horizontalMove_animation for scripted movement

public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D controller;
    public Animator animator;
    public ParticleSystem dust;
    public ParticleSystem dashps;
    // Start is called before the first frame update

    public float runSpeed = 40f;
    public float walkSpeed = 15f;
    public float horizontalMove = 0f;
    //public float horizontalMove_animation = 0f;
    public float currentCharSpeed = 0;
    public bool jump = false;
    bool crouch = false;
    // running + crouch -> slide, gradually slow down into crouch 
    public float slide_speed = 120f; //initial speed of slide
    public float slide_minimumspeed = 70f;
    public float slide_decreasingrate = 0.995f;
    public int state = 0; //0:idle 1:walk 2:run 3:attacking 4:sliding 5:slideEnded
    int state_reserved = 0; //jump while running stays at run state after landing
    


    public float tapSpeed = 0.2f; //in seconds
    public float tapSpeed_attack = 0.4f;
    private float lastTapTime_left = 0;
    int counter_left = 0;
    private float lastTapTime_right = 0;
    int counter_right = 0;
    private float lastTapTime_attack = 0;
    public int counter_attack = 0;
    bool holdD = false;

    float attackCD = 100;
    float attackCDCounter = 100; //if attack, counter starts from 0

    public int max_jumps = 2;
    public int jumpsleft = 2;
    public bool jumpKeyHeld = false;
    public bool grounded;

    //wall slide/jump
    public Transform wallCheck;
    public Transform wallCheckBack;
    public bool isTouchingWall;
    public bool isTouchingWallBack;
    bool wallSlide;
    public float wallSlideSpeed;
    public LayerMask groundMask;

    //dash
    public Transform pivot;
    public Transform pivotObject;
    public Vector2 relativePosition_cursor;
    public Vector2 dashVector;
    public bool dashAbled = true;
    int dashCoolDown = 100;
    public int dashCoolDownCounter = 0;

    //moving platform
    bool onPlatform;

    public LayerMask enemyLayers;
    public Collider2D hitbox;

    public int leftClickState = 1; //1: attack 2: create platform 3: grapple rope

    // Update is called once per frame
    void Update()
    {
        ///////////////
        /// Player Inputs (Left & Right, walk and run)
        ///////////////

        //animator.SetFloat("Speed", Mathf.Abs(input)); //sent to animator
        if (state == 2) //if running is initialized, it stays running until stopped/interrupted
        {
            if (!(Input.GetKey("d") || Input.GetKey("a"))) state = 0; //no input
            //add more condiitons   i.e got hit, took damage, etc
        }
        else if (state == 3)    //if player is in animation of attack / stunned
        {
            //do nothing for now
            state = 3;
        }
        else if (Input.GetKeyDown("1")) leftClickState = 1;
        else if (Input.GetKeyDown("2")) leftClickState = 2;
        else if (Input.GetKeyDown("3")) leftClickState = 3;

        else if (Input.GetKeyDown("d"))
        {
            counter_right += 1;
            if (counter_right == 2 && (Time.time - lastTapTime_right) < tapSpeed)
            {
                if (state == 3) state_reserved = 2;
                else state = 2;
            }
            else if (state == 3 || state == 4) { } //do nothing
            else
            {
                state = 1;
            }

            lastTapTime_right = Time.time;
        }
        else if (Input.GetKeyDown("a"))
        {
            counter_left += 1;
            if (counter_left == 2 && (Time.time - lastTapTime_left) < tapSpeed)
            {
                if (state == 3) state_reserved = 2;
                else state = 2;
            }
            else if (state == 3 || state == 4) { } //do nothing
            else
            {
                state = 1;
            }

            lastTapTime_left = Time.time;
        }
        else if ((Input.GetKey("a")) || (Input.GetKey("d")))
        {
            //do nothing
            if (state == 1) state = 1;
            if (state == 0) state = 1;
            //enable dash attack option
        }

        else //no input
        {
            state = 0;
        }

        if ((Time.time - lastTapTime_left) > tapSpeed) counter_left = 0;
        if ((Time.time - lastTapTime_right) > tapSpeed) counter_right = 0;

        if (state == 0)
        {
            animator.SetFloat("Speed", 0);
            currentCharSpeed = 0;
            if (counter_attack != 0) resetAttack();
        }
        if (state == 1)
        {
            animator.SetFloat("Speed", 1);
            currentCharSpeed = walkSpeed;
            if (counter_attack != 0) resetAttack();
        }
        if (state == 2)
        {
            if (Input.GetKey("d") && Input.GetKey("a")) animator.SetFloat("Speed", 0); //if both left and right is pressed = not moving but have run state
            else animator.SetFloat("Speed", 10);
            currentCharSpeed = runSpeed;
            if (counter_attack != 0) resetAttack();
        }
        //if sliding, *= by 0.8 until reaches 
        if (state == 4 || state == 5)
        {
            createDust();
            if (state == 4)
            {
                currentCharSpeed = slide_speed;
            }
            else
            {
                if (currentCharSpeed > slide_minimumspeed) currentCharSpeed *= slide_decreasingrate;
                else if (currentCharSpeed < slide_minimumspeed) state = 0;

            }

            if (controller.m_FacingRight && Input.GetKey("a")) state = 2;
            else if (!controller.m_FacingRight && Input.GetKey("d")) state = 2;
        }

        if (state == 3)
        {
            //do nothing
            //or scripted movements
            if (controller.m_FacingRight) horizontalMove = 1 * currentCharSpeed;
            else horizontalMove = -1 * currentCharSpeed;
        }
        else horizontalMove = Input.GetAxisRaw("Horizontal") * currentCharSpeed;
        ///////////////
        /// Character final movement calc
        ///////////////











        ///////////////
        /// Player Inputs (Jump & other movements)
        ///////////////

        if (Input.GetButtonDown("Jump"))
        {
            jumpKeyHeld = true;
            if (wallSlide)
            {
                jump = true;
                animator.SetBool("IsJumping", true);
                jumpsleft = max_jumps;
                //doesn't decrement jump counter
            }
            else if (state == 3 && animator.GetBool("CanJump") == true)
            {
                state = state_reserved;
                jump = true;
                animator.SetBool("IsJumping", true);
                jumpsleft -= 1;

            }
            else if (state == 0 || state == 1 || state == 2 || state == 4 || state == 5)
            {
                jump = true;
                animator.SetBool("IsJumping", true);
                jumpsleft -= 1;
            }

            animator.SetBool("dash", false);
            if (jump && !wallSlide) createDust();

        }
        if (Input.GetButtonUp("Jump"))
        {
            jumpKeyHeld = false;
        }

        if (Input.GetButtonDown("Crouch"))
        {
            crouch = true;
            if (state == 2 && grounded)
            {
                //sliding, gradually slowdown
                animator.SetTrigger("slide");
                state = 4;
            }
        }
        else if (Input.GetButtonUp("Crouch"))
        {
            crouch = false;
            if (state == 5) state = 2;
        }


        

        //dash (mouse rightclick), slow time when held
        if (Input.GetMouseButton(1) && dashAbled && dashCoolDownCounter == 0)
        {
            controller.m_Rigidbody2D.velocity = Vector2.zero;

            //controller.m_Rigidbody2D.velocity += new Vector2(dashVector.x, dashVector.y).normalized * 30;
            controller.m_Rigidbody2D.AddForce(dashVector.normalized * 2000);
            dashAbled = false;
            dashCoolDownCounter = dashCoolDown;

            animator.SetBool("dash", true);
            createDust();

            if (dashVector.y<0)
            {
                createDashPS(- Vector2.Angle(new Vector2(1, 0), -dashVector));
            }
            else
            {
                createDashPS(Vector2.Angle(new Vector2(1, 0), -dashVector));
            }


        }
        if (!dashAbled && dashCoolDownCounter > 0)
        {
            dashCoolDownCounter -= 1;
        }
        if (dashCoolDownCounter == 0)
        {
            dashAbled = true;
        }
        





        ///////////////
        /// Player Inputs (Attacks WIP)
        ///////////////
        /* supports holding button attack
        if (Input.GetButtonDown("Attack"))
        {
            lastTapTime_attack = Time.time;
        }


        if (Input.GetButtonUp("Attack"))
        {
            if ((Time.time - lastTapTime_attack) < tapSpeed_attack) //tapped attack key -> D
            {
                Attack();
            }
            else if (holdD)
            {
                holdD = false; //reset hold D
            }
        }
        else if ((Time.time - lastTapTime_attack) > tapSpeed_attack && Input.GetButton("Attack") && holdD == false) //held attack key -> D hold
        {
            Debug.Log("D hold");
            holdD = true;
            //call function for D hold
        }
        */

        //holding attack button will continuously attack instead
        //allow jumpcancel out of attack animation
        /*
        if (Input.GetMouseButton(0) && leftClickState == 1)
        {
            if (attackCDCounter == attackCD) attackCDCounter = 0;
            Attack();
        }
        */

        ///////////////////create small platform if leftclick with state =2 is pressed
        


        //slide wall detection
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, 0.4f, groundMask);
        isTouchingWallBack = Physics2D.OverlapCircle(wallCheckBack.position, 0.4f, groundMask);
        if (isTouchingWall && !grounded && ((Input.GetAxisRaw("Horizontal") < 0 && !controller.m_FacingRight)||
            Input.GetAxisRaw("Horizontal") > 0 && controller.m_FacingRight))
        {
            wallSlide = true;
            controller.Flip();
        }
        else if (isTouchingWallBack && !grounded && ((Input.GetAxisRaw("Horizontal") < 0 && controller.m_FacingRight) ||
            Input.GetAxisRaw("Horizontal") > 0 && !controller.m_FacingRight))
        {
            wallSlide = true;
        }
        else wallSlide = false;

        if (wallSlide)
        {
            controller.m_Rigidbody2D.velocity = new Vector2(controller.m_Rigidbody2D.velocity.x,
                Mathf.Clamp(controller.m_Rigidbody2D.velocity.y, -wallSlideSpeed, float.MaxValue));
        }






    }

    public void OnLanding ()
    {
        animator.SetBool("IsJumping", false);
    }

    public void OnCrouching (bool isCrouching)
    {
        animator.SetBool("IsCrouching", isCrouching);
    }

    void FixedUpdate ()
    {

        //moving character  
        controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump, jumpsleft, jumpKeyHeld, state, wallSlide);
        animator.SetFloat("vertical_velocity", controller.m_Rigidbody2D.velocity.y);
        animator.SetBool("IsGrounded", grounded);
        animator.SetInteger("state", state);
        animator.SetBool("IsWallSliding", wallSlide);

        dashVector = pivotObject.position - pivot.position;

        jump = false;
        grounded = controller.m_Grounded;
        if (attackCDCounter < attackCD) attackCDCounter += 1; 
        if(grounded)
        {
            jump = false;
            jumpsleft = max_jumps;

            animator.SetBool("dash", false);
        }



    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trig");
    }

    void Attack()
    {
        /*
        if (!animator.GetBool("Attack") && animator.GetInteger("Attack_counter") == counter_attack)
        {
            counter_attack += 1;
        }
        if (!animator.GetBool("Attack")) //before animation init
        {
            animator.SetInteger("Attack_counter", counter_attack);
        }
        */
        //create particle
        

        //initialize attack animation depending on the angle
        //

        animator.SetBool("Attack",true);


    }

    void resetAttack()
    { //reset counter if called
        counter_attack = 0;
        animator.SetInteger("Attack_counter", 0);
        animator.SetBool("Attack", false);
    }

    public void AlertObservers(string message)
    {
        if (message.Equals("AttackAnimationStarted"))
        {
            // Do other things based on an attack ending.

            if ((Input.GetAxisRaw("Horizontal") > 0 && !controller.m_FacingRight) || (Input.GetAxisRaw("Horizontal") < 0 && controller.m_FacingRight)) controller.Flip();
            if (state == 0 || state == 1) //initial attack while idle/walking
            {
                state = 3;
            }
            animator.SetBool("Attack", false);
            animator.SetBool("CanJump", false);


        }   

        if (message.Equals("AttackAnimationEnded"))
        {
            // Do other things based on an attack ending.

            if (!animator.GetBool("Attack") || (animator.GetBool("Attack") && counter_attack > 3))
            {
                state = 0;
                resetAttack();
                if (state_reserved != 0)
                {
                    state = state_reserved;
                    state_reserved = 0;
                }

            }
            animator.SetBool("CanJump", true);


        }

        if (message.Equals("JumpCancel"))
        {
            animator.SetBool("CanJump", true);
        }

        if (message.Equals("EndOfSlide"))
        {
            if (!crouch) state = 2;
            else state = 5;
        }
        /*
        if (message.Equals("EndofAttackString"))
        {
            state = 0;
            counter_attack = 0;
            animator.SetInteger("Attack_counter", 0);
            animator.SetBool("Attack")
        }
        */
    }

    public void createDust()
    {
        dust.Play();
    }

    public void createDashPS(float angle)
    {
        dashps.transform.rotation = Quaternion.identity;

        if (controller.m_FacingRight) dashps.transform.Rotate(new Vector3(0, 0, angle));
        else dashps.transform.Rotate(new Vector3(0, 0, -angle));


        dashps.Play();
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground") && !wallSlide)
        {
            this.transform.parent = other.gameObject.transform;
            onPlatform = true;
        }
    }

    public void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            this.transform.parent = null;
            onPlatform = false;
            //controller.m_Rigidbody2D.velocity = other.gameObject.GetComponent<Rigidbody>().velocity;
            //controller.m_Rigidbody2D.velocity = 
        }
    }

}
//천본앵