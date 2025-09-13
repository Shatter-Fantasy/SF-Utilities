using UnityEngine;
using UnityEngine.Rendering;

namespace SF.Utilities.Rendering
{
    public class WaveTesting : MonoBehaviour
    {
        [SerializeField] private Material _surfaceMaterial;
        private void Start()
        {
            if (_surfaceMaterial == null)
                return;
        }
    }
}
