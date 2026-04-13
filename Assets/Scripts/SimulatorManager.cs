using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SimulatorManager : MonoBehaviour // Controlador de todas las entidades y actualización de sus estados
{
    // Elementos para el control del tiempo de la simulación, necesario para la configuración de las entidades
    public float secondsPerIteration = 1.0f;
    private float time = 0f; // Cronómetro interno, este inicia en 0

    // Listas públicas de las entidades
    public List<Alien> aliens = new List<Alien>();
    public List<Soldier> soldiers = new List<Soldier>();
    public List<LaserTower> laserTowers = new List<LaserTower>();

    //Ajustes de población (detectará los prefabs de las entidades para que el usuario pueda agregar los que quiera a la simulación)
    public GameObject alienPrefab;
    public GameObject soldierPrefab; 
    public int numberAliens;
    public int numberSoldiers;

    void Start()
    {
        // Primero se crean las entidades
        for(int i = 0; i < numberAliens; i++)
        {
            Instantiate(alienPrefab, Vector3.zero, Quaternion.identity); // Ubica la entidad según las indicaciones de su script
        }

        for (int i = 0; i < numberSoldiers; i++)
        {
            Instantiate(soldierPrefab, Vector3.zero, Quaternion.identity); // Ubica la entidad según las indicaciones de su script
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
