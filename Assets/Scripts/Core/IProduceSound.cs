using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProduceSound{
    float NoiseLevel { get; }
    void EmitSound();
    GameObject GameObject {get;}
}
