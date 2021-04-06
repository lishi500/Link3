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
        maskLifeCounter -= 1;
        if (maskLifeCounter == 0) { 
            ClearMask();
        }
    }

    public override void OnRoundEnd() {
    }
}
