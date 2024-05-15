using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    Transform[] wheels = new Transform[4];
    Rigidbody carRB;
    private float engineForce = 8000f;


    [SerializeField] float suspensionRestDist = 1.5f;
    [SerializeField] float springStrength = 150f;
    [SerializeField] float dampingScale = 15f;
    [SerializeField] float carTopSpeed = 100f;
    [SerializeField] float gripFactor = 0.8f;
    [SerializeField] float tireMass = 1.0f;
    [SerializeField]
    [Range(0.0f, 1.0f)] float powerDistribution = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        carRB = GetComponent<Rigidbody>();
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i] = transform.GetChild(i).transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float steerInput = Input.GetAxis("Horizontal");
        for (int i = 0;i < 2;i++) 
        {
            //wheels[i].Rotate(new Vector3(0, steerInput * 45, 0));
            wheels[i].localRotation = Quaternion.Euler(0, steerInput * 45, 0);
            //wheels[i].rotation = Quaternion.Euler(0, steerInput * 45, 0);
            //print(wheels[i].rotation);
        }
      
    }

    private void FixedUpdate()
    {
        foreach(Transform tireTransform in wheels)
        {
            RaycastHit hit;
            bool didCastHit = Physics.Raycast(tireTransform.position, tireTransform.TransformDirection(Vector3.down), out hit, suspensionRestDist);
            Debug.DrawRay(tireTransform.position, tireTransform.TransformDirection(Vector3.down)* hit.distance, Color.blue); 

            if (didCastHit)
            {
                SuspensionUpdate(tireTransform, hit);

                //Acceleration/Braking
                AccelBrakingEvent(tireTransform, hit);

                //SteeringUpdate(tireTransform, hit);
                Vector3 steerDir = tireTransform.right;

                Vector3 tireWorldVel = carRB.GetPointVelocity(tireTransform.position);

                float steeringVel = Vector3.Dot(steerDir, tireWorldVel);

                float desiredChangeInVel = -steeringVel * gripFactor;

                float desiredLocalAccel = desiredChangeInVel / Time.fixedDeltaTime;

                carRB.AddForceAtPosition(steerDir * tireMass * desiredLocalAccel, tireTransform.position);
            }
        }
    }
    

    void SuspensionUpdate(Transform tireTransform, RaycastHit hit)
    {
        //print(hit.distance);
        //doesnt work
        Vector3 springDir = tireTransform.up;

        Vector3 tireWorldVel = carRB.GetPointVelocity(tireTransform.position);

        float offset = suspensionRestDist - hit.distance;

        float vel = Vector3.Dot(springDir, tireWorldVel);

        float force = (offset * springStrength) - (vel * dampingScale);

        //print(force);
        carRB.AddForceAtPosition(springDir * force, tireTransform.position);
    }

    void AccelBrakingEvent(Transform tireTransform, RaycastHit hit)
    {
        Vector3 accelDir = tireTransform.forward;
        float accelInput = Input.GetAxis("Vertical");

        //print(accelInput);

        if (powerDistribution >= 0.0f && powerDistribution < 0.5f)
        {
            //FWD
            if (tireTransform.name == "FL" ||  tireTransform.name == "FR")
            {
                if (accelInput > 0.0f)
                {
                    float carSpeed = Vector3.Dot(transform.forward, carRB.velocity);
                    float normalisedSpeed = Mathf.Clamp01(carSpeed) / carTopSpeed;
                    float torque = engineForce * accelInput;
                    //print(torque);
                    carRB.AddForceAtPosition(accelDir * torque, tireTransform.position);
                }
                else if (accelInput < 0.0f)
                {
                    float carSpeed = Vector3.Dot(transform.forward, carRB.velocity);
                    float normalisedSpeed = Mathf.Clamp01(carSpeed) / carTopSpeed;
                    float brakeForce = engineForce * accelInput;
                    //print(torque);
                    carRB.AddForceAtPosition(accelDir * brakeForce, tireTransform.position);

                }
                Debug.DrawLine(tireTransform.position, accelDir * 5f);
            }
        }
        else if (powerDistribution > 0.5f)
        {
            //RWD
            if (tireTransform.name == "RL" || tireTransform.name == "RR")
            {
                if (accelInput > 0.0f)
                {
                    float carSpeed = Vector3.Dot(transform.forward, carRB.velocity);
                    float normalisedSpeed = Mathf.Clamp01(carSpeed) / carTopSpeed;
                    float torque = engineForce * accelInput;
                    //print(torque);
                    carRB.AddForceAtPosition(accelDir * torque, tireTransform.position);
                }
                else if (accelInput < 0.0f)
                {
                    float carSpeed = Vector3.Dot(transform.forward, carRB.velocity);
                    float normalisedSpeed = Mathf.Clamp01(carSpeed) / carTopSpeed;
                    float brakeForce = engineForce * accelInput;
                    //print(torque);
                    carRB.AddForceAtPosition(accelDir * brakeForce, tireTransform.position);

                }
                Debug.DrawLine(tireTransform.position, accelDir * 5f);
            }
        }
        else
        {
            //AWD
            if (accelInput > 0.0f)
            {
                float carSpeed = Vector3.Dot(transform.forward, carRB.velocity);
                float normalisedSpeed = Mathf.Clamp01(carSpeed) / carTopSpeed;
                float torque = engineForce/2 * accelInput;
                //print(torque);
                carRB.AddForceAtPosition(accelDir * torque, tireTransform.position);
            }
            else if (accelInput < 0.0f)
            {
                float carSpeed = Vector3.Dot(transform.forward, carRB.velocity);
                float normalisedSpeed = Mathf.Clamp01(carSpeed) / carTopSpeed;
                float brakeForce = engineForce/2 * accelInput;
                //print(torque);
                carRB.AddForceAtPosition(accelDir * brakeForce, tireTransform.position);

            }
            Debug.DrawLine(tireTransform.position, accelDir * 5f);
        }
    }


}
