using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI2_hj : MonoBehaviour
{   
    private bool isop = true;
    private int hp = 100;
    //???????
    private float tran;
    Coroutine destruction;


    //test
    Coroutine cor_test;
    //test
    public GetComponents_hj getComponents_Hj;
    private enum State_AI{巡逻,死亡,受伤,追击,攻击};

    State_AI state_AI = new State_AI();
    State_AI lastState_AI = new State_AI();

    //仇恨值，仇恨目标
    private float hatred;
    private GameObject hatred_obj;

    //bool
    private bool isAttacked;    //是否受击
    private bool isDeaded;      //是否死亡

    //协程
    Coroutine hatred_cor;   //仇恨值协程，start开始，死亡结束
    Coroutine nav_cor;
    Coroutine Patrol_cor;
    Coroutine Attack_cor;
    Coroutine state_01;

    //属性
    float attackrange;
    float pursuitrange;
    float rage;
    Vector3 thisTranpos;
    Vector3 nav_tranpos;
    Animator thisAnimator;
    NavMeshAgent navMesh;
    readonly int m_HashGitHit = Animator.StringToHash("githit");
    readonly int m_HashWalk = Animator.StringToHash("walk");
    readonly int m_HashAttack = Animator.StringToHash("attack");

    void Start(){

        attackrange = 3;
        pursuitrange = 50;
        rage = 0;
        isAttacked = false;
        isDeaded = false;

        state_AI = State_AI.巡逻;
        lastState_AI = State_AI.巡逻;
        
        

        thisTranpos = this.transform.position;
        nav_tranpos = new Vector3(thisTranpos.x + 10,thisTranpos.y,thisTranpos.z + 5);
        thisAnimator = this.GetComponent<Animator>();
        navMesh = this.GetComponent<NavMeshAgent>();

        //Patrol_cor = StartCoroutine("Patrol_Cor");

        nav_cor = StartCoroutine("nav_Cor");
        cor_test = StartCoroutine("hatred_Cor");

        //GetComponent<NavMeshAgent>().destination = this.transform.position;
        //or_test = StartCoroutine("test");
    }

    void Update(){
        if(hp<=0){
            isDeaded = true;
        }

        thisAnimator.SetBool("isWalk",false);
        thisAnimator.SetBool("isAttack",false);
        thisAnimator.SetBool("isPatrol",false);
        thisAnimator.SetBool("isRun",false);
        lastState_AI = state_AI;

        //判断状态
        #region
        //死亡
        if(isDeaded){
            thisAnimator.SetBool("isDie",true);
            if(isop){
                StartCoroutine("");
                isop = false;
            }
        }
        //未死亡
        else{
            //受到攻击
            if(isAttacked){
                state_AI = State_AI.受伤;
            }
            //未受到攻击，且仇恨值<=0
            else if (hatred == 0){
                state_AI = State_AI.巡逻;
            }
            //未受到攻击，且仇恨值不为零
            else{
                //仇恨目标距离
                tran = Vector3.Distance(hatred_obj.transform.position,this.transform.position);
                //距离大于攻击范围，追击
                if(tran > 10){
                    state_AI = State_AI.追击;
                }
                else{
                    state_AI = State_AI.攻击;
                }
            }
        }
        #endregion
        //巡逻,死亡,受伤,追击,攻击
        //判断状态是否改变
        if(lastState_AI != state_AI ){
            if(lastState_AI == State_AI.攻击){
                thisAnimator.SetBool("isAttack",false);
                StopCoroutine(Attack_cor);
            }
            else if(state_AI == State_AI.攻击){
                thisAnimator.SetBool("isAttack",true);
                Attack_cor = StartCoroutine("Attack_Cor");
            }
        }
        switch(lastState_AI){
            case State_AI.巡逻:
            Patrol_Cor();
                break;
            case State_AI.追击:
                state_02();
                break;
            case State_AI.攻击:
                thisAnimator.SetBool("isAttack",true);
                break;
            case State_AI.受伤:
            if(thisAnimator.GetCurrentAnimatorStateInfo(0).tagHash != m_HashGitHit){
                thisAnimator.SetTrigger("isGitHit");
                
            }
                isAttacked = false;
                break;
            case State_AI.死亡:
                break;    
            }

        if(hatred_obj != null){
            //Debug.Log(Vector3.Distance(hatred_obj.transform.position,this.transform.position));
            //Debug.Log(hatred_obj.name);
        }
        else{
            //Debug.Log("111");
        }
        //Debug.Log(thisTranpos);
        //Debug.Log(this.transform.position);
        //Debug.Log(state_AI);
        //Debug.Log(thisAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name);
        //Debug.Log(12345686);
    }


    //判断敌人的距离
    //返回距离最近的单位，若无或超出范围，则为空
    public GameObject judgeDistance_hj(){
        if(getComponents_Hj.friends.Length > 0){
            float tranJudge = 200;
            int i;
            int j = 0;
            for(i = 0;i < getComponents_Hj.friends.Length;i++){
                //Debug.Log(getComponents_Hj.enemys[i].name);
                if(tranJudge > Vector3.Distance(getComponents_Hj.friends[i].transform.position,this.transform.position)){
                    tranJudge = Vector3.Distance(getComponents_Hj.friends[i].transform.position,this.transform.position);
                    j = i;
                }
            }
            if(tranJudge > pursuitrange){
                //Debug.Log(tranJudge);
                hatred = 0;
                return null;
            }
            hatred = 100;
            //Debug.Log(getComponents_Hj.enemys[j].name);
            return getComponents_Hj.friends[j];
        }
        //没有敌人，返回空值
        return null;
    }

    //攻击
    IEnumerator Attack_Cor(){
        thisAnimator.SetBool("isAttack",true);
        yield return new WaitForSeconds(0.1f);
        while(true){  
            //怒气值未满
            if(rage != 100){
                switch(Random.Range(0,4)){
                    case 1:
                        thisAnimator.SetTrigger("isAttack1");
                        break;
                    case 2:
                        thisAnimator.SetTrigger("isAttack2");
                        break;
                    case 3:
                        thisAnimator.SetTrigger("isAttack3");
                        break;
                    case 4:
                        thisAnimator.SetTrigger("isAttack4");
                        break;
                }
                yield return new WaitForSeconds(Random.Range(13,23)*0.1f);
            }
            else{
                thisAnimator.SetTrigger("isAttack1");
                thisAnimator.SetBool("isAnger",true);
                yield return new WaitForSeconds(3.9f);
                thisAnimator.SetBool("isAnger",false);
            }
        }
    }

    //追击
    void state_02(){
        thisAnimator.SetBool("isRun",true);
        if(hatred_obj != null){
            nav_tranpos = hatred_obj.transform.position;
            //GetComponent<NavMeshAgent>().destination = hatred_obj.transform.position;
        }
        else{
            Debug.Log("追击函数bug");
        }
    }

    //巡逻
    void Patrol_Cor(){
        //动画状态切换为isPatrol
        thisAnimator.SetBool("isPatrol",true);
        thisAnimator.SetBool("isWalk",true);

        nav_tranpos = new Vector3(this.transform.position.x + 10,this.transform.position.y,this.transform.position.z-100); 
    }

    IEnumerator nav_Cor(){    
        while(true){
            yield return 0;
            //当前为移动动画,
            if(thisAnimator.GetCurrentAnimatorStateInfo(0).tagHash == m_HashWalk){
                navMesh.isStopped = false;
                navMesh.SetDestination(nav_tranpos);
            }
            //不为移动动画
            else{
                navMesh.isStopped = true;
            }
        }
    }

    //判断仇恨值
    IEnumerator hatred_Cor(){
        float tranHat;
        while(true){
            yield return 0;
            //无仇恨目标时，判断范围内有无敌人.选择最近的作为仇恨目标
            //Debug.Log(hatred_obj);
            //1.游戏开始，为null；2.目标死亡销毁，为null；3.距离过远，目标丢失，为null
            //无仇恨目标,选择最近的敌人作为仇恨目标
            if(hatred_obj == null){
                //返回距离最近的单位，若无或超出范围，则为空
                hatred_obj = judgeDistance_hj();
            }
            //有仇恨目标
            else{
                //判断距离
                tranHat = Vector3.Distance(hatred_obj.transform.position,this.transform.position);
                //距离过远
                if(tranHat >= pursuitrange){
                    hatred -= Time.deltaTime * 50;
                }
                else{
                    
                }
                //距离过远，仇恨丢失
                if(hatred <= 0){
                    hatred = 0;
                    hatred_obj = null;
                }
            }
        }
    }

    IEnumerator test()
    {   
        yield return new WaitForSeconds(5);
        thisAnimator.SetBool("isAttack",true);
        /*
        //GetComponent<NavMeshAgent>().destination = new Vector3(0,1,10);
        thisAnimator.SetBool("isPatrol",true);
        thisAnimator.SetBool("isWalk",true);
        yield return new WaitForSeconds(5);  
        thisAnimator.SetTrigger("isGitHit");
        yield return new WaitForSeconds(0.05f);
        if(thisAnimator.GetCurrentAnimatorStateInfo(0).tagHash == m_HashGitHit){
        //if(thisAnimator.GetCurrentAnimatorClipInfo(0)[0]. != m_HashGitHit){
            Debug.Log("1230456k");
            thisAnimator.SetTrigger("isGitHit"); 
        } 
        nav_tranpos = new Vector3(0,1,10);
        yield return new WaitForSeconds(10);
        nav_tranpos = new Vector3(0,1,-20);
        //thisAnimator.SetBool("isAttack",true);
        /*foreach(GameObject enemy in getComponents_Hj.enemys){
            Debug.Log(enemy.name);
        }*/
        StopCoroutine(cor_test);
    }

    //test
    /*public bool att;
    public bool stop;

    Coroutine c; 
    // Start is called before the first frame update
    void Start()
    {
        c = StartCoroutine("GetCounter");
        stop = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(!stop){
            if(Random.Range(0,2000) == 0){
                StopCoroutine(c);
                Debug.Log("xiechengstop");
                Coroutine b = StartCoroutine("GetCounter2");
                StopCoroutine(b);
                Debug.Log("wumiaohou");
                stop = true;
            }
        }
    }

    IEnumerator GetCounter()
    {   
        while(true){
            //3-5
            //yield return new WaitForSeconds(Random.Range(3,6));
            yield return new WaitForSeconds(1);
            Debug.Log(Random.Range(0,10));  
            StopCoroutine(c);
        }
    }
    //等待x秒，
    IEnumerator waitRandom_hj(int i,int j)
    {   
        yield return new WaitForSeconds(5);
        Debug.Log("5555555");
        StopCoroutine(c);
    }*/

    void Hit(){
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("碰撞");
        if(thisAnimator.GetCurrentAnimatorStateInfo(0).tagHash == m_HashAttack){
            collision.gameObject.GetComponent<AI2_hj>().damage();
        }
        if(collision.collider.tag=="pick up")
        {
            Destroy(collision.collider.gameObject);
        }
    }
    public void damage(){
        hp -= 50;
        isAttacked = true;
    }
    
}
