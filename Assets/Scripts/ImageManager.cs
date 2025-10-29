using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ImageManager: MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Renderer targetRenderer;        // ���Ǿ� MeshRenderer

    [Header("Pages (0��1��2��...)")]
    [SerializeField] private List<Texture2D> pages = new();  // 0,1,2,3,4...

    [Header("Behaviour")]
    [SerializeField] private bool loop = true;

    // URP/Unlit ȣȯ�� �ؽ�ó ������Ƽ
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

    // --- ��ư OnClick()�� ������ �޼��� ---
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
        mpb.SetTexture(MainTexID, tex); // ǥ�� ���̴� ���
        targetRenderer.SetPropertyBlock(mpb);
    }

    bool Validate()
    {
        if (!targetRenderer)
        {
            Debug.LogError("[SphereImagePager] Target Renderer�� ������ϴ�.");
            return false;
        }
        if (pages == null || pages.Count == 0)
        {
            Debug.LogError("[SphereImagePager] Pages�� ������ϴ�.");
            return false;
        }
        return true;
    }
}
