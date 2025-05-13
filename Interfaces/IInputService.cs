namespace Bachelor.Interfaces;

public interface IInputService
{
    void SimulateKeyDown(string key, string name);
    void ReleaseKey(string key, string name);
    void MoveMouseRelative(int x, int y);
    void MouseDown(bool isRight);
    void MouseUp(bool isRight);
    void ScrollMouse(int amount);
}

