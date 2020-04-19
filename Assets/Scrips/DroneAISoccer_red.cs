using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Panda;


//namespace UnityStandardAssets.Vehicles.Car
//{
[RequireComponent(typeof(DroneController))]
public class DroneAISoccer_red : MonoBehaviour
{
    private DroneController m_Drone; // the drone controller we want to use

    public GameObject terrain_manager_game_object;
    TerrainManager terrain_manager;

    public GameObject[] friends;
    public string friend_tag;
    public GameObject[] enemies;
    public string enemy_tag;

    public GameObject own_goal;
    public GameObject other_goal;
    public GameObject ball;
	PandaBehaviour myPandaBT;
	int id = 0;
    static int counter = 0;
	bool GoalKeeper = false;
    private void Start()
    {
		id = counter;
    	counter++;
		if(id == 0) {
			GoalKeeper = true;
		}
		
		myPandaBT = GetComponent<PandaBehaviour>();
        m_Drone = GetComponent<DroneController>();
        terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();

        friend_tag = gameObject.tag;
        if (friend_tag == "Blue")
            enemy_tag = "Red";
        else
            enemy_tag = "Blue";

        friends = GameObject.FindGameObjectsWithTag(friend_tag);
        enemies = GameObject.FindGameObjectsWithTag(enemy_tag);
        ball = GameObject.FindGameObjectWithTag("Ball");
    }
	
	[Task]
	private bool RisInfrontBall()
	{
		if((ball.transform.position - other_goal.transform.position).sqrMagnitude < (transform.position - other_goal.transform.position).sqrMagnitude) {
			return false;
		}
		return true;
	}
	[Task]
	private bool RisBehindBall()
	{
		if((ball.transform.position - other_goal.transform.position).sqrMagnitude < (transform.position - other_goal.transform.position).sqrMagnitude) {
			return true;
		}
		return false;
	}
	[Task]
	private void RGoBehind()
	{
		Vector3 goal = ball.transform.position;
		goal.x += 15;
		Vector3 move = (goal - m_Drone.transform.position).normalized;
		Debug.DrawLine(transform.position, goal, Color.blue, 10f);
		m_Drone.Move_vect(move);
	}
	[Task]
	private bool RisGoalie()
	{
		return GoalKeeper;
	}
	[Task]
	private void RDefend(float p)
	{
		Debug.Log(id+" Defend " + p);
		Vector3 move = (own_goal.transform.position - m_Drone.transform.position).normalized;
		m_Drone.Move_vect(move);
	}
	[Task]
	private bool RisChaser()
	{
		Debug.Log(id+" Is Chaser?");
		float minDist = float.MaxValue;
		foreach(GameObject drone in friends) {
			if(drone.transform.position != m_Drone.transform.position) {
				float dist = (drone.transform.position-ball.transform.position).sqrMagnitude;
				if(dist < minDist) {
					minDist = dist;
				}
			}
		}
		if((m_Drone.transform.position-ball.transform.position).sqrMagnitude < minDist) {
			return true;
		}
		return false;
	}
	[Task]
	private void RInterceptBall()
	{
		Debug.Log(id+" Intercept?");
		Vector3 move = (ball.transform.position - m_Drone.transform.position).normalized;
		m_Drone.Move_vect(move);
	}
	[Task]
	private bool RIsBallCloserThan(float p)
	{
		Debug.Log(id+" Is ball closer?");
		return (m_Drone.transform.position-ball.transform.position).sqrMagnitude < p*p;
	}
	[Task]
	private void RDribble()
	{
		Debug.Log(id+" dribble?");
		Vector3 move = (ball.transform.position - m_Drone.transform.position).normalized;
		m_Drone.Move_vect(move);
	}
	private void Update() {
		myPandaBT.Reset();
		myPandaBT.Tick();
	}

    /*private void FixedUpdate()
    {


        // Execute your path here
        // ...

        Vector3 avg_pos = Vector3.zero;

        foreach (GameObject friend in friends)
        {
            avg_pos += friend.transform.position;
        }
        avg_pos = avg_pos / friends.Length;
        //Vector3 direction = (avg_pos - transform.position).normalized;
        Vector3 direction = (own_goal.transform.position - transform.position).normalized;



        // this is how you access information about the terrain
        int i = terrain_manager.myInfo.get_i_index(transform.position.x);
        int j = terrain_manager.myInfo.get_j_index(transform.position.z);
        float grid_center_x = terrain_manager.myInfo.get_x_pos(i);
        float grid_center_z = terrain_manager.myInfo.get_z_pos(j);

        Debug.DrawLine(transform.position, ball.transform.position, Color.black);
        Debug.DrawLine(transform.position, own_goal.transform.position, Color.green);
        Debug.DrawLine(transform.position, other_goal.transform.position, Color.yellow);
        Debug.DrawLine(transform.position, friends[0].transform.position, Color.cyan);
        //Debug.DrawLine(transform.position, enemies[0].transform.position, Color.magenta);



        // this is how you control the car
        m_Drone.Move_vect(direction);
        //m_Car.Move(0f, -1f, 1f, 0f);


    }*/
}
//}
