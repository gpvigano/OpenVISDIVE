using UnityEngine;
using System.Collections;

namespace OpenVISDIVE
{
    /// <summary>
    /// Matrix utility class
    /// </summary>
    public class MatUtil
    {
    }
    /// <summary>
    /// Conversion between left and right handed coordinate systems (CS)
    /// </summary>
    /// <remarks>In Unity the coodinate system is left-handed, with Y=up, Z=forward and X=right</remarks>
    public class CsConversion
    {
        private static Matrix4x4 matRLyy = Matrix4x4.identity;
        private static Matrix4x4 matRLyz = Matrix4x4.zero;
        private static Matrix4x4 MatRLyy
        {
            get
            {
                if(matRLyy[2, 2]!=-1f)
                {
                    matRLyy[2, 2] = -1f;
                }
                return matRLyy;
            }
        }


        private static Matrix4x4 MatRLzy
        {
            get
            {
                if(matRLyz[0]==0)
                {
                    matRLyz[0, 0] = 1f;
                    matRLyz[1, 2] = 1f;
                    matRLyz[2, 1] = 1f;
                }
                return matRLyz;
            }
        }


        /// <summary>
        /// Convert a quaternion to a rotation matrix in right-handed coordinate system.
        /// </summary>
        /// <param name="lhcsRotation">Quaternion representing the rotation</param>
        /// <param name="swapYZup">Set this to true if the input vertical axis is Z</param>
        /// <returns>Rotation matrix in right-handed coordinate system</returns>
        public static Matrix4x4 QuatToRotMat(Quaternion lhcsRotation, bool swapYZup)
        {
            Matrix4x4 rotMat = Matrix4x4.TRS(Vector3.zero, lhcsRotation, Vector3.one);
            return MatToMatRL(rotMat, swapYZup);
        }


        /// <summary>
        /// Convert a rotation matrix in right-handed coordinate system to a quaternion 
        /// </summary>
        /// <param name="rhcsRotMat">Rotation matrix in right-handed coordinate system</param>
        /// <param name="swapYZup">Set this to true if the input vertical axis is Z</param>
        /// <returns>Quaternion representing the rotation</returns>
        public static Quaternion RotMatToQuat(Matrix4x4 rhcsRotMat, bool swapYZup)
        {
            rhcsRotMat.SetColumn(3, Vector4.zero);
            Matrix4x4 unityRot = MatToMatRL(rhcsRotMat, swapYZup);
            return Quaternion.LookRotation(unityRot.GetColumn(2), unityRot.GetColumn(1));
        }


        /// <summary>
        /// Extract the forward (Z) vector from a roto-translation matrix, switching between right and left-handed coordinate system
        /// </summary>
        /// <param name="rhcsRotMat">roto-translation matrix (right-handed coordinate system)</param>
        /// <param name="swapYZup">Set this to true if the input axis is Z</param>
        /// <returns>Forward (Z) vector in left-handed coodinate system</returns>
        public static Vector3 RotMatForward(Matrix4x4 rhcsRotMat, bool swapYZup)
        {
            return VecToVecRL(rhcsRotMat.GetColumn(2), swapYZup);
        }


        /// <summary>
        /// Extract the up (Y) vector from a roto-translation matrix, switching between right and left-handed coordinate system
        /// </summary>
        /// <param name="rhcsRotMat">roto-translation matrix (right-handed coordinate system)</param>
        /// <param name="swapYZup">Set this to true if the input axis is Z</param>
        /// <returns>Up (Y) vector in left-handed coodinate system</returns>
        public static Vector3 RotMatUp(Matrix4x4 rhcsRotMat, bool swapYZup)
        {
            return VecToVecRL(rhcsRotMat.GetColumn(1), swapYZup);
        }


        /// <summary>
        /// Extract the up (Y) vector from a roto-translation matrix, switching between right and left-handed coordinate system
        /// </summary>
        /// <param name="rhcsRotMat">roto-translation matrix (right-handed coordinate system)</param>
        /// <param name="swapYZup">Set this to true if the input axis is Z</param>
        /// <returns>Right (X) vector in left-handed coodinate system</returns>
        public static Vector3 RotMatRight(Matrix4x4 rhcsRotMat, bool swapYZup)
        {
            return VecToVecRL(rhcsRotMat.GetColumn(0), swapYZup);
        }


        /// <summary>
        /// Extract the translation vector from a roto-translation matrix
        /// </summary>
        /// <param name="rhcsLocation">roto-translation matrix (right-handed coordinate system)</param>
        /// <param name="swapYZup">Set this to true if the input axis is Z</param>
        /// <returns>Position vector (translation) in left-handed coodinate system</returns>
        public static Vector3 PosFromMatR(Matrix4x4 rhcsLocation, bool swapYZup)
        {
            return VecToVecRL(rhcsLocation.GetColumn(3), swapYZup);
        }


        /// <summary>
        /// Convert a vector between right-handed CS and left-handed CS
        /// </summary>
        /// <param name="vec">Vector int the original CS</param>
        /// <param name="swapYZup">Up axis must be changed (Y,Z)</param>
        /// <returns>Converted vector</returns>
        public static Vector3 VecToVecRL(Vector3 vec, bool swapYZup)
        {
            return new Vector3(vec.x, vec.y, swapYZup ? vec.z : -vec.z);
        }


        /// <summary>
        /// Extract position and rotation from a roto-translation in right handed coordinate system
        /// </summary>
        /// <param name="rhcsLocation">Roto-translation matrix (right-handed coordinate system)</param>
        /// <param name="lhcsPosition">Position vector (left-handed coordinate system)</param>
        /// <param name="lhcsRotation">Rotation quaternion</param>
        /// <param name="swapYZup">Set this to true if the input axis is Z</param>
        public static void PosRotFromMatR(Matrix4x4 rhcsLocation, ref Vector3 lhcsPosition, ref Quaternion lhcsRotation, bool swapYZup)
        {
            lhcsPosition = PosFromMatR(rhcsLocation, swapYZup);
            lhcsRotation = RotMatToQuat(MathUtility.MatrixToRotMat(rhcsLocation), swapYZup);
        }


        /// <summary>
        /// Convert a position and rotation (left-handed, Y up system) to a roto-translation matrix (right-handed)
        /// </summary>
        /// <param name="lhcsPosition">Position vector (left-handed coordinate system)</param>
        /// <param name="lhcsRotation">Rotation quaternion</param>
        /// <param name="swapYZup"></param>
        /// <returns>Roto-translation matrix (right-handed coordinate system)</returns>
        public static Matrix4x4 PosRotToMatR( Vector3 lhcsPosition, Quaternion lhcsRotation, bool swapYZup)
        {
            Matrix4x4 lhcsLocation = Matrix4x4.TRS(lhcsPosition, lhcsRotation, Vector3.one);
            return MatToMatRL(lhcsLocation, swapYZup);
        }


        /// <summary>
        /// Convert a matrix between right-handed CS and left-handed CS 
        /// </summary>
        /// <param name="matrix">Input matrix</param>
        /// <param name="swapYZup">Up axis must be changed (Y,Z)</param>
        /// <returns>Converted matrix</returns>
        public static Matrix4x4 MatToMatRL(Matrix4x4 matrix, bool swapYZup)
        {
            Matrix4x4 S = swapYZup ? MatRLzy : MatRLyy;
            return S * matrix * S;
        }
    }
}
