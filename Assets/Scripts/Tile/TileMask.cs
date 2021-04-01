using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileMask : MonoBehaviour {
    public Tile maskedTile;
    public int xIndex {
        get {
           return maskedTile != null ? maskedTile.xIndex : -1;
        }
    }

    public int yIndex {
        get {
            return maskedTile != null ? maskedTile.yIndex : -1;
        }
    }


    public TileMaskType maskType;
    public int maskLifeCounter;
    public bool canErase;
    public int EraseId;
    public bool isDestroying = false;

    public delegate void TileMaskDestoryEvent(TileMask tileMask);
    public event TileMaskDestoryEvent notifyTileMaskDestroy;


    public bool CanMoveTile() {
        return maskType == TileMaskType.None || maskLifeCounter == 0;
    }
    public abstract void EraseOnce();
    public abstract void ClearMask();
    public abstract void OnRoundEnd();

    protected IEnumerator FadeOff() {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        float startAlpha = spriteRenderer.color.a;
        float targetAlpha = 0;
        float fadeOffTime = 0.5f;
        float elapsedTime = 0;
        bool reachTarget = false;
        if (!isDestroying) {
            isDestroying = true;

            while (!reachTarget) {
                if (Mathf.Abs(startAlpha - targetAlpha) < 0.05f) {
                    reachTarget = true;
                    if (notifyTileMaskDestroy != null) {
                        notifyTileMaskDestroy(this);
                    }
                    Destroy(gameObject);
                }

                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp(elapsedTime / fadeOffTime, 0f, 1f);
                t = CommonUtil.Instance.Smoothstep(t);

                Color tmp = spriteRenderer.color;
                tmp.a = Mathf.Lerp(startAlpha, targetAlpha, t);
                spriteRenderer.color = tmp;
                yield return null;
            }
        }
    }

}
