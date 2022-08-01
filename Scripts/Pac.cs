using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pac : MonoBehaviour
{
    public float speed = 6.0f;
    public Vector2 Orientation;
    public bool CanMove = true;
    private Vector2 Direction = Vector2.zero, NextDirection;

    public int PelletsConsumed = 0;

    public AudioClip Chomp1;
    public AudioClip Chomp2;
    private bool PlayedChomp = false;
    private AudioSource Audio;

    private Node CurrentNode, PreviousNode, TargetNode;

    public Sprite idle;
    public RuntimeAnimatorController Eat;
    public RuntimeAnimatorController Death;

    private Node startingPosition;

    // Start is called before the first frame update
    void Start()
    {
        Audio = transform.GetComponent<AudioSource>();

        Node node = GetNode(transform.localPosition);

        startingPosition = node;

        if (node != null)
        {
            CurrentNode = node;
            Debug.Log(CurrentNode);
        }
        else
            Debug.Log("Decimal Fun Time");

        Direction = Vector2.right;
        Orientation = Vector2.right;
        ChangePosition(Direction);
    }


    // Update is called once per frame
    void Update()
    {
        if (CanMove) 
        {
            CheckInput();
            Move();
            FaceDirection();
            CheckAnimation();
            ConsumePellet();
        }
       // Debug.Log("Score is: " + GameObject.Find("GameMaster").GetComponent<Game>().Score);
    }

    void Chomp() 
    {
        if (PlayedChomp)
        {
            Audio.PlayOneShot(Chomp2);
            PlayedChomp = false;
        }
        else 
        {
            Audio.PlayOneShot(Chomp1);
            PlayedChomp = true;
        }
    }

    void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) 
        {     
            ChangePosition(Vector2.left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow)) 
        {
            ChangePosition(Vector2.right);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ChangePosition(Vector2.up);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ChangePosition(Vector2.down);
        }
    }

    void ChangePosition(Vector2 direction) 
    {
        if(direction != Direction) 
        {
            NextDirection = direction;// If the player inputs a direction other than the one he's moving to, save it in NextDirection
        }

        if(CurrentNode != null) 
        {
            Node MoveTo = ValidMove(direction);
            
            if(MoveTo != null) 
            {
                Direction = direction;
                TargetNode = MoveTo;
                PreviousNode = CurrentNode;
                CurrentNode = null;
            }
        }
    }

    void Move() 
    {
        if(TargetNode != CurrentNode && TargetNode != null) 
        {
            if(NextDirection == Direction * -1) 
            {
                Direction *= -1;
                Node temp = TargetNode;
                TargetNode = PreviousNode;
                PreviousNode = temp;
            }

            if (OverShotTarget()) 
            {
                CurrentNode = TargetNode;
                transform.localPosition = CurrentNode.transform.position;
                GameObject portal = GetPortal(CurrentNode.transform.position);
                if(portal != null) 
                {
                    transform.position = portal.transform.position;
                    CurrentNode = portal.GetComponent<Node>();
                }

                Node MoveTo = ValidMove(NextDirection);
                
                if(MoveTo != null) 
                {
                    Direction = NextDirection;
                }
                if(MoveTo == null)
                {
                    MoveTo = ValidMove(Direction); 
                }

                if(MoveTo != null) // check again after the last if as MoveTo could have been changed by entering else
                {
                    TargetNode = MoveTo;
                    PreviousNode = CurrentNode;
                    CurrentNode = null;
                }
                else 
                {
                    Direction = Vector2.zero;
                }
            }
            else 
            {
                transform.localPosition += (Vector3)Direction * speed * Time.deltaTime;
            }
        }
    }

    void FaceDirection() //Flips the scale and rotation of Pacman to look towards the direction they're moving to
    {
        if(Direction == Vector2.left) 
        {
            Orientation = Vector2.left;
            transform.localScale = new Vector3(-1,1,1);
            transform.localRotation = Quaternion.Euler(0,0,0);
        }
        else if(Direction == Vector2.right) 
        {
            Orientation = Vector2.right;
            transform.localScale = new Vector3(1, 1, 1);
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        else if (Direction == Vector2.up)
        {
            Orientation = Vector2.up;
            transform.localScale = new Vector3(1, 1, 1);
            transform.localRotation = Quaternion.Euler(0,0,90);
        }
        else if (Direction == Vector2.down)
        {
            Orientation = Vector2.down;
            transform.localScale = new Vector3(1, 1, 1);
            transform.localRotation = Quaternion.Euler(0, 0, 270);
        }
    }

    void CheckAnimation() 
    {
        if(Direction == Vector2.zero) 
        {
            GetComponent<Animator>().enabled = false;
            GetComponent<SpriteRenderer>().sprite = idle;
        }
        else 
        {
            GetComponent<Animator>().enabled = true;
        }
    }

    public void Restart()
    {
        transform.position = startingPosition.transform.position;
        CurrentNode = startingPosition;
        CanMove = true;
        transform.GetComponent<Animator>().runtimeAnimatorController = Eat;
        transform.GetComponent<Animator>().enabled = true;
        transform.GetComponent<SpriteRenderer>().enabled = true;
        Direction = Vector2.right;
        Orientation = Vector2.right;
        NextDirection = Vector2.right;
        ChangePosition(Direction);
    }

    Node ValidMove(Vector2 direction) // Checks if the direction is a valid move and returns the node to move to in that case
    {
        for(int i = 0; i < CurrentNode.Neighbors.Length; i++) 
        {
            GameObject Tile = GetTile(CurrentNode.Neighbors[i].transform.position);

            if (CurrentNode.ValidDirections[i] == direction && !Tile.transform.GetComponent<Tile>().isGhostHouse) 
            {
                return CurrentNode.Neighbors[i];
            }
        }
        return null;
    }

    GameObject GetTile(Vector2 position) 
    {
        GameObject tile = GameObject.Find("GameMaster").GetComponent<Game>().Map[(int)position.x, (int)position.y];
        if(tile != null) 
        {
            return tile;
        }
        return null;
    }

    void ConsumePellet() 
    {
        GameObject Object = GetTile(transform.position);
        Tile tile = Object.GetComponent<Tile>();
        if(tile != null) 
        {
            
            if(!tile.isConsumed && (tile.isPellet || tile.isEnergizedPellet)) 
            {
                Chomp();
                Object.GetComponent<SpriteRenderer>().enabled = false;
                tile.isConsumed = true;
                Game.Score += 10;
                PelletsConsumed++;
                if (tile.isEnergizedPellet) 
                {
                    foreach (GameObject Ghost in GameObject.FindGameObjectsWithTag("Ghost"))
                    {
                       if (!Ghost.GetComponent<Ghost>().isInGhostHouse) //Frightened BGM Issues on consuming all ghosts before timer ends
                        {
                            Ghost.GetComponent<Ghost>().Frighten();
                        }
                    }
                }
            }
        }
    }

    Node GetNode(Vector2 position) 
    {
        GameObject node = GameObject.Find("GameMaster").GetComponent<Game>().Map[(int)position.x, (int)position.y];
        
        if(node != null) 
        {
            return node.GetComponent<Node>();
        }
        
        return null;
    }

    bool OverShotTarget() 
    {
        float NodeToTarget = LengthFromNode(TargetNode.transform.position);
        float NodeToSelf = LengthFromNode(transform.localPosition);
        return NodeToSelf > NodeToTarget;
    }

    float LengthFromNode(Vector2 TargetPosition) 
    {
        Vector2 Length = TargetPosition - (Vector2)PreviousNode.transform.position;
        return Length.sqrMagnitude;
    }

    GameObject GetPortal(Vector2 position) 
    {
        GameObject tile = GameObject.Find("GameMaster").GetComponent<Game>().Map[(int)position.x, (int)position.y];
        if(tile != null) 
        {
            if(tile.GetComponent<Tile>() != null) 
            {
                if (tile.GetComponent<Tile>().isPortal)
                {
                    tile = tile.GetComponent<Tile>().Portal;
                    return tile;
                }
            }
        }
        
        return null;
    }
}
