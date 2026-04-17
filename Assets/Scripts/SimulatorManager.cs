using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimulatorManager : MonoBehaviour // Controlador de todas las entidades y actualización de sus estados
{
    // Elementos para el control del tiempo de la simulación, necesario para la configuración de las entidades
    public float secondsPerIteration = 0.01f;
    private float time = 0f; // Cronómetro interno, este inicia en 0

    // Listas públicas de las entidades
    public List<Alien> aliens = new List<Alien>();
    public List<Soldier> soldiers = new List<Soldier>();
    public List<LaserTower> laserTowers = new List<LaserTower>();

    // Ajustes de población (detectará los prefabs de las entidades para que el usuario pueda agregar los que quiera a la simulación)
    public GameObject alienPrefab;
    public GameObject soldierPrefab;
    public GameObject towerPrefab;
    public int numberAliens;
    public int numberSoldiers;
    public int numberLaserTowers;

    void Start()
    {
        // Primero se crean las entidades
        for(int i = 0; i < numberAliens; i++)
        {
            Instantiate(alienPrefab, Vector3.zero, Quaternion.identity); // Ubica la entidad según las indicaciones de su script
        }

        for (int i = 0; i < numberSoldiers; i++)
        {
            Instantiate(soldierPrefab, new Vector3
                (
                    Random.Range(-20f, 20f),
                    Random.Range(-18f, 18f),
                    0
                ), Quaternion.identity); // Ubica la entidad según las indicaciones de su script
        }

        for (int i = 0; i < numberLaserTowers; i++)
        {
            Instantiate(towerPrefab, new Vector3
                (
                    Random.Range(-20f, 20f),
                    Random.Range(-18f, 18f),
                    0
                ), Quaternion.identity); // Ubica la entidad según las indicaciones de su script
        }

        // Luego, buscará los objetos que tengan el script de la entidad, agregándolos a su respectiva lista
        Alien[] foundAliens = FindObjectsByType<Alien>(FindObjectsSortMode.InstanceID);
        aliens = new List<Alien>(foundAliens);

        Soldier[] foundSoldiers = FindObjectsByType<Soldier>(FindObjectsSortMode.InstanceID);
        soldiers = new List<Soldier>(foundSoldiers);

        LaserTower[] foundLaserTowers = FindObjectsByType<LaserTower>(FindObjectsSortMode.InstanceID);
        laserTowers = new List<LaserTower>(foundLaserTowers);
    }

    void Update()
    {
        // Se acumula el tiempo en segundos que pasó entre el último frame y el actual
        time += Time.deltaTime;

        if (time >= secondsPerIteration) // Si el tiempo transcurrido es mayor o igual al tiempo por interacción definido anteriormente...
        {
            time = 0f; // Entonces se reinicia el cronómetro
            Simulate(); // Además de llamar a la función de simulate
        }

        if (Input.GetMouseButtonDown(0)) // Si presionas clic izquierdo (Botón 0)...
        {
            SpawnEntity(alienPrefab); // Crea la entidad seleccionada en el lugar donde hiciste clic
        }

        if (Input.GetMouseButtonDown(1)) // Si presionas clic derecho (Botón 1)...
        {
            SpawnEntity(soldierPrefab); // Crea la entidad seleccionada en el lugar donde hiciste clic
        }

        if (Input.GetKeyDown(KeyCode.T)) // Si presionas la tecla T...
        {
            SpawnEntity(towerPrefab); // Crea la entidad seleccionada 
        }

        if (Input.GetKeyDown(KeyCode.Space)) // Si presionas la barra espaciadora...
        {
            Time.timeScale = (Time.timeScale == 0) ? 1 : 0;
            Debug.Log(Time.timeScale == 0 ? "PAUSA ACTIVADA" : "SIMULACIÓN REANUDADA"); // Pausará la simulación
        }

        
        if (Input.GetKeyDown(KeyCode.R)) // Si presionas R...
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);// Reinicia la escena
        }
    }

    void SpawnEntity(GameObject prefab) // Desarrolla ciertas funciones según lo programado
    {
        if (prefab == null) return; // Si olvidaste arrastrar el prefab en Unity, no hace nada.

        // Como el mouse se mueve en pixeles por la pantalla, ScreenToWorldPoint convierte esos píxeles en coordenadas del mapa de Unity
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Para evitar que la entidad aparezca en la misma posicion de la camara, se fuerza a esta en permanecer z = 0
        mousePos.z = 0;

        Collider2D hit = Physics2D.OverlapPoint(mousePos); // Para evitar crear entidades en zonas que no estan permitidas
        if (hit != null)
        {
            if(prefab == soldierPrefab || prefab == towerPrefab)
            {
                if (hit.CompareTag("SpaceShip") || hit.CompareTag("Area51"))
                {
                    Debug.Log("<color=red> Zona restringida para humanos</color>");
                    return;
                }
            }

            if (prefab == alienPrefab)
            {
                if (hit.CompareTag("SpaceShip") || hit.CompareTag("DesertZone"))
                {
                    Debug.Log("<color=red> Los aliens no pueden aparecer dentro de la nave ni a las afueras del área 51</color>");
                    return;
                }
            }
        }

        // Crea una copia exacta del prefab en la posición del mouse sin rotación
        GameObject addedEntity = Instantiate(prefab, mousePos, Quaternion.identity);

        // Se le avisa al manager que se incluso una nueva entidad
        SimulatorManager manager = FindFirstObjectByType<SimulatorManager>();
        if (manager != null) 
        {
            if (prefab == soldierPrefab) manager.soldiers.Add(addedEntity.GetComponent<Soldier>()); // Si detecta que se uso clic derecho agregará un soldado con sus funciones
            if (prefab == alienPrefab) manager.aliens.Add(addedEntity.GetComponent<Alien>()); // Si detecta que se uso clic izquierdo agregará un alien con sus funciones
            if (prefab == towerPrefab) manager.laserTowers.Add(addedEntity.GetComponent<LaserTower>()); // Si detecta que se uso la tecla T agregará una torre con sus funciones
        }
    }

    void Simulate()
    {
        // Llamado de todas las funciones Simulate de las entidades
        
        foreach (Alien a in aliens) // a es la variable temporal del alien actual
        {
            if (a != null && a.isAlive) // Por cada alien de la lista que esté vivo...
            {
                a .Simulate(secondsPerIteration); // Realizará su proceso de simulación implementado
            }
        }

        foreach (Soldier s in soldiers) // s es la variable temporal del soldado actual
        {
            if (s != null && s.isAlive) // Por cada soldado de la lista que esté vivo...
            {
                s.Simulate(secondsPerIteration); // Realizará su proceso de simulación implementado
            }
        }

        foreach (LaserTower lt in laserTowers) // lt es la variable temporal de la torre láser actual
        {
            if (lt != null && lt.isStable) // Por cada torre de la lista que esté estable...
            {
                lt.Simulate(secondsPerIteration); // Realizará su proceso de simulación implementado
            }
        }
    }
}
