using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Windows;
using Random = UnityEngine.Random;

public class TileManager : MonoBehaviour
{
    public static int GridSize = 4;

    private readonly Transform[,] _tilePosition = new Transform[GridSize, GridSize];
    private readonly Tile[,] _tiles = new Tile[GridSize, GridSize];

    [SerializeField] private Tile tilePrefab;
    [SerializeField] private GameOverScreen gameOverScreen;

    private bool _isAnimating;

    private int _score;

    private int _bestScore;

    private int _moveCount;

    private System.Diagnostics.Stopwatch _GameStopwatch = new System.Diagnostics.Stopwatch();

    private IInputManager _inputManager = new SwipeInputManager(); // If you want to use keyboard remove new SwipeInputManager() and add new KeyBoardInputManager() 

    [SerializeField] private TileSetting _tileSetting;

    [SerializeField] private UnityEvent<int> scoreUpdate;
    [SerializeField] private UnityEvent<int> bestScoreUpdate;
    [SerializeField] private UnityEvent<int> moveCountUpdate;
    [SerializeField] private UnityEvent<System.TimeSpan> gameTimeUpdate;

    private Stack<GameState> _gameStates = new Stack<GameState>();
    // Start is called before the first frame update
    void Start()
    {
        GetTilePosition();
        TrySpawnTile();
        TrySpawnTile();

        UpdateTilePosition(true);

        _GameStopwatch.Start();

        _bestScore = PlayerPrefs.GetInt("BestScore", 0);

        bestScoreUpdate.Invoke(_bestScore);

    }
    private int _lastXInput;
    private int _lastYInput;

    // Update is called once per frame
    void Update()
    {
        gameTimeUpdate.Invoke(_GameStopwatch.Elapsed);

        InputResult input = _inputManager.GetInput();

        if (!_isAnimating)
        {
            TryMove(input.xInput, input.yInput);
        }

    }

    public void AddScore(int value)
    {
        _score += value;
        scoreUpdate.Invoke(_score);

        if (_score > _bestScore)
        {
            _bestScore = _score;
            bestScoreUpdate.Invoke(_bestScore);
            PlayerPrefs.SetInt("BestScore", _bestScore);
        }
    }

    public void RestartGame()
    {
        var activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    private void GetTilePosition()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        int x = 0;
        int y = 0;
        foreach (Transform transform in this.transform)
        {
            _tilePosition[x, y] = transform;
            x++;

            if (x >= GridSize)
            {
                x = 0;
                y++;
            }
        }
    }

    private bool TrySpawnTile()
    {
        List<Vector2Int> availablespots = new List<Vector2Int>();
        
        for(int x = 0; x < GridSize; x++)
        for(int y = 0; y < GridSize; y++)
            {
                if (_tiles[x, y] == null)
                {
                    availablespots.Add(new Vector2Int(x, y));
                }
            }
            
          

        if (!availablespots.Any())
        {
            return false;
        }
        int randomIndex = Random.Range(0, availablespots.Count);
        Vector2Int spot = availablespots[randomIndex];

        var tile = Instantiate(tilePrefab, transform.parent);
        tile.SetValue(GetRandomValue());
        _tiles[spot.x, spot.y] = tile;


        return true;
        

    }

    private int GetRandomValue()
    {
        var rand = Random.Range(0f, 1f);
        if (rand <= .8f)
        {
            return 2;
        }
        else
        {
            return 4;
        }

    }

