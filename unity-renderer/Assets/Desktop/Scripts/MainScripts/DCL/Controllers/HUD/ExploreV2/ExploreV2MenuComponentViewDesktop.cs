using DCL;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExploreV2MenuComponentViewDesktop : ExploreV2MenuComponentView
{
    internal static ExploreV2MenuComponentView Create()
    {
        #if DCL_VR
        ExploreV2MenuComponentView exploreV2View = Instantiate(Resources.Load<GameObject>("ExploreV2MenuDesktopVR")).GetComponent<ExploreV2MenuComponentView>();
        exploreV2View.name = "_ExploreV2Desktop";
        #else
        ExploreV2MenuComponentView exploreV2View = Instantiate(Resources.Load<GameObject>("ExploreV2MenuDesktop")).GetComponent<ExploreV2MenuComponentView>();
        exploreV2View.name = "_ExploreV2Desktop";
        #endif


        return exploreV2View;
    }
}
