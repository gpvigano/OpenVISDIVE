using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenVISDIVE;

namespace OpenVISDIVE
{
    /// <summary>
    /// Check the state of an entity and its interactions.
    /// </summary>
    public class EntityValidator : MonoBehaviour
    {
        public virtual bool IsStatic(EntityData entityData)
        {
            return entityData != null && (entityData.type == "Building" || entityData.isStatic);
        }

        public virtual bool CanBeSelected(EntityData entityData)
        {
            return entityData != null && !IsStatic(entityData);
        }


        public virtual bool CanBeLeantAgainst(EntityData entityData, EntityData bearingSurfaceEntity)
        {
            return entityData != null && ((bearingSurfaceEntity != null && IsStatic(bearingSurfaceEntity)) || bearingSurfaceEntity == null);
        }


        public virtual bool CanCollideWith(EntityData entityData, EntityData bearingSurfaceEntity, EntityData collidingEntity, CollisionState collisionState)
        {
            return !collisionState.IsColliding || collidingEntity == bearingSurfaceEntity;
        }


        public virtual bool IsValid(GameObject aliasObject, EntityData entityData, EntityData bearingSurfaceEntity, EntityData collidingEntity, CollisionState collisionState)
        {
            if (!CanBeLeantAgainst(entityData, bearingSurfaceEntity)
               || !CanCollideWith(entityData,bearingSurfaceEntity, collidingEntity, collisionState))
            {
                return false;
            }
            // other tests...
            return true;
        }


        public virtual bool HasPendingIssues(GameObject aliasObject, EntityData entityData, EntityData bearingSurfaceEntity, EntityData collidingEntity)
        {
            return entityData == null || bearingSurfaceEntity == null || (bearingSurfaceEntity != null && !IsStatic(bearingSurfaceEntity)) || (collidingEntity != null && !IsStatic(collidingEntity));
        }

        #region DRAFT
        public EntityData GetCloserConnectableEntity(GameObject aliasObject, EntityData entityData)
        {
            throw new NotImplementedException();
        }


        public GameObject GetEntityConnector(GameObject aliasObject, EntityData entityData, EntityData connectedEntity)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
