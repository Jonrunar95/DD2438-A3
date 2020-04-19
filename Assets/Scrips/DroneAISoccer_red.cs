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
	static SManager manager = new SManager(4f, 30f, 7f);
	public GameObject[] friends;
	public string friend_tag;
	public GameObject[] enemies;
	public string enemy_tag;
	public GameObject own_goal;
	public GameObject other_goal;
	public GameObject ball;
	int id = 0;
	static int counter = 0;
	public const float base_velocity = 8;
	public List<Vector3> my_path;
	public PandaBehaviour myPandaBT;
	public Vector3 ball_velocity;
	public Vector3 ball_position;

	public float ball_mass = 1f;
	public float drone_mass = 1000f;
	public float drone_max_speed = 15f;
    public float ball_radius = 20f;
	public float drone_radius = 1.2f;

	InterceptMathOperations ops = new InterceptMathOperations();

	public float velocity = 0;
	bool GoalKeeper = false;
	Vector3 oldBallPos;
	Vector3 deltaBall = Vector3.zero;
	Vector3 dir = Vector3.one;    private void Start()
    {
		id = counter;
    	counter++;
		myPandaBT = GetComponent<PandaBehaviour>();
        // get the car controller
        m_Drone = GetComponent<DroneController>();
        terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();


        // note that both arrays will have holes when objects are destroyed
        // but for initial planning they should work
        friend_tag = gameObject.tag;
        if (friend_tag == "Blue")
            enemy_tag = "Red";
        else
            enemy_tag = "Blue";

        friends = GameObject.FindGameObjectsWithTag(friend_tag);
        enemies = GameObject.FindGameObjectsWithTag(enemy_tag);

        ball = GameObject.FindGameObjectWithTag("Ball");


        // Plan your path here
        // ...
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
		goal.x -= 15;
		Vector3 move = (goal - m_Drone.transform.position).normalized;
		//Debug.DrawLine(transform.position, goal, Color.blue, 10f);
		m_Drone.Move_vect(move);
	}
	[Task]
	private bool RisGoalie()
	{
		float minDist = float.MaxValue;
		foreach(GameObject drone in friends) {
			if(drone.transform.position != m_Drone.transform.position) {
				float dist = Vector3.Magnitude(drone.transform.position-own_goal.transform.position);
				if(dist < minDist) {
					minDist = dist;
				}
			}
		}
		if(Vector3.Magnitude(m_Drone.transform.position-own_goal.transform.position) < minDist) {
			return true;
		}
		return false;
	}
	[Task]
	private void RDefend()
	{
		bool b = false;
		for (int t = 1; t < 51; t++)
		{
			Vector3 ballPos = oldBallPos + t * deltaBall;
			if (
				ballPos.x > own_goal.transform.position.x - 1 && ballPos.x < own_goal.transform.position.x + 1 &&
				ballPos.y > own_goal.transform.position.y && ballPos.y < own_goal.transform.position.y + 10 &&
				ballPos.z > own_goal.transform.position.z - 15 && ballPos.z < own_goal.transform.position.z + 15
			)
			{
				Vector3 move = (ballPos - m_Drone.transform.position).normalized;
				m_Drone.Move_vect(move);
				b = true;
				return;
			}
		}
		if (!b)
		{
			Vector3 ballPos = own_goal.transform.position;
			if (ball.transform.position.z < ballPos.z - 10)
			{
				ballPos.z -= 10;
			}
			else if (ball.transform.position.z > ballPos.z + 10)
			{
				ballPos.z += 10;
			}
			else
			{
				ballPos.z = ball.transform.position.z;
			}

			Vector3 move = (ballPos - m_Drone.transform.position).normalized;
			m_Drone.Move_vect(move);
		}

	}
	[Task]
	private bool RisChaser()
	{
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
		Vector3 move = (ball.transform.position - m_Drone.transform.position).normalized;
		m_Drone.Move_vect(move);
	}
	[Task]
	private bool RIsBallCloserThan(float p)
	{
		return (m_Drone.transform.position-ball.transform.position).sqrMagnitude < p*p;
	}
	[Task]
	private void RDribble()
	{
		Vector3 move = (ball.transform.position - m_Drone.transform.position).normalized;
		m_Drone.Move_vect(move);
	}
	private void Update()
	{
		myPandaBT.Reset();
		myPandaBT.Tick();
	}

    private void FixedUpdate()
    {
		float dt = Time.deltaTime;

		ball_velocity = -(ball_position - ball.transform.position)/dt;
		ball_position = ball.transform.position;

		//manager.Update(m_Drone);
        //Debug.DrawLine(ball_position, ball_position + ball_velocity, Color.black);
	}
}
//}
