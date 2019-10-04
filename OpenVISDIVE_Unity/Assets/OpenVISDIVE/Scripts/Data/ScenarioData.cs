using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace OpenVISDIVE
{
    /// <summary>
    /// Component holding the data related to the scenario
    /// and the references to all its entities.
    /// </summary>
    public class ScenarioData : MonoBehaviour
    {

        protected Dictionary<string, EntityData> entityObjects = new Dictionary<string, EntityData>();

        public int EntityCount { get => entityObjects.Count; }

        public void DestroyAllEntities()
        {
            foreach (var entry in entityObjects)
            {
                Destroy(entry.Value.gameObject);
            }
            entityObjects.Clear();           
        }


        public void AddEntity(EntityData entity)
        {
            entityObjects.Add(entity.id, entity);
        }


        public void DestroyEntity(EntityData entity)
        {
            entity.UnlinkRelationships();
            var ownedRelationship = entity.GetComponents<Relationship>();
            foreach (var rel in ownedRelationship)
            {
                Destroy(rel);
            }
            entityObjects.Remove(entity.id);
            Destroy(entity);
        }

        /// <summary>
        /// Check if the entity with the given identifier is defined in the scenario data
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public bool ContainsEntity(string entityId)
        {
            return entityObjects.ContainsKey(entityId);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnDestroy()
        {
            DestroyAllEntities();
        }
    }
}
