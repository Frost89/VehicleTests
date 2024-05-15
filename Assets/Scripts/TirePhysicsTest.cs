using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TirePhysicsTest : MonoBehaviour
{
    Rigidbody rb;

    [Header("Suspension Props")]
    [SerializeField] float restLength = 0.5f;
    [SerializeField] float travel = 0.2f;
    [SerializeField] float springStiffness = 500f;
    [SerializeField] float damperStiffness = 15f;
    [SerializeField] float minForce = -2000f;
    [SerializeField] float maxForce = 6000f;

    private float maxLength;
    private float minLength;
    private float springLength;

    private Vector3 springForce;

    [Header("Wheel Props")]
    [SerializeField] float mass = 10f;
    [SerializeField] float radius = 0.34f;
 
    // Start is called before the first frame update
    void Start()
    {
        rb = transform.root.GetComponent<Rigidbody>();
        minLength = restLength - travel;
        maxLength = restLength + travel;
    }

    private void FixedUpdate()
    {
        RaycastHit hit;

        bool didCastHit = Physics.Raycast(transform.position, -transform.up, out hit, (maxLength + radius));

        if (didCastHit)
        {
            Vector3 displacement = transform.position - (hit.point + (transform.up * radius));
            springLength = Mathf.Clamp(displacement.magnitude, minLength, maxLength);
        }
        else
        {
            springLength = maxLength;
        }

        if (didCastHit)
        {
            float suspForce = springStiffness * (restLength - springLength);
            suspForce = Mathf.Clamp(suspForce, minForce, maxForce);

            springForce = suspForce * transform.up;

            rb.AddForceAtPosition(springForce, transform.position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
