
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGJ2026
{
    public class DoorMarker : MonoBehaviour
    {
        public string id;
        [HideInInspector]
        public Vector2Int cell;
        public bool startClosed = true;
    }

}