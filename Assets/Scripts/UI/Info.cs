using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Info : MonoBehaviour
{
    public TMP_Text Header;
    public TMP_Text Body;
    public float UpdateCooldown;
    private Func<string[]> _infoFunction;
    private float _updateCooldown;

    void Update()
    {
        _updateCooldown -= Time.deltaTime;
        if (_updateCooldown < 0)
        {
            _updateCooldown = UpdateCooldown;
            ChangeInfo(_infoFunction());
        }
    }

    public void ChangeInfoSource(Func<string[]> func)
    {
        _infoFunction = func;
        ChangeInfo(_infoFunction());
    }

    public void ChangeInfo(string[] info)
    {
        ChangeInfo(info[0], info[1]);
    }

    public void ChangeInfo(string header, string body)
    {
        gameObject.SetActive(true);
        Header.text = header;
        Body.text = body;
    }
}
