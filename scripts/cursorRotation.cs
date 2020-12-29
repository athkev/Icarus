using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cursorRotation : MonoBehaviour
{
    public Transform CursorPivot;
    public Camera m_camera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
        RotateGun(mousePos, true);
    }

    void RotateGun(Vector3 lookPoint, bool allowRotationOverTime)
    {
        Vector3 distanceVector = lookPoint - CursorPivot.position;

        float angle = Mathf.Atan2(distanceVector.y, distanceVector.x) * Mathf.Rad2Deg;

        CursorPivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

    }
}
