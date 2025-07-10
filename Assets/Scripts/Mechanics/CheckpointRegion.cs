using UnityEngine;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Updates the spawn point when the player enters this region.
    /// Attach this to a GameObject with a 2D trigger collider.
    /// </summary>
    public class CheckpointRegion : MonoBehaviour
    {
        public SpawnPoint regionSpawnPoint;

        void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null && regionSpawnPoint != null)
            {
                var model = Platformer.Core.Simulation.GetModel<Platformer.Model.PlatformerModel>();
                model.spawnPoint = regionSpawnPoint.transform; 
            }
        }
    }
}