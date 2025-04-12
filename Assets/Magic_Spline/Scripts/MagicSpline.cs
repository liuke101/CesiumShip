using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SplineDraggableContext
{
    public GameObject Dragable;
    public float Time;
    public bool Invalid;
    public GameObject DependentObject;
}

public enum DraggableSplineEndAction
{
    Respawn,
    Destroy
}

public class MagicSpline : MonoBehaviour
{
    public GameObject DraggablePrefab;
    public float NewNodeOffset;
    public List<SplineSegment> Segments = new List<SplineSegment>();
    public float SpawnStartDelay = 0;
    public float SpawnDelay = 1;
    public int SpawnCount = 0;
    public float MoveSpeed = 0.5f;
    public DraggableSplineEndAction DraggableSplineEndAction;
    public float PreWarmTime = 100f;


    private float _spawnTimer;
    private float _totalLen;
    private int _spawned;

    private List<SplineDraggableContext> _contexts = new List<SplineDraggableContext>();
    public void AddNode()
    {
        Vector3 point = transform.position;
        Vector3 dir = Vector3.forward;
        float len = NewNodeOffset;
        if (_lines != null && _lines.Count > 0)
        {
            dir = (_lines.Last().To - _lines.Last().From).normalized;
            point = _lines.Last().From;
        }

        var obj = new GameObject($"Spline{Segments.Count}");
        obj.transform.parent = transform;
        obj.transform.position = point + dir * len;
        var segment = obj.AddComponent<SplineSegment>();

        var lastSegment = Segments.LastOrDefault();

        segment.H1 = new GameObject("Handle1").AddComponent<SplineHandle>();
        segment.H2 = new GameObject("Handle2").AddComponent<SplineHandle>();
        segment.H1.transform.parent = segment.transform;
        segment.H1.transform.position = point + dir * (len / 3);
        segment.H2.transform.parent = segment.transform;
        segment.H2.transform.position = point + dir * ((len / 3) * 2);

        Segments.Add(segment);
    }

    public void Close()
    {
        var first = Segments.First();
        var last = Segments.Last();
        last.transform.position = transform.position;
        Init();

        Segments.First().H1.SnapOppositeToAxis();
    }

