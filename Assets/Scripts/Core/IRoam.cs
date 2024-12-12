using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Survivors and Priest, roaming behavior
public interface IRoam{
    void StartRoaming();
    void StopRoaming();
    void SetRoamArea(Vector3 center, float radius);
}