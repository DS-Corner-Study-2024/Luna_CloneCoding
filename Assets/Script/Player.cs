using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed; // �ν�����â������ ������ ���� public ����
    public GameObject[] weapons; // ���� �迭 
    public bool[] hasWeapons; // ���� ������ �ִ��� Ȯ�� => ���� ���� �̸� �������� ������ ���� �߻��� ( inspector���� ���� �������ֱ� )

    // �̵�
    float hAxis;
    float vAxis;

    // �޸���
    bool wDown;

    // ����
    bool jDown;
    bool isJump;

    // ������
    bool iDown;
    bool isSwap; // ���� �ð����� ����

    // ������ ����
    bool sDown1;
    bool sDown2;
    bool sDown3;

    // ȸ��
    bool isDodge;

    Vector3 moveVec;
    Vector3 dodgeVec; // ȸ�� �� ���� ��ȯx

    // ����ȿ��
    Rigidbody rigid;

    Animator anim;

    // Ʈ���� �� ������ ���� ����
    GameObject nearObject;
    // ���� ���� ���� ���� ����
    GameObject equipWeapon;
    // ���� ���� ����� ��ü ���Ⱑ �������� Ȯ���ϴ� ����
    int equipWeaponIndex = -1;

    void Start()
    {
        // �ʱ�ȭ
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        GetInput(); // ���� ���� �־���� -> ���⼭ ������ �Լ����� �Ʒ����� ����ϱ� ����
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
         * �밢���� ���� �� ũ�� ������ �� ������ ���� normalized �̿�
         * normalized : ���� ���� 1�� ������ ����
         */
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge)
            moveVec = dodgeVec;
        /*
         * �����̸鼭�� ���� ��ü �����ϰ� ����
        if (isSwap)
            moveVec = Vector3.zero;
        */

        transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }

    void Turn()
    {
        transform.LookAt(transform.position + moveVec); // ���ư��� ������ �ٶ�
    }

    void Jump()
    {
        if(jDown && moveVec == Vector3.zero && !isJump && !isDodge)
        {
            rigid.AddForce(Vector3.up * 15, ForceMode.Impulse); // Impulse => �ﰢ���� ���� �� �� ����
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
            speed *= 2; // ȸ�� = �̵��ӵ� 2��
            anim.SetTrigger("doDodge");
            isDodge = true;

            // �ð��� �Լ� ȣ��
            Invoke("DodgeOut", 0.5f);
        }
    }

    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }

    // ���� ��ü
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

    // ���� ����
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

    // ���� ����
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
