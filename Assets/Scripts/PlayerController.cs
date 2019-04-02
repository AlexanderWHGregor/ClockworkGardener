using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float moveSpeed;

    private Animator anim;

    private bool playerMoving;
    private Vector2 lastMove;

    public bool canMove;

    //private RigidBody2D myRigidBody;

    // Start is called before the first frame update
    void Start()
    {

        anim = GetComponent<Animator>();

        //myRigidBody = GetComponent<RigidBody2D>();

        canMove = true;

    }

    // Update is called once per frame
    void Update()
    {



        playerMoving = false;

        if (!canMove)
        {

            //myRigidBody.velocity = Vector2.zero;

            transform.Translate(new Vector3(0f, 0f, 0f));
            transform.Translate(new Vector3(0f, 0f, 0f));

            anim.SetFloat("MoveX", 0);
            anim.SetFloat("MoveY", 0);

            anim.SetBool("PlayerMoving", playerMoving);
            anim.SetFloat("LastMoveX", 0);
            anim.SetFloat("LastMoveY", 0);

            return;

        }

        if (Input.GetAxisRaw("Horizontal") > 0.5f || Input.GetAxisRaw("Horizontal") < -0.5f)
        {

            transform.Translate(new Vector3(Input.GetAxisRaw("Horizontal") * moveSpeed * Time.deltaTime, 0f, 0f));
            playerMoving = true;
            lastMove = new Vector2(Input.GetAxisRaw("Horizontal"), 0f);

        }

        if (Input.GetAxisRaw("Vertical") > 0.5f || Input.GetAxisRaw("Vertical") < -0.5f)
        {

            transform.Translate(new Vector3(0f, Input.GetAxisRaw("Vertical") * moveSpeed * Time.deltaTime, 0f));
            playerMoving = true;
            lastMove = new Vector2(0f, Input.GetAxisRaw("Vertical"));

        }

        anim.SetFloat("MoveX", Input.GetAxisRaw("Horizontal"));
        anim.SetFloat("MoveY", Input.GetAxisRaw("Vertical"));

        anim.SetBool("PlayerMoving", playerMoving);
        anim.SetFloat("LastMoveX", lastMove.x);
        anim.SetFloat("LastMoveY", lastMove.y);

    }

    // Sets enemy health and timer depending on which one player runs into
    void OnTriggerEnter2D(Collider2D other)
    {
        // First enemy
        if (other.gameObject.name == "Enemy 1")
        {
            LoadPuzzle.setHealth(100);
            LoadPuzzle.setTimer(30);
        }
        // Second enemy
        if (other.gameObject.name == "Enemy 2")
        {
            LoadPuzzle.setHealth(400);
            LoadPuzzle.setTimer(40);
        }
        // Third enemy
        if (other.gameObject.name == "Enemy 3")
        {
            LoadPuzzle.setHealth(800);
            LoadPuzzle.setTimer(60);
        }
    }

}