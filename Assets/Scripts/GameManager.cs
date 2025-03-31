using UnityEngine;

public class GameManager : MonoBehaviour
{
    public TileBoard board;
    
    private void Start()
    {
        NewGame();
    }

    public void NewGame() 
    {
        board.ClearBoard();
        board.CreateTile();
        board.CreateTile();
        board.enabled = true;
    }

    public void GameOver() {
        board.enabled = false;
    }
}
