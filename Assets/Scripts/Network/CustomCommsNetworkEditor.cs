#if UNITY_EDITOR
using Dissonance;
using UnityEditor;

[CustomEditor(typeof(CustomCommsNetwork))]
public class CustomCommsNetworkEditor : Dissonance.Editor.BaseDissonnanceCommsNetworkEditor<
        CustomCommsNetwork,
        CustomServer,
        CustomClient,
        CustomConn,
        Unit,
        Unit
    >
{
}
#endif