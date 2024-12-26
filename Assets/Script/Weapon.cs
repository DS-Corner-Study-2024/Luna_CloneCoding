using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // 무기 타입
    public enum Type { Melee, Range };
    // 실제 타입 저장
    public Type type;


    // 공격력
    public int damage;
    // 속도
    public float rate;
    // 범위
    public BoxCollider meleeArea;
    // 효과 
    public TrailRenderer trailEffect;

    // 총알
    public GameObject bullet;
    // 총알 위치 
    public Transform bulletPos;

    // 전체 탄약
    public int maxAmmo;
    // 현재 탄약
    public int curAmmo;

    // 탄피
    public GameObject bulletCase;
    // 탄피 위치 
    public Transform bulletCasePos;

    // 무기 사용
    public void Use()
    {
        if(type == Type.Melee)
        {
            StopCoroutine("Swing"); // 코루틴 중지 함수
            StartCoroutine("Swing"); // 코루틴 실행 함수
        } else if (type == Type.Range && curAmmo > 0)
        {
            curAmmo--;
            StartCoroutine("Shot"); // 코루틴 실행 함수
        }
    }

    // 코루틴 : 메인루틴과 서브루틴 같이 실행 => 서브 루틴 = 코루틴
    // 결과 전달 키워드 yield 1개 이상 필요
    // yield return null; // 1프레임 대기 => 여러개 사용해 시간차 로직 가능
    // yield return new WaitForSeconds(0.1f); // 대기 설정 가능
    // yield break; // 해당 명령어 아래에 코루틴 있으면 비활성화

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
        // 1. 총알 발사
        GameObject intantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = intantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 50; // 총알이 만드어지면서 속도가 붙음

        yield return null; // 한 프레임 쉼

        // 2. 탄피 배출
        GameObject intantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody caseRigid = intantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletPos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(3, 2); // 가해지는 힙의 방향 
        caseRigid.AddForce(caseVec, ForceMode.Impulse);
        caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse); // 회전 추가
    }
}
