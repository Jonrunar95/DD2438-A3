using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class Node : IComparable
{
    public int i, j;
    public Vector3 pos;
    public Node parent;
    public float priority;

    public Node(int i, int j, Vector3 pos, Node parent, float priority = 0)
    {
        this.i = i;
        this.j = j;
        this.pos = pos;
        this.parent = parent;
        this.priority = priority;
    }

    public int CompareTo(object obj)
    {
        return priority.CompareTo(((Node)obj).priority);
    }

    public override bool Equals(object obj)
    {
        return ((Node)obj).i == i && ((Node)obj).j == j;
    }

    public override int GetHashCode()
    {
        return (i + 1) * (j + 1);
    }
}

public class Edge
{
    public Node from, to;
    public float cost;

    public Edge(Node from, Node to, float cost = 1)
    {
        this.from = from;
        this.to = to;
        this.cost = cost;
    }

    public override bool Equals(object obj)
    {
        Edge other = (Edge)obj;

        return other.from.Equals(from) && other.to.Equals(to);
    }

    public override int GetHashCode()
    {
        return from.GetHashCode() * to.GetHashCode();
    }

}

public class ResolutionTerrain
{
    TerrainManager terrain;
    private int resolution, x_N, z_N;
    public float x_high, x_low, z_high, z_low; 
    public float[,] resolution_matrix;
    public HashSet<int> seen = new HashSet<int>();
    public Dictionary<Edge, Edge> edges = new Dictionary<Edge, Edge>();
    private float[,] wall_distance;
    private List<Tuple<int, int>> walls;

    public ResolutionTerrain(TerrainManager terrain, int resolution)
    {
        //Blows up the dimension of terrain by resolution
        this.terrain = terrain;
        this.resolution = resolution;
        x_N = terrain.myInfo.x_N;
        z_N = terrain.myInfo.z_N;
        x_high = terrain.myInfo.x_high;
        x_low = terrain.myInfo.x_low;
        z_high = terrain.myInfo.z_high;
        z_low = terrain.myInfo.z_low;

        CreateResolutionMatrix();

        CreateEdges();
    }

    public void CreateEdges()
    {
        for(int i = 0; i < x_N*resolution; i++)
        {
            for(int j = 0; j < z_N*resolution; j++)
            {
                Node parent = new Node(i, j, new Vector3(), null);

                foreach(Node child in Expand(parent))
                {
                    Edge front_edge = new Edge(parent, child);
                    Edge back_edge = new Edge(child, parent);

                    if (!edges.ContainsKey(front_edge))
                    {
                        edges.Add(front_edge, front_edge);
                    }
                    if (!edges.ContainsKey(back_edge))
                    {
                        edges.Add(back_edge, back_edge);
                    }
                    
                }
            }
        }
    }

    public int GetXDim()
    {
        return x_N * resolution;
    }

    public int GetZDim()
    {
        return z_N * resolution;
    }


    public bool Visible(Vector3 from, Vector3 to)
    {
        float distance = Vector3.Distance(from, to);
        Vector3 direction = (to - from).normalized;

        int layer_mask = LayerMask.GetMask("CubeWall");

        

        //Debug.DrawLine(from, from + direction * distance, Color.yellow, 1f);

        RaycastHit[] hit =  Physics.SphereCastAll(from+direction,1.5f, direction, distance);


        foreach(RaycastHit hh in hit)
        {
            
            if (hh.collider.name == "Cube")
            {
                //Debug.DrawLine(from, hh.point, Color.yellow);
                return false;
            }
        }
        //if (h) Debug.DrawLine(from, hit.point, Color.yellow);
        //if(h && hit.collider.name != "Cube") Debug.Log(">> Collider name: " + hit.collider.name);
        //if ((h && hit.collider.name) == "Cube" || (h && Vector3.Distance(hit.point, from) <= 10f) return false;

        return true;
        //Debug.DrawLine(from, hit.point, Color.red, 500f);
        Debug.DrawLine(from, to, Color.yellow);

    }

