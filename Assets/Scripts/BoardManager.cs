using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    // Script for the Board Manager to initiate the game board

    private static BoardManager instance = null; // Implementing Singleton pattern

    private Tile[,] tiles; 				// For future use maybe?
    private GameObject[,] allChars; 	// All character objects in the board
    private int[,] allCharsIndex; 		// Indexes of the characters in all tiles

    public int width; 					// Board width
    public int height; 					// Board height
    public GameObject tilePrefab; 		// Empty tile prefabs
    public List<GameObject> characters = new List<GameObject>(); 	// List of character prefabs


    // Singleton method
    public static BoardManager getInstance()
    {
        if (instance == null)
            instance = new BoardManager();

        return instance;
    }

    // Start is called before the first frame update 
    void Start()
    { 
        tiles = new Tile[width, height]; // No idea what's that for, might be used later
        allChars = new GameObject[width, height];
        allCharsIndex = new int[width, height];
        SetUp();
    }

    // Create the board using a 2D array storing all the tiles stating at (0,0)
    private void SetUp()
    {
        // 2 loops to create the game board from (0,0)
        // Camera position should be modified to make sure
        // the game board is at the center of the screen
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // Create an empty tile at current position
                Vector2 tempPos = new Vector2(i, j);
                GameObject emptyTile = Instantiate(tilePrefab, tempPos, Quaternion.identity) as GameObject;
                emptyTile.transform.parent = this.transform;
                emptyTile.name = "(" + i + "," + j + ")";

                // Preparing for possible characters to allocate
                List<int> possibleIndexes = new List<int>();
                for (int k = 0; k < characters.Count; k++) possibleIndexes.Add(k);

                if (i >= 2 && allCharsIndex[i - 2,j] == allCharsIndex[i - 1,j])
                    possibleIndexes.Remove(allCharsIndex[i - 2,j]);
                if (j >= 2 && allCharsIndex[i,j - 2] == allCharsIndex[i,j - 1])
                    possibleIndexes.Remove(allCharsIndex[i,j - 2]);

                // Randomly pick a character from the character set
                int index = possibleIndexes[Random.Range(0, possibleIndexes.Count)];
                allCharsIndex[i, j] = index;

                // Allocation of the picked character to the empty tile
                GameObject character = Instantiate(characters[index], tempPos, Quaternion.identity);
                character.transform.parent = emptyTile.transform;
                character.name = emptyTile.name + " : " + index;
            }
        }
    }

    void Update()
    {

    }

    public void test()
    {
        Debug.Log("Get Called");
    }

}
