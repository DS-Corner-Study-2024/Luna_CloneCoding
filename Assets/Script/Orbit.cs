using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    // ���� �߽�
    public Transform target;
    // ���� �ӵ�
    public float orbitSpeed;
    // �Ÿ�
    Vector3 offset;

    void Start()
    {
        offset = transform.position - target.position;
    }

    void Update()
    {
        // ĳ���͸� ��������� ��ġ ���� ���� 
        transform.position = target.position + offset;
        // Ÿ�� ������ ȸ���ϴ� �Լ�
        transform.RotateAround(target.position, Vector3.up, orbitSpeed * Time.deltaTime);
        // ��ġ�� ������ ��ǥ���� �Ÿ� ����
        offset = transform.position - target.position;

    }
}
