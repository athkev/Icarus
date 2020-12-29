using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class capePoint : MonoBehaviour
{
    public Transform offsetobject;
    public Vector3 v;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = offsetobject.position - v;
    }
}
