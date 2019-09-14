using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// These allow us to use the ARFoundation API.
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using System;


public class PlaceGameBoards : MonoBehaviour
{
    public GameObject block;
    public GameObject chess;
    public GameObject gameOverPanel;
    public Text gameOverText;
    public GameObject restartButton;
    //Board size
    public static int BOARD_SIZE = 16;	
    private float block_size = 0.1f;
    // These will store references to our other components.
    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    // This will indicate whether the game board is set.
    private bool placed = false;
    //store all the blocks
    private GameObject[,] blocks = new GameObject[BOARD_SIZE,BOARD_SIZE];
    //store all the chesses
    private GameObject[,] chesses = new GameObject[BOARD_SIZE,BOARD_SIZE];
    //store all the chess positions
    private Dictionary<GameObject, Tuple<int, int>> chessPositions = new Dictionary<GameObject, Tuple<int, int>>();
    //store chess colors
    private int[,] chessColors = new int[BOARD_SIZE,BOARD_SIZE];
    //record player turns. 0 is player 1's turn and 1 is player 2's turn 
    private int turn = 0;
    //track how many chesses are claimed
    private int claimed = 0;
    //wait time between placing board and claiming chess
    private float waitTime = 1f;

    // Start is called before the first frame update
    void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
        gameOverPanel.SetActive(false);
        restartButton.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
       if (!placed)
        {
            if (Input.touchCount > 0)
            {
                Vector2 touchPosition = Input.GetTouch(0).position;

                // Raycast will return a list of all planes intersected by the
                // ray as well as the intersection point.
                List<ARRaycastHit> hits = new List<ARRaycastHit>();
                if (raycastManager.Raycast(
                    touchPosition, hits, TrackableType.PlaneWithinPolygon))
                {
                    // The list is sorted by distance so to get the location
                    // of the closest intersection we simply reference hits[0].
                    var hitPose = hits[0].pose;
                    // Now we will activate our game board and place it at the
                    // chosen location.
                    float x = hitPose.position.x;
                    float y = hitPose.position.y;
                    float z = hitPose.position.z;

                    //clean up 
                    for (int i = 0; i < BOARD_SIZE; i++) {
                        for (int j = 0; j < BOARD_SIZE; j++) {
                            if (blocks[i,j]) {
                                Destroy(blocks[i,j]);
                            }
                            if (chesses[i, j]) {
                                Destroy(chesses[i,j]);
                            }
                        }
                    }
                    chessPositions = new Dictionary<GameObject, Tuple<int, int>>();
                    chessColors = new int[BOARD_SIZE, BOARD_SIZE];
                    waitTime = 1f;
                    claimed = 0;
                    turn = 0;
                    
                    //create new board
                    for (int i = 0; i < BOARD_SIZE; i++) {
                        for (int j = 0; j < BOARD_SIZE; j++) {
                            blocks[i,j] = Instantiate(block, new Vector3(x + i * block_size - block_size * BOARD_SIZE * 0.5f, y, z + j * block_size - block_size * BOARD_SIZE * 0.5f), Quaternion.identity);
                            chesses[i,j] = Instantiate(chess, new Vector3(x + i * block_size - block_size * BOARD_SIZE * 0.5f, y + 0.02f, z + j * block_size - block_size * BOARD_SIZE * 0.5f), Quaternion.identity);
                            chessPositions.Add(chesses[i,j], new Tuple<int, int>(i,j));
                            // indicate the chess is not claimed by any player yet
                            chessColors[i,j] = -1;
                        }
                    }
                    placed = true;
                    // After we have placed the game board we will disable the
                    // planes in the scene as we no longer need them.
                    planeManager.SetTrackablesActive(false);

                }
            }
        }
        else
        {
            // The plane manager will set newly detected planes to active by 
            // default so we will continue to disable these.
            planeManager.SetTrackablesActive(false);
        }

