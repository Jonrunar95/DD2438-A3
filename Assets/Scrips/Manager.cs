using System;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone
{
    public Vector3 position, direction;
    public float velocity, R;
    public int id;
    public PidController controller;
    public float target_velocity;


    private DateTime start;

    public Drone(DroneController drone, float R)
    {
        
        this.R = R;
        controller = new PidController(0.1, 0.1, 0.3, drone.max_speed, 0); //TODO: Need to fine tune these
        start = DateTime.Now;
        id = drone.GetInstanceID();
        Update(drone);
    }

    public float GetEstimatedVelocityInput()
    {
        float res = (float)controller.ControlVariable(DateTime.Now - start);
        start = DateTime.Now;
        return res;
    }

    public void SetTargetVelocity(float velocity)
    {
        target_velocity = velocity;
        controller.SetPoint = velocity;
    }

    public void UpdateController()
    {
        controller.ProcessVariable = this.velocity;
    }

    public void Update(DroneController drone)
    {
        position = drone.transform.position;
        direction = drone.velocity.normalized;
        velocity = drone.velocity.magnitude;
        UpdateController();
    }

    public float NextIntercept(Drone drone)
    {

        /*
         * Returns t - the scalar producting a + t*v_1 = b + t*v_2. If no such scalar exists returns float.NaN
         */


        float dt = Time.deltaTime;

        Vector2 a = new Vector2(position.x, position.z);
        Vector2 b = new Vector2(drone.position.x, drone.position.z);


        Vector2 v1 = new Vector2(direction.x, direction.z) * velocity*dt;
        Vector2 v2 = new Vector2(drone.direction.x, drone.direction.z) * velocity*dt;


        float x = Vector2.Dot(v1 - v2, v1 - v2);
        float y = Vector2.Dot(a - b, v1 - v2);
        float z = Vector2.Dot(a - b, a - b);

        if (x == 0 && y != 0)
        {
            return -((z - 4 * R*R) / 2 * y);
        }

        if (x != 0)
        {
            float t1 = (float)-((Math.Sqrt(4 * R * R * x - x * z + y * y) + y) / x);
            float t2 = (float)((Math.Sqrt(4 * R * R * x - x * z + y * y) - y) / x);

            //if (t1 < 0 || t2 < 0) throw new Exception("Invalid intercept calculations");
            float t = Mathf.Clamp(Math.Min(t1, t2), 0, 10000f);

            if (t >= 100f)
            {
                return float.NaN;
            }
            return t;
        }


        return float.NaN;
    }

    public float Distance(Drone drone)
    {
        return Vector3.Distance(drone.position, position);
    }

    public float Angle(Drone drone)
    {
        return Vector3.Angle(direction, drone.direction);
    }

    public override int GetHashCode()
    {
        return id;
    }

    public override bool Equals(object obj)
    {
        return ((Drone)obj).id == id;
    }
}

public class Manager
{
    private Dictionary<int, Drone> drone_population;
    private float drone_radius;

    public Manager(float drone_radius)
    {
        this.drone_radius = drone_radius;
        drone_population = new Dictionary<int, Drone>();
    }

    private List<Tuple<Drone, float>> Interceptions(Drone drone)
    {
        List<Tuple<Drone, float>> res = new List<Tuple<Drone, float>>();

        foreach(int key in drone_population.Keys)
        {
            Drone check = drone_population[key];
            if (check != drone)
            {
                float intercept = drone.NextIntercept(check);

                if (!float.IsNaN(intercept))
                {
                    res.Add(new Tuple<Drone, float>(check, intercept));
                }
            }
        }
        return res;
    }

    private void DrawIntercept(Drone from, float t)
    {
        //Debug.Log(">> Min intercept: " + t*Time.deltaTime);
        //Debug.Log(">> direction x, y, z :" + from.direction.x + "," + from.direction.y + "," + from.direction.z);
        //Debug.Log(">> velocity: " + from.velocity);

        Debug.DrawLine(from.position, from.position + from.direction * from.velocity * t * Time.deltaTime, Color.magenta, 0.1f);
    }

    public Vector3 NextMove(DroneController drone, Vector3 goal)
    {
        
        Drone info_drone = drone_population[drone.GetInstanceID()];

        
        info_drone.Update(drone);

        float v = info_drone.GetEstimatedVelocityInput();

        /*Test pid controller


        

        return (goal - info_drone.position) * v;
        */

        Vector3 move = (goal - info_drone.position).normalized;

        List<Tuple<Drone, float>> intercepts = Interceptions(info_drone);

        Debug.Log(">> count : " + intercepts.Count);
        if (intercepts.Count != 0)
        {
            
            Drone min_drone = null;
            float min_intercept = 1000000000;

            foreach(Tuple<Drone, float> t in intercepts)
            {
                if (t.Item2 < min_intercept)
                {
                    min_intercept = t.Item2;
                    min_drone = t.Item1;
                } 
            }

            float angle = Vector3.Angle(move, min_drone.direction);
            move = Quaternion.AngleAxis(angle, Vector3.up) * move;

            //info_drone.SetTargetVelocity(Mathf.Clamp(info_drone.target_velocity - (5 * (1 / min_intercept)), 2, 15));
            DrawIntercept(info_drone, min_intercept);

            return move * v;

        }
        else
        {
            info_drone.SetTargetVelocity(Mathf.Clamp(info_drone.target_velocity + 0.5f, 0, 15));
            return move * v;
        }
        
    }

    public void AddDrone(DroneController drone, float target_velocity)
    {
        Drone d = new Drone(drone, drone_radius);
        d.SetTargetVelocity(target_velocity);
        drone_population.Add(drone.GetInstanceID(), d);
    }

    public void Update(DroneController drone)
    {
        drone_population[drone.GetInstanceID()].Update(drone);
    }
}
