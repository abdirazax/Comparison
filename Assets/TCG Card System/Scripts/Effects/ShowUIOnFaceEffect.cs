using UnityEngine;

namespace TCG_Card_System.Scripts.Effects
{
    public class ShowUIOnFaceEffect : MonoBehaviour
    {
        private static Camera MainCamera => Camera.main;
        
        private Mesh _mesh;
        private bool? _visible;
        
        private void Awake()
        {
            // _mesh = GetComponent<MeshFilter>().mesh;
        }
        
        private void Update()
        {
            // if (!MainCamera)
            //     return;
            //
            // var toCamera = MainCamera.transform.position - transform.position;
            //
            // // Assuming the gameObject has a MeshFilter component with a mesh
            // var faceNormal = _mesh.normals[0]; // Example: using the first normal
            //
            // // Transform the normal to world space
            // var worldNormal = transform.TransformDirection(faceNormal);
            //
            // var facing = Vector3.Dot(worldNormal, toCamera) > 0;
            //
            // if (facing == _visible)
            //     return;
            //
            // _visible = facing;
            // SetVisibility();
        }
        
        private void SetVisibility()
        {
            foreach (var r in GetComponentsInChildren<Renderer>())
                if (gameObject != r.gameObject)
                    r.enabled = _visible ?? false;
        }
    }
}