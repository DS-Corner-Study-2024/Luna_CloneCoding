using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed; // 인스펙터창에서의 수정을 위해 public 설정
    public GameObject[] weapons; // 무기 배열 
    public bool[] hasWeapons; // 무기 가지고 있는지 확인 => 배일 길이 미리 정해주지 않으면 오류 발생함 ( inspector에서 길이 지정해주기 )

    // 이동
    float hAxis;
    float vAxis;

    // 달리기
    bool wDown;

    // 점프
    bool jDown;
    bool isJump;

    // 아이템
    bool iDown;
    bool isSwap; // 교차 시간차를 위해

    // 아이템 선택
    bool sDown1;
    bool sDown2;
    bool sDown3;

    // 회피
    bool isDodge;

    Vector3 moveVec;
    Vector3 dodgeVec; // 회피 중 방향 전환x

    // 물리효과
    Rigidbody rigid;

    Animator anim;

    // 트리거 된 아이템 저장 변수
    GameObject nearObject;
    // 장착 중인 무기 저장 변수
    GameObject equipWeapon;
    // 장착 중인 무기와 교체 무기가 동일한지 확인하는 변수
    int equipWeaponIndex = -1;

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
        Swap();
        Interation();

    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        iDown = Input.GetButtonDown("Interation");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
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
        /*
         * 움직이면서도 무기 교체 가능하게 수정
        if (isSwap)
            moveVec = Vector3.zero;
        */

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
        if (jDown && moveVec != Vector3.zero && !isJump)
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

    // 무기 교체
    void Swap()
    {
        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0))
            return;
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))
            return;
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2))
            return;

        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        if (sDown1 || sDown2 || sDown3)
        {
            if (equipWeapon != null)
                equipWeapon.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex];
            equipWeapon.SetActive(true);

            anim.SetTrigger("doSwap");

            isSwap = true;

            Invoke("SwapOut", 0.4f);

        }
    }
    void SwapOut()
    {
        isSwap = false;
    }

    // 무기 얻음
    void Interation()
    {
        if(iDown && nearObject != null)
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] =  true;

                Destroy(nearObject);
            }
        }
    }

    // 착지 구현
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);      
            isJump = false;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if(other.tag == "Weapon")
            nearObject = other.gameObject;

        Debug.Log(nearObject.name);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
            nearObject = null;
    }
}
