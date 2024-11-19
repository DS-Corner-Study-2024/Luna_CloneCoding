using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    // 변수 x, 하나의 타입일 뿐임
    public enum Type { Ammo, Coin, Grenade, Heart, Weapon }
    // 변수 선언
    public Type type;
    public int value;

    void Update()
    {
        // 계속 회전되도록 함
        transform.Rotate(Vector3.up * 20 * Time.deltaTime);
    }

}
