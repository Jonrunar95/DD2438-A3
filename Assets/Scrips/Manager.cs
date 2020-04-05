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
            if (t1 <= 0 && t2 <= 0) return float.NaN;

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
    private float drone_radius, intercept_upper_m, surrounding_radius;

    public Manager(float drone_radius, float intercept_upper_m, float surrounding_radius)
    {
        this.drone_radius = drone_radius;
        this.intercept_upper_m = intercept_upper_m;
        this.surrounding_radius = surrounding_radius;
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

    private List<Tuple<Drone, float>> Surrounding(Drone drone)
    {
        List<Tuple<Drone, float>> res = new List<Tuple<Drone, float>>();

        foreach (int key in drone_population.Keys)
        {
            Drone check = drone_population[key];
            if (check != drone)
            {
                float dist = drone.Distance(check);

                if (dist <= drone_radius)
                {
                    res.Add(new Tuple<Drone, float>(check, dist));
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

    public float InterceptDistance(Drone info_drone, float t)
    {
        return (info_drone.direction * info_drone.velocity * t * Time.deltaTime).magnitude;
    }


    public Vector3 AdjustIntercept(Drone info_drone, Vector3 move)
    {
        /*
            If drone intercepts another drone, sway away from it.
         */

        List<Tuple<Drone, float>> intercepts = Interceptions(info_drone);

        Debug.Log(">> count : " + intercepts.Count);
        if (intercepts.Count != 0)
        {

            Drone min_drone = null;
            float min_intercept = 1000000000;

            foreach (Tuple<Drone, float> t in intercepts)
            {
                if (t.Item2 < min_intercept)
                {
                    min_intercept = t.Item2;
                    min_drone = t.Item1;
                }
            }

            Debug.Log(">> min: " + InterceptDistance(info_drone, min_intercept));
            if (InterceptDistance(info_drone, min_intercept) <= intercept_upper_m)
            {
                float angle = Vector3.Angle(move, min_drone.direction);
                move = Quaternion.AngleAxis(-angle*Time.deltaTime, Vector3.up) * move;
            }


            if (InterceptDistance(info_drone, min_intercept) <= 10f)
            {
                info_drone.SetTargetVelocity(Mathf.Clamp(info_drone.target_velocity - ((1 / InterceptDistance(info_drone, min_intercept))), 6, 15));
            }
            else
            {
                info_drone.SetTargetVelocity(Mathf.Clamp(info_drone.target_velocity + (1 / InterceptDistance(info_drone, min_intercept)), 0, 15));
            }
            DrawIntercept(info_drone, min_intercept);

        }
        else
        {
            info_drone.SetTargetVelocity(Mathf.Clamp(info_drone.target_velocity + 0.5f, 0, 15));
        }
        return move;
    }

    public Vector3 AdjustFormation(Drone info_drone, Vector3 move)
    {
        List<Tuple<Drone, float>> surrounding = Surrounding(info_drone);
        Vector3 adjust_vector = new Vector3(0, 0, 0);
        if (surrounding.Count != 0)
        {
            //Compute weights

            
            List<float> weights = new List<float>();
            float sum = 0;
            foreach(Tuple<Drone, float> t in surrounding)
            {
                weights.Add(t.Item2);
                sum += t.Item2;
            }

            for(int i = 0; i < surrounding.Count; i++)
            {
                adjust_vector += (-info_drone.position + surrounding[i].Item1.position).normalized * (weights[i] / sum);
            }
        }

        return (0.1f*move - 0.9f*adjust_vector).normalized;
    }

    public void AdjustGoalSpeed(Drone info_drone, Vector3 goal)
    {
        if (Vector3.Distance(goal, info_drone.position) <= 15f)
        {
            info_drone.SetTargetVelocity(Mathf.Clamp(info_drone.target_velocity - 0.5f, 1, 15));
        }
    }

    public Vector3 NextMove(DroneController drone, Vector3 goal)
    {
        float dt = Time.deltaTime;
        Drone info_drone = drone_population[drone.GetInstanceID()];

        
        info_drone.Update(drone);

        float v = info_drone.GetEstimatedVelocityInput();

        Vector3 move = (goal - drone.transform.position).normalized;


        move = AdjustIntercept(info_drone, move);
        move = AdjustFormation(info_drone, move);
        AdjustGoalSpeed(info_drone, goal);
        return move * v;
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
