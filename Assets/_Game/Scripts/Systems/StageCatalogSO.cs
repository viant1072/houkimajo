using System.Collections.Generic;
using UnityEngine;

namespace Houkimajo.Systems
{
    /// <summary>
    /// Danh sách màn theo thứ tự chơi. Thêm màn mới = tạo StageSO rồi kéo vào list này.
    /// Tạo qua Create → Houkimajo → Stage Catalog.
    /// </summary>
    [CreateAssetMenu(fileName = "StageCatalog", menuName = "Houkimajo/Stage Catalog")]
    public class StageCatalogSO : ScriptableObject
    {
        public List<StageSO> stages = new List<StageSO>();

        public int Count => stages.Count;

        public StageSO Get(int index)
        {
            if (stages.Count == 0) return null;
            return stages[Mathf.Clamp(index, 0, stages.Count - 1)];
        }
    }
}
