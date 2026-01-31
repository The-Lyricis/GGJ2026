
using System.Collections.Generic;
using UnityEngine;


namespace GGJ2026
{
    public class DoorMarker : MonoBehaviour
    {
        public List<string> listenIds = new();
        [HideInInspector]
        public Vector2Int cell;
        public bool startClosed = true;
    }

}