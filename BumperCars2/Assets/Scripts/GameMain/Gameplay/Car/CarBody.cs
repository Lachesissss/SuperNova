using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Lachesis.GamePlay
{
    public class CarBody : MonoBehaviour
    {
       [FormerlySerializedAs("carController")] public CarComponent carComponent;
    }
}

