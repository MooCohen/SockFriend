using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeExpression : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        InvokeRepeating("Expression", 2.0f, 1.0f);
    }

    void Expression(){
        int randomExpression = Random.Range(0, 7);
        animator.SetInteger("Expression", randomExpression);
    }
}
