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


    private DateTime start;

    public Drone(DroneController drone, float R)
    {
        this.R = R;
        controller = new PidController(1, 1, 1, 1, 0); //TODO: Need to fine tune these
        start = DateTime.Now;
        id = drone.GetInstanceID();
        Update(drone);
    }

    public float GetEstimatedVelocityInput()
    {
        float res = (float)controller.ControlVariable(start - DateTime.Now);
        start = DateTime.Now;
        return res;
    }

    public void SetTargetVelocity(float velocity)
    {
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
    }

    public float NextIntercept(Drone drone)
    {

        /*
         * Returns t - the scalar producting a + t*v_1 = b + t*v_2. If no such scalar exists returns float.NaN
         */


        System.Numerics.Complex a = new System.Numerics.Complex(position.x, position.z);
        System.Numerics.Complex b = new System.Numerics.Complex(drone.position.x, drone.position.z);

        //Vector2 a = new Vector2(position.x, position.z);
        //Vector2 b = new Vector2(drone.position.x, drone.position.z);

        if (Math.Abs(a.Real - b.Real) <= float.Epsilon || Math.Abs(a.Imaginary - b.Imaginary) <= float.Epsilon) return float.NaN;

        System.Numerics.Complex v1 = new System.Numerics.Complex(direction.x, direction.z) * velocity;
        System.Numerics.Complex v2 = new System.Numerics.Complex(drone.direction.x, drone.direction.z) * velocity;

        //Vector2 v1 = new Vector2(direction.x, direction.z) * velocity;
        //Vector2 v2 = new Vector2(drone.direction.x, drone.direction.z) * velocity;

        float delta = Time.deltaTime;


        float t1 = (float)((-a + b - 2 * R) / (delta*(v1 + v2))).Imaginary;
        float t2 = (float)((-a + b + 2 * R) / (delta*(v1 + v2))).Imaginary;

        if (t1 < 0 && t2 < 0) return float.NaN;

        if (t1 < 0) return t2;

        if (t2 < 0) return t1;

        return Math.Min(t1, t2);
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
        Debug.Log(">> Min intercept: " + t);
        Debug.Log(">> direction x, y, z :" + from.direction.x + "," + from.direction.y + "," + from.direction.z);
        Debug.Log(">> velocity: " + from.velocity);

        Debug.DrawLine(from.position, from.position + from.direction * from.velocity * t * Time.deltaTime, Color.magenta, 0.1f);
    }

    public Vector3 NextMove(DroneController drone, Vector3 goal)
    {
        
        Drone info_drone = drone_population[drone.GetInstanceID()];

        info_drone.UpdateController();
        info_drone.Update(drone);


        Vector3 move = goal - info_drone.position;

        List<Tuple<Drone, float>> intercepts = Interceptions(info_drone);

        if (intercepts.Count != 0)
        {
            Drone min_drone;
            float min_intercept = 10000;

            foreach(Tuple<Drone, float> t in intercepts)
            {
                if (t.Item2 < min_intercept)
                {
                    min_intercept = t.Item2;
                    min_drone = t.Item1;
                } 
            }
            
            DrawIntercept(info_drone, min_intercept);
            //info_drone.SetTargetVelocity(min_intercept);
            //TODO add move

        }
        return move;
    }

    public void AddDrone(DroneController drone)
    {
        drone_population.Add(drone.GetInstanceID(), new Drone(drone, drone_radius));
    }

    public void Update(DroneController drone)
    {
        drone_population[drone.GetInstanceID()].Update(drone);
    }
}
