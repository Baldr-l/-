using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CharacterControllerHj : MonoBehaviour
{
    //脚本
    public PlayerInputHj m_Input;

    //
    public Animator m_Animator;
    public CinemachineFreeLook cinemachine;

    //bool
    private bool canAttack = true;
    private bool can_walk;

    //人物参数
    public float acceleration = 30f;
    public float maxWalkSpeed = 10f; 
    public float maxRunSpeed = 29f;
    private float DesiredForwardSpeed;
    public float m_Speed;
    protected Quaternion m_TargetRotation;
    protected float m_AngleDiff;
    float groundedTurnSpeed = 360f;

    //动画状态机
    #region 
    protected AnimatorStateInfo m_CurrentStateInfo;    
    protected AnimatorStateInfo m_NextStateInfo;
    protected bool m_IsAnimatorTransitioning;
    protected AnimatorStateInfo m_PreviousCurrentStateInfo;    
    protected AnimatorStateInfo m_PreviousNextStateInfo;
    protected bool m_PreviousIsAnimatorTransitioning;
    #endregion

    //动画状态机参数
    #region 
    readonly int m_HashAttack = Animator.StringToHash("isAttack");
    readonly int m_HashStateTime = Animator.StringToHash("stateTime");
    readonly int m_HashDamage = Animator.StringToHash("isDamage");
    readonly int m_HashDie = Animator.StringToHash("isDie");
    readonly int m_HashSpeed = Animator.StringToHash("speed");
    readonly int m_HashAttackTwo = Animator.StringToHash("attackTwo");
    readonly int m_HashAttackFire = Animator.StringToHash("attackFire");
    readonly int m_HashAttackJump = Animator.StringToHash("attackJump");
    readonly int m_HashBlockInput = Animator.StringToHash("BlockInput");
    readonly int m_HashAttackBlock = Animator.StringToHash("AttackBlock");
    readonly int m_HashBlockWalk = Animator.StringToHash("BlockWalk");
    
    #endregion
    void Start() {
         Cursor.lockState = CursorLockMode.Locked;
    }
    private void FixedUpdate() {

        //保存当前的动画状态
        CacheAnimatorState();
        //判断当前是否可以输入
        UpdateInputBlocking();
        //判断是否可以行走
        Updatewalk();
        //重置开关
        ResetTrigger_Hj();
        //记录当前动画播放进度
        m_Animator.SetFloat(m_HashStateTime, Mathf.Repeat(m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f));
        //设置开关
        SetTrigger_Hj();

        //移动
        SpeedHj();

        //旋转
        if(m_Speed>0){
            SetRotationHj();
        }
    }

    //保存当前的动画状态
    void CacheAnimatorState()
    {   
        m_PreviousCurrentStateInfo = m_CurrentStateInfo;
        m_PreviousNextStateInfo = m_NextStateInfo;
        m_PreviousIsAnimatorTransitioning = m_IsAnimatorTransitioning;

        m_CurrentStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
        m_NextStateInfo = m_Animator.GetNextAnimatorStateInfo(0);
        m_IsAnimatorTransitioning = m_Animator.IsInTransition(0);
    }
    
    //判断当前是否可以输入
    void UpdateInputBlocking()
    {     
        bool inputBlocked = ((m_CurrentStateInfo.tagHash == m_HashBlockInput) && (!m_IsAnimatorTransitioning));
        inputBlocked = (inputBlocked | (m_NextStateInfo.tagHash == m_HashBlockInput));
        m_Input.playerControllerInputBlocked = inputBlocked;
    }

    //判断是否可以行走
    void Updatewalk(){ 
        can_walk = ((m_CurrentStateInfo.tagHash == m_HashBlockWalk) && (!m_IsAnimatorTransitioning));
        can_walk = (can_walk | (m_NextStateInfo.tagHash == m_HashBlockWalk));
        can_walk = !can_walk;
        //m_Input.playerControllerInputBlocked = inputBlocked;
    }
    void ResetTrigger_Hj(){
        m_Animator.ResetTrigger(m_HashAttackFire);
        m_Animator.ResetTrigger(m_HashAttackJump);
        m_Animator.ResetTrigger(m_HashAttackTwo);
        m_Animator.ResetTrigger(m_HashAttack);
        //m_Animator.ResetTrigger(m_HashJump);
    }

    void SetTrigger_Hj(){
        if (m_Input.AttackInput && canAttack){
            m_Animator.SetTrigger(m_HashAttack);
        }
        if (m_Input.AttackInputFire && canAttack){
            m_Animator.SetTrigger(m_HashAttackFire);
        }
        if (m_Input.AttackInputJump && canAttack){
            m_Animator.SetTrigger(m_HashAttackJump);
        }
        if (m_Input.AttackInputTwo && canAttack){
            m_Animator.SetTrigger(m_HashAttackTwo);
        }
    }
    public void is_Damage(){
        m_Animator.SetTrigger(m_HashDamage);
    } 
    public void is_Die(){
        m_Animator.SetTrigger(m_HashDamage);
    } 
    
    //设置移动速度
    void SpeedHj()
        {   //缓存移动输入并将其大小限制为1
            Vector2 moveInput = m_Input.MoveInput;
            if (moveInput.sqrMagnitude > 1f)
                moveInput.Normalize();

            //计算输入所需的速度
            if(!m_Input.RunInput){
                DesiredForwardSpeed = moveInput.magnitude * maxWalkSpeed;
            }
            else{
                DesiredForwardSpeed = moveInput.magnitude * maxRunSpeed;
            }
            
            //将前进速度调整到所需速度
            if(can_walk){
                m_Speed = Mathf.MoveTowards(m_Speed, DesiredForwardSpeed, acceleration * Time.deltaTime);
            }
            else{
                m_Speed = 0;
            }

            m_Animator.SetFloat(m_HashSpeed, m_Speed);
        }
    //设置旋转速度
    void SetTargetRotation()
        {   //m_CamForward = Vector3.Scale(transform_camera.forward, new Vector3(1, 0, 1)).normalized;
            //m_Move = m_Input.MoveInput.y*transform_camera.forward + m_Input.MoveInput.x*transform_camera.right;
            //Debug.Log(m_Move);
            
            Vector2 moveInput = m_Input.MoveInput;
            Vector3 localMovementDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            #region 
            //Current Freecameralook  其中的属性
            //x的旋转值
            Vector3 forward = Quaternion.Euler(0f, cinemachine.m_XAxis.Value, 0f) * Vector3.forward;
            forward.y = 0f;
            forward.Normalize();

            Quaternion targetRotation;

            //如果局部移动方向与前进方向相反，则目标旋转应朝向相机。
            // If the local movement direction is the opposite of forward then the target rotation should be towards the camera.
            //如果两个值相似，返回true
            //点积，moveInout.y与-1.0相比，
            //即若输入w键，不相似，
            //输入s键，相似，tar = （0,-value,0）
            if (Mathf.Approximately(Vector3.Dot(localMovementDirection, Vector3.forward), -1.0f))
            {
                targetRotation = Quaternion.LookRotation(-forward);
            }
            else
            {
                // Otherwise the rotation should be the offset of the input from the camera's forward.
                //计算从（0,0,1）到（左右，0，前后）所需的角度
                Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localMovementDirection);
                targetRotation = Quaternion.LookRotation(cameraToInputOffset * forward);
            }
            Vector3 resultingForward = targetRotation * Vector3.forward;

            float angleCurrent = Mathf.Atan2(transform.forward.x, transform.forward.z) * Mathf.Rad2Deg;
            float targetAngle = Mathf.Atan2(resultingForward.x, resultingForward.z) * Mathf.Rad2Deg;

            m_AngleDiff = Mathf.DeltaAngle(angleCurrent, targetAngle);
            m_TargetRotation = targetRotation;
            //埃伦想要的前进方向
            // The desired forward direction of Ellen.
            Vector3 localInput = new Vector3(m_Input.MoveInput.x, 0f, m_Input.MoveInput.y);
            //float groundedTurnSpeed = Mathf.Lerp(maxTurnSpeed, minTurnSpeed, m_ForwardSpeed / m_DesiredForwardSpeed);
            //float actualTurnSpeed = m_IsGrounded ? groundedTurnSpeed : Vector3.Angle(transform.forward, localInput) * k_InverseOneEighty * k_AirborneTurnSpeedProportion * groundedTurnSpeed;
            float groundedTurnSpeed = 360f;
            m_TargetRotation = Quaternion.RotateTowards(transform.rotation, m_TargetRotation, 360f * Time.deltaTime);

            transform.rotation = m_TargetRotation;
            #endregion
        }
    void SetRotationHj()
        {   
            Vector2 moveInput = m_Input.MoveInput;
            Vector3 localMovementDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            #region 
            //Current Freecameralook  其中的属性
            //x的旋转值
            Vector3 forward = Quaternion.Euler(0f, cinemachine.m_XAxis.Value, 0f) * Vector3.forward;
            forward.y = 0f;
            forward.Normalize();

            Quaternion targetRotation;

            //如果局部移动方向与前进方向相反，则目标旋转应朝向相机。
            // If the local movement direction is the opposite of forward then the target rotation should be towards the camera.
            //如果两个值相似，返回true
            //点积，moveInout.y与-1.0相比，
            //即若输入w键，不相似，
            //输入s键，相似，tar = （0,-value,0）
            if (Mathf.Approximately(Vector3.Dot(localMovementDirection, Vector3.forward), -1.0f))
            {
                targetRotation = Quaternion.LookRotation(-forward);
            }
            else
            {
                // Otherwise the rotation should be the offset of the input from the camera's forward.
                //计算从（0,0,1）到（左右，0，前后）所需的角度
                Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localMovementDirection);
                targetRotation = Quaternion.LookRotation(cameraToInputOffset * forward);
            }
            Vector3 resultingForward = targetRotation * Vector3.forward;

            float angleCurrent = Mathf.Atan2(transform.forward.x, transform.forward.z) * Mathf.Rad2Deg;
            float targetAngle = Mathf.Atan2(resultingForward.x, resultingForward.z) * Mathf.Rad2Deg;

            m_AngleDiff = Mathf.DeltaAngle(angleCurrent, targetAngle);
            m_TargetRotation = targetRotation;
            //埃伦想要的前进方向
            // The desired forward direction of Ellen.
            Vector3 localInput = new Vector3(m_Input.MoveInput.x, 0f, m_Input.MoveInput.y);
            //float groundedTurnSpeed = Mathf.Lerp(maxTurnSpeed, minTurnSpeed, m_ForwardSpeed / m_DesiredForwardSpeed);
            //float actualTurnSpeed = m_IsGrounded ? groundedTurnSpeed : Vector3.Angle(transform.forward, localInput) * k_InverseOneEighty * k_AirborneTurnSpeedProportion * groundedTurnSpeed;
            
            m_TargetRotation = Quaternion.RotateTowards(transform.rotation, m_TargetRotation, 360f * Time.deltaTime);

            transform.rotation = m_TargetRotation;
            #endregion
        }
}