    public List<Node> Expand(Node node)
    {
        List<Node> children = new List<Node>();
        

        List<Tuple<int, int>> available = new List<Tuple<int, int>>();
        available.Add(new Tuple<int, int>(node.i + 1, node.j));
        available.Add(new Tuple<int, int>(node.i - 1, node.j));
        available.Add(new Tuple<int, int>(node.i, node.j+1));
        available.Add(new Tuple<int, int>(node.i, node.j -1));
        
        available.Add(new Tuple<int, int>(node.i + 1, node.j + 1));
        available.Add(new Tuple<int, int>(node.i + 1, node.j - 1));
        available.Add(new Tuple<int, int>(node.i - 1, node.j + 1));
        available.Add(new Tuple<int, int>(node.i - 1, node.j - 1));
        

        foreach (Tuple<int, int> t in available)
        {
            if (IsValid(t.Item1, t.Item2))
            {
                Vector3 pos = new Vector3(get_x_pos(t.Item1), 0, get_z_pos(t.Item2));
                children.Add(new Node(t.Item1, t.Item2, pos, node, node.priority  + Vector3.Distance(node.pos, pos)));
            }
        }

        return children;
    }

    public int hasht(Tuple<int, int> t)
    {
        return t.Item2 * x_N + t.Item2;
    }

    public List<Node> ExpandWeighted(Node node, Manager manager)
    {
        List<Node> children = new List<Node>();


        List<Tuple<int, int>> available = new List<Tuple<int, int>>();
        available.Add(new Tuple<int, int>(node.i + 1, node.j));
        available.Add(new Tuple<int, int>(node.i - 1, node.j));
        available.Add(new Tuple<int, int>(node.i, node.j + 1));
        available.Add(new Tuple<int, int>(node.i, node.j - 1));
        
        //available.Add(new Tuple<int, int>(node.i + 1, node.j + 1));
        //available.Add(new Tuple<int, int>(node.i + 1, node.j - 1));
        //available.Add(new Tuple<int, int>(node.i - 1, node.j + 1));
        //available.Add(new Tuple<int, int>(node.i - 1, node.j - 1));
        

        foreach (Tuple<int, int> t in available)
        {
            if (IsValid(t.Item1, t.Item2) && wall_distance[t.Item1, t.Item2] >= 5)
            {
                Vector3 pos = new Vector3(get_x_pos(t.Item1), 0, get_z_pos(t.Item2));
                Node child = new Node(t.Item1, t.Item2, pos, node, node.priority);
                Edge edge = edges[new Edge(node, child)];
                child.priority += edge.cost + Vector3.Distance(node.pos, child.pos);
                children.Add(child);
            }
        }

        return children;
    }

    public int NumberOfDronesInProximity(Vector3 pos, Manager manager, float R)
    {
        int res = 0;

        foreach(Drone drone in manager.drone_population.Values)
        {
            if (Vector3.Distance(drone.position, pos) <= R) res++;
        }
        return res;
    }


    public List<Vector3> GetWeightedPath(Vector3 from, Vector3 to, Manager manager)
    {

        List<Vector3> res = new List<Vector3>();
        PriorityQueue<Node> q = new PriorityQueue<Node>();

        HashSet<Node> seen = new HashSet<Node>();

        Node init = new Node(get_i_index(from.x), get_j_index(from.z), from, null);
        Node g = new Node(get_i_index(to.x), get_j_index(to.z), to, null);
        q.Enqueue(init);



        Node closest = new Node(0, 0, new Vector3(), null);
        float min_dist = 1000000;
        while (q.Count != 0)
        {
            Node parent = q.Dequeue();

            if (Vector3.Distance(parent.pos, to) < min_dist)
            {
                closest = parent;
                min_dist = Vector3.Distance(parent.pos, to);
            }
            if (parent.i == g.i && parent.j == g.j)
            {
                while (parent != null)
                {
                    this.seen.Add(hasht(new Tuple<int, int>(get_i_index(parent.pos.x), get_j_index(parent.pos.y))));
                    res.Add(parent.pos);
                    Node next = parent.parent;
                    if(next != null)
                    {
                        edges[new Edge(parent, next)].cost += 10f;
                        edges[new Edge(next, parent)].cost -= 1f;

                    }
                    
                    parent = next;
                    
                    
                }

                res.Reverse();

                return res;
            }

            foreach (Node child in ExpandWeighted(parent, manager))
            {
                if (!seen.Contains(child))
                {
                    q.Enqueue(child);
                    seen.Add(child);
                }
            }

        }

        return res;

    }

