using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed; // �ν�����â������ ������ ���� public ����
    // �̵�
    float hAxis;
    float vAxis;

    // �޸���
    bool wDown;

    // ����
    bool jDown;
    bool isJump;

    // ����
    bool isDodge;

    Vector3 moveVec;
    Vector3 dodgeVec;

    // ����ȿ��
    Rigidbody rigid;


    Animator anim;

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
         * �밢���� ���� �� ũ�� ������ �� ������ ���� normalized �̿�
         * normalized : ���� ���� 1�� ������ ����
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
        if (jDown && moveVec == Vector3.zero && !isJump)
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

    // ���� ����
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);      
            isJump = false;
        }
    }
}
