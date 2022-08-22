using System;
using TicTacToeLib.Game;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public GameObject Cross;
    public GameObject Nought;

    public int Row { get; private set; }
    public int Col { get; private set; }

    private event Action<Cell> onCellClick;

    public void Init(int row, int col, Action<Cell> onCellClickAction)
    {
        Row = row;
        Col = col;

        name = string.Format("Cell [{0}, {1}]", row, col);

        onCellClick = onCellClickAction;
    }

    // Unity component lifecycle

    void Start()
    {
        Cross.SetActive(false);
        Nought.SetActive(true);
    }
    
    public void OnMouseDown()
    {
        if (onCellClick != null)
        {
            onCellClick(this);
        }
    }

    // UI state representation

    public void SetSeed(Seed seed)
    {
        switch (seed)
        {
            case Seed.None:
                {
                    Cross.SetActive(false);
                    Nought.SetActive(false);
                    break;
                }
            case Seed.Nought:
                {
                    Cross.SetActive(false);
                    Nought.SetActive(true);
                    break;
                }
            case Seed.Cross:
                {
                    Cross.SetActive(true);
                    Nought.SetActive(false);
                    break;
                }
        }
    }
}
