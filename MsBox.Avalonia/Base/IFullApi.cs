namespace MsBox.Avalonia.Base;

public interface IFullApi<T>: IClose//ICopy, IClose
{
    void SetButtonResult(T bdName);
    T GetButtonResult();
}