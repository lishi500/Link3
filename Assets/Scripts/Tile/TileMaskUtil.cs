using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMaskUtil : Singleton<TileMaskUtil>
{
    public TileMask CreateTileMask(TileMaskType maskType) {
        GameObject prefab = GetTileMaskPrefab(maskType);
        GameObject tileMaskObj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        return tileMaskObj.GetComponent<TileMask>();
    }

    private GameObject GetTileMaskPrefab(TileMaskType maskType) {
        switch (maskType) {
            case TileMaskType.Ice:
                return CommonUtil.Instance.GetPrefabByName("IceMask");
            default:
                return CommonUtil.Instance.GetPrefabByName("IceMask");
        }
    }
}
