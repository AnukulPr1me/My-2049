public class GameState
{
    public int [,] tileValue = new int[TileManager.GridSize, TileManager.GridSize];

    public int score;
    public int moveCount;
}