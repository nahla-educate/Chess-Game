using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;

using System;
using UnityEngine;

public class ChessBoard : MonoBehaviour
{
    [Header("Art stuff")]
    [SerializeField] private Material titleMaterial;
    [SerializeField] private float tileSize = 0.12f;
    [SerializeField] private float yOffset = 0.4f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private Transform rematchIndicator;


    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;
    [SerializeField] private float deathSize = 0.1f;
    [SerializeField] private float dragOffset = 1.5f;
    [SerializeField] private float deathSpacing = 0.6f;

    //LOGIC

    private ChessPiece[,] chessPieces;



    private ChessPiece currentlyDragging;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();

    private Vector3 initialPiecePosition;


    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;

    private List<ChessPiece> deadWhites = new List<ChessPiece>();
    private List<ChessPiece> deadBlacks = new List<ChessPiece>();

    //multi logic
    private int playerCount = -1;
    private int currentTeam = -1;

    private bool localGame = true;
    private bool[] playerRematch = new bool[2];

    private bool isWhiteTurn = true;

    // Awake is called before the application start
    private void Start()
    {
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnAllPieces();
        PositionAllPieces();

        RegisterAvents();
    }


    //update
    private void Update()
    {

        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }
        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100f, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
            //GEt the indexes of the tile i've hit
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            //if we're hoverring q tile qfter not hovering qny tiles
            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            //if we were already hovering a tile, change the previous one
            if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = (ConstainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");


            }

