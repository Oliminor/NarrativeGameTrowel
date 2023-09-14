using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CassetteButtons : MonoBehaviour
{
    Vector3 startingPos;

    bool isColliding;

    private void Awake()
    {
        startingPos = transform.position;
    }

    private void Update()
    {
        if (transform.position != startingPos && !isColliding)
        {
            transform.position = Vector3.MoveTowards(transform.position, startingPos, Time.deltaTime * 0.5f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        isColliding = true;
    }
    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;
    }
}
