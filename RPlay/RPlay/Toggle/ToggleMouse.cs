using Silk.NET.Input;

namespace RPlay.Toggle;

public class ToggleMouse
{
    private IMouse _mouse;

    public ToggleMouse(IMouse mouse)
    {
        _mouse = mouse;
    }
    
    private bool _leftChangeState = false;
    private bool _rightChangeState = false;

    public bool IsLeft()
    {
        if (_mouse == null) return false;

        bool isPress = _mouse.IsButtonPressed(MouseButton.Left);
        if (isPress != _leftChangeState)
        {
            _leftChangeState = isPress;
            return isPress;
        }

        return false;
    }
    
    public bool IsRight()
    {
        if (_mouse == null) return false;

        bool isPress = _mouse.IsButtonPressed(MouseButton.Right);
        if (isPress != _rightChangeState)
        {
            _rightChangeState = isPress;
            return isPress;
        }

        return false;
    }
}