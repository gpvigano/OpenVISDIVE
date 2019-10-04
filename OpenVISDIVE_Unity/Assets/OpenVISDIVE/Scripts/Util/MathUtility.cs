using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenVISDIVE
{
    public static class MathUtility
    {
        public enum Axis { Left, Right, Down, Up, Back, Front }
        public static Vector3 GetBoundsFaceCenter(Bounds bounds, Axis axis)
        {
            Vector3 center = Vector3.zero;
            switch (axis)
            {
                case Axis.Left:
                    center = new Vector3(bounds.min.x, bounds.center.y, bounds.center.z);
                    break;
                case Axis.Right:
                    center = new Vector3(bounds.max.x, bounds.center.y, bounds.center.z);
                    break;
                case Axis.Down:
                    center = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
                    break;
                case Axis.Up:
                    center = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
                    break;
                case Axis.Back:
                    center = new Vector3(bounds.center.x, bounds.center.y, bounds.min.z);
                    break;
                case Axis.Front:
                    center = new Vector3(bounds.center.x, bounds.center.y, bounds.max.z);
                    break;

            }
            return center;
        }


        public static Vector3 GetAxisVector(Axis axis)
        {
            Vector3 vector = Vector3.zero;
            switch (axis)
            {
                case Axis.Left:
                    vector = Vector3.left;
                    break;
                case Axis.Right:
                    vector = Vector3.right;
                    break;
                case Axis.Down:
                    vector = Vector3.down;
                    break;
                case Axis.Up:
                    vector = Vector3.up;
                    break;
                case Axis.Back:
                    vector = Vector3.back;
                    break;
                case Axis.Front:
                    vector = Vector3.forward;
                    break;

            }
            return vector;
        }


        public static Vector3 GetAbsAxisVector(Axis axis)
        {
            Vector3 vector = Vector3.zero;
            switch (axis)
            {
                case Axis.Left:
                case Axis.Right:
                    vector = Vector3.right;
                    break;
                case Axis.Down:
                case Axis.Up:
                    vector = Vector3.up;
                    break;
                case Axis.Back:
                case Axis.Front:
                    vector = Vector3.forward;
                    break;

            }
            return vector;
        }


        public static void SetTransformAxis(Transform transform, Axis axis, Vector3 versor)
        {
            switch (axis)
            {
                case Axis.Left:
                    transform.right = -versor;
                    break;
                case Axis.Right:
                    transform.right = versor;
                    break;
                case Axis.Down:
                    transform.up = -versor;
                    break;
                case Axis.Up:
                    transform.up = versor;
                    break;
                case Axis.Back:
                    transform.forward = -versor;
                    break;
                case Axis.Front:
                    transform.forward = versor;
                    break;

            }
        }


        public static Vector3 GetBoundCenter(GameObject obj)
        {
            Renderer[] rendererList = obj.GetComponentsInChildren<Renderer>();
            if (rendererList.Length == 0)
            {
                return obj.transform.position;
            }
            Vector3 boundCenter = Vector3.zero;
            foreach (Renderer rend in rendererList)
            {
                boundCenter += rend.bounds.center;
            }
            boundCenter = boundCenter / rendererList.Length;
            return boundCenter;
        }


        /// <summary>
        /// Convert a quaternion to a rotation matrix
        /// </summary>
        /// <param name="rotation">Quaternion representing the rotation</param>
        /// <returns>Rotation matrix</returns>
        public static Matrix4x4 QuaternionToMatrix(Quaternion rotation)
        {
            Matrix4x4 unityRot = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
            return unityRot;
        }


        /// <summary>
        /// Convert a rotation matrix to a quaternion
        /// </summary>
        /// <param name="matrix">Input rotation matrix</param>
        /// <returns>Quaternion representing the rotation</returns>
        public static Quaternion MatrixToQuaternion(Matrix4x4 matrix)
        {
            return Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
        }


        /// <summary>
        /// Extract the rotation matrix from a roto-translation matrix
        /// </summary>
        /// <param name="location">Roto-translation matrix</param>
        /// <returns>Rotation matrix</returns>
        public static Matrix4x4 MatrixToRotMat(Matrix4x4 location)
        {
            Matrix4x4 rotMat = new Matrix4x4();
            rotMat = location;
            rotMat.SetColumn(3, Vector4.zero);
            return rotMat;
        }


        /// <summary>
        /// Extract the forward (Z) vector from a roto-translation matrix
        /// </summary>
        /// <param name="rotMat">roto-translation matrix</param>
        /// <returns>Forward (Z) vector</returns>
        public static Vector3 MatrixForward(Matrix4x4 rotMat)
        {
            return rotMat.GetColumn(2);
        }


        /// <summary>
        /// Extract the up (Y) vector from a roto-translation matrix
        /// </summary>
        /// <param name="rotMat">roto-translation matrix</param>
        /// <returns>Up (Y) vector</returns>
        public static Vector3 MatrixUp(Matrix4x4 rotMat)
        {
            return rotMat.GetColumn(1);
        }


        /// <summary>
        /// Extract the right (X) vector from a roto-translation matrix
        /// </summary>
        /// <param name="rotMat">roto-translation matrix</param>
        /// <returns>Right (X) vector</returns>
        public static Vector3 MatrixRight(Matrix4x4 rotMat)
        {
            return rotMat.GetColumn(0);
        }
    }


}
