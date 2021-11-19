//PlayerController copy
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.InputSystem;


/** Back up of PlayerController in case we fuck it up this is working */
/** Minus wall jumping trying to fix */

public class PlayerController : MonoBehaviour
{
    private float jumpTimer;
    private float turnTimer;
    private float wallJumpTimer;

    private int facingDirection = 1;
    private float movementInputDirection;
    private int amountOfJumpsLeft;
    private int lastWallJumpDirection;

    public Joystick joystick;


    public Vector2 RawMovementInput { get; private set; }


    private bool isFacingRight = true;
    private bool isRunning;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool canJump;
    private bool canNormalJump;
    private bool canWallJump;
    private bool isAtemptingToJump;
    private bool checkJumpMultiplier;
    private bool canMove;
    private bool canFlip;
    private bool hasWallJumped;

    private Rigidbody2D rb;
    private Animator anim;

    public int amountOfJumps = 1;
    public float movementSpeed = 10.0f;
    public float jumpForce = 16.0f;
    public float groundCheckRadius;
    public float wallCheckDistance;
    public float wallSlidingSpeed;
    public float movmentForceInAir;
    public float airDragMultiplier = 0.95f;
    public float variableJumpHeightMultiplier = 0.5f;
    //public float wallHopForce;
    public float wallJumpForce;
    public float jumpTimerSet = 0.15f;
    public float turnTimerSet = 0.1f;
    public float wallJumpTimerSet = 0.5f;

    //public Vector2 wallHopDirection;
    public Vector2 wallJumpDirection;

    public Transform groundCheck;
    public Transform wallCheck;

    public LayerMask whatIsGround;
    public LayerMask wahtIsWall;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();



        //amountOfJumpsLeft = amountOfJumps;

        //wallHopDirection.Normalize();
        //wallJumpDirection.Normalize();
    }

    // Update is called once per frame
    void Update()
    {
        //CheckInput(); // tried with old and new input system. 
        // new for joystick was bad swithing from 0 to 1 even when the stick was 
        //allwas in on direction.
        CheckMovementDirection();
        CheckInputJoyStickOldInputSystem();
        
        UpdateAnimations();
        CheckIfCanJump();
        CheckIfWallSliding();
        CheckJump();
    }

    private void FixedUpdate()
    {
        ApplayMovement();
        CheckSurroundings();
    }

    private void CheckIfWallSliding()
    {
        if (isTouchingWall && movementInputDirection == facingDirection && rb.velocity.y < 0)
        {
            isWallSliding = true;
            //canWallJump = true; // I added etc testing

            //Debug.Log("CheckIfWallSliding : Test isWallSliding then canWallJump" + canWallJump); Test OK
        }
        else
        {
            isWallSliding = false;
            //canWallJump = false;
        }
    }

    private void CheckSurroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, wahtIsWall); //whatIsGround); //wahtIsWall);
    }

    private void CheckIfCanJump()
    {
        if (isGrounded && rb.velocity.y <= 0.01f)
        {
            amountOfJumpsLeft = amountOfJumps;
        }

        if (isTouchingWall) // added !isGrounded ??
        {
            canWallJump = true;
            checkJumpMultiplier = false;
            //canWallJump = true;
        }

        if (amountOfJumpsLeft <= 0)
        {
            canNormalJump = false;
        }
        else
        {
            canNormalJump = true;
        }

        /*
        if (isWallSliding)
        {
            canJump = true;
        }
        */

    }

    private void CheckMovementDirection()
    {
        if (isFacingRight && movementInputDirection < 0)
        {
            Flip();
        }
        else if (!isFacingRight && movementInputDirection > 0)
        {
            Flip();
        }

        if (Mathf.Abs(rb.velocity.x) >= 0.01f) //if(rb.velocity.x != 0)
        {

            Debug.Log("isRunning: " + isRunning);
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }

    }

    private void UpdateAnimations()
    {
        anim.SetBool("isRunning", isRunning);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isWallSliding", isWallSliding);
    }

    // Usees the new input on joystick but don't work so well - check input from the controller 
    // looks to send 0 even when the stick i smoving !!! 
   /* 
    public void OnMoveInput(InputAction.CallbackContext context)
    {
        RawMovementInput = context.ReadValue<Vector2>();

        if (Mathf.Abs(RawMovementInput.x) > 0.5f)
        {
            movementInputDirection = (int)(RawMovementInput * Vector2.right).normalized.x;
        }
        else
        {
            movementInputDirection = 0;
        }
        //Debug.Log("Raw data + " + RawMovementInput + " x Only: " + movementInputDirection);
    }
    */


    // New Input System for the button Calls
    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {

            if (isGrounded || (amountOfJumpsLeft > 0 && isTouchingWall))
            {
                NormalJump();
            }
            else
            {
                jumpTimer = jumpTimerSet;
                isAtemptingToJump = true;
            }
        }


        if (context.started)
        {
            if(!isGrounded && isTouchingWall)
            {
                canWallJump = true;
                //Invoke("setWalljumpingToFalse()", wallJumpTimer);
            }
        }

        if (!isGrounded && movementInputDirection != facingDirection)
        {
            canMove = false;
            canFlip = false;

            turnTimer = turnTimerSet;
        }
