using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    public float speed = 5.85f;
    public float FrightenedSpeed = 5.0f;
    public float ConsumedSpeed = 10f;
    public float NormalSpeed = 5.85f;
    public bool CanMove = true;
    public int PinkyReleaseTimer = 5;
    public int InkyReleaseTimer = 14;
    public int ClydeReleaseTimer = 21;
    public float ghostReleaseTimer = 0;
    public bool isInGhostHouse = false;

    public Node StartPosition;
    public Node ScatterNode;
    public Node GhostHouse;

    public int ScatterTimer1 = 5;
    public int ChaseTimer = 20;
    public int ScatterTimer2 = 4;
    public int ScatterTimer3 = 3;
    public int ScatterTimer4 = 2;
    public int FrightenedTimer = 7;
    public int FrightenedBlinkStartTimer = 4;

    private int ModeChangeCount = 1;
    private float ModeChangeTimer = 0;
    private float FrightenedModeChangeTimer = 0;
    private float BlinkChangeTimer = 0;

    public bool isFrightened;
    private bool isBlinking = false;

    public Sprite EyesLeft;
    public Sprite EyesRight;
    public Sprite EyesDown;
    public Sprite EyesUp;

    public RuntimeAnimatorController AnimationLeft;
    public RuntimeAnimatorController AnimationRight;
    public RuntimeAnimatorController AnimationDown;
    public RuntimeAnimatorController AnimationUp;
    public RuntimeAnimatorController AnimationFrightened;
    public RuntimeAnimatorController AnimationBlink;

    private AudioSource BGM;

    public enum Mode 
    {
        Chase, Scatter, Frightened, Consumed
    }
    
    public enum GhostType
    {
        Red, Pink, Orange, Blue
    }

    public GhostType ghostType = GhostType.Red;
    Mode CurrentMode = Mode.Scatter;
    Mode PreviousMode;

    private GameObject Pacman;

    private Node CurrentNode, PreviousNode, TargetNode;
    private Vector2 Direction;

    // Start is called before the first frame update
    void Start()
    {
        BGM = GameObject.Find("GameMaster").transform.GetComponent<AudioSource>();

        Pacman = GameObject.FindGameObjectWithTag("Pacman");

        Node node = GetNode(transform.localPosition);
        if(node != null) 
        {
            CurrentNode = node;
            PreviousNode = CurrentNode;
        }
        else 
        {
            Debug.Log("Decimals");
        }

        if (isInGhostHouse)
        {
            Direction = Vector2.up;
            TargetNode = CurrentNode.Neighbors[0];
        }
        else
        {
            Direction = Vector2.right;
            TargetNode = ChooseNode();
        }

        PreviousNode = CurrentNode;
        UpdateAnimation();
    }


    // Update is called once per frame
    void Update()
    {
        if (CanMove) 
        {
            UpdateMode();
            Move();
            ReleaseGhosts();
            Checkcollision();
            CheckGhostHouse();
        } 
    }


    void Move() 
    {
        if (TargetNode != CurrentNode && TargetNode != null && !isInGhostHouse)
        {
            if (OverShotTarget()) 
            {
                CurrentNode = TargetNode;
                transform.localPosition = CurrentNode.transform.position;
                GameObject portal = GetPortal(CurrentNode.transform.position);
               
                if(portal != null) 
                {
                    transform.localPosition = portal.transform.position;
                    CurrentNode = portal.GetComponent<Node>();
                }
                
                TargetNode = ChooseNode();
                PreviousNode = CurrentNode;
                CurrentNode = null;
                UpdateAnimation();
            }
            else
            {
                transform.localPosition += (Vector3)Direction * speed * Time.deltaTime;
            }
        }
    }

    void UpdateAnimation() 
    {
        if(CurrentMode == Mode.Scatter || CurrentMode == Mode.Chase) 
        {
            if (Direction == Vector2.left)
            {
                transform.GetComponent<Animator>().runtimeAnimatorController = AnimationLeft;
            }
            else if (Direction == Vector2.right)
            {
                transform.GetComponent<Animator>().runtimeAnimatorController = AnimationRight;
            }
            else if (Direction == Vector2.down)
            {
                transform.GetComponent<Animator>().runtimeAnimatorController = AnimationDown;
            }
            else
            {
                transform.GetComponent<Animator>().runtimeAnimatorController = AnimationUp;
            }
        }
        else if(CurrentMode == Mode.Frightened) 
        {
            transform.GetComponent<Animator>().runtimeAnimatorController = AnimationFrightened;
        }
        else 
        {
            transform.GetComponent<Animator>().runtimeAnimatorController = null;
            if (Direction == Vector2.left)
            {
                transform.GetComponent<SpriteRenderer>().sprite = EyesLeft;
            }
            else if (Direction == Vector2.right)
            {
                transform.GetComponent<SpriteRenderer>().sprite = EyesRight;
            }
            else if (Direction == Vector2.down)
            {
                transform.GetComponent<SpriteRenderer>().sprite = EyesDown;
            }
            else if (Direction == Vector2.down)
            {
                transform.GetComponent<SpriteRenderer>().sprite = EyesUp;
            }
        }
        
    }

    void CheckGhostHouse()
    {
        if (CurrentMode == Mode.Consumed)
        {
            GameObject tile = GetTile(transform.position);
            if (tile != null)
            {
                if (tile.transform.GetComponent<Tile>() != null)
                {
                    if (tile.transform.GetComponent<Tile>().isGhostHouse)
                    {
                        speed = NormalSpeed;
                        Node node = GetNode(transform.position);
                        if (node != null)
                        {
                            CurrentNode = node;
                            Direction = Vector2.up;
                            TargetNode = CurrentNode.Neighbors[0];
                            PreviousNode = CurrentNode;
                            CurrentMode = Mode.Chase;
                            UpdateAnimation();
                        }
                    }
                }
            }
        }
    }

    public void Restart()
    {
        BGM.clip = GameObject.Find("GameMaster").transform.GetComponent<Game>().BGM;
        BGM.Play();
        transform.position = StartPosition.transform.position;
        transform.GetComponent<SpriteRenderer>().enabled = true;
        CanMove = true;
        ghostReleaseTimer = 0;
        ModeChangeCount = 1;
        ModeChangeTimer = 0;
        ChangeMode(Mode.Scatter);
        if (transform.name != "Blinky")
        {
            isInGhostHouse = true;
        }
        CurrentNode = StartPosition;

        if (isInGhostHouse)
        {
            Direction = Vector2.up;
            TargetNode = CurrentNode.Neighbors[0];
        }
        else
        {
            Direction = Vector2.right;
            TargetNode = ChooseNode();
        }
        PreviousNode = CurrentNode;
        
        UpdateAnimation();

    }

    void Checkcollision()
    {
        Rect ghostRect = new Rect(transform.position, transform.GetComponent<SpriteRenderer>().sprite.bounds.size / 4);
        Rect pacManRect = new Rect(Pacman.transform.position, Pacman.transform.GetComponent<SpriteRenderer>().sprite.bounds.size / 4);
        if (ghostRect.Overlaps(pacManRect))
        {
            if (CurrentMode == Mode.Frightened)
            {
                consumed();
            }
            else if(CurrentMode != Mode.Consumed)
            {
               GameObject.Find("GameMaster").transform.GetComponent<Game>().StartDeath();
            }
        }

    }

    void ChangeMode(Mode mode) 
    {

        if(mode == Mode.Frightened) 
        {
            isFrightened = true;
            FrightenedModeChangeTimer = 0;
            speed = FrightenedSpeed;
        }
        else if(mode == Mode.Consumed) 
        {
            isFrightened = false;
            speed = ConsumedSpeed;
            GameObject.Find("GameMaster").GetComponent<Game>().StartConsume(this.GetComponent<Ghost>());
        }
        else 
        {
            isFrightened = false;
            speed = NormalSpeed;
        }

        if(CurrentMode != Mode.Frightened) 
        {
            PreviousMode = CurrentMode;
        }

        CurrentMode = mode;
        UpdateAnimation();
    }

    void consumed()
    {
        Game.Score += 200;
        ChangeMode(Mode.Consumed);

    }

    public void Frighten() 
    {
        if (CurrentMode != Mode.Consumed)
        {
            ChangeMode(Mode.Frightened);
            BGM.clip = GameObject.Find("GameMaster").transform.GetComponent<Game>().FrightenedBGM;
            BGM.Play();
        }
    }

    void UpdateMode() 
    {
        if(CurrentMode != Mode.Frightened) 
        {
            ModeChangeTimer += Time.deltaTime;
            
            if(ModeChangeCount == 1) 
            {
                if(CurrentMode == Mode.Scatter && ModeChangeTimer > ScatterTimer1) 
                {
                    ChangeMode(Mode.Chase);
                    ModeChangeTimer = 0;
                }

                if (CurrentMode == Mode.Chase && ModeChangeTimer > ChaseTimer)
                {
                    ModeChangeCount = 2;
                    ChangeMode(Mode.Scatter);
                    ModeChangeTimer = 0;
                }
            }
            else if (ModeChangeCount == 2)
            {
                if (CurrentMode == Mode.Scatter && ModeChangeTimer > ScatterTimer2)
                {
                    ChangeMode(Mode.Chase);
                    ModeChangeTimer = 0;
                }

                if (CurrentMode == Mode.Chase && ModeChangeTimer > ChaseTimer)
                {
                    ModeChangeCount = 3;
                    ChangeMode(Mode.Scatter);
                    ModeChangeTimer = 0;
                }
            }
            else if (ModeChangeCount == 3)
            {
                if (CurrentMode == Mode.Scatter && ModeChangeTimer > ScatterTimer3)
                {
                    ChangeMode(Mode.Chase);
                    ModeChangeTimer = 0;
                }

                if (CurrentMode == Mode.Chase && ModeChangeTimer > ChaseTimer)
                {
                    ModeChangeCount = 4;
                    ChangeMode(Mode.Scatter);
                    ModeChangeTimer = 0;
                }
            }
            else
            {
                if (CurrentMode == Mode.Scatter && ModeChangeTimer > ScatterTimer4)
                {
                    ChangeMode(Mode.Chase);
                    ModeChangeTimer = 0;
                }
            }
        }
        else 
        {
            FrightenedModeChangeTimer += Time.deltaTime;
            if(FrightenedModeChangeTimer >= FrightenedTimer ) 
            {
                BGM.clip = GameObject.Find("GameMaster").transform.GetComponent<Game>().BGM;
                BGM.Play();
                FrightenedModeChangeTimer = 0;
                ChangeMode(PreviousMode);
            }
            if(FrightenedModeChangeTimer >= FrightenedBlinkStartTimer) 
            {
                BlinkChangeTimer += Time.deltaTime;
                if(BlinkChangeTimer >= 0.1f) 
                {
                    BlinkChangeTimer = 0;
                    if (isBlinking) 
                    {
                        transform.GetComponent<Animator>().runtimeAnimatorController = AnimationFrightened;
                        isBlinking = false;
                    }
                    else 
                    {
                        transform.GetComponent<Animator>().runtimeAnimatorController = AnimationBlink;
                        isBlinking = true;
                    }
                }
            }
        }
    }

    Vector2 GetRedGhostTargetTile()
    {
        Vector2 PacmanPosition = Pacman.transform.localPosition; 

        Vector2 targetTile = new Vector2(Mathf.RoundToInt(PacmanPosition.x), Mathf.RoundToInt(PacmanPosition.y)); 

        return targetTile;
    }
    Vector2 GetPinkGhostTargetTile()
    {
        Vector2 PacmanPosition = Pacman.transform.localPosition;

        Vector2 PacmanOrientation = Pacman.GetComponent<Pac>().Orientation;

        int PacmanPositionX = Mathf.RoundToInt(PacmanPosition.x);

        int PacmanPositionY = Mathf.RoundToInt(PacmanPosition.y);

        Vector2 PacmanTile = new Vector2(PacmanPositionX, PacmanPositionY);

        Vector2 targetTile = PacmanTile + (2 * PacmanOrientation);
        
        return targetTile;
    }

    Vector2 GetBlueGhostTargetTile()
    {
        Vector2 TargetTile;
        Vector2 PacmanPosition = Pacman.transform.position;
        if (GetDistance(transform.position,PacmanPosition) < 10) //Make Sure Ghost is in the same half of the map as pacman Give or Take
        {
            Vector2 PacmanOrientation = Pacman.GetComponent<Pac>().Orientation;
            Vector2 PacTile = new Vector2((int)PacmanPosition.x, (int)PacmanPosition.y);
            TargetTile = PacTile + (2 * PacmanOrientation); // Pinky's Target
            Vector2 BlinkyPosition = GameObject.Find("Blinky").transform.position;
            BlinkyPosition = new Vector2((int)BlinkyPosition.x, (int)BlinkyPosition.y);
            float distance = GetDistance(BlinkyPosition, TargetTile);
            distance *= 2;
            TargetTile = new Vector2(BlinkyPosition.x + distance, BlinkyPosition.y + distance);
        }
        else
        {
            
            TargetTile = new Vector2((int)PacmanPosition.x, (int)PacmanPosition.y);
        }
        return TargetTile;
    }

    Vector2 GetOrangeGhostTargetTile()
    {
        int Rand = Random.Range(0, CurrentNode.Neighbors.Length);
        Vector2 TargetTile = CurrentNode.Neighbors[Rand].transform.position;
        return TargetTile;
    }


    Vector2 GetTargetTile()
    {
        Vector2 targetTile = Vector2.zero;
        if (ghostType == GhostType.Red)
        {
            targetTile = GetRedGhostTargetTile();
        }
        else if (ghostType == GhostType.Pink)
        {
            targetTile = GetPinkGhostTargetTile();
        }
        else if(ghostType == GhostType.Blue) 
        {
            targetTile = GetBlueGhostTargetTile();
        }
        else if(ghostType == GhostType.Orange) 
        {
            targetTile = GetOrangeGhostTargetTile();
        }
        return targetTile;
    }

    void ReleasePinkGhost()
    {
        if (ghostType == GhostType.Pink && isInGhostHouse)
        {
            isInGhostHouse = false;
        }
    }

    void ReleaseBlueGhost()
    {
        if (ghostType == GhostType.Blue && isInGhostHouse)
        {
            isInGhostHouse = false;
        }
    }

    void ReleaseOrangeGhost()
    {
        if (ghostType == GhostType.Orange && isInGhostHouse)
        {
            isInGhostHouse = false;
        }
    }

    void ReleaseGhosts()
    {
        ghostReleaseTimer += Time.deltaTime;

        if (ghostReleaseTimer > PinkyReleaseTimer)
        {
            ReleasePinkGhost();
        }
        if (ghostReleaseTimer > InkyReleaseTimer)
        {
            ReleaseBlueGhost();
        }
        if (ghostReleaseTimer > ClydeReleaseTimer)
        {
            ReleaseOrangeGhost();
        }

    }

    Node ChooseNode()
    {
        Vector2 TargetTile = Vector2.zero;
        if (CurrentMode == Mode.Chase)
        {
            TargetTile = GetTargetTile();
        }
        else if (CurrentMode == Mode.Scatter)
        {
            TargetTile = ScatterNode.transform.position;
        }
        else if(CurrentMode == Mode.Frightened)
        {
            TargetTile = GetOrangeGhostTargetTile(); //Orange already move Randomly so we use his function when frightened
        }
        else 
        {
            TargetTile = GhostHouse.transform.position;
        }


        Node MoveTo = null;
        Node[] foundNodes = new Node[4];
        Vector2[] Directions = new Vector2[4];

        int NodeCounter = 0;

        for (int i = 0; i < CurrentNode.Neighbors.Length; i++)
        {
            if (CurrentNode.ValidDirections[i] != Direction * -1)
            {
                if (CurrentMode != Mode.Consumed)
                {
                    GameObject tile = GetTile(CurrentNode.transform.position);
                    if (tile.transform.GetComponent<Tile>().isEntrance == true)
                    {
                        if (CurrentNode.ValidDirections[i] != Vector2.down)
                        {
                            foundNodes[NodeCounter] = CurrentNode.Neighbors[i];

                            Directions[NodeCounter] = CurrentNode.ValidDirections[i];

                            NodeCounter++;
                        }
                    }
                    else
                    {
                        foundNodes[NodeCounter] = CurrentNode.Neighbors[i];

                        Directions[NodeCounter] = CurrentNode.ValidDirections[i];

                        NodeCounter++;
                    }
                }
                else
                {
                    foundNodes[NodeCounter] = CurrentNode.Neighbors[i];

                    Directions[NodeCounter] = CurrentNode.ValidDirections[i];

                    NodeCounter++;
                }
                /*foundNodes[NodeCounter] = CurrentNode.Neighbors[i];

                Directions[NodeCounter] = CurrentNode.ValidDirections[i];

                NodeCounter++;*/
            }
        }

        float MinDistance = 0;

        for (int i = 0; i < foundNodes.Length; i++)
        {
            if (CurrentMode != Mode.Consumed)
                {
                    GameObject tile = GetTile(CurrentNode.transform.position);
                    if (tile.transform.GetComponent<Tile>().isGhostHouse == true)
                    {
                        if (CurrentNode.ValidDirections[i] != Vector2.down)
                        {
                            if (Directions[i] != Vector2.zero)
                            {
                                float Distance = GetDistance(foundNodes[i].transform.position, TargetTile);
                                if (Distance < MinDistance || MinDistance == 0)
                                {
                                    MinDistance = Distance;

                                    MoveTo = foundNodes[i];

                                    Direction = Directions[i];
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Directions[i] != Vector2.zero)
                        {
                            float Distance = GetDistance(foundNodes[i].transform.position, TargetTile);
                            if (Distance < MinDistance || MinDistance == 0)
                            {
                                MinDistance = Distance;

                                MoveTo = foundNodes[i];

                                Direction = Directions[i];
                            }
                        }
                    }
                }
                else
                {
                    if (Directions[i] != Vector2.zero)
                    {
                        float Distance = GetDistance(foundNodes[i].transform.position, TargetTile);
                        if (Distance < MinDistance || MinDistance == 0)
                        {
                            MinDistance = Distance;

                            MoveTo = foundNodes[i];

                            Direction = Directions[i];
                        }
                    }
                }
            /*if (Directions[i] != Vector2.zero)
            {
                float Distance = GetDistance(foundNodes[i].transform.position, TargetTile);
                if (Distance < MinDistance || MinDistance == 0)
                {
                    MinDistance = Distance;

                    MoveTo = foundNodes[i];

                    Direction = Directions[i];
                }
            }*/
        }
        return MoveTo;
    }

    Node ChooseNodeRedGhost() 
    {
        Vector2 TargetTile = Vector2.zero;
        if(CurrentMode == Mode.Chase) 
        {
            TargetTile = GetTargetTile();
        }
        else if(CurrentMode == Mode.Scatter) 
        {
            TargetTile = ScatterNode.transform.position;
        }
        else 
        {
            TargetTile = GetOrangeGhostTargetTile(); //Random Tile in neighbors;
        }
        
       
        Node MoveTo = null;
        Node[] foundNodes = new Node[4];
        Vector2[] Directions = new Vector2[4];
        int NodeCounter = 0;

        for(int i = 0; i < CurrentNode.Neighbors.Length; i++) 
        {
            if(CurrentNode.ValidDirections[i] != Direction * -1) 
            {
                foundNodes[NodeCounter] = CurrentNode.Neighbors[i];

                Directions[NodeCounter] = CurrentNode.ValidDirections[i];

                NodeCounter++;
            }
        }
        
        float MinDistance = 999999999f;

        for(int i = 0; i < foundNodes.Length; i++) 
        {
            
            if(Directions[i] != Vector2.zero) 
            {
                
                float MinDistance2 = 99999999f;
                for (int j = 0; j < foundNodes[i].Neighbors.Length; j++)
                {
                    if (foundNodes[i].ValidDirections[j] != Directions[i] * -1)
                    {
                        float DistanceStep2 = GetDistance(foundNodes[i].Neighbors[j].transform.position, TargetTile);
                        if (DistanceStep2 < MinDistance2)
                        {
                            MinDistance2 = DistanceStep2;
                        }
                    }
                }


                float Distance = GetDistance(foundNodes[i].transform.position, TargetTile);
                float GhostDistance = GetDistance(transform.position, TargetTile);
                float NeighborDistance = GetDistance(transform.position, foundNodes[i].transform.position);
                if ( ( Distance < MinDistance  && ( Distance == 0 || MinDistance2 < Distance) ) || GhostDistance <= NeighborDistance) 
                {
                    MinDistance = Distance;
                    
                    MoveTo = foundNodes[i];      
                    Direction = Directions[i];
                }
               
            }
        }
        if(MoveTo == null) 
        {
            MoveTo = ChooseNode();
        }
        return MoveTo;
    }

    GameObject GetTile(Vector2 position)
    {
        GameObject tile = GameObject.Find("GameMaster").GetComponent<Game>().Map[(int)position.x, (int)position.y];
        if (tile != null)
        {
            return tile;
        }
        return null;
    }

    Node GetNode(Vector2 position)
    {
        GameObject node = GameObject.Find("GameMaster").GetComponent<Game>().Map[(int)position.x, (int)position.y];

        if (node != null)
        {
            return node.GetComponent<Node>();
        }

        return null;
    }

    GameObject GetPortal(Vector2 position)
    {
        GameObject tile = GameObject.Find("GameMaster").GetComponent<Game>().Map[(int)position.x, (int)position.y];
        if (tile != null)
        {
            if (tile.GetComponent<Tile>() != null)
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

    float LengthFromNode(Vector2 TargetPosition)
    {
        Vector2 Length = TargetPosition - (Vector2)PreviousNode.transform.position;
        return Length.sqrMagnitude;
    }

    bool OverShotTarget()
    {
        float NodeToTarget = LengthFromNode(TargetNode.transform.position);
        float NodeToSelf = LengthFromNode(transform.localPosition);
        return NodeToSelf > NodeToTarget;
    }

    float GetDistance(Vector2 A, Vector2 B) 
    {
        float Distance = Mathf.Sqrt( ((A.x - B.x)*(A.x - B.x)) + ((A.y - B.y)*(A.y - B.y)) );
        return Distance;
    }
}
