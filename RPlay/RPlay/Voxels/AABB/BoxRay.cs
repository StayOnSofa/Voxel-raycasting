using System.Numerics;

namespace RPlay.Voxels
{
    public struct BoxRay {

        public Vector3 Point { private set; get; }
        public Vector3 Dir { private set; get; }
        public Vector3 InvDir { private set; get; }
        public Vector3 Sign { private set; get; }

        public BoxRay(Vector3 point, Vector3 dir)
        {
            Point = point;

            Dir = dir;
            InvDir = new Vector3(
                1f / dir.X,
                1f / dir.Y,
                1f / dir.Z
            );
            Sign = new Vector3(
                InvDir.X < 0f ? 1 : 0,
                InvDir.Y < 0f ? 1 : 0,
                InvDir.Z < 0f ? 1 : 0
            );
        }
    }
}