﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    // Script for the Board Manager to initiate the game board
    struct Gem
    {
        public int resourceIndex;           // Determines what the gem looks like               
        public GameObject gemObject;
        public bool isSelected;
    }

    private Gem[,] allGems;                 // Stores all gem objects on the board       
    private Gem draggedGem;                 // Reference to the dragged gem
    private Gem draggedGemClone = new Gem();// Reference to the clone of the gem dragged
    private bool lock1 = false;
    private bool lock2 = false;
    private int lock3 = 0;
    private Vector2 mousePos = Vector2.zero;// Vector of the mouse position
    private bool dragging = false;          // Boolean object to show dragging status
    private bool matchFound = false;        

    public int width;                       // Board width
    public int height;                      // Board height
    public List<GameObject> gemResources = new List<GameObject>();  // List of gem prefabs

    // Start is called before the first frame update 
    void Start()
    {
        SetUp();
    }

    // Create the board using a 2D array storing all the tiles stating at (0,0)
    private void SetUp()
    {

        // 2 loops to create the game board from (0,0)
        // Camera position should be modified to make sure
        // the game board is at the center of the screen
        allGems = new Gem[width, height + 1];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // Create a gem at current position
                Vector2 pos = new Vector2(i, j);

                // Preparing for possible gems to allocate
                List<int> possibleIndexes = new List<int>();
                for (int k = 0; k < gemResources.Count; k++) possibleIndexes.Add(k);

                // Avoid repeating 3 gems at the beginning
                if (i >= 2 && allGems[i - 2, j].resourceIndex == allGems[i - 1, j].resourceIndex)
                    possibleIndexes.Remove(allGems[i - 2, j].resourceIndex);
                if (j >= 2 && allGems[i, j - 2].resourceIndex == allGems[i, j - 1].resourceIndex)
                    possibleIndexes.Remove(allGems[i, j - 2].resourceIndex);

                // Randomly pick one from the gem resource set
                int index = possibleIndexes[Random.Range(0, possibleIndexes.Count)];

                // Creating the object
                GameObject gem = Instantiate(gemResources[index], pos, Quaternion.identity);
                gem.transform.name = "(" + i + "," + j + ") : " + index;
                allGems[i, j] = new Gem
                {
                    resourceIndex = index,
                    gemObject = gem,
                    isSelected = false
                };
            }
        }
    }

    void Update()
    {
        // If the board is processing, control will be denied
        if (lock1 || lock2 || lock3 > 0)
        {
            return;
        }

        // When press down the mouse primary key (start of dragging)
        if (Input.GetMouseButtonDown(0))
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            createClone(); // Create a gem clone
        } // When holding primary key (dragging)
        else if (Input.GetMouseButton(0) && dragging)
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            trackDragging();
        }

        // When ending with primary key (end of dragging)
        if (Input.GetMouseButtonUp(0) && dragging)
        {
            trackDragging();
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            releaseGem();
            dragging = false;
        }

        // Check selected gem
        if (draggedGem.gemObject != null && draggedGem.isSelected)
        {
            draggedGem.gemObject.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 0, 5f);
            if (Input.GetMouseButtonUp(0))
                draggedGem.isSelected = false;
        }
    }

    // Create a gem clone when dragging the gem
    void createClone()
    {
        // Get mouse position
        int pos_x = Mathf.RoundToInt(mousePos.x);
        int pos_y = Mathf.RoundToInt(mousePos.y);

        // Determine if the mouse is inside the board
        if (pos_x >= width || pos_x < 0 || pos_y > height || pos_y < 0)
        {
            Debug.Log("Click position out of board");
            return;
        }
        else
        {
            dragging = true;
            // Select the gem at the mouse position
            draggedGem = allGems[pos_x, pos_y];

            GameObject gemObjectClone = Instantiate(draggedGem.gemObject,
            new Vector2(pos_x, pos_y), Quaternion.identity);
            gemObjectClone.name = "(" + pos_x + "," + pos_y + ") : " + draggedGem.resourceIndex;

            draggedGemClone = new Gem
            {
                resourceIndex = draggedGem.resourceIndex,
                gemObject = gemObjectClone,
                isSelected = false
            };

            draggedGem.isSelected = true;

            // Change the transparency so user can see the gem underneath 
            Color32 color = draggedGem.gemObject.GetComponent<SpriteRenderer>().material.color;
            color.a = 100;
            draggedGem.gemObject.GetComponent<SpriteRenderer>().material.color = color;

            // Change reference in list
            allGems[pos_x, pos_y] = draggedGemClone;

        }
    }

    // Move the dragged gem toward the direction of the mouse 
    private void trackDragging()
    {

        int draggedX = Mathf.RoundToInt(draggedGemClone.gemObject.transform.position.x);
        int draggedY = Mathf.RoundToInt(draggedGemClone.gemObject.transform.position.y);

        int targetX = draggedX, targetY = draggedY;

        // Get mouse position
        int pos_x = Mathf.RoundToInt(mousePos.x);
        int pos_y = Mathf.RoundToInt(mousePos.y);

        // The gem to swap is the next gem at the direction
        // of the mouse 
        if (pos_x > draggedX && draggedX + 1 < width)
            targetX++;
        else if (pos_x < draggedX && draggedX - 1 >= 0)
            targetX--;
        else if (pos_y > draggedY && draggedY + 1 < height)
            targetY++;
        else if (pos_y < draggedY && draggedY - 1 >= 0)
            targetY--;



        // Get the target gem to swap with dragged gem
        swapGems(draggedX,draggedY,targetX,targetY);
    }

    // Swap two gems
    private void swapGems(int x1, int y1, int x2, int y2)
    {
        // TODO: Animation

        if (x1 == x2 && y1 == y2) return;

        if (allGems[x1, y1].gemObject == null ||
            allGems[x2, y2].gemObject == null)
            return;

        // Swap positions
        Vector2 tempPos = allGems[x1, y1].gemObject.transform.position;
        allGems[x1, y1].gemObject.transform.position =
            allGems[x2, y2].gemObject.transform.position;
        allGems[x2, y2].gemObject.transform.position = tempPos;

        // Swap gems in the array
        Gem temp = new Gem
        {
            resourceIndex = allGems[x1, y1].resourceIndex,
            gemObject = allGems[x1, y1].gemObject,
            isSelected = false
        };
        allGems[x1, y1] = new Gem
        {
            resourceIndex = allGems[x2, y2].resourceIndex,
            gemObject = allGems[x2, y2].gemObject,
            isSelected = false
        };
        allGems[x2, y2] = temp;
    }

    // Release the dragged gem to current position
    private void releaseGem()
    {
        // Remove the reference to the dragged gem and destroy the object
        Destroy(draggedGem.gemObject);
        draggedGem = new Gem();
        draggedGem.isSelected = false;

        matchFound = true;

        while (matchFound)
        {
            matchFound = false;

            // Check Match
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (allGems[i, j].resourceIndex >= 0)
                    {
                        StartCoroutine(checkMatch(i, j));
                        StartCoroutine(dropGems());
                    }
                        
                }
            }
        }

        // Remove the reference to the clone
        draggedGemClone = new Gem();
    }

    // Check if there are at least 3 identical gems at (x,y)
    private IEnumerator checkMatch(int x, int y)
    {
        while (lock1 || lock2 || lock3 > 0)
            yield return new WaitForSeconds(0.1f);

        debugCheck();

        List<GameObject> toDestroy = new List<GameObject>();

        lock1 = true;

        Gem target = allGems[x, y];
        int index = target.resourceIndex;

        // Setting counters
        int up = 0, down = 0, left = 0, right = 0;

        // Four loops to detect the number of identical gems at each direction
        while (y + up + 1 < height && allGems[x, y + up + 1].resourceIndex == index)
            up++;
        while (y - down - 1 >= 0 && allGems[x, y - down - 1].resourceIndex == index)
            down++;
        while (x + right + 1 < width && allGems[x + right + 1, y].resourceIndex == index)
            right++;
        while (x - left - 1 >= 0 && allGems[x - left - 1, y].resourceIndex == index)
            left++;


        if (left + right >= 2 && left + right >= up + down)
        {
            matchFound = true;

            for (int i = x - left; i <= x + right; i ++)
            {
                allGems[i,y].resourceIndex = -1;
                allGems[i,y].gemObject.GetComponent<SpriteRenderer>().color = Color.red;
                toDestroy.Add(allGems[i, y].gemObject);
            }
        }
        else if (up + down >= 2)
        {
            matchFound = true;

            for (int j = y - down; j <= y + up; j ++)
            {
                allGems[x,j].resourceIndex = -1;
                allGems[x,j].gemObject.GetComponent<SpriteRenderer>().color = Color.red;
                toDestroy.Add(allGems[x, j].gemObject);
            }
        }

        if (toDestroy.Count > 0) yield return new WaitForSeconds(0.5f);

        foreach (GameObject obj in toDestroy) Destroy(obj);

        lock1 = false;
    }

    // Drop all gems based on the drop distance recorded
    private IEnumerator dropGems()
    {
        while (lock1 || lock2 || lock3 > 0)
            yield return new WaitForSeconds(0.1f);

        lock1 = true;

        for (int i = 0; i < width; i ++)
        {
            Vector2 emptyPos = getLowestEmpty(i);

            while (emptyPos.x >= 0)
            {
                allGems[i, height] = generateGem(i, height);
                StartCoroutine(dropByOne(i,Mathf.RoundToInt(emptyPos.y)));

                while (lock2) yield return new WaitForSeconds(0.1f);

                emptyPos = getLowestEmpty(i);
            }
        }

        lock1 = false;
    }

    // Randomly generate and return a new gem
    private Gem generateGem(int x, int y)
    {
        Vector2 pos = new Vector2(x, y);
        int index = Random.Range(0, gemResources.Count);

        GameObject gem = Instantiate(gemResources[index], pos, Quaternion.identity);
        gem.transform.name = "(" + x + "," + y + ") : " + index;
        return new Gem
        {
            resourceIndex = index,
            gemObject = gem,
            isSelected = false,
        };
    }

    private Vector2 getLowestEmpty(int col)
    {
        for (int i = 0; i < height; i ++)
        {
            if (isEmpty(allGems[col, i])) return new Vector2(col, i); 
        }

        return new Vector2(-1,-1);
    }

    // Determine if the gem is empty
    private bool isEmpty(Gem gem)
    {
        if (gem.gemObject == null) return true;
        if (gem.resourceIndex == -1) return true;

        return false;
    }

    // Drop all gems in the column by one 
    private IEnumerator dropByOne(int col, int row)
    {
        lock2 = true;
        for (int i = row + 1; i < height + 1; i ++)
        {
            if (!isEmpty(allGems[col,i]))
            {
                StartCoroutine(dropSingleGemByOne(allGems[col, i].gemObject, col, i));
                lock3++;

                allGems[col, i - 1] = new Gem
                {
                    resourceIndex = allGems[col,i].resourceIndex,
                    gemObject = allGems[col, i].gemObject,
                    isSelected = false
                };
                allGems[col,i-1].gemObject.name = "(" + col + "," + (i-1) + 
                ") : " + allGems[col,i-1].resourceIndex;

                allGems[col, i].gemObject = null;
                allGems[col, i].resourceIndex = -1;
            }
        }

        while (lock3 > 0)
            yield return new WaitForSeconds(0.1f);

        lock2 = false;
    }

    private IEnumerator dropSingleGemByOne(GameObject obj, int col, int i)
    {
        WaitForSeconds delay = new WaitForSeconds(0.001f);
        float lerpPercent = 0;
        while (lerpPercent <= 1 && obj != null)
        {
            obj.transform.position = Vector2.Lerp(new Vector2(col, i),
                new Vector2(col, i - 1), lerpPercent);
            lerpPercent += 0.05f; //Distance of every drop frame
            yield return delay;
        }
        lock3--;
    }

    private void debugCheck()
    {
        for (int i = 0; i < width; i ++)
        {
            for (int j = 0; j < height; j ++)
            {
                if (allGems[i, j].gemObject == null)
                {
                    Debug.Log(i + "," + j + " Object Missing!");
                }

                if (allGems[i,j].resourceIndex == -1 &&
                    allGems[i,j].gemObject != null)
                {
                    Debug.Log(i + "," + j + " Index Error");
                    Debug.Log("Position: " + allGems[i, j].gemObject.transform.position);
                }

                if (allGems[i,j].gemObject != null &&
                    (Mathf.RoundToInt(allGems[i,j].gemObject.transform.position.x) != i ||
                    Mathf.RoundToInt(allGems[i,j].gemObject.transform.position.y) != j))
                {
                    Debug.Log(i + "," + j + " Position Error:\n" + 
                        "Array: " + i + "," + j + "\tActual: " +
                        allGems[i,j].gemObject.transform.position.x + 
                        "," + allGems[i,j].gemObject.transform.position.y);
                }

                if (string.Compare(allGems[i, j].gemObject.tag, getPrefabName(allGems[i, j])) != 0)
                {
                    Debug.Log(i + "," + j + " Index Error");
                }
            }
        }
    }

    private string getPrefabName(Gem gem)
    {
        switch (gem.resourceIndex)
        {
            case 0:
                return "4star";
            case 1:
                return "6star";
            case 2:
                return "heart";
            case 3:
                return "circle";
            case 4:
                return "diamond";
            default:
                return "empty";
        }
    }

    /*
    private IEnumerator test()
    {
        Debug.Log("Coroutine begins ");

        yield return new WaitForSeconds(3);

        Debug.Log("Coroutine ends");
    }
    */
}
