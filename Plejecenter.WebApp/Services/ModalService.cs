using Microsoft.AspNetCore.Components;

namespace Services;

public class ModalService
{
    public bool IsOpen { get; private set; }
    public EventCallback<bool> OnResult { get; private set; }

    public void Show(EventCallback<bool> onResult)
    {
        IsOpen = true;
        OnResult = onResult;
    }

    public void Close()
    {
        IsOpen = false;
    }
}

