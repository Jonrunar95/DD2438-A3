using System;
// Found on https://stackoverflow.com/questions/46383172/disjoint-set-implementation-in-c-sharp

class DisjointSetUBR
{
    int[] parent;
    int[] rank; // height of tree

    public DisjointSetUBR(int N)
    {
        parent = new int[N +1];
        rank = new int[N + 1];
    }

    public void MakeSet(int i)
    {
        parent[i] = i;
    }

    public int Find(int i)
    {
        while (i!=parent[i]) // If i is not root of tree we set i to his parent until we reach root (parent of all parents)
        {
            i = parent[i]; 
        }
        return i;
    }

    // Path compression, O(log*n). For practical values of n, log* n <= 5
    public int FindPath(int i)
    {
        if (i!=parent[i])
        {
            parent[i] = FindPath(parent[i]);
        }
        return parent[i];
    }

    public void Union(int i, int j)
    {
        int i_id = Find(i); // Find the root of first tree (set) and store it in i_id
        int j_id = Find(j); // // Find the root of second tree (set) and store it in j_id

        if (i_id == j_id) // If roots are equal (they have same parents) than they are in same tree (set)
        {
            return;
        }

        if (rank[i_id] > rank[j_id]) // If height of first tree is larger than second tree
        {
            parent[j_id] = i_id; // We hang second tree under first, parent of second tree is same as first tree
        }
        else
        {
            parent[i_id] = j_id; // We hang first tree under second, parent of first tree is same as second tree
            if (rank[i_id] == rank[j_id]) // If heights are same
            {
                rank[j_id]++; // We hang first tree under second, that means height of tree is incremented by one
            }
        }
    }
}