        if (waitTime > 0) {
            waitTime -= Time.deltaTime;
        }
    }

    public void AllowMoveGameBoard()
    {
        placed = false;
        planeManager.SetTrackablesActive(true);
        gameOverPanel.SetActive(false);
        restartButton.SetActive(false);
    }

    public bool Placed()
    {
        return placed;
    }

    public int PlayerTurn()
    {
        return turn;
    }

    //check if chess can be claimed now 
    public bool PlayTime() {
        return waitTime <= 0;
    }

    public void UpdateChessColor(GameObject chess, int color) 
    {
        Tuple<int,int> pos = chessPositions[chess];
        if (chessColors[pos.Item1, pos.Item2] == -1) {
            chessColors[pos.Item1, pos.Item2] = color;
        }
    }

    public void UpdateTurn(){
        claimed ++;
        if (turn == 0) {
            turn = 1;
        } else {
            turn = 0;
        }
    }

    public bool CheckWin(GameObject chess, int player) {
        Tuple<int,int> pos = chessPositions[chess];
        int i = pos.Item1;
        int j = pos.Item2;
        int hori_count = 1;
        
        // for (int x = 0; x < i; x++) {
        //     for (int y = 0; y < j; y++) {
        //         Debug.Log(chessColors[x,y]);
        //     }
        // }
        
        //check horizontal line
        int k = j - 1;
        while (ValidatePosition(i, k)) {
            if (chessColors[i,k] == player) {
                hori_count++;
                k--;
            } else {
                break;
            }
        }
        k = j + 1;
        while (ValidatePosition(i, k)) {
            if (chessColors[i,k] == player) {
                hori_count++;
                k++;
            } else {
                break;
            }
        }
        if (hori_count >= 5) {
            return true;
        }

        //check vertical line
        int verti_count = 1;
        k = i - 1;
        while (ValidatePosition(k, j)) {
            if (chessColors[k,j] == player) {
                verti_count++;
                k--;
            } else {
                break;
            }
        }
        k = i + 1;
        while (ValidatePosition(k, j)) {
            if (chessColors[k,j] == player) {
                verti_count++;
                k++;
            } else {
                break;
            }
        }
        if (verti_count >= 5) {
            return true;
        }

        //check diagonal top-left to bottom-right
        int dia1_count = 1;
        int a = i - 1;
        int b = j - 1;
        while (ValidatePosition(a, b)) {
            if(chessColors[a,b] == player) {
                dia1_count++;
                a--;
                b--;
            } else {
                break;
            }
        }
        a = i + 1;
        b = j + 1;
        while (ValidatePosition(a, b)) {
            if(chessColors[a,b] == player) {
                dia1_count++;
                a++;
                b++;
            } else {
                break;
            }
        }
        if (dia1_count >= 5) {
            return true;
        }

        //check diagonal top-right to bottom-left
        int dia2_count = 1;
        a = i + 1;
        b = j - 1;
        while (ValidatePosition(a,b)) {
            if(chessColors[a,b] == player) {
                dia2_count++;
                a++;
                b--;
            } else {
                break;
            }
        }
        a = i - 1;
        b = j + 1;
        while (ValidatePosition(a,b)) {
            if(chessColors[a,b] == player) {
                dia2_count++;
                a--;
                b++;
            } else {
                break;
            }
        }
        if (dia2_count >= 5) {
            return true;
        }
        return false;
    }

    private bool ValidatePosition(int i, int j) {
        return i >= 0 && i < BOARD_SIZE && j >= 0 && j < BOARD_SIZE;
    }

    public bool Full() {
        return claimed == BOARD_SIZE * BOARD_SIZE;
    }

    public void GameOver(int player) {
        gameOverPanel.SetActive(true);
        if (player == -1) {
            gameOverText.text = "It's a draw!";
        } else if (player == 0) {
            gameOverText.text = "Player 1 Wins!";
        } else {
            gameOverText.text = "Player 2 Wins!";
        }
        restartButton.SetActive(true);
        
    }

    public void RestartGame() {
        placed = false;
        planeManager.SetTrackablesActive(true);
        gameOverPanel.SetActive(false);
        restartButton.SetActive(false);
        for (int i = 0; i < BOARD_SIZE; i++) {
            for (int j = 0; j < BOARD_SIZE; j++) {
                if (blocks[i,j]) {
                    Destroy(blocks[i,j]);
                }
                if (chesses[i, j]) {
                    Destroy(chesses[i,j]);
                }
            }
        }
    }
}
