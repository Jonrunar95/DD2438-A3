using System;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InterceptMathOperations
{
    public static bool QuadraticEqSolver(float a, float b, float c, out float x1, out float x2)
	{
        if (a == 0)
        {
            x1 = float.NaN;
            x2 = float.NaN;
            return false;
        }

        float sq = (float)Math.Sqrt(b * b - 4 * a * c);
        x1 = (-b + sq) / (2 * a);
        x2 = (-b - sq) / (2 * a);
        return true;
    }

    public List<Vector3> GeneratePointsOnCircle(Vector3 center, float radius, int nr_points)
    {
        List<Vector3> res = new List<Vector3>();

        float angle = (float)Math.PI * 2 / nr_points;

        float accum_angle = 0;
        
        for(int i = 0; i < nr_points; i++)
        {
            float x = center.x + radius * (float)Math.Cos(accum_angle);
            float y = center.z + radius * (float)Math.Sin(accum_angle);

            
            res.Add(new Vector3(x, 0, y));
            //Debug.DrawLine(center, res[res.Count - 1], Color.yellow, 1f);
            accum_angle += angle;
        }
        return res;
    }

    public Vector3 GetBallVelocityAfterCollision(Vector3 drone_v, Vector3 ball_v, Vector3 drone_loc_collision, Vector3 ball_loc_collision,  float drone_mass, float ball_mass)
    {
        Vector2 v1 = new Vector2(drone_v.x, drone_v.z);
        Vector2 v2 = new Vector2(ball_v.x, ball_v.z);
        Vector2 x1 = new Vector2(drone_loc_collision.x, drone_loc_collision.z);
        Vector2 x2 = new Vector2(ball_loc_collision.x, ball_loc_collision.z);

        float mh = ((2 * drone_mass) / (drone_mass + ball_mass));
        
        float dot = Vector2.Dot(v2 - v1, x2 - x1);
        float mg = (float)Math.Pow((x2 - x1).magnitude, 2);

        Vector2 res = v2 - (mh * (dot / mg)) * (x2 - x1);

        return new Vector3(res.x, 0, res.y);
    }

    public bool Intercept(Vector3 drone_loc, Vector3 ball_loc, float drone_max_speed, Vector3 ball_v, float drone_radius, out float t, out Vector3 direction)
	{
        if(Vector3.Distance(drone_loc, ball_loc) <= 4)
		{
            t = 0;
            direction = new Vector3();
            return true;
		}

        Vector2 bd = new Vector2(drone_loc.x - ball_loc.x, drone_loc.z - drone_loc.z);
        float bd_dist = bd.magnitude;
        float b_speed = (new Vector2(ball_v.x, ball_v.z)).magnitude;

        if (b_speed < 1e-3)
		{
            t = bd_dist / drone_max_speed;
            direction = -(drone_loc - ball_loc)/t;
            return true;
		}
		else
		{
            float a = drone_max_speed * drone_max_speed  - b_speed * b_speed;
            float b = 2 * Vector2.Dot(new Vector2(ball_v.x, ball_v.z), bd);
            float c = (float)-Math.Pow(bd_dist - drone_radius, 2);

            float t1, t2;

            if(!QuadraticEqSolver(a, b, c, out t1, out t2))
			{
                direction = new Vector3();
                t = -1;
                return false;
			}

            else if(t1 < 0 && t2 < 0)
			{
                direction = new Vector3();
                t = -1;
                return false;
			}

            else if (t1 > 0 && t2 > 0)
			{
                t = Math.Min(t1, t2);
			}
			else
			{
                t = Math.Max(t1, t2);
			}

            Vector3 i_position = ball_loc + ball_v * t;

            direction = (i_position - drone_loc) / t;

            return true;

		}

	}
}
public class SManager
{
    private Dictionary<int, Drone> drone_population;
    private float drone_radius, intercept_upper_m, surrounding_radius;

    public SManager(float drone_radius, float intercept_upper_m, float surrounding_radius)
    {
        this.drone_radius = drone_radius;
        this.intercept_upper_m = intercept_upper_m;
        this.surrounding_radius = surrounding_radius;
        drone_population = new Dictionary<int, Drone>();
    }

    public float GetVelocity(DroneController drone)
    {
        Drone info_drone = drone_population[drone.GetInstanceID()];

        return info_drone.GetEstimatedVelocityInput();
    }

    public void SetTargetVelocity(DroneController drone, float v)
    {
        Drone info_drone = drone_population[drone.GetInstanceID()];

        info_drone.SetTargetVelocity(v);
    }

    public Vector3 NextMove(DroneController drone, Vector3 goal)
    {
        float dt = Time.deltaTime;
        Drone info_drone = drone_population[drone.GetInstanceID()];


        info_drone.Update(drone);

        float v = info_drone.GetEstimatedVelocityInput();

        Vector3 move = (goal - drone.transform.position).normalized;

        return move * v;
    }

    public void AddDrone(DroneController drone, float target_velocity, Vector3 goal)
    {
        Drone d = new Drone(drone, drone_radius, goal);
        d.SetTargetVelocity(target_velocity);
        drone_population.Add(drone.GetInstanceID(), d);
    }

    public void Update(DroneController drone)
    {
        drone_population[drone.GetInstanceID()].Update(drone);
    }

}