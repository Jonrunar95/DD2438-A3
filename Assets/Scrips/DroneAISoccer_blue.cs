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
	public PandaBehaviour myPandaBT;
	public Vector3 ball_velocity;
	public Vector3 ball_position;

	public float ball_mass = 1f;
	public float drone_mass = 1000f;
	public float drone_max_speed = 15f;
    public float ball_radius = 2.0f;
	public float drone_radius = 1.2f;
	public float field_width = 180f;
	InterceptMathOperations ops = new InterceptMathOperations();

	public float velocity = 0;


	bool GoalKeeper = false;
	bool Chaser = false;
	bool Center = false;

	Vector3 oldBallPos;
	Vector3 deltaBall = Vector3.zero;
	Vector3 dir = Vector3.one;

	private void Start()
	{
		id = counter;
		counter++;
		if (id == 0) GoalKeeper = true;
		if (id == 1) Chaser = true;
		if (id == 2) Center = true;

		ball_mass = 1f;
	    drone_mass = 1000f;
	    drone_max_speed = 10f;
	    ball_radius = 2.1f;
	    drone_radius = 1.6f;
		field_width = 180f;

		Debug.Log("Start");
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
		Debug.Log("Friends: " + friends.Length + " Enemies: " + enemies.Length);
		ball = GameObject.FindGameObjectWithTag("Ball");
		ball_position = ball.transform.position;




		
		System.Random rand = new System.Random();
		manager.AddDrone(m_Drone, base_velocity, other_goal.transform.position);
		my_path = new List<Vector3>();
		my_path.Add(other_goal.transform.position);

		
	}

    [Task]
    private void Bfoo()
    {

    }

	[Task]
	private bool BisInfrontBall()
	{
		if ((ball.transform.position - other_goal.transform.position).sqrMagnitude < (transform.position - other_goal.transform.position).sqrMagnitude)
		{
			return false;
		}
		return true;
	}

	[Task]
	private bool BisBehindBall()
	{
		if ((ball.transform.position - other_goal.transform.position).sqrMagnitude < (transform.position - other_goal.transform.position).sqrMagnitude)
		{
			return true;
		}
		return false;
	}

	[Task]
	private void BGoBehind()
	{
		Vector3 goal = ball.transform.position;
		goal.x -= 5;
		Vector3 move = (goal - m_Drone.transform.position).normalized;
		//Debug.DrawLine(transform.position, goal, Color.blue, 10f);
		m_Drone.Move_vect(move);
		/*Vector3 goal = ball.transform.position;
		goal.x -= 5;
		bool crash = false;
		for (int i = 1; i < 51; i++)
		{
			Vector3 ballPos = ball.transform.position + ball_velocity * i;
			Vector3 dronePos = transform.position + m_Drone.velocity * i;
			Vector3 diff = ballPos - dronePos;
			if (Mathf.Abs(diff.x) > 1 || Mathf.Abs(diff.y) > 1 || Mathf.Abs(diff.z) > 1)
			{
				crash = true;
				i = 51;
			}
		}
		if (crash)
		{
			goal.z += 5;
		}
		Vector3 move = (goal - m_Drone.transform.position).normalized;
		Debug.DrawRay(goal, dir, Color.black, 10f);
		m_Drone.Move_vect(move);*/
	}

	[Task]
	private bool BisGoalie()
	{
		//Debug.Log("is Goalie: " + GoalKeeper);
		return GoalKeeper;
	}

    [Task]
    private bool BisBallInDefenceLine (float p)
    {
		if (Math.Abs(ball.transform.position.x - own_goal.transform.position.x) <= p * field_width) return true;
		return false;
    }

	[Task]
	private void BDefend(float p)
	{

		RaycastHit hit;

		RaycastHit[] hh = Physics.SphereCastAll(ball.transform.position, ball_radius, ball_velocity);

		bool intercept = false;
        /*foreach(RaycastHit h in hh)
        {
			if (h.collider.name.Contains(friend_tag) && h.collider.name.Contains("goal")) intercept = true;
        }

        if (intercept ||  Math.Abs(ball.transform.position.x - own_goal.transform.position.x) <= p * field_width)
        {
			intercept = true;
			float tt;

			Vector3 move = FindIntercept(out tt, "Goalie");

            if((tt*dir).magnitude <= p*2 * field_width)
            {
                
				manager.SetTargetVelocity(m_Drone, move.magnitude);

				m_Drone.Move_vect(move.normalized * manager.GetVelocity(m_Drone));
			}
            else
            {
				intercept = false;
            }
        }*/

        if(!intercept)
        {
			//Debug.Log("p: " + p);
			int sign = (friend_tag == "Blue") ? 1 : -1;
			float z = Mathf.Clamp(ball.transform.position.z, own_goal.transform.position.z - 10, own_goal.transform.position.z + 10);
			float x = own_goal.transform.position.x + sign * p * field_width * 0.5f;

			Vector3 pos = new Vector3(x, 0, (GoalKeeper) ? z : ball.transform.position.z);

            Vector3 move = (pos - m_Drone.transform.position).normalized;
			m_Drone.Move_vect(move.normalized);
		}

	}

    [Task]
    void BRepulse()
    {
		foreach (GameObject obj in enemies)
		{
			if (Vector3.Distance(obj.transform.position, m_Drone.transform.position) <= 2 * drone_radius + 0.5)
			{
				m_Drone.Move_vect(m_Drone.velocity*10);
			}
		}
		foreach (GameObject obj in friends)
		{
			if (Vector3.Distance(obj.transform.position, m_Drone.transform.position) >= drone_radius/2 && Vector3.Distance(obj.transform.position, m_Drone.transform.position) <= 2 * drone_radius + 2)
			{
				m_Drone.Move_vect(m_Drone.velocity*10);
			}
		}
	}

    [Task]
    bool BisStuck()
    {
		foreach(GameObject obj in enemies)
        {
            if(Vector3.Distance(obj.transform.position, m_Drone.transform.position) <= 2 * drone_radius + 2)
            {
				return true;
            }
        }
		foreach (GameObject obj in friends)
		{
			if (obj.transform.position != m_Drone.transform.position && Vector3.Distance(obj.transform.position, m_Drone.transform.position) <= 2 * drone_radius + 2)
			{
				return true;
			}
		}

		return false;
	}

	[Task]
	bool BisChaser()
	{
		//Debug.Log(id + " Is Chaser?");
		return Chaser;
	}

    [Task]
    bool BisCenter()
    {
		return Center;
    }


	[Task]
	private bool BIsBallCloserThan(float p)
	{
		//Debug.Log(id + " Is ball closer?");
		return (m_Drone.transform.position - ball.transform.position).sqrMagnitude < p * p;
	}

	[Task]
	void BDribble()
	{
		
		//Debug.Log(id + " dribble?");
		float tt;
		Vector3 move = FindIntercept(out tt);//(ball.transform.position - m_Drone.transform.position).normalized;

		manager.SetTargetVelocity(m_Drone, move.magnitude);

		m_Drone.Move_vect(move.normalized * manager.GetVelocity(m_Drone));
	}

    public Vector3 FindGoodDirectionVector(List<Vector3> vecs, out int idx)
    {
		float sep_angle = 5;

		DisjointSetUBR uf = new DisjointSetUBR(vecs.Count);

        //Seperate into disjoint sets by angle proximity
        for(int i = 0; i < vecs.Count; i++)
        {
            for(int j = i + 1; j < vecs.Count; j++)
            {
                if(Vector3.Angle(vecs[i], vecs[j]) <= sep_angle)
                {
					uf.Union(i, j);
                }
            }
        }

		Dictionary<int, List<Vector3>> sets = new Dictionary<int, List<Vector3>>();
		int max_set = 0;
		int max_id = 0;
        for(int i = 0; i < vecs.Count; i++)
        {
			int id = uf.Find(i);

			if (!sets.ContainsKey(id)) sets.Add(id, new List<Vector3>());
			sets[id].Add(vecs[i]);

            //Keep the largest set
            if(sets[id].Count > max_set)
            {
				max_set = sets[id].Count;
				max_id = id;
            }
        }

		//Find the vector that has the minimum max angle to all others in set

		Vector3 res = new Vector3();
		float mn_angle = 10000f;
		idx = 0;
		int ii = 0;
        foreach(Vector3 v in sets[max_id])
        {
			float max_angle = 0;
            foreach(Vector3 vv in sets[max_id])
            {
				max_angle = Math.Max(max_angle, Vector3.Angle(v, vv));
            }

            if (mn_angle > max_angle)
            {
				mn_angle = max_angle;
				res = v;

            }
			ii++;
        }
        for(int i = 0; i < vecs.Count; i++)
        {
			if (vecs[i] == res) idx = i;
        }
		return res;

	}

    public Vector3 FindIntercept(out float tt, string player = "Chaser")
    {
		int N = 100;
		List<Vector3> potential_intercept_points = ops.GeneratePointsOnCircle(ball.transform.position, ball_radius, N);
		//(">> ball radius: " + ball_radius);
		ops.GeneratePointsOnCircle(m_Drone.transform.position, drone_radius, N);
		Vector3 res = (ball.transform.position - m_Drone.transform.position).normalized;
		float mn = 100000f;
		List<Vector3> all_vecs = new List<Vector3>();
		List<float> all_t = new List<float>();
		foreach (Vector3 p in potential_intercept_points)
        {
			//Vector3 drone_loc, Vector3 ball_loc, float drone_max_speed, Vector3 ball_v, float drone_radius, out float t, out Vector3 direction

			float t;
			Vector3 direction;

            if(ops.Intercept(m_Drone.transform.position, p, drone_max_speed, ball_velocity, drone_radius, out t, out direction))
            {
				//Vector3 drone_v, Vector3 ball_v, Vector3 drone_loc_collision, Vector3 ball_loc_collision,  float drone_mass, float ball_mass

				Vector3 drone_loc_collision = m_Drone.transform.position + t * direction;
				Vector3 ball_loc_collision = ball.transform.position + t * ball_velocity;

				Vector3 new_ball_v = ops.GetBallVelocityAfterCollision(direction, ball_velocity, drone_loc_collision, ball_loc_collision, drone_mass, ball_mass);

				if(player == "Chaser")
                {
					RaycastHit hit;

					//Debug.Log(">> Collider found " + new_ball_v.magnitude + "id :" + id);
					bool h = Physics.SphereCast(ball_loc_collision, ball_radius, new_ball_v, out hit);


					if (h && hit.collider.name.Contains(enemy_tag))
					{
						float dot = Vector3.Dot(direction, new_ball_v) / Vector3.Distance(direction, new_ball_v);
						Debug.DrawLine(ball_loc_collision, ball_loc_collision + new_ball_v, Color.magenta);
						Debug.DrawLine(m_Drone.transform.position, m_Drone.transform.position + direction, Color.cyan);
						all_vecs.Add(direction);
						all_t.Add(t);
					}
				}
                else if(player == "Goalie")
                {
					Vector3 dir = (new_ball_v - other_goal.transform.position).normalized;
					
					float angle = (float)(Math.Atan2(1, 0) - Math.Atan2(dir.z, dir.x)) * Mathf.Rad2Deg;
					//Debug.Log(">> angle: " + angle);
					if (angle <= 120 && angle >= 0)
                    {
						Debug.DrawLine(ball_loc_collision, ball_loc_collision + new_ball_v, Color.magenta);
						Debug.DrawLine(m_Drone.transform.position, m_Drone.transform.position + direction, Color.cyan);
						all_vecs.Add(direction);
						all_t.Add(t);
					}
					
				}
				

			}
		}

        if(all_vecs.Count != 0)
        {
			int idx;
			res = FindGoodDirectionVector(all_vecs, out idx);
			tt = all_t[idx];
			return res;
        }
        else if(player == "Goalie")
        {
			tt = 0;
			return new Vector3();
        }
		tt = 0;
		return res;
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

		manager.Update(m_Drone);
        //Debug.DrawLine(ball_position, ball_position + ball_velocity, Color.black);



	}
}