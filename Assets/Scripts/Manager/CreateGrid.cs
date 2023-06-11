using UnityEngine;

public class CreateGrid : MonoBehaviour
{
     public int X = 3;
     public int Z = 3;
     public GameObject GridItemPrefab;
     public Transform parent;
     void Start()
     {

          float x = 0;
          float z = 0;


          float objX = 0;
          float objZ = 0;

          for (int i = 0; i < Z; i++)
          {
               for (int j = 0; j < X; j++)
               {
                    GameObject obj = Instantiate(GridItemPrefab, parent);
                    obj.transform.localPosition = Vector3.zero;

                    if (objZ == 0)
                    {
                         objX = 0.82f;
                         objZ = 0.82f;
                    }

                    float y = obj.transform.localPosition.y;

                    obj.transform.localPosition = new Vector3(x, y, z);
                    x += objX;
               }

               z += objZ;
               x = 0;
          }
     }
}
