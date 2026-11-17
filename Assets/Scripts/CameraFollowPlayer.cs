using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
   private GameObject _player;

   private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        FollowPlayer();
    }

    private void FollowPlayer()
    {
        if (_player != null)
        {
            Vector3 newPosition = _player.transform.position;
            transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
        }
    }
}
