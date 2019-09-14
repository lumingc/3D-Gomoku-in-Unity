using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chess : MonoBehaviour
{
    public Material player1;
    public Material player2;

    private bool placed = false;

    public void OnTouch(PlaceGameBoards placeGameBoards)
    {       
	    if (!placed && placeGameBoards.PlayTime()) {
            placed = true;
            int color =  placeGameBoards.PlayerTurn();
            placeGameBoards.UpdateChessColor(this.gameObject, color);
            if (color == 0) {
                this.gameObject.GetComponent<Renderer>().material = player1;
            } else {
                this.gameObject.GetComponent<Renderer>().material = player2;
            }
            bool win = placeGameBoards.CheckWin(this.gameObject, color);
            if (win) {
                placeGameBoards.GameOver(color);
            } else {
                placeGameBoards.UpdateTurn();
                if (placeGameBoards.Full()) {
                    placeGameBoards.GameOver(-1);
                }
            }
        }
    }

}
