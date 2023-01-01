using System.Numerics;
using Silk.NET.Input;

namespace RPlay
{
    public class Camera
    {
        private const float Deg2Rad = (MathF.PI * 2f) / 360f;
        
        public Vector3 Forward { private set; get; }
        public Vector3 Position;

        private bool _isGrabbed;
        
        public bool MouseGrab
        {
            set
            {
                if (_mouse == null) return;

                var middleCenter = _flow.Window.Size / 2;
                
                _mouse.Cursor.CursorMode = value ? CursorMode.Raw : CursorMode.Normal;
                _isGrabbed = value;

                if (_isGrabbed)
                    _mouse.Position = _lastMousePosition;
                else
                    _mouse.Position = new Vector2(middleCenter.X, middleCenter.Y);

            }
            get => _isGrabbed;
        }

        private Flow _flow;
        private IKeyboard? _keyboard;
        private IMouse? _mouse;
        
        private float _yaw = -90f;
        private float _pitch;
        
        private Vector2 _lastMousePosition;
        
        public Camera(Flow flow)
        {
            _flow = flow;
            _mouse = flow.Input.Mice.FirstOrDefault();
            
            if (_mouse != null)
                _mouse.MouseMove += OnMouseMove;

            Position = Vector3.Zero;
            Forward = Vector3.Zero;

            MouseGrab = true;
        }
        
        private void OnMouseMove(IMouse mouse, Vector2 position)
        {
            if (MouseGrab)
            {
                var lookSensitivity = 0.01f;
                if (_lastMousePosition == default)
                {
                    _lastMousePosition = position;
                }
                else
                {
                    var xOffset = (position.X - _lastMousePosition.X) * lookSensitivity;
                    var yOffset = (position.Y - _lastMousePosition.Y) * lookSensitivity;
                    _lastMousePosition = position;

                    _yaw -= xOffset;
                    _pitch -= yOffset;

                    _pitch = Math.Clamp(_pitch, -89.0f, 89.0f);

                    var forward = Forward;

                    forward.X = (float) Math.Cos(Deg2Rad * (_pitch)) * (float) Math.Cos(Deg2Rad * (_yaw));
                    forward.Y = (float) Math.Sin(Deg2Rad * (_pitch));
                    forward.Z = (float) Math.Cos(Deg2Rad * (_pitch)) * (float) Math.Sin(Deg2Rad * (_yaw));
                    forward = Vector3.Normalize(forward);

                    Forward = forward;
                }
            }
        }

        public Vector2 GetMousePosition() => _mouse.Position;
    }
}