using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.InputSystem;


/** ToDo: */
/**
  * Jump force stopes if no direction input feels wrong!! *
 * */
public class MovePlayerController : MonoBehaviour
{
    public float speed;
    public float jumpForce;
    private float moveInput;

    

    private Rigidbody2D rb;
    private Animator anim;

    public Joystick joystick;

    private bool isRunning;
    
    //private bool isTouchingWall;
    private bool isWallSliding;

    private bool facingRight = true;
    private int facingDirection = 1;

    private bool isGrounded;
    public Transform groundCheck;
    public float checkRadius;
    public LayerMask whatIsGround;
    public LayerMask whatIsWall;

    private int extraJumps;
    public int extraJumpsValue;


    //private float jumpTimer;
    //private bool canNormalJump;
    private bool isAtemptingToJump;
    //public float jumpTimerSet = 0.15f;
    public float variableJumpHeightMultiplier = 0.5f;
    //private bool checkJumpMultiplier;



    private bool isTouchingFront;
    public Transform frontCheck;

    public Transform HitBox_Check_A;
    public Transform HitBox_Check_B;
    public Transform HitBox_Check_C;

    public float check_HitBox_Radius_A;
    public float check_HitBox_Radius_B;
    public float check_HitBox_Radius_C;


    //public Transform wallCheck;
    public float wallCheckDistance;
    private bool wallSliding;
    public float wallSlidingSpeed;

    private bool wallJumping;
    public float xWallForce;
    public float yWallForce;
    public float wallJumpTime;



    private bool isAttacking;// = false;
    private bool isFirstAttack;// = false;
    private bool isSecondAttack;
    private bool isThirdAttack;


    private bool canAttack;

    private bool isAttackingA = false;
    private bool isAttackingB = false;
    private bool isAttackingC = false;
    private bool isAttackingJump = false;

    //anim.SetBool("A_Attack_Started", isAttack_A_Started);
    private bool isAttack_A_Started;
    private bool isAttack_A_Ended;

    private bool isAttack_B_Started;
    private bool isAttack_B_Ended;

    private bool isAttack_C_Started;
    private bool isAttack_C_Ended;



    //private bool canMove = true;

    private float lastInputTime = Mathf.NegativeInfinity;

    [SerializeField]
    private Transform attack1HitBoxPos_A, attack1HitBoxPos_B, attack1HitBoxPos_C;

    [SerializeField]
    private float inputTimer, attack_A_Damage, attack_B_Damage, attack_C_Damage;

    private LayerMask whatIsDamageable;


    private void Start()
    {
        extraJumps = extraJumpsValue;
        rb = GetComponent<Rigidbody2D>();

        

        anim = GetComponent<Animator>();

    }
    private bool AnimatorIsPlaying()
    {
        return anim.GetCurrentAnimatorStateInfo(0).length >
               anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    bool AnimatorIsPlaying(string stateName)
    {
        return AnimatorIsPlaying() && anim.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
        isTouchingFront = Physics2D.OverlapCircle(frontCheck.position, checkRadius, whatIsWall);
        //isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsWall);
        if (AnimatorIsPlaying("isAttackingB"))
        {
            //Debug.Log("B is attacking");
        }

        //Debug.Log("Animation is running: " + AnimatorIsPlaying("isAttackingB"));

        if (!wallJumping)
        {

            
            //  ASDW KeyBoard Input Old InputSystem 
            if (Input.GetAxisRaw("Horizontal") != 0 ) // && !isAttackingB) //&& !isAttackingB && !isAttackingC) 
            {
                moveInput = Input.GetAxisRaw("Horizontal");
                rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);
            }
            else
            {
                // JoyStick Controles Old InputSystem 
                if (joystick.Horizontal >= .8f)
                {
                    moveInput = joystick.Horizontal;
                }
                else if (joystick.Horizontal <= -.8f)
                {
                    moveInput = joystick.Horizontal;
                }
                else
                {
                    moveInput = 0;
                }
                rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);
            }
            

        }


        if (facingRight == false && moveInput > 0)
        {
            Flip();
        } else if (facingRight == true && moveInput < 0)
        {
            Flip();
        }

