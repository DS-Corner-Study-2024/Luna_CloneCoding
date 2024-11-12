using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed; // 인스펙터창에서의 수정을 위해 public 설정
    // 이동
    float hAxis;
    float vAxis;

    // 달리기
    bool wDown;

    // 점프
    bool jDown;
    bool isJump;

    // 닷지
    bool isDodge;

    Vector3 moveVec;
    Vector3 dodgeVec;

    // 물리효과
    Rigidbody rigid;


    Animator anim;

    void Start()
    {
        // 초기화
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        GetInput(); // 제일 위에 있어야함 -> 여기서 정해진 함수들을 아래에서 사용하기 때문
        Move();
        Turn();
        Jump();
        Dodge();

    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
    }

    void Move()
    {
        /*
         * 대각선의 값이 더 크기 떄문에 값 통일을 위해 normalized 이용
         * normalized : 방향 값이 1로 보정된 벡터
         */
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge)
            moveVec = dodgeVec;

        transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }

    void Turn()
    {
        transform.LookAt(transform.position + moveVec); // 나아가는 방향을 바라봄
    }

    void Jump()
    {
        if(jDown && moveVec == Vector3.zero && !isJump && !isDodge)
        {
            rigid.AddForce(Vector3.up * 15, ForceMode.Impulse); // Impulse => 즉각적인 힘을 줄 수 있음
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Dodge()
    {
        if (jDown && moveVec == Vector3.zero && !isJump)
        {
            dodgeVec = moveVec;
            speed *= 2; // 회피 = 이동속도 2배
            anim.SetTrigger("doDodge");
            isDodge = true;

            // 시간차 함수 호출
            Invoke("DodgeOut", 0.5f);
        }
    }

    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }

    // 착지 구현
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);      
            isJump = false;
        }
    }
}
