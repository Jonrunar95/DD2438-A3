using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DroneController))]
public class DroneAI : MonoBehaviour
{

    private DroneController m_Drone; // the car controller we want to use

    public GameObject my_goal_object;
    public GameObject terrain_manager_game_object;
    TerrainManager terrain_manager;
    static Manager manager = new Manager(3f, 70f, 15f);
    public GameObject[] friends;
    int id = 0;

    public Vector3 current_goal;
    public int current_idx;

    static int counter = 0;
    bool follow_path = true;

    public List<Vector3> my_path = new List<Vector3>();

    public const float base_velocity = 1.5f;
    float elapsed = 0;
    static List<int> ids = new List<int>();

    private void Randomize(List<int> ids)
    {
        System.Random rand = new System.Random();

        for(int i = 0; i < ids.Count; i++)
        {
            int a = rand.Next(0, ids.Count);
            int b = rand.Next(0, ids.Count);

            int t = ids[a];
            ids[a] = ids[b];
            ids[b] = t;
        }
    }
    private void Start()
    {
        id = counter;
        ids.Add(id);
        Randomize(ids);
        id = ids[id];
        counter++;
        System.Random rand = new System.Random();
        // get the drone controller
        m_Drone = GetComponent<DroneController>();
        terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
        manager.AddDrone(m_Drone, base_velocity, my_goal_object.transform.position);
        

        Vector3 start_pos = terrain_manager.myInfo.start_pos;
        Vector3 goal_pos = terrain_manager.myInfo.goal_pos;
        friends = GameObject.FindGameObjectsWithTag("Player");


        //my_path = TerrainManager.r_terrain.GetPath(m_Drone.transform.position, my_goal_object.transform.position, manager);
        //my_path.Add(my_goal_object.transform.position);
        Vector3 old_wp;
        
        
        my_path = TerrainManager.r_terrain.GetWeightedPath(m_Drone.transform.position, my_goal_object.transform.position, manager);
        Debug.Log("Path length: " + my_path.Count);
        old_wp = my_path[0];
        current_goal = my_path[0];
        current_idx = 0;
        TerrainManager.r_terrain.seen.Clear();
        foreach (var wp in my_path)
        {
            //Debug.DrawLine(old_wp, wp, Color.red, 100f);
            old_wp = wp;
        }
        


        //yield return new WaitForSeconds(rand.Next(0, 100));
        /*
        while (true)
        {


            my_path = TerrainManager.r_terrain.GetWeightedPath(m_Drone.transform.position, my_goal_object.transform.position, manager);
            Debug.Log("Path length: " + my_path.Count);
            old_wp = my_path[0];
            current_goal = my_path[0];
            current_idx = 0;
            TerrainManager.r_terrain.seen.Clear();
            foreach (var wp in my_path)
            {
                Debug.DrawLine(old_wp, wp, Color.red, 15f);
                old_wp = wp;
            }
            yield return new WaitForSeconds(15f);
            
            
        }*/
        

        Debug.Log(">> path length " + my_path.Count);
        // Plan your path here
        // ...





        // Plot your path to see if it makes sense


        
    }

    public Vector3 GetNextGoal()
    {
        /*
        if (Vector3.Distance(m_Drone.transform.position, current_goal) <= 4)
        {
            current_idx++;
        }
        //if (current_idx < my_path.Count && !TerrainManager.r_terrain.Visible(m_Drone.transform.position, my_path[current_idx])) current_idx--;
        if (current_idx >= my_path.Count) return my_goal_object.transform.position;
        current_goal = my_path[current_idx];
        return current_goal;
        */
        for (int i = my_path.Count - 1; i >= 0; i--)
        {
            if (TerrainManager.r_terrain.Visible(m_Drone.transform.position, my_path[i]))
            {
                return my_path[i];
            }
        }

        return my_goal_object.transform.position;
        

    }

    private void FixedUpdate()
    {
        // Execute your path here
        // ...

        // this is how you access information about the terrain
        elapsed += Time.deltaTime;
        if (elapsed < id * 0.28f)
        {
            //Vector3 relVect = my_goal_object.transform.position - transform.position;

            //m_Drone.Move_vect(-relVect);
            return;
        }
        if (my_path.Count == 0) return;

        int i = terrain_manager.myInfo.get_i_index(transform.position.x);
        int j = terrain_manager.myInfo.get_j_index(transform.position.z);
        float grid_center_x = terrain_manager.myInfo.get_x_pos(i);
        float grid_center_z = terrain_manager.myInfo.get_z_pos(j);

        //Debug.DrawLine(transform.position, new Vector3(grid_center_x, 0f, grid_center_z), Color.white, 1f);

        Vector3 goal = my_goal_object.transform.position;
        if (follow_path)
        {
            goal = GetNextGoal();
        }
        
        //Vector3 relVect = my_goal_object.transform.position - transform.position;

        //m_Drone.Move_vect(relVect);

        
        Vector3 move = manager.NextMove(m_Drone, goal);
        //Debug.Log(">> Move : " + move.x + "," + move.z);
        
        m_Drone.Move_vect(move);
        manager.Update(m_Drone);
        Debug.DrawLine(m_Drone.transform.position, goal, Color.cyan);

        Debug.DrawLine(m_Drone.transform.position, m_Drone.transform.position + move.normalized*15, Color.black);
        if(id == 0) Debug.Log(">> Drone velocity: " + m_Drone.velocity.magnitude);
        

        

    }

 

    // Update is called once per frame
    void Update()
    {
        
    }
}
