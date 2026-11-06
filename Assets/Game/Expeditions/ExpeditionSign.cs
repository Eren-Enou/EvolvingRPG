using UnityEngine;
namespace EvolvingRPG.Expeditions
{
    public sealed class ExpeditionSign : MonoBehaviour
    {
        void LateUpdate() { if (Camera.main != null) transform.rotation = Camera.main.transform.rotation; }
    }
}
