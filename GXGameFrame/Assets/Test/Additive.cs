using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class SegmentTree {
    int[] tree;
    int[] points;
    int n;

    public SegmentTree(int[] points) {
        this.points = points;
        n = points.Length;
        tree = new int[2 * n];
        Build(0, 0, n - 1);
        for (int i = 0; i < tree.Length; i++)
        {
            Debug.Log(tree[i]);
        }
    }

    void Build(int node, int l, int r) {
        if (l == r) {
            tree[node] = points[l];
            return;
        }
        int mid = l + (r - l) / 2;
        Build(2 * node + 1, l, mid);
        Build(2 * node + 2, mid + 1, r);
        tree[node] = Mathf.Min(tree[2 * node + 1], tree[2 * node + 2]);
    }

    int Query(int node, int l, int r, int ql, int qr) {
        if (ql <= l && r <= qr) {
            return tree[node];
        }
        int mid = l + (r - l) / 2;
        int res = int.MaxValue;
        if (ql <= mid) {
            res = Mathf.Min(res, Query(2 * node + 1, l, mid, ql, qr));
        }
        if (qr > mid) {
            res = Mathf.Min(res, Query(2 * node + 2, mid + 1, r, ql, qr));
        }
        return res;
    }

    public List<int> RangeQuery(int l, int r) {
        List<int> res = new List<int>();
        int min = Query(0, 0, n - 1, l, r);
        for (int i = l; i <= r; i++) {
            if (points[i] == min) {
                res.Add(points[i]);
            }
        }
        return res;
    }
}

public class Additive : MonoBehaviour
{
    // Start is called before the first frame update
    // private Material a;
    void Start()
    {
        // int[] pont = new[] {1, 2, 3, 4, 5, 6, 7, 8,9};
        // SegmentTree a = new SegmentTree(pont);
        // var aa=  a.RangeQuery(3, 8);
        // for (int i = 0; i < aa.Count; i++)
        // {
        //     Debug.Log("xxx"+aa[i]);
        // }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            // a.EnableKeyword("CLIPPED");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            // a.DisableKeyword("CLIPPED");
        }
    }
}
