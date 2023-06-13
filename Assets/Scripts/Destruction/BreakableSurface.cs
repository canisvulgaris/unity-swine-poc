using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GK
{
    public class BreakableSurface : MonoBehaviour
    {
        public bool isIntact = true;

        public bool flipAxes = false;

        public Material brokenMaterial;

        public float expPower = 1.0f;
        public float expRadius = 2.0f;

        public MeshFilter Filter { get; private set; }
        public MeshRenderer Renderer { get; private set; }
        public MeshCollider Collider { get; private set; }
        public Rigidbody Rigidbody { get; private set; }

        // public GameObject GlobalVars;

        public List<Vector2> Polygon;
        public float Thickness = 0.1f;
        public float MinBreakArea = 0.3f;
        public float MinImpactToBreak = 10.0f;
        public float MinShardMass = 1.0f;
        public float CurrentMass;
        public float ShardClearTime = 20.0f;

        bool destroyFlag = false;
        float _Area = -1.0f;
        public float Area
        {
            get
            {
                if (_Area < 0.0f)
                {
                    _Area = Geom.Area(Polygon);
                }

                return _Area;
            }
        }

        void Start()
        {
            // SetGlobalVars();
            Init();
        }

        // public void SetGlobalVars()
        // {
        //     if (GlobalVars != null)
        //     {
        //         GlobalVars BGV = GlobalVars.GetComponent<GlobalVars>();

        //         Thickness = BGV.Thickness;
        //         MinBreakArea = BGV.MinBreakArea;
        //         MinImpactToBreak = BGV.MinImpactToBreak;
        //         MinShardMass = BGV.MinShardMass;
        //         ShardClearTime = BGV.ShardClearTime;
        //     }
        // }

        public void Init()
        {            
            var pos = transform.position;

            if (Filter == null) Filter = GetComponent<MeshFilter>();
            if (Renderer == null) Renderer = GetComponent<MeshRenderer>();
            if (Collider == null) Collider = GetComponent<MeshCollider>();
            if (Rigidbody == null) Rigidbody = GetComponent<Rigidbody>();

            if (Polygon.Count == 0)
            {
                // Assume it's a cube with localScale dimensions
                var scale = 0.5f * transform.localScale;

                if (flipAxes) 
                { 
                    Polygon.Add(new Vector2(-scale.x, -scale.z));
                    Polygon.Add(new Vector2(scale.x, -scale.z));
                    Polygon.Add(new Vector2(scale.x, scale.z));
                    Polygon.Add(new Vector2(-scale.x, scale.z));

                    Thickness = 2.0f * scale.y;
                }
                else 
                {
                    Polygon.Add(new Vector2(-scale.x, -scale.y));
                    Polygon.Add(new Vector2(scale.x, -scale.y));
                    Polygon.Add(new Vector2(scale.x, scale.y));
                    Polygon.Add(new Vector2(-scale.x, scale.y));

                    Thickness = 2.0f * scale.z;
                }                

                transform.localScale = Vector3.one;
            }

            var mesh = MeshFromPolygon(Polygon, Thickness);

            Filter.sharedMesh = mesh;
            Collider.sharedMesh = mesh;
        }

        void FixedUpdate()
        {
            var pos = transform.position;
            
            CurrentMass = Rigidbody.mass;

            if (pos.magnitude > 1000.0f)
            {
                DestroyImmediate(gameObject);
            }

            // optimization to remove small pieces
            if (CurrentMass < MinShardMass && !destroyFlag)
            {
                Destroy(gameObject, ShardClearTime * CurrentMass);
                destroyFlag = true;
            }
        }

        void OnCollisionEnter(Collision coll)
        {            
            CheckForIntact();
            float impactModifier = 1.0f;
            if (coll.gameObject.tag == "Player") {
                impactModifier = 0.5f;
            }
            else
            {
                impactModifier = 1.0f;
            }

            // Debug.Log("Collision with " + coll.gameObject.name + " magnitude " + coll.relativeVelocity.magnitude + " needs " + MinImpactToBreak * impactModifier);

            if (coll.relativeVelocity.magnitude > MinImpactToBreak * impactModifier)
            {
                var pnt = coll.contacts[0].point;
                Break((Vector2)transform.InverseTransformPoint(pnt));
            }
        }

        void CheckForIntact()
        {  
            if (isIntact == true && Rigidbody.mass < 1.0f)
            {
                isIntact = false;
                //Fetch the Renderer from the GameObject
                // Renderer rend = GetComponent<Renderer>();

                //Set the main Color of the Material to green
                // rend.material.shader = Shader.Find("_Color");
                // rend.material.SetColor("_Color", Color.yellow);
                if (brokenMaterial) {
                    Renderer.material = brokenMaterial;      
                }
                else {
                    Renderer.material.color = new Color(1.0f, 0.52f, 0.18f, 1.0f);
                }

                Rigidbody.AddExplosionForce(expPower, transform.position, expRadius, 3.0F);
            }
            else
            {
                isIntact = true;
            }
        }

        static float NormalizedRandom(float mean, float stddev)
        {
            var u1 = UnityEngine.Random.value;
            var u2 = UnityEngine.Random.value;

            var randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
                Mathf.Sin(2.0f * Mathf.PI * u2);

            return mean + stddev * randStdNormal;
        }

        public void Break(Vector2 position)
        {            
            var area = Area;
            
            if (area > MinBreakArea)
            {
                var calc = new VoronoiCalculator();
                var clip = new VoronoiClipper();

                var sites = new Vector2[10];

                for (int i = 0; i < sites.Length; i++)
                {
                    var dist = Mathf.Abs(NormalizedRandom(0.0f, 1.0f / 2.0f));
                    var angle = 2.0f * Mathf.PI * Random.value;

                    sites[i] = position + new Vector2(
                            dist * Mathf.Cos(angle),
                            dist * Mathf.Sin(angle));
                }

                var diagram = calc.CalculateDiagram(sites);

                var clipped = new List<Vector2>();

                for (int i = 0; i < sites.Length; i++)
                {
                    clip.ClipSite(diagram, Polygon, i, ref clipped);

                    if (clipped.Count > 0)
                    {
                        var newGo = Instantiate(gameObject, transform.parent);

                        newGo.transform.localPosition = transform.localPosition;
                        newGo.layer = LayerMask.NameToLayer("Debris");

                        if (flipAxes)
                        {
                            newGo.transform.localRotation = transform.localRotation * Quaternion.Euler(90, 0, 0);
                        }
                        else
                        {
                            newGo.transform.localRotation = transform.localRotation;
                        }

                        var bs = newGo.GetComponent<BreakableSurface>();

                        if (flipAxes)
                        {
                            bs.flipAxes = true;
                        }
                        
                        bs.Thickness = Thickness;
                        bs.Polygon.Clear();
                        bs.Polygon.AddRange(clipped);

                        var childArea = bs.Area;

                        var rb = bs.GetComponent<Rigidbody>();
                        rb.isKinematic = false;

                        rb.mass = Rigidbody.mass * (childArea / area);
                    }
                }

                gameObject.SetActive(false);
                Destroy(gameObject);
            }
        }

        static Mesh MeshFromPolygon(List<Vector2> polygon, float thickness)
        {
            var count = polygon.Count;
            // TODO: cache these things to avoid garbage
            var verts = new Vector3[6 * count];
            var norms = new Vector3[6 * count];
            var tris = new int[3 * (4 * count - 4)];
            // TODO: add UVs

            var vi = 0;
            var ni = 0;
            var ti = 0;

            var ext = 0.5f * thickness;

            // Top
            for (int i = 0; i < count; i++)
            {
                verts[vi++] = new Vector3(polygon[i].x, polygon[i].y, ext);
                norms[ni++] = Vector3.forward;
            }

            // Bottom
            for (int i = 0; i < count; i++)
            {
                verts[vi++] = new Vector3(polygon[i].x, polygon[i].y, -ext);
                norms[ni++] = Vector3.back;
            }

            // Sides
            for (int i = 0; i < count; i++)
            {
                var iNext = i == count - 1 ? 0 : i + 1;

                verts[vi++] = new Vector3(polygon[i].x, polygon[i].y, ext);
                verts[vi++] = new Vector3(polygon[i].x, polygon[i].y, -ext);
                verts[vi++] = new Vector3(polygon[iNext].x, polygon[iNext].y, -ext);
                verts[vi++] = new Vector3(polygon[iNext].x, polygon[iNext].y, ext);

                var norm = Vector3.Cross(polygon[iNext] - polygon[i], Vector3.forward).normalized;

                norms[ni++] = norm;
                norms[ni++] = norm;
                norms[ni++] = norm;
                norms[ni++] = norm;
            }


            for (int vert = 2; vert < count; vert++)
            {
                tris[ti++] = 0;
                tris[ti++] = vert - 1;
                tris[ti++] = vert;
            }

            for (int vert = 2; vert < count; vert++)
            {
                tris[ti++] = count;
                tris[ti++] = count + vert;
                tris[ti++] = count + vert - 1;
            }

            for (int vert = 0; vert < count; vert++)
            {
                var si = 2 * count + 4 * vert;

                tris[ti++] = si;
                tris[ti++] = si + 1;
                tris[ti++] = si + 2;

                tris[ti++] = si;
                tris[ti++] = si + 2;
                tris[ti++] = si + 3;
            }

            Debug.Assert(ti == tris.Length);
            Debug.Assert(vi == verts.Length);

            var mesh = new Mesh();

            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.normals = norms;

            return mesh;
        }
    }
}
