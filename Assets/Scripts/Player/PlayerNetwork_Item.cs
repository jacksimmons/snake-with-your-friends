using Mirror;
using UnityEngine;

public partial class PlayerNetwork : NetworkBehaviour
{
    // Constants
    private const float PROJ_SPEED_FAST = 0.25f;
    private const float PROJ_SPEED_SLOW = 0.1f;

    [SerializeField]
    private GameObject m_fbProjectile;
    [SerializeField]
    private GameObject m_shitProjectile;

    [Range(0f, 360f)]
    // An angle either side of the player defining the random range of RocketShitting.
    // Recommended range: 0-90. Past 90 will give very shitty results.
    private const float SHIT_EXPLOSIVENESS = 45;

    private PlayerBehaviour m_player;


    private void Start()
    {
        PlayerObjectController lpo = GameObject.Find("LocalPlayerObject").GetComponent<PlayerObjectController>();
        m_player = CustomNetworkManager.Instance.Players[lpo.playerNo - 1].GetComponent<PlayerBehaviour>();
    }


    public void Spawn(EEffect effect)
    {
        if (isOwned)
            CmdSpawn(effect);
    }


    /// <summary>
    /// Handles spawning of projectiles, determined by the effect enum passed.
    /// Some objects are synced with the server, some just have synced spawn times.
    /// </summary>
    /// <param name="effect">The projectile is based on the effect.</param>
    [Command]
    private void CmdSpawn(EEffect effect)
    {
        switch (effect)
        {
            case EEffect.RocketShitting:
                ClientSpawnUnsynced(effect);
                break;
            case EEffect.BreathingFire:
                BodyPart head = m_player.BodyParts[0];

                GameObject fireball = Instantiate(m_fbProjectile, GameObject.Find("Projectiles").transform);
                fireball.transform.position = head.Position + (Vector3)head.Direction;
                fireball.GetComponent<ProjectileBehaviour>()
                .Proj = Projectiles.ConstructFireball(
                    head.Direction * PROJ_SPEED_FAST,
                    head.RegularAngle);

                NetworkServer.Spawn(fireball);
                break;
        }
    }


    /// <summary>
    /// Spawns an unsynced object, at a synced time (as every client does the same
    /// thing).
    /// </summary>
    /// <param name="effect">The projectile is based on the effect.</param>
    [ClientRpc]
    private void ClientSpawnUnsynced(EEffect effect)
    {
        switch (effect)
        {
            case EEffect.RocketShitting:
                float randomRotation = Random.Range(-SHIT_EXPLOSIVENESS, SHIT_EXPLOSIVENESS);

                GameObject shit = Instantiate(m_shitProjectile, GameObject.Find("Projectiles").transform);
                shit.transform.position = m_player.BodyParts[^1].Position - (Vector3)m_player.BodyParts[^1].Direction;
                shit.transform.Rotate(Vector3.forward * randomRotation);

                shit.GetComponent<ProjectileBehaviour>()
                .Proj = Projectiles.ConstructShit(
                    Extensions.Vectors.Rotate(-m_player.BodyParts[^1].Direction, randomRotation) * PROJ_SPEED_SLOW,
                    m_player.BodyParts[^1].RegularAngle);
                break;
        }
    }
}