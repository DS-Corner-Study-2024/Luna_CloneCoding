using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class Player : MonoBehaviour
{
    public float speed; // 인스펙터창에서의 수정을 위해 public 설정
    public GameObject[] weapons; // 무기 배열 
    public bool[] hasWeapons; // 무기 가지고 있는지 확인 => 배일 길이 미리 정해주지 않으면 오류 발생함 ( inspector에서 길이 지정해주기 )
    public GameObject[] grenades;
    public int hasGrenades; // 수류탄
    public GameObject grenadeObj; // 수류탄 프리펩 저장 변수
    public Camera followCamera; // 메인 카메라 변수

    // 이동
    float hAxis;
    float vAxis;

    //총알, 코인, 체력
    public int ammo;
    public int coin;
    public int health;

    // 최댓값
    public int maxAmmo;
    public int maxCoin;
    public int maxHealth;
    public int maxHasGrenades;

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

    // 공격
    bool fDown;
    bool isFireReady;

    // 재장전
    bool rDown;
    bool isReload;

    // 수류탄
    bool gDown;

    Vector3 moveVec;
    Vector3 dodgeVec; // 회피 중 방향 전환x

    // 물리효과
    Rigidbody rigid;

    Animator anim;

    MeshRenderer[] meshs;

    // 트리거 된 아이템 저장 변수
    GameObject nearObject;
    // 장착 중인 무기 저장 변수
    Weapon equipWeapon;
    // 장착 중인 무기와 교체 무기가 동일한지 확인하는 변수
    int equipWeaponIndex = -1;
    // 공격 딜레이
    float fireDelay;

    // 벽 충돌
    bool isBorder;

    // 무적 타임
    bool isDamage;


    void Start()
    {
        // 초기화
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        meshs = GetComponentsInChildren<MeshRenderer>();
    }

    void Update()
    {
        GetInput(); // 제일 위에 있어야함 -> 여기서 정해진 함수들을 아래에서 사용하기 때문
        Move();
        Turn();
        Jump();
        Grenade();
        Attack();
        Reload();
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
        fDown = Input.GetButton("Fire1");
        gDown = Input.GetButtonDown("Fire2");
        rDown = Input.GetButtonDown("Reload");
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
        /**
        if (isSwap)
            moveVec = Vector3.zero;
        */

        if(!isBorder) // 이동 제한
            transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }

    void Turn()
    {
        // 키보드 회전
        transform.LookAt(transform.position + moveVec); // 나아가는 방향을 바라봄

        // 마우스 회전
        if (fDown)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100)) // out : return처럼 반환값을 변수에 저장해줌
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0; // 높이 무시하도록 y=0으로 설정 
                transform.LookAt(transform.transform.position + nextVec);
            }
        }
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

    void Grenade()
    {
        if (hasGrenades == 0)
            return;
        if (gDown && !isReload && !isSwap)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100)) // out : return처럼 반환값을 변수에 저장해줌
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 10; 

                // 수류탄 던짐
                GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation);
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();

                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);

                hasGrenades--;
                grenades[hasGrenades].SetActive(false);
            }
        }
    }

    void Attack()
    {
        if (equipWeapon == null)
            return;

        fireDelay += Time.deltaTime; // 공격 딜레이 시간 더해 공격 가능 여부 확인
        isFireReady = equipWeapon.rate < fireDelay;

        if(fDown && isFireReady && !isDodge && !isSwap)
        {
            equipWeapon.Use(); // 조건 충조하면 무기에 있는 함수 실행 => 실제 공격 로직은 Weapon에서 사용ㅇ
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot"); // 무기에 따라 근접, 원거리 공격 결정
            fireDelay = 0; // 공격 딜레이 0으로 돌려 다음 공격까지 기다림
        }
    }

    void Reload()
    {
        if (equipWeapon == null)
            return;

        if (equipWeapon.type == Weapon.Type.Melee)
            return;

        if (ammo == 0)
            return;

        if(rDown && !isJump && !isDodge && !isSwap && isFireReady)
        {
            anim.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 3f);
        }
    }

    void ReloadOut()
    {
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        equipWeapon.curAmmo = reAmmo;
        ammo -= reAmmo;
        isReload = false;
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
                equipWeapon.gameObject.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

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

    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero; // 물리 회전 속도
    }

    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        isBorder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall"));
    }

    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
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

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if(ammo > maxAmmo)
                        ammo = maxAmmo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin)
                        coin = maxCoin;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth)
                        health = maxHealth;
                    break;
                case Item.Type.Grenade:
                    if (hasGrenades == maxHasGrenades)
                        return;
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    if (hasGrenades > maxHasGrenades)
                        hasGrenades = maxHasGrenades;
                    break;
            }
            Destroy(other.gameObject);
        }
        else if (other.tag == "EnemyBullet")
        {
            if (!isDamage)
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;

                if (other.GetComponent<Rigidbody>() != null)
                    Destroy(other.gameObject);

                StartCoroutine(OnDamage());
            }  
        }
    }

    IEnumerator OnDamage()
    {
        isDamage = true;
        foreach(MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.yellow;
        }
        yield return new WaitForSeconds(1f);

        isDamage = false;
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if(other.tag == "Weapon")
            nearObject = other.gameObject;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
            nearObject = null;
    }
}
