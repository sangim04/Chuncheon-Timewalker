using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class JulyeonManager : MonoBehaviour
{
    public static JulyeonManager Instance { get; private set; }

    // 주련이 들어갈 소켓들
    public SocketManager[] sockets;

    // 초기 위치로 돌릴 주련 오브젝트들
    public Transform[] itemsToReset;
    private Vector3[] initialPositions;
    private Quaternion[] initialRotations;

    private bool isPuzzleSolved = false;
    
    public AudioClip correctSound;
    public AudioClip uncorrectSound;
    
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 초기 위치와 회전을 저장할 배열 초기화
        initialPositions = new Vector3[itemsToReset.Length];
        initialRotations = new Quaternion[itemsToReset.Length];

        // 주련 오브젝트들의 초기 위치와 회전 저장
        for (int i = 0; i < itemsToReset.Length; i++)
        {
            initialPositions[i] = itemsToReset[i].position;
            initialRotations[i] = itemsToReset[i].rotation;
        }
        
        audioSource = GetComponent<AudioSource>();
    }

    // 소켓에 주련이 들어갔을 때 호출될 이벤트 리스너 추가
    private void OnEnable()
    {
        foreach (var socket in sockets)
        {
            if (socket != null)
            {
                // 소켓에 XRSocketInteractor가 있는지 확인 후 이벤트 리스너 추가
                UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor interactor = socket.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
                if (interactor != null)
                {
                    interactor.selectEntered.AddListener(OnSocketSelectEntered);
                }
            }
        }
    }

    // 이벤트 리스너를 제거하여 메모리 누수 방지
    private void OnDisable()
    {
        foreach (var socket in sockets)
        {
            if (socket != null)
            {
                UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor interactor = socket.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
                if (interactor != null)
                {
                    interactor.selectEntered.RemoveListener(OnSocketSelectEntered);
                }
            }
        }
    }

    private void OnSocketSelectEntered(SelectEnterEventArgs args)
    {
        CheckAnswer();
    }

    public void CheckAnswer()
    {
        // 퍼즐이 이미 해결되었다면 더 이상 확인 X
        if (isPuzzleSolved) return;

        bool allSocketsFilled = true;
        bool allCorrect = true;

        // 모든 소켓이 채워졌는지 확인
        for (int i = 0; i < sockets.Length; i++)
        {
            if (sockets[i].GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>().interactablesSelected.Count == 0)
            {
                allSocketsFilled = false;
                break; // 하나라도 비어 있으면 반복문 종료
            }
        }
        
        // allSocketsFilled false로 바꾸지 말고 바로 리턴 시키면 밑에 if문 바깥거 없애도 될듯?

        if (allSocketsFilled)
        {
            // 모든 소켓이 채워졌다면 정답 확인
            for (int i = 0; i < sockets.Length; i++)
            {
                // SocketManager의 IsCorrect()를 사용해 정답 여부 확인
                if (!sockets[i].IsCorrect())
                {
                    allCorrect = false;
                    break; // 하나라도 오답이면 반복문 종료
                }
            }

            if (allCorrect)
            {
                Debug.Log("정답입니다!");
                isPuzzleSolved = true;
                audioSource.PlayOneShot(correctSound);
                // 다음 단계로 넘어가는 로직을 여기에 추가하세요.
            }
            else
            {
                Debug.Log("오답입니다. 다시 시도하세요.");
                audioSource.PlayOneShot(uncorrectSound);
                StartCoroutine(ResetPuzzleCoroutine());
            }
        }
    }
    
    // 물건을 소켓에서 분리하고 초기 위치로 부드럽게 이동시키는 코루틴
    IEnumerator ResetPuzzleCoroutine()
    {
        // 1. 소켓의 콜라이더를 잠시 비활성화하여 다시 들어가는 것을 방지
        foreach (var socket in sockets)
        {
            Collider collider = socket.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }
        
        yield return new WaitForSeconds(0.3f);
        
        // 2. 주련을 소켓에서 먼저 분리
        foreach (var socket in sockets)
        {
            UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor interactor = socket.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
            if (interactor != null && interactor.interactablesSelected.Count > 0)
            {
                interactor.interactionManager.SelectExit(interactor, interactor.interactablesSelected[0]);
            }
        }

        // 3. 주련들을 초기 위치로 선형 이동
        float duration = 1.0f; // 이동에 걸리는 시간
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            for (int i = 0; i < itemsToReset.Length; i++)
            {
                Transform item = itemsToReset[i];
                if (item != null)
                {
                    item.position = Vector3.Lerp(item.position, initialPositions[i], t);
                    item.rotation = Quaternion.Slerp(item.rotation, initialRotations[i], t);
                }
            }
            yield return null;
        }

        // 4. 마지막으로 위치와 회전 초기 상태로 복원
        for (int i = 0; i < itemsToReset.Length; i++)
        {
            if (itemsToReset[i] != null)
            {
                itemsToReset[i].position = initialPositions[i];
                itemsToReset[i].rotation = initialRotations[i];
            }
        }
        
        yield return new WaitForSeconds(0.3f);

        // 5. 소켓들의 콜라이더 다시 활성화
        foreach (var socket in sockets)
        {
            Collider collider = socket.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = true;
            }
        }
    }
}
