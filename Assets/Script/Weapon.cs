using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // ���� Ÿ��
    public enum Type { Melee, Range };
    // ���� Ÿ�� ����
    public Type type;


    // ���ݷ�
    public int damage;
    // �ӵ�
    public float rate;
    // ����
    public BoxCollider meleeArea;
    // ȿ�� 
    public TrailRenderer trailEffect;

    // �Ѿ�
    public GameObject bullet;
    // �Ѿ� ��ġ 
    public Transform bulletPos;

    // ��ü ź��
    public int maxAmmo;
    // ���� ź��
    public int curAmmo;

    // ź��
    public GameObject bulletCase;
    // ź�� ��ġ 
    public Transform bulletCasePos;

    // ���� ���
    public void Use()
    {
        if(type == Type.Melee)
        {
            StopCoroutine("Swing"); // �ڷ�ƾ ���� �Լ�
            StartCoroutine("Swing"); // �ڷ�ƾ ���� �Լ�
        } else if (type == Type.Range && curAmmo > 0)
        {
            curAmmo--;
            StartCoroutine("Shot"); // �ڷ�ƾ ���� �Լ�
        }
    }

    // �ڷ�ƾ : ���η�ƾ�� �����ƾ ���� ���� => ���� ��ƾ = �ڷ�ƾ
    // ��� ���� Ű���� yield 1�� �̻� �ʿ�
    // yield return null; // 1������ ��� => ������ ����� �ð��� ���� ����
    // yield return new WaitForSeconds(0.1f); // ��� ���� ����
    // yield break; // �ش� ��ɾ� �Ʒ��� �ڷ�ƾ ������ ��Ȱ��ȭ

    IEnumerator Swing()
    {
        yield return new WaitForSeconds(0.4f);
        meleeArea.enabled = true;
        trailEffect.enabled = true;

        yield return new WaitForSeconds(0.3f);
        meleeArea.enabled = false;

        yield return new WaitForSeconds(0.3f);
        trailEffect.enabled = false;
    }

    IEnumerator Shot()
    {
        // 1. �Ѿ� �߻�
        GameObject intantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = intantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 50; // �Ѿ��� ��������鼭 �ӵ��� ����

        yield return null; // �� ������ ��

        // 2. ź�� ����
        GameObject intantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody caseRigid = intantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletPos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(3, 2); // �������� ���� ���� 
        caseRigid.AddForce(caseVec, ForceMode.Impulse);
        caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse); // ȸ�� �߰�
    }
}
