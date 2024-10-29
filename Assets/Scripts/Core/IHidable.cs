using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//interface allow acgor to affect detection
public interface IHideable{
    bool isHiding {get;}
    void Hide();
    void ExitHide();
}