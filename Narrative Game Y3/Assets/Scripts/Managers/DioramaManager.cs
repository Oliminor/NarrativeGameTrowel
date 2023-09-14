using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class DioramaManager : MonoBehaviour
{
    public static DioramaManager instance;

    [SerializeField] private Transform animationTransform;

    [Header("Sound")]
    [SerializeField] AudioSource dioramaAudioSource;
    [SerializeField] AudioClip scrapeSound;

    [SerializeField] float playbackVolume = 1f;
    [SerializeField] float playbackDelay = 1f;

    private Animator anim;
    private Transform newDiorama;

    public Transform GetAnimationTransform() { return animationTransform; }
    public Transform GetCurrentDiorama() { return animationTransform.GetChild(0); }

    void Awake()
    {
        if (instance != null) Debug.Log("Error: There are multiple instances exits at the same time (DioramaManager)");
        instance = this;
    }

    private void Start()
    {
        Initialize();
    }
    private void Initialize()
    {
        anim = GetComponent<Animator>();
        GetCurrentDiorama().GetComponent<Diorama>().ChangeDioramaLight();
    }

    /// <summary>
    ///  Set the new Diorama and starts the changing Coroutine 
    /// </summary>
    public void TriggerDioramaAnimation(Transform _newDiorama)
    {
        newDiorama = _newDiorama;
        newDiorama.GetComponent<Diorama>().ChangeDioramaLight();
        StartCoroutine(DioramaAnimation());
    }

    /// <summary>
    ///  Triggers the Diorama changing animation after the Map -> Table camera transition ends
    /// </summary>
    private IEnumerator DioramaAnimation()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        while (NavigationCamera.instance.IsCameraTransitionOver()) yield return null;

        HandManager.instance.StartSwapDioramaHandPosition();

        yield return new WaitForSecondsRealtime(0.1f);
        while (HandManager.instance.IsAnimating()) yield return null;

        anim.SetTrigger("ChangeDiorama");
    }

    public void SetHandsBackToTable()
    {
        HandManager.instance.BackToTable();
    }

    /// <summary>
    ///  When the Diarama is under the table the old diorama disabled and the new enabled
    /// </summary>
    public void ChangeDiorama()
    {
        HandManager.instance.ChangeHandParent(transform.root, HandManager.instance.GetLeftHand());
        HandManager.instance.ChangeHandParent(transform.root, HandManager.instance.GetRightHand());

        animationTransform.GetChild(0).gameObject.SetActive(false);
        animationTransform.GetChild(0).SetParent(transform);

        newDiorama.SetParent(animationTransform);
        newDiorama.gameObject.SetActive(true);

        StartCoroutine(ParentSwap());
    }

    IEnumerator ParentSwap()
    {
        yield return new WaitForSeconds(0.1f);
        HandManager.instance.ChangeHandParent(newDiorama.GetComponent<Diorama>().GetLeftHandOffset(), HandManager.instance.GetLeftHand());
        HandManager.instance.ChangeHandParent(newDiorama.GetComponent<Diorama>().GetRightHandOffset(), HandManager.instance.GetRightHand());
        yield return new WaitForSecondsRealtime(playbackDelay);
        dioramaAudioSource.PlayOneShot(scrapeSound, playbackVolume);
    }

    /// <summary>
    ///  Checks if the animation is over
    /// </summary>
    public bool IsDioramaAnimationOver()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsTag("DioramaAnimation");
    }
}
