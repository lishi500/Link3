using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// screen height = 2 * orthographic size
// screen width = 2 * orthographic size

// for vertical
// orthographic size = content height (in unit) / 2 + borderSize (liubai)

// for horizontal
// orthographic size = ((content width / 2) + borderSize) / aspect ratio
public class OrthgraphicSizeHelper : Singleton<OrthgraphicSizeHelper>
{

    public float horizontalFit(float contentWidth, float horizontalBorder, float screenRatio) {
        return (contentWidth / 2 + horizontalBorder) / screenRatio;
    }

    public float verticalFit(float contentHeight, float verticalBorder) {
        return contentHeight / 2 + verticalBorder;
    }

    public float autoFit(float contentWidth, float contentHeight, float horizontalBorder, float verticalBorder, float screenRatio) {
        float horizontalOrthographic = horizontalFit(contentWidth, horizontalBorder, screenRatio);
        float verticalOrthographic = verticalFit(contentHeight, verticalBorder);
        return Mathf.Max(horizontalOrthographic, verticalOrthographic);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
