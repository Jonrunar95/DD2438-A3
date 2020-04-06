using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class Node
{
    public int i, j;
    public Vector3 pos;
    public Node parent;

    public Node(int i, int j, Vector3 pos, Node parent)
    {
        this.i = i;
        this.j = j;
        this.pos = pos;
        this.parent = parent;
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
public class ResolutionTerrain
{
    TerrainManager terrain;
    private int resolution, x_N, z_N;
    public float x_high, x_low, z_high, z_low; 
    public float[,] resolution_matrix;

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

        foreach(Tuple<int, int> t in available)
        {
            if (IsValid(t.Item1, t.Item2))
            {
                children.Add(new Node(t.Item1, t.Item2, new Vector3(get_x_pos(t.Item1), 0, get_z_pos(t.Item2)), node));
            }
        }

        return children;
    }

    public List<Vector3> GetPath(Vector3 from, Vector3 to)
    {

        List<Vector3> res = new List<Vector3>();
        Queue<Node> q = new Queue<Node>();

        HashSet<Node> seen = new HashSet<Node>();

        Node init = new Node(get_i_index(from.x), get_j_index(from.z), from, null);
        Node g  = new Node(get_i_index(to.x), get_j_index(to.z), to, null);
        q.Enqueue(init);
        



        while (q.Count != 0)
        {
            Node parent = q.Dequeue();

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
                if (!seen.Contains(child))
                {
                    q.Enqueue(child);
                    seen.Add(child);
                }
            }

        }

        return res;
    
    }

    private void CreateResolutionMatrix()
    {
        resolution_matrix = new float[x_N * resolution, z_N * resolution];

        for(int i = 0; i < x_N; i++)
        {
            for(int j = 0; j < z_N; j++)
            {
                for(int ir = i*resolution; ir < (i+1)*resolution; ir++)
                {
                    for(int jr = j*resolution; jr < (j+1)*resolution; jr++)
                    {
                        resolution_matrix[ir, jr] = terrain.myInfo.traversability[i, j];
                    }
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