    public List<Vector3> GetPath(Vector3 from, Vector3 to, Manager manager)
    {

        List<Vector3> res = new List<Vector3>();
        Queue<Node> q = new Queue<Node>();

        HashSet<Node> seen = new HashSet<Node>();

        Node init = new Node(get_i_index(from.x), get_j_index(from.z), from, null);
        Node g  = new Node(get_i_index(to.x), get_j_index(to.z), to, null);
        q.Enqueue(init);



        Node closest = new Node(0, 0, new Vector3(), null);
        float min_dist = 1000000;
        while (q.Count != 0)
        {
            Node parent = q.Dequeue();

            if (Vector3.Distance(parent.pos, to) < min_dist)
            {
                closest = parent;
                min_dist = Vector3.Distance(parent.pos, to);
            }
            if (parent.i == g.i && parent.j == g.j)
            {
                while (parent != null)
                {
                    res.Add(parent.pos);
                    parent = parent.parent;
                }

                res.Reverse();

                return res;
            }

            foreach(Node child in Expand(parent))
            {
                if (!seen.Contains(child) && NumberOfDronesInProximity(child.pos, manager, 2) <= 1)
                {
                    q.Enqueue(child);
                    seen.Add(child);
                }
            }

        }

        
        while (closest != null)
        {
            res.Add(closest.pos);
            closest = closest.parent;
        }

        res.Reverse();

        return res;
    
    }

    private void CreateResolutionMatrix()
    {
        resolution_matrix = new float[x_N * resolution, z_N * resolution];

        wall_distance = new float[x_N * resolution, z_N * resolution];

        walls = new List<Tuple<int, int>>();

        for (int i = 0; i < x_N; i++)
        {
            for(int j = 0; j < z_N; j++)
            {
                for(int ir = i*resolution; ir < (i+1)*resolution; ir++)
                {
                    for(int jr = j*resolution; jr < (j+1)*resolution; jr++)
                    {
                        resolution_matrix[ir, jr] = terrain.myInfo.traversability[i, j];
                        if (terrain.myInfo.traversability[i, j] > 0.5f)
                        {
                            walls.Add(new Tuple<int, int>(ir, jr));
                        }
                    }
                }
            }
        }

        for (int ir = 0; ir < x_N * resolution; ir++)
        {
            for (int jr = 0; jr < z_N * resolution; jr++)
            {
                if (resolution_matrix[ir, jr] > 0.5f)
                {
                    wall_distance[ir, jr] = 0;
                }
                else
                {
                    float min = 1000000f;
                    Vector2 cell_pos = new Vector2(get_x_pos(ir), get_z_pos(jr));
                    foreach (Tuple<int, int> wall in walls)
                    {
                        Vector2 wall_pos = new Vector2(get_x_pos(wall.Item1), get_z_pos(wall.Item2));

                        min = Math.Min(min, Vector2.Distance(cell_pos, wall_pos));
                    }
                    wall_distance[ir, jr] = min;
                }
            }
        }

    }

    public bool IsValid(int i, int j)
    {
        
        if( 0 <= i && i < x_N * resolution && 0 <= j && j < z_N * resolution)
        {
            return resolution_matrix[i,j] <= 0.5;
        }
        return false;
    }


    public float GetCellXLen()
    {
        return (x_high - x_low) / GetXDim();
    }

    public float GetCellYLen()
    {
        return (z_high - z_low) / GetZDim();
    }

    public int get_i_index(float x)
    {
        int index = (int)Mathf.Floor((x_N*resolution) * (x - x_low) / (x_high - x_low));
        if (index < 0)
        {
            index = 0;
        }
        else if (index > (x_N*resolution) - 1)
        {
            index = (x_N*resolution) - 1;
        }
        return index;

    }

    public int get_j_index(float z) // get index of given coordinate
    {
        int index = (int)Mathf.Floor((z_N*resolution) * (z - z_low) / (z_high - z_low));
        if (index < 0)
        {
            index = 0;
        }
        else if (index > (z_N*resolution) - 1)
        {
            index = (z_N*resolution) - 1;
        }
        return index;
    }

    public float get_x_pos(int i)
    {
        float step = (x_high - x_low) / (x_N*resolution);
        return x_low + step / 2 + step * i;
    }

    public float get_z_pos(int j) // get position of given index
    {
        float step = (z_high - z_low) / (z_N*resolution);
        return z_low + step / 2 + step * j;
    }


}

