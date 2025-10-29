using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ImageManager: MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Renderer targetRenderer;        // 스피어 MeshRenderer

    [Header("Pages (0→1→2→...)")]
    [SerializeField] private List<Texture2D> pages = new();  // 0,1,2,3,4...

    [Header("Behaviour")]
    [SerializeField] private bool loop = true;

    // URP/Unlit 호환용 텍스처 프로퍼티
    static readonly int BaseMapID = Shader.PropertyToID("_BaseMap");
    static readonly int MainTexID = Shader.PropertyToID("_MainTex");

    int index = 0;
    MaterialPropertyBlock mpb;

    void Start()
    {
        if (!Validate()) { enabled = false; return; }
        mpb = new MaterialPropertyBlock();
        ApplyPage();
    }

    // --- 버튼 OnClick()에 연결할 메서드 ---
    public void NextPage() => Shift(+1);
    public void PrevPage() => Shift(-1);

    void Shift(int delta)
    {
        if (pages.Count == 0) return;
        int newIndex = index + delta;
        if (loop)
            newIndex = (newIndex % pages.Count + pages.Count) % pages.Count;
        else
            newIndex = Mathf.Clamp(newIndex, 0, pages.Count - 1);

        if (newIndex == index) return;
        index = newIndex;
        ApplyPage();
    }

    void ApplyPage()
    {
        if (!targetRenderer || pages.Count == 0) return;
        var tex = pages[index];
        if (!tex) return;

        targetRenderer.GetPropertyBlock(mpb);
        mpb.SetTexture(BaseMapID, tex); // URP Unlit
        mpb.SetTexture(MainTexID, tex); // 표준 셰이더 대비
        targetRenderer.SetPropertyBlock(mpb);
    }

    bool Validate()
    {
        if (!targetRenderer)
        {
            Debug.LogError("[SphereImagePager] Target Renderer가 비었습니다.");
            return false;
        }
        if (pages == null || pages.Count == 0)
        {
            Debug.LogError("[SphereImagePager] Pages가 비었습니다.");
            return false;
        }
        return true;
    }
}
