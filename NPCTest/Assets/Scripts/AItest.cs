using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AItest : MonoBehaviour
{
   
readonly int m_HashAttack = Animator.StringToHash("attack");
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("碰撞");
        //if(collision.gameObject.tag )
        //if(thisAnimator.GetCurrentAnimatorStateInfo(0).tagHash == m_HashAttack){
            collision.gameObject.GetComponent<AI2_hj>().damage();
        //}
        /*if(collision.collider.tag=="pick up")
        {
            Destroy(collision.collider.gameObject);
        }*/
    }

}
