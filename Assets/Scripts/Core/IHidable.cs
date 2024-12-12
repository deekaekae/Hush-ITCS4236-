using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//interface allow player to affect detection (hold breath)
public interface IHideable{
    bool isHiding {get;}
    void Hide();
    void ExitHide();
}