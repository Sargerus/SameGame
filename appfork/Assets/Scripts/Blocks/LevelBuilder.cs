
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelBuilder : MonoBehaviour
{
    [Range(0, 1)]
    public float MarginTopPercent;
    public int Rows;
    public int Columns;
    public List<Color> Colors;
    public SpriteRenderer SpriteRenderer;
    public FallDirection FallDirection;
    public ShiftDirection ShiftDirection;
    public float ShiftSpeed;

    private Camera _camera;
    private Vector3 _scaleVector;
    private float _actualWidth;
    private float _actualHeight;
    private List<Indexable> _blocks;
    private bool _gameOver;
    private Vector3 _cameraTopLeft;
    private Vector3 _cameraBottomRight;
    private float _marginTop;
    private bool _freeze;

    public event Notify OnPlayerWin;
    public event Notify OnPlayerLose;
    public event Action<int> OnBlocksBurn;

    private void Awake()
    {
        _camera = Camera.main;
        Random.InitState(System.DateTime.Now.Millisecond);

        if (FindObjectOfType<SerializeManager>() == null)
        {
            var go = new GameObject();
            go.AddComponent<SerializeManager>();
        }
    }

    void Start()
    {
        if (Colors == null || Colors.Count <= 0)
            Debug.LogError("No colors presented");

        _gameOver = false;

        BuildLevel();
        CheckWinLoseCondition();
    }

    private void BuildLevel()
    {
        _cameraTopLeft = _camera.ViewportToWorldPoint(new Vector3(0, 1, 10f));
        _cameraBottomRight = _camera.ViewportToWorldPoint(new Vector3(1, 0, 10f));

        _actualWidth = _cameraBottomRight.x - _cameraTopLeft.x;
        _actualHeight = _cameraTopLeft.y - _cameraBottomRight.y;
        _marginTop = MarginTopPercent * _actualHeight;

        _actualHeight -= _marginTop;

        _scaleVector = new Vector3(_actualWidth / Columns, _actualHeight / Rows, 1);

        _blocks = new List<Indexable>();
        SpriteRenderer sr;
        Vector3 pos = Vector3.zero;
        int type = -1;
        pos.y = (_cameraTopLeft.y - _marginTop) + _scaleVector.y / 2;

        for (int i = 0; i < Rows; i++)
        {
            pos.y -= _scaleVector.y;
            pos.x = _cameraTopLeft.x + _scaleVector.x / 2;

            for (int j = 0; j < Columns; j++)
            {
                sr = Instantiate(SpriteRenderer, pos, Quaternion.identity);
                type = Random.Range(0, Colors.Count);
                sr.color = Colors[type];
                sr.transform.localScale = _scaleVector;
                sr.GetComponent<Indexable>().Type = type;
                pos.x += _scaleVector.x;
                _blocks.Add(sr.GetComponent<Indexable>());
            }
        }
    }

    private int Index(int row, int col) => (row * Columns) + col;
    private int RightIndex(int index) => index + 1;
    private int LeftIndex(int index) => index - 1;
    private int TopIndex(int index) => index - Columns;
    private int BottomIndex(int index) => index + Columns;
    private bool FirstInRow(int index) => index % Columns == 0;
    private bool LastInRow(int index) => (index + 1) % Columns == 0;
    private bool UpperInColumn(int index) => TopIndex(index) < 0;
    private bool LowerInColumn(int index) => BottomIndex(index) >= _blocks.Count;
    private Vector2 IndexToRowAndColumn(int index)
    {
        Vector2 retValue = new Vector2(-1, -1);
        for (int i = 0; i < Rows; i++)
        {
            if (index < (Columns * i) + Columns && index >= Columns * i)
            {
                retValue = new Vector2(index - Columns * i, i);
            }
        }

        return retValue;
    }
    private Vector3 IndexToPosition(int index)
    {
        Vector2 coordinates = IndexToRowAndColumn(index);

        return new Vector3(x: (_cameraTopLeft.x - _scaleVector.x / 2) + _scaleVector.x * (coordinates.x + 1),
                           y: (_cameraTopLeft.y + _scaleVector.y / 2) - _scaleVector.y * (coordinates.y + 1),
                           z: 0);
    }

    void Update()
    {
        if (_gameOver || _freeze) return;
        HandleTouch();
    }

    private void HandleTouch()
    {
        if (_gameOver || _freeze) return;
        if (Input.touchCount <= 0) return;
        if (!_camera) return;

        Vector3 pos;
        Touch touch = Input.GetTouch(0);

        pos = _camera.ScreenToWorldPoint(touch.position);

        int column = Mathf.FloorToInt((pos.x + _actualWidth / 2) / _scaleVector.x);
        int row = Mathf.FloorToInt(((_actualHeight - MarginTopPercent * _actualHeight) / 2 - pos.y) / _scaleVector.y);
        Debug.Log("Row: " + row + '\n' + "Column: " + column);

        if (column >= Columns || column < 0 || row >= Rows || row < 0)
        {
            Debug.LogError("Click out of bounds");
            return;
        }

        ProcessTableFrom(Index(row, column));
    }
    private void ProcessTableFrom(int index)
    {
        _freeze = true;
        CheckBlock(index);
        DeleteMarkedForDeletion();
        StartCoroutine(PositionBlocks());
    }
    private void CheckBlock(int index, int compareType = -1)
    {
        if (_blocks[index] == null) return;
        if (index < 0 || index >= _blocks.Count) return;

        if (compareType == -1) //only for first call
            compareType = _blocks[index].Type;
        else if (_blocks[index].Type == -1) //returned here again, out condition
            return;
        else if (_blocks[index].Type == compareType)
            _blocks[index].Type = -1; //-1 means marked for deletion
        else if (_blocks[index].Type != compareType) //obvious, out condition
            return;

        if (!FirstInRow(index))
            CheckBlock(LeftIndex(index), compareType);

        if (!UpperInColumn(index))
            CheckBlock(TopIndex(index), compareType);

        if (!LastInRow(index))
            CheckBlock(RightIndex(index), compareType);

        if (!LowerInColumn(index))
            CheckBlock(BottomIndex(index), compareType);
    }
    private void DeleteMarkedForDeletion()
    {
        int burntBlocks = 0;

        for (int i = 0; i < _blocks.Count; i++)
        {
            if (_blocks[i] == null) continue;

            if (_blocks[i].Type == -1)
            {
                burntBlocks++;
                Destroy(_blocks[i].gameObject);
                _blocks[i] = null;
            }
        }

        OnBlocksBurn?.Invoke(burntBlocks);
    }
    private IEnumerator PositionBlocks()
    {
        yield return FallDown();
        yield return Shift();

        CheckWinLoseCondition();
    }

    private IEnumerator FallDown()
    {
        List<Coroutine> wait = new List<Coroutine>();

        for (int i = 0; i < Columns; i++)
        {
            int dist = 0;
            for (int j = Rows - 1; j >= 0; j--)
            {
                if (_blocks[Index(j, i)] == null)
                    dist += 1;
                else if (dist > 0)
                {
                    _blocks[Index(j + dist, i)] = _blocks[Index(j, i)];
                    _blocks[Index(j, i)] = null;
                    wait.Add(StartCoroutine(Move(Index(j + dist, i))));
                }
            }
        }

        for (int i = 0; i < wait.Count; i++)
            yield return wait[i];
    }

    private IEnumerator Shift()
    {
        int dist = 0;
        List<Coroutine> wait = new List<Coroutine>();

        switch (ShiftDirection)
        {
            case ShiftDirection.Left:
                {
                    for (int i = 0; i < Columns; i++)
                        if (_blocks[Index(Rows - 1, i)] == null)
                            dist++;
                        else if (dist > 0)
                        {
                            for (int j = Rows - 1; j >= 0; j--)
                            {
                                if (_blocks[Index(j, i)] == null) break;

                                _blocks[Index(j, i - dist)] = _blocks[Index(j, i)];
                                _blocks[Index(j, i)] = null;
                                wait.Add(StartCoroutine(Move(Index(j, i - dist))));
                            }
                        }

                    break;
                }
            case ShiftDirection.Right:
                {
                    for (int i = Columns - 1; i >= 0; i--)
                        if (_blocks[Index(Rows - 1, i)] == null)
                            dist++;
                        else if (dist > 0)
                        {
                            for (int j = Rows - 1; j >= 0; j--)
                            {
                                if (_blocks[Index(j, i)] == null) break;

                                _blocks[Index(j, i + dist)] = _blocks[Index(j, i)];
                                _blocks[Index(j, i)] = null;
                                wait.Add(StartCoroutine(Move(Index(j, i + dist))));
                            }
                        }

                    break;
                }
        }

        for (int i = 0; i < wait.Count; i++)
            yield return wait[i];
    }

    private IEnumerator Move(int index)
    {
        Vector3 targetPos = IndexToPosition(index);
        targetPos.y -= _marginTop;

        Transform movingObject = _blocks[index].transform;

        while ((movingObject.position - targetPos).magnitude > 0.001f)
        {
            movingObject.position = Vector3.MoveTowards(movingObject.position, targetPos, ShiftSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void CheckWinLoseCondition()
    {
        if (_blocks.FindIndex(g => g != null) < 0)
        {
            _gameOver = true;
            OnPlayerWin?.Invoke();
        }
        else
        {
            for (int i = 0; i < _blocks.Count; i++)
            {
                if (NeighbourOfSameTypeExists(i))
                {
                    _freeze = false;
                    return;
                }
            }

            _gameOver = true;
            OnPlayerLose?.Invoke();
        }
    }
    private bool NeighbourOfSameTypeExists(int blockIndex)
    {
        if (_blocks[blockIndex] == null) return false;

        bool retValue = false;

        int compareType = GetBlockType(blockIndex);

        if (!UpperInColumn(blockIndex) && GetBlockType(TopIndex(blockIndex)) == compareType)
            retValue = true;
        else if (!LastInRow(blockIndex) && GetBlockType(RightIndex(blockIndex)) == compareType)
            retValue = true;
        else if (!LowerInColumn(blockIndex) && GetBlockType(BottomIndex(blockIndex)) == compareType)
            retValue = true;
        else if (!FirstInRow(blockIndex) && GetBlockType(LeftIndex(blockIndex)) == compareType)
            retValue = true;

        return retValue;
    }
    private int GetBlockType(int blockIndex)
    {
        int retValue = -1;

        if (blockIndex >= 0 && blockIndex < Columns * Rows && _blocks[blockIndex] != null)
            retValue = _blocks[blockIndex].Type;

        return retValue;
    }
}