    private void UpdateTilePosition(bool instant)
    {
        if (!instant)
        {
            _isAnimating = true;
            StartCoroutine(WaitForTileAnimation());
        }
        for(int x = 0; x< GridSize;x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                if (_tiles[x, y] != null)
                {
                    _tiles[x, y].setPosition(_tilePosition[x, y].position, instant);
                }

            }

        }

    }

    private IEnumerator WaitForTileAnimation()
    {
        yield return new WaitForSeconds(_tileSetting.AnimationTime);
        if (!TrySpawnTile())
        {
            Debug.LogError("unable to move");

        }
        UpdateTilePosition(true);

        if(!AnyMoveLeft())
        {
            gameOverScreen.SetGameOver(true);
        }

        _isAnimating = false;
    }

    private bool AnyMoveLeft()
    {
       return CanMoveDown() || CanMoveUp() || CanMoveRight() || CanMoveLeft();
    }

    private bool _tilesUpdated;

    //Movement

    private void TryMove(int x, int y)
    {
        if (x == 0 && y==0)
            return;

        if (Mathf.Abs(x) == 1 && MathF.Abs(y) == 1)
        {
            return;
        }

        _tilesUpdated = false;
        int[,] preMoveTileValues = GetCurrentTileValues();

        if (x == 0 )
        {
            if (y > 0)
            {
                TryMoveUp();
            }
            else
            {
                TryMoveDown();
            }
        }
        else
        {
            if (x < 0)
            {
                TryMoveLeft();
            }
            else
            {
                TryMoveRight();
            }
        }
        if (_tilesUpdated)
        {
            _gameStates.Push(new GameState() { tileValue = preMoveTileValues, score = _score, moveCount = _moveCount });
            _moveCount++;
            moveCountUpdate.Invoke(_moveCount);
            UpdateTilePosition(false);
        }
        
    }

    private int[,] GetCurrentTileValues()
    {
        int[,] result = new int[GridSize, GridSize];
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                if (_tiles[x, y] != null)
                {
                    result[x, y] = _tiles[x, y].GetValue();
                }
            }
        }
        return result;
    }

    public void LoadLastGameState()
    {
        if (_isAnimating)
        {
            return;
        }

        if(!_gameStates.Any())
        {
            return;
        }
        GameState previousGameState = _gameStates.Pop();

        gameOverScreen.SetGameOver(false);

        _score = previousGameState.score;
        scoreUpdate.Invoke(_score);

        _moveCount = previousGameState.moveCount;
        moveCountUpdate.Invoke(_moveCount);


        foreach (Tile t in _tiles)
        {
            if (t != null)
            {
                Destroy(t.gameObject);
            }
        }

        for(int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                _tiles[x, y] = null;
                if (previousGameState.tileValue[x, y] == 0)
                {
                    continue;

                }
                Tile tile = Instantiate(tilePrefab, transform.parent);
                tile.SetValue(previousGameState.tileValue[x, y]);
                _tiles[x, y] = tile;
            }
        }
        UpdateTilePosition(true);
    }
    

    private bool TileExistsBetween(int x, int y, int x2, int y2)
    {
        if (x == x2)
        {
            return TilesExistsBetweenVertical(x, y, y2);
        }
        else if (y == y2)
        {
            return TileExistsHorizontal(x, x2, y);
        }
        return true;
    }

    private bool TileExistsHorizontal(int x, int x2, int y)
    {
        int minX = Mathf.Min(x, x2);
        int minY = Mathf.Min(y, x2);
        for (int xIndex = minX + 1;  xIndex < minX; xIndex++)
        {
            if (_tiles[xIndex, y] != null)
            {
                return true;
            }
        }
        return false;
    }

    private bool TilesExistsBetweenVertical(int x, int y, int y2)
    {
        int minX = Mathf.Min(y, y2);
        int minY = Mathf.Min(y, y2);
        for (int yIndex = minY + 1; yIndex < minY; yIndex++)
        {
            if (_tiles[x, yIndex] != null)
            {
                return true;
            }
        }
        return false;
    }


    private void TryMoveRight()
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = GridSize - 1; x >= 0; x--)
            {
                if (_tiles[x, y] == null) continue;

                for (int x2 = GridSize - 1; x2 > x; x2--)
                {
                    if (_tiles[x2, y] != null)
                    {
                        if (TileExistsBetween(x, y, x2, y))
                        {
                            continue;
                        }
                        if (_tiles[x2, y].Merge(_tiles[x, y]))
                        {
                            _tiles[x, y] = null;
                            _tilesUpdated = true;
                        }
                        continue;
                    }

                    _tilesUpdated = true;
                    _tiles[x2, y] = _tiles[x, y];
                    _tiles[x, y] = null;
                    break;
                }

            }

        }

    }

    private void TryMoveLeft()
    {
        for (int y = 0; y < GridSize ; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if(_tiles[x, y] == null) continue;
                for (int x2 = 0; x2 < x; x2++)
                {
                    if (_tiles[x2, y] != null)
                    {
                        if (TileExistsBetween(x, y, x2, y))
                        {
                            continue;
                        }
                        if (_tiles[x2, y].Merge(_tiles[x, y]))
                        {
                            _tiles[x, y] = null;
                            _tilesUpdated = true;
                        }
                        continue;
                    }



                    _tilesUpdated = true;
                    _tiles[x2, y] = _tiles[x, y];
                    _tiles[x, y] = null;
                    break;
                }
            }
        }
    }

    private void TryMoveDown()
    {
        for (int x =0; x < GridSize ; x++)
        {
            for (int y = GridSize - 1; y >= 0; y--)
            {
                if (_tiles[x, y] == null) continue;
                for (int y2 = GridSize - 1; y2 > y; y2--)
                {
                    if (_tiles[x, y2] != null)
                    {
                        if (TileExistsBetween(x, y, x, y2))
                        {
                            continue;
                        }
                        if (_tiles[x, y2].Merge(_tiles[x, y]))
                        {
                            _tiles[x, y] = null;
                            _tilesUpdated = true;
                        }
                        continue;
                    }


                    _tilesUpdated = true;
                    _tiles[x, y2] = _tiles[x, y];
                    _tiles[x, y] = null;
                    break;
                }

            }
        }

    }


    private void TryMoveUp()
    {
        for (int x=0; x < GridSize ; x++)
        {
            for (int y =0; y < GridSize; y++)
            {
                if (_tiles[x, y] == null) continue;
                for (int y2 = 0; y2 < y; y2++)
                {
                    if (_tiles[x, y2] != null)
                    {
                        if (TileExistsBetween(x, y, x, y2))
                        {
                            continue;
                        }
                        if (_tiles[x, y2].Merge(_tiles[x, y]))
                        {
                            _tiles[x, y] = null;
                            _tilesUpdated = true;
                        }
                        continue;
                    }

                    _tilesUpdated = true;
                    _tiles[x, y2] = _tiles[x, y];
                    _tiles[x, y] = null;
                    break;
                }
            }
        }

    }


    private bool CanMoveRight()
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = GridSize - 1; x >= 0; x--)
            {
                if (_tiles[x, y] == null) continue;

                for (int x2 = GridSize - 1; x2 > x; x2--)
                {
                    if (_tiles[x2, y] != null)
                    {
                        if (TileExistsBetween(x, y, x2, y))
                        {
                            continue;
                        }
                        if (_tiles[x2, y].CanMerge(_tiles[x, y]))
                        {
                            return true;
                        }
                        continue;
                    }

                    return true;
                }

            }

        }
        return false;

    }

    private bool CanMoveLeft()
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (_tiles[x, y] == null) continue;
                for (int x2 = 0; x2 < x; x2++)
                {
                    if (_tiles[x2, y] != null)
                    {
                        if (TileExistsBetween(x, y, x2, y))
                        {
                            continue;
                        }
                        if (_tiles[x2, y].CanMerge(_tiles[x, y]))
                        {
                            return true;
                        }
                        continue;
                    }



                    return true;
                }
            }
        }
        return false;
    }

    private bool CanMoveDown()
    {
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = GridSize - 1; y >= 0; y--)
            {
                if (_tiles[x, y] == null) continue;
                for (int y2 = GridSize - 1; y2 > y; y2--)
                {
                    if (_tiles[x, y2] != null)
                    {
                        if (TileExistsBetween(x, y, x, y2))
                        {
                            continue;
                        }
                        if (_tiles[x, y2].CanMerge(_tiles[x, y]))
                        {
                            return true;
                        }
                        continue;
                    }


                    return true;
                }

            }
        }

        return false;

    }


    private bool CanMoveUp()
    {
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                if (_tiles[x, y] == null) continue;
                for (int y2 = 0; y2 < y; y2++)
                {
                    if (_tiles[x, y2] != null)
                    {
                        if (TileExistsBetween(x, y, x, y2))
                        {
                            continue;
                        }
                        if (_tiles[x, y2].CanMerge(_tiles[x, y]))
                        {
                            return true;
                        }
                        continue;
                    }

                    return true;
                }
            }
        }

        return false;
    }


}
