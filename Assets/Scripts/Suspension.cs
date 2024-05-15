using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Suspension : MonoBehaviour
{
    private Rigidbody carRigidBody;
    private CarController carController;


    [SerializeField] public bool frontLeft;
    [SerializeField] public bool frontRight;
    [SerializeField] public bool rearLeft;
    [SerializeField] public bool rearRight;

    [Header("Suspension Properties")]
    [SerializeField] float springRestLength;
    [SerializeField] float springMaxTravel;
    [SerializeField] float springStrength;
    [SerializeField] float damperStrength;

    private float minSpringLength;
    private float maxSpringLength;
    private float springLength;
    private float springOffset;
    private float springVelocity;
    private float springForce;
    private float damperForce;
    private float totalSuspensionForce;


    private Vector3 suspensionForce;
    private Vector3 steerForce;
    private Transform wheelMesh;

    [Header("Wheel Properties")]
    [SerializeField] float wheelRadius;
    [SerializeField] public float steerAngle;
    [SerializeField] float steerTime = 7.0f;
    [SerializeField] private float wheelAngle;
    [HideInInspector] public float wheelAccelScale;
    [SerializeField] float gripFactor = 0.8f;
    [SerializeField] float tireMass = 10f;

    //AccelerationForces
    private Vector3 tractiveForce;
    private Vector3 dragForce;
    private Vector3 rollingResistForce;
    private Vector3 maxTractionForce;


    private float engineForce;
    private float dragConstant;
    private float rrConstant;
    private float brakingConstant;
    private float carWeight;

    private float frontWeight;
    private float rearWeight;

    // Start is called before the first frame update
    void Start()
    {
        carController = transform.root.GetComponent<CarController>();

        wheelMesh       = transform.GetChild(0);
        carRigidBody    = transform.root.GetComponent<Rigidbody>();
        minSpringLength = springRestLength - springMaxTravel;
        maxSpringLength = springRestLength + springMaxTravel;

        engineForce     = 3000;
        dragConstant    = 0.4257f;
        rrConstant      = 30 * dragConstant;
        brakingConstant = 1500;
        carWeight = carRigidBody.mass * 9.8f;

    }

    private void Update()
    {
        wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, steerTime);
        transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);
        //wheelMesh.localRotation = transform.localRotation;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        bool didCastHit = Physics.Raycast(transform.position, 
                                          transform.TransformDirection(Vector3.down), 
                                          out RaycastHit hitInfo, 
                                         (maxSpringLength + wheelRadius));
        Debug.DrawRay(transform.position, -transform.up * hitInfo.distance, Color.green);
        if (didCastHit) 
        {
            SuspensionUpdate(hitInfo);
            SteerUpdate(hitInfo);
            AccelerationUpdate();
            ApplyTireFriction(hitInfo);
            ApplyTireRotation(hitInfo);
            //Vector3 acceleration = carRigidBody.GetAccumulatedForce(Time.fixedDeltaTime)/ carRigidBody.mass;
            //Vector3 carVelocity = carRigidBody.velocity + Time.fixedDeltaTime * acceleration;
            //Vector3 carPostion = carRigidBody.transform.position + Time.fixedDeltaTime * carVelocity;
            //float h = 2.1f;
            ////print(h);
            //frontWeight = 0.5f * carWeight - (h / 0.6f) * carRigidBody.mass * acceleration.magnitude;
            //rearWeight = 0.5f * carWeight + (h / 0.6f) * carRigidBody.mass * acceleration.magnitude;

            //Vector3 frontWeightForce = frontWeight * transform.forward;
            //Vector3 rearWeightForce = rearWeight * transform.forward;

            //if (frontLeft || frontRight) 
            //{
            //    maxTractionForce = tireFrictionCoeff * frontWeightForce;
            //}
            //else
            //{
            //    maxTractionForce = tireFrictionCoeff * rearWeightForce;
            //}
            ////carRigidBody.AddForceAtPosition(maxTractionForce, transform.position);
            //print("Max Traction force: " + maxTractionForce);
            //print("Tractive Force: " + tractiveForce);
        }
    }

    private void AccelerationUpdate()
    {
        Vector3 accelDir = transform.forward;
        if(wheelAccelScale < 0.0f)
        {
            tractiveForce = -accelDir * brakingConstant; 
        }
        else
        {
            tractiveForce = accelDir * engineForce * wheelAccelScale;
        }

        dragForce = -dragConstant * carRigidBody.velocity.magnitude * carRigidBody.velocity ;
        

        rollingResistForce = -rrConstant * carRigidBody.velocity;

        Vector3 longForce = tractiveForce + dragForce + rollingResistForce;
        //float debugTorque = wheelAccelScale * engineForce;
        if (carRigidBody.velocity.magnitude > 0)
        {
            carRigidBody.AddForceAtPosition(longForce, transform.position);
        }
    }

    private void SteerUpdate(RaycastHit hit)
    {
        Vector3 steerDir = transform.right;
        Vector3 tireVel = carRigidBody.GetPointVelocity(transform.position);
        float wheelSpeed = Vector3.Dot(steerDir, tireVel);
        Vector3 wheelSpeedLS = transform.InverseTransformDirection(carRigidBody.GetPointVelocity(hit.point));
        float desiredChangeInVel = -wheelSpeed * gripFactor;
        float tireAccel = desiredChangeInVel * Time.fixedDeltaTime;
        steerForce = tireAccel * tireMass * steerDir;
        //wheelSpeedLS.x * -steerDir * tireMass;
        carRigidBody.AddForceAtPosition(steerForce, transform.position);
    }

    private void SuspensionUpdate(RaycastHit hit)
    {
        Vector3 springDir = transform.up;
        Vector3 tireVel = carRigidBody.GetPointVelocity(transform.position);
        springVelocity = Vector3.Dot(springDir, tireVel);

        springLength = hit.distance - wheelRadius;
        springLength = Mathf.Clamp(springLength, minSpringLength, maxSpringLength);
        springOffset = springRestLength - hit.distance;//springLength;

        springForce = springStrength * springOffset;
        damperForce = damperStrength * springVelocity;

        totalSuspensionForce = springForce - damperForce; //use this to setup tire friction
        suspensionForce = totalSuspensionForce * springDir;
        carRigidBody.AddForceAtPosition(suspensionForce, transform.position);
    }

    void ApplyTireFriction(RaycastHit hit)
    {
        Vector3 tireVel = transform.InverseTransformDirection(carRigidBody.GetPointVelocity(wheelMesh.position - (transform.up*wheelRadius)));
        Vector3 steerDir = transform.right;

        float friction = -totalSuspensionForce * tireVel.x;
        friction = Mathf.Clamp(friction, -totalSuspensionForce, totalSuspensionForce);

        Vector3 frictionForce = friction * steerDir;
        carRigidBody.AddForceAtPosition(frictionForce, transform.position);
    }

    void ApplyTireRotation(RaycastHit hit)
    {
        Vector3 tireVel = transform.InverseTransformDirection(carRigidBody.GetPointVelocity(wheelMesh.position - (transform.up * wheelRadius)));
        float angularVelocity = tireVel.z / wheelRadius;
        float rad2degAngularVelocity = angularVelocity * Time.fixedDeltaTime * Mathf.Rad2Deg;
        wheelMesh.localRotation = Quaternion.Euler(rad2degAngularVelocity, wheelMesh.localRotation.y, wheelMesh.localRotation.z);
    }

}
