using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    private int _value = 2;  // =2

    [SerializeField] private TMP_Text text;

    private Vector3 _startPos;

    private Vector3 _endPos;

    private bool _isAnimating;

    private float _count;
    [SerializeField] private TileSetting TileSetting;

    private Tile _mergeTile;

    private Animator _animator;

    private TileManager _tileManager;

    private Image _tileImage;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _tileManager = FindObjectOfType<TileManager>();
        _tileImage = GetComponent<Image>();
    }

/*    [SerializeField] private float AnimationTime = .3f;
    [SerializeField] private AnimationCurve AnimationCurve;*/
    
    public void SetValue(int value)
    {
        _value = value;
        text.text = value.ToString();
        TileColor newColor = TileSetting.TileColors.FirstOrDefault(Color => Color.value == _value) ?? new TileColor();
        text.color = newColor.fgColor;
        _tileImage.color = newColor.bgColor;
    }

    private void Update()
    {
        if (!_isAnimating)
        {
            return;
        }
        _count += Time.deltaTime;
        float t = _count / TileSetting.AnimationTime;
        t = TileSetting.AnimationCurve.Evaluate(t);

        Vector3 newPos = Vector3.Lerp(_startPos, _endPos, t);
        transform.position = newPos;

        if (_count >= TileSetting.AnimationTime)
        {
            _isAnimating = false;
            if (_mergeTile != null)
            {
                int newValue = _value + _mergeTile._value;
                _tileManager.AddScore(newValue);
                SetValue(newValue);
                Destroy(_mergeTile.gameObject);
                _animator.SetTrigger("Merge");
                _mergeTile = null;
            }

        }
    } 
    


    public bool Merge(Tile otherTile)
    {
        if (!CanMerge(otherTile))
        {
            return false;
        }
        _mergeTile = otherTile;
        return true;
    }
        
    

    public bool CanMerge(Tile otherTile)
    {
        if (_value != otherTile._value)            // some errorwhat don't know why this.
        {
            return false;
        }
        if (_mergeTile != null || otherTile._mergeTile != null)
        {
            return false;
        }

        return true;
    }

    public int GetValue()
    {
        return _value;
    }

    public void setPosition(Vector3 newPos, bool instant)
    {
        if (instant)
        {
            transform.position = newPos;
            return;
        }
        _startPos = transform.position;
        _endPos = newPos;
        _count = 0;
        _isAnimating = true;
        if (_mergeTile != null)
        {
            _mergeTile.setPosition(newPos, false);
        }
    }
}
