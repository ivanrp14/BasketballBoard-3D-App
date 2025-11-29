using UnityEngine;

public class PlayActor : MonoBehaviour
{
    public int id;           // 0..N para jugadores, 99 para pelota
    public Animator animator;
    public Transform lookTarget;
    private Draggable draggable;

    void Awake()
    {
        animator = GetComponent<Animator>();
        draggable = GetComponent<Draggable>();
        draggable.OnDraggingStarted += EnableFloatAnim;
        draggable.OnDraggingEnded += DisableFloatAnim;
    }
    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    public void LookAt(Vector3 target)
    {
        Vector3 look = target;
        look.y = transform.position.y;
        transform.LookAt(look);
    }

    public void SetMoving(bool isMoving)
    {
        if (animator)
            animator.SetBool("Run", isMoving);
    }

    public void SetBlock(bool block)
    {
        //if (animator)
        //animator.SetBool("Block", block);
    }

    public void ResetAnimations()
    {
        if (animator)
        {
            animator.SetBool("Run", false);
            //animator.SetBool("Block", false);
            animator.SetBool("Float", false);
        }
    }
    void EnableFloatAnim()
    {
        if (animator)
        {
            animator.SetBool("Float", true);
        }
    }
    void DisableFloatAnim()
    {
        if (animator)
        {
            animator.SetBool("Float", false);
        }
    }

}
