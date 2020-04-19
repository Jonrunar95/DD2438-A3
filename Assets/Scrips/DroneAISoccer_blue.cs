using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Panda;

//namespace UnityStandardAssets.Vehicles.Car
//{
    [RequireComponent(typeof(DroneController))]
    public class DroneAISoccer_blue : MonoBehaviour
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
	    int id = 0;
    	static int counter = 0;
		public const float base_velocity = 8;
		public List<Vector3> my_path;
		PandaBehaviour myPandaBT;
		bool GoalKeeper = false;
		Vector3 oldBallPos;
		Vector3 deltaBall = Vector3.zero;
		Vector3 dir = Vector3.one;
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
			oldBallPos = ball.transform.position;
        }
		[Task]
		private bool BisInfrontBall()
		{
			if((ball.transform.position - other_goal.transform.position).sqrMagnitude < (transform.position - other_goal.transform.position).sqrMagnitude) {
				return false;
			}
			return true;
		}
		[Task]
		private bool BisBehindBall()
		{
			if((ball.transform.position - other_goal.transform.position).sqrMagnitude < (transform.position - other_goal.transform.position).sqrMagnitude) {
				return true;
			}
			return false;
		}
		[Task]
		private void BGoBehind()
		{
			Vector3 goal = ball.transform.position;
			goal.x -= 15;
			Vector3 move = (goal - m_Drone.transform.position).normalized;
			m_Drone.Move_vect(move);
		}
		[Task]
        private bool BisGoalie()
        {
			Debug.Log("is Goalie: " + GoalKeeper);
			return GoalKeeper;
		}
		[Task]
		private void BDefend(float p)
		{
			bool b = false;
			for(int t = 1; t < 101; t++) {
				Vector3 ballPos = oldBallPos + t*deltaBall;
				if(
					ballPos.x > own_goal.transform.position.x-1 && ballPos.x < own_goal.transform.position.x+1 &&
					ballPos.y > own_goal.transform.position.y && ballPos.y < own_goal.transform.position.y+10 &&
					ballPos.z > own_goal.transform.position.z-15 && ballPos.z < own_goal.transform.position.z+15
				) {
					Vector3 move = (ballPos - m_Drone.transform.position).normalized;
					m_Drone.Move_vect(move);
					b = true;
					break;
				}
			}
			if(!b) {
				Vector3 ballPos = own_goal.transform.position;
				if(ball.transform.position.z < ballPos.z-15) {
					ballPos.z -= 15;
				} else if(ball.transform.position.z > ballPos.z+15) {
					ballPos.z += 15;
				} else {
					ballPos.z = ball.transform.position.z;
				}
				
				Vector3 move = (ballPos - m_Drone.transform.position).normalized;
				m_Drone.Move_vect(move);
			}
		}
		[Task]
		private bool BisChaser()
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
		private void BInterceptBall()
		{
			Debug.Log(id+" Intercept?");
			Vector3 move = (ball.transform.position - m_Drone.transform.position).normalized;
			m_Drone.Move_vect(move);
		}
		[Task]
		private bool BIsBallCloserThan(float p)
		{
			Debug.Log(id+" Is ball closer?");
			return (m_Drone.transform.position-ball.transform.position).sqrMagnitude < p*p;
		}
		[Task]
		private void BDribble()
		{
			float dist = Mathf.Sqrt(Mathf.Pow(ball.transform.position.x-transform.position.x, 2) + Mathf.Pow(ball.transform.position.z-transform.position.z, 2));
			Debug.Log("Distance: " + dist + " from: " + transform.position + " to: " + ball.transform.position);
			Vector3 move = (ball.transform.position - m_Drone.transform.position).normalized;
			m_Drone.Move_vect(move);
		}
		private void Update() {
			Vector3 newBallPos = ball.transform.position;
			deltaBall = newBallPos-oldBallPos;
			oldBallPos = newBallPos;
			myPandaBT.Reset();
			myPandaBT.Tick();
		}
    }
//}
