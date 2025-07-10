using Platformer.Gameplay;
using Platformer.Mechanics;
using UnityEngine;
using UnityEngine.Rendering;
using static Platformer.Core.Simulation;



namespace Platformer.Mechanics
{

    public class CarSpawner : MonoBehaviour
    {
        [Header("Car Prefabs")]
        public GameObject[] carPrefabs; // Collection of car prefabs 

        [Header("Audio")]
        public AudioClip[] honkSounds; // Collection of honk sound clips 
        public float honkVolume = 0.2f;
        private AudioSource audioSource;

        [Header("Spawn Settings")]
        public float minSpawnTime = 2f;
        public float maxSpawnTime = 5f;
        public float spawnXOffset = 12f; // How far off-screen to spawn
        public float roadYPosition = -2f; // Y position of the road
        [Range(0f, 1f)]
        public float rightDirectionChance = 0.3f; // Chance car goes right instead of left
        public float rightDirectionYOffset = -0.5f; // Additional Y offset for cars going right
        public int leftCarsSortingOrder = 0; // Sorting order for cars going left
        public int rightCarsSortingOrder = 1; // Sorting order for cars going right (higher = in front)

        private float nextSpawnTime;
        private int carSpawnCount = 0; // Track number of cars spawned

        void Start()
        {
            SetNextSpawnTime();

            // Get or add AudioSource component
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        void Update()
        {
            if (Time.time >= nextSpawnTime)
            {
                SpawnRandomCar();
                SetNextSpawnTime();
            }
        }

        void SpawnRandomCar()
        {
            if (carPrefabs.Length == 0) return;

            // Increment car spawn counter
            carSpawnCount++;

            // Pick random car prefab
            int randomIndex = Random.Range(0, carPrefabs.Length);
            GameObject carPrefab = carPrefabs[randomIndex];

            // Randomly decide direction
            bool goingRight = Random.value < rightDirectionChance;

            // Get the car's individual Y offset
            CarController carController = carPrefab.GetComponent<CarController>();
            float carYOffset = carController ? carController.yOffset : 0f;

            // Spawn position depends on direction
            Vector3 spawnPosition;
            if (goingRight)
            {
                // Spawn on left side for cars going right (with additional Y offset)
                spawnPosition = new Vector3(
                    Camera.main.transform.position.x - spawnXOffset,
                    roadYPosition + carYOffset + rightDirectionYOffset,
                    0f
                );
            }
            else
            {
                // Spawn on right side for cars going left (default lane)
                spawnPosition = new Vector3(
                    Camera.main.transform.position.x + spawnXOffset,
                    roadYPosition + carYOffset,
                    0f
                );
            }

            // Spawn the car
            GameObject newCar = Instantiate(carPrefab, spawnPosition, Quaternion.identity);

            // Set direction and flip sprite if going right
            CarController newCarController = newCar.GetComponent<CarController>();
            SpriteRenderer spriteRenderer = newCar.GetComponent<SpriteRenderer>();

            if (newCarController)
            {
                newCarController.goingRight = goingRight;

                // Flip sprite and set sorting order for cars going right
                if (goingRight)
                {
                    if (spriteRenderer)
                    {
                        spriteRenderer.flipX = true;
                        spriteRenderer.sortingOrder = rightCarsSortingOrder;
                        newCarController.speed += 6f; // Increase speed for right-going cars
                        Debug.Log("New car speed: " + newCarController.speed);
                    }
                }
                else
                {
                    // Set sorting order for cars going left
                    if (spriteRenderer)
                    {
                        spriteRenderer.sortingOrder = leftCarsSortingOrder;
                    }
                }
            }

            // Play honk sound every 2nd car
            Debug.Log("Car spawn count: " + carSpawnCount);
            if (carSpawnCount % 2 == 0)
            {
                PlayRandomHonk();
            }
        }

        void PlayRandomHonk()
        {
            //Debug.Log("PlayRandomHonk called");
            //Debug.Log("audioSource is null: " + (audioSource == null));
            //Debug.Log("honkSounds.Length: " + honkSounds.Length);

            if (honkSounds.Length == 0 || audioSource == null) return;

            // Pick random honk sound
            int randomHonkIndex = Random.Range(0, honkSounds.Length);
            AudioClip randomHonk = honkSounds[randomHonkIndex];

            Debug.Log($"Playing sound: [{(randomHonk != null ? randomHonk.name : "NULL")}]");

            // Play the sound
            audioSource.volume = honkVolume;
            audioSource.PlayOneShot(randomHonk);

            // Debug.Log("PlayOneShot called");
            // audioSource.clip = randomHonk;
            // audioSource.Play();
        }

        void SetNextSpawnTime()
        {
            float randomDelay = Random.Range(minSpawnTime, maxSpawnTime);
            nextSpawnTime = Time.time + randomDelay;
        }
    }
}