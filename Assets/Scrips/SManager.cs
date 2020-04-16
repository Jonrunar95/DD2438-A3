using System;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
