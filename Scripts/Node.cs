using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Node[] Neighbors;
    public Vector2[] ValidDirections;

    // Start is called before the first frame update
    void Start()
    {
        ValidDirections = new Vector2[Neighbors.Length];
        for(int i = 0; i < Neighbors.Length; i++) 
        {
            Node Neighbor = Neighbors[i];
            Vector2 Direction = Neighbor.transform.localPosition - transform.localPosition; // As all neighbor Nodes are on either the same x or y axis 
            ValidDirections[i] = Direction.normalized; // their difference is equal a vector of (x,0) or (0,y) which when normalized becomes
            // a direction as the numbers are scaled with the biggest magnitude being 1 which is always the x or y which is a direction to that neighbor
        }
    }
}
