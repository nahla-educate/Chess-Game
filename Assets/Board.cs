using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("Art stuff")]
    [SerializeField] private Material tileMaterial;
   // [SerializeField] private float tileSize = 0.12f;

    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;

    // Awake is called before the application start
    private void Awake()
    {
        GenerateAllTiles(1, TILE_COUNT_X, TILE_COUNT_Y);
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
        if (Physics.Raycast(ray, out info, 100f, LayerMask.GetMask("TileTest")))
        {
            //GEt the indexes of the tile i've hit
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            //if we're hoverring q tile qfter not hovering qny tiles
            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("HoverTest");
            }

            //if we were already hovering a tile, change the previous one
            if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("TileTest");
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("HoverTest");
            } else
        {
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("TileTest");
                currentHover = -Vector2Int.one;
            }
        }

        }
       
    }

            //Generate the board
            // board
            private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
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
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        //ordre obligatoire
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, 0, y * tileSize) ;
        vertices[1] = new Vector3(x * tileSize, 0, (y + 1) * tileSize) ;
        vertices[2] = new Vector3((x + 1) * tileSize, 0, y * tileSize) ;
        vertices[3] = new Vector3((x + 1) * tileSize, 0, (y + 1) * tileSize);


        //order to create the vertices
        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("TileTest");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }


    //operations
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
}
