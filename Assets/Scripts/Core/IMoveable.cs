using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//basic movement methods for any moving actor
public interface IMoveable{
    void Move(Vector3 direction);
    void Stop();
}