    private bool Changed()
    {
        return Segments.Select(_ => _.Changed()).ToArray().Any(_ => _);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    void OnValidate()
    {
        Init();
    }

    void DrawDraggable(SplineDraggableContext ctx, float delta)
    {
        ctx.Time += delta;

        var dist = MoveSpeed * ctx.Time;
        if (dist > _totalLen)
        {
            if (DraggableSplineEndAction == DraggableSplineEndAction.Respawn)
            {
                dist = dist % _totalLen;
                var loopTime = _totalLen / MoveSpeed;
                ctx.Time = ctx.Time % loopTime;
            }
            else
            {
                ctx.Invalid = true;
                return;
            }
        }

        for (int i = 0; i < _lines.Count; ++i)
        {
            if (dist < _lines[i].Length)
            {
                var dir = (_lines[i].To - _lines[i].From).normalized;
                ctx.Dragable.transform.position = _lines[i].From;
                ctx.Dragable.transform.LookAt(_lines[i].To);
                return;
            }
            else
                dist -= _lines[i].Length;
        }
    }

    void DoPrewarm()
    {
        while (PreWarmTime > 0)
        {
            var dt = 0f;
            if (SpawnCount == 0 || _spawned < SpawnCount)
            {
                if (_contexts.Count == 0)
                {
                    var d = SpawnStartDelay;
                    PreWarmTime -= d;

                }
                else
                {
                    var d = SpawnDelay;
                    PreWarmTime -= d;
                }

                var obj = Instantiate(DraggablePrefab, transform);

                var particleSystem = obj.GetComponentInChildren<ParticleSystem>();
                _contexts.Add(new SplineDraggableContext()
                {
                    Dragable = obj,
                    DependentObject = particleSystem != null ? particleSystem.gameObject : null
                });
                ++_spawned;

                if (PreWarmTime > 0)
                {
                    dt = Mathf.Min(SpawnDelay, PreWarmTime);
                    PreWarmTime -= dt;
                }
            }
            else
            {
                dt = PreWarmTime;
                PreWarmTime = 0;
            }

            foreach (var context in _contexts)
            {
                DrawDraggable(context, dt);
                if (context.Invalid || context.DependentObject == null)
                    Destroy(context.Dragable);
            }

            _contexts.RemoveAll(_ => _.Invalid || _.DependentObject == null);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Changed())
            Init();
        if(Segments.RemoveAll(_ => _ == null) > 0)
            Init();
        if (PreWarmTime > 0)
        {
            DoPrewarm();
        }

        var dt = Time.deltaTime;
        if (SpawnCount == 0 || _spawned < SpawnCount)
        {
            var spawnNew = false;
            _spawnTimer += dt;
            if (_contexts.Count == 0)
            {
                if (spawnNew = _spawnTimer > SpawnStartDelay)
                {
                    _spawnTimer -= SpawnStartDelay;
                }
            }
            else
            {
                if (spawnNew = _spawnTimer > SpawnDelay)
                    _spawnTimer -= SpawnDelay;
            }

            if (spawnNew)
            {
                var obj = Instantiate(DraggablePrefab, transform);

                var particleSystem = obj.GetComponentInChildren<ParticleSystem>();
                _contexts.Add(new SplineDraggableContext()
                {
                    Dragable = obj,
                    DependentObject = particleSystem != null ? particleSystem.gameObject : null
                });
                ++_spawned;
            }
        }

        foreach (var context in _contexts)
        {
            DrawDraggable(context, dt);
            if (context.Invalid || context.DependentObject == null)
                Destroy(context.Dragable);
        }

        _contexts.RemoveAll(_ => _.Invalid || _.DependentObject == null);
        PreWarmTime = 0;
    }


    void Init()
    {
        _lines.Clear();
        _controls.Clear();
        _totalLen = 0;
        Segments.RemoveAll(_ => _ == null);
        var p0 = transform.position;
        for(var s = 0; s < Segments.Count; ++s)
        {
            var segment = Segments[s];
            Vector3 p1 = segment.H1.transform.position;
            Vector3 p2 = segment.H2.transform.position;
            Vector3 p3 = segment.transform.position;
            Vector3 v0 = p0;

            Gizmos.color = Color.white;
            for (int i = 1; i < 1001; i++)
            {
                var t = i / 1000.0f;
                var v1 = Mathf.Pow(1f - t, 3) * p0
                         + 3 * t * Mathf.Pow(1 - t, 2) * p1
                         + 3 * Mathf.Pow(t, 2) * (1 - t) * p2
                         + Mathf.Pow(t, 3) * p3;

                var len = Vector3.Distance(v0, v1);
                _totalLen += len;
                _lines.Add(new Line()
                {
                    From = v0,
                    To = v1,
                    Length = len
                });
                v0 = v1;
            }
            _controls.Add(new Line()
            {
                From = p0,
                To = p1
            });
            _controls.Add(new Line()
            {
                From = p3,
                To = p2
            });

            p0 = p3;

            segment.H1.Opposite = null;
            segment.H1.Origin = null;
            segment.H2.Opposite = null;
            segment.H2.Origin = null;

            if (s > 0)
            {
                var prevSegment = Segments[s - 1];
                prevSegment.H2.Opposite = segment.H1.gameObject;
                prevSegment.H2.Origin = prevSegment.gameObject;
                segment.H1.Opposite = prevSegment.H2.gameObject;
                segment.H1.Origin = prevSegment.gameObject;

                if (s + 1 == Segments.Count)
                {
                    var firstSegment = Segments.First();
                    var dist = Vector3.Distance(segment.transform.position, transform.position);
                    if (dist < 0.0000001f)
                    {
                        segment.H2.Opposite = firstSegment.H1.gameObject;
                        segment.H2.Origin = gameObject;
                        firstSegment.H1.Opposite = segment.H2.gameObject;
                        firstSegment.H1.Origin = gameObject;
                    }
                }
            }
        }

        Debug.Log($"Full spline len {_totalLen}.");
        transform.hasChanged = false;
    }

    void OnDrawGizmos()
    {
        if(Segments.RemoveAll(_ => _ == null) > 0)
            Init();
        if (Changed())
            Init();
        Gizmos.color = Color.white;
        foreach (var line in _lines)
        {
            Gizmos.DrawLine(line.From, line.To);
        }

        Gizmos.color = Color.green;
        foreach (var line in _controls)
        {
            Gizmos.DrawLine(line.From, line.To);
        }
    }

    private List<Line> _controls = new List<Line>();
    private List<Line> _lines = new List<Line>();

    struct Line
    {
        public Vector3 From;
        public Vector3 To;
        public float Length;
    }

    
}
