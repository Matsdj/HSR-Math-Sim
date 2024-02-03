using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Combat : MonoBehaviour
{
    public Character[] Allies;
    public Character[] Enemies;
    public Queue<Character> ActionOrder;
    public List<string> ActionHistory;
    public HorizontalLayoutGroup AllyUI;
    public HorizontalLayoutGroup EnemyUI;
    public VerticalLayoutGroup ActionOrderUI;
    public VerticalLayoutGroup ActionHistoryUI;

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
