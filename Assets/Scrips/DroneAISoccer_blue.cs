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
		PandaBehaviour myPandaBT;
        private void Start()
        {
			Debug.Log("Start");
			myPandaBT = GetComponent<PandaBehaviour>();
            // get the car controller
            m_Drone = GetComponent<DroneController>();
            terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();

            // note that both arrays will have holes when objects are destroyed
            // but for initial planning they should work
            friend_tag = "Blue";
            if (friend_tag == "Blue")
                enemy_tag = "Red";
            else
                enemy_tag = "Blue";

            friends = GameObject.FindGameObjectsWithTag(friend_tag);
            enemies = GameObject.FindGameObjectsWithTag(enemy_tag);
			Debug.Log("Friends: " + friends.Length + " Enemies: " + enemies.Length);
            ball = GameObject.FindGameObjectWithTag("Ball");

			id = counter;
			Debug.Log("Car ID: " + id);
    	    counter++;
        	System.Random rand = new System.Random();
			manager.AddDrone(m_Drone, rand.Next(0, 100) < 90 ? 8f : base_velocity, other_goal.transform.position);
			my_path = new List<Vector3>();
			my_path.Add(other_goal.transform.position);
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
			Debug.DrawLine(transform.position, goal, Color.blue, 10f);
			m_Drone.Move_vect(move);
		}
		[Task]
        private bool BisGoalie()
        {
			Debug.Log(id+" Is Goalie?");
			float minDist = float.MaxValue;
			foreach(GameObject drone in friends) {
				Debug.Log((drone.transform.position != m_Drone.transform.position) + " " + drone.transform.position + " " + m_Drone.transform.position);
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
		private void BDefend(float p)
		{
			Debug.Log(id+" Defend " + p);
			Vector3 move = (own_goal.transform.position - m_Drone.transform.position).normalized;
			m_Drone.Move_vect(move);
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
			
			Vector3 move = manager.NextMove(m_Drone, ball.transform.position);

			m_Drone.Move_vect(move);
        	manager.Update(m_Drone);

            // Execute your path here
            // ...

            Vector3 avg_pos = Vector3.zero;

            foreach (GameObject friend in friends)
            {
                avg_pos += friend.transform.position;
            }
            avg_pos = avg_pos / friends.Length;
            //Vector3 direction = (avg_pos - transform.position).normalized;
            Vector3 direction = (ball.transform.position - transform.position).normalized;

            

            // this is how you access information about the terrain
            int i = terrain_manager.myInfo.get_i_index(transform.position.x);
            int j = terrain_manager.myInfo.get_j_index(transform.position.z);
            float grid_center_x = terrain_manager.myInfo.get_x_pos(i);
            float grid_center_z = terrain_manager.myInfo.get_z_pos(j);

            Debug.DrawLine(transform.position, ball.transform.position, Color.black);
            Debug.DrawLine(transform.position, own_goal.transform.position, Color.green);
            Debug.DrawLine(transform.position, other_goal.transform.position, Color.yellow);
            Debug.DrawLine(transform.position, friends[0].transform.position, Color.cyan);
            Debug.DrawLine(transform.position, enemies[0].transform.position, Color.magenta);



            // this is how you control the car
            m_Drone.Move_vect(direction);
            //m_Car.Move(0f, -1f, 1f, 0f);

			
        }*/
    }
//}
