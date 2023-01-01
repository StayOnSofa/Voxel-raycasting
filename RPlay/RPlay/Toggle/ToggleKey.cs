using Silk.NET.Input;
using Silk.NET.SDL;

namespace RPlay.Toggle;

public class ToggleKey
{
    private IKeyboard _keyboard;
    private Key _keyCode;
    
    public ToggleKey(Key keyCode, IKeyboard keyboard)
    {
        _keyCode = keyCode;
        _keyboard = keyboard;
    }
    
    private bool _keyChangeState = false;

    public bool IsToggle()
    {
        if (_keyboard == null) return false;

        bool isPress = _keyboard.IsKeyPressed(_keyCode);
        if (isPress != _keyChangeState)
        {
            _keyChangeState = isPress;
            return isPress;
        }

        return false;
    }
}