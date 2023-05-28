using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class MoveCounter : MonoBehaviour
{
    private TMP_Text _Text;
    private 
    void Awake()
    {
       _Text = GetComponent<TMP_Text>();
    }

    public void UpdateCount(int moveCount)
    {
        bool shouldDisplayPlural = moveCount != 1;
        _Text.text = $"{moveCount} {(shouldDisplayPlural ? "moves" : "move")}";

    }
}