        if (Mathf.Abs(rb.velocity.x) >= 0.01f && isGrounded)
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }

        if(!isGrounded && isTouchingFront)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }


        //CheckAttacks();


    }
    private void UpdateAnimations()
    {
        anim.SetBool("isRunning", isRunning);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isAttackingJump", isAttackingJump);
        anim.SetBool("isWallSliding", isWallSliding);
    }

    private void Update()
    {
        
        if (isGrounded == true)
        {
            extraJumps = extraJumpsValue;
        }

        // Keyboard kontrolles with inn Update
        if(Input.GetKeyDown(KeyCode.Space) && extraJumps > 0)
        {
            NormalJump();
            extraJumps--;
            
        }
        else
        {
            isAtemptingToJump = true;
        }
        
        if(Input.GetKeyDown(KeyCode.Space) && extraJumps == 0 && isGrounded)
        {
            NormalJump();
        }
        

        if(isTouchingFront == true && isGrounded == false && moveInput != 0)
        {
            wallSliding = true;
        }else
        {
            wallSliding = false;
        }

        if (wallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }

        if(Input.GetKeyDown(KeyCode.Space) && wallSliding == true)
        {
            wallJumping = true;
            Invoke("SetWallJumpingToFalse", wallJumpTime);
        }

        /** WallJumping */
        if(wallJumping == true)
        {
            rb.velocity = new Vector2(xWallForce * -moveInput, yWallForce);

        }


        if (Input.GetKeyUp(KeyCode.Space) && isAtemptingToJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeightMultiplier);
        }

        UpdateAnimations();
        CheckAttacks();
    }

    public void OnJumpAttackInput(InputAction.CallbackContext context)
    {
        if (context.started )
        {
            //
            if (isGrounded)
            {
                canAttack = true; // input is true
                anim.SetBool("canAttack", canAttack);
                lastInputTime = Time.time;
                //CheckAttacks();
            }
            //Debug.Log("isFirstAttack: " + isFirstAttack);
            //Debug.Log("isSecondAttack: " + isSecondAttack);
            //Debug.Log("canAttack: " + canAttack);
        }

        if (context.canceled ){  }
    }


    /** Default value of a bool is false NB !!! */
    private void CheckAttacks()
    {
        if (canAttack) // Button Click Enabled - set to true
        {
            //Debug.Log("CheckAttacks()");

            //Debug.Log("isAttack_A start: " + isAttack_A_Started);
            //Debug.Log("isAttack_A_Ended: " + isAttack_A_Ended);

            //Debug.Log("isAttack_B start: " + isAttack_B_Started);
            //Debug.Log("isAttack_B_Ended: " + isAttack_B_Ended);

            //Debug.Log("isAttack_C start: " + isAttack_C_Started);
            //Debug.Log("isAttack_C_Ended: " + isAttack_C_Ended);


            //Debug.Log("isThirdAttack: " + isThirdAttack);
            
            
            if (!isAttacking && !isFirstAttack) // false
            {
                canAttack = false;  // Button Clicked set back to false since this running in update !!
                //anim.SetBool("attack1", true);
                isAttacking = true; // true 
                anim.SetBool("isAttacking", isAttacking);

                //Debug.Log("isFirstAttack is befor sett: " + isFirstAttack);

                isFirstAttack = !isFirstAttack; // sets the var the - to what it is ???

                //Debug.Log("isFirstAttack is after sett: " + isFirstAttack);

                anim.SetBool("firstAttack", isFirstAttack);
                
            }

            if (!isAttacking && !isSecondAttack) // false
            {
                canAttack = false;  // Button Clicked set back to false since this running in update !!
                //anim.SetBool("attack1", true);
                isAttacking = true; // true 
                anim.SetBool("isAttacking", isAttacking);

                //Debug.Log("isFirstAttack is befor sett: " + isFirstAttack);

                isSecondAttack = !isSecondAttack; // sets the var the - to what it is ???

                //Debug.Log("isFirstAttack is after sett: " + isFirstAttack);

                anim.SetBool("secondAttack", isSecondAttack);

            }

            if (!isAttacking && !isThirdAttack) // false
            {
                canAttack = false;  // Button Clicked set back to false since this running in update !!
                //anim.SetBool("attack1", true);
                isAttacking = true; // true 
                anim.SetBool("isAttacking", isAttacking);

                //Debug.Log("isFirstAttack is befor sett: " + isFirstAttack);

                isThirdAttack = !isThirdAttack; // sets the var the - to what it is ???

                //Debug.Log("isFirstAttack is after sett: " + isFirstAttack);

                anim.SetBool("thiredAttack", isThirdAttack);

            }

        }   
                
        

        if(Time.time >= lastInputTime + inputTimer)
        {
            canAttack = false;
        }
    }



    

    private void StartAttack( string anim_name)
    {
        //Debug.Log("Attack animation Start: " + anim_name);


        switch (anim_name)
        {
            case "A":
                Debug.Log("Attack Anim Started on A");
                isAttack_A_Started = true;
                anim.SetBool("A_Attack_Started", isAttack_A_Started);
                break;
            case "B":
                Debug.Log("Attack Anim Started on B");
                isAttack_B_Started = true;
                anim.SetBool("B_Attack_Started", isAttack_B_Started);
                break;
            case "C":
                Debug.Log("Attack Anim Started on C");
                isAttack_C_Started = true;
                anim.SetBool("C_Attack_Started", isAttack_C_Started);
                break;
        }


    }

    private void EndAttack( string anim_name)
    {
        //Debug.Log("Attack animation End: " + anim_name);

        switch (anim_name)
        {
            case "A":
                isAttacking = false;
                anim.SetBool("isAttacking", isAttacking);

                isAttack_A_Ended = true;
                anim.SetBool("A_Attack_Ended", isAttack_A_Ended);

                anim.SetBool("firstAttack", false);
                Debug.Log("Attack Anim Ended on A");

                isThirdAttack = false;
                anim.SetBool("thiredAttack", isThirdAttack);

                break;
            case "B":
                isAttacking = false;
                anim.SetBool("isAttacking", isAttacking);

                isAttack_B_Ended = true;
                anim.SetBool("B_Attack_Ended", isAttack_B_Ended);

                anim.SetBool("secondAttack", false);
                Debug.Log("Attack Anim Ended on B");
                break;
            case "C":
                isAttacking = false;
                anim.SetBool("isAttacking", isAttacking);

                isAttack_C_Ended = true;
                anim.SetBool("C_Attack_Ended", isAttack_C_Ended);

                anim.SetBool("thiredAttack", false);
                Debug.Log("Attack Anim Ended on C");

                isFirstAttack = false;
                anim.SetBool("firstAttack", isFirstAttack);

                isSecondAttack = false;
                anim.SetBool("secondAttack", isSecondAttack);

                break;
        }

    }

    private void CheckAttackHitBoxAnim(string anim_name)
    {
        //Debug.Log("Attack hitbox: " + anim_name);
        switch (anim_name)
        {
            case "A":
                break;
            case "B":
                break;
            case "C":
                break;
        }
    }

    private void CheckAttackHitBox()
    {
        Collider2D[] detectedObjects1 = Physics2D.OverlapCircleAll(attack1HitBoxPos_A.position, check_HitBox_Radius_A, whatIsDamageable);
        Collider2D[] detectedObjects2 = Physics2D.OverlapCircleAll(attack1HitBoxPos_B.position, check_HitBox_Radius_B, whatIsDamageable);
        Collider2D[] detectedObjects3 = Physics2D.OverlapCircleAll(attack1HitBoxPos_C.position, check_HitBox_Radius_C, whatIsDamageable);

        //attackDetails.damageAmount = attack1Damage;
        //attackDetails.position = transform.position;
        //attackDetails.stunDamageAmount = stunDamageAmount;

        //foreach (Collider2D collider in detectedObjects)
        //{
        //    collider.transform.parent.SendMessage("Damage", attackDetails);
        //Instantiate hit particle
        //}
    }

    /** Jump Input Button New InputSystem */

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.started && extraJumps > 0)
        {
            rb.velocity = Vector2.up * jumpForce;
            extraJumps--;
        }else if( context.started && extraJumps == 0 && isGrounded)
        {
            rb.velocity = Vector2.up * jumpForce;
        }

        if(context.canceled && isAtemptingToJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeightMultiplier);
        }


        if (context.started && wallSliding == true)
        {
            wallJumping = true;
            Invoke("SetWallJumpingToFalse", wallJumpTime);
        }
    }

    /** works but need to change - trye with imagePooling !!?*/
    public void OnDashInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            rb.AddForce(new Vector2(20000f * facingDirection, 0.0f ));
            //Debug.Log("Dash called: " + facingDirection );
        }
    }




    private void NormalJump()
    {
       rb.velocity = new Vector2(rb.velocity.x, jumpForce);
       isAtemptingToJump = false;
    }

    private void Flip()
    {
        //facingRight = !facingRight;
        //Vector3 Scaler = transform.localScale;
        //Scaler.x *= -1;
        //transform.localScale = Scaler;
        facingDirection *= -1;
        facingRight = !facingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        Gizmos.DrawWireSphere(frontCheck.position, checkRadius);

        Gizmos.DrawWireSphere(HitBox_Check_A.position, check_HitBox_Radius_A);
        Gizmos.DrawWireSphere(HitBox_Check_B.position, check_HitBox_Radius_B);
        Gizmos.DrawWireSphere(HitBox_Check_C.position, check_HitBox_Radius_C); 
        //Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }

    private void SetWallJumpingToFalse()
    {
        wallJumping = false;
    }
}