            // if we press down on the mouse
            if (Input.GetMouseButtonDown(0))
            {
                //just our pieces can drag
                if ((chessPieces[hitPosition.x, hitPosition.y].team == 0 && isWhiteTurn && currentTeam == 0) || (chessPieces[hitPosition.x, hitPosition.y].team == 1 && isWhiteTurn && currentTeam == 1))
                {
                    // Is it our turn
                    if (true)
                    {
                        currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];
                        // initialPiecePosition = currentlyDragging.transform.position;

                        //get a list of where i can go , hightlight tiles as well
                        availableMoves = currentlyDragging.GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
                        HighlightTiles();
                    }
                }
            }

            // if we are releasing the mouse button
            if (currentlyDragging != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);

                if (ConstainsValidMove(ref availableMoves, new Vector2(hitPosition.x, hitPosition.y)))
                {
                    MoveTo(previousPosition.x, previousPosition.y, hitPosition.x, hitPosition.y);
                    //Net Implementation

                    NetMakeMove mm = new NetMakeMove();
                    mm.originalX = previousPosition.x;
                    mm.originalY = previousPosition.y;
                    mm.destinationX = hitPosition.x;
                    mm.destinationY = hitPosition.y;
                    mm.teamId = currentTeam;
                    Client.Instance.SendToServer(mm);
                }
                else
                {
                    currentlyDragging.transform.position = GetTileCenter(previousPosition.x, previousPosition.y);
                    currentlyDragging = null;
                    RemoveHighLightTiles();

                }



                /*  //bool validMove = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y);
                  if (!validMove)
                  {

                  }
                 currentlyDragging = null;*/

            }

            // if we are currently dragging a piece
            if (currentlyDragging != null)
            {

                Vector3 mousePosition = currentCamera.ScreenToWorldPoint(Input.mousePosition);
                Vector3 dragPosition = new Vector3(mousePosition.x, yOffset, mousePosition.z);

                currentlyDragging.transform.position = dragPosition;
                /* Plane plane = new Plane(Vector3.up, Vector3.zero);
            float distance;
            
            if (plane.Raycast(ray, out distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                Vector3 dragPosition = new Vector3(hitPoint.x, yOffset, hitPoint.z);
                
                currentlyDragging.transform.position = dragPosition;
            }*/
            }



        }
        else
        {
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = (ConstainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;

            }

            if (currentlyDragging && Input.GetMouseButtonUp(0))
            {
                currentlyDragging.transform.position = GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY);

                currentlyDragging = null;
                RemoveHighLightTiles();
            }
        }
        if (currentlyDragging)
        {
            Plane plane = new Plane(Vector3.up, Vector3.up * yOffset);
            float distance = 0.0f;
            if (plane.Raycast(ray, out distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                Vector3 dragPosition = new Vector3(hitPoint.x, yOffset, hitPoint.z);

                currentlyDragging.transform.position = dragPosition;

            }
        }




    }


    //Generate the board
    // board
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
            }
        }
    }

    //square of the board 
    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {



        //tous les couples possibles(x,y); x=[0,7] y[0,7]
        GameObject tileObject = new GameObject(string.Format("X={0}, Y={1}", x, y));
        tileObject.transform.parent = transform;

        //mesh allows me to create (triangles) vertices
        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = titleMaterial;

        //ordre obligatoire
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - bounds;


        //order to create the vertices
        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();
        return tileObject;
    }

    // Spawning of the pieces
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];
        int whiteTeam = 1, blackTeam = 0;

        //white team

        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);

        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);


        }

        //black team

        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.King, blackTeam);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.Queen, blackTeam);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);

        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);

        }
    }
    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
    {
        ChessPiece cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<ChessPiece>();

        cp.type = type;
        cp.team = team;
        cp.GetComponent<MeshRenderer>().material = teamMaterials[team];

        return cp;
    }
    //modifier
    //Positions of pieces
    public void PositionAllPieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    PositionSinglePiece(x, y, true); // Change true to false here
                }
            }
        }
    }


    public void PositionSinglePiece(int x, int y, bool force = false)
    {
        if (y == 1)
        {
            chessPieces[x, y].currentX = x;
            chessPieces[x, y].currentY = y;
            chessPieces[x, y].transform.position = GetTileCenter(x, y);

        }
        else
        {
            chessPieces[x, y].currentX = x;
            chessPieces[x, y].currentY = y;
            chessPieces[x, y].transform.position = GetTileCenter(x, y);
        }

    }

    private Vector3 GetTileCenter(int x, int y)
    {
        if (y == 1 || y == 6)
        {
            return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0.2f, tileSize / 2);

        }
        else
        {
            return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
        }
    }

    //highlight Tile
    private void HighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
        }
    }
    private void RemoveHighLightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }
        availableMoves.Clear();
    }
    private void GameReset()
    {
        //UI
        rematchIndicator.transform.GetChild(0).gameObject.SetActive(false);
        rematchIndicator.transform.GetChild(1).gameObject.SetActive(false);

        //Fields reset
        currentlyDragging = null;
        availableMoves.Clear();
        playerRematch[0] = playerRematch[1] = false;
    }

    public void OnRematchButton()
    {
        if (localGame)
        {
            NetRematch wrm = new NetRematch();
            wrm.teamId = 0;
            wrm.wantRematch = 1;
            Client.Instance.SendToServer(wrm);

            NetRematch brm = new NetRematch();
            brm.teamId = 1;
            brm.wantRematch = 1;
            Client.Instance.SendToServer(brm);

        }
        else
        {
            NetRematch rm = new NetRematch();
            rm.teamId = currentTeam;
            rm.wantRematch = 1;
            Client.Instance.SendToServer(rm);

        }

    }
    public void OnMenuButton()
    {
        NetRematch rm = new NetRematch();
        rm.teamId = currentTeam;
        rm.wantRematch = 0;
        Client.Instance.SendToServer(rm);

        // GameReset();
        GameUI.Instance.OnLeaveFromGameMenu();
        Client.Instance.Shutdown();
        Server.Instance.Shutdown();

        //reset some values
        playerCount = -1;
        currentTeam = -1;
    }

    //operations
    private bool ConstainsValidMove(ref List<Vector2Int> moves, Vector2 pos)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            if (moves[i].x == pos.x && moves[i].y == pos.y)
            {
                return true;
            }
        }
        return false;
    }

    private void MoveTo(int originalX, int originalY, int x, int y)
    {
        ChessPiece cp = chessPieces[originalX, originalY];
        Vector2Int previousPosition = new Vector2Int(originalX, originalY);

        // Check if there is another piece on the target position
        if (chessPieces[x, y] != null)
        {
            /* if(!ConstainsValidMove(ref availableMoves, new Vector2(x, y))){
                 return false;
             }*/

            ChessPiece ocp = chessPieces[x, y];
            if (cp.team == ocp.team)
            {
                return; // Same team, invalid move
            }

            // Remove the captured piece from the chessboard
            chessPieces[x, y] = null;

            // If it's the enemy team, add the captured piece to the dead list
            if (ocp.team == 0)
            {
                deadWhites.Add(ocp);
                ocp.transform.position = new Vector3(8 * tileSize, yOffset, -1 * tileSize)
                            - bounds
                            + new Vector3(tileSize / 2, 0, tileSize / 2)
                            + (Vector3.forward * deathSpacing) * deadWhites.Count;

            }
            else
            {
                deadBlacks.Add(ocp);
                ocp.transform.position = new Vector3(-1 * tileSize, yOffset, 8 * tileSize)
                           - bounds
                           + new Vector3(tileSize / 2, 0, tileSize / 2)
                           + (Vector3.back * deathSpacing) * deadBlacks.Count;
            }
        }//back

        // Move the current piece to the target position
        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y);
        if (localGame)
        {
            //currentTeam = (currentTeam == 0) ? 1 : 0;
            if (currentTeam == 0)
            {
                currentTeam = 1;
            }
            else
            {
                currentTeam = 0;
            }

        }
        /*if (currentlyDragging) { currentlyDragging = null;}
        
        RemoveHighLightTiles();*/
        return;
    }

    /*ivate bool MoveTo(ChessPiece cp, int x, int y)
    {
        Vector2Int previousPosition = new Vector2Int(cp.currentX, cp.currentY);

        //is there another piece on the target position
        if (chessPieces[x, y] != null)
        {
            ChessPiece ocp = chessPieces[x, y];
            if (cp.team == ocp.team)
            {
                return false;
            }

            //if its the enemy team
            if (ocp.team == 0)
            {
                deadWhites.Add(ocp);
                ocp.SetScale(Vector3.one * deathSize);
                ocp.SetPosition(new Vector3(8 * tileSize, yOffset, -1 * tileSize) 
                    - bounds 
                    + new Vector3(tileSize / 2, 0, tileSize / 2)
                    + (Vector3.forward * deathSpacing) * deadWhites.Count);
                //back
            }
            else
            {
                deadBlacks.Add(ocp);
             ocp.SetScale(Vector3.one * deathSize);
            }

        }
        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y);
        return true;
    }*/

    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)

            for (int y = 0; y < TILE_COUNT_Y; y++)

                if (tiles[x, y] == hitInfo)
                {
                    return new Vector2Int(x, y);
                }


        return -Vector2Int.one; //invalid


    }

    #region
    private void RegisterAvents()
    {
        NetUtility.S_WELCOME += OnWelcomeServer;
        NetUtility.S_MAKE_MOVE += OnMakeMoveServer;
        NetUtility.S_REMATCH += OnRematchServer;

        NetUtility.C_WELCOME += OnWelcomeClient;
        NetUtility.C_START_GAME += OnStartGameClient;
        NetUtility.C_MAKE_MOVE += OnMakeMoveClient;
        NetUtility.C_REMATCH += OnRematchClient;

        GameUI.Instance.SetLocalGame += OnSetLocalGame;
    }



    private void UnregisterEvents()
    {
        NetUtility.S_WELCOME -= OnWelcomeServer;
        NetUtility.S_MAKE_MOVE -= OnMakeMoveServer;
        NetUtility.S_REMATCH -= OnRematchServer;

        NetUtility.C_WELCOME -= OnWelcomeClient;
        NetUtility.C_START_GAME -= OnStartGameClient;
        NetUtility.C_MAKE_MOVE -= OnMakeMoveClient;
        NetUtility.C_REMATCH -= OnRematchClient;

        GameUI.Instance.SetLocalGame -= OnSetLocalGame;

    }
    //server
    private void OnWelcomeServer(NetMessage msg, NetworkConnection cnn)
    {
        //Client has connected, assign a team and return msg
        NetWelcome nw = msg as NetWelcome;

        //assign a team
        nw.AssignedTeam = ++playerCount;

        //Return back to the client
        Server.Instance.SendToClient(cnn, nw);

        if (playerCount == 1)
        {
            Server.Instance.Broadcast(new NetStartGame());
        }
    }

    private void OnMakeMoveServer(NetMessage msg, NetworkConnection cnn)
    {
        //receive the msg, broadcast it back
        NetMakeMove mm = msg as NetMakeMove;
        //Receive and broadcast is back
        Server.Instance.Broadcast(mm);
    }
    private void OnRematchServer(NetMessage msg, NetworkConnection cnn)
    {

        Server.Instance.Broadcast(msg);
    }
    //client
    private void OnWelcomeClient(NetMessage msg)
    {
        //Client has connected, assign a team and return msg
        NetWelcome nw = msg as NetWelcome;

        //assign a team
        currentTeam = nw.AssignedTeam;

        Debug.Log($"My assigned team is {nw.AssignedTeam}");
        if (localGame && currentTeam == 0)
        {
            Server.Instance.Broadcast(new NetStartGame());
        }
    }
    private void OnStartGameClient(NetMessage msg)
    {
        if (currentTeam == 1)
        {
            Debug.Log("turn");
            GameUI.Instance.ChangeCamera(CameraAngle.whiteTeam);
            Debug.Log("turnyes");
        }
        else if (currentTeam == 0)
        {
            GameUI.Instance.ChangeCamera(CameraAngle.blackTeam);
        }
       // GameUI.Instance.menuAnimator.SetTrigger("InGameMenu");
    }
    private void OnMakeMoveClient(NetMessage msg)
    {
        NetMakeMove mm = msg as NetMakeMove;

        Debug.Log($"MM : {mm.teamId} : {mm.originalX} {mm.originalY} -> {mm.destinationX} {mm.destinationY}");

        if (mm.teamId != currentTeam)
        {
            ChessPiece target = chessPieces[mm.originalX, mm.originalY];

            availableMoves = target.GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
            MoveTo(mm.originalX, mm.originalY, mm.destinationX, mm.destinationY);
        }
        //GameUI.Instance.ChangeCamera((currentTeam == 0) ? CameraAngle.whiteTeam : CameraAngle.blackTeam);
    }

    private void OnRematchClient(NetMessage msg)
    {
        NetRematch rm = msg as NetRematch;
        //set the boolean for the rematch
        playerRematch[rm.teamId] = rm.wantRematch == 1;

        //active the piece of ui
        if (rm.teamId != currentTeam)
        {
            rematchIndicator.transform.GetChild((rm.wantRematch == 1) ? 0 : 1).gameObject.SetActive(true);
        }
        //if both want to rematch
        if (playerRematch[0] && playerRematch[1])
        {
            //GameReset();
        }
    }

    private void OnSetLocalGame(bool v)
    {
        playerCount = -1;
        currentTeam = -1;
        localGame = v;
    }
    #endregion
}