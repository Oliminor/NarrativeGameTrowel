using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager instance;

    GameManager.GameStatus prevStatus;
    GameManager.GameStatus gameStatus;
    bool isAnimating;

    [Header ("Character Positions")]
    [SerializeField] Transform detectivePiece;
    [SerializeField] Transform nPCSSpotlightPos;
    [SerializeField] Transform detectiveSpotlightPos;

    [Header("Hand Instances")]
    [SerializeField] GameObject LeftHand;
    [SerializeField] GameObject RightHand;

    [Header("Available Positions")]
    [SerializeField] Transform leftRestPos;
    [SerializeField] Transform rightRestPos;
    [SerializeField] Transform telephonePos;
    [SerializeField] Transform newsPaperPos;

    [SerializeField] Transform detectivePieceOffsetPos;

    [SerializeField]
    float pawnPivotOffset = -0.3f;

    [Header("Sound")]
    [SerializeField] AudioSource handAudioSource;
    [SerializeField] AudioClip pickupSound;
    [SerializeField] AudioClip boardPickupSound;

    [SerializeField] float playbackVolume = 1f;


    Vector3 tempNPCPos;
    Vector3 tempDetectivePos;

    IEnumerator upAndDownIE;

    float pieceStartY;
    Transform currentMovingPiece;

    // animator variables
    int animIDSwitching, animIDHoldingPawn;
    Animator leftHandAnimator, rightHandAnimator;

    public bool IsAnimating() { return isAnimating; }
    public Transform GetRightHand() { return RightHand.transform; }
    public Transform GetLeftHand() { return LeftHand.transform; }
    public void SetPrevStatus(GameManager.GameStatus _prevStatus) { prevStatus = _prevStatus; }

    private void Awake()
    {
        if (instance != null) Debug.Log("Error: There are multiple instances exits at the same time (HandManager)");
        instance = this;
    }

    void Start()
    {
        GameManager.instance.onStatusUpdated += UpdateHands;

        leftHandAnimator = LeftHand.GetComponent<Animator>();
        rightHandAnimator = RightHand.GetComponent<Animator>();
        AssignAnimationIDs();
    }

    void AssignAnimationIDs()
    {
        animIDSwitching = Animator.StringToHash("Switching");
        animIDHoldingPawn = Animator.StringToHash("Holding Pawn");
    }

    private void UpdateHands()
    {
        gameStatus = GameManager.instance.GetStatus();  

        switch (gameStatus)
        {
            case GameManager.GameStatus.None:
                break;
            case GameManager.GameStatus.Table:
                AnimationUpdate();
                break;
            case GameManager.GameStatus.Map:
                break;
            case GameManager.GameStatus.Diorama:
                BackToTable();
                break;
            case GameManager.GameStatus.Newspaper:
                StartCoroutine(ChangeHandPosition(newsPaperPos, RightHand.transform));
                break;
            case GameManager.GameStatus.PlayingCard:
                break;
            case GameManager.GameStatus.InspectEvidence:
                break;
            case GameManager.GameStatus.Dialogue:
                leftHandAnimator.SetBool(animIDHoldingPawn, true);
                rightHandAnimator.SetBool(animIDHoldingPawn, true);
                StartCoroutine(ChangeHandPosition(detectivePieceOffsetPos, LeftHand.transform));
                StartCoroutine(ChangeHandPosition(GameManager.instance.GetSelectedNPC().GetHandOffsetPos(), RightHand.transform));
                break;
        }
    }

    private void AnimationUpdate()
    {
        switch (prevStatus)
        {
            case GameManager.GameStatus.None:
                break;
            case GameManager.GameStatus.Table:
                break;
            case GameManager.GameStatus.Map:
                break;
            case GameManager.GameStatus.Diorama:
                break;
            case GameManager.GameStatus.Newspaper:
                StartCoroutine(PutDownNewsPaper());
                break;
            case GameManager.GameStatus.PlayingCard:
                break;
            case GameManager.GameStatus.InspectEvidence:
                break;
            case GameManager.GameStatus.Dialogue:
                break;
            default:
                break;
        }
    }

    private void AdditionalTrigger()
    {
        switch (gameStatus)
        {
            case GameManager.GameStatus.None:
                break;
            case GameManager.GameStatus.Table:
                break;
            case GameManager.GameStatus.Map:
                break;
            case GameManager.GameStatus.Diorama:
                break;
            case GameManager.GameStatus.Newspaper:
                NewsPaper.instance.PickUpNewspaper();
                break;
            case GameManager.GameStatus.PlayingCard:
                break;
            case GameManager.GameStatus.InspectEvidence:
                break;;
            case GameManager.GameStatus.Dialogue:
                BlackBars.instance.BlackBarOn();
                Lamp.instance.ChangeLampTarget(LampTarget.DIORAMA);
                MoveDetectivePieceToSpotlight();
                MoveNPCPieceToSpotlight();
                break;
            default:
                break;
        }
    }

    public void StartSwapDioramaHandPosition()
    {
        leftHandAnimator.SetBool(animIDSwitching, true);
        rightHandAnimator.SetBool(animIDSwitching, true);
        StartCoroutine(ChangeHandPosition(DioramaManager.instance.GetCurrentDiorama().GetComponent<Diorama>().GetLeftHandOffset(), LeftHand.transform, boardPickupSound));
        StartCoroutine(ChangeHandPosition(DioramaManager.instance.GetCurrentDiorama().GetComponent<Diorama>().GetRightHandOffset(), RightHand.transform, boardPickupSound));
    }

    IEnumerator PutDownNewsPaper()
    {
        NewsPaper.instance.PutDownNewspaper();

        yield return new WaitForSeconds(0.1f);

        while (NewsPaper.instance.IsAnimationOver()) yield return null;

        StartCoroutine(ChangeHandPosition(rightRestPos, RightHand.transform));
    }

    public void PickUpTelephone()
    {
        StartCoroutine(PickUpPhone());
    }

    public void PutDownTelephone()
    {
        StartCoroutine(PutDownPhone());
    }

    IEnumerator PickUpPhone()
    {
        StartCoroutine(ChangeHandPosition(telephonePos, LeftHand.transform));

        yield return new WaitForSeconds(0.1f);

        while (isAnimating) yield return null;

        Telephone.instance.PickUpPhone();
    }

    IEnumerator PutDownPhone()
    {
        Telephone.instance.PutDownpPhone();

        yield return new WaitForSeconds(0.1f);

        while (Telephone.instance.IsAnimationOver()) yield return null;

        StartCoroutine(ChangeHandPosition(leftRestPos, LeftHand.transform));
    }

    public void BackToTable()
    {
        leftHandAnimator.SetBool(animIDHoldingPawn, false);
        rightHandAnimator.SetBool(animIDHoldingPawn, false);
        leftHandAnimator.SetBool(animIDSwitching, false);
        rightHandAnimator.SetBool(animIDSwitching, false);
        StartCoroutine(ChangeHandPosition(leftRestPos, LeftHand.transform));
        StartCoroutine(ChangeHandPosition(rightRestPos, RightHand.transform));
    }

    public void ChangeHandParent(Transform _to, Transform _hand)
    {
        _hand.SetParent(_to);
        _hand.transform.localPosition = Vector3.zero;
        _hand.transform.localRotation = Quaternion.identity;
    }

    public void MovePiecesBack()
    {
        StartCoroutine(ChangePiecePosition(tempNPCPos, GameManager.instance.GetSelectedNPC().transform, pickupSound, 2));
        StartCoroutine(ChangePiecePosition(tempDetectivePos, detectivePiece, pickupSound, 2));
    }

    public void MoveNPCPieceToSpotlight()
    {
        tempNPCPos = GameManager.instance.GetSelectedNPC().transform.position;
        StartCoroutine(ChangePiecePosition(nPCSSpotlightPos.position + transform.up * pawnPivotOffset, GameManager.instance.GetSelectedNPC().transform, pickupSound, 1));
    }

    public void MoveDetectivePieceToSpotlight()
    {
        tempDetectivePos = detectivePiece.position;
        StartCoroutine(ChangePiecePosition(detectiveSpotlightPos.position + transform.up * pawnPivotOffset, detectivePiece, pickupSound, 1));
    }

    public void StopUpAndDownMovement()
    {
        if (upAndDownIE == null) return;

        currentMovingPiece.position = new Vector3(currentMovingPiece.position.x, pieceStartY, currentMovingPiece.position.z);

        StopCoroutine(upAndDownIE);
    }

    public void MovePiecesUpAndDown(int _targetIndex)
    {
        upAndDownIE = MovePiecesUpAndDownEnume(_targetIndex);
        StartCoroutine(upAndDownIE);
    }

    private Transform GetPieceTransform(int _targetIndex)
    {
        Transform _piece = transform;

        switch (_targetIndex)
        {
            case 0:
                _piece = detectivePiece;
                break;
            case 1:
                _piece = GameManager.instance.GetSelectedNPC().transform;
                break;
        }

        currentMovingPiece = _piece;

        return _piece;
    }

    IEnumerator MovePiecesUpAndDownEnume(int _targetIndex)
    {
        yield return new WaitForSeconds(0.1f);

        Transform _piece = GetPieceTransform(_targetIndex);

        float offset = 0.1f;
        float frequency = 20;
        pieceStartY = _piece.position.y;
        float time = 0.0f;

        while (NarrativeGame.Dialogue.PlayerConversant.instance.GetIsAudioPlaying() || !NarrativeGame.UI.DialogueUI.instance.GetIsDialogueFinished() || pieceStartY > _piece.position.y + 0.005f || pieceStartY < _piece.position.y - 0.005f)
        {
            yield return new WaitForSeconds(Time.deltaTime);

            float sin = Mathf.Sin(frequency * time);

            time += Time.deltaTime;

            float newY = pieceStartY + offset * sin;

            _piece.position = new Vector3(_piece.position.x, newY, _piece.position.z);
        }

        _piece.position = new Vector3(_piece.position.x, pieceStartY, _piece.position.z);
        upAndDownIE = null;
    }

    IEnumerator ChangeHandPosition(Transform _to, Transform _hand)
    {
        if (_to == _hand.parent) yield break;

        isAnimating = true;

        _hand.SetParent(null);

        float lerp = 0;

        Vector3 startPos = _hand.position;
        Vector3 endPos = _to.position;

        Vector3 startAngle = _hand.rotation.eulerAngles;
        Vector3 endAngle = _to.rotation.eulerAngles;

        float delta = Time.deltaTime / 0.75f;

        while (lerp < 1)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            lerp += delta;

            float angle = 90 - lerp * 180;

            float y = 0.75f * Mathf.Cos(angle * Mathf.Deg2Rad);

            _hand.position = Vector3.Lerp(startPos, endPos, lerp);
            _hand.position += transform.up * y;
            _hand.rotation = Quaternion.Slerp(Quaternion.Euler(startAngle), Quaternion.Euler(endAngle), lerp);
        }

        _hand.SetParent(_to);

        isAnimating = false;

        AdditionalTrigger();
    }

    IEnumerator ChangeHandPosition(Transform _to, Transform _hand, AudioClip audioClip)
    {
        if (_to == _hand.parent) yield break;

        isAnimating = true;

        _hand.SetParent(null);

        float lerp = 0;

        Vector3 startPos = _hand.position;
        Vector3 endPos = _to.position;

        Vector3 startAngle = _hand.rotation.eulerAngles;
        Vector3 endAngle = _to.rotation.eulerAngles;

        float delta = Time.deltaTime / 0.75f;

        while (lerp < 1)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            lerp += delta;

            float angle = 90 - lerp * 180;

            float y = 0.75f * Mathf.Cos(angle * Mathf.Deg2Rad);

            _hand.position = Vector3.Lerp(startPos, endPos, lerp);
            _hand.position += transform.up * y;
            _hand.rotation = Quaternion.Slerp(Quaternion.Euler(startAngle), Quaternion.Euler(endAngle), lerp);
        }

        _hand.SetParent(_to);
        handAudioSource.PlayOneShot(audioClip, playbackVolume * 0.5f);

        isAnimating = false;

        AdditionalTrigger();
    }

    IEnumerator ChangePiecePosition(Vector3 _to, Transform _piece)
    {
        isAnimating = true;

        float lerp = 0;

        Vector3 startPos = _piece.position;
        Vector3 endPos = _to;

        float delta = Time.deltaTime / 0.5f;

        while (lerp < 1)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            lerp += delta;

            float angle = 90 - lerp * 180;

            float y = 0.75f * Mathf.Cos(angle * Mathf.Deg2Rad);

            _piece.position = Vector3.Lerp(startPos, endPos, lerp);
            _piece.position += transform.up * y;
        }
        _piece.position = _to;
        

        isAnimating = false;
    }

    IEnumerator ChangePiecePosition(Vector3 _to, Transform _piece, AudioClip audioClip, int playOrder)
    {
        isAnimating = true;

        float lerp = 0;

        Vector3 startPos = _piece.position;
        Vector3 endPos = _to;

        float delta = Time.deltaTime / 0.5f;
        if(playOrder == 1) handAudioSource.PlayOneShot(audioClip, playbackVolume * 0.5f);

        while (lerp < 1)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            lerp += delta;

            float angle = 90 - lerp * 180;

            float y = 0.75f * Mathf.Cos(angle * Mathf.Deg2Rad);

            _piece.position = Vector3.Lerp(startPos, endPos, lerp);
            _piece.position += transform.up * y;
        }

        if (playOrder == 2) handAudioSource.PlayOneShot(audioClip, playbackVolume * 0.5f);
        _piece.position = _to;


        isAnimating = false;
    }
}
