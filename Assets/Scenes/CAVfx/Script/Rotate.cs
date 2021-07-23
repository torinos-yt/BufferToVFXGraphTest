using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] float _speed = .1f;

    void Update()
        => this.transform.rotation *= Quaternion.Euler(0, _speed, 0);
}
