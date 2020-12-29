using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyScript : MonoBehaviour
{
    public Rigidbody2D rigidbody;
    Vector2 startingPos;
    public Vector2 vel = new Vector2(10, 0);

    // Start is called before the first frame update
    void Start()
    {
        startingPos = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rigidbody.MovePosition(rigidbody.position+ vel * Time.fixedDeltaTime);
    }



    public void onHit(int damage)
    {
        Debug.Log("HIT!");
    }


}
