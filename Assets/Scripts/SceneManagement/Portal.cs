using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] int sceneToLoad;
    [SerializeField] DestinationIdentifier destinationPortal;
    [SerializeField] Transform spawnPoint;

    PlayerController player;

    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        this.player = player;
        StartCoroutine(SwitchScene());
    }

    Fader fader;
    private void Start()
    {
        fader = FindObjectOfType<Fader>();
    }

    IEnumerator SwitchScene()
    {
        DontDestroyOnLoad(gameObject); // Ensures remaining code will be ran in this function
        GameController.Instance.PauseGame(true); // Prevent player from acting when paused, e.g. scene is loading
        yield return fader.FadeIn(0.5f);
        yield return SceneManager.LoadSceneAsync(sceneToLoad);
        var destPortal = FindObjectsOfType<Portal>().First(x => x != this && x.destinationPortal == this.destinationPortal);
        player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);
        yield return fader.FadeOut(0.5f);
        GameController.Instance.PauseGame(false); // Give control back to player;
        Destroy(gameObject);

    }

    public Transform SpawnPoint => spawnPoint;
}

public enum DestinationIdentifier { A, B, C, D, E }
