using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceTileMask : TileMask
{
    public override void ClearMask() {
        maskLifeCounter = 0;
        StartCoroutine(FadeOff());
    }

    public override void EraseOnce() {
        maskLifeCounter = 0;
        ClearMask();
    }

    public override void OnRoundEnd() {
    }

    private void Awake() {
        maskType = TileMaskType.Ice;
        maskLifeCounter = 2;
    }
}
