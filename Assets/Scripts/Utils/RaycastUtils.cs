using System;
using System.Collections.Generic;
using UnityEngine;

public static class RaycastUtils
{
    private static RaycastHit[] _internalCache = new RaycastHit[128];

    public static Func<RaycastHit, bool> IgnoreSelf(this Transform trans) => t => t.transform != trans;
    public static Func<RaycastHit, bool> IgnoreSelfAndChilds(this Transform trans) => t => t.transform != trans || t.transform.IsChildOf(trans);

    public static bool RaycastAll(Vector3 origin, Vector3 direction, Func<RaycastHit, bool> filter, out RaycastHit[] result, float distance = Mathf.Infinity, int mask = ~0)
    {
        var raycast = Physics.RaycastNonAlloc(origin, direction, _internalCache, distance, mask);
        List<RaycastHit> hits = new List<RaycastHit>();

        for(int i = 0; i < raycast; i++)
        {
            if (filter(_internalCache[i]))
            {
                hits.Add(_internalCache[i]);
            }
        }

        result = hits.ToArray();
        return hits.Count > 0;
    }

    public static bool Raycast(Vector3 origin, Vector3 direction, Func<RaycastHit, bool> filter, out RaycastHit result, float distance = Mathf.Infinity, int mask = ~0)
    {
        var raycast = Physics.RaycastNonAlloc(origin, direction, _internalCache, distance, mask);

        for (int i = 0; i < raycast; i++)
        {
            if (filter(_internalCache[i]))
            {
                result = _internalCache[i];
                return true;
            }
        }

        result = default;
        return false;
    }
}