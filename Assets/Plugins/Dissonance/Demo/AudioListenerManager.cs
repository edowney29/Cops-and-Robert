using UnityEngine;

namespace Dissonance.Demo
{
    /// <summary>
    /// The body of the demo player has a _disabled_ AudioSource attached. This script toggles it on (and all others off) if it can determine that this is the local player
    /// </summary>
    public class AudioListenerManager
        : MonoBehaviour
    {
        private IDissonancePlayer _player;
        private AudioListener _listener;

        private void OnEnable()
        {
            // Try to find the sibling AudioListener, if there is not one then disable this script
            _listener = GetComponent<AudioListener>();
            if (_listener == null)
            {
                enabled = false;
                return;
            }

            // Try to find the `IDissonancePlayer`, if there is not one then disable this script
            _player = GetComponentInParent<IDissonancePlayer>();
            if (_player == null)
            {
                enabled = false;
                return;
            }
        }

        private void Update()
        {
            if (!_player.IsTracking)
                return;

            if (_player.Type == NetworkPlayerType.Unknown)
                return;

            if (_player.Type == NetworkPlayerType.Remote)
            {
                enabled = false;
                return;
            }

            // This is the local player. Disable all other AudioListeners
            var listeners = FindObjectsOfType<AudioListener>();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].enabled = false;
            _listener.enabled = true;
        }
    }
}
