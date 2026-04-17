using UnityEngine;

public class LaserTower : MonoBehaviour
{
    [Header("LaserTower Settings")]
    public float resistance = 150f; // Resistencia de la torre
    public float energy = 100f; // Energía de la torre (No recargable)
    public float visionRange = 10f; // Alcance de detección y disparo
    public float damage = 15f;
    public int munition = 40; // Munición estándar del soldado, sin embargo, al inicio, la cantidad de munición será al azar (sin salirse de este rango)

    [Header("Alien visual")]
    public Sprite spriteNormal;
    public Sprite spriteDestroyed;
    private SpriteRenderer sr;

    [Header("LaserTower States")]
    public bool isStable = true;

    private float h; // h es el tiempo que dura cada paso de la simulación, esto viene de SimulationManager.secondsPerIteration =1f;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Simulate(float h)
    {
        // Ajuste principal de los estados y acciones de la torre láser (base)

        if (!isStable) return; // Si la torre láser fue destruida (no es estable) entonces no hace nada

        this.h = h;

        CheckStateTower(); // Revisa el estado de la torre
    }

    void CheckStateTower()
    {
        //Si la torre tiene una resistencia menor o igual a 0...
        if (resistance <= 0)
        {
            isStable = false; // Entonces la torre fue destruida
            sr.sprite = spriteDestroyed;
            Debug.Log("<color=red> LA TORRE HA SIDO DESTRUIDA :( </color>");
        } else 
        {
            sr.sprite = spriteNormal;
            ScanPerimeter(); // Si no, entonces se pondrá a vigilar la zona
        }
    }

    void ScanPerimeter()
    {
        // Durante su vigilancia detecta la presencia de algo
        Collider2D something = Physics2D.OverlapCircle(transform.position, visionRange);

        if (something != null)
        {
            if (something.CompareTag("Alien")) // Si lo que detecta es un alien...
            {
                Debug.Log("<color=Amarillo> Alien detectado. Comenzando prótocolo de exterminio alienígena</color>");
                AttackAlien(something.gameObject); // Procede a realizar el protocólo de exterminio de aliens
            }
            else
            if (something.CompareTag("Soldier"))
            {
                Debug.Log("Soldado detectado. Protocólo de revisión de estado...");
                CheckSoldier(something.gameObject);
            }
        }
        else 
        {
            Debug.Log("<color=cian> Perímetro despejado</color>");
        }
    }

    void AttackAlien(GameObject something)
    {
        // Antes de atacar revisa la energía disponible para el disparo
        if (energy > 0)
        {
            if (munition > 0) // Luego revisa si tiene municiones para atacar
            {
                // Se crea una variable temporal para acceder a la información de salud actual del alien, de esta manera se define hasta que momento se atacará
                Alien scriptAlien = something.GetComponent<Alien>();
                if (scriptAlien != null && scriptAlien.isAlive)
                {
                    scriptAlien.TakeDamage(damage); // Se le otorga los puntos de daño que el soldado hace
                    munition--; // Gastando munición en el proceso
                    energy -= 2f; // Gastando energía en el proceso
                    Debug.Log("Torre en modo ataque. Energía restante: " + energy + " Balas restantes: " + munition);
                }
            }
            else // Si no tiene munición...
            {
                Debug.Log("<color=orange> La torre se quedó sin recursos para atacar.</color>");
            }
        }
        else // Si no tiene energía...
        {
            Debug.Log("<color=orange> La torre se quedó sin energía para atacar.</color>");
        }
    }

    void CheckSoldier(GameObject something)
    {
        // Se crea una variable temporal para acceder a la información actual del soldado, de esta manera se define que recurso necesita aumentar
        Soldier scriptSoldier = something.GetComponent<Soldier>();

        // Se verifica que el script exista y el soldado esté vivo
        if (scriptSoldier != null && scriptSoldier.isAlive)
        {
            if (scriptSoldier.health < 30 || scriptSoldier.munition <= 4) // Si el soldado necesita ayuda...
            {
                if (energy >= 15) // Pregunta si la torre tiene la energía para sanar o dar municiones al soldado...
                {
                    Debug.Log("<color=green> Iniciando reabastecimiento del soldado... </color>");

                    if (scriptSoldier.health < 30 && energy >= 10)
                    {
                        scriptSoldier.health = 100f; // Si la salud del soldado está critica y la torre tiene los recursos, le recupera ese dato al soldado
                        energy -= 10; // Quitándole energía en el proceso
                    }

                    if (scriptSoldier.munition <= 4 && energy >= 5)
                    {
                        scriptSoldier.munition = 15; // Si la cantidad de municiones del soldado está critica y la torre tiene los recursos, le recupera ese dato al soldado
                        energy -= 5; // Quitándole energía en el proceso
                    }

                    Debug.Log("<color=green> Reabastecimiento de soldado completado. Energía actual de la torre:  </color>" + energy);
                    return;
                }
                else
                {
                    Debug.Log("<color=red>Energía insuficiente para ayudar al soldado</color>");
                }
            }
        }
    }

    public void TakeDamageTower(float quantifyDamage)
    {
        // Cuando la torre es atacada, va reduciendo la resistencia
        if (isStable)
        {
            resistance -= quantifyDamage; // quantifyDamage es quien recibe el número de resistencia que el alien le quita
            Debug.Log("<color=orange> Torre bajo ataque!!! Resistencia actual: </color>" + resistance);
        }

    }

    private void OnDrawGizmosSelected()
    {
        // Vista de la torre representada en scene

        // Área despejada
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // Obstáculo (Amenaza) detectada
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // Indicación de dirección en la que va
        Gizmos.color = Color.yellow;
        //Gizmos.DrawLine(transform.position, );
    }
}
