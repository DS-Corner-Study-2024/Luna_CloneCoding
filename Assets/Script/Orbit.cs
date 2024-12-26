using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    // 공전 중심
    public Transform target;
    // 공전 속도
    public float orbitSpeed;
    // 거리
    Vector3 offset;

    void Start()
    {
        offset = transform.position - target.position;
    }

    void Update()
    {
        // 캐릭터를 따라오도록 위치 직접 지정 
        transform.position = target.position + offset;
        // 타켓 주위를 회전하는 함수
        transform.RotateAround(target.position, Vector3.up, orbitSpeed * Time.deltaTime);
        // 위치를 가지고 목표와의 거리 유지
        offset = transform.position - target.position;

    }
}