/*
        if (!canMove)
        {
            turnTimer -= Time.deltaTime;

            if (turnTimer <= 0)
            {
                canMove = true;
                canFlip = true;
            }
        }
*/
        if (context.canceled && checkJumpMultiplier)
        {
            checkJumpMultiplier = false;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeightMultiplier);

        }

        //Debug.Log("jump");
    }


    // Joystick Old InputSystem
    private void CheckInputJoyStickOldInputSystem()
    {

        if (!isWallSliding)
        {

            if (joystick.Horizontal >= .8f)
            {
                movementInputDirection = joystick.Horizontal;
            }
            else if (joystick.Horizontal <= -.8f)
            {
                movementInputDirection = joystick.Horizontal;
            }
            else
            {
                movementInputDirection = 0;
            }

            /** ASDW KeyBoard input */
            if (Input.GetAxisRaw("Horizontal") != 0)
            {
                movementInputDirection = Input.GetAxisRaw("Horizontal");
            }
        }

        
        if (!canMove)
        {
            turnTimer -= Time.deltaTime;

            if (turnTimer <= 0)
            {
                canMove = true;
                canFlip = true;
            }
        }
        

    }



    private void CheckJump()
    {

        if (jumpTimer > 0)
        {
            //WallJump
            //Debug.Log("!isGrounded: " + isGrounded); // Test OK
            //Debug.Log("isTouchingWall: " + isTouchingWall); // Test OK
            //Debug.Log("movementInputDirection !=0: " + movementInputDirection); // Test OK
            //Debug.Log("movementInputDirection != FacingDirection: " + facingDirection); // Test Failed

            if (!isGrounded && isTouchingWall && movementInputDirection != 0 && movementInputDirection == facingDirection) // check more ?
            {
                //Debug.Log("WallJump is called"); // Test OK
                WallJump();

            }
            else if (isGrounded)
            {
                NormalJump();
            }
        }

        if (isAtemptingToJump)
        {
            jumpTimer -= Time.deltaTime;
        }

        if (wallJumpTimer > 0)
        {
            if (hasWallJumped && movementInputDirection == -lastWallJumpDirection)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0.0f);
                hasWallJumped = false;
            }
            else if (wallJumpTimer <= 0)
            {
                hasWallJumped = false;
            }
            else
            {
                wallJumpTimer -= Time.deltaTime;
            }
        }

    }

    private void NormalJump()
    {
        if (canNormalJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            amountOfJumpsLeft--;
            jumpTimer = 0;
            isAtemptingToJump = false;
            checkJumpMultiplier = true;
        }
    }

    private void WallJump()
    {
        if (canWallJump)
        {

            //Debug.Log("canWallJump"); // Test OK
            rb.velocity = new Vector2(rb.velocity.x, 0.0f);
            isWallSliding = false;
            amountOfJumpsLeft = amountOfJumps;
            amountOfJumpsLeft--;
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * movementInputDirection, wallJumpForce * wallJumpDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
            //Debug.Log("impulse added: " + forceToAdd);
            
            jumpTimer = 0;
            isAtemptingToJump = false;
            checkJumpMultiplier = true;
            turnTimer = 0;
            canMove = true;
            canFlip = true;
            hasWallJumped = true;
            wallJumpTimer = wallJumpTimerSet;
            lastWallJumpDirection = -facingDirection;
            
        }
    }

    private void ApplayMovement()
    {
        //Debug.Log("ApplayMovement called!!"); OK on Update
        if (!isGrounded && !isWallSliding && movementInputDirection == 0)
        {
            //Debug.Log("ApplayMovement MoveInPutDirection == 0");
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
        }
        else if (canMove)
        {
            //Debug.Log("ApplayMovement canMove InputDircetionn :" + movementInputDirection );
            rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);
        }


        if (isWallSliding)
        {
            if (rb.velocity.y < -wallSlidingSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlidingSpeed);
            }
            //canJump = true;
        }

    }

    private void Flip()
    {

        if (!isWallSliding && canFlip)
        {
            facingDirection *= -1;
            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }

    }

    /*
    private void setWalljumpingToFalse()
    {
        canWallJump = false;
    }
    */
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }
}
