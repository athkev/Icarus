using System.Diagnostics;
using System.Threading;
using UnityEngine;

public class Tutorial_GrapplingGun : MonoBehaviour
{
    [Header("Scripts Ref:")]
    public Tutorial_GrapplingRope grappleRope;
    public PlayerMovement playerMovement;

    [Header("Layers Settings:")]
    [SerializeField] private bool grappleToAll = false;
    [SerializeField] private int grappableLayerNumber = 9;
    [SerializeField] public LayerMask ignoreLayer = 8;

    [Header("Main Camera:")]
    public Camera m_camera;

    [Header("Transform Ref:")]
    public Transform gunHolder;
    public Transform gunPivot;
    public Transform firePoint;

    [Header("Physics Ref:")]
    public DistanceJoint2D m_distanceJoint2D;
    public Rigidbody2D m_rigidbody;

    [Header("Rotation:")]
    [SerializeField] private bool rotateOverTime = true;
    [Range(0, 60)] [SerializeField] private float rotationSpeed = 4;

    [Header("Distance:")]
    [SerializeField] private bool hasMaxDistance = false;
    [SerializeField] private float maxDistance = 20;

    private enum LaunchType
    {
        Transform_Launch,
        Physics_Launch
    }

    [Header("Launching:")]
    [SerializeField] private bool launchToPoint = true;
    [SerializeField] private LaunchType launchType = LaunchType.Physics_Launch;
    [SerializeField] private float launchSpeed = 1;

    [Header("No Launch To Point")]
    [SerializeField] private bool autoConfigureDistance = false;
    [SerializeField] private float targetDistance = 3;

    public Vector2 grapplePoint;
    public Vector2 grappleDistanceVector;


    //if rope hits nowhere to the layer
    public bool hit = false;
    public bool finishedDraw = false;
    public bool tempbool = true;


    private void Start()
    {
        grappleRope.enabled = false;
        m_distanceJoint2D.enabled = false;

    }

    private void Update()
    {
        if (finishedDraw && !hit)
        {
            grappleRope.enabled = false;
            m_distanceJoint2D.enabled = false;
            m_rigidbody.gravityScale = 4;

            finishedDraw = false;
        }
        else if (Input.GetKeyDown(KeyCode.Mouse0) && playerMovement.leftClickState == 3)
        {
            SetGrapplePoint();
            tempbool = true;
        }
        //let player launch with a force towards hitpoint, disable line 
        else if (finishedDraw && Input.GetKey(KeyCode.Mouse0) && Input.GetButton("Crouch") && tempbool)
        {
            m_distanceJoint2D.enabled = false;
            finishedDraw = false;
            tempbool = false;


            Vector3 distanceVector = (Vector3)grapplePoint - gunPivot.position;
            m_rigidbody.AddForce(distanceVector.normalized * 1250);

            //after player is launched
            //line is attached to player for t sec
            //have line fixed length
            //
            

            

        }
        else if (Input.GetKey(KeyCode.Mouse0))
        {
            if (grappleRope.enabled)
            {
                RotateGun(grapplePoint, false);
            }
            else
            {
                Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
                RotateGun(mousePos, true);
            }   

            if (launchToPoint && grappleRope.isGrappling)
            {
                /*
                if (launchType == LaunchType.Transform_Launch)
                {
                    Vector2 firePointDistnace = firePoint.position - gunHolder.localPosition;
                    Vector2 targetPos = grapplePoint - firePointDistnace;
                    gunHolder.position = Vector2.Lerp(gunHolder.position, targetPos, Time.deltaTime * launchSpeed);
                }
                */
            }
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            finishedDraw = false;
            grappleRope.enabled = false;
            m_distanceJoint2D.enabled = false;
            m_rigidbody.gravityScale = 4;
        }
        else
        {
            Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
            RotateGun(mousePos, true);
        }

    }

    void RotateGun(Vector3 lookPoint, bool allowRotationOverTime)
    {
        Vector3 distanceVector = lookPoint - gunPivot.position;

        float angle = Mathf.Atan2(distanceVector.y, distanceVector.x) * Mathf.Rad2Deg;
        if (rotateOverTime && allowRotationOverTime)
        {
            gunPivot.rotation = Quaternion.Lerp(gunPivot.rotation, Quaternion.AngleAxis(angle, Vector3.forward), Time.deltaTime * rotationSpeed);
        }
        else
        {
            gunPivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void SetGrapplePoint()
    {
        hit = false;
        finishedDraw = false;
        Vector2 distanceVector = m_camera.ScreenToWorldPoint(Input.mousePosition) - gunPivot.position;

        if (Physics2D.Raycast(firePoint.position, distanceVector.normalized, maxDistance, ~ignoreLayer))
        {
            RaycastHit2D _hit = Physics2D.Raycast(firePoint.position, distanceVector.normalized, maxDistance, ~ignoreLayer);
            
            if (Vector2.Distance(_hit.point, firePoint.position) <= maxDistance)
            {
                grapplePoint = _hit.point;
                hit = true;
            }
            else
            {
                grapplePoint = (Vector2)gunPivot.position + distanceVector.normalized * maxDistance;
                hit = false;
            }
        }
        else
        {
            grapplePoint = (Vector2)gunPivot.position + distanceVector.normalized * maxDistance;
            hit = false;
        }
        grappleDistanceVector = grapplePoint - (Vector2)gunPivot.position;
        grappleRope.enabled = true;
            
            /*
            if (_hit.transform.gameObject.layer == grappableLayerNumber || grappleToAll)
            {
                if (Vector2.Distance(_hit.point, firePoint.position) <= maxDistance || !hasMaxDistance)
                {
                    grapplePoint = _hit.point;
                    grappleDistanceVector = grapplePoint - (Vector2)gunPivot.position;
                    grappleRope.enabled = true;
                }
            }
            */
            
        
    }

    public void Grapple()
    {
        m_distanceJoint2D.autoConfigureDistance = false;
        if (!launchToPoint && !autoConfigureDistance)
        {
            m_distanceJoint2D.distance = targetDistance;
        }
        if (!launchToPoint) //1
        {
            if (autoConfigureDistance)
            {
                m_distanceJoint2D.autoConfigureDistance = true;
                //m_distanceJoint2D.maxDistance = targetDistance;
            }

            m_distanceJoint2D.connectedAnchor = grapplePoint;
            if(hit) m_distanceJoint2D.enabled = true;
        }
        else
        {
            
            switch (launchType)
            {
                case LaunchType.Physics_Launch:
                    m_distanceJoint2D.connectedAnchor = grapplePoint;

                    Vector2 distanceVector = firePoint.position - gunHolder.position;

                    m_distanceJoint2D.distance = distanceVector.magnitude;
                    m_distanceJoint2D.enabled = true;
                    break;
                case LaunchType.Transform_Launch:
                    m_rigidbody.gravityScale = 0;
                    m_rigidbody.velocity = Vector2.zero;
                    break;
            }
            
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (firePoint != null && hasMaxDistance)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(firePoint.position, maxDistance);
        }
    }

}