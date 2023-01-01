using System.Numerics;
using Silk.NET.Maths;

namespace RPlay.Voxels
{

    public class CameraVoxelSelector
    {
        private enum Pole
        {
            Zero,
            Up,
            Down,
            West,
            North,
            East,
            South,
        }

        private struct Collidable
        {
            public Collidable(bool isCollide, Vector3D<int> block, Pole pole, Vector3D<int> scale,
                Vector3D<int> padding)
            {
                IsCollide = isCollide;
                Block = block;
                Pole = pole;
                Scale = scale;
                Padding = padding;
            }

            public bool IsCollide;
            public Vector3D<int> Block;
            public Pole Pole;
            public Vector3D<int> Scale;
            public Vector3D<int> Padding;
        }

        private Camera _camera;
        
        private VoxelMap _map;
        private VoxelLights _lights;
        
        private int _distance;
        
        public CameraVoxelSelector(Camera camera, VoxelMap map, VoxelLights lights, int distance = 10)
        {
            _camera = camera;
            _map = map;
            _distance = distance;
            _lights = lights;
        }

        private Collidable RayCastBlock(Vector3 a, Vector3 dir, float maxDist)
        {
            BoxRay ray = new BoxRay(a, dir);

            float px = a.X;
            float py = a.Y;
            float pz = a.Z;

            float dx = dir.X;
            float dy = dir.Y;
            float dz = dir.Z;

            float t = 0.0f;
            int ix = (int)Math.Floor(px);
            int iy = (int)Math.Floor(py);
            int iz = (int)Math.Floor(pz);

            int stepx = (dx > 0.0f) ? 1 : -1;
            int stepy = (dy > 0.0f) ? 1 : -1;
            int stepz = (dz > 0.0f) ? 1 : -1;

            Pole sidex = (dx > 0.0f) ? Pole.West : Pole.East;
            Pole sidey = (dy > 0.0f) ? Pole.Down : Pole.Up;
            Pole sidez = (dz > 0.0f) ? Pole.North : Pole.South;

            float infinity = float.MaxValue;

            float txDelta = (dx == 0.0f) ? infinity : Math.Abs(1.0f / dx);
            float tyDelta = (dy == 0.0f) ? infinity : Math.Abs(1.0f / dy);
            float tzDelta = (dz == 0.0f) ? infinity : Math.Abs(1.0f / dz);

            float xdist = (stepx > 0) ? (ix + 1 - px) : (px - ix);
            float ydist = (stepy > 0) ? (iy + 1 - py) : (py - iy);
            float zdist = (stepz > 0) ? (iz + 1 - pz) : (pz - iz);

            float txMax = (txDelta < infinity) ? txDelta * xdist : infinity;
            float tyMax = (tyDelta < infinity) ? tyDelta * ydist : infinity;
            float tzMax = (tzDelta < infinity) ? tzDelta * zdist : infinity;

            int steppedIndex = -1;


            Vector3 scale = Vector3.One;
            Vector3 padding = Vector3.Zero;

            Vector3D<int> block = new Vector3D<int>();
            Pole side = Pole.Zero;
            Vector3D<int> normal = new Vector3D<int>(0,0,0);
            Vector3 end;


            while (t <= maxDist)
            {
                block = new Vector3D<int>(ix, iy, iz);

                ushort blockId = _map.GetBlock(block.X, block.Y, block.Z);
                bool isCollide = blockId != 0;

                if (isCollide)
                {
                    end.X = px + t * dx;
                    end.Y = py + t * dy;
                    end.Z = pz + t * dz;

                    normal.X = normal.Y = normal.Z = 0;
                    if (steppedIndex == 0)
                    {
                        side = sidex;
                        normal.X = -stepx;
                    }
                    else if (steppedIndex == 1)
                    {
                        side = sidey;
                        normal.Y = -stepy;
                    }
                    else if (steppedIndex == 2)
                    {
                        side = sidez;
                        normal.Z = -stepz;
                    }

                    return new Collidable(true, block, side, new Vector3D<int>(1,1,1), new Vector3D<int>(1,1,1));
                }

                if (txMax < tyMax)
                {
                    if (txMax < tzMax)
                    {
                        ix += stepx;
                        t = txMax;
                        txMax += txDelta;
                        steppedIndex = 0;
                    }
                    else
                    {
                        iz += stepz;
                        t = tzMax;
                        tzMax += tzDelta;
                        steppedIndex = 2;
                    }
                }
                else
                {
                    if (tyMax < tzMax)
                    {
                        iy += stepy;
                        t = tyMax;
                        tyMax += tyDelta;
                        steppedIndex = 1;
                    }
                    else
                    {
                        iz += stepz;
                        t = tzMax;
                        tzMax += tzDelta;
                        steppedIndex = 2;
                    }
                }
            }

            return new Collidable(false, block, side, new Vector3D<int>(1,1,1), new Vector3D<int>(1,1,1));
        }

        private Vector3D<int> GetBlockSide(Vector3D<int> block, Pole side)
        {
            if (side == Pole.Up)
                return new  Vector3D<int>(block.X, block.Y + 1, block.Z);
            if (side == Pole.Down)
                return new  Vector3D<int>(block.X, block.Y - 1, block.Z);
            if (side == Pole.East)
                return new  Vector3D<int>(block.X + 1, block.Y, block.Z);
            if (side == Pole.West)
                return new  Vector3D<int>(block.X - 1, block.Y, block.Z);
            if (side == Pole.North)
                return new  Vector3D<int>(block.X, block.Y, block.Z - 1);
          
            return new Vector3D<int>(block.X, block.Y, block.Z + 1);
        }

        public bool RayCast(out Vector3D<int> block)
        {
            Collidable collidable = RayCastBlock(_camera.Position, _camera.Forward, _distance);
            
            if (collidable.IsCollide)
            {
                block = collidable.Block;
                return true;
            }

            block = Vector3D<int>.Zero;
            return false;
        }

        public void BreakRayCast()
        {
            Collidable collidable = RayCastBlock(_camera.Position, _camera.Forward, _distance);
            if (collidable.IsCollide)
            {
                var block = collidable.Block;
                
                _map.BreakBlock(block.X, block.Y, block.Z);
                _lights.SetPixel(LightState.Empty, block.X, block.Y, block.Z);
            }
        }

        public void PlaceRayCast(ushort type)
        {
            Collidable collidable = RayCastBlock(_camera.Position, _camera.Forward, _distance);
            if (collidable.IsCollide)
            {
                var block = collidable.Block;
                
                block = GetBlockSide(block, collidable.Pole);
                
                _map.PlaceBlock(type, block.X, block.Y, block.Z);
                _lights.SetPixel(LightState.Block, block.X, block.Y, block.Z);
            }
        }
        
        public void LampRayCast()
        {
            Collidable collidable = RayCastBlock(_camera.Position, _camera.Forward, _distance);
            if (collidable.IsCollide)
            {
                var block = collidable.Block;
                
                block = GetBlockSide(block, collidable.Pole);
                _map.PlaceBlock(68, block.X, block.Y, block.Z);
                _lights.SetPixel(LightState.Lamp, block.X, block.Y, block.Z);
            }
        }
    }
}